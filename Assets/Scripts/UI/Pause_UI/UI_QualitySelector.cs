using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UI_QualitySelector : MonoBehaviour
{
    public GameObject[] pages; // 頁面陣列：圖片 1, 2, 3
    private int currentIndex = 2; // 預設從「高畫質」開始（第 2 頁）

    void Start()
    {
        ApplyCurrentPage();
        Debug.Log($"[初始化] 可用畫質等級總數：{QualitySettings.names.Length}");
    }

    public void NextPage()
    {
        currentIndex = (currentIndex + 1) % 3; // 限制只有三個選項
        ApplyCurrentPage();
    }

    public void PreviousPage()
    {
        currentIndex = (currentIndex - 1 + 3) % 3;
        ApplyCurrentPage();
    }

    void ApplyCurrentPage()
    {
        // 顯示對應頁面，隱藏其他頁面
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex);
        }

        // 將畫質設定套用
        SetQualityByPageIndex(currentIndex);
    }

    void SetQualityByPageIndex(int index)
    {
        string[] qualityLabels = { "低", "中", "高" };

        int qualityLevel = Mathf.Clamp(index, 0, 2); // 確保畫質索引在 0~2
        QualitySettings.SetQualityLevel(qualityLevel, true);

        Debug.Log($"[畫質切換] 畫質等級：{qualityLabels[qualityLevel]}（Index: {qualityLevel}） → 圖片 Page_{qualityLevel}");
    }
}