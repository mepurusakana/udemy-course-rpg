using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossEndingFlowController : MonoBehaviour
{
    [Header("References")]
    public DialogueFlowController dialogueFlow;
    public DialogueTrigger dialogueTrigger;
    public AudioManager audioManager;
    public UI_FadeScreen fadeScreen;

    [SerializeField] private float fadeOutDuration = 2f;

    [Header("BGM Index")]
    public int endingBgmIndex = 2;   // 結局音樂
    public float delayBeforeDialogue = 0.5f;

    private void Awake()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnThisDialogueStarted += OnFinalDialogueStarted;
            dialogueTrigger.OnThisDialogueFinished += OnFinalDialogueFinished;
        }
    }

    private void OnDestroy()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnThisDialogueStarted -= OnFinalDialogueStarted;
            dialogueTrigger.OnThisDialogueFinished -= OnFinalDialogueFinished;
        }
    }

    private void Start()
    {
        fadeScreen = FindObjectOfType<UI_FadeScreen>();

        if (fadeScreen == null)
            Debug.LogError("找不到 UI_FadeScreen！請確保場景中有淡入淡出UI物件");

        // 監聽 DialogueFlow 的完成事件
        dialogueFlow.GetComponent<DialogueFlowController>();
    }

    private void OnEnable()
    {
        dialogueFlow.GetComponent<DialogueFlowController>()
            .GetComponent<DialogueFlowController>();
    }

    private void OnFinalDialogueStarted()
    {
        StartCoroutine(PlayEndingBgmRoutine());
    }

    private IEnumerator PlayEndingBgmRoutine()
    {
        yield return new WaitForSeconds(delayBeforeDialogue);

        if (audioManager != null)
            audioManager.PlayBGM(endingBgmIndex);
    }

    public void OnFinalDialogueFinished()
    {
        StartCoroutine(BackToMainMenuRoutine());
    }

    private IEnumerator BackToMainMenuRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        //  1. 刪存檔（真正乾淨）
        if (SaveManager.instance != null)
        {
            SaveManager.instance.DeleteSaveData();
        }

        //  2. 停音樂
        if (audioManager != null)
            audioManager.StopAllBGM();

        //  3. Fade
        if (fadeScreen != null)
            fadeScreen.FadeOut(fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration);


        Application.Quit();
        //  4. 進 BootScene（等於重開遊戲）
        //SceneManager.LoadScene("BootScene");
    }

    private void InitializeGame()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.CreateNewGame();
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopAllBGM();
        }
    }
}