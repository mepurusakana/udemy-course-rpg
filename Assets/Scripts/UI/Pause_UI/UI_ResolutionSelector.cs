using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResolutionSelector : MonoBehaviour
{
    [Header("解析度頁面 (0=1920, 1=1600, 2=1366)")]
    public GameObject[] pages;

    [Header("顯示文字")]
    public TextMeshProUGUI tmpText;
    public string displayFormat = "{0}"; // 例如 "解析度：{0}"

    private int currentIndex = 0;

    // ★ 在這裡定義你想要的解析度
    private readonly Vector2Int[] resolutions = {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1366, 768)
    };

    private readonly string[] labels = { "1920 x 1080", "1600 x 900", "1366 x 768" };

    void Start()
    {
        // 初始化：試著找出目前最接近的解析度，讓 UI 停在那一頁
        currentIndex = FindClosestResolutionIndex();
        UpdateUI();
    }

    public void NextPage()
    {
        currentIndex = (currentIndex + 1) % resolutions.Length;
        ApplyResolution();
    }

    public void PreviousPage()
    {
        currentIndex = (currentIndex - 1 + resolutions.Length) % resolutions.Length;
        ApplyResolution();
    }

    void ApplyResolution()
    {
        // 抓取目標解析度
        Vector2Int targetRes = resolutions[currentIndex];

        // 抓取目前的模式 (全螢幕/視窗)，保持模式不變
        FullScreenMode currentMode = Screen.fullScreenMode;

        // 執行切換
        Screen.SetResolution(targetRes.x, targetRes.y, currentMode);

        Debug.Log($"[解析度切換] {labels[currentIndex]} (模式: {currentMode})");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (tmpText != null)
            tmpText.text = string.Format(displayFormat, labels[currentIndex]);

        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
                if (pages[i] != null) pages[i].SetActive(i == currentIndex);
        }
    }

    // 輔助方法：比對目前螢幕寬度，找出最接近的選單索引
    int FindClosestResolutionIndex()
    {
        int currentWidth = Screen.width;
        int bestIndex = 0;
        int minDiff = int.MaxValue;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int diff = Mathf.Abs(resolutions[i].x - currentWidth);
            if (diff < minDiff)
            {
                minDiff = diff;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}