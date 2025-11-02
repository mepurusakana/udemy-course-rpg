using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 5f;
    public int damage = 10;
    public float spawnAnimationDuration = 0.5f; // 進場動畫時間
    public float projectileSpeed = 6f;

    [Header("Animation")]
    public string spawnAnimationName = "Spawn";
    public string flyAnimationName = "Fly";
    public string hitAnimationName = "Hit";

    private Rigidbody2D rb;
    private Animator anim;
    private Transform target;
    private bool isFlying = false;
    private bool isHit = false;
    private CircleCollider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<CircleCollider2D>();

        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
        }
    }

    public void SetupProjectile(Transform _target)
    {
        target = _target;

        // 禁用物理和碰撞，直到進場動畫結束
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        // 播放進場動畫
        if (anim != null && !string.IsNullOrEmpty(spawnAnimationName))
        {
            anim.Play(spawnAnimationName);
        }

        // 在進場動畫後開始飛行
        Invoke(nameof(StartFlying), spawnAnimationDuration);

        // 設定生命週期
        Destroy(gameObject, lifeTime + spawnAnimationDuration);
    }

    private void StartFlying()
    {
        isFlying = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        col.enabled = true;

        // 播放飛行動畫
        if (anim != null && !string.IsNullOrEmpty(flyAnimationName))
        {
            anim.Play(flyAnimationName);
        }

        // 計算飛向玩家的方向
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * projectileSpeed;

            // 旋轉子彈朝向飛行方向
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit || !isFlying) return;

        // 碰到玩家
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage, this.transform);
            }
            PlayHitAnimation();
        }
        // 碰到地面
        else if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
        {
            PlayHitAnimation();
        }
    }

    private void PlayHitAnimation()
    {
        isHit = true;
        isFlying = false;

        // 停止移動
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        // 播放消失動畫
        if (anim != null && !string.IsNullOrEmpty(hitAnimationName))
        {
            anim.Play(hitAnimationName);
            // 等待動畫播放完畢後銷毀
            Destroy(gameObject, 0.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}