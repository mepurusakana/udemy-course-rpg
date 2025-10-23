using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();
    private SkillManager skillManager => GetComponentInParent<SkillManager>();

    public void AnimationTrigger()
    {
        player.AnimationTrigger();
    }

    private void AttackTrigger()
    {
        AudioManager.instance.PlaySFX(2, null);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                if (_target != null)
                    player.stats.DoDamage(_target);
            }
        }
    }

    /// <summary>
    /// </summary>
    public void SkillEffectTrigger()
    {
        if (skillManager != null)
        {
            skillManager.ExecutePendingSkillEffect();
        }
    }

    /// <summary>
    /// </summary>
    public void HitGroundSkillTrigger()
    {
        if (skillManager != null)
        {
            skillManager.ExecutePendingSkillEffect();
        }
    }

    public void ProjectileSkillTrigger()
    {
        if (skillManager != null)
        {
            skillManager.ExecutePendingSkillEffect();
        }
    }

    public void SummonSkillTrigger()
    {
        if (skillManager != null)
        {
            skillManager.ExecutePendingSkillEffect();
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    // ==========  ========== //

    /// <summary>
    /// </summary>
    public void CloseFlash()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.CloseFlash(0.3f); // ©µ¿ð 0.3 ¬íÃö³¬
        }
    }

    public void TriggerBlackFlash()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.BlackFlash(1f);
        }
    }

    /// <summary>
    /// </summary>
    public void TriggerWhiteFlash()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.WhiteFlash(1f);
        }
    }

    /// <summary>
    /// </summary>
    public void TriggerBlackFade()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.BlackFade(0.2f, 0.1f, 0.3f);
        }
    }

    /// <summary>
    /// </summary>
    public void TriggerWhiteFade()
    {
        if (ScreenFlashController.instance != null)
        {
            ScreenFlashController.instance.WhiteFade(0.2f, 0.1f, 0.3f);
        }
    }
}