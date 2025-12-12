using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : Enemy
{
    [Header("Boss Settings")]
    public EnemyBossCore bossCore; // 核心引用
    public Transform leftHand;
    public Transform rightHand;
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Attack Settings")]
    public int attacksBeforeTired = 4; // 攻擊4次後進入Tired
    private int currentAttackCount = 0;

    [Header("Hand Attack Settings")]
    public float handAttackCooldown = 2f;
    private float lastHandAttackTime;

    [Header("Core Settings")]
    public Transform coreSpawnPosition; // Core出現的位置
    public float coreAppearDuration = 10f; // Core出現持續時間

    [Header("Platform Settings")]
    public GameObject movingPlatformPrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public float platformSpawnInterval = 3f;
    private Coroutine platformSpawnCoroutine;

    #region States
    public EnemyBossIdleState idleState { get; private set; }
    public EnemyBossBattleState battleState { get; private set; }
    public EnemyBossAttackState attackState { get; private set; }
    public EnemyBossTiredState tiredState { get; private set; }
    public EnemyBossStunnedState stunnedState { get; private set; }
    public EnemyBossDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyBossIdleState(this, stateMachine, "Idle", this);
        battleState = new EnemyBossBattleState(this, stateMachine, "Idle", this);
        attackState = new EnemyBossAttackState(this, stateMachine, "Attack", this);
        tiredState = new EnemyBossTiredState(this, stateMachine, "Tired", this);
        stunnedState = new EnemyBossStunnedState(this, stateMachine, "Stunned", this);
        deadState = new EnemyBossDeadState(this, stateMachine, "Death", this);
    }

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        stateMachine.Initialize(idleState);

        // 初始隱藏Core
        if (bossCore != null)
            bossCore.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
    }

    // 手部攻擊（左右手輪流）
    public void PerformHandAttack()
    {
        currentAttackCount++;

        // 隨機選擇左手或右手攻擊
        bool useLeftHand = Random.value > 0.5f;

        if (useLeftHand)
            TriggerLeftHandAttack();
        else
            TriggerRightHandAttack();

        // 檢查是否達到Tired條件
        if (currentAttackCount >= attacksBeforeTired)
        {
            currentAttackCount = 0;
            stateMachine.ChangeState(tiredState);
        }
    }

    private void TriggerLeftHandAttack()
    {
        // 隨機選擇劍或投射物
        bool useSword = Random.value > 0.5f;

        if (leftHand != null)
        {
            Animator handAnim = leftHand.GetComponent<Animator>();
            if (handAnim != null)
            {
                handAnim.SetTrigger(useSword ? "LeftSword" : "LeftProjectile");
            }
        }
    }

    private void TriggerRightHandAttack()
    {
        bool useSword = Random.value > 0.5f;

        if (rightHand != null)
        {
            Animator handAnim = rightHand.GetComponent<Animator>();
            if (handAnim != null)
            {
                handAnim.SetTrigger(useSword ? "RightSword" : "RightProjectile");
            }
        }
    }

    // 進入Tired狀態，召喚Core
    public void EnterTiredState()
    {
        if (bossCore != null)
        {
            bossCore.transform.position = coreSpawnPosition.position;
            bossCore.gameObject.SetActive(true);
            bossCore.ResetCore();
        }

        // 開始生成移動平台
        if (platformSpawnCoroutine != null)
            StopCoroutine(platformSpawnCoroutine);
        platformSpawnCoroutine = StartCoroutine(SpawnPlatformsRoutine());

        // 設置計時器
        Invoke(nameof(ExitTiredState), coreAppearDuration);
    }

    // 退出Tired狀態
    public void ExitTiredState()
    {
        // 停止生成平台
        if (platformSpawnCoroutine != null)
        {
            StopCoroutine(platformSpawnCoroutine);
            platformSpawnCoroutine = null;
        }

        // 隱藏Core
        if (bossCore != null && bossCore.gameObject.activeSelf)
        {
            bossCore.gameObject.SetActive(false);
            // 如果Core沒被破壞，進入Stunned
            stateMachine.ChangeState(stunnedState);
        }
    }

    // Core被破壞時調用
    public void OnCoreDestroyed()
    {
        CancelInvoke(nameof(ExitTiredState));

        // 停止生成平台
        if (platformSpawnCoroutine != null)
        {
            StopCoroutine(platformSpawnCoroutine);
            platformSpawnCoroutine = null;
        }

        // 扣除一半血量
        TakeDamage(maxHealth / 2);

        // 隱藏Core
        if (bossCore != null)
            bossCore.gameObject.SetActive(false);

        // 檢查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            stateMachine.ChangeState(battleState);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // 可以在這裡添加受傷特效
        Debug.Log($"Boss HP: {currentHealth}/{maxHealth}");
    }

    // 生成移動平台
    private IEnumerator SpawnPlatformsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(platformSpawnInterval);

            // 隨機從左邊或右邊生成
            bool fromLeft = Random.value > 0.5f;
            Transform spawnPoint = fromLeft ? leftSpawnPoint : rightSpawnPoint;

            if (movingPlatformPrefab != null && spawnPoint != null)
            {
                GameObject platform = Instantiate(movingPlatformPrefab, spawnPoint.position, Quaternion.identity);

                // 設置平台移動方向
                BossMovingPlatform platformScript = platform.GetComponent<BossMovingPlatform>();
                if (platformScript != null)
                {
                    platformScript.SetDirection(fromLeft ? Vector2.right : Vector2.left);
                }
            }
        }
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }

    // Boss本體不受傷害
    public override void DamageImpact() { }
    public override void SetupKnockbackDir(Transform _damageDirection) { }
    public override void SlowEntityBy(float a, float b) { }
}