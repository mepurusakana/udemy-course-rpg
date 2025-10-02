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

    [Header("組件引用")]
    private Animator anim;
    private Player player; // 召喚者
    private float attackTimer;
    private float lifeTimer;
    private bool isActive = false;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();

        lifeTimer = lifeTime;
        attackTimer = attackInterval;

        // 播放召喚動畫
        //anim.SetTrigger("Summon");
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

        // 攻擊計時器
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            TryAttackEnemy();
            attackTimer = attackInterval;
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

    // 在Scene視圖中顯示偵測範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}