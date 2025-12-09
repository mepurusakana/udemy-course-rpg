using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        AudioManager.instance.PlayLoopSFX(8);
    }

    public override void Exit()
    {
        base.Exit();
        AudioManager.instance.StopSFX(8);
        player.StopMoveDust();
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);

        // 進入移動狀態 → 播放煙霧
        player.PlayMoveDust();

        //AudioManager.instance.PlaySFX(8, null);

        if (xInput == 0 || player.IsWallDetected())
            stateMachine.ChangeState(player.idleState);
    }
}
