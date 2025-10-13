using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Speaker", fileName = "Speaker - ")]
public class DialogueSpeaker : ScriptableObject
{
    [Header("基本")]
    public string speakerName;
    public Sprite speakerPortrait;

    [Header("UI 樣式（可選）")]
    public Color nameColor = Color.white;     // 名字顏色
    public Sprite panelSprite;                // 對話框底圖（9-sliced 最佳）
    public Color panelTint = Color.white;     // 對話框顏色
}
