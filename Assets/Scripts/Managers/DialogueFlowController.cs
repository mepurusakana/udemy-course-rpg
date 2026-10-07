using UnityEngine;
using System.Collections.Generic;

public class DialogueFlowController : MonoBehaviour, ISaveable
{
    [Header("需要被殺光的敵人")]
    public List<Enemy> requiredEnemies;

    [Header("對話觸發器")]
    public DialogueTrigger dialogueTrigger;

    [Header("對話完成後啟用的傳送門")]
    public SceneGate targetGate;

    private int aliveEnemyCount;
    private bool dialogueFinished = false;

    [SerializeField] private string flowId;

    private void Start()
    {
        // 初始：鎖死 Dialogue & Gate
        if (dialogueTrigger != null)
        {
            dialogueTrigger.gameObject.SetActive(false);
            dialogueTrigger.OnThisDialogueFinished += OnDialogueFinished;
        }

        if (targetGate != null)
            targetGate.gameObject.SetActive(false);

        aliveEnemyCount = requiredEnemies.Count;

        // 監聽敵人死亡
        foreach (var enemy in requiredEnemies)
        {
            if (enemy != null)
                enemy.OnEnemyDead += OnEnemyDead;
        }

        //// 監聽對話結束
        //if (dialogueTrigger != null)
        //    dialogueTrigger.GetDialogueSystem().OnDialogueComplete += OnDialogueFinished;
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(flowId))
            flowId = System.Guid.NewGuid().ToString();
    }

    private void OnDestroy()
    {
        foreach (var enemy in requiredEnemies)
        {
            if (enemy != null)
                enemy.OnEnemyDead -= OnEnemyDead;
        }

        if (dialogueTrigger != null)
            dialogueTrigger.OnThisDialogueFinished -= OnDialogueFinished;
    }

    private void OnEnemyDead(Enemy enemy)
    {
        aliveEnemyCount--;

        if (aliveEnemyCount <= 0)
        {
            UnlockDialogue();
        }
    }

    private void UnlockDialogue()
    {
        if (dialogueTrigger != null)
            dialogueTrigger.gameObject.SetActive(true);
    }

    private void OnDialogueFinished()
    {
        if (dialogueFinished) return;
        dialogueFinished = true;

        SaveManager.instance?.GetGameData()
            .finishedDialogueFlowIds.Add(flowId);


        if (targetGate != null)
            targetGate.gameObject.SetActive(true);

    }


    public void LoadData(GameData data)
    {
        if (data.finishedDialogueFlowIds.Contains(flowId))
        {
            dialogueFinished = true;

            if (dialogueTrigger != null)
                dialogueTrigger.gameObject.SetActive(false);

            if (targetGate != null)
                targetGate.gameObject.SetActive(true);
        }
    }

    public void SaveData(ref GameData data)
    {

    }
}
