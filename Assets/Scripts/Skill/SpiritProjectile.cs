using UnityEngine;

public class SpiritProjectile : MonoBehaviour
{
    public float speed = 8f;
    public LayerMask groundLayer;
    private int damage;
    private Vector2 direction;

    public void Setup(int _damage, Vector2 _direction)
    {
        damage = _damage;
        direction = _direction;
        Destroy(gameObject, 5f); // 最長存在時間
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
