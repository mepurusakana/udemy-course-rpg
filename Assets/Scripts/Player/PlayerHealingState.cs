using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealingState : PlayerGroundedState
{
    public PlayerHealingState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.holdTime = 0f;
        player.rb.velocity = Vector2.zero;

        // 檢查是否有咏唱次數
        if (player.chantCharges <= 0)
        {
            stateMachine.ChangeState(player.idleState); // 沒有咏唱次數直接退出
            return;
        }

        player.SetZeroVelocity();

        //  啟動治療特效（Light2D 和 Particle System）
        if (player.healingFX != null)
        {
            player.healingFX.StartHealingEffects();
        }
        else
        {
            Debug.LogWarning("PlayerHealingState: healingFX 未設置！");
        }
    }

    public override void Exit()
    {
        base.Exit();

        //  停止治療特效（無論如何都會執行)
        if (player.healingFX != null)
        {
            player.healingFX.StopHealingEffects();
        }
    }

    public override void Update()
    {
        base.Update();

        // 保持玩家靜止
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
            player.rb.velocity = Vector2.zero;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            player.holdTime += Time.deltaTime;

            // 達到治療時間
            if (player.holdTime >= player.healHoldTime)
            {
                player.Heal(100);            // 回血量
                player.UseChantCharge();    // 扣掉一格血藥
                stateMachine.ChangeState(player.idleState);
                // Exit() 會自動調用 StopHealingEffects()
            }
        }
        else
        {
            // 若玩家鬆開 Q，則取消治療
            stateMachine.ChangeState(player.idleState);
            // Exit() 會自動調用 StopHealingEffects()
        }
    }
}