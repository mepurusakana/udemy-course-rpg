using UnityEngine;
using UnityEngine.Tilemaps;

public class FlyingSwordController : MonoBehaviour
{
    public float speed = 10f;
    public LayerMask groundLayer;
    private int damage;
    private int direction;
    private Rigidbody2D rb;
    private bool isStuck = false;

    public void Setup(int _damage, float _direction)
    {
        damage = _damage;
        direction = (int)_direction; // ← 儲存方向
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(speed * direction, 0);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
            {
                enemyStats.TakeDamage(damage);
            }

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
            // 黏地面時 → 強制在 Tilemap 後面
            if (target.GetComponentInChildren<TilemapRenderer>() != null)
            {
                sr.sortingLayerName = "Ground";
                sr.sortingOrder = -1; // 比 TilemapRenderer 小
            }
            // 黏敵人時 → 比敵人的 renderer 小
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
