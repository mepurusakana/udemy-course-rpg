using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// 掛在「每一顆按鈕（Button 所在物件）」上：
/// - 滑鼠指到 / 鍵盤選取：更換字體並把「文字與倒影」的字體大小漸變到目標值
/// - 滑鼠離開 / 取消選取：恢復原本字體與大小
/// 提示：為了讓倒影方向/漸層維持正確，請持續使用你現有的 MultiTextReflection（或單顆 TextButtonReflection）。
/// </summary>
[DisallowMultipleComponent]
public class ButtonHoverFontSwap : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("要控制的文字（本字 + 倒影）")]
    public TextMeshProUGUI label;            // 本字（必填）
    public TextMeshProUGUI reflection;       // 倒影（可留空；會嘗試自動尋找名為 LabelReflection）

    [Header("一般狀態（未指到）")]
    public TMP_FontAsset normalFont;         // 不指定時，使用執行時讀到的原字體
    public float normalFontSize = 0f;        // <=0 表示使用執行時讀到的原大小

    [Header("指到/選取狀態")]
    public TMP_FontAsset hoverFont;          // 指到時要切換的字體（可留空=沿用一般字體）
    public float hoverFontSize = 0f;         // <=0 表示使用「一般大小 * hoverSizeMultiplier」
    [Range(1f, 2f)] public float hoverSizeMultiplier = 1.12f;

    [Header("動畫")]
    public bool animateSize = true;
    public float sizeInTime = 0.12f;         // 指到時大小漸變時間（秒）
    public float sizeOutTime = 0.10f;        // 離開時大小漸變時間（秒）
    public bool useUnscaledTime = true;      // UI 推薦：不受 Time.timeScale 影響

    [Header("是否也響應鍵盤/手把選取")]
    public bool respondToSelection = true;

    // 內部狀態
    TMP_FontAsset _origLabelFont, _origRefFont;
    float _origLabelSize, _origRefSize;
    bool _captured = false;

    Coroutine _anim;
    bool _hovered = false;

    void Reset()
    {
        if (label == null)
        {
            var t = transform.Find("Label");
            if (t) label = t.GetComponent<TextMeshProUGUI>();
        }
        if (reflection == null)
        {
            var t = transform.Find("LabelReflection");
            if (t) reflection = t.GetComponent<TextMeshProUGUI>();
        }
    }

    void Awake()
    {
        CaptureOriginals();
        ApplyInstant(false); // 先套回一般狀態（保險）
    }

    void OnEnable()
    {
        // 保證回到一般狀態
        StopAnim();
        ApplyInstant(false);
        _hovered = false;
    }

    void OnDisable()
    {
        StopAnim();
        // 停用時也回復
        ApplyInstant(false);
        _hovered = false;
    }

    // ===== 事件 =====
    public void OnPointerEnter(PointerEventData e) { Hover(true); }
    public void OnPointerExit(PointerEventData e) { Hover(false); }

    public void OnSelect(BaseEventData e)
    {
        if (respondToSelection) Hover(true);
    }
    public void OnDeselect(BaseEventData e)
    {
        if (respondToSelection) Hover(false);
    }

    // ===== 主流程 =====
    void Hover(bool on)
    {
        if (_hovered == on) return;
        _hovered = on;

        if (!_captured) CaptureOriginals();

        // 先切字體（大小用補間動畫）
        var targetFontForLabel = on ? (hoverFont ? hoverFont : ResolveNormalFont()) : ResolveNormalFont();
        var targetFontForRef = on ? (hoverFont ? hoverFont : ResolveNormalFont(_origRefFont)) : ResolveNormalFont(_origRefFont);

        if (label) label.font = targetFontForLabel;
        if (reflection) reflection.font = targetFontForRef;

        // 目標大小
        float baseNormalSize = ResolveNormalSize(label ? label.fontSize : _origLabelSize);
        float targetHoverSize = hoverFontSize > 0f ? hoverFontSize : baseNormalSize * Mathf.Max(1f, hoverSizeMultiplier);

        float fromSize = label ? label.fontSize : baseNormalSize;
        float toSize = on ? targetHoverSize : baseNormalSize;

        if (!animateSize)
        {
            SetSizes(toSize);
        }
        else
        {
            StopAnim();
            _anim = StartCoroutine(AnimateSize(fromSize, toSize, on ? sizeInTime : sizeOutTime));
        }
    }

    void CaptureOriginals()
    {
        if (_captured) return;
        if (label)
        {
            _origLabelFont = label.font;
            _origLabelSize = label.fontSize;
            label.raycastTarget = false; // 點擊交給 Button
        }
        if (reflection)
        {
            _origRefFont = reflection.font;
            _origRefSize = reflection.fontSize;
            reflection.raycastTarget = false;
        }
        _captured = true;
    }

    // 立即套用一般或指到狀態（不動畫）
    void ApplyInstant(bool hover)
    {
        if (!_captured) CaptureOriginals();

        var fontLabel = hover
            ? (hoverFont ? hoverFont : ResolveNormalFont())
            : ResolveNormalFont();
        var fontRef = hover
            ? (hoverFont ? hoverFont : ResolveNormalFont(_origRefFont))
            : ResolveNormalFont(_origRefFont);

        if (label) label.font = fontLabel;
        if (reflection) reflection.font = fontRef;

        float baseNormalSize = ResolveNormalSize(_origLabelSize);
        float targetHoverSize = hoverFontSize > 0f ? hoverFontSize : baseNormalSize * Mathf.Max(1f, hoverSizeMultiplier);
        float size = hover ? targetHoverSize : baseNormalSize;

        SetSizes(size);
    }

    // 幫你處理「未指定 normalFont」時使用原字體
    TMP_FontAsset ResolveNormalFont(TMP_FontAsset fallback = null)
    {
        if (normalFont) return normalFont;
        if (fallback) return fallback;
        return _origLabelFont;
    }

    // 幫你處理「未指定 normalSize」時使用原大小
    float ResolveNormalSize(float defaultSize)
    {
        return normalFontSize > 0f ? normalFontSize : defaultSize;
    }

    void SetSizes(float size)
    {
        if (label) label.fontSize = size;
        if (reflection) reflection.fontSize = size; // 倒影跟著同大小
    }

    IEnumerator AnimateSize(float from, float to, float dur)
    {
        dur = Mathf.Max(0.0001f, dur);
        float t = 0f;
        float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        while (t < dur)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / dur);
            // SmoothStep
            float eased = k * k * (3f - 2f * k);
            float s = Mathf.Lerp(from, to, eased);
            SetSizes(s);
            yield return null;
        }
        SetSizes(to);
        _anim = null;
    }

    void StopAnim()
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = null;
    }
}
