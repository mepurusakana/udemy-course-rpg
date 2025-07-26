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

        if (triggerCalled) // 放在Animation最後
        {
            player.stats.IncreaseHealthBy(30); // 回復 30 HP
            player.UseChantCharge(); // 消耗一次咏唱次數
            stateMachine.ChangeState(player.idleState);
        }
    }
}
