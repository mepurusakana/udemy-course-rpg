using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Dialogue Data/New Line Data", fileName = "Line - ")]
public class DialogueLineSO : ScriptableObject
{   
    [Header("Dialogue info")]
    public string dialogueGroupName;
    public DialogueSpeaker speaker;   // ← 型別要跟上面完全一樣

    [Header("Text options")]
    [TextArea] public string[] textLine;
}