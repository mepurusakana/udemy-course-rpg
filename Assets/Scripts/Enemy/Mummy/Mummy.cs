using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mummy : Enemy
{

    #region States

    public MummyIdleState idleState { get; private set; }
    public MummyMoveState moveState { get; private set; }
    public MummyBattleState battleState { get; private set; }
    public MummyAttackState attackState { get; private set; }
    //public MummyStunnedState stunnedState { get; private set; }
    public MummyDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new MummyIdleState(this, stateMachine, "Idle", this);
        moveState = new MummyMoveState(this, stateMachine, "Move", this);
        battleState = new MummyBattleState(this, stateMachine, "Move", this);
        attackState = new MummyAttackState(this, stateMachine, "Attack", this);
        //stunnedState = new MummyStunnedState(this, stateMachine, "Stunned", this);
        deadState = new MummyDeadState(this, stateMachine, "Idle", this);
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



    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
}
