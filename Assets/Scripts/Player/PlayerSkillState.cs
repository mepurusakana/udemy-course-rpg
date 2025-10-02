using System.Collections;
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
        // base.Enter() already sets the animator bool for this state's animBoolName

        // 啟動技能效果 (SkillManager 負責生成 prefab / cooldown / effect)
        if (skillManager != null)
            player.StartCoroutine(skillManager.ActivateSkill(skill, skillIndex));
    }

    public override void Update()
    {
        base.Update();

        // 可視需要讓角色在技能期間不能移動（確保不會被外部移動）
        player.SetZeroVelocity();

        // 等待動畫事件把 triggerCalled 設成 true（由 AnimationTrigger() 觸發）
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // base.Exit() 會把該狀態綁定的 animator bool 設為 false
    }
}
