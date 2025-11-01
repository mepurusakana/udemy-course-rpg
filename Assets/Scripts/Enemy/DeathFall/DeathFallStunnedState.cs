using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DeathFallStunnedState : EnemyState
{
    private DeathFall enemy;
    private Transform player;

    public DeathFallStunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, DeathFall _enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.instance.player.transform;

        // 面向玩家
        int playerDir = player.position.x > enemy.transform.position.x ? 1 : -1;
        if (enemy.facingDir != playerDir)
            enemy.Flip();

        // 開始閃爍
        enemy.fx.InvokeRepeating("RedColorBlink", 0, 0.1f);

        stateTimer = enemy.stunDuration;

        // 反方向擊退
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.fx.Invoke("CancelColorChange", 0);
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.battleState);
    }
}
