using TMPro;
using UnityEngine;

public class TMPTextBlink : MonoBehaviour
{
    [Header("閃爍速度")]
    public float blinkSpeed = 2f;

    [Header("最低透明度")]
    [Range(0, 1)]
    public float minAlpha = 0.2f;

    [Header("最高透明度")]
    [Range(0, 1)]
    public float maxAlpha = 1f;

    private TextMeshProUGUI textUI;

    private void Awake()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Sin 值從 -1 到 1，換算成 0 到 1
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

        // 用 t 在 minAlpha 和 maxAlpha 之間插值
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color color = textUI.color;
        color.a = alpha;
        textUI.color = color;
    }
}