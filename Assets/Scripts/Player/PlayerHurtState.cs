using UnityEngine;

public class PlayerHurtState : PlayerState
{
    private bool isGrounded;
    private bool hasAppliedKnockback = false;

    public PlayerHurtState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("進入受擊狀態");

        // 設定狀態計時器（受擊僵直時間）
        stateTimer = 0.4f;

        // 重置標記
        hasAppliedKnockback = false;

        // 禁止玩家操作
        player.isBusy = true;

        // 立即應用受擊回彈力道
        ApplyHurtKnockback();
    }

    public override void Exit()
    {
        base.Exit();

        Debug.Log("離開受擊狀態");

        // 恢復玩家操作
        player.isBusy = false;

        // 確保重力恢復正常
        if (player.rb.gravityScale == 0)
        {
            player.rb.gravityScale = 1;
        }
    }

    public override void Update()
    {
        base.Update();

        // 檢測是否落地
        isGrounded = player.IsGroundDetected();

        player.rb.gravityScale = 0;
        // 如果計時器結束且已著地，回到 Idle 狀態
        //if (stateTimer < 0 && isGrounded)
        //{
        //    stateMachine.ChangeState(player.idleState);
        //}

        // 在空中時允許玩家微調水平方向（可選，模擬空中控制）
        // 如果不想要空中控制，可以註解掉下面這段
        /*
        if (!isGrounded && xInput != 0)
        {
            player.SetVelocity(player.moveSpeed * 0.2f * xInput, rb.velocity.y);
        }
        */
    }

    /// <summary>
    /// 應用受擊回彈力道（朝向相反 + 向上）
    /// </summary>
    private void ApplyHurtKnockback()
    {
        if (hasAppliedKnockback) return;

        // 獲取已設定的擊退力道
        Vector2 knockbackForce = player.GetKnockbackPower();

        // 如果沒有設定擊退力道，使用預設值
        if (knockbackForce == Vector2.zero)
        {
            knockbackForce = new Vector2(8f, 12f);
        }

        // 計算擊退方向（朝向相反）
        int knockbackDirection = player.knockbackDir;

        // 如果沒有設定擊退方向，則根據玩家朝向反向擊退
        if (knockbackDirection == 0)
        {
            knockbackDirection = -player.facingDir;
        }

        // 應用擊退力（X軸朝向相反，Y軸向上）
        rb.velocity = new Vector2(knockbackForce.x * knockbackDirection, knockbackForce.y);

        Debug.Log($"受擊回彈：方向={knockbackDirection}, 力道={knockbackForce}");

        hasAppliedKnockback = true;
    }
}