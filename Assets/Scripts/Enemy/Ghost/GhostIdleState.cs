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

        // 若發現玩家 → 進入攻擊或移動狀態
        if (enemy.IsPlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
        else if (stateTimer <= 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}
