using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DisplaySelector : MonoBehaviour
{
    [Header("顯示模式頁面 (對應 視窗/無邊框/全螢幕 的圖示)")]
    public GameObject[] pages;

    [Header("顯示文字")]
    public Text legacyText;
    public TextMeshProUGUI tmpText;
    public string displayFormat = "{0}";

    private int currentIndex = 0;

    // 定義三種模式
    private readonly FullScreenMode[] modes = {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow, // 無邊框
        FullScreenMode.ExclusiveFullScreen // 全螢幕
    };

    private readonly string[] labels = { "視窗化", "無邊框", "全螢幕" };

    void Start()
    {
        // 1. 初始化讀取狀態
        FullScreenMode currentMode = Screen.fullScreenMode;

        // 如果有 Service 就用 Service 的資料
        if (SettingsService.Instance != null)
        {
            currentMode = SettingsService.Instance.Settings.screenMode;
        }

        // 轉換 Mode -> Index
        currentIndex = IndexFromMode(currentMode);

        // 2. 更新 UI
        ApplyCurrentPage(false);
    }

    public void NextPage()
    {
        currentIndex = (currentIndex + 1) % modes.Length;
        ApplyCurrentPage(true);
    }

    public void PreviousPage()
    {
        currentIndex = (currentIndex - 1 + modes.Length) % modes.Length;
        ApplyCurrentPage(true);
    }

    void ApplyCurrentPage(bool applyToScreen = true)
    {
        // ... (省略前半段 UI 更新) ...

        var targetMode = modes[currentIndex];

        if (applyToScreen)
        {
            if (SettingsService.Instance != null)
                SettingsService.Instance.SetScreenMode(targetMode);

            // ===  修改這裡：針對視窗模式給一個明顯的測試解析度 ===

            int w = Screen.width;
            int h = Screen.height;

            // 如果目標是「視窗化」，強制縮小解析度，不然會看起來像全螢幕
            if (targetMode == FullScreenMode.Windowed)
            {
                w = 1280;
                h = 720;
            }
            // 如果是全螢幕或無邊框，就用螢幕原始最大解析度 (或你指定的解析度)
            else
            {
                w = Screen.currentResolution.width;
                h = Screen.currentResolution.height;
            }

            // 補上 refreshRate: 0 (代表使用最大更新率)，避免因為更新率不對而被系統拒絕切換
            Screen.SetResolution(w, h, targetMode, 0);

            Debug.Log($"[強制執行] SetResolution: {w}x{h}, Mode: {targetMode}");
        }

        UpdateLabelText();
    }

    void UpdateLabelText()
    {
        if (currentIndex < 0 || currentIndex >= labels.Length) return;

        string label = labels[currentIndex];
        string finalText = string.IsNullOrEmpty(displayFormat) ? label : string.Format(displayFormat, label);

        if (tmpText) tmpText.text = finalText;
        if (legacyText) legacyText.text = finalText;
    }

    private int IndexFromMode(FullScreenMode m)
    {
        for (int i = 0; i < modes.Length; i++)
        {
            if (modes[i] == m) return i;
        }
        return 0; // 預設回傳 0 (視窗化)
    }
}