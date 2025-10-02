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

    private Vector3 GetSpawnPosition(SkillData skill)
    {
        if (skill.spawnPoint != null)
            return skill.spawnPoint.position;

        // 如果沒設定 spawnPoint，預設從玩家本體生成
        return transform.position;
    }




    private bool CheckSkillCondition(SkillData skill)
    {
        if (skill.requiresAirborne && player.IsGroundDetected()) return false;
        if (skill.requiresGrounded && !player.IsGroundDetected()) return false;
        if (player.rb == null || player.rb.bodyType == RigidbodyType2D.Static) return false;

        return true;
    }


    private void HandleSummonSkill(SkillData skill)
    {
        GameObject summonObj = Instantiate(skill.skillPrefab, transform.position + Vector3.right * player.facingDir, Quaternion.identity);
        summonObj.GetComponent<SummonSpirit>().Setup(player.facingDir);
    }

    public IEnumerator ActivateSkill(SkillData skill, int index)
    {
        isOnCooldown[index] = true;

        // 播放技能動畫（前搖）
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
        else if (skill.isFlyingSword)
        {
            yield return StartCoroutine(UseFlyingSwordSkill(skill));
        }
        else if (skill.isDimensionGun) //  新增這裡
        {
            yield return StartCoroutine(UseDimensionGunSkill(skill));
        }
        else
        {
            yield return StartCoroutine(HandleNormalSkill(skill));
        }

        // 冷卻結束
        yield return new WaitForSeconds(skill.cooldown);
        isOnCooldown[index] = false;

        if (!string.IsNullOrEmpty(skill.animationBoolName))
            anim.SetBool(skill.animationBoolName, false);
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

        // 根據玩家朝向調整方向
        float direction = player.facingDir; // facingDir = 1 (右) 或 -1 (左)

        // 翻轉投射物外觀
        projectile.transform.localScale = new Vector3(direction, 1, 1);

        // 傳方向給投射物腳本
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.Setup(skill.damageAmount, direction);
    }
    private IEnumerator UseFlyingSwordSkill(SkillData skill)
    {
        if (!string.IsNullOrEmpty(skill.animationBoolName))
            anim.SetBool(skill.animationBoolName, true);

        yield return new WaitForSeconds(0.1f); // 播放攻擊前搖

        GameObject sword = Instantiate(skill.skillPrefab, skillSpawnPoint.position, Quaternion.identity);

        float direction = player.facingDir;

        // 翻轉劍的外觀
        sword.transform.localScale = new Vector3(direction, 1, 1);

        // 傳方向給飛劍腳本
        FlyingSwordController swordController = sword.GetComponent<FlyingSwordController>();
        swordController.Setup(skill.damageAmount, direction);
    }

    private IEnumerator UseDimensionGunSkill(SkillData skill)
    {
        // 生成次元槍，初始是進場狀態
        GameObject gunObj = Instantiate(skill.skillPrefab, GetSpawnPosition(skill), Quaternion.identity);

        float direction = player.facingDir;
        

        SpearController gunController = gunObj.GetComponent<SpearController>();
        gunController.Setup(skill.damageAmount, direction);

        // 等待進場動畫結束再開火
        yield return new WaitUntil(() => gunController.HasFinishedIntro);

        gunController.Fire();

        // 等待結束動畫播放完
        yield return new WaitUntil(() => gunController.HasFinishedOutro);
    }
}
