using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float throwForceX = 8f;
    public float throwForceY = 6f;
    public float initialBounceForceY = 5f; // 第一次彈跳高度
    public float bounceDecay = 0.7f;       // 每次反彈衰減比例 (0~1之間)
    public int maxBounces = 0;
    public GameObject explosionPrefab;

    private int bounces = 0;
    private int damage;
    private Rigidbody2D rb;
    private float lifetime = 3f;
    private bool exploded = false;
    private float currentBounceForceY;
    private float direction = 1f; // 保存朝向 (1=右, -1=左)

    public void Setup(int _damage, float facingDir)
    {
        damage = _damage;
        direction = facingDir;

        rb = GetComponent<Rigidbody2D>();

        // 設定初始速度，依照朝向
        rb.velocity = new Vector2(throwForceX * direction, throwForceY);

        // 翻轉外觀
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);

        currentBounceForceY = initialBounceForceY; // 設定第一次彈跳力
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0 && !exploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            bounces++;

            //// 彈跳：保留水平速度，使用當前彈跳力
            //rb.velocity = new Vector2(throwForceX * direction, currentBounceForceY);

            //// 下次反彈高度會更低
            //currentBounceForceY *= bounceDecay;

            if (bounces > maxBounces && !exploded)
            {
                StartCoroutine(DelayedExplode(0f));
            }
        }
    }

    private System.Collections.IEnumerator DelayedExplode(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Explosion exp = explosion.GetComponent<Explosion>();
        exp.Setup(damage);

        Destroy(gameObject);
    }
}
