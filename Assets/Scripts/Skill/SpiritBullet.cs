using UnityEngine;

public class SpiritBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 15;
    private Vector2 direction;

    public void Setup(Vector3 targetPos)
    {
        direction = (targetPos - transform.position).normalized;
        Destroy(gameObject, 5f);
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
                enemyStats.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
