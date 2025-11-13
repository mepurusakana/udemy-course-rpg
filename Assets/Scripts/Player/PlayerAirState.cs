using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 重力
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (player.fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (player.lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //  空中攻擊
        if (Input.GetKeyDown(KeyCode.Mouse1) && !player.isBusy)
        {
            stateMachine.ChangeState(player.primaryAttack);
        }

        //  空中衝刺
        if (Input.GetKeyDown(KeyCode.LeftShift) && 
            Time.time >= player.lastDashTime + player.dashCooldown &&
            player.airDashCount < player.maxAirDashes)
        {
            stateMachine.ChangeState(player.dashState);
            return;
        }

        //  二段跳
        if (Input.GetKeyDown(KeyCode.Space) && player.airJumpCount < player.maxAirJumps)
        {
            player.airJumpCount++;
            stateMachine.ChangeState(player.jumpState);
        }

        if (player.IsGroundDetected())
        {
            player.airJumpCount = 0;
            player.airDashCount = 0; // 登地重置空中衝刺次數
            stateMachine.ChangeState(player.idleState);
        }

        if (xInput != 0)
            player.SetVelocity(player.moveSpeed * .8f * xInput, rb.velocity.y);
    }

    //  二段跳邏輯封裝
    private void DoDoubleJump()
    {
        player.jumpTimer = 0.2f;
        player.SetVelocity(rb.velocity.x, player.jumpForce); // 再次施加跳躍力
        //player.fx.CreateJumpEffect(); // 若你有 PlayerFX，可播放跳躍特效
        if (player.doubleJumpVFX != null)
            player.doubleJumpVFX.Play(); // 播放額外特效
    }


}
