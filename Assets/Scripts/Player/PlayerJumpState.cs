using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //player.SetVelocity(rb.velocity.x, player.jumpForce);
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
        //if (rb.velocity.y < 0)
        //stateMachine.ChangeState(player.airState);
    }
}
