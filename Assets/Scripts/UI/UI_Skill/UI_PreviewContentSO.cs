using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "PreviewContent", menuName = "UI/Preview Content")]
public class PreviewContentSO : ScriptableObject
{
    [TextArea] public string description;
    public VideoClip clip;
    public bool videoOnTop; // true=影片在上方；false=在下方
}
