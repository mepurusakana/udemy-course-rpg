using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private SkillData skill;
    private int skillIndex;
    private SkillManager skillManager;

    public PlayerSkillState(Player player, PlayerStateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName)
    {
        skillManager = player.GetComponent<SkillManager>();
    }

    public void SetSkill(SkillData skill, int index)
    {
        this.skill = skill;
        this.skillIndex = index;
    }

    public override void Enter()
    {
        base.Enter();

        // 播放動畫
        player.anim.SetBool(skill.animationBoolName, true);

        // 啟動協程 → 動畫播完後回 Idle
        player.StartCoroutine(SkillRoutine());
    }

    private IEnumerator SkillRoutine()
    {
        // 啟動技能效果
        yield return player.StartCoroutine(skillManager.ActivateSkill(skill, skillIndex));

        // 技能播完 → 切回 Idle，由狀態機控制
        stateMachine.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        // 不要在 Exit 裡切換狀態
        base.Exit();
        player.anim.SetBool(skill.animationBoolName, false);
    }
}
