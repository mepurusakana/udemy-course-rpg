using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : Enemy
{
    [Header("Boss Settings")]
    public List<EnemyBossCore> cores = new List<EnemyBossCore>(); // 多個核心
    public int bossHealth = 3;


    #region States

    public EnemyBossIdleState idleState { get; private set; }
    public EnemyBossBattleState battleState { get; private set; }
    public EnemyBossAttackState attackState { get; private set; }
    public EnemyBossStunnedState stunnedState { get; private set; }
    public EnemyBossDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyBossIdleState(this, stateMachine, "Idle", this);
        battleState = new EnemyBossBattleState(this, stateMachine, "Idle", this);
        attackState = new EnemyBossAttackState(this, stateMachine, "Attack", this);
        stunnedState = new EnemyBossStunnedState(this, stateMachine, "Stunned", this);
        deadState = new EnemyBossDeadState(this, stateMachine, "Stunned", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    stateMachine.ChangeState(stunnedState);
        //}
    }




    public void CoreDestroyed()
    {
        bossHealth--;

        if (bossHealth <= 0)
            Die();
    }

    // Boss 不受傷
    public override void DamageImpact() { }
    public override void SetupKnockbackDir(Transform _damageDirection) { }
    public override void SlowEntityBy(float a, float b) { }
}
