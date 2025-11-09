using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("技能列表")]
    public List<SkillData> skills = new List<SkillData>(); // 改為 public，讓 Player 可以訪問

    [Header("統一觸發鍵")]
    public KeyCode unifiedActivationKey = KeyCode.E;

    [Header("當前裝備技能索引（-1 表示未裝備）")]
    [SerializeField] private int selectedIndex = -1;

    private readonly Dictionary<SkillData, float> cooldownTimers = new Dictionary<SkillData, float>();

    private Player player;
    private PlayerStats playerStats;

    //private CloneController clone;
    private GameObject currentSpirit; // 當前召喚的精靈
    private GameObject currentClone; //當前召喚的分身

    private SkillData pendingSkill;

    public int SkillCount => skills.Count;
    public int SelectedIndex => selectedIndex;
    public SkillData SelectedSkill => (selectedIndex >= 0 && selectedIndex < skills.Count) ? skills[selectedIndex] : null;


    private void Start()
    {
        player = GetComponent<Player>();
        playerStats = GetComponent<PlayerStats>();

        // 初始化冷卻計時器
        foreach (var s in skills)
        {
            if (s != null && !cooldownTimers.ContainsKey(s))
                cooldownTimers.Add(s, 0f);
        }
    }

    private void Update()
    {
        if (cooldownTimers.Count > 0)
        {
            var keys = new List<SkillData>(cooldownTimers.Keys);
            foreach (var s in keys)
            {
                if (s == null) continue;
                if (cooldownTimers[s] > 0f)
                    cooldownTimers[s] -= Time.deltaTime;
            }
        }

        // 只監聽 E（或你設定的 unifiedActivationKey），且只嘗試施放「被選中的唯一技能」
        if (Input.GetKeyDown(unifiedActivationKey))
        {
            if (SelectedSkill != null)
                TryUseSkill(SelectedSkill);
            else
                Debug.Log("[SkillManager] 尚未選擇任何技能（selectedIndex = -1）。");
        }
    }


    /// <summary>由 TwoStateButtonGroup 傳入索引。</summary>
    public void SetSelectedIndex(int index)
    {
        if (index >= 0 && index < skills.Count)
            selectedIndex = index;
        else
            selectedIndex = -1; // 支援取消選取（TwoState allowDeselect 時會傳 -1）

        Debug.Log($"[SkillManager] 選中技能 => index={selectedIndex}, name={(SelectedSkill ? SelectedSkill.skillName : "None")}");
    }



    private void TryUseSkill(SkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("技能數據為空！");
            return;
        }

        // 以「技能」檢查冷卻
        if (cooldownTimers.TryGetValue(skill, out float remain) && remain > 0f)
        {
            Debug.Log($"{skill.skillName} 還在冷卻中！剩餘: {remain:F1} 秒");
            return;
        }

        // 使用條件
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

        // 忙碌判斷
        if (player.isBusy)
        {
            Debug.Log("玩家正在執行其他動作！");
            return;
        }

        // MP 檢查與扣除
        if (playerStats.currentMP < skill.mpCost)
        {
            Debug.Log("MP 不足，無法使用技能！");
            ShowMPNotEnoughMessage();
            return;
        }
        playerStats.ConsumeMP(skill.mpCost);

        // 進入技能前置（交由動畫事件呼叫 ExecutePendingSkillEffect 真正生成）
        UseSkill(skill, selectedIndex);
    }


    public void UseSkill(SkillData skill, int skillIndexInList)
    {
        if (skill == null) return;

        pendingSkill = skill;

        // 設置冷卻（以技能為 key）
        if (!cooldownTimers.ContainsKey(skill))
            cooldownTimers.Add(skill, skill.cooldown);
        else
            cooldownTimers[skill] = skill.cooldown;

        // 進入對應的 SkillState（索引需和 skills 對齊）
        if (player.skillStates != null && skillIndexInList >= 0 && skillIndexInList < player.skillStates.Length)
        {
            player.stateMachine.ChangeState(player.skillStates[skillIndexInList]);
        }
        else
        {
            Debug.LogWarning("[SkillManager] player.skillStates 數量或順序與 skills 不一致。");
        }
    }

    /// <summary>
    /// 在動畫事件中調用此方法來執行技能效果
    /// </summary>
    /// <summary>由動畫事件（Animation Event）呼叫，真正施放技能效果。</summary>
    public void ExecutePendingSkillEffect()
    {
        if (pendingSkill == null)
        {
            Debug.LogWarning("沒有待執行的技能！");
            return;
        }

        var s = pendingSkill;
        pendingSkill = null;

        if (s.isClone) ExecuteCloneSkill(s);
        else if (s.isSummon) ExecuteSpiritSkill(s);
        else if (s.isProjectile) ExecuteProjectileSkill(s);
        else if (s.isFlyingSword) ExecuteFlyingSwordSkill(s);
        else if (s.isDimensionGun) ExecuteDimensionGunSkill(s);
        else if (s.isHitGround) ExecuteHitGroundSkill(s);
    }



    private IEnumerator SummonCloneDelayed(SkillData skill, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (skill.skillPrefab != null)
        {
            // 根據玩家面向決定召喚位置
            Vector3 spawnOffset = new Vector3(player.facingDir * 2f, 0.5f, 0);
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

        else
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
        else
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

            Vector3 offset = new Vector3(2f * player.facingDir, 0, 0f);
            spawnPos += offset;

            GameObject projectile = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);

            // 設置投射物方向
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(player.facingDir * 10f, 10f);
            }

            // 設置投射物的傷害
            Projectile controller = projectile.GetComponent<Projectile>();
            if (controller != null)
            {
                controller.Setup(skill.damageAmount, player.facingDir);
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
    public float GetCooldownRemainingByIndex(int index)
    {
        if (index < 0 || index >= skills.Count) return 0f;
        var s = skills[index];
        if (s == null) return 0f;
        return cooldownTimers.TryGetValue(s, out float t) ? Mathf.Max(0f, t) : 0f;
    }

    // 檢查技能是否可用
    public bool IsSelectedSkillReady()
    {
        var s = SelectedSkill;
        if (s == null) return false;
        return !cooldownTimers.TryGetValue(s, out float t) || t <= 0f;
    }

    // ====== 其他 ======
    public void ClearSpiritReference() { currentSpirit = null; }
    public void ClearCloneReference() { currentClone = null; }

    // 檢查是否有精靈存在
    public bool HasActiveSpirit()
    {
        return currentSpirit != null;
    }

    public bool HasActiveClone()
    {
        return currentClone != null; 
    }

    private void ShowMPNotEnoughMessage()
    {
        if (UI.instance != null)
        {
            // UI.instance.ShowCenterMessage("MP 不足！");
        }
        else
        {
            Debug.LogWarning("MP 不足！");
        }
    }
}