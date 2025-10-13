using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class UI_PreviewTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [TextArea] public string description;
    public VideoClip previewClip;
    public bool videoOnTop = false;

    private bool isInside = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isInside = true;
        Debug.Log($"[UI_PreviewTarget] ENTER obj='{name}' clip={(previewClip ? previewClip.name : "null")}");
        UITooltip.Instance?.Show(eventData.position, description, previewClip, videoOnTop);
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
}
