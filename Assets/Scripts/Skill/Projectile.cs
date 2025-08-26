using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float throwForceX = 8f;
    public float throwForceY = 6f;
    public int maxBounces = 2;
    public GameObject explosionPrefab;

    private int bounces = 0;
    private int damage;
    private Rigidbody2D rb;
    private float lifetime = 3f;
    private bool exploded = false;

    public void Setup(int _damage, float facingDir)
    {
        damage = _damage;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(throwForceX * facingDir, throwForceY);
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
            if (bounces > maxBounces)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        exploded = true;
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Explosion exp = explosion.GetComponent<Explosion>();
        exp.Setup(damage);
        Destroy(gameObject);
    }
}
