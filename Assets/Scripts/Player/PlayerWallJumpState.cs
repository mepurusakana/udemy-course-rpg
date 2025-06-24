using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 1f;
        player.SetVelocity(5 * -player.facingDir, player.jumpForce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKey(KeyCode.Space))
        {
            if (player.jumpTimer > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, player.jumpForce);
                player.jumpTimer -= Time.deltaTime;
            }
            else
            {
                stateMachine.ChangeState(player.airState);
                player.jumpTimer = 0.1f;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            player.jumpTimer = 0f;
            stateMachine.ChangeState(player.airState);
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            //coyoteAirTimer = coyoteTime + 1f; // Prevent coyote double jump bug
            player.jumpTimer = 0.1f;
        }

        //if (stateTimer < 0)
        //    stateMachine.ChangeState(player.airState);

        //if (player.IsGroundDetected())
        //    stateMachine.ChangeState(player.idleState);
    }
}
