using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFallAttackState : EnemyState
{
    private DeathFall enemy;
    public DeathFallAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, DeathFall _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastAttackTime = Time.time;
    }

    public override void Update()
    {
        base.Update();

        enemy.SetZeroVelocity();

        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
}