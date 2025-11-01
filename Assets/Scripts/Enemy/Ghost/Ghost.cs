using UnityEngine;

public class Ghost : Enemy
{
    #region States
    public GhostIdleState idleState { get; private set; }
    public GhostMoveState moveState { get; private set; }
    public GhostAttackState attackState { get; private set; }
    public GhostStunnedState stunnedState { get; private set; }
    public GhostDeadState deadState { get; private set; }
    #endregion

    [Header("Flying Range Info")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Projectile Info")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    protected override void Awake()
    {
        base.Awake();

        idleState = new GhostIdleState(this, stateMachine, "Idle", this);
        moveState = new GhostMoveState(this, stateMachine, "Move", this);
        attackState = new GhostAttackState(this, stateMachine, "Attack", this);
        stunnedState = new GhostStunnedState(this, stateMachine, "Stunned", this);
        deadState = new GhostDeadState(this, stateMachine, "Dead", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
}
