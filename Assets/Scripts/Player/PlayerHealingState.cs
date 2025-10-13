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


        if (player.chantCharges <= 0)
        {
            stateMachine.ChangeState(player.idleState); // 沒有咏唱次數直接退出
            return;
        }

        player.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        player.rb.velocity = Vector2.zero;

        if (Input.GetKey(KeyCode.Q))
        {
            player.holdTime += Time.deltaTime;

            if (player.holdTime >= player.healHoldTime)
            {
                player.Heal(100);            // 回血量
                player.UseChantCharge();    // 扣掉一格血藥
                stateMachine.ChangeState(player.idleState);
            }
        }
        else
        {
            // 若玩家鬆開 Q，則取消治療
            stateMachine.ChangeState(player.idleState);
        }
    }
}
