using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.airJumpCount = 0;
        player.airDashCount = 0; //  登地時重置空中衝刺次數
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse1) && !player.isBusy)
            stateMachine.ChangeState(player.primaryAttack);

        if (!player.IsGroundDetected())
            stateMachine.ChangeState(player.airState);

        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected() && !player.isBusy)
            stateMachine.ChangeState(player.jumpState);

        if (Input.GetKeyDown(KeyCode.LeftShift) && !player.isBusy)
            stateMachine.ChangeState(player.dashState);

        if (Input.GetKeyDown(KeyCode.Q) && player.chantCharges > 0 && !player.isBusy)
            stateMachine.ChangeState(player.healingState);

        if (Input.GetKey(KeyCode.S) && !player.isBusy /*&& Input.GetKeyDown(KeyCode.Space)*/) // ↓ + 空白
        {
            // 檢查是不是在木板上
            Collider2D hit = Physics2D.OverlapCircle(player.groundCheck.position, 0.1f, player.whatIsGround);
            if (hit != null)
            {
                DropThroughPlatform platform = hit.GetComponent<DropThroughPlatform>();
                if (platform != null)
                {
                    platform.DisableCollisionTemporarily(player.cd); // player.cd 是 CapsuleCollider2D
                }
            }
        }




        if (player.IsGroundDetected() && player.rb.velocity.y <= 0.1f)
        {
            Collider2D hit = Physics2D.OverlapCircle(player.groundCheck.position, 0.1f, player.whatIsGround);
            if (hit != null)
            {
                MovingPlatform platform = hit.GetComponent<MovingPlatform>();
                if (platform != null)
                {
                    Vector2 newVelocity = player.rb.velocity + platform.CurrentVelocity;

                    float maxSpeed = 5f;//The maximum speed you want to limit

                    newVelocity = Vector2.ClampMagnitude(newVelocity, maxSpeed);

                    if (platform.waitTimer > 0)
                    {
                        player.rb.velocity = Vector2.zero;
                    }
                    else
                        player.rb.velocity = newVelocity;

                }
            }
        }
    }

    private bool HasNoSword()
    {
        if (!player.sword)
        {
            return true;
        }

        //player.sword.GetComponent<Sword_Skill_Controller>().ReturnSword();
        return false;
    }

}
