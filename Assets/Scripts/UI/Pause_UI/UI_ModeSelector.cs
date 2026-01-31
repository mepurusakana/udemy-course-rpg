using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ModeSelector : MonoBehaviour
{
    [Header("模式頁面 (0=視窗, 1=全螢幕)")]
    // 如果你有做對應的圖示(例如視窗圖/全螢幕圖)再拖進來，沒有就留空
    public GameObject[] pages;

    [Header("顯示文字")]
    public TextMeshProUGUI tmpText;
    public string displayFormat = "{0}"; // 例如 "模式：{0}"

    private int currentIndex = 1; // 預設全螢幕

    // 定義兩種模式
    private readonly FullScreenMode[] modes = {
        FullScreenMode.Windowed,             // 視窗化
        FullScreenMode.ExclusiveFullScreen   // 全螢幕
    };

    private readonly string[] labels = { "視窗化", "全螢幕" };

    void Start()
    {
        // 自動偵測目前是用哪種，讓 UI 顯示正確
        currentIndex = Screen.fullScreen ? 1 : 0;
        UpdateUI();
    }

    public void NextPage()
    {
        // 在 0 和 1 之間切換
        currentIndex = (currentIndex + 1) % 2;
        ApplyMode();
    }

    public void PreviousPage()
    {
        currentIndex = (currentIndex - 1 + 2) % 2;
        ApplyMode();
    }

    void ApplyMode()
    {
        // 抓取目前的解析度 (切換模式時，保持解析度不變)
        int w = Screen.width;
        int h = Screen.height;

        // 執行切換
        Screen.SetResolution(w, h, modes[currentIndex]);

        Debug.Log($"[模式切換] {labels[currentIndex]} ({w}x{h})");
        UpdateUI();
    }

    void UpdateUI()
    {
        // 更新文字
        if (tmpText != null)
            tmpText.text = string.Format(displayFormat, labels[currentIndex]);

        // 更新圖示頁面 (如果有)
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
                if (pages[i] != null) pages[i].SetActive(i == currentIndex);
        }
    }
}