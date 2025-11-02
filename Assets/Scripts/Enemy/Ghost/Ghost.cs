using System.Collections;
using UnityEngine;

public class Ghost : Enemy
{
    #region States
    public GhostIdleState idleState { get; private set; }
    public GhostMoveState moveState { get; private set; }
    public GhostAttackState attackState { get; private set; }
    public GhostStunnedState stunnedState { get; private set; }
    public GhostDeadState deadState { get; private set; }
    #endregion

    [Header("Flying Range Info")]
    public Transform leftPoint;
    public Transform rightPoint;
    private Vector2 patrolCenter;
    private float patrolRadius;

    [Header("Projectile Info")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Player Detection")]
    public float detectionRadius = 5f;
    public float detectionAngle = 90f; // 扇形角度
    public LayerMask playerLayer; // 若留為 0，會 fallback 使用 Enemy.whatIsPlayer

    private Coroutine knockbackCoroutine; // 記錄擊退協程，避免重複



    protected override void Awake()
    {
        base.Awake();

        idleState = new GhostIdleState(this, stateMachine, "Idle", this);
        moveState = new GhostMoveState(this, stateMachine, "Move", this);
        attackState = new GhostAttackState(this, stateMachine, "Attack", this);
        stunnedState = new GhostStunnedState(this, stateMachine, "Stunned", this);
        deadState = new GhostDeadState(this, stateMachine, "Dead", this);
    }

    protected override void Start()
    {
        base.Start();

        // 計算巡邏區域（如果 left/right 沒設會報錯，請在 Inspector 填好）
        if (leftPoint != null && rightPoint != null)
        {
            patrolCenter = (leftPoint.position + rightPoint.position) / 2f;
            patrolRadius = Vector2.Distance(leftPoint.position, rightPoint.position) / 2f;
        }

        // 設定無重力
        rb.gravityScale = 0f;

        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        // 更新FirePoint的方向
        UpdateFirePointDirection();
    }

    // —— 修正：必須回傳 RaycastHit2D 與父類一致 —— 
    public override RaycastHit2D IsPlayerDetected()
    {
        // 如果 player 尚未被指派，fallback 試圖從 PlayerManager 拿
        if (player == null && PlayerManager.instance != null)
            player = PlayerManager.instance.player;

        if (player == null)
            return default(RaycastHit2D);

        Vector2 dirToPlayer = (player.transform.position - transform.position);
        float dist = dirToPlayer.magnitude;
        if (dist > detectionRadius)
            return default(RaycastHit2D);

        Vector2 dirNorm = dirToPlayer.normalized;
        Vector2 facing = facingDir == 1 ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(facing, dirNorm);
        if (angleToPlayer > detectionAngle * 0.5f)
            return default(RaycastHit2D);

        // 選擇使用的 LayerMask：若 playerLayer 沒設定(=0)，就 fallback 用 Enemy 的 whatIsPlayer
        LayerMask mask = playerLayer != 0 ? playerLayer : whatIsPlayer;

        // 從自己位置往玩家方向發射 ray，檢查是否有直線可見玩家（避免穿牆偵測）
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirNorm, detectionRadius, mask);

        if (hit.collider != null)
        {
            // 如果 raycast 擊中玩家的 collider，就回傳該 hit
            if (hit.collider.gameObject == player.gameObject)
                return hit;
        }

        // 若你希望在沒有 collider 精準命中的情況下依然當作偵測到（例如玩家 Layer 沒開），
        // 可以在這裡改成 return a fake RaycastHit2D，但通常不建議 — 我們回傳 default 表示沒偵測到。
        return default(RaycastHit2D);
    }

    private void UpdateFirePointDirection()
    {
        if (firePoint == null) return;

        // FirePoint跟隨Ghost的朝向
        Vector3 localPos = firePoint.localPosition;
        localPos.x = Mathf.Abs(localPos.x) * facingDir;
        firePoint.localPosition = localPos;
    }

    public override void OnTakeDamage(Transform _attacker)
    {
        base.OnTakeDamage(_attacker);

        // 面向攻擊者
        if (_attacker.position.x < transform.position.x && facingDir == 1)
            Flip();
        else if (_attacker.position.x > transform.position.x && facingDir == -1)
            Flip();

        //  執行擊退
        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(ApplyKnockback(_attacker));

        // 進入暈眩狀態（若未死亡）
        if (!isDead)
        {
            stateMachine.ChangeState(stunnedState);
        }
    }

    private IEnumerator ApplyKnockback(Transform attacker)
    {
        // 根據攻擊者方向決定擊退方向
        float knockbackDir = transform.position.x < attacker.position.x ? -1 : 1;

        // 設定擊退速度（可自行調整力度）
        Vector2 knockbackVelocity = new Vector2(knockbackDir * 8f, 2f);
        rb.velocity = knockbackVelocity;

        // 可選：暫時允許重力（如果想要稍微往上拋的感覺）
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 1f;

        // 保持擊退 0.2 秒
        yield return new WaitForSeconds(0.2f);

        // 恢復狀態
        rb.gravityScale = originalGravity;
        rb.velocity = Vector2.zero;
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }

    // —— 修正：覆寫時權限必須與父類一致（protected override） —— 
    protected override void OnDrawGizmos()
    {
        // 呼叫父類以維持原先 Enemy 的 gizmo（例如攻擊距離線）
        base.OnDrawGizmos();

        // 繪製巡邏圓形區域（如果有設定點）
        if (leftPoint != null && rightPoint != null)
        {
            Vector2 center = (leftPoint.position + rightPoint.position) / 2f;
            float radius = Vector2.Distance(leftPoint.position, rightPoint.position) / 2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, radius);
        }

        // 繪製玩家偵測扇形（僅示意用）
        Gizmos.color = Color.red;
        Vector3 facingDir3D = facingDir == 1 ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, detectionAngle * 0.5f) * facingDir3D * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -detectionAngle * 0.5f) * facingDir3D * detectionRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
