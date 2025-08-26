using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerDoubleSlashState : PlayerGroundedState
{
    public PlayerDoubleSlashState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.rb.velocity = Vector2.zero;
        stateTimer = 0.5f; // 攻擊動畫時間
    }

    public override void Update()
    {
        base.Update();
        if (triggerCalled) // 動畫事件呼叫時觸發傷害
        {
        }

        if (stateTimer <= 0)
            stateMachine.ChangeState(player.idleState);
    }
}
