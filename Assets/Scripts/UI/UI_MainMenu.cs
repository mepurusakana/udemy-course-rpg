using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主選單控制（含：從另一個場景 Additive 載入設定 UI，
/// 並透過 Binder 回填引用；不再依賴物件名稱搜尋）
/// 英→中：Additive=附加載入；Binder=綁定器；SortingOrder=繪製順序
/// </summary>
public class UI_MainMenu : MonoBehaviour
{
    [Header("場景切換")]
    [SerializeField] private string sceneName = "MainScene";
    [SerializeField] private string targetSceneName;
    [SerializeField] private UI_FadeScreen fadeScreen;

    [Header("設定 UI 來自另一個場景（Additive）")]
    [Tooltip("包含設定 UI 的場景名稱（必須加到 File > Build Settings）")]
    [SerializeField] private string settingsSceneName = "SettingsScene";

    [Tooltip("關閉設定時是否卸載該附加場景（省資源）")]
    [SerializeField] private bool unloadSettingsSceneOnClose = true;

    [Tooltip("打開設定時是否暫停時間（Time.timeScale=0）")]
    [SerializeField] private bool pauseWhenOpenSettings = false;

    [Tooltip("為了確保設定面板在最上層，可臨時提高 Sorting Order（數字越大越上層）")]
    [SerializeField] private int settingsCanvasSortingOrder = 5000;

    [Header("偵錯")]
    [SerializeField] private bool enableLogs = true;
    [Tooltip("等待 Binder 回填的最長秒數（避免無限等待）")]
    [SerializeField] private float waitBinderTimeout = 3f;

    // ====== 由 Binder 回填的引用（Do NOT 手動指定）======
    private GameObject _settingsCanvasRoot;   // 設定 Canvas 根
    private GameObject _settingUI;            // 總設定頁
    private GameObject _audioUI;              // 音訊子頁
    private GameObject _videoUI;              // 影像子頁
    private GameObject _instUI;               // 說明子頁（可空）

    // 內部：記錄附加載入的場景
    private Scene _settingsScene;

    // ====== 你原本就有的功能 ======
    public void ContinueGame() { StartCoroutine(LoadSceneWithFadeEffect(1.5f)); }
    public void NewGame() { StartCoroutine(LoadSceneWithFadeEffect(1.5f)); }
    public void ExitGame() { Debug.Log("Exit game"); Application.Quit(); }

    public void SwitchScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        { SceneManager.LoadScene(targetSceneName); Debug.Log($"切換到場景：{targetSceneName}"); }
        else Debug.LogError("目標場景名稱未設定!");
    }

    private IEnumerator LoadSceneWithFadeEffect(float delay)
    {
        if (fadeScreen) fadeScreen.FadeOut();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    // ====== 給按鈕 OnClick 用 ======
    public void OpenSettings() { StartCoroutine(OpenSettingsRoutine(null)); }
    public void OpenAudioSettings() { StartCoroutine(OpenSettingsRoutine("audio")); }
    public void OpenVideoSettings() { StartCoroutine(OpenSettingsRoutine("video")); }
    public void OpenInstructions() { StartCoroutine(OpenSettingsRoutine("inst")); } // 可有可無

    public void CloseSettings()
    {
        SetActiveSafe(_settingsCanvasRoot, false);
        if (pauseWhenOpenSettings) Time.timeScale = 1f;

        // 可選：卸載附加載入的設定場景
        if (unloadSettingsSceneOnClose && _settingsScene.IsValid() && _settingsScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(_settingsScene);
            // 清引用（確保下次會再等待 Binder）
            _settingsScene = default;
            _settingsCanvasRoot = _settingUI = _audioUI = _videoUI = _instUI = null;
        }
    }

    // === A 方法的核心：給設定場景的 Binder 回填引用 ===
    // 英→中：BindSettingsRefs = 綁定（回填）設定頁引用
    public void BindSettingsRefs(
        GameObject setting, GameObject audio, GameObject video,
        GameObject inst = null, GameObject canvasRoot = null)
    {
        _settingUI = setting;
        _audioUI = audio;
        _videoUI = video;
        _instUI = inst;
        if (canvasRoot) _settingsCanvasRoot = canvasRoot;

        // 一進來先關掉根，避免載入瞬間閃一下
        if (_settingsCanvasRoot) _settingsCanvasRoot.SetActive(false);

        if (enableLogs) Debug.Log("[UI_MainMenu] Settings 引用已由 Binder 綁定完成。");
    }

    // ====== 內部流程 ======
    private IEnumerator OpenSettingsRoutine(string tab)
    {
        // 1) 若尚未載入設定場景 → 先 Additive 載入
        if (!_settingsScene.IsValid() || !_settingsScene.isLoaded)
        {
            if (string.IsNullOrEmpty(settingsSceneName))
            { Debug.LogError("[UI_MainMenu] settingsSceneName 未設定"); yield break; }

            var op = SceneManager.LoadSceneAsync(settingsSceneName, LoadSceneMode.Additive);
            if (op == null) { Debug.LogError($"[UI_MainMenu] 無法載入場景：{settingsSceneName}"); yield break; }
            while (!op.isDone) yield return null;

            _settingsScene = SceneManager.GetSceneByName(settingsSceneName);
            if (!_settingsScene.IsValid() || !_settingsScene.isLoaded)
            { Debug.LogError("[UI_MainMenu] 設定場景載入失敗"); yield break; }

            if (enableLogs) Debug.Log($"[UI_MainMenu] Additive 載入完成：{settingsSceneName}");
        }

        // 2) 等待 Binder 在 Awake() 回填引用（最多 waitBinderTimeout 秒）
        float waited = 0f;
        while (!HasAllSettingsRefs() && waited < waitBinderTimeout)
        {
            waited += Time.unscaledDeltaTime;
            yield return null; // 等 1 frame
        }
        if (!HasAllSettingsRefs())
        {
            Debug.LogError("[UI_MainMenu] 等待 Binder 超時：仍未回填 Setting/Audio/Video 引用。請確認設定場景的 SettingsCanvas 有掛 Binder，且欄位已拖好。");
            yield break;
        }

        // 3) 確保設定 Canvas 在最上層
        if (_settingsCanvasRoot)
        {
            foreach (var c in _settingsCanvasRoot.GetComponentsInChildren<Canvas>(true))
            {
                c.overrideSorting = true;             // 覆寫排序（英→中：overrideSorting=覆蓋排序）
                c.sortingOrder = settingsCanvasSortingOrder;
            }
        }

        // 4) 顯示
        SetActiveSafe(_settingsCanvasRoot, true);
        SetActiveSafe(_settingUI, true);
        ShowOnly(tab);

        if (pauseWhenOpenSettings) Time.timeScale = 0f;
    }

    private void ShowOnly(string tab)
    {
        SetActiveSafe(_audioUI, false);
        SetActiveSafe(_videoUI, false);
        SetActiveSafe(_instUI, false);

        if (tab == "audio") SetActiveSafe(_audioUI, true);
        else if (tab == "video") SetActiveSafe(_videoUI, true);
        else if (tab == "inst") SetActiveSafe(_instUI, true);
        // tab==null → 只開總設定頁，不強制子頁
    }

    private static void SetActiveSafe(GameObject go, bool v)
    {
        if (go && go.activeSelf != v) go.SetActive(v);
    }

    private bool HasAllSettingsRefs()
        => _settingsCanvasRoot && _settingUI && _audioUI && _videoUI;
}
