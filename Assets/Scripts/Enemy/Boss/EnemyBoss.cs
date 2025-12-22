using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBoss : Enemy
{
    [Header("Boss Root Default")]
    private Vector3 bossDefaultWorldPos;

    [Header("Boss Components")]
    public Transform bossHead;
    public Transform bossBody;
    public BossCore bossCore;
    public BossHand leftHand;
    public BossHand rightHand;

    [Header("Animators")]
    private Animator headAnimator;
    private Animator bodyAnimator;
    private Animator coreAnimator;


    #region State
    public EnemyBossIdleState idleState { get; private set; }
    public EnemyBossAttackState attackState { get; private set; }
    public EnemyBossTiredState tiredState { get; private set; }
    public EnemyBossDefeatedState defeatedState { get; private set; }
    #endregion

    [Header("Attack Settings")]
    //public float attackCooldown = 2f;
    public float attackDuration;
    public float tiredDuration;
    //[HideInInspector] public float lastAttackTime;

    [Header("Independent Cooldowns")]
    public float handAttackIntervalMin = 2f;
    public float handAttackIntervalMax = 4f;
    public float ultimateCooldown = 10f; // 大招冷卻時間

    private float leftHandTimer;
    private float rightHandTimer;
    private float ultimateTimer;

    // 狀態標記
    public bool isSweepLocked {  get; private set; }
    public bool isPreparingTired = false;


    public bool isUsingSkill = false;      // 正在施放大招中 (鎖定所有行動)
    private bool isUltimatePending = false; // 大招準備就緒，正在等待雙手空閒

    private float totalAttackPhaseTimer = 0f;

    [Header("Default Positions")]
    private Vector3 headDefaultPos;
    private Vector3 bodyDefaultPos;
    private Vector3 coreDefaultPos;
    private Vector3 leftHandDefaultPos;
    private Vector3 rightHandDefaultPos;

    [Header("Attack Prefabs")]
    public GameObject energyBallPrefab;

    private bool isInTired = false;
    private float attackTimer = 0f;


    protected override void Awake()
    {
        base.Awake();

        if (bossHead != null) headAnimator = bossHead.GetComponent<Animator>();
        if (bossBody != null) bodyAnimator = bossBody.GetComponent<Animator>();
        if (bossCore != null) coreAnimator = bossCore.GetComponent<Animator>();


        // 初始化狀態
        idleState = new EnemyBossIdleState(this, stateMachine, "Idle", this);
        attackState = new EnemyBossAttackState(this, stateMachine, "Attack", this);
        tiredState = new EnemyBossTiredState(this, stateMachine, "Tired", this);
        defeatedState = new EnemyBossDefeatedState(this, stateMachine, "Defeated", this);
    }

    protected override void Start()
    {
        base.Start();

        // 記錄所有部位的初始位置
        SaveDefaultPositions();

        // 設置初始狀態
        stateMachine.Initialize(idleState);

        // --- 修改重點 1: 初始設為 Kinematic (完全不受力，不會被手帶動) ---
        rb.bodyType = RigidbodyType2D.Kinematic;
        // rb.gravityScale = 0; // Kinematic 不需要設重力，它本身就不受重力
        rb.velocity = Vector2.zero;

        // 初始化計時器，給予一點隨機錯開，避免開場左右手同時攻擊太生硬
        ResetAttackTimers();
    }

    // 每次進入 Attack State 時重置計時器
    public void ResetAttackTimers()
    {
        leftHandTimer = Random.Range(1f, 2f);
        rightHandTimer = Random.Range(2.5f, 3.5f);
        ultimateTimer = ultimateCooldown;

        isUsingSkill = false;
        isUltimatePending = false;
        totalAttackPhaseTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();

        // 攻擊計時器
        if (stateMachine.currentState == attackState)
        {
            {
                HandleAttackLogic();
                CheckPhaseDuration();
            }
        }
    }

    private void CheckPhaseDuration()
    {
        if (isPreparingTired || isInTired)
            return;

        if (!isUsingSkill && !isUltimatePending)
        {
            totalAttackPhaseTimer += Time.deltaTime;

            if (totalAttackPhaseTimer >= attackDuration)
            {
                totalAttackPhaseTimer = 0f;

                //  不馬上進 Tired
                isPreparingTired = true;
            }
        }
    }

    private void HandleAttackLogic()
    {
        //準備進入 Tired，不再發送任何攻擊
        if (isPreparingTired)
            return;


        // 1. 如果正在「施放大招中」，完全鎖死，什麼都不做
        if (isUsingSkill) return;

        // 2. 如果「大招等待中 (Pending)」，只檢查是否可以發動
        if (isUltimatePending)
        {
            // 檢查條件：左右手都不在攻擊狀態
            if (!leftHand.isAttacking && !rightHand.isAttacking)
            {
                // 發動大招
                StartCoroutine(EnergyBallAttackRoutine());
                isUltimatePending = false; // 清除等待標記
            }
            return; // 等待期間，不執行下面的左右手計時器
        }

        // 3. 正常階段：分別計算三個計時器

        // --- 左手計時器 ---
        leftHandTimer -= Time.deltaTime;
        if (leftHandTimer <= 0)
        {
            // 只有手有空才發動
            if (!leftHand.isAttacking)
            {
                PerformHandAction(leftHand);
                leftHandTimer = Random.Range(handAttackIntervalMin, handAttackIntervalMax);
            }
        }

        // --- 右手計時器 ---
        rightHandTimer -= Time.deltaTime;
        if (rightHandTimer <= 0)
        {
            // 只有手有空才發動
            if (!rightHand.isAttacking)
            {
                PerformHandAction(rightHand);
                rightHandTimer = Random.Range(handAttackIntervalMin, handAttackIntervalMax);
            }
        }

        // --- 大招計時器 ---
        ultimateTimer -= Time.deltaTime;
        if (ultimateTimer <= 0)
        {
            // 時間到，標記為「準備發動」，這會暫停上面的左右手計時器邏輯
            isUltimatePending = true;
            ultimateTimer = ultimateCooldown; // 重置
        }
    }

    // 隨機選擇手的動作 (劍 或 橫掃)
    private void PerformHandAction(BossHand hand)
    {
        int action = Random.Range(0, 2); // 0 or 1
        if (action == 0)
            hand.PerformSwordAttack();
        else
            hand.PerformSweepAttack();
    }

    public void LockSweep()
    {
        isSweepLocked = true;
    }

    public void UnlockSweep()
    {
        isSweepLocked = false;
    }


    private IEnumerator EnergyBallAttackRoutine()
    {
        isUsingSkill = true; // 鎖定狀態

        // ... (這裡是你原本的能量球邏輯，移動到中間生成球) ...
        // 手掌移動到較高位置
        Vector3 leftTargetPos = new Vector3(30f, 10f, 0f);
        Vector3 rightTargetPos = new Vector3(-30f, 10f, 0f);

        float moveDuration = 1f;
        float elapsed = 0f;
        Vector3 leftStart = leftHand.transform.localPosition;
        Vector3 rightStart = rightHand.transform.localPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            leftHand.transform.localPosition = Vector3.Lerp(leftStart, leftTargetPos, t);
            rightHand.transform.localPosition = Vector3.Lerp(rightStart, rightTargetPos, t);
            yield return null;
        }

        float centerX = (leftHand.transform.position.x + rightHand.transform.position.x) * 0.5f;
        float centerY = 8f; // 你想要的高度（世界座標）
        Vector3 centerPos = new Vector3(centerX, centerY, 0f);
        GameObject energyBall = Instantiate(energyBallPrefab, centerPos, Quaternion.identity);

        yield return new WaitForSeconds(3f);
        if (energyBall != null) Destroy(energyBall);

        // 結束後復位
        ResetToDefaultPositions();
        yield return new WaitForSeconds(1f);

        isUsingSkill = false; // 解除鎖定
    }

    private void SaveDefaultPositions()
    {
        bossDefaultWorldPos = transform.position;

        if (bossHead != null) headDefaultPos = bossHead.localPosition;
        if (bossBody != null) bodyDefaultPos = bossBody.localPosition;
        if (bossCore != null) coreDefaultPos = bossCore.transform.localPosition;
        if (leftHand != null) leftHandDefaultPos = leftHand.transform.localPosition;
        if (rightHand != null) rightHandDefaultPos = rightHand.transform.localPosition;

        // 同步回手（如果你在 BossHand 新增了 defaultLocalPos）
        if (leftHand != null) leftHand.defaultLocalPos = leftHandDefaultPos;
        if (rightHand != null) rightHand.defaultLocalPos = rightHandDefaultPos;
    }

    public void ResetToDefaultPositions()
    {
        StartCoroutine(MoveToDefaultPositions());
    }

    private IEnumerator MoveToDefaultPositions()
    {
        float duration = 2.5f;
        float elapsed = 0f;

        Vector3 headStart = bossHead.localPosition;
        Vector3 bodyStart = bossBody.localPosition;
        Vector3 coreStart = bossCore.transform.localPosition;
        Vector3 leftStart = leftHand.transform.localPosition;
        Vector3 rightStart = rightHand.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (bossHead != null) bossHead.localPosition = Vector3.Lerp(headStart, headDefaultPos, t);
            if (bossBody != null) bossBody.localPosition = Vector3.Lerp(bodyStart, bodyDefaultPos, t);
            if (bossCore != null) bossCore.transform.localPosition = Vector3.Lerp(coreStart, coreDefaultPos, t);
            if (leftHand != null) leftHand.transform.localPosition = Vector3.Lerp(leftStart, leftHandDefaultPos, t);
            if (rightHand != null) rightHand.transform.localPosition = Vector3.Lerp(rightStart, rightHandDefaultPos, t);

            yield return null;
        }
    }

    public void PlayIntoTiredAnimation()
    {
        if (headAnimator != null) headAnimator.SetTrigger("IntoTired");
        if (bodyAnimator != null) bodyAnimator.SetTrigger("IntoTired");
        if (coreAnimator != null) coreAnimator.SetTrigger("IntoTired");

        // 左右手的動畫由 BossHand 自己觸發
        if (leftHand != null) leftHand.PlayIntoTiredAnimation();
        if (rightHand != null) rightHand.PlayIntoTiredAnimation();
    }

    public void PlaySwitchFromTiredAnimation()
    {
        if (headAnimator != null) headAnimator.SetTrigger("SwitchFromTired");
        if (bodyAnimator != null) bodyAnimator.SetTrigger("SwitchFromTired");
        if (coreAnimator != null) coreAnimator.SetTrigger("SwitchFromTired");

        if (leftHand != null) leftHand.PlaySwitchFromTiredAnimation();
        if (rightHand != null) rightHand.PlaySwitchFromTiredAnimation();
    }

    public void EnterTiredState()
    {
        if (isInTired) return;

        isInTired = true;
        isPreparingTired = false;

        //  停止發送任何攻擊命令
        isUsingSkill = true;
        isUltimatePending = false;

        // 播放進入疲勞動畫
        PlayIntoTiredAnimation();

        // --- 修改重點 2: Rigidbody 還在，但 不參與碰撞 / 重力 / 解算
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        //  子物件開始掉落
        SetBossPartsPhysics(true);

        bossCore.SetVulnerable(true); // 核心可被攻擊
        stateMachine.ChangeState(tiredState);
    }

    public void ExitTiredState()
    {

        isInTired = false;
        isUsingSkill = false;

        // 播放離開疲勞動畫
        PlaySwitchFromTiredAnimation();

        //rb.simulated = true;
        //rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(MoveBossBackToAir(0.8f));

        StopBossPartsPhysics(true);

        bossCore.SetVulnerable(false); // 核心不可被攻擊
        ResetToDefaultPositions();

        // 等待位置重置完成後回到 Idle
        StartCoroutine(WaitAndReturnToIdle());
    }

    private void SetBossPartsPhysics(bool enable)
    {
        SetPart(bossHead, enable);
        SetPart(bossBody, enable);
        SetPart(bossCore.transform, enable);
        SetPart(leftHand.transform, enable);
        SetPart(rightHand.transform, enable);
    }

    private void StopBossPartsPhysics(bool enable)
    {
        SetPart(bossHead, !enable);
        SetPart(bossBody, !enable);
        SetPart(bossCore.transform, !enable);
        SetPart(leftHand.transform, !enable);
        SetPart(rightHand.transform, !enable);
    }

    private void SetPart(Transform part, bool enable)
    {
        if (part == null) return;

        Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (enable)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 6f;
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }
    }

    private IEnumerator MoveBossBackToAir(float duration)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, bossDefaultWorldPos, t / duration);
            yield return null;
        }

        transform.position = bossDefaultWorldPos;
    }


    private IEnumerator WaitAndReturnToIdle()
    {
        yield return new WaitForSeconds(1f);
        stateMachine.ChangeState(idleState);
    }

    // 隨機選擇攻擊
    public void PerformRandomAttack()
    {
        // 1. 如果正在施放技能（能量球），直接退出，不執行新攻擊
        if (isUsingSkill) return;

        // 2. (選用優化) 如果手正在單獨攻擊，也不要強行施放能量球，避免手瞬間瞬移
        if (leftHand.isAttacking || rightHand.isAttacking) return;

        int attackType = Random.Range(0, 5);

        switch (attackType)
        {
            case 0:
                leftHand.PerformSwordAttack();
                break;
            case 1:
                leftHand.PerformSweepAttack();
                break;
            case 2:
                rightHand.PerformSwordAttack();
                break;
            case 3:
                rightHand.PerformSweepAttack();
                break;
            case 4:
                PerformDoubleHandEnergyBall();
                break;
        }
    }

    private void PerformDoubleHandEnergyBall()
    {
        StartCoroutine(EnergyBallAttackRoutine());
    }

    //private IEnumerator EnergyBallAttackRoutine()
    //{
    //    isUsingSkill = true;
    //    // 手掌移動到較高位置
    //    Vector3 leftTargetPos = new Vector3(30f, 30f, 0f);
    //    Vector3 rightTargetPos = new Vector3(-30f, 30f, 0f);

    //    float moveDuration = 1f;
    //    float elapsed = 0f;

    //    Vector3 leftStart = leftHand.transform.localPosition;
    //    Vector3 rightStart = rightHand.transform.localPosition;

    //    while (elapsed < moveDuration)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = elapsed / moveDuration;

    //        leftHand.transform.localPosition = Vector3.Lerp(leftStart, leftTargetPos, t);
    //        rightHand.transform.localPosition = Vector3.Lerp(rightStart, rightTargetPos, t);

    //        yield return null;
    //    }

    //    // 在兩手中間生成能量球
    //    Vector3 centerPos = (leftHand.transform.position + rightHand.transform.position) / 2f;
    //    GameObject energyBall = Instantiate(energyBallPrefab, centerPos, Quaternion.identity);

    //    // 持續發射 3 秒
    //    yield return new WaitForSeconds(3f);

    //    if (energyBall != null) Destroy(energyBall);

    //    // 手部歸位 (建議加這段，讓手平滑回到原位，而不是瞬間跳回)
    //    ResetToDefaultPositions();
    //    yield return new WaitForSeconds(1f); // 等待歸位

    //    // --- 步驟 B: 解鎖 ---
    //    isUsingSkill = false;
    //}

    public override void Die()
    {
        base.Die();
        // --- 修改重點 4: 死亡時切換回 Dynamic (掉落) ---
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        stateMachine.ChangeState(defeatedState);
    }
}