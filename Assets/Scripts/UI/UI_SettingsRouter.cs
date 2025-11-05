using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 設定導向器（單檔）
/// - 在 MainMenu 按鈕呼叫 OpenSettings/OpenAudio/OpenVideo/OpenInstructions。
/// - 會切到指定場景（例如 A001），載入後：
///   1) 優先用 UI_Manager 開啟對應頁；
///   2) 若沒有 UI_Manager 或尚未就緒，改用「名稱後備」直接把該頁物件 SetActive(true)。
/// 名詞：Router=導向器；fallback=後備；sceneLoaded=場景載入事件；sorting order=繪製層級
/// </summary>
public class UI_SettingsRouter : MonoBehaviour
{
    [Header("要切去的設定場景")]
    [SerializeField] private string settingsSceneName = "A001";

    [Header("名稱後備（若 UI_Manager 失敗就用這些名字直接開物件）")]
    [Tooltip("設定總頁物件名稱（例：UI_SETTING 或 UI_Setting）")]
    [SerializeField] private string settingUIName = "UI_SETTING";
    [Tooltip("音訊頁物件名稱（例：Audio_Setting）")]
    [SerializeField] private string audioUIName = "Audio_Setting";
    [Tooltip("影像頁物件名稱（例：Video_Setting）")]
    [SerializeField] private string videoUIName = "Video_Setting";
    [Tooltip("（可選）說明頁物件名稱")]
    [SerializeField] private string instUIName = "UI_Instructions";

    [Header("外觀/時序")]
    [Tooltip("為確保在最上層，臨時拉高 Canvas 的 sortingOrder")]
    [SerializeField] private int settingsCanvasSortingOrder = 5000;
    [Tooltip("在新場景每幀嘗試多久（秒）去等待 UI_Manager 或名稱後備")]
    [SerializeField] private float waitTimeoutSeconds = 3f;

    [Header("偵錯")]
    [SerializeField] private bool enableLogs = true;

    // ===== 靜態暫存（切場景後仍存在）=====
    private enum Action { None, Setting, Audio, Video, Inst }
    private static Action _pending = Action.None;
    private static string _pendingScene;
    private static string _nSetting, _nAudio, _nVideo, _nInst;
    private static int _pendingOrder;
    private static float _pendingTimeout;
    private static bool _hooked;

    // -- 在任何一個 Router 存活時，確保有掛 sceneLoaded 事件
    private void Awake()
    {
        if (!_hooked)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _hooked = true;
        }
    }

    // ===== 給 Button.OnClick 用 =====
    public void OpenSettings() => Launch(Action.Setting);
    public void OpenAudio() => Launch(Action.Audio);
    public void OpenVideo() => Launch(Action.Video);
    public void OpenInstructions() => Launch(Action.Inst);

    private void Launch(Action act)
    {
        // 將必要參數寫入靜態（跨場景保留）
        _pending = act;
        _pendingScene = settingsSceneName;
        _nSetting = settingUIName;
        _nAudio = audioUIName;
        _nVideo = videoUIName;
        _nInst = instUIName;
        _pendingOrder = settingsCanvasSortingOrder;
        _pendingTimeout = waitTimeoutSeconds;

        if (enableLogs) Debug.Log($"[SettingsRouter] Go → scene={_pendingScene}, act={_pending}");

        // 同場景就地執行；不同場景先切換
        if (SceneManager.GetActiveScene().name == _pendingScene)
        {
            // 直接嘗試在本場景開（建立一個跑協程的 Host）
            CreateHostAndRun();
        }
        else
        {
            SceneManager.LoadScene(_pendingScene, LoadSceneMode.Single);
        }
    }

    // 場景載入完成時觸發
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pending != Action.None && scene.name == _pendingScene)
        {
            CreateHostAndRun();
        }
    }

    // 生成一個臨時 Host 來跑協程（因為 sceneLoaded 是靜態事件）
    private static void CreateHostAndRun()
    {
        var hostGO = new GameObject("SettingsRouterHost");
        var host = hostGO.AddComponent<SettingsRouterHost>();
        host.StartCoroutine(host.TryOpenRoutine(_pending, _pendingScene, _nSetting, _nAudio, _nVideo, _nInst, _pendingOrder, _pendingTimeout));
    }

    // ====== 內部 Host ======
    private class SettingsRouterHost : MonoBehaviour
    {
        public IEnumerator TryOpenRoutine(Action act, string sceneName,
                                          string nSetting, string nAudio, string nVideo, string nInst,
                                          int sortingOrder, float timeout)
        {
            float t = 0f;
            bool done = false;

            while (t < timeout && !done)
            {
                // 1) 優先嘗試 UI_Manager
                var mgr = UI_Manager.Instance;
                if (mgr != null)
                {
                    // 等一幀讓 A001 的 Start 跑完
                    yield return null;

                    switch (act)
                    {
                        case Action.Setting: mgr.ShowSettingUI(); break;
                        case Action.Audio: mgr.ShowAudioSettingUI(); break;
                        case Action.Video: mgr.ShowVideoSettingUI(); break;
                        case Action.Inst: mgr.ShowInstructions(); break;
                    }

                    done = true;
                    break;
                }

                // 2) 後備：直接用「名稱」在該場景找到 UI 物件並打開
                var sc = SceneManager.GetSceneByName(sceneName);
                if (sc.IsValid() && sc.isLoaded)
                {
                    GameObject target = null;
                    GameObject setGO = FindInScene(sc, nSetting);
                    GameObject audGO = string.IsNullOrEmpty(nAudio) ? null : FindInScene(sc, nAudio);
                    GameObject vidGO = string.IsNullOrEmpty(nVideo) ? null : FindInScene(sc, nVideo);
                    GameObject insGO = string.IsNullOrEmpty(nInst) ? null : FindInScene(sc, nInst);

                    switch (act)
                    {
                        case Action.Setting: target = setGO; break;
                        case Action.Audio: target = audGO; break;
                        case Action.Video: target = vidGO; break;
                        case Action.Inst: target = insGO; break;
                    }

                    if (target != null)
                    {
                        // 取得 Canvas 根，全部打開
                        var root = target.transform.root.gameObject;
                        if (!root.activeSelf) root.SetActive(true);

                        // 讓它在最上層
                        foreach (var c in root.GetComponentsInChildren<Canvas>(true))
                        {
                            c.overrideSorting = true;
                            c.sortingOrder = sortingOrder;
                        }

                        // 關閉其他子頁，只開目標頁
                        SafeSetActive(audGO, false);
                        SafeSetActive(vidGO, false);
                        SafeSetActive(insGO, false);
                        SafeSetActive(setGO, act == Action.Setting); // 若只是 Setting 總頁

                        SafeSetActive(target, true);

                        done = true;
                        break;
                    }
                }

                t += Time.unscaledDeltaTime;
                yield return null; // 下一幀再試
            }

            if (!done)
            {
                Debug.LogError("[SettingsRouter] 無法開啟設定頁：\n" +
                               "- 請確認 A001 內存在 UI_Manager（且能開對應頁），\n" +
                               "  或名稱後備是否與實際物件名稱一致（含大小寫/底線）。");
            }

            // 清除 pending 狀態並自毀 Host
            _pending = Action.None;
            Destroy(gameObject);
        }

        private static void SafeSetActive(GameObject go, bool v)
        {
            if (go && go.activeSelf != v) go.SetActive(v);
        }

        // 在指定場景的 Root 之下遞迴找名字（包含未啟用物件）
        private static GameObject FindInScene(Scene sc, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var roots = sc.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var t = FindDeep(roots[i].transform, name);
                if (t) return t.gameObject;
            }
            return null;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (!parent) return null;
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                var f = FindDeep(c, name);
                if (f) return f;
            }
            return null;
        }
    }
}
