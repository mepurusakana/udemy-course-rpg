using UnityEngine;

/// <summary>
/// 角色位置枚舉
/// </summary>
public enum CharacterPosition
{
    Left,   // 左邊
    Right   // 右邊
}

[System.Serializable]
public class DialogueData
{
    [Header("對話內容")]
    public string characterName;      // 角色名稱
    public Sprite characterImage;     // 角色圖片
    [TextArea(2, 5)]
    public string dialogueText;       // 對話文字

    [Header("視覺設定")]
    public Color nameTagColor = Color.white;      // 名牌顏色
    public CharacterPosition characterPosition = CharacterPosition.Left;  // 角色位置（左/右）
}