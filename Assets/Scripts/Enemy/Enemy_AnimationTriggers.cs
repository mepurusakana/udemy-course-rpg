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

    private void GhostShootProjectile()
    {
        if (enemy is Ghost ghost)
        {
            Player targetPlayer = ghost.player ?? PlayerManager.instance?.player;
            if (targetPlayer == null || ghost.projectilePrefab == null || ghost.firePoint == null)
                return;

            GameObject projectile = Instantiate(ghost.projectilePrefab, ghost.firePoint.position, Quaternion.identity);
            GhostProjectile projectileScript = projectile.GetComponent<GhostProjectile>();

            if (projectileScript != null)
            {
                projectileScript.SetupProjectile(targetPlayer.transform);
            }
        }
    }


    private void SpeicalAttackTrigger()
    {
        enemy.AnimationSpecialAttackTrigger();
    }

    private void OpenCounterWindow() => enemy.OpenCounterAttackWindow();
    private void CloseCounterWindow() => enemy.CloseCounterAttackWindow();

    private void UpTriggerSmooth(float height)
    {
        StartCoroutine(UpSmoothCoroutine(height));
    }

    private void DownTriggerSmooth(float height)
    {
        StartCoroutine(DownSmoothCoroutine(height));
    }

    private IEnumerator UpSmoothCoroutine(float height)
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

    private IEnumerator DownSmoothCoroutine(float height)
    {
        Vector3 startPos = enemy.transform.position;
        Vector3 endPos = startPos - new Vector3(0, height, 0);
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
