// 在你現有的 EntityFX.cs 中添加以下內容

using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class EntityFX : MonoBehaviour
{
    protected Player player;
    protected SpriteRenderer sr;

    [Header("Pop Up Text")]
    [SerializeField] private GameObject popUpTextPrefab;

    [Header("Flash FX")]
    [SerializeField] private float flashDuration;
    [SerializeField] private Material hitMat;
    private Material originalMat;

    [Header("Hit FX")]
    [SerializeField] private GameObject hitFx;
    [SerializeField] private GameObject criticalHitFx;

    // ========== 新增：無敵特效設定 ========== //
    [Header("Invincibility FX")]
    [SerializeField] private float invincibilityFlashSpeed = 0.1f;
    [SerializeField] private Color invincibilityColor = new Color(1f, 1f, 1f, 0.5f); // 半透明白色
    [SerializeField] private bool useGlowEffect = false; // 是否使用發光效果而非閃爍

    private Coroutine invincibilityCoroutine;
    private Color originalColor;
    // ======================================== //

    private GameObject myHealthBar;

    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        player = PlayerManager.instance.player;

        originalMat = sr.material;
        originalColor = sr.color; // 儲存原始顏色

        myHealthBar = GetComponentInChildren<UI_HealthBar>(true).gameObject;
    }

    // ========== 原有的方法保持不變 ========== //

    public void CreatePopUpText(string _text)
    {
        float randomX = Random.Range(-1, 1);
        float randomY = Random.Range(1.5f, 3);

        Vector3 positionOffset = new Vector3(randomX, randomY, 0);

        GameObject newText = Instantiate(popUpTextPrefab, transform.position + positionOffset, Quaternion.identity);

        newText.GetComponent<TextMeshPro>().text = _text;
    }

    public void MakeTransprent(bool _transprent)
    {
        if (_transprent)
        {
            myHealthBar.SetActive(false);
            sr.color = Color.clear;
        }
        else
        {
            myHealthBar.SetActive(true);
            sr.color = Color.white;
        }
    }

    private IEnumerator FlashFX()
    {
        sr.material = hitMat;
        Color currentColor = sr.color;
        sr.color = Color.white;

        yield return new WaitForSeconds(flashDuration);

        sr.color = currentColor;
        sr.material = originalMat;
    }

    private void RedColorBlink()
    {
        if (sr.color != Color.white)
            sr.color = Color.white;
        else
            sr.color = Color.red;
    }

    private void CancelColorChange()
    {
        CancelInvoke();
        sr.color = Color.white;
    }



    public void CreateHitFx(Transform _target, bool _critical)
    {
        float zRotation = Random.Range(-90, 90);
        float xPosition = Random.Range(-.5f, .5f);
        float yPosition = Random.Range(-.5f, .5f);

        Vector3 hitFxRotaion = new Vector3(0, 0, zRotation);

        GameObject hitPrefab = hitFx;

        if (_critical)
        {
            hitPrefab = criticalHitFx;

            float yRotation = 0;
            zRotation = Random.Range(-45, 45);

            if (GetComponent<Entity>().facingDir == -1)
                yRotation = 180;

            hitFxRotaion = new Vector3(0, yRotation, zRotation);
        }

        GameObject newHitFx = Instantiate(hitPrefab, _target.position + new Vector3(xPosition, yPosition), Quaternion.identity);
        newHitFx.transform.Rotate(hitFxRotaion);
        Destroy(newHitFx, .5f);
    }

    #region Invincibility
    // ========== 新增：無敵特效方法 ========== //
    /// <summary>
    /// 開始無敵特效
    /// </summary>
    public void StartInvincibilityEffect()
    {
        // 停止之前的無敵特效（如果有）
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        // 根據設定選擇閃爍或發光效果
        if (useGlowEffect)
        {
            invincibilityCoroutine = StartCoroutine(InvincibilityGlowEffect());
        }
        else
        {
            invincibilityCoroutine = StartCoroutine(InvincibilityFlashEffect());
        }
    }

    /// <summary>
    /// 停止無敵特效
    /// </summary>
    public void StopInvincibilityEffect()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        // 恢復原本的顏色
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    /// <summary>
    /// 無敵閃爍效果（經典風格）
    /// </summary>
    private IEnumerator InvincibilityFlashEffect()
    {
        while (true)
        {
            // 變成半透明
            sr.color = invincibilityColor;
            yield return new WaitForSeconds(invincibilityFlashSpeed);

            // 恢復正常
            sr.color = originalColor;
            yield return new WaitForSeconds(invincibilityFlashSpeed);
        }
    }

    /// <summary>
    /// 無敵發光效果（脈衝風格）
    /// </summary>
    private IEnumerator InvincibilityGlowEffect()
    {
        float glowIntensity = 0f;
        bool increasing = true;

        while (true)
        {
            // 脈衝呼吸效果
            if (increasing)
            {
                glowIntensity += Time.deltaTime * 3f;
                if (glowIntensity >= 1f)
                {
                    glowIntensity = 1f;
                    increasing = false;
                }
            }
            else
            {
                glowIntensity -= Time.deltaTime * 3f;
                if (glowIntensity <= 0.3f)
                {
                    glowIntensity = 0.3f;
                    increasing = true;
                }
            }

            // 在原始顏色和白色之間插值
            sr.color = Color.Lerp(originalColor, Color.white, glowIntensity * 0.6f);

            yield return null;
        }
    }

    /// <summary>
    /// 快速閃爍效果（受傷前的預警）
    /// </summary>
    public void InvincibilityEndWarning(float duration)
    {
        StartCoroutine(InvincibilityEndWarningCoroutine(duration));
    }

    private IEnumerator InvincibilityEndWarningCoroutine(float duration)
    {
        float elapsed = 0f;
        float fastFlashSpeed = 0.05f; // 更快的閃爍速度

        while (elapsed < duration)
        {
            sr.color = invincibilityColor;
            yield return new WaitForSeconds(fastFlashSpeed);

            sr.color = originalColor;
            yield return new WaitForSeconds(fastFlashSpeed);

            elapsed += fastFlashSpeed * 2;
        }

        sr.color = originalColor;
    }

    // ======================================== //
    #endregion
}