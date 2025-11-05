using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BrightnessOverlayController : MonoBehaviour
{
    [Header("覆蓋黑圖（Alpha=亮度；0=最亮,1=最暗）")]
    public Image overlayImage;

    private void OnEnable()
    {
        SettingsService.onChanged += Apply;
        if (SettingsService.Instance != null) Apply(SettingsService.Instance.Settings);
    }
    private void OnDisable()
    {
        SettingsService.onChanged -= Apply;
    }

    private void Apply(GameSettings s)
    {
        if (!overlayImage) return;
        var c = overlayImage.color;
        c.a = s.brightness;
        overlayImage.color = c;
    }
}
