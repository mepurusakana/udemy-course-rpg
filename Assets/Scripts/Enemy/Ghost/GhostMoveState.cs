using UnityEngine;

public class GhostMoveState : EnemyState
{
    private Ghost enemy;
    private Vector2 moveDirection;

    public GhostMoveState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.battleTime;

        // 決定隨機移動方向
        ChooseRandomDirection();
    }

    public override void Update()
    {
        base.Update();

        // 檢測玩家
        if (enemy.IsPlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        // 在巡邏區域內移動
        enemy.SetVelocity(moveDirection.x * enemy.moveSpeed, moveDirection.y * enemy.moveSpeed);

        // 檢查是否超出巡邏範圍
        if (IsOutOfPatrolRange())
        {
            MoveBackToCenter();
        }

        // 移動時間結束
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }

    private void ChooseRandomDirection()
    {
        // 在圓形區域內選擇隨機方向
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        // 根據移動方向翻轉Ghost
        if (moveDirection.x < 0)
            enemy.Flip();
        else if (moveDirection.x > 0 && enemy.facingDir == -1)
            enemy.Flip();
    }

    private bool IsOutOfPatrolRange()
    {
        Vector2 centerPosition = (enemy.leftPoint.position + enemy.rightPoint.position) / 2f;
        float patrolRadius = Vector2.Distance(enemy.leftPoint.position, enemy.rightPoint.position) / 2f;
        float distanceFromCenter = Vector2.Distance(enemy.transform.position, centerPosition);

        return distanceFromCenter > patrolRadius;
    }

    private void MoveBackToCenter()
    {
        Vector2 centerPosition = (enemy.leftPoint.position + enemy.rightPoint.position) / 2f;
        moveDirection = (centerPosition - (Vector2)enemy.transform.position).normalized;

        if (moveDirection.x < 0 && enemy.facingDir == 1)
            enemy.Flip();
        else if (moveDirection.x > 0 && enemy.facingDir == -1)
            enemy.Flip();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetZeroVelocity();
    }
}