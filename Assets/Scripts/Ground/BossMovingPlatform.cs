using UnityEngine;

public class BossMovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private Vector2 moveDirection = Vector2.right;

    [Header("Lifetime Settings")]
    public float lifetime = 10f; // 平台存活時間
    private float spawnTime;

    [Header("Bounds")]
    public float destroyDistance = 20f; // 超出這個距離就銷毀

    private Rigidbody2D rb;
    private Vector3 startPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        spawnTime = Time.time;
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 移動平台
        rb.velocity = moveDirection * moveSpeed;

        // 檢查是否超時或超出範圍
        if (Time.time - spawnTime > lifetime ||
            Vector3.Distance(startPosition, transform.position) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // 讓玩家可以站在平台上
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 將玩家設為平台的子物件，讓玩家跟著平台移動
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 玩家離開平台時取消父子關係
            collision.transform.SetParent(null);
        }
    }
}