using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("對話內容")]
    public DialogueData[] dialogues;

    [Header("互動提示（可選）")]
    public GameObject interactHint;

    private DialogueSystem dialogueSystem;
    private bool playerInRange = false;
    private bool isDialogueActive = false;

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
    }

    private void OnDestroy()
    {
        if (dialogueSystem != null)
            dialogueSystem.OnDialogueComplete -= OnDialogueEnd;
    }

    private void Update()
    {
        if (!playerInRange || isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
            TriggerDialogue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (interactHint != null) interactHint.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (interactHint != null) interactHint.SetActive(false);
    }

    public void TriggerDialogue()
    {
        if (dialogues == null || dialogues.Length == 0) return;

        isDialogueActive = true;
        if (interactHint != null) interactHint.SetActive(false);

        dialogueSystem.StartDialogue(dialogues);
    }

    private void OnDialogueEnd()
    {
        isDialogueActive = false;
        if (playerInRange && interactHint != null) interactHint.SetActive(true);
    }
}
