using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private SkillData skillData;
    private int skillIndex;

    public PlayerSkillState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public void SetSkill(SkillData _skillData, int _index)
    {
        skillData = _skillData;
        skillIndex = _index;
    }

    public override void Enter()
    {
        base.Enter(); // 設置 Bool true

        if (skillData != null)
        {
            stateTimer = skillData.skillDuration;
            player.SetZeroVelocity();
            player.isBusy = true; // 同步忙碌
        }
    }

    public override void Exit()
    {
        base.Exit(); // 設置 Bool false
        player.isBusy = false; // 同步忙碌結束
    }

    public override void Update()
    {
        base.Update();

        player.SetZeroVelocity();

        if (stateTimer < 0 || triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}