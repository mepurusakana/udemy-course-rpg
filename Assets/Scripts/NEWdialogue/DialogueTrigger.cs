using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        Manual, // 手動按鍵觸發
        Auto    // 進入範圍自動觸發
    }

    [Header("對話內容")]
    public DialogueData[] dialogues;
    public AudioManager audioManager;
    public Player player;

    [Header("觸發方式")]
    public TriggerMode triggerMode = TriggerMode.Manual;

    [Header("手動觸發設定（Manual 模式才會用到）")]
    public KeyCode interactKey = KeyCode.E;

    [Header("自動觸發設定（Auto 模式才會用到）")]
    public bool triggerOnce = true;          // 進入一次後不再重複觸發
    public bool autoTriggerOnEnter = true;   // 進入就觸發（一般都開）

    [Header("互動提示（可選）")]
    public GameObject interactHint;

    private DialogueSystem dialogueSystem;
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool hasTriggered = false;

    //private bool hasTriggered = false;
    private bool isThisDialogueActive = false;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            Debug.LogError("找不到 DialogueSystem！");
            return;
        }

        dialogueSystem.OnDialogueComplete += OnDialogueEnd;

        if (interactHint != null)
            interactHint.SetActive(false);

        TryFindPlayerAndAudio();
    }

    private void OnDestroy()
    {
        if (dialogueSystem != null)
            dialogueSystem.OnDialogueComplete -= OnDialogueEnd;
    }

    private void Update()
    {
        if (triggerMode != TriggerMode.Manual) return;
        if (!playerInRange || isDialogueActive) return;

        if (Input.GetKeyDown(interactKey))
            TriggerDialogue();
    }

    private void TryFindPlayerAndAudio()
    {
        // 首先從 UI 單例取得
        if (PlayerManager.instance != null && Player.instance != null)
        {
            player = Player.instance;
            //if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已從 UI.instance 綁定 UI_Skill", this);
            //return;
        }

        //  若 UI.instance 還沒初始化，用 FindObjectOfType (含 inactive)
        //var foundSkillUI = FindObjectOfType<TwoStateButtonGroup>(true);
        //if (foundSkillUI != null)
        //{
        //    skillsUIRootFallback = foundSkillUI.gameObject;
        //    if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已透過 FindObjectOfType 綁定 UI_Skill", this);
        //}

        if (AudioManager.instance != null)
        {
            audioManager = AudioManager.instance;
            //if (showDebugLogs) Debug.Log("[UI_SwitchToOpenSkills] 已從 UI.instance 綁定 UI_Skill", this);
            //return;
        }

        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;



        if (Player.instance != null)
        {
            //  完全鎖死
            player.LockCompletely();

            // 強制回 Idle（避免 Attack / Air State）
            player.stateMachine.ChangeState(player.idleState);
        }

        //Time.timeScale = 0f;

        playerInRange = true;

        // Manual：顯示提示
        if (triggerMode == TriggerMode.Manual)
        {
            if (!isDialogueActive && interactHint != null)
                interactHint.SetActive(true);
        }
        // Auto：進入就觸發（可一次性）
        else
        {
            if (interactHint != null) interactHint.SetActive(false);

            if (!autoTriggerOnEnter) return;
            if (isDialogueActive) return;
            if (triggerOnce && hasTriggered) return;

            TriggerDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    public void TriggerDialogue()
    {
        if (dialogues == null || dialogues.Length == 0) return;
        if (dialogueSystem == null) return;

        // 避免重複觸發
        if (isDialogueActive) return;

        isDialogueActive = true;
        isThisDialogueActive = true;
        hasTriggered = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        dialogueSystem.StartDialogue(dialogues);
    }

    private void OnDialogueEnd()
    {
        if(!isThisDialogueActive) return;

        isDialogueActive = false;
        isThisDialogueActive = false;

        Player.instance.GetOffBusy();
        Player.instance.UnlockCompletely();

        //Time.timeScale = 1f;

        // Manual：對話結束後，如果玩家仍在範圍內就把提示再打開
        if (triggerMode == TriggerMode.Manual && playerInRange && interactHint != null)
            interactHint.SetActive(true);

        if (triggerOnce && gameObject.activeSelf)
        {
            // 先關掉互動提示，避免 UI 殘留
            if (interactHint != null)
                interactHint.SetActive(false);

            // 只關閉當前物件
            this.gameObject.SetActive(false);
        }

    }
}
