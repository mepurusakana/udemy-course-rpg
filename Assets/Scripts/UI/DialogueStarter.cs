using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 掛在 NPC 或其子物件（需帶 Trigger Collider）
/// 功能：
/// - 角色進入觸發範圍可「自動開始」或「按鍵開始」對話
/// - 對話進行中按同一顆鍵（預設 E）前進下一句（由 UI_Dialogue 處理）
/// - 支援「只觸發一次」或「可重複觸發」，可在 Inspector 個別調整
/// - 可選顯示「按 E 互動」提示 UI
/// </summary>
public class DialogueStarter : MonoBehaviour
{
    [Header("資料")]
    [SerializeField] private DialogueLineSO dialogue;      // 指到 DialogueLineSO 資產（speaker + textLine[]）

    [Header("觸發方式")]
    [SerializeField] private bool autoStartOnEnter = false; // 勾選：進入範圍自動開始；未勾：需按鍵
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("觸發次數規則（可個別調整）")]
    [Tooltip("勾選後，本 NPC 只會觸發一次。")]
    [SerializeField] private bool oneShot = false;

    [Header("玩家識別")]
    [SerializeField] private string playerTag = "Player";   // 玩家 Tag

    [Header("互動提示（可選）")]
    [Tooltip("例如『按 E 互動』的小面板；留空則不顯示")]
    [SerializeField] private GameObject interactHint;

    [Header("偵錯")]
    [SerializeField] private bool enableLogs = true;

    // 內部狀態
    private bool playerInRange = false;
    private bool isTalking = false;
    private bool consumed = false; // oneShot 用

    // 快取的 Manager（避免 Instance 尚未建立）
    private UI_Manager manager;

    private void Awake()
    {
        // 嘗試尋找並快取 UI_Manager（包含未啟用物件）
        manager = FindObjectOfType<UI_Manager>(true);
        if (enableLogs)
            Debug.Log($"[DialogueStarter:{name}] Awake. autoStart={autoStartOnEnter}, oneShot={oneShot}, playerTag={playerTag}, manager={(manager ? "OK" : "NULL")}");

        if (manager == null)
            Debug.LogError($"[DialogueStarter:{name}] 找不到 UI_Manager！請確認場景中有一個啟用的 UI_Manager，且其 uiDialogue 欄位已指派。");
    }

    private void Update()
    {
        if (consumed || isTalking || !playerInRange || autoStartOnEnter) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (enableLogs) Debug.Log($"[DialogueStarter:{name}] InteractKey {interactKey} pressed → StartDialogue()");
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (consumed) { if (enableLogs) Debug.Log($"[DialogueStarter:{name}] consumed=true, skip."); return; }
        if (isTalking) { if (enableLogs) Debug.Log($"[DialogueStarter:{name}] already talking, skip."); return; }

        // 再保險找一次（若場景此刻才載入 Manager）
        if (manager == null) manager = FindObjectOfType<UI_Manager>(true);
        if (manager == null)
        {
            Debug.LogError($"[DialogueStarter:{name}] UI_Manager 為 NULL！無法開啟對話。");
            return;
        }
        if (dialogue == null)
        {
            Debug.LogError($"[DialogueStarter:{name}] dialogue 為 NULL！請在 Inspector 指到 DialogueLineSO 資產。");
            return;
        }

        isTalking = true;
        SetHintActive(false);

        if (enableLogs)
        {
            string spk = dialogue.speaker ? dialogue.speaker.speakerName : "(no speaker)";
            int cnt = dialogue.textLine != null ? dialogue.textLine.Length : 0;
            Debug.Log($"[DialogueStarter:{name}] StartDialogue() speaker={spk}, lines={cnt}");
        }

        manager.OpenDialogue(dialogue, OnDialogueClosed);
    }

    private void OnDialogueClosed()
    {
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] OnDialogueClosed()");
        isTalking = false;
        if (oneShot)
        {
            consumed = true;
            if (enableLogs) Debug.Log($"[DialogueStarter:{name}] oneShot consumed=true. Won't trigger again.");
        }

        // 非自動、未消耗、仍在範圍 → 顯示提示
        if (playerInRange && !consumed && !autoStartOnEnter)
            SetHintActive(true);
    }

    // ----------------- 3D 觸發 -----------------
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] OnTriggerEnter(3D) with {other.name}");
        HandleEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] OnTriggerExit(3D) with {other.name}");
        HandleExit();
    }

    // ----------------- 2D 觸發 -----------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] OnTriggerEnter2D with {other.name}");
        HandleEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] OnTriggerExit2D with {other.name}");
        HandleExit();
    }

    // ----------------- 共用處理 -----------------
    private void HandleEnter()
    {
        playerInRange = true;

        if (consumed) { SetHintActive(false); return; }

        if (autoStartOnEnter)
        {
            if (enableLogs) Debug.Log($"[DialogueStarter:{name}] autoStartOnEnter=true → StartDialogue()");
            StartDialogue();
        }
        else
        {
            SetHintActive(true);
        }
    }

    private void HandleExit()
    {
        playerInRange = false;
        SetHintActive(false);
    }

    private void SetHintActive(bool active)
    {
        if (interactHint) interactHint.SetActive(active);
        if (enableLogs) Debug.Log($"[DialogueStarter:{name}] Hint {(interactHint ? "set" : "skip(null)")} active={active}");
    }
}