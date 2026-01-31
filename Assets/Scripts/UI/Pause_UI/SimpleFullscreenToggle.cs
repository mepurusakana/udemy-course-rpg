using UnityEngine;
using UnityEngine.UI;

public class SimpleFullscreenToggle : MonoBehaviour
{
    [Header("請拖入 UI 上的 Toggle 元件")]
    public Toggle fullscreenToggle;

    [Header("視窗模式下的解析度設定")]
    public int windowWidth = 1200;
    public int windowHeight = 900;

    void Start()
    {
        // 1. 初始化：檢查遊戲當前是否為全螢幕
        bool isCurrentFullscreen = Screen.fullScreen;

        if (fullscreenToggle != null)
        {
            // 設定 UI 勾選狀態，但不觸發事件
            fullscreenToggle.SetIsOnWithoutNotify(isCurrentFullscreen);

            // 2. 綁定事件
            fullscreenToggle.onValueChanged.AddListener(OnToggleChanged);
        }
        else
        {
            Debug.LogError("[SimpleFullscreenToggle] 尚未綁定 Toggle！請在 Inspector 拖入。");
        }
    }

    public void OnToggleChanged(bool isFullscreen)
    {
        if (isFullscreen)
        {
            // === 全螢幕 ===
            Resolution nativeRes = Screen.currentResolution;
            Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.ExclusiveFullScreen);
            Debug.Log($"[螢幕設定] 切換為全螢幕: {nativeRes.width} x {nativeRes.height}");
        }
        else
        {
            // === 視窗模式 ===
            Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
            Debug.Log($"[螢幕設定] 切換為視窗: {windowWidth} x {windowHeight}");
        }
    }
}