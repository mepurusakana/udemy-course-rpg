using UnityEngine;

public class Explosion : MonoBehaviour
{
    private int damage;

    public void Setup(int _damage)
    {
        damage = _damage;
        Destroy(gameObject, 0.5f); // Ãz¬µ0.5¬í«á§R°£
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
            }
        }
    }
}

