using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DisplaySelector : MonoBehaviour
{
    public GameObject[] pages; // 拖入 Page_0（視窗）、Page_1（無邊界）、Page_2（全螢幕）
    private int currentIndex = 0; // 預設為 Page_0：視窗模式

    void Start()
    {
        ApplyCurrentPage();
        Debug.Log("[初始化] 顯示模式控制已啟用，共有 " + pages.Length + " 種顯示模式");
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
        // 切換 UI 頁面
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex);
        }

        // 設定顯示模式
        SetDisplayMode(currentIndex);
    }

    void SetDisplayMode(int index)
    {
        // 顯示模式標籤（用於 Debug）
        string[] displayModeLabels = { "視窗模式", "無邊界視窗", "全螢幕模式" };

        // 對應 Unity 的 FullScreenMode
        FullScreenMode[] modes = {
            FullScreenMode.Windowed,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.ExclusiveFullScreen
        };

        int clampedIndex = Mathf.Clamp(index, 0, modes.Length - 1);
        Screen.fullScreenMode = modes[clampedIndex];

        Debug.Log($"[顯示模式切換] 第 {clampedIndex} 頁 → {displayModeLabels[clampedIndex]}（{modes[clampedIndex]}）");
    }
}
