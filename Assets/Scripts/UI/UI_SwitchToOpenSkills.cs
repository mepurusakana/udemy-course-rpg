using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 新輸入系統
#endif

public class UI_SwitchToOpenSkills : MonoBehaviour
{
    [Header("觸發設定")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool notifyOnExit = true;

    [Header("圖像切換")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite offSprite;   // 玩家不在範圍內
    [SerializeField] private Sprite onSprite;    // 玩家在範圍內

    [Header("UI 參考（後備用）")]
    [Tooltip("當 UI_Manager.Instance 為 null 時，直接打開這個物件驗證 UI 是否能顯示")]
    [SerializeField] private GameObject skillsUIRootFallback;

    [Header("除錯")]
    [SerializeField] private bool showDebugLogs = true;

    public bool IsPlayerInRange { get; private set; } = false;

#if ENABLE_INPUT_SYSTEM
    // 新輸入系統：可在 Inspector 綁定，或在 Awake 內動態建立
    [Header("Input System（新）")]
    [SerializeField] private InputAction openSkillsAction; // 綁定鍵位：<Keyboard>/e
#endif

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ApplySprite(false);

#if ENABLE_INPUT_SYSTEM
         若未在 Inspector 綁定，動態建立一個 E 鍵
        if (openSkillsAction == null || openSkillsAction.bindings.Count == 0)
        {
            openSkillsAction = new InputAction("OpenSkills", InputActionType.Button);
            openSkillsAction.AddBinding("<Keyboard>/e");
            // 也可加手把鍵位：
            // openSkillsAction.AddBinding("<Gamepad>/south"); // A 鍵 (Xbox)
        }
        openSkillsAction.Enable();
        if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] InputAction enabled (新輸入系統)", this);
#endif
    }

    private void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        if (openSkillsAction != null) openSkillsAction.Disable();
#endif
    }

    private void Update()
    {
        // ---- 舊輸入系統 ----
        bool pressedE_Old = Input.GetKeyDown(KeyCode.E);

        // ---- 新輸入系統（若有啟用）----
        bool pressedE_New = false;
#if ENABLE_INPUT_SYSTEM
        if (openSkillsAction != null)
            pressedE_New = openSkillsAction.WasPerformedThisFrame();
#endif

        if (IsPlayerInRange && (pressedE_Old || pressedE_New))
        {
            TryOpenSkillsUI();
        }
    }

    private void TryOpenSkillsUI()
    {
        if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 嘗試開啟 Skills UI", this);

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.ShowSkillsUI();
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已呼叫 UI_Manager.ShowSkillsUI()", this);
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("[UI_SwitchToOpenSkills] UI_Manager.Instance 為 null，改用後備物件", this);
            if (skillsUIRootFallback != null)
            {
                skillsUIRootFallback.SetActive(true);
                if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 後備 Skills UI 已 SetActive(true)", this);
            }
            else
            {
                Debug.LogError("[UI_SwitchToOpenSkills] 沒有 UI_Manager、也沒有指定後備 skillsUIRootFallback。");
            }
        }
    }

    // ===== 2D Trigger =====
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerInRange = true;
        ApplySprite(true);
        if (showDebugLogs) Debug.Log($"[UI_SwitchToOpenSkills] 玩家進入範圍 (2D)", this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        IsPlayerInRange = false;
        ApplySprite(false);
        if (showDebugLogs) Debug.Log($"[UI_SwitchToOpenSkills] 玩家離開範圍 (2D)", this);

        if (notifyOnExit)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.HideSkillsUI();
                if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已呼叫 UI_Manager.HideSkillsUI()", this);
            }
            else if (skillsUIRootFallback != null)
            {
                skillsUIRootFallback.SetActive(false);
                if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 後備 Skills UI 已 SetActive(false)", this);
            }
        }
    }

    // ===== 圖片切換 =====
    private void ApplySprite(bool inRange)
    {
        if (spriteRenderer == null) return;

        if (inRange && onSprite != null) spriteRenderer.sprite = onSprite;
        else if (!inRange && offSprite != null) spriteRenderer.sprite = offSprite;
    }
}