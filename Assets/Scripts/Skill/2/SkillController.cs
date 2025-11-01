using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public float lifeTime = 0.3f;
    private int damage;
    public Transform player;
    private List<CharacterStats> hitTargets = new List<CharacterStats>();

    public void Setup(int _damage)
    {
        damage = _damage;
        Destroy(gameObject, lifeTime);
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
