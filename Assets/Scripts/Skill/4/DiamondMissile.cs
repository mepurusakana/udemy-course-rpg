using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondMissile : MonoBehaviour
{
    [Header("飛彈設定")]
    [SerializeField] private float speed = 8f; // 飛行速度
    [SerializeField] private float maxLifeTime = 5f; // 最大存在時間

    [Header("進場動畫設定")]
    [SerializeField] private bool hasIntroAnimation = true; // 是否有進場動畫
    [SerializeField] private float introAnimationDuration = 0.3f; // 進場動畫時長
    [SerializeField] private AnimationCurve introCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 進場曲線
    [SerializeField] private float introScaleMultiplier = 0.3f; // 進場時的初始縮放

    [Header("特效設定（可選）")]
    [SerializeField] private GameObject hitEnemyEffectPrefab; // 擊中敵人特效
    [SerializeField] private GameObject hitGroundEffectPrefab; // 擊中地面特效
    [SerializeField] private GameObject spawnEffectPrefab; // 生成特效

    private Transform target; // 目標（用於計算初始方向）
    private int damage;
    private float lifeTimer;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private bool isIntroFinished = false;
    private Vector2 direction; // 飛行方向
    private Vector3 originalScale; // 原始縮放
    private Animator anim; // 動畫控制器

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        lifeTimer = maxLifeTime;
        originalScale = transform.localScale;
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

        // 播放生成特效
        if (spawnEffectPrefab != null)
        {
            Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
        }

        // 如果有進場動畫，啟動進場協程
        if (hasIntroAnimation)
        {
            StartCoroutine(PlayIntroAnimation());
        }
        else
        {
            // 沒有進場動畫，直接開始飛行
            isIntroFinished = true;
            rb.velocity = direction * speed;
        }
    }

    private IEnumerator PlayIntroAnimation()
    {
        // 觸發進場動畫（如果有 Animator）
        if (anim != null)
        {
            anim.SetTrigger("Intro");
        }

        // 設置初始縮放
        transform.localScale = originalScale * introScaleMultiplier;

        // 在進場動畫期間，飛彈慢速移動
        float elapsedTime = 0f;
        Vector2 introVelocity = direction * (speed * 0.3f); // 進場時速度較慢

        while (elapsedTime < introAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / introAnimationDuration;

            // 使用曲線來平滑縮放
            float curveValue = introCurve.Evaluate(progress);
            transform.localScale = Vector3.Lerp(
                originalScale * introScaleMultiplier,
                originalScale,
                curveValue
            );

            // 進場時慢速移動
            rb.velocity = Vector2.Lerp(introVelocity, direction * speed, curveValue);

            yield return null;
        }

        // 確保最終縮放正確
        transform.localScale = originalScale;

        // 進場動畫結束，切換到正常飛行
        isIntroFinished = true;
        rb.velocity = direction * speed;

        // 觸發正常飛行動畫（如果有）
        if (anim != null)
        {
            anim.SetBool("Flying", true);
        }
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

        // 只有進場動畫結束後才保持直線飛行
        if (isIntroFinished)
        {
            // 保持直線飛行（以防受到其他力影響）
            rb.velocity = direction * speed;
        }
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
        if (hitEnemyEffectPrefab != null)
        {
            Instantiate(hitEnemyEffectPrefab, transform.position, Quaternion.identity);
        }

        // 播放擊中敵人音效
        // AudioManager.instance.PlaySFX(4, null);

        Destroy(gameObject);
    }

    private void OnHitGround()
    {
        // 添加擊中地面的特效
        if (hitGroundEffectPrefab != null)
        {
            Instantiate(hitGroundEffectPrefab, transform.position, Quaternion.identity);
        }

        // 播放擊中地面音效
        // AudioManager.instance.PlaySFX(5, null);

        Destroy(gameObject);
    }

    // 如果使用 Animator 的事件系統（可選）
    public void OnIntroFinished()
    {
        isIntroFinished = true;
        rb.velocity = direction * speed;

        if (anim != null)
        {
            anim.SetBool("Flying", true);
        }
    }
}