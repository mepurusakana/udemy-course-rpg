using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    public int comboCounter { get; private set; }

    private float lastTimeAttacked;
    private float comboWindow = 2;

    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 攻擊光效控制
        if (player.attackLight != null)
        {
            player.StopFadeOut();
            player.attackLight.enabled = true;
            player.attackLight.intensity = 0.32f;
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
        else
        {
            // 確保進入攻擊時停下
            player.SetZeroVelocity();
            xInput = 0;
        }


        // Combo 計數重設邏輯
        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
            comboCounter = 0;

        player.anim.SetInteger("ComboCounter", comboCounter);

        // 攻擊方向
        float attackDir = player.facingDir;
        if (xInput != 0)
            attackDir = xInput;

        //  僅使用攻擊定義的推進，不保留原本速度
        Vector2 attackMove = player.attackMovement[comboCounter];
        player.SetVelocity(attackMove.x * attackDir, attackMove.y);

        // 攻擊期間暫停重力
        player.rb.gravityScale = 0f;

        // 攻擊持續時間
        stateTimer = 0.1f;
    }

    public override void Exit()
    {
        base.Exit();

        // 恢復重力
        player.rb.gravityScale = player.defaultGravity; //  建議你在 Player 腳本中定義 public float defaultGravity

        // 淡出攻擊光
        if (player.attackLight != null)
            player.StartFadeOut(0.3f);

        player.StartCoroutine("BusyFor", 0.15f);

        comboCounter++;
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        // 攻擊結束後歸零速度
        if (stateTimer < 0)
            player.SetZeroVelocity();

        if (triggerCalled)
            stateMachine.ChangeState(player.airState);

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
}
