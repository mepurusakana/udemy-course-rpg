using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public float lifeTime = 0.3f;
    private int damage;
    private List<CharacterStats> hitTargets = new List<CharacterStats>();

    public void Setup(int _damage)
    {
        damage = _damage;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterStats targetStats = collision.GetComponent<CharacterStats>();
        if (targetStats != null && !hitTargets.Contains(targetStats))
        {
            hitTargets.Add(targetStats);
            targetStats.TakeDamage(damage);
        }
    }
}
