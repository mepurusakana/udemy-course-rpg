using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

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
    private void SpeicalAttackTrigger()
    {
        enemy.AnimationSpecialAttackTrigger();
    }

    private void OpenCounterWindow() => enemy.OpenCounterAttackWindow();
    private void CloseCounterWindow() => enemy.CloseCounterAttackWindow();

    private void JumpUpTriggerSmooth(float height)
    {
        StartCoroutine(JumpUpSmoothCoroutine(height));
    }

    private IEnumerator JumpUpSmoothCoroutine(float height)
    {
        Vector3 startPos = enemy.transform.position;
        Vector3 endPos = startPos + new Vector3(0, height, 0);
        float t = 0f;
        float duration = 0.15f; // 調整上升時間

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            enemy.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}
