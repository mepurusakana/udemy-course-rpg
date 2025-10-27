using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 掛在共同父物件（例如 BottomButtons）上：
/// - 集中管理多顆按鈕的「指到/離開 → 字體與大小切換」
/// - 倒影(LabelReflection) 會與本字(Label)同步字體與大小
/// - 自動在各按鈕上加一個 HoverRelay 來轉送 Pointer/Select 事件
/// 提示：建議另外搭配你的 MultiTextReflection / TextButtonReflection 保持倒影鏡像與漸層
/// </summary>
[DisallowMultipleComponent]
public class MultiButtonHoverFontSwap : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Header("這一顆按鈕")]
        public Button button;

        [Header("文字與倒影（TMP）")]
        public TextMeshProUGUI label;          // 本字
        public TextMeshProUGUI reflection;     // 倒影（若留空將自動在該按鈕下找名為 LabelReflection）

        [Header("一般狀態")]
        public TMP_FontAsset normalFont;        // 不填＝用執行時讀到的原字體
        public float normalFontSize = 0f;       // <=0 用執行時讀到的原大小

        [Header("指到/選取狀態")]
        public TMP_FontAsset hoverFont;         // 不填＝沿用一般字體
        public float hoverFontSize = 0f;        // <=0 = 一般大小 * hoverSizeMultiplier
        [Range(1f, 2f)] public float hoverSizeMultiplier = 1.12f;

        [Header("動畫")]
        public bool animateSize = true;
        public float sizeInTime = 0.12f;        // 指到漸變時間
        public float sizeOutTime = 0.10f;       // 離開漸變時間
        public bool useUnscaledTime = true;

        [Header("鍵盤/手把選取也要觸發")]
        public bool respondToSelection = true;

        // --- 內部快取 ---
        [HideInInspector] public TMP_FontAsset _origLabelFont, _origRefFont;
        [HideInInspector] public float _origLabelSize, _origRefSize;
        [HideInInspector] public bool _captured;
        [HideInInspector] public bool _hovered;
        [HideInInspector] public Coroutine _anim;
    }

    [Header("要管理的按鈕清單（四組或更多）")]
    public List<Entry> entries = new List<Entry>(4);

    // ===== 生命週期 =====
    void Awake()
    {
        // 初始化與自動掛事件中繼
        for (int i = 0; i < entries.Count; i++)
        {
            SetupEntry(i);
            ApplyInstant(i, false); // 一律先回一般狀態
        }
    }

    void OnDisable()
    {
        // 保證停用時全部恢復一般狀態
        for (int i = 0; i < entries.Count; i++)
        {
            StopAnim(i);
            ApplyInstant(i, false);
        }
    }

    // ===== 公開：切換某顆的 hover 狀態（由中繼呼叫）=====
    public void SetHover(int index, bool on)
    {
        if (index < 0 || index >= entries.Count) return;
        var e = entries[index];
        if (e == null) return;
        if (e._hovered == on) return;
        e._hovered = on;

        // 先換字體（大小用補間）
        TMP_FontAsset targetLabelFont = on ? (e.hoverFont ? e.hoverFont : ResolveNormalFont(e))
                                          : ResolveNormalFont(e);
        TMP_FontAsset targetRefFont = on ? (e.hoverFont ? e.hoverFont : ResolveNormalFont(e, useRef: true))
                                          : ResolveNormalFont(e, useRef: true);

        if (e.label) e.label.font = targetLabelFont;
        if (e.reflection) e.reflection.font = targetRefFont;

        float baseNormal = ResolveNormalSize(e);
        float targetHover = e.hoverFontSize > 0f ? e.hoverFontSize : baseNormal * Mathf.Max(1f, e.hoverSizeMultiplier);

        float fromSize = e.label ? e.label.fontSize : baseNormal;
        float toSize = on ? targetHover : baseNormal;

        if (!e.animateSize)
        {
            SetSizes(e, toSize);
        }
        else
        {
            StopAnim(index);
            e._anim = StartCoroutine(AnimateSize(e, fromSize, toSize, on ? e.sizeInTime : e.sizeOutTime));
        }
    }

    // ===== 內部：初始化單顆 =====
    void SetupEntry(int i)
    {
        var e = entries[i];
        if (e == null || e.button == null) return;

        // 自動找 Label / LabelReflection
        if (e.label == null)
        {
            var t = e.button.transform.Find("Label");
            if (t) e.label = t.GetComponent<TextMeshProUGUI>();
        }
        if (e.reflection == null)
        {
            var t = e.button.transform.Find("LabelReflection");
            if (t) e.reflection = t.GetComponent<TextMeshProUGUI>();
        }

        // 捕捉原字體/大小
        CaptureOriginals(e);

        // 關閉 Raycast Target，點擊交由 Button
        if (e.label) e.label.raycastTarget = false;
        if (e.reflection) e.reflection.raycastTarget = false;

        // 在按鈕上加事件中繼（自動）
        var relay = e.button.GetComponent<HoverRelay>();
        if (relay == null) relay = e.button.gameObject.AddComponent<HoverRelay>();
        relay.Bind(this, i, e.respondToSelection);
    }

    void CaptureOriginals(Entry e)
    {
        if (e._captured) return;
        if (e.label)
        {
            e._origLabelFont = e.label.font;
            e._origLabelSize = e.label.fontSize;
        }
        if (e.reflection)
        {
            e._origRefFont = e.reflection.font;
            e._origRefSize = e.reflection.fontSize;
        }
        e._captured = true;
    }

    // 立即套某顆：一般或指到狀態
    void ApplyInstant(int index, bool hover)
    {
        if (index < 0 || index >= entries.Count) return;
        var e = entries[index];
        if (e == null) return;
        CaptureOriginals(e);

        TMP_FontAsset fontLabel = hover ? (e.hoverFont ? e.hoverFont : ResolveNormalFont(e))
                                        : ResolveNormalFont(e);
        TMP_FontAsset fontRef = hover ? (e.hoverFont ? e.hoverFont : ResolveNormalFont(e, useRef: true))
                                        : ResolveNormalFont(e, useRef: true);

        if (e.label) e.label.font = fontLabel;
        if (e.reflection) e.reflection.font = fontRef;

        float baseNormal = ResolveNormalSize(e);
        float targetHover = e.hoverFontSize > 0f ? e.hoverFontSize : baseNormal * Mathf.Max(1f, e.hoverSizeMultiplier);
        float size = hover ? targetHover : baseNormal;

        SetSizes(e, size);
        e._hovered = hover;
    }

    // 解析一般狀態字體
    TMP_FontAsset ResolveNormalFont(Entry e, bool useRef = false)
    {
        if (e.normalFont) return e.normalFont;
        if (useRef)
            return e._origRefFont ? e._origRefFont : e._origLabelFont;
        else
            return e._origLabelFont;
    }

    // 解析一般狀態大小
    float ResolveNormalSize(Entry e)
    {
        if (e.normalFontSize > 0f) return e.normalFontSize;
        return e._origLabelSize > 0f ? e._origLabelSize : (e.label ? e.label.fontSize : 24f);
    }

    void SetSizes(Entry e, float size)
    {
        if (e.label) e.label.fontSize = size;
        if (e.reflection) e.reflection.fontSize = size; // 倒影跟著同大小
    }

    IEnumerator AnimateSize(Entry e, float from, float to, float dur)
    {
        dur = Mathf.Max(0.0001f, dur);
        float t = 0f;
        float Dt() => e.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        while (t < dur)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / dur);
            // SmoothStep 曲線
            float eased = k * k * (3f - 2f * k);
            float s = Mathf.Lerp(from, to, eased);
            SetSizes(e, s);
            yield return null;
        }
        SetSizes(e, to);
        e._anim = null;
    }

    void StopAnim(int index)
    {
        var e = entries[index];
        if (e != null && e._anim != null)
        {
            StopCoroutine(e._anim);
            e._anim = null;
        }
    }

    // ===== 事件中繼：自動加到每顆按鈕上 =====
    private class HoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private MultiButtonHoverFontSwap owner;
        private int idx;
        private bool respondSelect;

        public void Bind(MultiButtonHoverFontSwap o, int index, bool respondSelection)
        {
            owner = o;
            idx = index;
            respondSelect = respondSelection;
        }

        public void OnPointerEnter(PointerEventData eventData) => owner?.SetHover(idx, true);
        public void OnPointerExit(PointerEventData eventData) => owner?.SetHover(idx, false);

        public void OnSelect(BaseEventData eventData)
        {
            if (respondSelect) owner?.SetHover(idx, true);
        }
        public void OnDeselect(BaseEventData eventData)
        {
            if (respondSelect) owner?.SetHover(idx, false);
        }
    }

    // ===== 方便綁定：自動掃描子階層四顆按鈕 =====
    [ContextMenu("Auto Bind First 4 Buttons (Label/LabelReflection)")]
    public void AutoBindFirst4()
    {
        entries ??= new List<Entry>();
        if (entries.Count < 4)
        {
            for (int i = entries.Count; i < 4; i++) entries.Add(new Entry());
        }

        int idx = 0;
        foreach (Transform child in transform)
        {
            if (idx >= 4) break;
            var btn = child.GetComponent<Button>();
            if (btn == null) continue;

            entries[idx].button = btn;

            if (entries[idx].label == null)
            {
                var t = child.Find("Label");
                if (t) entries[idx].label = t.GetComponent<TextMeshProUGUI>();
            }
            if (entries[idx].reflection == null)
            {
                var t = child.Find("LabelReflection");
                if (t) entries[idx].reflection = t.GetComponent<TextMeshProUGUI>();
            }
            idx++;
        }

        // 重新初始化
        Awake();
    }
}
