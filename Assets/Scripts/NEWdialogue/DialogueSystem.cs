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
    public float dialogueBoxSlideSpeed = 0.3f;
    public float textSpeed = 0.05f;

    [Header("對話框移動時淡入淡出（只有移動時才用）")]
    public bool enableMoveFade = true;
    [Range(0f, 1f)]
    public float moveFadeMinAlpha = 0f;

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

    [Header("對話框位置（系統預設）")]
    public Vector2 leftDialogueBoxPosition = new Vector2(-200f, -300f);
    public Vector2 rightDialogueBoxPosition = new Vector2(200f, -300f);
    public Vector2 centerDialogueBoxPosition = new Vector2(0f, -300f);

    [Header("對話框移動設定")]
    public bool enableDialogueBoxMovement = true;

    [Header("除錯模式")]
    public bool debugMode = true;

    private DialogueData[] currentDialogues;
    private int currentDialogueIndex = 0;

    private bool isTyping = false;
    private bool isAnimating = false;
    private string currentText = "";
    private Coroutine typingCoroutine;

    private CanvasGroup dialogueBoxCanvasGroup;
    private CanvasGroup characterCanvasGroup;
    private RectTransform characterRectTransform;
    private RectTransform dialogueBoxRectTransform;

    private CharacterPosition currentCharacterPosition;
    private string currentCharacterName = "";
    private Sprite currentCharacterImage = null;
    private bool wasCanvasActive = false;

    public System.Action OnDialogueComplete;

    private Vector2 lastDialogueBoxTargetPos;
    private bool hasLastDialogueBoxTargetPos = false;

    private Coroutine moveDialogueBoxCoroutine;

    private void Start()
    {
        if (dialogueCanvas == null && dialogueBox != null)
            dialogueCanvas = dialogueBox.GetComponentInParent<Canvas>();

        if (dialogueCanvas != null)
            wasCanvasActive = dialogueCanvas.gameObject.activeSelf;

        bool needRestore = false;
        if (dialogueCanvas != null && !dialogueCanvas.gameObject.activeSelf)
        {
            dialogueCanvas.gameObject.SetActive(true);
            needRestore = true;
        }

        if (dialogueBox != null)
        {
            dialogueBoxCanvasGroup = dialogueBox.GetComponent<CanvasGroup>();
            if (dialogueBoxCanvasGroup == null)
                dialogueBoxCanvasGroup = dialogueBox.AddComponent<CanvasGroup>();

            dialogueBoxRectTransform = dialogueBox.GetComponent<RectTransform>();
        }

        if (characterImageUI != null)
        {
            characterCanvasGroup = characterImageUI.GetComponent<CanvasGroup>();
            if (characterCanvasGroup == null)
                characterCanvasGroup = characterImageUI.gameObject.AddComponent<CanvasGroup>();

            characterRectTransform = characterImageUI.GetComponent<RectTransform>();
        }

        //if (dialogueBoxCanvasGroup != null) dialogueBoxCanvasGroup.alpha = 0f;
        if (characterCanvasGroup != null) characterCanvasGroup.alpha = 0f;

        if (continueIndicator != null) continueIndicator.SetActive(false);

        if (needRestore && dialogueCanvas != null)
            dialogueCanvas.gameObject.SetActive(wasCanvasActive);
    }

    private void Update()
    {
        if (Input.GetKeyDown(nextDialogueKey) && !isAnimating)
            NextDialogue();
    }

    public void StartDialogue(DialogueData[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("沒有對話資料!");
            return;
        }

        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        currentCharacterName = "";
        currentCharacterImage = null;

        // 先記住目前位置（即使物件還沒啟用也可以讀到 RectTransform）
        if (dialogueBoxRectTransform != null)
        {
            lastDialogueBoxTargetPos = dialogueBoxRectTransform.anchoredPosition;
            hasLastDialogueBoxTargetPos = true;
        }

        // 關鍵：先把 alpha 設成 0（避免 SetActive 那一幀閃一下）
        if (dialogueBoxCanvasGroup != null)
            dialogueBoxCanvasGroup.alpha = 0f;

        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(true);
        if (dialogueBox != null) dialogueBox.SetActive(true);

        StartCoroutine(ShowDialogueBox());
    }

    private IEnumerator ShowDialogueBox()
    {
        isAnimating = true;

        // 開場：淡入，且若第一句需要換位置，移動會和淡入同步進行
        yield return StartCoroutine(OpenDialogueBoxSequence(currentDialogues[currentDialogueIndex]));

        // 進入第一句（MoveDialogueBox 會因為已經到位而不再移動/不干擾 alpha）
        yield return StartCoroutine(ShowCharacterAndDialogue(currentDialogues[currentDialogueIndex]));

        isAnimating = false;
    }

    private IEnumerator HideDialogueBox()
    {
        isAnimating = true;

        yield return StartCoroutine(CharacterExit(currentCharacterPosition));

        if (dialogueBoxCanvasGroup != null)
            yield return StartCoroutine(FadeToBySpeed(dialogueBoxCanvasGroup, 0f, fadeSpeed));

        if (dialogueBox != null) dialogueBox.SetActive(false);
        //if (dialogueCanvas != null && !wasCanvasActive) dialogueCanvas.gameObject.SetActive(false);

        isAnimating = false;
        OnDialogueComplete?.Invoke();
    }

    public void NextDialogue()
    {
        if (currentDialogues == null || currentDialogues.Length == 0)
            return;

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

        if (needSwitch)
        {
            StartCoroutine(SwitchCharacter(nextDialogue));
        }
        else
        {
            StartCoroutine(PlayLineSameCharacter(nextDialogue));
        }
    }

    private IEnumerator PlayLineSameCharacter(DialogueData nextDialogue)
    {
        isAnimating = true;

        UpdateDialogueUI(nextDialogue);

        if (enableDialogueBoxMovement)
        {
            if (moveDialogueBoxCoroutine != null)
                StopCoroutine(moveDialogueBoxCoroutine);

            moveDialogueBoxCoroutine = StartCoroutine(MoveDialogueBox(nextDialogue));
            yield return moveDialogueBoxCoroutine;
        }

        isAnimating = false;
        StartTyping(nextDialogue.dialogueText);
    }

    private bool IsCharacterChanged(DialogueData dialogue)
    {
        bool nameChanged = currentCharacterName != dialogue.characterName;
        bool imageChanged = currentCharacterImage != dialogue.characterImage;
        bool positionChanged = currentCharacterPosition != dialogue.characterPosition;
        return nameChanged || imageChanged || positionChanged;
    }

    private IEnumerator ShowCharacterAndDialogue(DialogueData dialogue)
    {
        UpdateDialogueUI(dialogue);

        currentCharacterName = dialogue.characterName;
        currentCharacterImage = dialogue.characterImage;
        currentCharacterPosition = dialogue.characterPosition;

        if (enableDialogueBoxMovement)
            yield return StartCoroutine(MoveDialogueBox(dialogue));

        yield return StartCoroutine(CharacterEnter(dialogue.characterPosition));

        StartTyping(dialogue.dialogueText);
        while (isTyping) yield return null;
    }

    private void UpdateDialogueUI(DialogueData dialogue)
    {
        if (characterImageUI != null) characterImageUI.sprite = dialogue.characterImage;
        if (characterNameText != null) characterNameText.text = dialogue.characterName;
        if (nameTagBackground != null) nameTagBackground.color = dialogue.nameTagColor;
    }

    private IEnumerator MoveDialogueBox(DialogueData dialogue)
    {
        if (dialogueBoxRectTransform == null) yield break;

        if (!hasLastDialogueBoxTargetPos)
        {
            lastDialogueBoxTargetPos = dialogueBoxRectTransform.anchoredPosition;
            hasLastDialogueBoxTargetPos = true;
        }

        if (dialogue.dialogueBoxPosition == DialogueBoxPosition.Keep)
            yield break;

        Vector2 rawTarget = GetTargetDialogueBoxPosition(dialogue);

        Vector2 startPos = lastDialogueBoxTargetPos;
        Vector2 targetPos = new Vector2(rawTarget.x, startPos.y);

        bool willMove = Mathf.Abs(targetPos.x - startPos.x) > 0.01f;

        if (!willMove)
        {
            dialogueBoxRectTransform.anchoredPosition = targetPos;
            lastDialogueBoxTargetPos = targetPos;
            if (dialogueBoxCanvasGroup != null) dialogueBoxCanvasGroup.alpha = 1f;
            yield break;
        }

        float elapsed = 0f;
        float dur = Mathf.Max(0.0001f, dialogueBoxSlideSpeed);

        dialogueBoxRectTransform.anchoredPosition = startPos;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);

            dialogueBoxRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            if (enableMoveFade && dialogueBoxCanvasGroup != null)
            {
                float a;
                if (t < 0.5f)
                    a = Mathf.Lerp(1f, moveFadeMinAlpha, t * 2f);
                else
                    a = Mathf.Lerp(moveFadeMinAlpha, 1f, (t - 0.5f) * 2f);

                dialogueBoxCanvasGroup.alpha = a;
            }

            yield return null;
        }

        dialogueBoxRectTransform.anchoredPosition = targetPos;
        lastDialogueBoxTargetPos = targetPos;

        if (dialogueBoxCanvasGroup != null)
            dialogueBoxCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeToBySpeed(CanvasGroup cg, float targetAlpha, float speed)
    {
        if (cg == null) yield break;

        float spd = Mathf.Max(0.0001f, speed);
        while (!Mathf.Approximately(cg.alpha, targetAlpha))
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * spd);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    private Vector2 GetTargetDialogueBoxPosition(DialogueData dialogue)
    {
        switch (dialogue.dialogueBoxPosition)
        {
            case DialogueBoxPosition.Left:
                return leftDialogueBoxPosition;

            case DialogueBoxPosition.Right:
                return rightDialogueBoxPosition;

            case DialogueBoxPosition.FollowCharacter:
            default:
                return (dialogue.characterPosition == CharacterPosition.Left)
                    ? leftDialogueBoxPosition
                    : rightDialogueBoxPosition;
        }
    }

    private IEnumerator CharacterEnter(CharacterPosition position)
    {
        if (characterRectTransform == null || characterCanvasGroup == null) yield break;

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

        characterRectTransform.anchoredPosition = enterPos;
        characterCanvasGroup.alpha = 0f;

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
    }

    private IEnumerator CharacterExit(CharacterPosition position)
    {
        if (characterRectTransform == null || characterCanvasGroup == null) yield break;

        Vector2 exitPos = (position == CharacterPosition.Left) ? leftExitPosition : rightExitPosition;

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
    }

    private IEnumerator SwitchCharacter(DialogueData newDialogue)
    {
        isAnimating = true;

        yield return StartCoroutine(CharacterExit(currentCharacterPosition));

        UpdateDialogueUI(newDialogue);

        currentCharacterName = newDialogue.characterName;
        currentCharacterImage = newDialogue.characterImage;
        currentCharacterPosition = newDialogue.characterPosition;

        if (enableDialogueBoxMovement)
            yield return StartCoroutine(MoveDialogueBox(newDialogue));

        yield return StartCoroutine(CharacterEnter(newDialogue.characterPosition));

        StartTyping(newDialogue.dialogueText);
        while (isTyping) yield return null;

        isAnimating = false;
    }

    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentText = text;

        if (dialogueTextUI != null)
            dialogueTextUI.text = "";

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        foreach (char c in text)
        {
            if (dialogueTextUI != null)
                dialogueTextUI.text += c;

            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueTextUI != null)
            dialogueTextUI.text = currentText;

        isTyping = false;
        typingCoroutine = null;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    private void DebugLog(string msg)
    {
        if (debugMode) Debug.Log("[DialogueSystem] " + msg);
    }

    private IEnumerator OpenDialogueBoxSequence(DialogueData firstDialogue)
    {
        if (dialogueBoxCanvasGroup == null)
            yield break;

        // 再保險一次，確保開場從 0 開始
        dialogueBoxCanvasGroup.alpha = 0f;

        // 如果沒有 RectTransform，就只做淡入
        if (dialogueBoxRectTransform == null)
        {
            yield return StartCoroutine(FadeToBySpeed(dialogueBoxCanvasGroup, 1f, fadeSpeed));
            yield break;
        }

        // 起點：目前位置
        Vector2 startPos = dialogueBoxRectTransform.anchoredPosition;

        // 先把 last target 設好，避免第一句 MoveDialogueBox 用到舊資料
        lastDialogueBoxTargetPos = startPos;
        hasLastDialogueBoxTargetPos = true;

        // 如果沒開啟移動、或 Keep，就只淡入（位置不動）
        if (!enableDialogueBoxMovement || firstDialogue.dialogueBoxPosition == DialogueBoxPosition.Keep)
        {
            yield return StartCoroutine(FadeToBySpeed(dialogueBoxCanvasGroup, 1f, fadeSpeed));
            yield break;
        }

        // 計算第一句目標位置（只動 X，Y 固定）
        Vector2 rawTarget = GetTargetDialogueBoxPosition(firstDialogue);
        Vector2 targetPos = new Vector2(rawTarget.x, startPos.y);

        bool willMove = Mathf.Abs(targetPos.x - startPos.x) > 0.01f;

        // 第一句不需要移動：只淡入
        if (!willMove)
        {
            yield return StartCoroutine(FadeToBySpeed(dialogueBoxCanvasGroup, 1f, fadeSpeed));
            lastDialogueBoxTargetPos = startPos;
            yield break;
        }

        // 第一句需要移動：移動期間同步淡入（0 -> 1）
        float elapsed = 0f;
        float dur = Mathf.Max(0.0001f, dialogueBoxSlideSpeed);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);

            dialogueBoxRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            dialogueBoxCanvasGroup.alpha = t;

            yield return null;
        }

        dialogueBoxRectTransform.anchoredPosition = targetPos;
        dialogueBoxCanvasGroup.alpha = 1f;

        // 更新 last target，讓後續 MoveDialogueBox 起點正確
        lastDialogueBoxTargetPos = targetPos;
    }
}
