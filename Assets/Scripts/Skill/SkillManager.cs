using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private bool[] isOnCooldown;
    private Animator anim;
    private Player player;
    private Transform skillSpawnPoint;

    public SkillData[] skills; // 保持 public 給 Player 用
    public int SkillCount => skills.Length;

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
                    player.stateMachine.ChangeState(player.skillStates[i]);
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

    private IEnumerator UseFlyingSwordSkill(SkillData skill, int index)
    {
        isOnCooldown[index] = true;

        if (!string.IsNullOrEmpty(skill.animationBoolName))
            anim.SetBool(skill.animationBoolName, true);

        yield return new WaitForSeconds(0.2f); // 播放攻擊前搖

        GameObject sword = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);
        sword.transform.localScale = new Vector3(transform.localScale.x, 1, 1); // 翻轉
        FlyingSwordController swordController = sword.GetComponent<FlyingSwordController>();
        swordController.Setup(skill.damageAmount, transform.localScale.x);

        anim.SetBool(skill.animationBoolName, false);

        yield return new WaitForSeconds(skill.cooldown);
        isOnCooldown[index] = false;
    }

    private void HandleSummonSkill(SkillData skill)
    {
        GameObject summonObj = Instantiate(skill.skillPrefab, transform.position + Vector3.right * player.facingDir, Quaternion.identity);
        summonObj.GetComponent<SummonSpirit>().Setup(player.facingDir);
    }

    public IEnumerator ActivateSkill(SkillData skill, int index)
    {
        isOnCooldown[index] = true;

        if (!string.IsNullOrEmpty(skill.animationBoolName))
            anim.SetBool(skill.animationBoolName, true);

        yield return new WaitForSeconds(0.3f);

        if (skill.isSummon)
        {
            HandleSummonSkill(skill);
        }
        else if (skill.isProjectile)
        {
            HandleProjectileSkill(skill);
        }
        else if (skill.requiresAirborne)
        {
            yield return StartCoroutine(HandleAirDropSkill(skill));
        }
        else
        {
            yield return StartCoroutine(HandleNormalSkill(skill));
        }

        anim.SetBool(skill.animationBoolName, false);
        anim.SetBool("Idle", true);
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

        anim.SetBool("SkillLandImpact", true);
        player.rb.gravityScale = 5f;

        yield return new WaitForSeconds(0.1f);
        GameObject shockwave = Instantiate(skill.skillPrefab, transform.position, Quaternion.identity);
        shockwave.GetComponent<SkillAttack>().Setup(skill.damageAmount);
        Destroy(shockwave, skill.skillDuration);
        anim.SetBool("SkillLandImpact", false);
    }

    private void HandleProjectileSkill(SkillData skill)
    {
        GameObject projectile = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.Setup(skill.damageAmount, transform.localScale.x);
    }
}
