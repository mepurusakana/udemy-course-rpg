public class EnemyBossDefeatedState : EnemyState
{
    private EnemyBoss boss;

    public EnemyBossDefeatedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyBoss _boss) 
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.boss = _boss;
    }

    public override void Enter()
    {
        base.Enter();
        
        // 停止所有動作
        boss.SetZeroVelocity();
        
        // 關閉所有碰撞
        boss.cd.enabled = false;
    }

    public override void Update()
    {
        base.Update();
        
        // 在這個狀態下什麼都不做，等待掉落動畫完成
    }

    public override void Exit()
    {
        base.Exit();
    }
}