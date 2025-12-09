using UnityEngine;

public class EnemyBossIdleState : EnemyState
{
    private EnemyBoss enemy;

    public EnemyBossIdleState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, EnemyBoss _enemy)
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
        //if (stateTimer < 0)
        //{
        //    stateMachine.ChangeState(enemy.moveState);
        //}
    }
}