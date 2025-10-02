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

        AudioManager.instance.PlaySFX(8, null);
    }

    public override void Exit()
    {
        base.Exit();

        AudioManager.instance.StopSFX(8);

        // Â÷¶}²¾°Êª¬ºA ¡÷ °±¤î·ÏÃú
        player.StopMoveDust();
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);

        // ¶i¤J²¾°Êª¬ºA ¡÷ ¼½©ñ·ÏÃú
        player.PlayMoveDust();


        if (xInput == 0 || player.IsWallDetected())
            stateMachine.ChangeState(player.idleState);
    }
}
