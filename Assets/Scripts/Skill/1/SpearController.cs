using UnityEngine;

public class SpearController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;
    public float travelDistance = 30f;

    [Header("Damage Settings")]
    public int damage = 20;

    [Header("Animation Settings")]
    public float introTime = 0.3f;  // 出現動畫時間
    public float outroTime = 0.3f;  // 消失動畫時間

    private Vector3 startPosition;
    private Vector3 direction;
    private float distanceTraveled = 0f;
    private bool isFlying = false;
    private bool isIntroPlaying = true;
    private bool isOutroPlaying = false;

    public Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    void Awake()
    {
        // 獲取組件
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        // 確保 Rigidbody2D 設置正確
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;  // 使用 Kinematic 以便手動控制移動
            rb.gravityScale = 0f;
        }
    }

    void Start()
    {
        startPosition = transform.position;

        // 初始時關閉碰撞器，等出現動畫播完再開啟
        if (capsuleCollider != null)
            capsuleCollider.enabled = false;

        // 播放出現動畫
        if (animator != null)
        {
            animator.SetTrigger("Intro");
        }
        else
        {
            // 如果沒有動畫，直接開始飛行
            OnIntroFinished();
        }
    }

    /// <summary>
    /// 設置長矛的方向和傷害（從技能系統調用）
    /// </summary>
    public void Setup(Vector2 _direction, int _damage)
    {
        direction = _direction.normalized;
        damage = _damage;

        // 根據方向旋轉長矛
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (isFlying)
        {
            MoveSpear();
        }
    }

    private void MoveSpear()
    {
        // 計算移動距離
        float moveDistance = speed * Time.deltaTime;
        transform.Translate(Vector3.right * moveDistance, Space.Self);

        distanceTraveled += moveDistance;

        // 檢查是否超過飛行距離
        if (distanceTraveled >= travelDistance)
        {
            StartOutro();
        }
    }

    /// <summary>
    /// 出現動畫結束時調用（從 AnimationTriggerForwarder 調用）
    /// </summary>
    public void OnIntroFinished()
    {
        isIntroPlaying = false;
        isFlying = true;

        // 開啟碰撞器
        if (capsuleCollider != null)
            capsuleCollider.enabled = true;

        Debug.Log("長矛開始飛行！");
    }

    /// <summary>
    /// 開始播放消失動畫
    /// </summary>
    private void StartOutro()
    {
        float moveDistance = speed * Time.deltaTime;
        transform.Translate(Vector3.right * moveDistance, Space.Self);

        if (isOutroPlaying) return;

        isOutroPlaying = true;

        // 關閉碰撞器
        if (capsuleCollider != null)
            capsuleCollider.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Outro");
        }
        else
        {
            // 如果沒有動畫，直接銷毀
            OnOutroFinished();
        }
    }

    /// <summary>
    /// 消失動畫結束時調用（從 AnimationTriggerForwarder 調用）
    /// </summary>
    public void OnOutroFinished()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只在飛行時才能造成傷害
        if (!isFlying) return;

        // 檢查是否碰到敵人
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage, this.transform);
                Debug.Log($"長矛擊中敵人，造成 {damage} 點傷害！");
            }

            // 可選：擊中敵人後立即消失
            // StartOutro();
        }

        // 檢查是否碰到地面或牆壁
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("長矛碰到地面！");
            StartOutro();
        }
    }

    // 在編輯器中顯示飛行軌跡
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && isFlying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, startPosition + direction * travelDistance);
        }
    }
}