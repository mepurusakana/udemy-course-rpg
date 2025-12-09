using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossCoreDeadState : EnemyState
{

    private EnemyBossCore enemy;

    public EnemyBossCoreDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string animBoolName, EnemyBossCore _enemy) : base(_enemyBase, _stateMachine, animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();

        //enemy.anim.SetBool(enemy.lastAnimBoolName, true);
        enemy.anim.SetBool("Idle", true);
        enemy.anim.speed = 0;
        enemy.cd.enabled = false;

        stateTimer = .1f;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer > 0)
            rb.velocity = new Vector2(0, 10);
    }
}
