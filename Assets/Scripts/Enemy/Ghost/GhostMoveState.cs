using UnityEngine;

public class GhostMoveState : EnemyState
{
    private Ghost enemy;
    private int moveDir = 1;

    public GhostMoveState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        moveDir = 1;
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(enemy.moveSpeed * moveDir, 0);

        // 巡邏邊界檢查
        if (enemy.transform.position.x >= enemy.rightPoint.position.x)
        {
            moveDir = -1;
            enemy.Flip();
        }
        else if (enemy.transform.position.x <= enemy.leftPoint.position.x)
        {
            moveDir = 1;
            enemy.Flip();
        }

        // 偵測玩家
        if (enemy.IsPlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
    }
}
