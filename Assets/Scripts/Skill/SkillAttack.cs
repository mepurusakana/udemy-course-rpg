using UnityEngine;

public class SkillAttack : MonoBehaviour
{
    private int damage;

    public void Setup(int _damage)
    {
        damage = _damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage, this.transform);
            }
        }
    }
}