using System.Collections;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private float maxLifeTime = 60f; // 存在時間60秒
    [SerializeField] private float fadeInDuration = 1f; // 顯形時間
    [SerializeField] private float moveSpeed = 3f; // 移動速度
    [SerializeField] private float detectionRange = 8f; // 偵測敵人範圍
    [SerializeField] public float attackRange = 3f; // 攻擊範圍
    [SerializeField] private int damage = 15; // 攻擊傷害

    [Header("Ground & Wall Detection")]
    [SerializeField] private Transform groundCheck; // 地面檢測點
    [SerializeField] private Transform wallCheck; // 牆壁檢測點
    [SerializeField] private LayerMask whatIsGround; // 地面Layer
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    [Header("Attack Settings")]
    [SerializeField] public Transform attackCheck; // 攻擊檢測點
    [SerializeField] private float attackCooldown = 1f; // 攻擊冷卻

    // 組件引用
    private Player player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // 狀態變數
    private float lifeTimer;
    private float attackTimer;
    private Transform enemy;
    private int facingDir = 1; // 1 = 右, -1 = 左
    private bool canAttack = true;
    private bool isActive = false;
    public bool isAttacking = false;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {

            // 初始化為完全透明
        Color color = spriteRenderer.color;
        color.a = 0;
        spriteRenderer.color = color;
        
        // 開始顯形
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (!isActive)
            return;

        // 生命計時
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifeTime)
        {
            DismissClone();
            return;
        }

        // 攻擊冷卻計時
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0;
            }
        }

        // 尋找目標
        FindClosestEnemy();

        // 根據狀態執行行為
        if (enemy != null)
        {
            HandleCombat();
        }
        else
        {
            HandlePatrol();
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        Color color = spriteRenderer.color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        // 確保完全不透明
        color.a = 1;
        spriteRenderer.color = color;
        isActive = true;
    }

    private void FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            // 檢查是否為敵人
            if (hit.GetComponent<Enemy>() != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);

                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }
        }

        enemy = closestEnemy;
    }

    private void HandleCombat()
    {
        if (enemy == null)
            return;

        float distanceToTarget = Vector2.Distance(transform.position, enemy.position);

        // 在攻擊範圍內
        if (distanceToTarget <= attackRange)
        {
            // 停止移動
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetBool("isMoving", false);

            // 面向敵人
            if (enemy.position.x > transform.position.x && facingDir == -1)
                Flip();
            else if (enemy.position.x < transform.position.x && facingDir == 1)
                Flip();

            // 攻擊
            if (canAttack)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                Attack();
                isAttacking = true;
            }
        }
        else
        {
            if(!isAttacking)
            // 移動向敵人
            MoveTowardsTarget(enemy.position);
        }
    }

    private void HandlePatrol()
    {
        
        // 檢查前方是否有牆或沒有地面
        bool isWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

        if (isWall || !isGroundAhead)
        {
            Flip();
        }

        // 繼續巡邏
        rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
        anim.SetBool("isMoving", true);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        // 決定移動方向
        if (targetPosition.x > transform.position.x && facingDir == -1)
            Flip();
        else if (targetPosition.x < transform.position.x && facingDir == 1)
            Flip();

        // 移動
        rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
        anim.SetBool("isMoving", true);
    }

    private void Attack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        canAttack = false;
        attackTimer = 0;

        // 播放攻擊動畫
        anim.SetTrigger("Attack");

        // 攻擊判定會在動畫事件中調用 DealDamage()
    }

    // 這個方法會被動畫事件調用
    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackRange);

        foreach (var enemy in hitEnemies)
        {
            CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
            if (enemyStats != null && enemy.GetComponent<Enemy>() != null)
            {
                enemyStats.TakeDamage(damage);
            }
        }
    }

    public void Flip()
    {
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
    }
    public void SetFacingDirection(int direction)
    {
        if (direction != facingDir)
        {
            Flip();
        }
    }

    public void DismissClone()
    {
        if (!isActive) return;

        isActive = false;

        // 通知 SkillManager 清除引用
        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            skillManager.ClearCloneReference();
        }

        float elapsedTime = 0;
        float fadeOutDuration = 0.5f;
        Color color = spriteRenderer.color;

        if (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeOutDuration);
            spriteRenderer.color = color;
            return;
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDestroy()
    {
        // 當精靈被銷毀時，確保清除 SkillManager 的引用
        if (player != null)
        {
            SkillManager skillManager = player.GetComponent<SkillManager>();
            if (skillManager != null)
            {
                skillManager.ClearCloneReference();
            }
        }
    }

    // Debug用：繪製偵測範圍
    private void OnDrawGizmosSelected()
    {
        // 偵測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻擊範圍
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheck.position, attackRange);
        }

        // 地面檢測
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        // 牆壁檢測
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheckDistance * facingDir);
        }
    }
}