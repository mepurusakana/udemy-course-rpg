using UnityEngine;

public class GhostDeadState : EnemyState
{
    private Ghost enemy;

    public GhostDeadState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.cd.enabled = false;
        enemy.rb.gravityScale = 2;
        GameObject.Destroy(enemy.gameObject, 5f);
    }
}
