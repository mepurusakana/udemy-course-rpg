using System.Collections;
using UnityEngine;

public class PlayerHurtState : PlayerState
{
    private bool hasAppliedKnockback = false;
    private float hurtDuration = 0.35f;       // 僵直時間（秒）
    private float invincibleDuration = 1.2f;  // 無敵總時長
    private Coroutine hurtRoutine;
    private bool isDeadDuringHurt = false;

    public PlayerHurtState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("進入受擊狀態");

        player.isBusy = true;
        hasAppliedKnockback = false;

        player.SetZeroVelocity();
        player.anim.SetTrigger("Hurt");

        // 啟動無敵
        player.stats.MakeInvincible(true);
        player.fx.StartCoroutine("FlashFX");

        ApplyHurtKnockback();

        // 啟動受擊流程
        hurtRoutine = player.StartCoroutine(HurtRoutine());
    }

    public override void Exit()
    {
        base.Exit();

        // 若玩家死亡，不解除控制與無敵，由 GameManager 處理
        if (isDeadDuringHurt)
        {
            Debug.Log("離開受擊狀態（死亡中，不解除控制）");
            return;
        }

        Debug.Log("離開受擊狀態（正常受傷恢復）");

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

        player.rb.velocity = knockbackForce;
        hasAppliedKnockback = true;
        Debug.Log($"玩家受擊反彈 (力道={knockbackForce})");
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtDuration);

        //  判斷玩家是否死亡
        if (player.stats.currentHealth <= 0)
        {
            Debug.Log("玩家在受擊中死亡，進入重生流程");
            isDeadDuringHurt = true;

            // 防止操作、交給 GameManager 處理
            player.isBusy = true;
            player.SetZeroVelocity();
            player.rb.gravityScale = player.defaultGravity;

            GameManager.instance.RespawnPlayer();
            yield break;
        }

        // === 若還活著 ===
        stateMachine.ChangeState(player.airState);

        // 保持無敵一小段時間
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
