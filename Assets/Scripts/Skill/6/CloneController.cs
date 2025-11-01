using System.Collections;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    [Header("Clone Settings")]
    [SerializeField] private float maxLifeTime = 60f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] public float attackRange = 3f;
    [SerializeField] private int damage = 15;

    [Header("Ground & Wall Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    [Header("Attack Settings")]
    [SerializeField] public Transform attackCheck;
    [SerializeField] private float attackCooldown = 1f;

    private Player player;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float lifeTimer;
    private float attackTimer;
    private Transform enemy;
    private Transform clone;
    private int facingDir = 1;
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

        // 顯形
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if (!isActive)
            return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifeTime)
        {
            DismissClone();
            return;
        }

        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0;
            }
        }

        FindClosestEnemy();

        if (enemy != null)
            HandleCombat();
        else
            HandlePatrol();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1;
        spriteRenderer.color = color;
        isActive = true;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isActive = false;
        Color color = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 0;
        spriteRenderer.color = color;

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
            skillManager.ClearCloneReference();

        Destroy(gameObject);
    }

    private void FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = hit.transform;
                }
            }
        }

        enemy = closestEnemy;
    }

    private void HandleCombat()
    {
        if (enemy == null) return;

        float dist = Vector2.Distance(transform.position, enemy.position);

        if (dist <= attackRange)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            anim.SetBool("isMoving", false);

            if (enemy.position.x > transform.position.x && facingDir == -1) Flip();
            else if (enemy.position.x < transform.position.x && facingDir == 1) Flip();

            if (canAttack)
            {
                rb.velocity = Vector2.zero;
                Attack();
                isAttacking = true;
            }
        }
        else if (!isAttacking)
        {
            MoveTowardsTarget(enemy.position);
        }
    }

    private void HandlePatrol()
    {
        bool isWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

        if (isWall || !isGroundAhead)
            Flip();

        rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
        anim.SetBool("isMoving", true);
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        if (target.x > transform.position.x && facingDir == -1)
            Flip();
        else if (target.x < transform.position.x && facingDir == 1)
            Flip();

        rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
        anim.SetBool("isMoving", true);
    }

    private void Attack()
    {
        rb.velocity = Vector2.zero;
        canAttack = false;
        attackTimer = 0;
        anim.SetTrigger("Attack");
    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackRange);
        foreach (var enemy in hitEnemies)
        {
            CharacterStats stats = enemy.GetComponent<CharacterStats>();
            if (stats != null && enemy.GetComponent<Enemy>() != null)
            {
                stats.TakeDamage(damage, this.transform);
            }
        }
    }

    public void Flip()
    {
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
    }

    public void SetFacingDirection(int dir)
    {
        if (dir != facingDir)
            Flip();
    }

    public void DismissClone()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private void OnDrawGizmosSelected()
    {
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheck.position, attackRange);
        }
    }
}
