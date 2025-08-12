using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Object_Checkpoint : MonoBehaviour, ISaveable
{
    private Player player;
    private PlayerStats stats;
    private Object_Checkpoint[] allCheckpoints;

    private void Awake()
    {
        allCheckpoints = FindObjectsByType<Object_Checkpoint>(FindObjectsSortMode.None);

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
        if(collision.gameObject.GetComponent<Player>() != null )
        {
            foreach (var point in allCheckpoints)
                point.ActivateCheckpoint(false);

            var data = SaveManager.instance.GetGameData();
            data.savedCheckpoint = transform.position;
            data.playerHealth = stats.currentHealth;

            SaveManager.instance.SaveGame();
            ActivateCheckpoint(true);
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
        bool active = data.savedCheckpoint == transform.position;
    }
}