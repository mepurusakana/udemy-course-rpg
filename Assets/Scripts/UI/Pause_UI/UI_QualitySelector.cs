using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_QualitySelector : MonoBehaviour
{
    [Header("畫質頁面（對應 低/中/高 的圖示或物件）")]
    public GameObject[] pages;

    [Header("顯示文字")]
    public Text legacyText;
    public TextMeshProUGUI tmpText;

    [Header("設定")]
    public string[] labels = new string[] { "低", "中", "高" };
    public string displayFormat = "畫質：{0}";

    private int currentIndex = 1; // 預設為中

    // 事件：當畫質改變時通知外界
    public static System.Action<int> OnQualityChanged;

    void Start()
    {
        // 1. 嘗試從 Service 讀取，若無則讀取 Unity 目前設定
        if (SettingsService.Instance != null)
        {
            currentIndex = SettingsService.Instance.Settings.qualityIndex;
        }
        else
        {
            currentIndex = QualitySettings.GetQualityLevel();
        }

        // 2. 確保索引不超出範圍
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, labels.Length - 1));

        // 3. 初始化畫面
        ApplyCurrentPage(false); // false = 初始化時不要重複存檔
    }

    public void NextPage()
    {
        // 循環切換： (0 -> 1 -> 2 -> 0)
        currentIndex = (currentIndex + 1) % labels.Length;
        ApplyCurrentPage(true);
    }

    public void PreviousPage()
    {
        // 循環切換： (0 -> 2 -> 1 -> 0)
        currentIndex = (currentIndex - 1 + labels.Length) % labels.Length;
        ApplyCurrentPage(true);
    }

    // save: 是否要寫入設定 (Start 時不需要)
    void ApplyCurrentPage(bool save = true)
    {
        // 1. 切換頁面顯示
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(i == currentIndex);
            }
        }

        // 2. 執行畫質變更
        if (save)
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.SetQualityIndex(currentIndex);
            }
            else
            {
                QualitySettings.SetQualityLevel(currentIndex, true);
            }

            OnQualityChanged?.Invoke(currentIndex);

            // === 修正這裡 ===
            // 使用 labels[currentIndex] 來取得文字，並加入簡單防呆
            string currentLabel = (labels != null && currentIndex >= 0 && currentIndex < labels.Length)
                                  ? labels[currentIndex]
                                  : currentIndex.ToString();

            Debug.Log($"【驗證報告】 請求畫質: {currentLabel} (Index: {currentIndex}) | 實際生效畫質 Index: {QualitySettings.GetQualityLevel()}");
        }

        // 3. 更新 UI 文字
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
}