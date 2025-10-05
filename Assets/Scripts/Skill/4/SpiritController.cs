using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritController : MonoBehaviour
{
    [Header("精靈設定")]
    [SerializeField] private float detectionRadius = 10f; // 偵測半徑
    [SerializeField] private float attackInterval = 1.5f; // 攻擊間隔
    [SerializeField] private float lifeTime = 60f; // 存在時間
    [SerializeField] private LayerMask enemyLayer; // 敵人圖層

    [Header("飛彈設定")]
    [SerializeField] private GameObject diamondMissilePrefab; // 菱形飛彈預製體
    [SerializeField] private Transform firePoint; // 發射點
    [SerializeField] private int missileDamage = 15; // 飛彈傷害

    [Header("跟隨設定")]
    [SerializeField] private float followSpeed = 5f; // 跟隨速度
    [SerializeField] private float maxFollowDistance = 8f; // 最大跟隨距離（超過此距離會移動）
    [SerializeField] private float attackRangeFromPlayer = 6f; // 離玩家多近才能攻擊
    [SerializeField] private float followOffsetX = 2f; // 跟隨時的X軸偏移
    [SerializeField] private float followOffsetY = 0.5f; // 跟隨時的Y軸偏移
    [SerializeField] private float smoothTime = 0.3f; // 平滑跟隨時間

    [Header("組件引用")]
    private Animator anim;
    private Player player; // 召喚者
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float attackTimer;
    private float lifeTimer;
    private bool isActive = false;
    private bool isFollowing = false;
    private Vector2 velocity = Vector2.zero; // 用於平滑移動

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        player = FindObjectOfType<Player>();

        lifeTimer = lifeTime;
        attackTimer = attackInterval;

        // 設置 Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0; // 精靈漂浮，不受重力影響
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 防止旋轉
        }

        StartCoroutine(ActivateAfterSummon());
    }

    private IEnumerator ActivateAfterSummon()
    {
        yield return new WaitForSeconds(0.5f);
        isActive = true;
        anim.SetBool("Idle", true);
    }

    private void Update()
    {
        if (!isActive) return;

        // 生命計時器
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            DismissSpirit();
            return;
        }

        // 檢查玩家是否再次按下4鍵
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DismissSpirit();
            return;
        }

        // 檢查與玩家的距離
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // 如果離玩家太遠，先跟隨玩家
        if (distanceToPlayer > maxFollowDistance)
        {
            FollowPlayer();
            isFollowing = true;
        }
        // 如果在合適距離內，停止跟隨並嘗試攻擊
        else
        {
            if (isFollowing)
            {
                StopFollowing();
                isFollowing = false;
            }

            // 只有在離玩家足夠近時才攻擊
            if (distanceToPlayer <= attackRangeFromPlayer)
            {
                // 攻擊計時器
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    TryAttackEnemy();
                    attackTimer = attackInterval;
                }
            }
            else
            {
                // 不在攻擊範圍內，慢慢移動到合適位置
                MoveToIdealPosition();
            }
        }

        // 根據移動方向翻轉精靈
        FlipSprite();
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        // 計算目標位置（玩家位置加上偏移）
        Vector2 targetPosition = new Vector2(
            player.transform.position.x + (player.facingDir * followOffsetX),
            player.transform.position.y + followOffsetY
        );

        // 平滑移動到目標位置
        Vector2 newPosition = Vector2.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // 應用移動
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }

        // 設置移動動畫（如果有）
        if (anim != null)
        {
            anim.SetBool("Idle", true);
            // 如果你有 Move 動畫，可以這樣切換：
            // anim.SetBool("Move", true);
        }
    }

    private void MoveToIdealPosition()
    {
        if (player == null) return;

        // 計算理想位置（比 maxFollowDistance 稍微近一點）
        float idealDistance = attackRangeFromPlayer * 0.8f;
        Vector2 direction = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
        Vector2 idealPosition = (Vector2)player.transform.position + direction * idealDistance;

        // 慢慢移動到理想位置
        Vector2 newPosition = Vector2.MoveTowards(
            transform.position,
            idealPosition,
            followSpeed * 0.5f * Time.deltaTime
        );

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }

    private void StopFollowing()
    {
        // 停止移動，進入待機狀態
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        velocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("Idle", true);
            // 如果你有 Move 動畫：
            // anim.SetBool("Move", false);
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null || player == null) return;

        // 計算精靈到玩家的方向
        float directionToPlayer = player.transform.position.x - transform.position.x;

        // 根據方向翻轉精靈
        if (Mathf.Abs(directionToPlayer) > 0.1f)
        {
            spriteRenderer.flipX = directionToPlayer < 0;
        }
    }

    private void TryAttackEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform closestEnemy = GetClosestEnemy(enemies);

            if (closestEnemy != null)
            {
                // 切換成攻擊動畫
                anim.SetTrigger("Attack");

                // 發射飛彈
                FireMissile(closestEnemy);

                // 回到 Idle 狀態
                StartCoroutine(ReturnToIdle());
            }
        }
    }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(0.5f); // 等攻擊動畫播完
        anim.SetBool("Idle", true);
    }

    private Transform GetClosestEnemy(Collider2D[] enemies)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            // 確保敵人還活著
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    private void FireMissile(Transform target)
    {
        if (diamondMissilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("飛彈預製體或發射點未設定！");
            return;
        }

        // 生成飛彈
        GameObject missile = Instantiate(diamondMissilePrefab, firePoint.position, Quaternion.identity);
        DiamondMissile missileScript = missile.GetComponent<DiamondMissile>();

        if (missileScript != null)
        {
            missileScript.Setup(target, missileDamage);
        }

        // 播放攻擊音效（如果有）
        // AudioManager.instance.PlaySFX(3, null);
    }

    public void DismissSpirit()
    {
        if (!isActive) return;

        isActive = false;

        // 通知 SkillManager 清除引用
        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            skillManager.ClearSpiritReference();
        }

        // 播放消失動畫
        if (anim != null)
        {
            anim.SetTrigger("Dismiss");
        }

        // 等待消失動畫播放完畢後銷毀
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
                skillManager.ClearSpiritReference();
            }
        }
    }

    // 在Scene視圖中顯示偵測範圍和跟隨範圍
    private void OnDrawGizmosSelected()
    {
        // 攻擊偵測範圍（黃色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 最大跟隨距離（紅色）
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.transform.position, maxFollowDistance);

            // 攻擊範圍（綠色）
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.transform.position, attackRangeFromPlayer);

            // 連線顯示精靈到玩家的距離
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }
}