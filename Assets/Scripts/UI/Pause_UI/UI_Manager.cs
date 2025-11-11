using System;                           // Action 等委派
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class UI_Manager : MonoBehaviour
{
    // ========== 單例（Singleton：全域唯一） ==========
    public static UI_Manager Instance { get; private set; }

    

    [Header("主 UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingUI;

    [Header("子設定頁面")]
    [SerializeField] private GameObject audioSettingUI;
    [SerializeField] private GameObject videoSettingUI;
    [SerializeField] private GameObject instructionsUI;
    [SerializeField] private string targetSceneName;  // Continue / Load 選單要前往的場景名

    [Header("其他 UI")]
    [Tooltip("遊戲中 HUD（可留空）。ContinueGame 時會打開。")]
    [SerializeField] private GameObject inGameUI;

    [Header("對話 UI")]
    [Tooltip("指到場景中的 UI_Dialogue 物件")]
    [SerializeField] private UI_Dialogue uiDialogue;
    [Tooltip("對話期間是否暫停 Time.timeScale")]
    [SerializeField] private bool pauseDuringDialogue = true;

    [Header("AudioManager")]
    public AudioManager audioManager;

    [Header("（可選）互動提示 UI")]
    [Tooltip("例如『按 E 開啟技能』之類的小提示；沒有可留空")]
    [SerializeField] private GameObject skillsHintUI;

    [Header("Skills UI")]
    [Tooltip("指到 Skills 面板（例如 UI_Skill 根物件）")]
    [SerializeField] private GameObject uiSkills;
    [Tooltip("開啟 Skills UI 是否暫停時間")]
    [SerializeField] private bool pauseDuringSkills = true;

    [Header("偵錯")]
    [SerializeField] private bool enableLogs = true;

    // ====== 方案A：跨場景設定 UI 的「自動尋找 / 動態生成 / 後備載入 / 綁定回填」 ======
    [Header("跨場景設定 UI 管理（方案A）")]
    [Tooltip("若場景裡沒綁引用，是否用名稱自動尋找 SettingUI/AudioSettingUI/VideoSettingUI")]
    [SerializeField] private bool autoFindSettingsByName = true;

    [Tooltip("找不到時，是否自動生成一份設定 Canvas Prefab（會設為 DontDestroyOnLoad）")]
    [SerializeField] private bool autoSpawnSettingsIfMissing = true;

    [Tooltip("設定 Canvas 預置（Prefab）。內部需含三頁：SettingUI / AudioSettingUI / VideoSettingUI")]
    [SerializeField] private GameObject settingsCanvasPrefab;

    // ★ 新增：如果沒有在 Inspector 指定 Prefab，會嘗試從 Resources 後備載入
    [Tooltip("Resources 後備路徑（例如 Resources/UI/SettingsCanvas.prefab → 填 UI/SettingsCanvas）")]
    [SerializeField] private string settingsCanvasResourcePath = "UI/SettingsCanvas";

    [Header("（選用）名稱搜尋對應")]
    [SerializeField] private string settingUIName = "SettingUI";
    [SerializeField] private string audioSettingUIName = "AudioSettingUI";
    [SerializeField] private string videoSettingUIName = "VideoSettingUI";
    [SerializeField] private string instructionsUIName = "InstructionsUI";

    private GameObject _spawnedSettingsCanvas; // 動態生成的 Canvas 快取

    // ====== 跨場景 UI：以『附加載入（Additive）』方式顯示的 bgstart Canvas ======
    [Header("跨場景 UI（例如：bgstart Canvas）")]
    [Tooltip("包含 bgstart Canvas 的場景名稱（必須加入 Build Settings）")]
    [SerializeField] private string bgStartSceneName = ""; // 例：TitleScene 或 MainMenu
    [Tooltip("要控制的 Canvas 物件名稱（大小寫一致），例：BG_Start 或 bgstart")]
    [SerializeField] private string bgStartCanvasName = "bgstart";
    [Tooltip("呼叫 Hide 時是否直接卸載該附加場景（較省資源）")]
    [SerializeField] private bool unloadBgStartSceneWhenHidden = true;
    [Tooltip("執行 HideAll() 時是否一併關閉 bgstart Canvas")]
    [SerializeField] private bool includeBGStartInHideAll = false;

    [Header("場景分類")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // 主選單場景名稱
    [SerializeField] private string[] gameplaySceneNames = { "A001", "B001" }; // 關卡場景清單

    // 內部狀態
    private GameObject _bgStartCanvasCached;  // 快取找到的 Canvas
    private bool _isLoadingBGStart = false;   // 正在載入保護旗標

    // ========== 生命週期 ==========
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (enableLogs) Debug.LogWarning("[UI_Manager] Duplicate found, destroy this.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 跨場景不銷毀（DontDestroyOnLoad）
        DontDestroyOnLoad(gameObject);

        if (enableLogs) Debug.Log("[UI_Manager] Awake OK.", this);
    }

    private void Start()
    {
        TryBindAudioManager();   // 預先快取；若之後場景換了，行為方法裡還會再自動重綁
    }


    // ========== 主/子選單控制 ==========
    public void ShowPauseMenu()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ShowPauseMenu", this);

        HideAll();
        SetActiveSafe(pauseMenuUI, true);
        Time.timeScale = 0f;

        // === 音樂 ===
        TryBindAudioManager();
        if (audioManager != null)
        {
            // 關閉 BGM 旗標，並立即停止所有 BGM
            audioManager.playBgm = false;
            audioManager.StopAllBGM();
        }
    }

    public void ShowSetting()
    {
        if (!EnsureSettingsUIAvailable()) return;
        if (enableLogs) Debug.Log("[UI_Manager] ShowSetting", this);
        HideAll();
        SetActiveSafe(settingUI, true);
    }

    public void ShowAudioSetting()
    {
        if (!EnsureSettingsUIAvailable()) return;
        if (enableLogs) Debug.Log("[UI_Manager] ShowAudioSetting", this);
        HideAll();
        SetActiveSafe(settingUI, true);      // 可先顯示總頁
        SetActiveSafe(audioSettingUI, true);
    }

    public void ShowVideoSetting()
    {
        if (!EnsureSettingsUIAvailable()) return;
        if (enableLogs) Debug.Log("[UI_Manager] ShowVideoSetting", this);
        HideAll();
        SetActiveSafe(settingUI, true);      // 可先顯示總頁
        SetActiveSafe(videoSettingUI, true);
    }

    public void ShowInstructions()
    {
        if (!EnsureSettingsUIAvailable()) return;
        if (enableLogs) Debug.Log("[UI_Manager] ShowInstructions", this);
        HideAll();
        SetActiveSafe(instructionsUI, true);
    }

    public void ContinueGame()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ContinueGame", this);

        HideAll();
        Time.timeScale = 1f;
        SetActiveSafe(inGameUI, true);

        // === 音樂 ===
        TryBindAudioManager();
        if (audioManager != null)
        {
            // 打開 BGM 旗標即可；AudioManager.Update() 會自動續播目前曲目
            audioManager.playBgm = true;
            // 不額外挑曲，避免換歌；若你想每次續玩都換曲，可加：am.PlayRandomBGM();
        }
    }

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (enableLogs) Debug.Log($"[UI_Manager] LoadTargetScene: {targetSceneName}", this);
            HideAll();
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[UI_Manager] 請在 Inspector 設定 targetSceneName！", this);
        }
    }

    // ========== 對話入口 ==========
    public void OpenDialogue(DialogueLineSO firstLine, Action onClosed = null)
    {
        if (enableLogs) Debug.Log("[UI_Manager] OpenDialogue called.", this);

        if (uiDialogue == null)
        {
            Debug.LogError("[UI_Manager] uiDialogue 未指派！請把場景中的 UI_Dialogue 指到此欄位。", this);
            return;
        }
        if (firstLine == null)
        {
            Debug.LogError("[UI_Manager] firstLine 為 null！", this);
            return;
        }

        HideAll();
        if (pauseDuringDialogue) Time.timeScale = 0f;

        uiDialogue.Open(firstLine, () =>
        {
            if (pauseDuringDialogue) Time.timeScale = 1f;
            SetActiveSafe(inGameUI, true);
            if (enableLogs) Debug.Log("[UI_Manager] Dialogue closed callback.", this);
            onClosed?.Invoke();
        });
    }

    // ========== Skills UI ==========
    public void ShowSkillsUI()
    {
        if (uiSkills == null)
        {
            Debug.LogError("[UI_Manager] uiSkills 未指派！", this);
            return;
        }
        HideAll();
        if (pauseDuringSkills) Time.timeScale = 0f;

        uiSkills.SetActive(true);
        if (enableLogs) Debug.Log("[UI_Manager] ShowSkillsUI", this);

    }

    public void HideSkillsUI()
    {
        if (uiSkills != null) uiSkills.SetActive(false);
        if (pauseDuringSkills) Time.timeScale = 1f;
        SetActiveSafe(inGameUI, true);
        if (enableLogs) Debug.Log("[UI_Manager] HideSkillsUI", this);
    }

    // ========== （可選）互動提示 ==========
    public void OnPlayerEnterOpenSkillsArea(MonoBehaviour source)
    {
        if (skillsHintUI) skillsHintUI.SetActive(true);
        if (enableLogs) Debug.Log($"[UI_Manager] Show skills hint (from {source.name})", this);
    }

    public void OnPlayerExitOpenSkillsArea(MonoBehaviour source)
    {
        if (skillsHintUI) skillsHintUI.SetActive(false);
        if (enableLogs) Debug.Log($"[UI_Manager] Hide skills hint (from {source.name})", this);
    }

    // ========== 跨場景 bgstart 顯示/隱藏 ==========
    /// <summary>顯示 bgstart（必要時先用『附加載入（Additive）』把含有它的場景載入）</summary>
    public void ShowBGStart() => StartCoroutine(ShowBGStartRoutine());

    /// <summary>隱藏 bgstart（可選擇一併卸載附加載入的場景）</summary>
    public void HideBGStart() => StartCoroutine(HideBGStartRoutine());

    /// <summary>切換 bgstart 顯示/隱藏</summary>
    public void ToggleBGStart(bool on)
    {
        if (on) ShowBGStart();
        else HideBGStart();
    }

    private IEnumerator ShowBGStartRoutine()
    {
        if (string.IsNullOrEmpty(bgStartSceneName))
        {
            Debug.LogError("[UI_Manager] bgStartSceneName 未設定！");
            yield break;
        }
        if (string.IsNullOrEmpty(bgStartCanvasName))
        {
            Debug.LogError("[UI_Manager] bgStartCanvasName 未設定！");
            yield break;
        }

        // 若該場景尚未載入，先『附加載入』（Additive）
        Scene scene = SceneManager.GetSceneByName(bgStartSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (_isLoadingBGStart)
            {
                while (_isLoadingBGStart) yield return null;
            }
            else
            {
                _isLoadingBGStart = true;
                AsyncOperation op = SceneManager.LoadSceneAsync(bgStartSceneName, LoadSceneMode.Additive);
                if (op == null)
                {
                    Debug.LogError($"[UI_Manager] 無法附加載入場景：{bgStartSceneName}");
                    _isLoadingBGStart = false;
                    yield break;
                }
                while (!op.isDone) yield return null;
                _isLoadingBGStart = false;
                scene = SceneManager.GetSceneByName(bgStartSceneName);
            }
        }

        // 在該場景中尋找指定 Canvas（名稱遞迴搜尋）
        if (_bgStartCanvasCached == null)
        {
            _bgStartCanvasCached = FindCanvasInSceneByName(scene, bgStartCanvasName);
            if (_bgStartCanvasCached == null)
            {
                Debug.LogError($"[UI_Manager] 在場景「{bgStartSceneName}」找不到名為「{bgStartCanvasName}」的物件！");
                yield break;
            }
        }

        _bgStartCanvasCached.SetActive(true);
        if (enableLogs) Debug.Log("[UI_Manager] ShowBGStart → Active(true)", this);
    }

    private IEnumerator HideBGStartRoutine()
    {
        if (_bgStartCanvasCached != null) _bgStartCanvasCached.SetActive(false);

        if (unloadBgStartSceneWhenHidden && !string.IsNullOrEmpty(bgStartSceneName))
        {
            Scene scene = SceneManager.GetSceneByName(bgStartSceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                if (op != null)
                {
                    while (!op.isDone) yield return null;
                }
                _bgStartCanvasCached = null;
                if (enableLogs) Debug.Log("[UI_Manager] HideBGStart → Unload additive scene & clear cache", this);
            }
        }
        else
        {
            if (enableLogs) Debug.Log("[UI_Manager] HideBGStart → Active(false)", this);
        }
    }

    private GameObject FindCanvasInSceneByName(Scene scene, string name)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var found = FindDeepChildByName(roots[i].transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private Transform FindDeepChildByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        // 迭代（BFS）找子孫
        Queue<Transform> q = new Queue<Transform>();
        q.Enqueue(parent);
        while (q.Count > 0)
        {
            var t = q.Dequeue();
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++) q.Enqueue(t.GetChild(i));
        }
        return null;
    }

    // ========== 內部工具 ==========

    /// <summary>
    /// ★ 新增：給外部（例如 SettingsCanvas 上的 Binder）回填引用。
    /// 若你的 Prefab 名稱不是 SettingUI/AudioSettingUI/VideoSettingUI，也能確保綁到正確頁面。
    /// </summary>
    public void BindSettingsUI(GameObject setting, GameObject audio, GameObject video, GameObject instructions = null)
    {
        settingUI = setting;
        audioSettingUI = audio;
        videoSettingUI = video;
        if (instructions != null) instructionsUI = instructions;
        if (enableLogs) Debug.Log("[UI_Manager] Settings UI 已由 Binder 綁定。", this);
    }

    /// <summary>
    /// 確保設定 UI 可以在「任何場景」使用：
    /// 1) 已綁 → 用之
    /// 2) 允許自動尋找 → 以名稱在所有已載入場景找（含 Inactive）
    /// 3) 允許動態生成 → Instantiate 設定 Canvas（Inspector 或 Resources 後備），並 DontDestroyOnLoad
    /// </summary>
    private bool EnsureSettingsUIAvailable()
    {
        // 1) 已經有綁到（主場景那份）就直接用
        if (settingUI && audioSettingUI && videoSettingUI)
            return true;

        // 2) 自動尋找（by name）
        if (autoFindSettingsByName)
        {
            if (!settingUI) settingUI = FindByNameGlobally(settingUIName);
            if (!audioSettingUI) audioSettingUI = FindByNameGlobally(audioSettingUIName);
            if (!videoSettingUI) videoSettingUI = FindByNameGlobally(videoSettingUIName);
            if (!instructionsUI && !string.IsNullOrEmpty(instructionsUIName))
                instructionsUI = FindByNameGlobally(instructionsUIName);

            if (settingUI && audioSettingUI && videoSettingUI)
                return true;
        }

        // 3) 動態生成（Instantiate Prefab）
        if (autoSpawnSettingsIfMissing)
        {
            // ★ 後備：若 Inspector 沒指定 Prefab，改用 Resources 路徑載入
            if (settingsCanvasPrefab == null && !string.IsNullOrEmpty(settingsCanvasResourcePath))
            {
                var loaded = Resources.Load<GameObject>(settingsCanvasResourcePath);
                if (loaded != null)
                {
                    settingsCanvasPrefab = loaded;
                    if (enableLogs) Debug.Log("[UI_Manager] 已從 Resources 載入 SettingsCanvas Prefab。");
                }
                else
                {
                    Debug.LogWarning($"[UI_Manager] Resources 未找到：{settingsCanvasResourcePath}");
                }
            }

            if (settingsCanvasPrefab != null && _spawnedSettingsCanvas == null)
            {
                _spawnedSettingsCanvas = Instantiate(settingsCanvasPrefab);
                DontDestroyOnLoad(_spawnedSettingsCanvas);
                if (enableLogs) Debug.Log("[UI_Manager] 已動態生成 SettingsCanvas（DontDestroyOnLoad）", _spawnedSettingsCanvas);
            }

            // 生成後再抓一遍（依名稱）
            if (_spawnedSettingsCanvas != null)
            {
                if (!settingUI) settingUI = FindUnder(_spawnedSettingsCanvas.transform, settingUIName);
                if (!audioSettingUI) audioSettingUI = FindUnder(_spawnedSettingsCanvas.transform, audioSettingUIName);
                if (!videoSettingUI) videoSettingUI = FindUnder(_spawnedSettingsCanvas.transform, videoSettingUIName);
                if (!instructionsUI && !string.IsNullOrEmpty(instructionsUIName))
                    instructionsUI = FindUnder(_spawnedSettingsCanvas.transform, instructionsUIName);

                if (settingUI && audioSettingUI && videoSettingUI)
                    return true;
            }
        }

        Debug.LogError("[UI_Manager] 找不到 SettingUI/AudioSettingUI/VideoSettingUI。請在 Inspector 綁引用、或開啟 autoFindSettingsByName、或指定 settingsCanvasPrefab/Resources 後備。", this);
        return false;
    }

    private GameObject FindByNameGlobally(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var root in GetAllLoadedSceneRoots())
        {
            var t = FindDeepChildByName(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private GameObject FindUnder(Transform root, string name)
    {
        if (!root || string.IsNullOrEmpty(name)) return null;
        var t = FindDeepChildByName(root, name);
        return t ? t.gameObject : null;
    }

    private IEnumerable<GameObject> GetAllLoadedSceneRoots()
    {
        int count = SceneManager.sceneCount;
        for (int i = 0; i < count; i++)
        {
            var sc = SceneManager.GetSceneAt(i);
            if (!sc.IsValid() || !sc.isLoaded) continue;
            foreach (var go in sc.GetRootGameObjects()) yield return go;
        }
    }

    // === 加在方法區 ===
    private void TryBindAudioManager()
    {
        if (audioManager != null) return;

        // 依序嘗試：單例 / 場景查找 (含 inactive)
        audioManager = AudioManager.instance
             ?? AudioManager.InstanceInScene
#if UNITY_2022_1_OR_NEWER
             ?? UnityEngine.Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
#else
         ?? UnityEngine.Object.FindObjectOfType<AudioManager>(true);
#endif

        if (enableLogs) Debug.Log(audioManager ? "[UI_Manager] AudioManager 綁定完成" : "[UI_Manager] 場景中找不到 AudioManager", this);
    }


    private void HideAll()
    {
        SetActiveSafe(pauseMenuUI, false);
        SetActiveSafe(settingUI, false);
        SetActiveSafe(audioSettingUI, false);
        SetActiveSafe(videoSettingUI, false);
        SetActiveSafe(instructionsUI, false);
        SetActiveSafe(inGameUI, false);

        if (uiDialogue != null && uiDialogue.IsOpen)
        {
            if (enableLogs) Debug.Log("[UI_Manager] HideAll → close uiDialogue", this);
            uiDialogue.Close();
        }

        SetActiveSafe(uiSkills, false);
        SetActiveSafe(skillsHintUI, false);

        // （可選）一起處理 bgstart
        if (includeBGStartInHideAll && _bgStartCanvasCached != null)
        {
            _bgStartCanvasCached.SetActive(false);
        }
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    // === 別名（alias）：Inspector / 其他腳本可用統一命名呼叫 ===
    public void ShowSettingUI() => ShowSetting();
    public void ShowAudioSettingUI() => ShowAudioSetting();
    public void ShowVideoSettingUI() => ShowVideoSetting();
}
