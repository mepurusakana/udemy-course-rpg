using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public float lifeTime = 0.3f;
    private int damage;
    private Vector2 boxSize;
    public Transform player;
    private BoxCollider2D box;
    private List<CharacterStats> hitTargets = new List<CharacterStats>();

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        Vector2 size = box.size;
        Vector2 center = (Vector2)transform.position + box.offset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            size,
            transform.eulerAngles.z
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out SpriteShatter2D shatter))
                shatter.Shatter();
        }
    }
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

        //if (collision.TryGetComponent(out SpriteShatter2D shatter))
        //{
        //    shatter.Shatter();

        //}

        //if (collision.CompareTag("CanDestroy"))
        //{
        //    SpriteShatter2D shatter = collision.GetComponent<SpriteShatter2D>();
        //    if (shatter != null)
        //    {
        //        shatter.Shatter();
        //    }
        //}
    }
}
