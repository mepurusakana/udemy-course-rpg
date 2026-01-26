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
        // 1. 更新頁面物件顯示
        if (pages != null && pages.Length > 0)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (i < pages.Length && pages[i])
                    pages[i].SetActive(i == currentIndex);
            }
        }

        // 2. 套用螢幕設定
        var targetMode = modes[currentIndex];

        if (applyToScreen)
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.SetScreenMode(targetMode);
            }
            else
            {
                Screen.SetResolution(Screen.width, Screen.height, targetMode);
            }

            // ===原本只有這行 (顯示目標)  ===
            Debug.Log($"[顯示模式] 請求切換為：{labels[currentIndex]}");

            // ===【新增】這行才是真正的驗證 (顯示實際結果) ===
            // 注意：Unity 編輯器中 SetResolution 不會立即生效，數值可能會慢一幀才變，
            // 建議 Build 出來測試時看這行最準。
            Debug.Log($"【驗證報告】 實際解析度: {Screen.width} x {Screen.height} | 實際模式: {Screen.fullScreenMode}");
        }

        // 3. 更新文字
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