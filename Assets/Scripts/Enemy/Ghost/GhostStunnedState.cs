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

        // 開啟重力（讓Ghost掉落）
        enemy.rb.gravityScale = 0f;

        // 設定眩暈時間
        stateTimer = 0.2f; // 你要求的0.2秒

        // 清除當前速度
        Vector2 knockbackVelocity = new Vector2(enemy.knockbackDir * 8f, 0);
    }

    public override void Update()
    {
        base.Update();

        // 眩暈時間結束
        if (stateTimer <= 0)
        {
            // 恢復無重力狀態
            enemy.rb.gravityScale = 0f;
            enemy.rb.velocity = Vector2.zero;

            stateMachine.ChangeState(enemy.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.rb.velocity = Vector2.zero;
    }
}