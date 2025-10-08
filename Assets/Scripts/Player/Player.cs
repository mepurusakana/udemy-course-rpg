using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class Player : Entity, ISaveable
{
    public static Player instance;

    [Header("Attack details")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = .2f;

    [Header("Jump Info")]
    public float jumpTimer = 0.5f;
    public bool isBusy { get;  set; }
    [Header("Move info")]
    public float moveSpeed = 12f;
    public float jumpForce;
    public float swordReturnImpact;
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    [Header("Dash info")]
    public float dashSpeed;
    public float dashDuration;
    private float defaultDashSpeed;

    [Header("BladeLight info")]
    public GameObject slashEffectPrefab;
    //public Transform slashSpawnPoint;

    [Header("Healing Info")]
    public int maxChantCharges = 3;
    public int chantCharges;
    public List<GameObject> chantIcons; // 圖示列表
    public float holdTime;
    public float healHoldTime;
    public ParticleSystem healingVFX;

    [Header("Attack Light Effect")]
    public Light2D attackLight;
    private Coroutine fadeOutCoroutine; // 紀錄目前的淡出 Coroutine

    [Header("Movement Particles")]
    public ParticleSystem moveDustEffect;
    //public float platformCatcher;

    [SerializeField] public LayerMask whatIsOneWayPlatform;




    public float dashDir { get; private set; }


    //public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }
    public PlayerFX fx { get; private set; }

    public PlayerAimSwordState aimSword { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }

    




    #region States
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerDashState dashState { get; private set; }

    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCounterAttackState counterAttack { get; private set; }

    public PlayerAimSwordState aimSowrd { get; private set; }
    //public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerBlackholeState blackHole { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public PlayerHealingState healingState { get; private set; }
    public PlayerHurtState hurtState { get; private set; }
    public PlayerSkillState[] skillStates { get; private set; }
    #endregion



    protected override void Awake()
    {
        base.Awake();


        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSowrd = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        blackHole = new PlayerBlackholeState(this, stateMachine, "Jump");

        deadState = new PlayerDeadState(this, stateMachine, "Die");
        healingState = new PlayerHealingState(this, stateMachine, "Healing");
        hurtState = new PlayerHurtState(this, stateMachine, "Hurt");

        // 初始化技能狀態池
        SkillManager skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            skillStates = new PlayerSkillState[skillManager.SkillCount];
            for (int i = 0; i < skillManager.SkillCount; i++)
            {
                skillStates[i] = new PlayerSkillState(
                this,                     // Player
                stateMachine,             // StateMachine
                skillManager.skills[i].animationBoolName // 動畫 Bool
                );

                // 再設定技能資料
                skillStates[i].SetSkill(skillManager.skills[i], i);
            }
        }

        StopMoveDust();
    }

    protected override void Start()
    {
        base.Start();

        fx = GetComponent<PlayerFX>();

        //skill = SkillManager.instance;

        stateMachine.Initialize(idleState);

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;

        chantCharges = maxChantCharges;
        UpdateChantUI();

        if (SaveManager.instance != null && SaveManager.instance.gameData != null)
            transform.position = SaveManager.instance.gameData.savedCheckpoint;

        if (attackLight != null)
        {
            attackLight.enabled = false;
            attackLight.intensity = 0f;
        }
    }

    // ========== 新增：受擊相關方法 ========== //

    /// <summary>
    /// 受到傷害並進入受擊狀態（整合所有受擊邏輯）
    /// </summary>
    /// <param name="damageDirection">傷害來源的 Transform</param>
    /// <param name="knockbackForce">自訂擊退力道（可選）</param>
    public void TakeDamageAndEnterHurtState(Transform damageDirection, Vector2? knockbackForce = null)
    {
        // 1. 設置擊退方向（根據傷害來源）
        SetupKnockbackDir(damageDirection);

        // 2. 設置擊退力道（如果有自訂）
        if (knockbackForce.HasValue)
        {
            SetupKnockbackPower(knockbackForce.Value);
        }

        // 3. 切換到受擊狀態（狀態內會自動處理回彈和僵直）
        stateMachine.ChangeState(hurtState);
    }

    /// <summary>
    /// 獲取當前設定的擊退力道
    /// </summary>
    public Vector2 GetKnockbackPower()
    {
        // 從 Entity 繼承來的 knockbackPower
        return knockbackPower;
    }


    public void TeleportPlayer(Vector3 position) => transform.position = position;

    protected override void Update()
    {

        if (Time.timeScale == 0)
            return;

        base.Update();

        stateMachine.currentState.Update();

        CheckForDashInput();
    }


    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);

        Invoke("ReturnDefaultSpeed", _slowDuration);

    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword);
    }

    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    private void CheckForDashInput()
    {
        if (IsWallDetected())
            return;

        //if (skill.dash.dashUnlocked == false)
        //    return;

    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deadState);
    }

    protected override void SetupZeroKnockbackPower()
    {
        knockbackPower = new Vector2(0, 0);
    }

    //public void SpawnSlashEffect()
    //{
    //    GameObject effect = Instantiate(slashEffectPrefab, slashSpawnPoint.position, slashSpawnPoint.rotation);
    //}

    public void UseChantCharge()
    {
        if (chantCharges <= 0) return;

        chantCharges--;
        UpdateChantUI();
    }

    public void UpdateChantUI()
    {
        for (int i = 0; i < chantIcons.Count; i++)
        {
            chantIcons[i].SetActive(i < chantCharges);
        }
    }

    public void Heal(int amount)
    {
        stats.IncreaseHealthBy(amount);
    }

    public IEnumerator FadeOutLight(Light2D light, float duration)
    {
        float startIntensity = light.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (light == null) yield break;

            elapsedTime += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, 0f, elapsedTime / duration);
            yield return null;
        }

        light.intensity = 0f;
        light.enabled = false;
    }

    public void StopFadeOut()
    {
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }
    }

    public void StartFadeOut(float duration)
    {
        StopFadeOut(); // 確保不會有重複 Coroutine
        fadeOutCoroutine = StartCoroutine(FadeOutLight(attackLight, duration));
    }


    public void LoadData(GameData data)
    {
        transform.position = data.savedCheckpoint;
    }

    public void SaveData(ref GameData data)
    {
        data.savedCheckpoint = transform.position;
    }

    public override bool IsWallDetected()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        if (hit && hit.collider.CompareTag("OneWayPlatform"))
            return false;

        return hit;
    }

    public override bool IsGroundDetected()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            whatIsGround
        );

        if (hit)
        {
            if (hit.collider.CompareTag("OneWayPlatform"))
            {
                //只在往下掉落時才算 Ground
                if (rb.velocity.y <= 0f)
                    return true;
                else
                    return false;
            }  // 從下方打到 OneWayPlatform，不算地面

            return true;        // 打到其他地面，算地面
        }

        return false;           // 沒打到，當然不是地面
    }

    public void PlayMoveDust()
    {
        if (moveDustEffect != null)
            moveDustEffect.Play();
    }

    public void StopMoveDust()
    {
        if (moveDustEffect != null)
            moveDustEffect.Stop();
    }

}
