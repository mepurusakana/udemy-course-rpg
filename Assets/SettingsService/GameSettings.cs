using System;
using UnityEngine;

// 統一存的設定資料（會被序列化到 settings.json）
[Serializable]
public class GameSettings
{
    // 音量（0~1）
    public float master = 1f;
    public float bgm = 0.8f;
    public float sfx = 0.8f;

    // 影像
    public int qualityIndex = 2;                 // 0=低,1=中,2=高 （你現在就是三段）
    public int vSyncCount = 1;                   // 0=關,1=開(1),2=開(2)
    public int resolutionIndex = -1;             // -1=維持目前解析度
    public FullScreenMode screenMode = FullScreenMode.Windowed; // 視窗/全螢幕模式

    // 亮度（0=最亮、1=最暗）：你的黑圖 alpha
    public float brightness = 0.3f;
}
