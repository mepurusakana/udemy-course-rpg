using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float platformMoveSpeed = 2f;

    private Vector3 target;
    public Vector2 platformVelocity;

    public Player player;

    private void Start()
    {
        target = pointB.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, platformMoveSpeed * Time.deltaTime);


        Vector2 oldPos = transform.position;
        // 移動平台的邏輯
        // transform.position = ...
        platformVelocity = (Vector2)transform.position - oldPos;

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }
    }

    void FixedUpdate() {
        // 移動平台
        transform.position += (Vector3)platformVelocity * Time.fixedDeltaTime;
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            if (player != null) {
                // 將平台速度加到玩家身上
                player.rb.velocity += platformVelocity;
            }
        }
    }
}
