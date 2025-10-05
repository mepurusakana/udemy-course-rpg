using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondMissile : MonoBehaviour
{
    [Header("飛彈設定")]
    [SerializeField] private float speed = 8f; // 飛行速度
    [SerializeField] private float maxLifeTime = 5f; // 最大存在時間

    [Header("特效設定（可選）")]
    //[SerializeField] private GameObject hitEnemyEffectPrefab; // 擊中敵人特效
    //[SerializeField] private GameObject hitGroundEffectPrefab; // 擊中地面特效

    private Transform target; // 目標（用於計算初始方向）
    private int damage;
    private float lifeTimer;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private Vector2 direction; // 飛行方向

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lifeTimer = maxLifeTime;
    }

    public void Setup(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;

        // 計算朝向目標的方向
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            // 如果沒有目標，就朝右飛
            direction = Vector2.right;
        }

        // 立即設置飛彈的旋轉角度，讓尖端朝向飛行方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 設置速度
        rb.velocity = direction * speed;
    }

    private void Update()
    {
        if (hasHit) return;

        // 生命計時器
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Debug.Log("飛彈超時，自動消失");
            Destroy(gameObject);
            return;
        }

        // 保持直線飛行（以防受到其他力影響）
        rb.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // 檢查是否擊中敵人
        if (collision.CompareTag("Enemy"))
        {
            // 造成傷害
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
                Debug.Log($"飛彈擊中敵人 {collision.name}，造成 {damage} 點傷害！");
            }

            hasHit = true;
            OnHitEnemy();
        }
        // 檢查是否擊中地面
        else if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("飛彈擊中地面，消失！");
            hasHit = true;
            OnHitGround();
        }
    }

    private void OnHitEnemy()
    {
        // 添加擊中敵人的特效
        //if (hitEnemyEffectPrefab != null)
        //{
        //    Instantiate(hitEnemyEffectPrefab, transform.position, Quaternion.identity);
        //}

        // 播放擊中敵人音效
        // AudioManager.instance.PlaySFX(4, null);

        Destroy(gameObject);
    }

    private void OnHitGround()
    {
        // 添加擊中地面的特效
        //if (hitGroundEffectPrefab != null)
        //{
        //    Instantiate(hitGroundEffectPrefab, transform.position, Quaternion.identity);
        //}

        // 播放擊中地面音效
        // AudioManager.instance.PlaySFX(5, null);

        Destroy(gameObject);
    }
}