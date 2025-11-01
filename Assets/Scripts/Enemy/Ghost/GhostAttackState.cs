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
        enemy.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        if (!hasShot && triggerCalled)
        {
            hasShot = true;
            ShootProjectile();
        }

        if (hasShot && stateTimer <= 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }

    private void ShootProjectile()
    {
        if (enemy.projectilePrefab == null || enemy.firePoint == null)
            return;

        GameObject projectile = GameObject.Instantiate(enemy.projectilePrefab, enemy.firePoint.position, Quaternion.identity);
        Vector2 dir = (PlayerManager.instance.player.transform.position - enemy.firePoint.position).normalized;
        projectile.GetComponent<Rigidbody2D>().velocity = dir * 6f;
    }
}
