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

    // 防止 MoveDialogueBox 疊加
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

        if (dialogueBoxCanvasGroup != null) dialogueBoxCanvasGroup.alpha = 0f;
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

        if (dialogueBoxRectTransform != null)
        {
            lastDialogueBoxTargetPos = dialogueBoxRectTransform.anchoredPosition;
            hasLastDialogueBoxTargetPos = true;
        }

        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        currentCharacterName = "";
        currentCharacterImage = null;

        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(true);
        if (dialogueBox != null) dialogueBox.SetActive(true);

        StartCoroutine(ShowDialogueBox());
    }

    private IEnumerator ShowDialogueBox()
    {
        isAnimating = true;

        if (dialogueBoxCanvasGroup != null)
        {
            while (dialogueBoxCanvasGroup.alpha < 1f)
            {
                dialogueBoxCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            dialogueBoxCanvasGroup.alpha = 1f;
        }

        yield return StartCoroutine(ShowCharacterAndDialogue(currentDialogues[currentDialogueIndex]));
        isAnimating = false;
    }

    private IEnumerator HideDialogueBox()
    {
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

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (dialogueCanvas != null && !wasCanvasActive) dialogueCanvas.gameObject.SetActive(false);

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
            UpdateDialogueUI(nextDialogue);

            if (enableDialogueBoxMovement)
            {
                // 防止疊加
                if (moveDialogueBoxCoroutine != null)
                    StopCoroutine(moveDialogueBoxCoroutine);

                moveDialogueBoxCoroutine = StartCoroutine(MoveDialogueBox(nextDialogue));
            }

            StartTyping(nextDialogue.dialogueText);
        }
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

    /// <summary>
    /// 每句獨立控制對話框位置（Y 不動，起點=上一句的終點）
    /// </summary>
    private IEnumerator MoveDialogueBox(DialogueData dialogue)
    {
        if (dialogueBoxRectTransform == null) yield break;

        if (dialogue.dialogueBoxPosition == DialogueBoxPosition.Keep)
            yield break;

        Vector2 rawTarget = GetTargetDialogueBoxPosition(dialogue);

        if (!hasLastDialogueBoxTargetPos)
        {
            lastDialogueBoxTargetPos = dialogueBoxRectTransform.anchoredPosition;
            hasLastDialogueBoxTargetPos = true;
        }

        Vector2 startPos = lastDialogueBoxTargetPos;
        Vector2 targetPos = new Vector2(rawTarget.x, startPos.y);

        dialogueBoxRectTransform.anchoredPosition = startPos;

        float elapsed = 0f;
        float dur = Mathf.Max(0.0001f, dialogueBoxSlideSpeed);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            dialogueBoxRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        dialogueBoxRectTransform.anchoredPosition = targetPos;
        lastDialogueBoxTargetPos = targetPos;
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
}
