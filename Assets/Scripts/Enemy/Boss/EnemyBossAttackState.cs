using UnityEngine;

public class EnemyBossAttackState : EnemyState
{
    private EnemyBoss boss;
    private float attackInterval = 1.5f;
    private float nextAttackTime = 0f;

    public EnemyBossAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBoss _boss)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = _boss;
    }

    public override void Enter()
    {
        base.Enter();
        nextAttackTime = 0f;
    }

    public override void Update()
    {
        base.Update();

        boss.SetZeroVelocity();

        // 定期執行攻擊
        if (Time.time >= nextAttackTime)
        {
            boss.PerformRandomAttack();
            nextAttackTime = Time.time + attackInterval;
        }

        // 注意：攻擊時間由 EnemyBoss.Update() 中的 attackTimer 控制
        // 達到 attackDuration 後會自動調用 EnterTiredState()
    }

    public override void Exit()
    {
        base.Exit();
    }
}