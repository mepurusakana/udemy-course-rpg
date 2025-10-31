using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Controller : MonoBehaviour
{
    private Enemy enemy;

    [SerializeField] private int damage;
    [SerializeField] private string targetLayerName = "Player";

    [SerializeField] private float xVelocity;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private bool canMove;
    [SerializeField] private bool flipped;

    private void Update()
    {
        if(canMove)
            rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer(targetLayerName))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.lastAttacker = this.transform; // 記錄攻擊來源
                player.stats.TakeDamage(damage);
            }
            StuckInto(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StuckInto(collision);
        }
    }

    private void StuckInto(Collider2D collision)
    {
        GetComponentInChildren<ParticleSystem>().Stop();
        GetComponent<CapsuleCollider2D>().enabled = false;
        canMove = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent = collision.transform;

        Destroy(gameObject, Random.Range(5, 7));
    }

    public void SetDirection(int dir)
    {
        if (dir == -1)
        {
            FlipArrow();
        }
        canMove = true;
    }

    public void FlipArrow()
    {
        if (flipped)
            return;

        xVelocity *= -1;
        flipped = true;

        transform.Rotate(0, 180, 0); // ⬅️ 水平翻轉箭頭
    }
}
