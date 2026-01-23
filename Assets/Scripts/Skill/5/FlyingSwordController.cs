using UnityEngine;
using UnityEngine.Tilemaps;

public class FlyingSwordController : MonoBehaviour
{
    public float speed = 10f;
    public LayerMask groundLayer;

    public Transform player;
    private int damage;
    private int direction;
    private Rigidbody2D rb;
    private bool isStuck = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D swordCollider;

    //public float enemyEmbedDepthMin = 0.12f;
    //public float enemyEmbedBySpriteWidth = 0.3f;
    //public float enemyEmbedJitter = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        swordCollider = GetComponent<Collider2D>();
    }

    public void Setup(int _damage, float _direction)
    {
        damage = _damage;
        direction = (int)_direction;

        rb.velocity = new Vector2(speed * direction, 0);
        if (spriteRenderer != null)
            spriteRenderer.flipX = (direction == -1);
    }

    private void Update()
    {
        if (!isStuck)
        {
            rb.velocity = new Vector2(speed * direction, rb.velocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStuck) return;

        if (collision.TryGetComponent(out SpriteShatter2D shatter))
        {
            shatter.Shatter();

            // 你可以選擇是否讓長矛消失
            //StartOutro();
            Destroy(gameObject);
        }

        // 命中靶子
        SwordTarget target = collision.GetComponent<SwordTarget>();
        if (target != null)
        {
            StickToTarget(collision.transform, true);
            //target.Hit(this);
            return;
        }

        SwordFlyingTarget flyTarget = collision.GetComponent<SwordFlyingTarget>();
        if (flyTarget != null)
        {
            StickToTarget(collision.transform, true);
            //target.Hit(this);
            return;
        }

        // 命中敵人
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
                enemyStats.TakeDamage(damage, this.transform);

            StickToTarget(collision.transform, true);
        }
        // 命中地面
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            StickToTarget(collision.transform, false);
        }
    }

    private void StickToTarget(Transform target, bool playHitAnim)
    {
        isStuck = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        if (swordCollider != null)
            swordCollider.enabled = false;

        transform.SetParent(target, true);

        SpriteRenderer sr = spriteRenderer;
        if (sr != null)
        {
            // Tilemap → 排後面
            if (target.GetComponentInChildren<TilemapRenderer>() != null)
            {
                sr.sortingLayerName = "Ground";
                sr.sortingOrder = -1;
            }
            else
            {
                // 目標是敵人或靶子 → 排在前面
                SpriteRenderer targetSR = target.GetComponentInChildren<SpriteRenderer>();
                if (targetSR != null)
                {
                    sr.sortingLayerID = targetSR.sortingLayerID;
                    sr.sortingOrder = targetSR.sortingOrder + 1;
                }

                // 插入效果
                //Vector2 fwd = (rb != null && rb.velocity.sqrMagnitude > 0.001f)
                //    ? (Vector2)rb.velocity.normalized
                //    : new Vector2(direction, 0f).normalized;

                //float depthByWidth = sr.bounds.extents.x * enemyEmbedBySpriteWidth;
                //float depth = Mathf.Max(enemyEmbedDepthMin, depthByWidth) + Random.Range(-enemyEmbedJitter, enemyEmbedJitter);

                //transform.position += (Vector3)(fwd * depth);
            }
        }


        animator.SetTrigger("Hit");
        Destroy(gameObject, 5f); // 5秒後自動刪除
    }
}
