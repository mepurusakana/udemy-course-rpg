using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private SkillData skillData;
    private int skillIndex;

    protected bool lockMovement = true;
    private Coroutine invincibleCoroutine;

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
            lockMovement = true;

            player.SetZeroVelocity();
            player.isBusy = true; // 同步忙碌

            if (skillData.isHitGround)
            {
                invincibleCoroutine = player.StartCoroutine(
                    HitGroundInvincible(skillData.skillDuration)
                );
            }
        }
    }

    public override void Exit()
    {
        base.Exit(); // 設置 Bool false
        player.isBusy = false; // 同步忙碌結束

        if (skillData != null && skillData.isHitGround)
        {
            if (invincibleCoroutine != null)
                player.StopCoroutine(invincibleCoroutine);

            player.stats.MakeInvincible(false);
        }
    }

    public override void Update()
    {
        base.Update();

        if (lockMovement)
            player.SetZeroVelocity(); // 只有鎖定時才清速度

        if (stateTimer < 0 || triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public void UnlockMovement()
    {
        lockMovement = false;
    }

    private IEnumerator HitGroundInvincible(float duration)
    {
        player.stats.MakeInvincible(true);
        yield return new WaitForSeconds(duration);
        player.stats.MakeInvincible(false);
    }
}