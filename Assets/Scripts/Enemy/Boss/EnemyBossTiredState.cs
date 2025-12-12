using UnityEngine;

public class EnemyBossTiredState : EnemyState
{
    private EnemyBoss enemy;

    public EnemyBossTiredState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBoss _enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetZeroVelocity();

        // 召喚Core和平台
        enemy.EnterTiredState();

        Debug.Log("Boss進入Tired狀態！");
    }

    public override void Update()
    {
        base.Update();

        // 保持靜止
        enemy.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Boss退出Tired狀態");
    }
}