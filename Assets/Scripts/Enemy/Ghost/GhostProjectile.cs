using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerStats>().TakeDamage(damage, this.transform);
            Destroy(gameObject);
        }
    }
}
