using UnityEngine;

public class Explosion : MonoBehaviour
{
    private int damage;
    private float radius;
    private CircleCollider2D circle;

    private void Awake()
    {
        circle = GetComponent<CircleCollider2D>();
    }
    private void Start()
    {
        float radius = circle.radius * Mathf.Abs(transform.lossyScale.x);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + circle.offset,
            radius
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
        Destroy(gameObject, 0.5f); // 爆炸0.5秒後刪除
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Explosion hit: {collision.name}, Tag: {collision.tag}");

        //if (collision.TryGetComponent(out SpriteShatter2D shatter))
        //{
        //    shatter.Shatter();

        //}

        //if(collision.CompareTag("CanDestroy"))
        //{
        //    SpriteShatter2D shatter = collision.GetComponent<SpriteShatter2D>();
        //    if (shatter != null)    
        //    {
        //        shatter.Shatter();
        //    }
        //}

        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage, this.transform);
                Debug.Log("敵人受傷成功！");
            }
        }
    }
}

