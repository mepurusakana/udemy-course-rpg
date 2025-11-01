using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFall : Enemy
{

    #region States

    public DeathFallIdleState idleState { get; private set; }
    public DeathFallMoveState moveState { get; private set; }
    public DeathFallBattleState battleState { get; private set; }
    public DeathFallAttackState attackState { get; private set; }
    public DeathFallStunnedState stunnedState { get; private set; }
    public DeathFallDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new DeathFallIdleState(this, stateMachine, "Idle", this);
        moveState = new DeathFallMoveState(this, stateMachine, "Move", this);
        battleState = new DeathFallBattleState(this, stateMachine, "Move", this);
        attackState = new DeathFallAttackState(this, stateMachine, "Attack", this);
        stunnedState = new DeathFallStunnedState(this, stateMachine, "Stunned", this);
        deadState = new DeathFallDeadState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    stateMachine.ChangeState(stunnedState);
        //}
    }

    public override bool CanBeStunned()
    {
        if (base.CanBeStunned())
        {
            //stateMachine.ChangeState(stunnedState);
            return true;
        }

        return false;
    }


    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
}
