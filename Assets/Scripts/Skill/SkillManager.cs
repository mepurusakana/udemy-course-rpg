using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private SkillData[] skills;
    private bool[] isOnCooldown;
    private Animator anim;
    private Player player;
    private Transform skillSpawnPoint;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        player = GetComponent<Player>();
        skillSpawnPoint = transform;
        isOnCooldown = new bool[skills.Length];
    }

    private void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (Input.GetKeyDown(skills[i].activationKey) && !isOnCooldown[i])
            {
                if (CheckSkillCondition(skills[i]))
                {
                    if (skills[i].isSpiritSummon) // 新增的判斷
                    {
                        StartCoroutine(SummonSpirit(skills[i], i));
                    }
                    else
                    {
                        StartCoroutine(ActivateSkill(skills[i], i));
                    }
                }
            }
        }
    }

    private bool CheckSkillCondition(SkillData skill)
    {
        if (skill.requiresAirborne && player.IsGroundDetected()) return false;
        if (skill.requiresGrounded && !player.IsGroundDetected()) return false;
        if (player.rb == null || player.rb.bodyType == RigidbodyType2D.Static) return false;

        return true;
    }

    private IEnumerator SummonSpirit(SkillData skill, int index)
    {
        isOnCooldown[index] = true;

        if (!string.IsNullOrEmpty(skill.animationTriggerName))
            anim.SetTrigger(skill.animationTriggerName);

        yield return new WaitForSeconds(0.5f); // 播放召喚動畫的時間

        GameObject spiritObj = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);
        SpiritController spirit = spiritObj.GetComponent<SpiritController>();
        spirit.Setup(skill.damageAmount, skill.spiritLifeTime, skill.projectilePrefab);

        yield return new WaitForSeconds(skill.cooldown);
        isOnCooldown[index] = false;
    }

    private IEnumerator ActivateSkill(SkillData skill, int index)
    {
        isOnCooldown[index] = true;

        if (!string.IsNullOrEmpty(skill.animationTriggerName))
            anim.SetTrigger(skill.animationTriggerName);

        yield return new WaitForSeconds(0.3f);

        if (skill.isProjectile)
            HandleProjectileSkill(skill);
        else if (skill.requiresAirborne)
            yield return StartCoroutine(HandleAirDropSkill(skill));
        else
            yield return StartCoroutine(HandleNormalSkill(skill));

        yield return new WaitForSeconds(skill.cooldown);
        isOnCooldown[index] = false;
    }

    private IEnumerator HandleNormalSkill(SkillData skill)
    {
        yield return new WaitForSeconds(0.01f);
        GameObject skillObj = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);
        skillObj.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
        skillObj.GetComponent<SkillAttack>().Setup(skill.damageAmount);
        Destroy(skillObj, skill.skillDuration);
    }

    private IEnumerator HandleAirDropSkill(SkillData skill)
    {
        player.rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.01f);
        player.rb.gravityScale = 100f;

        while (!player.IsGroundDetected())
        {
            yield return null;
        }

        anim.SetTrigger("SkillLandImpact");
        player.rb.gravityScale = 5f;

        yield return new WaitForSeconds(0.1f);
        GameObject shockwave = Instantiate(skill.skillPrefab, transform.position, Quaternion.identity);
        shockwave.GetComponent<SkillAttack>().Setup(skill.damageAmount);
        Destroy(shockwave, skill.skillDuration);
    }

    private void HandleProjectileSkill(SkillData skill)
    {
        GameObject projectile = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.Setup(skill.damageAmount, transform.localScale.x);
    }
}
