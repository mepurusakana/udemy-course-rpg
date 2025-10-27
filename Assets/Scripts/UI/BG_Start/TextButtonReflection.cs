using UnityEngine;
using TMPro;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class TextButtonReflection : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Header("這一組的文字物件")]
        public TextMeshProUGUI label;        // 正常文字（必填）
        public TextMeshProUGUI reflection;   // 倒影文字（必填，建議命名 LabelReflection）

        [Header("是否覆寫全域樣式")]
        public bool overrideStyle = false;

        [Header("倒影樣式（覆寫時生效）")]
        [Range(0f, 1f)] public float topAlpha = 0.35f;    // 倒影靠近本字的透明度
        [Range(0f, 1f)] public float bottomAlpha = 0.0f;  // 倒影底部透明度
        [Min(0.2f)] public float heightScale = 1.0f;   // 倒影高度（1 = 等高；<1 = 較短）

        [Header("維持同步（內容）")]
        public bool keepSynced = true;                   // 是否每幀檢查 label 文字並同步到倒影

        [HideInInspector] public string _lastText;       // 內部快取
        [HideInInspector] public Color _lastColor;
        [HideInInspector] public float _lastFontSize;
        [HideInInspector] public bool _styleInited;
    }

    [Header("四組（或更多）文字與倒影")]
    public List<Entry> entries = new List<Entry>(4);

    [Header("全域倒影樣式（未覆寫者套用）")]
    [Range(0f, 1f)] public float globalTopAlpha = 0.35f;
    [Range(0f, 1f)] public float globalBottomAlpha = 0.0f;
    [Min(0.2f)] public float globalHeightScale = 1.0f;

    [Header("套用時機")]
    public bool applyOnAwake = true;         // 進場套一次
    public bool autoSyncBasicStyle = true;   // 輕量同步：若偵測到字色/字體大小有變更就更新倒影樣式

    void Awake()
    {
        if (applyOnAwake) ApplyAll(); // 初始化：鏡像+漸層
    }

    void OnValidate()
    {
        // 在 Inspector 調參時即時預覽
        ApplyAll();
    }

    void LateUpdate()
    {
        // 1) 同步文字內容（非常便宜）
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e == null || e.label == null || e.reflection == null) continue;

            if (e.keepSynced && e._lastText != e.label.text)
            {
                e.reflection.text = e.label.text;
                e._lastText = e.label.text;
            }

            // 2) 輕量同步樣式（只在顏色/字體大小變動時更新）
            if (autoSyncBasicStyle)
            {
                if (!e._styleInited ||
                    e._lastColor != e.label.color ||
                    Mathf.Abs(e._lastFontSize - e.label.fontSize) > 0.001f)
                {
                    ApplyStyle(e);
                }
            }
        }
    }

    /// <summary>對所有組套用：鏡像、高度、漸層、初始樣式</summary>
    public void ApplyAll()
    {
        if (entries == null) return;
        for (int i = 0; i < entries.Count; i++)
        {
            ApplyOne(entries[i]);
        }
    }

    void ApplyOne(Entry e)
    {
        if (e == null || e.label == null || e.reflection == null) return;

        // 文字先同步一次
        e.reflection.text = e.label.text;
        e._lastText = e.label.text;

        // 完整樣式複製
        e.reflection.font = e.label.font;
        e.reflection.fontSize = e.label.fontSize;
        e.reflection.fontStyle = e.label.fontStyle;
        e.reflection.alignment = e.label.alignment;
        e.reflection.characterSpacing = e.label.characterSpacing;
        e.reflection.wordSpacing = e.label.wordSpacing;
        e.reflection.lineSpacing = e.label.lineSpacing;
        e.reflection.enableWordWrapping = e.label.enableWordWrapping;

        // UI 點擊交給 Button
        e.label.raycastTarget = false;
        e.reflection.raycastTarget = false;

        // 鏡像 + 高度縮放（Y 要為負）
        float h = e.overrideStyle ? Mathf.Max(0.2f, e.heightScale)
                                  : Mathf.Max(0.2f, globalHeightScale);
        var rt = e.reflection.rectTransform;
        var s = rt.localScale;
        s.x = Mathf.Abs(s.x);
        s.y = -Mathf.Abs(h);
        s.z = 1f;
        rt.localScale = s;

        // 透明漸層（上 → 下）
        ApplyGradient(e);

        // 快取目前顏色與字體大小
        e._lastColor = e.label.color;
        e._lastFontSize = e.label.fontSize;
        e._styleInited = true;
    }

    void ApplyStyle(Entry e)
    {
        if (e == null || e.label == null || e.reflection == null) return;

        // 輕量同步：顏色與字體大小（字體/間距通常較少改，完整更動時再手動點 ApplyAll）
        e.reflection.fontSize = e.label.fontSize;
        ApplyGradient(e);

        e._lastColor = e.label.color;
        e._lastFontSize = e.label.fontSize;
        e._styleInited = true;
    }

    void ApplyGradient(Entry e)
    {
        var baseC = e.label.color;
        float ta = e.overrideStyle ? e.topAlpha : globalTopAlpha;
        float ba = e.overrideStyle ? e.bottomAlpha : globalBottomAlpha;

        Color topC = new Color(baseC.r, baseC.g, baseC.b, Mathf.Clamp01(ta));
        Color botC = new Color(baseC.r, baseC.g, baseC.b, Mathf.Clamp01(ba));

        e.reflection.enableVertexGradient = true;
        e.reflection.colorGradient = new VertexGradient(topC, topC, botC, botC);
    }

    // 便捷：自動綁前四組（依子物件名 Label / LabelReflection）
    [ContextMenu("Auto Bind First 4 Pairs (by name)")]
    public void AutoBindFirst4Pairs()
    {
        entries ??= new List<Entry>();
        if (entries.Count < 4)
        {
            for (int i = entries.Count; i < 4; i++)
                entries.Add(new Entry());
        }

        int groupIdx = 0;
        foreach (Transform child in transform)
        {
            if (groupIdx >= 4) break;

            var label = child.Find("Label") ? child.Find("Label").GetComponent<TextMeshProUGUI>() : null;
            var refl = child.Find("LabelReflection") ? child.Find("LabelReflection").GetComponent<TextMeshProUGUI>() : null;

            if (label != null && refl != null)
            {
                entries[groupIdx].label = label;
                entries[groupIdx].reflection = refl;
                groupIdx++;
            }
        }

        ApplyAll();
    }
}
