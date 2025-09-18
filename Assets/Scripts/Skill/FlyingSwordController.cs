using UnityEngine;

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
        direction = _direction > 0 ? 1 : -1;
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
        Destroy(gameObject, 5f); // 5¬í«á®ø¥¢
    }
}
