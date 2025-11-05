using UnityEngine;
using UnityEngine.Rendering.Universal; // 加入以使用 Light2D

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

    


    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        
        ApplySprite(false);

        AutoFindUISkill();
    }

    private void AutoFindUISkill()
    {
        if (skillsUIRootFallback != null)
            return; // 已手動指定則不再尋找

        // 嘗試尋找名為 "UI" 的物件
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot != null)
        {
            Transform skillUI = uiRoot.transform.Find("UI_Skill");
            if (skillUI != null)
            {
                skillsUIRootFallback = skillUI.gameObject;
                if (showDebugLogs) Debug.Log($"[UI_SwitchToOpenSkills] 已自動綁定 UI_Skill: {skillsUIRootFallback.name}", this);
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("[UI_SwitchToOpenSkills] 找不到子物件 UI_Skill", this);
            }
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("[UI_SwitchToOpenSkills] 找不到名為 'UI' 的物件", this);
        }
    }


    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        bool pressedE_Old = Input.GetKeyDown(KeyCode.F);
        if (IsPlayerInRange && pressedE_Old)
        {
            TryOpenSkillsUI();
        }
    }

    private void TryOpenSkillsUI()
    {
        if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 嘗試開啟 Skills UI", this);

        //  找到玩家並重置狀態
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ResetOnRespawn();
                if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 玩家狀態已重置 (ResetOnRespawn)", this);
            }
        }

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

        if (inRange && onSprite != null) 
            spriteRenderer.sprite = onSprite; 

        else if (!inRange && offSprite != null) 
            spriteRenderer.sprite = offSprite;

    }
}