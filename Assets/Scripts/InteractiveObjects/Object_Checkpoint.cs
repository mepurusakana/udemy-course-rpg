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
    }

    public void ActivateCheckpoint(bool activate)
    {
        // TODO: 播動畫/開燈效果
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player)
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
        bool active = data.savedCheckpoint == transform.position;
        ActivateCheckpoint(active);

        if(active)
        {
            Player.instance.TeleportPlayer(transform.position);
            stats.SetHealth(data.playerHealth);
        }
    }

    public void SaveData(ref GameData data)
    {
        bool active = data.savedCheckpoint == transform.position;
    }
}