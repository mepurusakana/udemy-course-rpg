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

    public void Setup(int _damage, float _direction)
    {
        damage = _damage;
        direction = (int)_direction;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 設定飛行方向
        rb.velocity = new Vector2(speed * direction, 0);

        //  根據方向翻轉 Sprite
        if (spriteRenderer != null)
            spriteRenderer.flipX = (direction == -1);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
                enemyStats.TakeDamage(damage, this.transform);

            StickToTarget(collision.transform);
        }
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            StickToTarget(collision.transform);
        }
    }

    private void StickToTarget(Transform target)
    {
        isStuck = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        transform.SetParent(target);

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            // 黏地面 → 放到 Tilemap 後面
            if (target.GetComponentInChildren<TilemapRenderer>() != null)
            {
                sr.sortingLayerName = "Ground";
                sr.sortingOrder = -1;
            }
            // 黏敵人 → 比敵人圖層低
            else
            {
                SpriteRenderer enemySR = target.GetComponentInChildren<SpriteRenderer>();
                if (enemySR != null)
                {
                    sr.sortingLayerID = enemySR.sortingLayerID;
                    sr.sortingOrder = enemySR.sortingOrder - 1;
                }
            }
        }

        Destroy(gameObject, 5f);
    }
}
