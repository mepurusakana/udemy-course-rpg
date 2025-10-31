using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private float jumpTimeCounter; // 控制持續跳的時間
    private bool isJumping;        // 是否還在持續跳躍

    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    { }

    public override void Enter()
    {
        base.Enter();
        

        player.SetVelocity(rb.velocity.x, player.jumpForce);
        jumpTimeCounter = player.jumpTimer;
        isJumping = true;
    }

    public override void Exit()
    {
        base.Exit();
        isJumping = false;
    }

    public override void Update()
    {
        base.Update();

        //  空中攻擊
        if (Input.GetKeyDown(KeyCode.Mouse0) && !player.isBusy)
        {
            stateMachine.ChangeState(player.primaryAttack);
        }

        //  空中衝刺
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            Time.time >= player.lastDashTime + player.dashCooldown &&
            player.airDashCount < player.maxAirDashes)
        {
            stateMachine.ChangeState(player.dashState);
            return;
        }

        // 若持續按著跳躍鍵且仍可繼續跳
        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                // 持續給予較弱的上升力，模擬「長按跳得更高」
                player.SetVelocity(rb.velocity.x, player.jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                // 到時間了 -> 結束跳躍
                isJumping = false;
                stateMachine.ChangeState(player.airState);
            }
        }

        // 一旦放開空白鍵，就立即結束跳躍
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            stateMachine.ChangeState(player.airState);

            // 強制降低上升速度，增加「放開鍵即下落」的靈敏度
            if (rb.velocity.y > 0)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // 當上升結束（自然頂點）也轉入空中狀態
        if (rb.velocity.y <= 0)
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
