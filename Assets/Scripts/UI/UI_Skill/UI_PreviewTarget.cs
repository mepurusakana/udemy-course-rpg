using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using TMPro; // 新增：使用 TMP_FontAsset

public class UI_PreviewTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("內容")]
    [TextArea] public string description;
    public VideoClip previewClip;
    public bool videoOnTop = false;

    [Header("字型（可選）")]
    [Tooltip("指定此預覽 Tooltip 的描述文字要使用的 TMP 字型資產；留空則使用預設字型")]
    [SerializeField] private TMP_FontAsset descriptionFont;

    [Tooltip("勾選後會用 <font=\"資產名\"> 包住整段 description；需確保該 Font Asset 能被 TMP 以名稱解析")]
    [SerializeField] private bool wrapWithRichTextFontTag = true;

    private bool isInside = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isInside = true;

        //  依需求將描述文字包上指定字型
        string finalDesc = BuildDescriptionWithFont(description);

        Debug.Log($"[UI_PreviewTarget] ENTER obj='{name}' clip={(previewClip ? previewClip.name : "null")}");
        UITooltip.Instance?.Show(eventData.position, finalDesc, previewClip, videoOnTop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isInside = false;
        Debug.Log($"[UI_PreviewTarget] EXIT  obj='{name}'");
        UITooltip.Instance?.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isInside)
            UITooltip.Instance?.SetScreenPosition(eventData.position);
    }

    private void OnDisable()
    {
        if (isInside)
        {
            UITooltip.Instance?.Hide();
            isInside = false;
        }
    }

    // ================== 私有工具：把描述包上 <font> ==================
    private string BuildDescriptionWithFont(string raw)
    {
        if (!wrapWithRichTextFontTag || descriptionFont == null || string.IsNullOrEmpty(raw))
            return raw;

        // 以 Font Asset 名稱套用；確保該資產能被 TMP 以名稱解析（放 Resources/、或加入 TMP 資源清單）
        // 也可在這裡加上其他 Rich Text，如 <size>、<color>...
        return $"<font=\"{descriptionFont.name}\">{raw}</font>";
    }

    // （可選）提供程式動態指定字型的 API
    public void SetDescriptionFont(TMP_FontAsset font, bool useFontTag = true)
    {
        descriptionFont = font;
        wrapWithRichTextFontTag = useFontTag;
    }
}
