using UnityEngine;
using System;
using TMPro;               // TextMeshProUGUI
using UnityEngine.UI;      // Image

public class UI_Dialogue : MonoBehaviour
{
    [Header("UI 元件")]
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("控制")]
    [SerializeField] private KeyCode advanceKey = KeyCode.E;
    [SerializeField] private bool enableLogs = true;

    [SerializeField] private Image panelImage;
    //對話面板底圖

    private DialogueLineSO currentLine;
    private int currentIndex = 0;
    private Action onClosed;

    public bool IsOpen { get; private set; }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(advanceKey))
        {
            Advance();
        }
    }

    // 打開並顯示第一句
    public void Open(DialogueLineSO line, Action onClosedCallback = null)
    {
        currentLine = line;
        currentIndex = 0;
        onClosed = onClosedCallback;
        var sp = currentLine ? currentLine.speaker : null;

        if (sp != null)
        {
            if (speakerPortrait) speakerPortrait.sprite = sp.speakerPortrait;

            if (speakerName)
            {
                speakerName.text = sp.speakerName;
                speakerName.color = sp.nameColor;          // 套名字顏色
            }

            if (panelImage)
            {
                panelImage.sprite = sp.panelSprite;        // 套面板底圖
                panelImage.color = sp.panelTint;          // 套面板色
                // 若使用 9-sliced，記得 Panel 的 Image Type 設為 Sliced
            }
        }
        else
        {
            if (speakerPortrait) speakerPortrait.sprite = null;
            if (speakerName) { speakerName.text = ""; speakerName.color = Color.white; }
        }

        Show();
        RenderLine();
    }

    // 下一句或關閉
    private void Advance()
    {
        currentIndex++;

        if (currentLine == null || currentLine.textLine == null)
        {
            Close();
            return;
        }

        if (currentIndex < currentLine.textLine.Length)
        {
            RenderLine();
        }
        else
        {
            Close();
        }
    }

    private void RenderLine()
    {
        if (currentLine == null) return;

        if (speakerPortrait && currentLine.speaker)
            speakerPortrait.sprite = currentLine.speaker.speakerPortrait;

        if (speakerName && currentLine.speaker)
            speakerName.text = currentLine.speaker.speakerName;

        if (dialogueText && currentLine.textLine.Length > currentIndex)
            dialogueText.text = currentLine.textLine[currentIndex];

        if (enableLogs)
            Debug.Log($"[UI_Dialogue] Render line {currentIndex + 1}/{currentLine.textLine.Length}: {dialogueText.text}");
    }

    public void Close()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        IsOpen = false;
        onClosed?.Invoke();

        if (enableLogs) Debug.Log("[UI_Dialogue] Close()");
    }

    private void Show()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }

        IsOpen = true;
        if (enableLogs) Debug.Log("[UI_Dialogue] Show()");
    }


}
