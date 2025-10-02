using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondMissile : MonoBehaviour
{
    [Header("飛彈設定")]
    [SerializeField] private float speed = 8f; // 飛行速度
    [SerializeField] private float rotationSpeed = 200f; // 旋轉速度
    [SerializeField] private float maxLifeTime = 5f; // 最大存在時間

    private Transform target; // 鎖定目標
    private int damage;
    private float lifeTimer;
    private Rigidbody2D rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lifeTimer = maxLifeTime;
    }

    public void Setup(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
    }

    private void Update()
    {
        if (hasHit) return;

        // 生命計時器
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // 追蹤目標
        if (target != null && target.gameObject.activeSelf)
        {
            TrackTarget();
        }
        else
        {
            // 目標消失，直線飛行
            rb.velocity = transform.right * speed;
        }
    }

    private void TrackTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * speed;

        // 飛彈旋轉對準敵人
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // 檢查是否擊中敵人
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage);
            }

            hasHit = true;
            OnHit();
        }
        // 檢查是否擊中地面
        else if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            OnHit();
        }
    }

    private void OnHit()
    {
        // 可以在這裡添加擊中特效
        // Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // 播放擊中音效
        // AudioManager.instance.PlaySFX(4, null);

        Destroy(gameObject);
    }
}