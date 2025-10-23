using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("技能列表")]
    public List<SkillData> skills = new List<SkillData>(); // 改為 public，讓 Player 可以訪問

    private Dictionary<KeyCode, float> cooldownTimers = new Dictionary<KeyCode, float>();
    private Player player;
    //private CloneController clone;
    private GameObject currentSpirit; // 當前召喚的精靈
    private GameObject currentClone; //當前召喚的分身

    private SkillData pendingSkill;
    

    // 添加屬性讓 Player 可以訪問技能數量
    public int SkillCount => skills.Count;

    private void Start()
    {
        player = GetComponent<Player>();

        // 初始化冷卻計時器
        foreach (SkillData skill in skills)
        {
            if (skill != null)
            {
                cooldownTimers[skill.activationKey] = 0f;
            }
        }
    }

    private void Update()
    {
        // 更新所有冷卻計時器
        List<KeyCode> keys = new List<KeyCode>(cooldownTimers.Keys);
        foreach (KeyCode key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
            }
        }

        // 檢查技能輸入
        foreach (SkillData skill in skills)
        {
            if (skill != null && Input.GetKeyDown(skill.activationKey))
            {
                TryUseSkill(skill);
            }
        }
    }

    private void TryUseSkill(SkillData skill)
    {
        // 檢查冷卻時間
        if (cooldownTimers.ContainsKey(skill.activationKey) && cooldownTimers[skill.activationKey] > 0)
        {
            Debug.Log($"{skill.skillName} 還在冷卻中！剩餘: {cooldownTimers[skill.activationKey]:F1}秒");
            return;
        }

        // 檢查使用條件
        if (skill.requiresGrounded && !player.IsGroundDetected())
        {
            Debug.Log($"{skill.skillName} 需要在地面上使用！");
            return;
        }

        if (skill.requiresAirborne && player.IsGroundDetected())
        {
            Debug.Log($"{skill.skillName} 需要在空中使用！");
            return;
        }

        // 檢查是否正在忙碌
        if (player.isBusy)
        {
            Debug.Log("玩家正在執行其他動作！");
            return;
        }

        // 使用技能
        int skillIndex = skills.IndexOf(skill); // 找到 index
        UseSkill(skill, skillIndex); // 傳遞 index
    }

    public void UseSkill(SkillData skill, int skillIndex)
    {
        if (skill == null)
        {
            Debug.LogWarning("技能數據為空！");
            return;
        }

        Debug.Log($"使用技能: {skill.skillName}");

        pendingSkill = skill;

        //// 如果是召喚分身技能
        //if (skill.isClone)
        //{
        //    UseCloneSkill(skill);
        //}
        //// 如果是召喚技能（精靈）
        //else if (skill.isSummon)
        //{
        //    UseSpiritSkill(skill);
        //}
        //// 如果是投射物技能
        //else if (skill.isProjectile)
        //{
        //    UseProjectileSkill(skill);
        //}
        //// 如果是飛劍技能
        //else if (skill.isFlyingSword)
        //{
        //    UseFlyingSwordSkill(skill);
        //}
        //// 如果是次元槍技能
        //else if (skill.isDimensionGun)
        //{
        //    UseDimensionGunSkill(skill);
        //}
        //// 一般技能
        //else 
        //{
        //    UseHitGroundSkill(skill);
        //}


        // 設置冷卻時間
        if (cooldownTimers.ContainsKey(skill.activationKey))
        {
            cooldownTimers[skill.activationKey] = skill.cooldown;
        }
        else
        {
            cooldownTimers.Add(skill.activationKey, skill.cooldown);
        }

        if (player.skillStates != null && skillIndex >= 0 && skillIndex < player.skillStates.Length)
        {
            player.stateMachine.ChangeState(player.skillStates[skillIndex]);
        }
    }

    /// <summary>
    /// 在動畫事件中調用此方法來執行技能效果
    /// </summary>
    public void ExecutePendingSkillEffect()
    {
        if (pendingSkill == null)
        {
            Debug.LogWarning("沒有待執行的技能！");
            return;
        }

        Debug.Log($"執行技能效果: {pendingSkill.skillName}");

        // 根據技能類型執行相應效果
        if (pendingSkill.isClone)
        {
            ExecuteCloneSkill(pendingSkill);
        }
        else if (pendingSkill.isSummon)
        {
            ExecuteSpiritSkill(pendingSkill);
        }
        else if (pendingSkill.isProjectile)
        {
            ExecuteProjectileSkill(pendingSkill);
        }
        else if (pendingSkill.isFlyingSword)
        {
            ExecuteFlyingSwordSkill(pendingSkill);
        }
        else if (pendingSkill.isDimensionGun)
        {
            ExecuteDimensionGunSkill(pendingSkill);
        }
        else
        {
            ExecuteHitGroundSkill(pendingSkill);
        }

        // 清除待執行技能
        pendingSkill = null;
    }



    private IEnumerator SummonCloneDelayed(SkillData skill, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill.skillPrefab != null)
        {
            // 根據玩家面向決定召喚位置
            Vector3 spawnOffset = new Vector3(player.facingDir * 1f, 0, 0);
            Vector3 spawnPosition = player.transform.position + spawnOffset;

            // 生成分身
            currentClone = Instantiate(skill.skillPrefab, spawnPosition, Quaternion.identity);

            // 獲取分身控制器
            CloneController cloneController = currentClone.GetComponent<CloneController>();
            if (cloneController != null)
            {
                // 將分身面向與玩家一致
                if (player.facingDir == -1)
                {
                    cloneController.SetFacingDirection(-1);
                }
                else
                {
                    cloneController.SetFacingDirection(1);
                }
            }

            Debug.Log($"分身已召喚，方向: {(player.facingDir == 1 ? "右" : "左")}");
        }
    }
    private IEnumerator SummonSpiritDelayed(SkillData skill, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill.skillPrefab != null)
        {
            Vector3 spawnPosition = player.transform.position + new Vector3(player.facingDir * 2f, 0.5f, 0);
            currentSpirit = Instantiate(skill.skillPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("精靈已召喚");
        }
    }



    //=========================================//



    private void ExecuteCloneSkill(SkillData skill)
    {
        if (currentClone != null)
        {
            // 如果已經有分身存在，則消除分身
            CloneController cloneController = currentClone.GetComponent<CloneController>();
            if (cloneController != null)
            {
                cloneController.DismissClone();
            }
            currentClone = null;
            return;
        }

            // 召喚新的分身
        StartCoroutine(SummonCloneDelayed(skill, 0.5f));
    }

    private void ExecuteSpiritSkill(SkillData skill)
    {
        if (currentSpirit != null)
        {
            SpiritController spiritController = currentSpirit.GetComponent<SpiritController>();
            if (spiritController != null)
            {
                spiritController.DismissSpirit();
            }
            currentSpirit = null;
            Debug.Log("精靈已消失");
            return;
        }

        // *** 移除 SetTrigger，讓 PlayerSkillState 處理（或如果召喚需播放動畫，在這裡啟動 Coroutine 播放 Trigger）***
        // 但為了統一，建議添加一個 SummonState 繼承 PlayerSkillState，並在 Animator 用 Trigger。
        //player.anim.SetBool(skill.animationBoolName, true);

        // 延遲一點召喚，讓玩家動畫能跑起來
        StartCoroutine(SummonSpiritDelayed(skill, 0.5f));
    }


    private void ExecuteProjectileSkill(SkillData skill)
    {
        // 生成投射物
        if (skill.skillPrefab != null)
        {
            Vector3 spawnPos = skill.spawnPoint != null ? skill.spawnPoint.position : player.transform.position;
            GameObject projectile = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

            // 設置投射物方向
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(player.facingDir * 10f, 10f);
            }

            // 設置投射物的傷害
            SkillController controller = projectile.GetComponent<SkillController>();
            if (controller != null)
            {
                controller.Setup(skill.damageAmount);
            }
        }

        // 設置忙碌狀態
        player.StartCoroutine("BusyFor", 0.3f);
    }

    private void ExecuteFlyingSwordSkill(SkillData skill)
    {
        // 生成飛劍
        if (skill.skillPrefab != null)
        {
            Vector3 spawnPos = player.transform.position + new Vector3(player.facingDir * 1f, 0);
            GameObject sword = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

            // 設置飛劍的傷害
            FlyingSwordController controller = sword.GetComponent<FlyingSwordController>();
            if (controller != null)
            {
                controller.Setup(skill.damageAmount, player.facingDir);
            }
        }

        player.StartCoroutine("BusyFor", 0.5f);
    }

    private void ExecuteDimensionGunSkill(SkillData skill)
    {
        // 生成次元槍效果
        if (skill.skillPrefab != null)
        {
            Vector3 spawnPos = player.transform.position + new Vector3(player.facingDir * 2f, 0, 0);
            GameObject effect = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

            // 設置傷害
            SpearController controller = effect.GetComponent<SpearController>();
            if (controller != null)
            {
                controller.Setup(new Vector2(player.facingDir, 0), skill.damageAmount);
            }
        }

        player.StartCoroutine("BusyFor", 0.4f);
    }

    private void ExecuteHitGroundSkill(SkillData skill)
    {
        // 生成技能效果
        if (skill.skillPrefab != null)
        {
            Vector3 spawnPos = player.transform.position;
            GameObject effect = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

            // 設置傷害
            SkillController controller = effect.GetComponent<SkillController>();
            if (controller != null)
            {
                controller.Setup(skill.damageAmount);
            }
        }

        player.StartCoroutine("BusyFor", skill.skillDuration);
    }





    //===================//





    // 獲取技能冷卻剩餘時間（用於UI顯示）
    public float GetCooldownRemaining(KeyCode key)
    {
        if (cooldownTimers.ContainsKey(key))
        {
            return Mathf.Max(0, cooldownTimers[key]);
        }
        return 0f;
    }

    // 檢查技能是否可用
    public bool IsSkillReady(KeyCode key)
    {
        if (cooldownTimers.ContainsKey(key))
        {
            return cooldownTimers[key] <= 0;
        }
        return false;
    }

    // 清除當前精靈的引用（當精靈被其他方式銷毀時）
    public void ClearSpiritReference()
    {
        currentSpirit = null;
    }

    public void ClearCloneReference()
    { 
        currentClone = null; 
    }

    // 檢查是否有精靈存在
    public bool HasActiveSpirit()
    {
        return currentSpirit != null;
    }

    public bool HasActiveClone()
    {
        return currentClone != null; 
    }
}