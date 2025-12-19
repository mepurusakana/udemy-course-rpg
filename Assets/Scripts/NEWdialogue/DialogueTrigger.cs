using UnityEngine;
using TMPro;

/// <summary>
/// 觸發模式
/// </summary>
public enum TriggerMode
{
    PressKey,       // 按鍵觸發（進入範圍後按鍵）
    AutoTrigger     // 自動觸發（進入範圍自動播放）
}

/// <summary>
/// 對話觸發器
/// 可掛在任何物件上，支援按鍵觸發或自動觸發
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("觸發設定")]
    [Tooltip("觸發模式：按鍵觸發或自動觸發")]
    public TriggerMode triggerMode = TriggerMode.PressKey;

    [Tooltip("觸發按鍵（僅按鍵模式有效）")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("玩家標籤（通常是 Player）")]
    public string playerTag = "Player";

    [Header("對話系統")]
    [Tooltip("對話系統參考（留空會自動尋找）")]
    public DialogueSystem dialogueSystem;

    [Header("對話內容")]
    [Tooltip("這個觸發器的對話資料")]
    public DialogueData[] dialogues;

    [Header("重複觸發設定")]
    [Tooltip("是否允許重複觸發")]
    public bool canRepeat = false;

    [Tooltip("重複觸發的冷卻時間（秒）")]
    public float repeatCooldown = 2f;

    [Header("互動提示 UI（可選）")]
    [Tooltip("互動提示文字物件（例如：按 F 互動）")]
    public GameObject interactHintUI;

    [Tooltip("提示文字內容")]
    public string hintText = "按 F 互動";

    [Header("進階設定")]
    [Tooltip("對話結束後是否自動隱藏提示")]
    public bool hideHintAfterDialogue = true;

    [Tooltip("對話結束後是否停用此觸發器")]
    public bool disableAfterDialogue = false;

    // 私有變數
    private bool playerInRange = false;
    private bool hasTriggered = false;
    private float lastTriggerTime = 0f;
    private TextMeshProUGUI hintTextComponent;
    private bool isDialoguePlaying = false;

    void Start()
    {
        // 自動尋找對話系統
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            if (dialogueSystem == null)
            {
                Debug.LogError("找不到 DialogueSystem！請確保場景中有 DialogueManager。");
            }
        }

        // 設定提示 UI
        if (interactHintUI != null)
        {
            interactHintUI.SetActive(false);
            hintTextComponent = interactHintUI.GetComponent<TextMeshProUGUI>();
            if (hintTextComponent != null)
            {
                hintTextComponent.text = hintText;
            }
        }

        // 確保物件有 Collider 且設為 Trigger
        Collider col = GetComponent<Collider>();
        Collider2D col2D = GetComponent<Collider2D>();

        if (col == null && col2D == null)
        {
            Debug.LogWarning($"{gameObject.name} 沒有 Collider！請添加 Collider 並勾選 Is Trigger。");
        }
        else
        {
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"{gameObject.name} 的 Collider 沒有勾選 Is Trigger！");
            }
            if (col2D != null && !col2D.isTrigger)
            {
                Debug.LogWarning($"{gameObject.name} 的 Collider2D 沒有勾選 Is Trigger！");
            }
        }

        // 驗證對話資料
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} 沒有設定對話內容！");
        }
    }

    void Update()
    {
        // 按鍵模式：玩家在範圍內且按下互動鍵
        if (triggerMode == TriggerMode.PressKey && playerInRange && !isDialoguePlaying)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TryTriggerDialogue();
            }
        }
    }

    /// <summary>
    /// 嘗試觸發對話
    /// </summary>
    void TryTriggerDialogue()
    {
        // 檢查是否可以觸發
        if (hasTriggered && !canRepeat)
        {
            Debug.Log($"{gameObject.name}: 此對話只能觸發一次。");
            return;
        }

        // 檢查冷卻時間
        if (canRepeat && Time.time - lastTriggerTime < repeatCooldown)
        {
            Debug.Log($"{gameObject.name}: 對話冷卻中...");
            return;
        }

        // 檢查對話資料
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 沒有對話資料可播放！");
            return;
        }

        // 觸發對話
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(dialogues);
            hasTriggered = true;
            lastTriggerTime = Time.time;
            isDialoguePlaying = true;

            // 隱藏提示
            HideHint();

            // 註冊對話結束回調
            dialogueSystem.OnDialogueComplete = OnDialogueFinished;

            Debug.Log($"對話已觸發：{gameObject.name}");
        }
    }

    /// <summary>
    /// 對話結束時的回調
    /// </summary>
    void OnDialogueFinished()
    {
        isDialoguePlaying = false;

        // 如果設定為對話結束後停用
        if (disableAfterDialogue)
        {
            this.enabled = false;
            Debug.Log($"{gameObject.name}: 觸發器已停用");
        }

        // 如果玩家還在範圍內且允許重複，重新顯示提示
        if (playerInRange && canRepeat && !hideHintAfterDialogue)
        {
            ShowHint();
        }
    }

    /// <summary>
    /// 顯示互動提示
    /// </summary>
    void ShowHint()
    {
        if (interactHintUI != null && triggerMode == TriggerMode.PressKey)
        {
            // 檢查是否還能觸發
            bool canShow = !hasTriggered || canRepeat;
            if (canShow && !isDialoguePlaying)
            {
                interactHintUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 隱藏互動提示
    /// </summary>
    void HideHint()
    {
        if (interactHintUI != null)
        {
            interactHintUI.SetActive(false);
        }
    }

    /// <summary>
    /// 重置觸發狀態（供外部呼叫）
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        isDialoguePlaying = false;
        this.enabled = true;
        Debug.Log($"觸發器已重置：{gameObject.name}");
    }

    /// <summary>
    /// 手動觸發對話（供外部呼叫）
    /// </summary>
    public void ManualTrigger()
    {
        TryTriggerDialogue();
    }

    // ==================== 2D 碰撞檢測 ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log($"玩家進入觸發區域：{gameObject.name}");

            if (triggerMode == TriggerMode.AutoTrigger)
            {
                TryTriggerDialogue();
            }
            else
            {
                ShowHint();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log($"玩家離開觸發區域：{gameObject.name}");
            HideHint();
        }
    }

    // ==================== 3D 碰撞檢測 ====================

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log($"玩家進入觸發區域：{gameObject.name}");

            if (triggerMode == TriggerMode.AutoTrigger)
            {
                TryTriggerDialogue();
            }
            else
            {
                ShowHint();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log($"玩家離開觸發區域：{gameObject.name}");
            HideHint();
        }
    }

    // ==================== 編輯器視覺化 ====================

    void OnDrawGizmos()
    {
        // 在編輯器中顯示觸發範圍
        Gizmos.color = triggerMode == TriggerMode.AutoTrigger ? Color.green : Color.yellow;

        Collider col = GetComponent<Collider>();
        Collider2D col2D = GetComponent<Collider2D>();

        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else if (col2D != null)
        {
            Gizmos.DrawWireCube(transform.position, col2D.bounds.size);
        }
    }
}