using UnityEngine;
using UnityEngine.UI;     // Unity 的舊 Text
using TMPro;              // TextMeshPro

public class UI_QualitySelector : MonoBehaviour
{
    [Header("畫質頁面（索引 0=低,1=中,2=高 ...）")]
    public GameObject[] pages;

    [Header("顯示文字（兩者擇一或都填，留空則不顯示）")]
    public Text legacyText;                 // UnityEngine.UI.Text
    public TextMeshProUGUI tmpText;         // TextMeshProUGUI

    [Header("自訂顯示標籤（對應 pages 長度）")]
    public string[] labels = new string[] { "低", "中", "高" };

    [Header("可選：前綴格式")]
    [Tooltip("例如：\"畫質模式：{0}\"，{0} 會被替換成標籤文字")]
    public string displayFormat = "畫質模式：{0}";  // ← 改這裡!

    private int currentIndex = 2;

    // 畫質變更事件（其他 UI 可訂閱）
    public static System.Action<int> OnQualityChanged;

    void Start()  // ← 改用 Start 避免 NullReferenceException
    {
        // 從服務抓目前畫質並安全夾在 0 ~ (pages.Length-1)
        int saved = (SettingsService.Instance != null)
            ? SettingsService.Instance.Settings.qualityIndex
            : 0;
        currentIndex = Mathf.Clamp(saved, 0, Mathf.Max(0, pages.Length - 1));
        ApplyCurrentPage();
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0) return;
        currentIndex = (currentIndex + 1) % pages.Length;
        ApplyCurrentPage();
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0) return;
        currentIndex = (currentIndex - 1 + pages.Length) % pages.Length;
        ApplyCurrentPage();
    }

    void ApplyCurrentPage()
    {
        // 顯示目前頁
        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(i == currentIndex);
            }
        }

        // 寫回服務（也可在這裡呼叫 Unity 內建畫質）
        SettingsService.Instance?.SetQualityIndex(currentIndex);
        // QualitySettings.SetQualityLevel(currentIndex, true); // 若要連動 Unity 內建畫質，取消註解

        // 更新顯示文字
        UpdateLabelText();

        // 廣播事件（讓其他 UI 能同步）
        OnQualityChanged?.Invoke(currentIndex);

        Debug.Log($"[畫質切換] {GetLabel(currentIndex)}（Index: {currentIndex}）");
    }

    void UpdateLabelText()
    {
        string label = GetLabel(currentIndex);
        string finalText = string.IsNullOrEmpty(displayFormat) ? label : string.Format(displayFormat, label);

        if (tmpText != null) tmpText.text = finalText;
        if (legacyText != null) legacyText.text = finalText;
    }

    string GetLabel(int index)
    {
        // 優先用自訂 labels
        if (labels != null && index >= 0 && index < labels.Length && !string.IsNullOrEmpty(labels[index]))
            return labels[index];

        // 預設三檔時給直覺中文
        if (pages != null && pages.Length == 3)
        {
            if (index == 0) return "低";
            if (index == 1) return "中";
            if (index == 2) return "高";
        }

        // 其他情況：模式 i
        return $"模式 {index}";
    }
}