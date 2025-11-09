using UnityEngine;
using UnityEngine.Rendering.Universal; // 加入以使用 Light2D
using System.Collections;
using System.Collections.Generic;

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

        //AutoFindUISkill();
    }

    private void OnEnable()
    {
        //  監聽 UI_Skill 載入事件
        UI.OnSkillUILoaded += BindSkillUI;
    }

    private void OnDisable()
    {
        UI.OnSkillUILoaded -= BindSkillUI;
    }

    private void Start()
    {
        //  嘗試立即綁定（包含 inactive 物件）
        TryFindSkillUI();
    }

    //private void AutoFindUISkill()
    //{
    //    if (skillsUIRootFallback != null)
    //        return; // 已手動指定則不再尋找

    //    // 嘗試尋找名為 "UI" 的物件
    //    GameObject uiRoot = GameObject.Find("UI");
    //    if (uiRoot != null)
    //    {
    //        Transform skillUI = uiRoot.transform.Find("UI_Skill");
    //        if (skillUI != null)
    //        {
    //            skillsUIRootFallback = skillUI.gameObject;
    //            if (showDebugLogs) Debug.Log($"[UI_SwitchToOpenSkills] 已自動綁定 UI_Skill: {skillsUIRootFallback.name}", this);
    //        }
    //        else
    //        {
    //            if (showDebugLogs) Debug.LogWarning("[UI_SwitchToOpenSkills] 找不到子物件 UI_Skill", this);
    //        }
    //    }
    //    else
    //    {
    //        if (showDebugLogs) Debug.LogWarning("[UI_SwitchToOpenSkills] 找不到名為 'UI' 的物件", this);
    //    }
    //}


    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        if (skillsUIRootFallback == null)
            TryFindSkillUI();

        if (IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            bool isActive = skillsUIRootFallback.activeSelf;

            if (isActive)
            {
                // 若已開啟則關閉
                CloseSkillsUI();
            }
            else
            {
                //  若未開啟則打開
                TryOpenSkillsUI();
            }
        }
    }

    private void TryFindSkillUI()
    {
        // 首先從 UI 單例取得
        if (UI.instance != null && UI.instance.UI_Skill != null)
        {
            skillsUIRootFallback = UI.instance.UI_Skill;
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已從 UI.instance 綁定 UI_Skill", this);
            return;
        }

        //  若 UI.instance 還沒初始化，用 FindObjectOfType (含 inactive)
        var foundSkillUI = FindObjectOfType<TwoStateButtonGroup>(true);
        if (foundSkillUI != null)
        {
            skillsUIRootFallback = foundSkillUI.gameObject;
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已透過 FindObjectOfType 綁定 UI_Skill", this);
        }
    }

    private void BindSkillUI(GameObject ui)
    {
        skillsUIRootFallback = ui;
        if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已透過事件綁定 UI_Skill", this);
    }



    private void TryOpenSkillsUI()
    {
        if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 嘗試開啟 Skills UI", this);

        // 找到玩家並重置狀態（可省略）
        var player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ResetOnRespawn();
                if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 玩家狀態已重置", this);
            }
        }

        //  優先透過 UI.instance 控制
        if (UI.instance != null)
        {
            UI.instance.ShowSkillsUI();
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已呼叫 UI.ShowSkillsUI()", this);
        }
        else if (skillsUIRootFallback != null)
        {
            skillsUIRootFallback.SetActive(true);
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 後備 Skills UI 已 SetActive(true)", this);
        }
        else
        {
            Debug.LogError("[UI_SwitchToOpenSkills] 沒有 UI.instance 也沒有後備 UI_Skill");
        }

    }

    private void CloseSkillsUI()
    {
        if (UI.instance != null)
        {
            UI.instance.HideSkillsUI();
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已關閉 Skills UI (經由 UI.instance)", this);
        }
        else if (skillsUIRootFallback != null)
        {
            skillsUIRootFallback.SetActive(false);
            if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已關閉 Skills UI (後備)", this);
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