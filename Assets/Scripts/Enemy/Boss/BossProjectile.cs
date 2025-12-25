using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private float speed;
    private float lifeTime;
    private float selfRotation;

    public int damage = 1;

    private Rigidbody2D rb;
    private Transform target;
    private CircleCollider2D col;


    public void Initialize(float speed, float lifeTime, float selfRotation)
    {
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.selfRotation = selfRotation;

        Destroy(gameObject, lifeTime);
    }

    private void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        col = rb.GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;

        if (selfRotation != 0)
        {
            transform.Rotate(0, 0, selfRotation * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        // 碰到玩家
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage, this.transform);
            }
            PlayHitAnimation();
        }
        // 碰到地面
        else if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
        {
            PlayHitAnimation();
        }
    }

    private void PlayHitAnimation()
    {

        // 停止移動
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        Destroy(gameObject);
    }
}
