using UnityEngine;

/// <summary>
/// 角色位置枚舉
/// </summary>
public enum CharacterPosition
{
    Left,
    Right
}

/// <summary>
/// 對話框位置（每一句可獨立設定）
/// </summary>
public enum DialogueBoxPosition
{
    FollowCharacter, // 跟隨角色位置（推薦預設）
    Left,            // 強制對話框到左側
    Right,           // 強制對話框到右側
    Keep             // 保持目前位置不移動
}

[System.Serializable]
public class DialogueData
{
    [Header("對話內容")]
    public string characterName;
    public Sprite characterImage;

    [TextArea(2, 5)]
    public string dialogueText;

    [Header("視覺設定")]
    public Color nameTagColor = Color.white;
    public CharacterPosition characterPosition = CharacterPosition.Left;

    [Header("對話框位置（每句獨立）")]
    public DialogueBoxPosition dialogueBoxPosition = DialogueBoxPosition.FollowCharacter;
}
