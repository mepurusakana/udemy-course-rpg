using UnityEngine;
using System.Collections;

public class BossHand : MonoBehaviour
{
    [Header("Hand Settings")]
    public bool isLeftHand = true;
    public float sweepSpeed;
    public float swordStabSpeed;
    public int swordDamage = 40;
    public int sweepDamage = 30;


    [Header("References")]
    public EnemyBoss boss;
    public Transform swordGroundCheck;
    public Transform handGroundCheck;
    public LayerMask groundLayer;

    [Header("Sweep Settings")]
    public Vector3 sweepActPos; //橫掃發起攻擊位置
    public Vector3 sweepStartPos; //橫掃進入前搖位置
    public float sweepDistance; // 橫掃距離

    [Header("Sweep Smoke Effect")]
    public GameObject sweepSmokePrefab;
    public Transform sweepSmokeSpawnPoint; // 通常在手掌前緣

    [Header("Impact Effect")]
    public GameObject groundImpactPrefab; // 地面衝擊特效

    [Header("Default (local)")]
    public Vector3 defaultLocalPos = Vector3.zero; // 可在 Inspector 設定

    private PolygonCollider2D attackCollider;
    private BoxCollider2D groundCollider;
    public Animator handAnimator;

    public bool isAttacking { get; private set; } = false;

    // 用於動畫系統
    private GameObject currentSword; // 當前生成的劍

    private void Awake()
    {
        groundCollider = GetComponentInChildren<BoxCollider2D>();
        attackCollider = GetComponent<PolygonCollider2D>();
        handAnimator = GetComponentInChildren<Animator>();

        if (groundCollider != null)
            groundCollider.enabled = true; // 初始開啟碰撞
    }

    private void Start()
    {
        if (defaultLocalPos == Vector3.zero)
            defaultLocalPos = transform.localPosition;
    }

    public void PlayIntoTiredAnimation()
    {
        if (handAnimator != null)
            handAnimator.SetTrigger("IntoTired");
    }

    public void PlaySwitchFromTiredAnimation()
    {
        if (handAnimator != null)
            handAnimator.SetTrigger("SwitchFromTired");
    }


    // 劍攻擊
    public void PerformSwordAttack()
    {
        if (boss.isUsingSkill) return;
        if (isAttacking) return;
        StartCoroutine(SwordAttackRoutine());
    }

    private IEnumerator SwordAttackRoutine()
    {
        float currentSpeed = 0f;
        float acceleration = swordStabSpeed * 3f;
        float maxSpeed = swordStabSpeed;


        isAttacking = true; // 鎖定


        // 1. 移動到空中位置
        Vector3 airPosition = new Vector3(isLeftHand ? 20f : -20f, 16f, 0f);
        yield return StartCoroutine(MoveToPosition(airPosition, 1f));

        // 2. 觸發動畫
        if (handAnimator != null)
            handAnimator.SetTrigger("SwordAttack");

        // 3. 生成劍
        //yield return new WaitForSeconds(1.2f); // 根據你的動畫長度調整
        Vector3 precastPosition = new Vector3(isLeftHand ? 20f : -20f, 18f, 0f);
        yield return StartCoroutine(MoveToPosition(precastPosition, 1.2f));
        //yield return new WaitForSeconds(1.2f); // 根據你的動畫長度調整

        // 4. 向下刺（使用 localPosition）
        bool hitGround = false;
        float stabDistance = 0f;
        float maxStabDistance = 100f;

        while (!hitGround && stabDistance < maxStabDistance)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            float move = currentSpeed * Time.deltaTime;
            transform.localPosition += Vector3.down * move;
            stabDistance += move;

            Vector2 checkPos = swordGroundCheck.position;

            if (Physics2D.Raycast(checkPos, Vector2.down, 0.5f, groundLayer))
            {
                hitGround = true;
            }

            yield return null;
        }

        // 5. 釋放地面衝擊
        if (groundImpactPrefab != null)
        {
            Vector3 impactPos = swordGroundCheck.position;
            impactPos.z = 0f;

            //Instantiate(groundImpactPrefab, impactPos, Quaternion.identity);
            GameObject impact = Instantiate(groundImpactPrefab, impactPos, Quaternion.identity);
            Destroy(impact, 2f);
        }

        // 6. 收回劍
        yield return new WaitForSeconds(1.4f); // 根據你的動畫長度調整

        // 回到預設位置
        yield return StartCoroutine(MoveToPosition(defaultLocalPos, 0.6f));

        isAttacking = false;
    }

    // 橫掃攻擊
    public void PerformSweepAttack()
    {
        if (boss.isUsingSkill) return;
        if (isAttacking) return;
        if (boss.isSweepLocked) return; // 關鍵：Boss 已被 Sweep 鎖定

        StartCoroutine(SweepAttackRoutine());
    }

    private IEnumerator SweepAttackRoutine()
    {
        //Vector3 pos = transform.localPosition;
        //pos.y =handGroundCheck.localPosition.y;
        //transform.localPosition = pos;

        float currentSpeed = 0f;
        float acceleration = sweepSpeed * 4f; // 橫掃通常更爆
        float maxSpeed = sweepSpeed;

        isAttacking = true;
        boss.LockSweep();

        Vector3 precastPos = new Vector3(isLeftHand ? 5.5f : -5.5f, 0f, 0f);
        yield return StartCoroutine(MoveToPosition(sweepStartPos, 1.2f));

        // 1. 觸發動畫
        if (handAnimator != null)
            handAnimator.SetTrigger("SweepAttack");

        // 2. 移動到起始位置（對角）
        Vector3 actPos = new Vector3(isLeftHand ? 6f : -6f, 0.5f, 0f);
        yield return StartCoroutine(MoveToPosition(sweepActPos, 0.4f));

        // 3. 等待 PrecastDelay 動畫播放完成
        yield return new WaitForSeconds(0.5f);

        // 4. 啟用碰撞
        if (attackCollider != null) attackCollider.enabled = true;

        // 5. 快速橫掃
        float sweepDir = isLeftHand ? -1f : 1f;
        float sweptDistance = 0f;

        while (sweptDistance < sweepDistance)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            float moveAmount = currentSpeed * Time.deltaTime;
            transform.localPosition += new Vector3(sweepDir * moveAmount, 0f, 0f);
            sweptDistance += Mathf.Abs(moveAmount);

            yield return null;
        }

        // 6. 關閉碰撞
        if (attackCollider != null) attackCollider.enabled = false;

        // 回到預設位置
        Vector3 endAttackPos = new Vector3(isLeftHand ? 60f : -60f, 12f, 0f);
        yield return StartCoroutine(MoveToPosition(endAttackPos, 0f));
        yield return StartCoroutine(MoveToPosition(defaultLocalPos, 0.8f));

        boss.UnlockSweep();
        isAttacking = false;
    }

    public void SpawnSweepSmoke()
    {
        if (sweepSmokePrefab == null) return;

        Transform parent = sweepSmokeSpawnPoint != null
            ? sweepSmokeSpawnPoint
            : transform;

        GameObject smoke = Instantiate(
            sweepSmokePrefab,
            parent.position,
            Quaternion.identity,
            parent   // 關鍵：設為子物件
        );

        // 重設 localPosition，避免 Prefab 偏移
        smoke.transform.localPosition = Vector3.zero;

        // --- 左右手 Flip ---
        SpriteRenderer sr = smoke.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = !isLeftHand;
        }

        Destroy(smoke, 0.5f);
    }


    // 在 GenerateSword 動畫的某一幀調用此方法
    public void OnSwordGenerated()
    {
        currentSword.transform.localPosition = Vector3.zero;
        currentSword.transform.localRotation = Quaternion.identity;
    }

    // 在 SwordAttackExit 動畫結束時調用（如果需要）
    public void OnSwordAttackComplete()
    {
        // 可以在這裡處理劍攻擊完成的邏輯
        Debug.Log($"{gameObject.name} 劍攻擊完成");
    }

    // 在 SweepAttack 動畫的關鍵幀調用（啟動橫掃碰撞）
    public void OnSweepAttackStart()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    // 在 SweepAttack 動畫結束前調用（關閉橫掃碰撞）
    public void OnSweepAttackEnd()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }





    // MoveToPosition (local)
    private IEnumerator MoveToPosition(Vector3 targetLocalPos, float duration)
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t);
            yield return null;
        }

        transform.localPosition = targetLocalPos;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (!isAttacking) return;

    //    PlayerStats player = collision.GetComponent<PlayerStats>();
    //    if (player != null)
    //    {
    //        boss.stats.DoDamage(player, transform);
    //    }
    //}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只在飛行時才能造成傷害
        if (!isAttacking) return;

        // 檢查是否碰到敵人
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(sweepDamage, this.transform);
                Debug.Log($"長矛擊中敵人，造成 {sweepDamage} 點傷害！");
            }

            // 可選：擊中敵人後立即消失
            // StartOutro();
        }
    }
}
