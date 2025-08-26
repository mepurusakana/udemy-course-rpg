using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerHitGroundState : PlayerState
{
    private float hitSpeed;

    public PlayerHitGroundState(Player player, PlayerStateMachine stateMachine, string animBoolName, float smashSpeed)
        : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.velocity = new Vector2(0, -hitSpeed); // ±j¨î¤U¼Y
    }

    public override void Update()
    {
        base.Update();
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
