using Unity.Burst.CompilerServices;
using UnityEngine;

public class SkillAttack : MonoBehaviour
{
    private Player player => GetComponent<Player>();

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

            BossCore bossCore = collision.GetComponent<BossCore>();
            if (bossCore != null)
            {
                bossCore.TakeCoreDamage(damage, this.transform);
            }
        }
    }
}