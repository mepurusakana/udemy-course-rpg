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
        AudioManager.instance.PlaySFX(2, null);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(clone.attackCheck.position, clone.attackRange);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                if (_target != null)
                    _target.TakeDamage(10, this.transform);

                //ItemData_Equipment weaponData = Inventory.instance.GetEquipment(EquipmentType.Weapon);

                //if (weaponData != null)
                //    weaponData.Effect(_target.transform);


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
