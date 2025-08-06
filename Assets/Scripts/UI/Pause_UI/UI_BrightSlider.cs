using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BrightSlider : MonoBehaviour
{
    public Image brightnessOverlay;               // 黑色圖片物件（Image）
    public Slider brightnessSlider;               // 控制亮度的滑條
    public TextMeshProUGUI brightnessValueText;   // 顯示亮度百分比（可選）

    void Start()
    {
        // 初始化
        UpdateBrightness();
        brightnessSlider.onValueChanged.AddListener(delegate { UpdateBrightness(); });
    }

    public void UpdateBrightness()
    {
        float sliderValue = brightnessSlider.value; // 範圍 0~1（0 最亮、1 最暗）

        // 更新圖片透明度
        Color color = brightnessOverlay.color;
        color.a = sliderValue;
        brightnessOverlay.color = color;

        // 更新文字
        if (brightnessValueText != null)
        {
            int percent = Mathf.RoundToInt((1f - sliderValue) * 100f);
            brightnessValueText.text = $"亮度：{percent}%";
        }

        Debug.Log($"[亮度調整] Slider: {sliderValue:F2} → 透明度: {sliderValue:F2}");
    }
}