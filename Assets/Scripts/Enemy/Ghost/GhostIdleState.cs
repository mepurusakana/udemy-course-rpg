using UnityEngine;

public class GhostIdleState : EnemyState
{
    private Ghost enemy;

    public GhostIdleState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
        enemy.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        // 檢測玩家
        if (enemy.IsPlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        // Idle時間結束，開始移動
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}