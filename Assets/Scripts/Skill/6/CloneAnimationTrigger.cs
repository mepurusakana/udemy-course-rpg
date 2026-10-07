using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneAnimationTrigger : MonoBehaviour
{
    private CloneController clone => GetComponentInParent<CloneController>();
    private Transform player;

    public void AnimationTrigger()
    {
        clone.isAttacking = false;
    }

    private void AttackTrigger()
    {
        //AudioManager.instance.PlaySFX(2, null);

        //Collider2D[] colliders = Physics2D.OverlapCircleAll(clone.attackCheck.position, clone.attackRange);

        //foreach (var hit in colliders)
        //{
        //    if (hit.GetComponent<Enemy>() != null)
        //    {
        //        EnemyStats _target = hit.GetComponent<EnemyStats>();

        //        if (_target != null)
        //            _target.TakeDamage(10, this.transform);

        //        //ItemData_Equipment weaponData = Inventory.instance.GetEquipment(EquipmentType.Weapon);

        //        //if (weaponData != null)
        //        //    weaponData.Effect(_target.transform);


        //    }
        //}

        AudioManager.instance.PlaySFX(2, null);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(clone.attackCheck.position, clone.attackRange);

        foreach (var hit in colliders)
        {
            if (hit == null) continue;

            if (hit.GetComponent<Enemy>() != null || hit.GetComponent<BossCore>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                AudioManager.instance.PlaySFX(2, null);

                if (_target != null)
                {
                    _target.TakeDamage(10, this.transform);
                }

                BossCore bossCore = hit.GetComponent<BossCore>();
                if (bossCore != null)
                {
                    bossCore.TakeCoreDamage(15, this.transform);
                }
            }

            if (hit.TryGetComponent(out SpriteShatter2D shatter))
            {
                shatter.Shatter();
            }
        }
    }
    private void ThrowSword()
    {
        //SkillManager.instance.sword.CreateSword();
    }

    public void SlashTrigger()
    {
        //player.SpawnSlashEffect();
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
