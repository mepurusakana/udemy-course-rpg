using UnityEngine;

public class GhostStunnedState : EnemyState
{
    private Ghost enemy;

    public GhostStunnedState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.rb.gravityScale = 2;
        stateTimer = enemy.stunDuration;
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer <= 0)
        {
            enemy.rb.gravityScale = 0;
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
