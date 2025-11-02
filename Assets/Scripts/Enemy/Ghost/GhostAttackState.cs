using UnityEngine;

public class GhostAttackState : EnemyState
{
    private Ghost enemy;
    private bool hasShot;

    public GhostAttackState(Enemy _enemyBase, EnemyStateMachine _sm, string _anim, Ghost _enemy)
        : base(_enemyBase, _sm, _anim)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        hasShot = false;
        stateTimer = 2f; // 攻擊後等待時間
        enemy.SetZeroVelocity();

        // 面向玩家
        FacePlayer();
    }

    public override void Update()
    {
        base.Update();

        // 當動畫觸發時發射子彈
        if (!hasShot && triggerCalled)
        {
            hasShot = true;
            ShootProjectile();
        }

        // 攻擊完成後回到Idle
        if (hasShot && stateTimer <= 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }

    private void FacePlayer()
    {
        Player targetPlayer = enemy.player ?? PlayerManager.instance?.player;
        if (targetPlayer == null) return;

        if (targetPlayer.transform.position.x < enemy.transform.position.x && enemy.facingDir == 1)
            enemy.Flip();
        else if (targetPlayer.transform.position.x > enemy.transform.position.x && enemy.facingDir == -1)
            enemy.Flip();
    }

    private void ShootProjectile()
    {
        if (enemy.projectilePrefab == null || enemy.firePoint == null)
            return;

        Player targetPlayer = enemy.player ?? PlayerManager.instance?.player;
        if (targetPlayer == null)
            return;

        GameObject projectile = GameObject.Instantiate(
            enemy.projectilePrefab,
            enemy.firePoint.position,
            Quaternion.identity
        );

        GhostProjectile projectileScript = projectile.GetComponent<GhostProjectile>();
        if (projectileScript != null)
        {
            // 傳入 Transform 而不是 Player（確保和 SetupProjectile 的簽名相符）
            projectileScript.SetupProjectile(targetPlayer.transform);
        }
    }
}
