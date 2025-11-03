using System.Collections;
using UnityEngine;

public class PlayerHurtState : PlayerState
{
    private bool hasAppliedKnockback = false;
    private float hurtDuration = 1.5f;
    private float invincibleDuration = 1.2f;
    private Coroutine hurtRoutine;
    private bool isDeadDuringHurt = false;

    public PlayerHurtState(Player _player, PlayerStateMachine _sm, string _anim)
        : base(_player, _sm, _anim) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("進入受擊狀態");
        hasAppliedKnockback = false;

        player.SetZeroVelocity();

        player.isBusy = true;
        player.stats.MakeInvincible(true);
        player.fx.StartCoroutine("FlashFX");

        ApplyHurtKnockback();

        // 若陷阱流程中不需要退出HurtState，就不自動切換
        hurtRoutine = player.StartCoroutine(HurtRoutine());
    }

    private void ApplyHurtKnockback()
    {
        if (hasAppliedKnockback) return;
        Vector2 knockback = player.GetKnockbackPower();
        if (knockback == Vector2.zero)
            knockback = new Vector2(1f, 2f);
        player.rb.velocity = knockback;
        hasAppliedKnockback = true;
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtDuration);

        // 若血量 > 0 且不忙碌 才轉狀態
        if (player.stats.currentHealth > 0)
            stateMachine.ChangeState(player.airState);
    }

    public override void Exit()
    {
        base.Exit();
        if (isDeadDuringHurt) { Debug.Log("離開受擊狀態（死亡中，不解除控制）"); return; }
        if (hurtRoutine != null) player.StopCoroutine(hurtRoutine);
        player.rb.gravityScale = player.defaultGravity;
        player.isBusy = false;
        player.stats.MakeInvincible(false);
    }

    public override void Update()
    {
        base.Update();
    }
}
