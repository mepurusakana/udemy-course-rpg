using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationTriggers : MonoBehaviour
{
    private EnemyBoss enemy => GetComponentInParent<EnemyBoss>();
    private BossHand hand => GetComponentInParent<BossHand>();

    private void AnimationTrigger()
    {
        enemy.AnimationFinishTrigger();
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackCheck.position, enemy.attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.TryGetComponent(out PlayerStats target))
            {
                enemy.stats.DoDamage(target, enemy.transform);
            }
        }
    }
    private void OnSweepAttackStart()
    {
        hand.OnSweepAttackStart();
    }

    // 在 SweepAttack 動畫結束前調用（關閉橫掃碰撞）
    public void OnSweepAttackEnd()
    {
        hand.OnSweepAttackEnd();
    }

    public void SpawnSweepSmoke()
    {
        hand.SpawnSweepSmoke();
    }

    private void OpenCounterWindow() => enemy.OpenCounterAttackWindow();
    private void CloseCounterWindow() => enemy.CloseCounterAttackWindow();
}
