using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("Canvas 設定")]
    public Canvas dialogueCanvas;

    [Header("UI 元件")]
    public GameObject dialogueBox;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueTextUI;
    public Image characterImageUI;
    public Image nameTagBackground;
    public GameObject continueIndicator;

    [Header("動畫設定")]
    public float fadeSpeed = 2f;
    public float characterSlideSpeed = 0.5f;
    public float dialogueBoxSlideSpeed = 0.3f;  // 對話框移動速度
    public float textSpeed = 0.05f;

    [Header("控制設定")]
    public KeyCode nextDialogueKey = KeyCode.E;

    [Header("角色位置 - 左邊")]
    public Vector2 leftEnterPosition = new Vector2(-800f, 0f);
    public Vector2 leftCenterPosition = new Vector2(-400f, 0f);
    public Vector2 leftExitPosition = new Vector2(-1200f, 0f);

    [Header("角色位置 - 右邊")]
    public Vector2 rightEnterPosition = new Vector2(800f, 0f);
    public Vector2 rightCenterPosition = new Vector2(400f, 0f);
    public Vector2 rightExitPosition = new Vector2(1200f, 0f);

    [Header("對話框位置")]
    public Vector2 leftDialogueBoxPosition = new Vector2(-200f, -300f);   // 左邊對話框位置
    public Vector2 rightDialogueBoxPosition = new Vector2(200f, -300f);   // 右邊對話框位置
    public Vector2 centerDialogueBoxPosition = new Vector2(0f, -300f);    // 中間對話框位置（預設）

    [Header("對話框移動設定")]
    public bool enableDialogueBoxMovement = true;  // 是否啟用對話框移動

    [Header("除錯模式")]
    public bool debugMode = true;

    // 私有變數
    private DialogueData[] currentDialogues;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool isAnimating = false;
    private string currentText = "";
    private Coroutine typingCoroutine;
    private CanvasGroup dialogueBoxCanvasGroup;
    private CanvasGroup characterCanvasGroup;
    private RectTransform characterRectTransform;
    private RectTransform dialogueBoxRectTransform;  // 對話框的 RectTransform
    private CharacterPosition currentCharacterPosition;
    private string currentCharacterName = "";
    private Sprite currentCharacterImage = null;
    private bool wasCanvasActive = false;

    public System.Action OnDialogueComplete;

    void Start()
    {
        DebugLog("=== DialogueSystem Start 初始化開始 ===");

        // 自動尋找 Canvas
        if (dialogueCanvas == null && dialogueBox != null)
        {
            dialogueCanvas = dialogueBox.GetComponentInParent<Canvas>();
            if (dialogueCanvas != null)
            {
                DebugLog("自動找到 Canvas");
            }
        }

        // 記錄 Canvas 原本的狀態
        if (dialogueCanvas != null)
        {
            wasCanvasActive = dialogueCanvas.gameObject.activeSelf;
            DebugLog("Canvas 原本狀態: " + (wasCanvasActive ? "啟用" : "關閉"));
        }

        // 暫時啟用必要的物件來初始化組件
        bool needRestore = false;
        if (dialogueCanvas != null && !dialogueCanvas.gameObject.activeSelf)
        {
            DebugLog("暫時啟用 Canvas 以初始化組件");
            dialogueCanvas.gameObject.SetActive(true);
            needRestore = true;
        }

        // 初始化 DialogueBox CanvasGroup 和 RectTransform
        if (dialogueBox != null)
        {
            if (dialogueBox.GetComponent<CanvasGroup>() == null)
                dialogueBoxCanvasGroup = dialogueBox.AddComponent<CanvasGroup>();
            else
                dialogueBoxCanvasGroup = dialogueBox.GetComponent<CanvasGroup>();

            // 取得對話框的 RectTransform
            dialogueBoxRectTransform = dialogueBox.GetComponent<RectTransform>();
            if (dialogueBoxRectTransform != null)
            {
                DebugLog("DialogueBox RectTransform 初始化成功");
            }

            DebugLog("DialogueBox CanvasGroup 初始化完成");
        }

        // 初始化 CharacterImage 組件
        if (characterImageUI != null)
        {
            DebugLog("CharacterImageUI 存在: " + characterImageUI.gameObject.name);

            if (characterImageUI.GetComponent<CanvasGroup>() == null)
            {
                characterCanvasGroup = characterImageUI.gameObject.AddComponent<CanvasGroup>();
                DebugLog("已添加 CanvasGroup 到 CharacterImage");
            }
            else
            {
                characterCanvasGroup = characterImageUI.GetComponent<CanvasGroup>();
                DebugLog("CharacterImage 已有 CanvasGroup");
            }

            characterRectTransform = characterImageUI.GetComponent<RectTransform>();
            DebugLog("CharacterImage RectTransform: " + (characterRectTransform != null));
        }
        else
        {
            Debug.LogError("CharacterImageUI 是 null! 請在 Inspector 中連接 Character Image UI!");
        }

        // 初始狀態設為隱藏
        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.alpha = 0f;
            DebugLog("DialogueBox Alpha 設為 0");
        }

        if (characterCanvasGroup != null)
        {
            characterCanvasGroup.alpha = 0f;
            DebugLog("CharacterImage Alpha 設為 0");
        }

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        // 恢復 Canvas 原本的狀態
        if (needRestore && dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(wasCanvasActive);
            DebugLog("Canvas 已恢復原本狀態");
        }

        DebugLog("=== 初始化完成 ===");
        DebugLog("最終檢查 - characterCanvasGroup: " + (characterCanvasGroup != null));
        DebugLog("最終檢查 - characterRectTransform: " + (characterRectTransform != null));
        DebugLog("最終檢查 - dialogueBoxRectTransform: " + (dialogueBoxRectTransform != null));
    }

    void Update()
    {
        if (Input.GetKeyDown(nextDialogueKey) && !isAnimating)
        {
            NextDialogue();
        }
    }

    public void StartDialogue(DialogueData[] dialogues)
    {
        DebugLog("=== StartDialogue 被呼叫 ===");

        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("沒有對話資料!");
            return;
        }

        DebugLog("對話數量: " + dialogues.Length);
        DebugLog("第一句角色: " + dialogues[0].characterName);
        DebugLog("第一句圖片: " + (dialogues[0].characterImage != null));

        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        currentCharacterName = "";
        currentCharacterImage = null;

        // 啟用 Canvas
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(true);
            DebugLog("Canvas 已啟用");
        }

        // 啟用 DialogueBox
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            DebugLog("DialogueBox 已啟用");
        }

        StartCoroutine(ShowDialogueBox());
    }

    IEnumerator ShowDialogueBox()
    {
        DebugLog("=== ShowDialogueBox 開始 ===");
        isAnimating = true;

        // 對話框淡入
        if (dialogueBoxCanvasGroup != null)
        {
            while (dialogueBoxCanvasGroup.alpha < 1f)
            {
                dialogueBoxCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            dialogueBoxCanvasGroup.alpha = 1f;
            DebugLog("DialogueBox 淡入完成");
        }

        // 顯示第一個對話
        yield return StartCoroutine(ShowCharacterAndDialogue(currentDialogues[currentDialogueIndex]));

        isAnimating = false;
    }

    IEnumerator HideDialogueBox()
    {
        DebugLog("=== HideDialogueBox 開始 ===");
        isAnimating = true;

        yield return StartCoroutine(CharacterExit(currentCharacterPosition));

        if (dialogueBoxCanvasGroup != null)
        {
            while (dialogueBoxCanvasGroup.alpha > 0f)
            {
                dialogueBoxCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            dialogueBoxCanvasGroup.alpha = 0f;
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (dialogueCanvas != null && !wasCanvasActive)
            dialogueCanvas.gameObject.SetActive(false);

        isAnimating = false;

        if (OnDialogueComplete != null)
            OnDialogueComplete();
    }

    public void NextDialogue()
    {
        // 檢查對話是否已經開始
        if (currentDialogues == null || currentDialogues.Length == 0)
        {
            DebugLog("錯誤：對話尚未開始或沒有對話資料");
            return;
        }

        if (isTyping)
        {
            StopTyping();
            return;
        }

        if (currentDialogueIndex >= currentDialogues.Length - 1)
        {
            StartCoroutine(HideDialogueBox());
            return;
        }

        currentDialogueIndex++;
        DialogueData nextDialogue = currentDialogues[currentDialogueIndex];

        bool needSwitch = IsCharacterChanged(nextDialogue);
        DebugLog("下一句對話 - 需要切換角色: " + needSwitch);

        if (needSwitch)
        {
            StartCoroutine(SwitchCharacter(nextDialogue));
        }
        else
        {
            UpdateDialogueUI(nextDialogue);
            StartCoroutine(TypeText(nextDialogue.dialogueText));
        }
    }

    bool IsCharacterChanged(DialogueData dialogue)
    {
        bool nameChanged = currentCharacterName != dialogue.characterName;
        bool imageChanged = currentCharacterImage != dialogue.characterImage;
        bool positionChanged = currentCharacterPosition != dialogue.characterPosition;

        return nameChanged || imageChanged || positionChanged;
    }

    IEnumerator ShowCharacterAndDialogue(DialogueData dialogue)
    {
        DebugLog("=== ShowCharacterAndDialogue 開始 ===");

        UpdateDialogueUI(dialogue);

        currentCharacterName = dialogue.characterName;
        currentCharacterImage = dialogue.characterImage;
        currentCharacterPosition = dialogue.characterPosition;

        DebugLog("角色名稱: " + currentCharacterName);
        DebugLog("角色圖片: " + (currentCharacterImage != null));
        DebugLog("角色位置: " + currentCharacterPosition);

        // 移動對話框到對應位置
        if (enableDialogueBoxMovement)
        {
            yield return StartCoroutine(MoveDialogueBox(dialogue.characterPosition));
        }

        // 角色進入動畫
        yield return StartCoroutine(CharacterEnter(dialogue.characterPosition));

        // 開始打字效果
        yield return StartCoroutine(TypeText(dialogue.dialogueText));
    }

    void UpdateDialogueUI(DialogueData dialogue)
    {
        if (characterImageUI != null)
        {
            characterImageUI.sprite = dialogue.characterImage;
            if (dialogue.characterImage != null)
            {
                DebugLog("設定角色圖片: " + dialogue.characterImage.name);
            }
        }
        if (characterNameText != null)
            characterNameText.text = dialogue.characterName;
        if (nameTagBackground != null)
            nameTagBackground.color = dialogue.nameTagColor;
    }

    /// <summary>
    /// 移動對話框到指定位置
    /// </summary>
    IEnumerator MoveDialogueBox(CharacterPosition position)
    {
        if (dialogueBoxRectTransform == null)
        {
            DebugLog("DialogueBox RectTransform 不存在，跳過移動");
            yield break;
        }

        Vector2 targetPosition;

        // 根據角色位置決定對話框位置
        if (position == CharacterPosition.Left)
        {
            targetPosition = leftDialogueBoxPosition;
            DebugLog("移動對話框到左邊: " + targetPosition);
        }
        else
        {
            targetPosition = rightDialogueBoxPosition;
            DebugLog("移動對話框到右邊: " + targetPosition);
        }

        Vector2 startPosition = dialogueBoxRectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < dialogueBoxSlideSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dialogueBoxSlideSpeed;

            dialogueBoxRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        dialogueBoxRectTransform.anchoredPosition = targetPosition;
        DebugLog("對話框移動完成: " + targetPosition);
    }

    IEnumerator CharacterEnter(CharacterPosition position)
    {
        DebugLog("=== CharacterEnter 開始 ===");

        if (characterRectTransform == null || characterCanvasGroup == null)
        {
            Debug.LogError("CharacterImage 組件缺失!");
            yield break;
        }

        Vector2 enterPos, centerPos;

        if (position == CharacterPosition.Left)
        {
            enterPos = leftEnterPosition;
            centerPos = leftCenterPosition;
        }
        else
        {
            enterPos = rightEnterPosition;
            centerPos = rightCenterPosition;
        }

        DebugLog("進入位置: " + enterPos);
        DebugLog("中心位置: " + centerPos);

        characterRectTransform.anchoredPosition = enterPos;
        characterCanvasGroup.alpha = 0f;

        DebugLog("開始移動動畫...");

        float elapsedTime = 0f;

        while (elapsedTime < characterSlideSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / characterSlideSpeed;

            characterRectTransform.anchoredPosition = Vector2.Lerp(enterPos, centerPos, t);
            characterCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        characterRectTransform.anchoredPosition = centerPos;
        characterCanvasGroup.alpha = 1f;

        DebugLog("角色進入完成! 最終 Alpha: " + characterCanvasGroup.alpha);
    }

    IEnumerator CharacterExit(CharacterPosition position)
    {
        DebugLog("=== CharacterExit 開始 ===");

        if (characterRectTransform == null || characterCanvasGroup == null)
            yield break;

        Vector2 exitPos;

        if (position == CharacterPosition.Left)
        {
            exitPos = leftExitPosition;
        }
        else
        {
            exitPos = rightExitPosition;
        }

        DebugLog("退出位置: " + exitPos);

        float elapsedTime = 0f;
        Vector2 startPosition = characterRectTransform.anchoredPosition;

        while (elapsedTime < characterSlideSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / characterSlideSpeed;

            characterRectTransform.anchoredPosition = Vector2.Lerp(startPosition, exitPos, t);
            characterCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        characterRectTransform.anchoredPosition = exitPos;
        characterCanvasGroup.alpha = 0f;

        DebugLog("角色退出完成");
    }

    IEnumerator SwitchCharacter(DialogueData newDialogue)
    {
        DebugLog("=== SwitchCharacter 開始 ===");
        isAnimating = true;

        yield return StartCoroutine(CharacterExit(currentCharacterPosition));

        UpdateDialogueUI(newDialogue);

        currentCharacterName = newDialogue.characterName;
        currentCharacterImage = newDialogue.characterImage;
        currentCharacterPosition = newDialogue.characterPosition;

        // 移動對話框到新位置
        if (enableDialogueBoxMovement)
        {
            yield return StartCoroutine(MoveDialogueBox(newDialogue.characterPosition));
        }

        yield return StartCoroutine(CharacterEnter(newDialogue.characterPosition));
        yield return StartCoroutine(TypeText(newDialogue.dialogueText));

        isAnimating = false;
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        if (dialogueTextUI != null)
            dialogueTextUI.text = "";
        currentText = text;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        foreach (char c in text)
        {
            if (dialogueTextUI != null)
                dialogueTextUI.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    void StopTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueTextUI != null)
            dialogueTextUI.text = currentText;
        isTyping = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    public void SetNextDialogueKey(KeyCode key)
    {
        nextDialogueKey = key;
    }

    public bool IsDialogueActive()
    {
        return dialogueBox != null && dialogueBox.activeSelf;
    }

    void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log("[DialogueSystem] " + message);
        }
    }
}