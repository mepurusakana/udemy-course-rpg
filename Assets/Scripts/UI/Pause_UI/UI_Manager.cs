using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UI_Manager : MonoBehaviour
{
    // ========== 單例（全域存取） ==========
    public static UI_Manager Instance { get; private set; }

    [Header("主 UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingUI;

    [Header("子設定頁面")]
    [SerializeField] private GameObject audioSettingUI;
    [SerializeField] private GameObject videoSettingUI;
    [SerializeField] private GameObject instructionsUI;
    [SerializeField] private string targetSceneName;

    [Header("其他 UI")]
    [Tooltip("遊戲中 HUD（可留空）。ContinueGame 時會打開。")]
    [SerializeField] private GameObject inGameUI;

    [Header("對話 UI")]
    [Tooltip("指到場景中的 UI_Dialogue 物件")]
    [SerializeField] private UI_Dialogue uiDialogue;
    [Tooltip("對話期間是否暫停 Time.timeScale")]
    [SerializeField] private bool pauseDuringDialogue = true;

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
        // 如需跨場景常駐可開啟：
        // DontDestroyOnLoad(gameObject);

        if (enableLogs) Debug.Log("[UI_Manager] Awake OK.", this);
    }

    // ========== 主/子選單控制 ==========
    public void ShowPauseMenu()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ShowPauseMenu", this);
        HideAll();
        SetActiveSafe(pauseMenuUI, true);
        Time.timeScale = 0f;
    }

    public void ShowSetting()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ShowSetting", this);
        HideAll();
        SetActiveSafe(settingUI, true);
    }

    public void ShowAudioSetting()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ShowAudioSetting", this);
        HideAll();
        SetActiveSafe(audioSettingUI, true);
    }

    public void ShowVideoSetting()
    {
        if (enableLogs) Debug.Log("[UI_Manager] ShowVideoSetting", this);
        HideAll();
        SetActiveSafe(videoSettingUI, true);
    }

    public void ShowInstructions()
    {
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
    }

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (enableLogs) Debug.Log($"[UI_Manager] LoadTargetScene: {targetSceneName}", this);
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[UI_Manager] 請在 Inspector 設定 targetSceneName！", this);
        }
    }

    // ========== 對話入口（給 NPC / 任何腳本呼叫） ==========
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

        // 顯示對話前先清畫面，避免互相遮擋
        HideAll();

        if (pauseDuringDialogue) Time.timeScale = 0f;

        uiDialogue.Open(firstLine, () =>
        {
            if (pauseDuringDialogue) Time.timeScale = 1f;

            // 關閉對話後回到 HUD（如不需要可移除）
            SetActiveSafe(inGameUI, true);

            if (enableLogs) Debug.Log("[UI_Manager] Dialogue closed callback.", this);
            onClosed?.Invoke();
        });
    }

    // ========== Skills UI 開關 ==========
    public void ShowSkillsUI()
    {
        if (uiSkills == null)
        {
            Debug.LogError("[UI_Manager] uiSkills 未指派！", this);
            return;
        }
        HideAll();                               // 先把其它 UI 收起來
        if (pauseDuringSkills) Time.timeScale = 0f;

        uiSkills.SetActive(true);
        if (enableLogs) Debug.Log("[UI_Manager] ShowSkillsUI", this);
    }

    public void HideSkillsUI()
    {
        if (uiSkills != null) uiSkills.SetActive(false);
        if (pauseDuringSkills) Time.timeScale = 1f;

        // 回到 HUD（可依需求移除）
        SetActiveSafe(inGameUI, true);

        if (enableLogs) Debug.Log("[UI_Manager] HideSkillsUI", this);
    }

    // ========== （可選）互動提示：配合 UI_SwitchToOpenSkills ==========
    // 若要使用，把 UI_SwitchToOpenSkills 內註解解除呼叫這兩個方法即可。
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

    // ========== 內部工具 ==========
    private void HideAll()
    {
        SetActiveSafe(pauseMenuUI, false);
        SetActiveSafe(settingUI, false);
        SetActiveSafe(audioSettingUI, false);
        SetActiveSafe(videoSettingUI, false);
        SetActiveSafe(instructionsUI, false);
        SetActiveSafe(inGameUI, false);

        // 對話 UI 若正在開啟，也一併關閉，確保狀態乾淨
        if (uiDialogue != null && uiDialogue.IsOpen)
        {
            if (enableLogs) Debug.Log("[UI_Manager] HideAll → close uiDialogue", this);
            uiDialogue.Close();
        }

        // Skills 面板也關掉（避免殘留）
        SetActiveSafe(uiSkills, false);

        // 互動提示也關掉
        SetActiveSafe(skillsHintUI, false);
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}