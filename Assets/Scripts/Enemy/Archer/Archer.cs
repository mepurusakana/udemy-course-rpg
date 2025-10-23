using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Enemy
{

    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;
    public float safeDistance;

    #region States

    public ArcherIdleState idleState { get; private set; }
    public ArcherMoveState moveState { get; private set; }
    public ArcherBattleState battleState { get; private set; }
    public ArcherAttackState attackState { get; private set; }
    public ArcherDeadState deadState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        idleState = new ArcherIdleState(this, stateMachine, "Idle", this);
        moveState = new ArcherMoveState(this, stateMachine, "Move", this);
        battleState = new ArcherBattleState(this, stateMachine, "Move", this);
        attackState = new ArcherAttackState(this, stateMachine, "Attack", this);
        //stunnedState = new MummyStunnedState(this, stateMachine, "Stunned", this);
        deadState = new ArcherDeadState(this, stateMachine, "Idle", this);
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
            return true;
        }

        return false;
    }


    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
    public void FireArrow()
    {
        Vector3 spawnPos = arrowSpawnPoint.position;

        if (facingDir == -1)
        {
            spawnPos.x = transform.position.x - Mathf.Abs(arrowSpawnPoint.localPosition.x);
        }

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<Arrow_Controller>().SetDirection(facingDir);

        lastAttackTime = Time.time;
    }
}
