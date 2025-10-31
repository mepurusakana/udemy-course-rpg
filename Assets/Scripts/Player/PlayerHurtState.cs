using System.Collections;
using UnityEngine;

public class PlayerHurtState : PlayerState
{
    private bool hasAppliedKnockback = false;
    private float hurtDuration = 0.35f;       // 僵直時間（秒）
    private float invincibleDuration = 1.2f;  // 無敵總時長
    private Coroutine hurtRoutine;

    public PlayerHurtState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log(" 進入受擊狀態");

        player.isBusy = true;
        hasAppliedKnockback = false;

        // 停止玩家移動
        player.SetZeroVelocity();

        // 播放受擊動畫
        player.anim.SetTrigger("Hurt");

        // 啟動無敵
        player.stats.MakeInvincible(true);

        // 開始閃爍
        player.fx.StartCoroutine("FlashFX");

        // 施加反方向擊退
        ApplyHurtKnockback();

        // 啟動受擊流程（僵直+無敵）
        hurtRoutine = player.StartCoroutine(HurtRoutine());
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("離開受擊狀態");

        if (hurtRoutine != null)
            player.StopCoroutine(hurtRoutine);

        player.isBusy = false;
        player.rb.gravityScale = player.defaultGravity;
        player.stats.MakeInvincible(false);
    }

    private void ApplyHurtKnockback()
    {
        if (hasAppliedKnockback) return;

        Vector2 knockbackForce = player.GetKnockbackPower();
        if (knockbackForce == Vector2.zero)
            knockbackForce = new Vector2(8f, 12f);

        int direction = knockbackForce.x >= 0 ? 1 : -1; // 確保X方向有正負值
        player.rb.velocity = knockbackForce;

        hasAppliedKnockback = true;
        Debug.Log($"玩家受擊反彈 (力道={knockbackForce})");
    }

    private IEnumerator HurtRoutine()
    {
        // 等待僵直期間
        yield return new WaitForSeconds(hurtDuration);

        // 切回空中狀態（或 Idle）
        stateMachine.ChangeState(player.airState);

        // 剩餘時間繼續無敵
        yield return new WaitForSeconds(invincibleDuration - hurtDuration);

        player.stats.MakeInvincible(false);
        player.isBusy = false;
    }

    public override void Update()
    {
        base.Update();
        // 不再每幀修改速度或重力，僅在初始擊退時套用一次即可
    }
}
