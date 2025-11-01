using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFallAnimationTriggers : MonoBehaviour
{
    private DeathFall enemy => GetComponentInParent<DeathFall>();

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
