using UnityEngine;

public class EnemyBossIdleState : EnemyState
{
    private EnemyBoss boss;
    private float idleTimer;

    public EnemyBossIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBoss _boss)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = _boss;
    }

    public override void Enter()
    {
        base.Enter();

        // 待機 2-4 秒後進入攻擊狀態
        idleTimer = Random.Range(2f, 4f);

        boss.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            stateMachine.ChangeState(boss.attackState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}