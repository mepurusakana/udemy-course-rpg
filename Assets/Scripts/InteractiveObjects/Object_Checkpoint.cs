using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Object_Checkpoint : MonoBehaviour, ISaveable
{
    private Checkpoint checkpoint;

    private Player player;
    private PlayerStats stats;
    private Object_Checkpoint[] allCheckpoints;

    private void Awake()
    {
        allCheckpoints = FindObjectsByType<Object_Checkpoint>(FindObjectsSortMode.None);

        checkpoint = GetComponent<Checkpoint>();

        player = FindFirstObjectByType<Player>(); // 或 Player.instance
        if (player != null)
            stats = player.GetComponent<PlayerStats>();
    }

    public void ActivateCheckpoint(bool activate)
    {
        // TODO: 播動畫/開燈效果
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Player>() != null )
        {
            if (!checkpoint.activationStatus)
            {
                checkpoint.ActivateCheckpoint();
                Debug.Log("Checkpoint activated!");

                // 可以加音效
                AudioManager.instance?.PlaySFX(15, transform);
            }

            var data = SaveManager.instance.GetGameData();
            data.savedCheckpoint = transform.position;
            SaveManager.instance.SaveGame();
        }
    }

    public void LoadData(GameData data)
    {
        // 不直接移動玩家，移動玩家的動作放到 SaveManager.LoadGame 之後統一處理
        bool active = Vector3.Distance(data.savedCheckpoint, transform.position) < 0.01f;
        ActivateCheckpoint(active);
    }

    public void SaveData(ref GameData data)
    {
        if (this == null || checkpoint == null)
            return; // 防止被銷毀後還被呼叫

        bool active = data.savedCheckpoint == transform.position;
    }
}