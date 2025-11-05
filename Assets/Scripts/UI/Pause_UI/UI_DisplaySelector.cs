using UnityEngine;
using UnityEngine.UI;     // Unity 的舊 Text
using TMPro;              // TextMeshPro

public class UI_DisplaySelector : MonoBehaviour
{
    [Header("顯示模式頁面")]
    public GameObject[] pages;

    [Header("顯示文字（兩者擇一或都填，留空則不顯示）")]
    public Text legacyText;                 // UnityEngine.UI.Text
    public TextMeshProUGUI tmpText;         // TextMeshProUGUI

    [Header("可選：前綴格式")]
    [Tooltip("例如：\"顯示模式：{0}\"，{0} 會被替換成模式名稱")]
    public string displayFormat = "{0}";    // 可改成 "顯示模式：{0}"

    private int currentIndex = 0;

    private readonly FullScreenMode[] modes = {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private readonly string[] labels = { "視窗", "無邊框視窗", "全螢幕" };

    void Start()
    {
        var m = SettingsService.Instance.Settings.screenMode;
        currentIndex = IndexFromMode(m);
        ApplyCurrentPage();
    }

    public void NextPage()
    {
        currentIndex = (currentIndex + 1) % pages.Length;
        ApplyCurrentPage();
    }

    public void PreviousPage()
    {
        currentIndex = (currentIndex - 1 + pages.Length) % pages.Length;
        ApplyCurrentPage();
    }

    void ApplyCurrentPage()
    {
        // 顯示目前頁面
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == currentIndex);

        // 套用顯示模式
        var mode = modes[Mathf.Clamp(currentIndex, 0, modes.Length - 1)];
        SettingsService.Instance.SetScreenMode(mode);

        // 更新文字顯示
        UpdateLabelText();

        Debug.Log($"[顯示模式切換] {labels[currentIndex]}（{mode}）");
    }

    void UpdateLabelText()
    {
        string label = labels[Mathf.Clamp(currentIndex, 0, labels.Length - 1)];
        string finalText = string.IsNullOrEmpty(displayFormat) ? label : string.Format(displayFormat, label);

        if (tmpText != null) tmpText.text = finalText;
        if (legacyText != null) legacyText.text = finalText;
    }

    private int IndexFromMode(FullScreenMode m)
    {
        for (int i = 0; i < modes.Length; i++)
            if (modes[i] == m) return i;
        return 0;
    }
}