// 完整的 PlayerFX.cs，包含無敵特效擴展

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFX : EntityFX
{
    [Header("Screen shake FX")]
    [SerializeField] private float shakeMultiplier;
    public Vector3 shakeSwordImpact;
    public Vector3 shakeHighDamage;
    private CinemachineImpulseSource screenShake;

    [Header("After image fx")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float colorLooseRate;
    [SerializeField] private float afterImageCooldown;
    private float afterImageCooldownTimer;
    [Space]
    [SerializeField] private ParticleSystem dustFx;

    // ========== 新增：玩家專屬無敵特效 ========== //
    [Header("Player Invincibility FX")]
    [SerializeField] private ParticleSystem invincibilityParticle; // 無敵粒子特效（可選）
    [SerializeField] private bool enableAfterImageDuringInvincibility = true; // 無敵時是否啟用殘影
    [SerializeField] private float invincibilityAfterImageInterval = 0.05f; // 無敵時殘影間隔

    private Coroutine invincibilityAfterImageCoroutine;
    // ========================================== //

    protected override void Start()
    {
        base.Start();
        screenShake = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        afterImageCooldownTimer -= Time.deltaTime;
    }

    public void ScreenShake(Vector3 _shakePower)
    {
        screenShake.m_DefaultVelocity = new Vector3(_shakePower.x * player.facingDir, _shakePower.y) * shakeMultiplier;
        screenShake.GenerateImpulse();
    }

    public void CreateAfterImage()
    {
        if (afterImageCooldownTimer < 0)
        {
            afterImageCooldownTimer = afterImageCooldown;
            GameObject newAfterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation);
            newAfterImage.GetComponent<AfterImageFX>().SetupAfterImage(colorLooseRate, sr.sprite);
        }
    }

    public void PlayDustFX()
    {
        if (dustFx != null)
            dustFx.Play();
    }

    // ========== 新增：玩家專屬無敵特效方法 ========== //

    /// <summary>
    /// 啟動完整的玩家無敵特效（包含閃爍、粒子、殘影）
    /// </summary>
    public void StartPlayerInvincibilityEffect()
    {
        // 啟動基礎閃爍特效
        StartInvincibilityEffect();

        // 啟動粒子特效（如果有設置）
        if (invincibilityParticle != null)
        {
            invincibilityParticle.Play();
        }

        // 啟動無敵殘影效果
        if (enableAfterImageDuringInvincibility && afterImagePrefab != null)
        {
            if (invincibilityAfterImageCoroutine != null)
            {
                StopCoroutine(invincibilityAfterImageCoroutine);
            }
            invincibilityAfterImageCoroutine = StartCoroutine(InvincibilityAfterImageEffect());
        }
    }

    /// <summary>
    /// 停止完整的玩家無敵特效
    /// </summary>
    public void StopPlayerInvincibilityEffect()
    {
        // 停止基礎閃爍特效
        StopInvincibilityEffect();

        // 停止粒子特效
        if (invincibilityParticle != null)
        {
            invincibilityParticle.Stop();
        }

        // 停止殘影效果
        if (invincibilityAfterImageCoroutine != null)
        {
            StopCoroutine(invincibilityAfterImageCoroutine);
            invincibilityAfterImageCoroutine = null;
        }
    }

    /// <summary>
    /// 無敵期間的連續殘影效果
    /// </summary>
    private IEnumerator InvincibilityAfterImageEffect()
    {
        while (true)
        {
            // 創建殘影但忽略冷卻時間
            GameObject newAfterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation);
            newAfterImage.GetComponent<AfterImageFX>().SetupAfterImage(colorLooseRate * 1.5f, sr.sprite); // 更快消失

            yield return new WaitForSeconds(invincibilityAfterImageInterval);
        }
    }

    /// <summary>
    /// 無敵結束前的警告特效（最後0.5秒快速閃爍）
    /// </summary>
    public void PlayInvincibilityEndWarning()
    {
        InvincibilityEndWarning(0.5f);
    }

    // ============================================= //
}