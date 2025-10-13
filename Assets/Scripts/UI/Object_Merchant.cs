using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Object_Merchant : MonoBehaviour
{
    [Header("Quest & Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;   // 版本A：含 speaker / textLine[]

    private bool isTalking = false;

    /// <summary>
    /// 由你的互動系統（或測試腳本）呼叫
    /// </summary>
    public void Interact()
    {
        // 防呆
        if (isTalking) return;
        if (UI_Manager.Instance == null)
        {
            Debug.LogError("[Object_Merchant] UI_Manager.Instance 為空，請確認場景有 UI_Manager。");
            return;
        }
        if (firstDialogueLine == null)
        {
            Debug.LogWarning("[Object_Merchant] firstDialogueLine 未指定，無法開啟對話。");
            return;
        }

        isTalking = true;

        // 開啟對話（UI_Dialogue 會讀取 line.speaker / line.textLine[]）
        UI_Manager.Instance.OpenDialogue(firstDialogueLine, OnDialogueClosed);
    }

    /// <summary>
    /// 對話關閉後的收尾（解除鎖定；之後要接任務/商店在這裡做）
    /// </summary>
    private void OnDialogueClosed()
    {
        isTalking = false;

        // 之後可在這裡串接其它 UI：
        // UI_Manager.Instance.OpenQuest(...);
        // UI_Manager.Instance.OpenMerchant(...);
    }
}