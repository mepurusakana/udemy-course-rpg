using UnityEngine;

public class BossCore : MonoBehaviour
{
    [Header("Core Settings")]
    public EnemyBoss boss;
    private PolygonCollider2D coreCollider;
    private bool isVulnerable = false;

    [Header("Visual Feedback")]
    public SpriteRenderer coreSprite;
    //public Color vulnerableColor = Color.red;
    //public Color invulnerableColor = Color.gray;

    private void Awake()
    {
        coreCollider = GetComponent<PolygonCollider2D>();
        if (coreSprite == null)
            coreSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetVulnerable(false);
    }

    public void TakeCoreDamage(Player player)
    {
        if (!isVulnerable) return;
        if (boss == null || boss.stats == null) return;

        player.stats.DoDamage(boss.stats, player.transform);

        EntityFX fx=GetComponent<EntityFX>();
        if (fx != null)
        {
            fx.CreateHitFx(transform, false);
        }
    }

    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;

        if (coreCollider != null)
            coreCollider.enabled = vulnerable;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有在 vulnerable 狀態才能被攻擊
        if (!isVulnerable) return;

        // 檢測玩家的攻擊
        if (collision.CompareTag("Player"))
        {
            // 通知 Boss 受到攻擊（可以在這裡處理傷害邏輯）
            if (boss != null && boss.stats != null)
            {
                // 假設玩家攻擊造成傷害
                Player player = PlayerManager.instance.player;
                if (player != null)
                {
                    player.stats.DoDamage(boss.stats, player.transform);
                }
            }
        }
    }
}