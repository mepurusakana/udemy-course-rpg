using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_BrightSlider : MonoBehaviour
{
    public Image brightnessOverlay;
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessValueText;

    // 改用 Start,確保所有 Awake 都執行完了
    private void Start()
    {
        var s = SettingsService.Instance.Settings;
        brightnessSlider.SetValueWithoutNotify(s.brightness);
        UpdateLocalPreview(s.brightness);
        SettingsService.onChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        SettingsService.onChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged(GameSettings s)
    {
        // 別處變動時同步
        brightnessSlider.SetValueWithoutNotify(s.brightness);
        UpdateLocalPreview(s.brightness);
    }

    public void OnSliderChanged(float v)
    {
        // 寫回服務（→ 會廣播、他場景黑圖會跟著更新）
        SettingsService.Instance.SetBrightness(v);
        UpdateLocalPreview(v);
    }

    private void UpdateLocalPreview(float sliderValue)
    {
        if (brightnessOverlay)
        {
            var color = brightnessOverlay.color;
            color.a = sliderValue;
            brightnessOverlay.color = color;
        }
        if (brightnessValueText)
        {
            int percent = Mathf.RoundToInt((1f - sliderValue) * 100f);
            brightnessValueText.text = $"亮度：{percent}%";
        }
    }
}