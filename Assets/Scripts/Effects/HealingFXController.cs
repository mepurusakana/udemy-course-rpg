using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D 需要這個命名空間

public class HealingFXController : MonoBehaviour
{
    [Header("Light 2D 光芒設置")]
    [Tooltip("將 HealingLight 物件拖到這裡")]
    public Light2D healingLight;

    [Tooltip("光芒是否有脈動動畫")]
    public bool useLightAnimation = true;

    [Header("Particle System 設置")]
    [Tooltip("將 Particle System 物件拖到這裡（可多個）")]
    public ParticleSystem[] crossParticles;

    [Header("音效設置（可選）")]
    [Tooltip("治療開始音效")]
    public AudioClip healingStartSound;

    [Tooltip("治療循環音效")]
    public AudioClip healingLoopSound;

    private AudioSource audioSource;
    private Animator lightAnimator;

    private void Awake()
    {
        // 獲取組件
        audioSource = GetComponent<AudioSource>();

        if (healingLight != null && useLightAnimation)
        {
            lightAnimator = healingLight.GetComponent<Animator>();
        }

        // 初始化時關閉所有特效
        DisableAllEffects();
    }

    /// <summary>
    /// 啟動治療特效（由 PlayerHealingState 調用）
    /// </summary>
    public void StartHealingEffects()
    {
        // 啟動 Light 2D
        if (healingLight != null)
        {
            healingLight.enabled = true;
            healingLight.gameObject.SetActive(true);

            // 啟動光芒動畫
            if (lightAnimator != null && useLightAnimation)
            {
                lightAnimator.enabled = true;
            }
        }

        // 啟動所有 Particle Systems
        if (crossParticles != null)
        {
            foreach (ParticleSystem ps in crossParticles)
            {
                if (ps != null)
                {
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }
            }
        }

        // 播放音效
        PlayHealingSound();
    }

    /// <summary>
    /// 停止治療特效（由 PlayerHealingState 調用）
    /// </summary>
    public void StopHealingEffects()
    {
        // 關閉 Light 2D
        if (healingLight != null)
        {
            healingLight.enabled = false;
            healingLight.gameObject.SetActive(false);

            // 停止光芒動畫
            if (lightAnimator != null)
            {
                lightAnimator.enabled = false;
            }
        }

        // 停止所有 Particle Systems
        if (crossParticles != null)
        {
            foreach (ParticleSystem ps in crossParticles)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
            }
        }

        // 停止音效
        StopHealingSound();
    }

    /// <summary>
    /// 禁用所有特效（初始化用）
    /// </summary>
    private void DisableAllEffects()
    {
        if (healingLight != null)
        {
            healingLight.enabled = false;
            healingLight.gameObject.SetActive(false);
        }

        if (crossParticles != null)
        {
            foreach (ParticleSystem ps in crossParticles)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 播放治療音效
    /// </summary>
    private void PlayHealingSound()
    {
        if (audioSource == null)
            return;

        // 播放開始音效
        if (healingStartSound != null)
        {
            audioSource.PlayOneShot(healingStartSound);
        }

        // 播放循環音效
        if (healingLoopSound != null)
        {
            audioSource.clip = healingLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// 停止治療音效
    /// </summary>
    private void StopHealingSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// 當腳本被禁用時確保清理
    /// </summary>
    private void OnDisable()
    {
        StopHealingEffects();
    }

    /// <summary>
    /// 除錯用：在場景視圖中顯示粒子發射範圍
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (crossParticles != null && crossParticles.Length > 0 && crossParticles[0] != null)
        {
            Gizmos.color = Color.green;
            var shape = crossParticles[0].shape;
            Gizmos.DrawWireSphere(transform.position + crossParticles[0].transform.localPosition, shape.radius);
        }
    }
}