using UnityEngine;

public class PlayerDashState : PlayerState
{
    private bool isAirDash;

    public PlayerDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        //  檢查冷卻
        if (Time.time < player.lastDashTime + player.dashCooldown)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        //  檢查是否在空中
        isAirDash = !player.IsGroundDetected();

        //  空中衝刺次數判定
        if (isAirDash)
        {
            if (player.airDashCount >= player.maxAirDashes)
            {
                stateMachine.ChangeState(player.airState);
                return;
            }
            player.airDashCount++;
        }

        //  記錄冷卻時間
        player.lastDashTime = Time.time;

        //  設定衝刺方向
        float inputX = Input.GetAxisRaw("Horizontal");
        player.dashDir = inputX != 0 ? inputX : player.facingDir;

        stateTimer = player.dashDuration;
        player.stats.MakeInvincible(true);

        //  播放特效或音效
        AudioManager.instance.PlaySFX(3, null); // 假設3是dash音效
        // player.fx.CreateAfterImage();
    }

    public override void Exit()
    {
        base.Exit();

        player.SetVelocity(0, rb.velocity.y);
        player.stats.MakeInvincible(false);
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocity(player.dashSpeed * player.dashDir, 0);

        if (stateTimer < 0)
        {
            // 空中衝刺結束 → 若沒落地，轉回 AirState
            if (isAirDash && !player.IsGroundDetected())
                stateMachine.ChangeState(player.airState);
            else
                stateMachine.ChangeState(player.idleState);
        }
    }
}
