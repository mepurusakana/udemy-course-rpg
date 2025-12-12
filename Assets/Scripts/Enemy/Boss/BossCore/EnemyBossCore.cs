using UnityEngine;

public class EnemyBossCore : Enemy
{
    [Header("Core Settings")]
    public EnemyBoss parentBoss; // 父Boss引用
    public int coreHealth = 3; // Core的血量
    private int currentCoreHealth;

    #region States
    public EnemyBossCoreIdleState idleState { get; private set; }
    public EnemyBossCoreDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyBossCoreIdleState(this, stateMachine, "Idle", this);
        deadState = new EnemyBossCoreDeadState(this, stateMachine, "Death", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
        ResetCore();
    }

    public void ResetCore()
    {
        currentCoreHealth = coreHealth;
        isDead = false;

        // 重置碰撞體
        if (cd != null)
            cd.enabled = true;
    }

    public override void OnTakeDamage(Transform attacker)
    {
        base.OnTakeDamage(attacker);

        if (isDead)
            return;

        currentCoreHealth--;

        Debug.Log($"Core HP: {currentCoreHealth}/{coreHealth}");

        // 可以添加受傷特效
        if (fx != null)
            fx.StartBlink(0.1f);

        if (currentCoreHealth <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        if (isDead)
            return;

        base.Die();
        stateMachine.ChangeState(deadState);

        // 通知父Boss
        if (parentBoss != null)
        {
            parentBoss.OnCoreDestroyed();
        }

        Debug.Log("Core被破壞！");
    }

    // Core不會被擊退
    public override void DamageImpact() { }
    public override void SetupKnockbackDir(Transform _damageDirection) { }
}