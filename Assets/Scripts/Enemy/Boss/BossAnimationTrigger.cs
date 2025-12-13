using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationTriggers : MonoBehaviour
{
    private EnemyBoss enemy => GetComponentInParent<EnemyBoss>();

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

    private void OpenCounterWindow() => enemy.OpenCounterAttackWindow();
    private void CloseCounterWindow() => enemy.CloseCounterAttackWindow();
}
