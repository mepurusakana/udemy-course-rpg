using UnityEngine;

public class EnemyBossTiredState : EnemyState
{
    private EnemyBoss boss;
    private float tiredTimer;

    public EnemyBossTiredState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBoss _boss)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = _boss;
    }

    public override void Enter()
    {
        base.Enter();

        tiredTimer = boss.tiredDuration;

        // Boss 停止所有行動
        boss.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        tiredTimer -= Time.deltaTime;

        if (tiredTimer <= 0)
        {
            boss.ExitTiredState();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}