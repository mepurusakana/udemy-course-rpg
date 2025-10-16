using System.Collections;
using UnityEngine;

public class HealingCrossBehaviour : MonoBehaviour
{
    private float riseSpeed;
    private float lifetime;
    private SpriteRenderer spriteRenderer;
    private float elapsedTime = 0f;
    private Color originalColor;

    /// <summary>
    /// 設置十字的參數
    /// </summary>
    public void Setup(float _riseSpeed, float _lifetime)
    {
        riseSpeed = _riseSpeed;
        lifetime = _lifetime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 啟動自動銷毀
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 向上移動
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

        // 淡出效果
        elapsedTime += Time.deltaTime;
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / lifetime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // 可選：輕微旋轉效果
        transform.Rotate(0, 0, 50f * Time.deltaTime);
    }
}