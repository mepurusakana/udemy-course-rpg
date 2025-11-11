using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using TMPro;

public class UI_PreviewTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("=== 預覽內容設定 ===")]
    [Space(5)]
    [TextArea(3, 10)]
    [Tooltip("滑鼠指向時要顯示的描述文字")]
    public string description = "在這裡輸入描述文字";

    [Space(5)]
    [Tooltip("要播放的影片素材")]
    public VideoClip previewVideo;

    [Space(5)]
    [Tooltip("要顯示的靜態圖片(如果沒有影片時使用)")]
    public Sprite previewImage;


    [Header("=== 影片顯示元件 ===")]
    [Space(5)]
    [Tooltip("顯示影片的 RawImage 元件")]
    public RawImage videoRawImage;

    [Space(5)]
    [Tooltip("VideoPlayer 組件")]
    public VideoPlayer videoPlayer;

    [Space(5)]
    [Tooltip("顯示靜態圖片的 Image 元件(沒有影片時使用)")]
    public Image staticImage;


    [Header("=== 文字顯示元件 ===")]
    [Space(5)]
    [Tooltip("顯示文字的 TextMeshProUGUI 元件")]
    public TextMeshProUGUI textDisplay;


    [Header("=== 顯示控制 ===")]
    [Space(5)]
    [Tooltip("滑鼠進入時是否顯示影片元件")]
    public bool showVideoOnHover = true;

    [Space(5)]
    [Tooltip("滑鼠進入時是否顯示文字元件")]
    public bool showTextOnHover = true;


    [Header("=== 文字樣式設定 ===")]
    [Space(5)]
    [Tooltip("文字使用的 TMP 字型資產")]
    public TMP_FontAsset textFont;

    [Space(5)]
    [Tooltip("文字大小")]
    [Range(10, 50)]
    public float fontSize = 16f;

    [Space(5)]
    [Tooltip("文字顏色")]
    public Color textColor = Color.white;

    [Space(5)]
    [Tooltip("文字對齊方式")]
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft;

    [Space(5)]
    [Tooltip("使用富文本標籤包裝字型名稱")]
    public bool useRichTextFontTag = true;


    [Header("=== 進階設定 ===")]
    [Space(5)]
    [Tooltip("影片播放設定")]
    public bool loopVideo = true;

    [Space(5)]
    [Tooltip("是否靜音播放")]
    public bool muteVideo = true;


    // 私有變數
    private bool isHovering = false;
    private RenderTexture videoRenderTexture;

    private void Start()
    {
        // 初始化時隱藏所有元件
        HideAllElements();

        // 設定文字樣式
        SetupTextStyle();

        Debug.Log($"[UI_PreviewTarget] 初始化完成: {gameObject.name}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        ShowPreview();
        Debug.Log($"[UI_PreviewTarget] 滑鼠進入: {gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        HidePreview();
        Debug.Log($"[UI_PreviewTarget] 滑鼠離開: {gameObject.name}");
    }

    private void ShowPreview()
    {
        // 顯示文字
        if (showTextOnHover)
        {
            ShowText();
        }

        // 顯示影片或圖片
        if (showVideoOnHover)
        {
            if (previewVideo != null)
            {
                ShowVideo();
            }
            else if (previewImage != null)
            {
                ShowStaticImage();
            }
        }
    }

    private void HidePreview()
    {
        HideAllElements();
    }

    private void ShowText()
    {
        if (textDisplay == null)
        {
            Debug.LogWarning("[UI_PreviewTarget] Text Display 未設定!");
            return;
        }

        // 顯示文字元件
        textDisplay.gameObject.SetActive(true);

        // 設定文字內容
        string finalText = description;
        if (useRichTextFontTag && textFont != null)
        {
            finalText = $"<font=\"{textFont.name}\">{description}</font>";
        }

        textDisplay.text = finalText;
    }

    private void ShowVideo()
    {
        if (videoRawImage == null || videoPlayer == null)
        {
            Debug.LogWarning("[UI_PreviewTarget] Video RawImage 或 Video Player 未設定!");
            return;
        }

        // 隱藏靜態圖片
        if (staticImage != null)
        {
            staticImage.gameObject.SetActive(false);
        }

        // 顯示影片元件
        videoRawImage.gameObject.SetActive(true);

        // 設定影片播放
        SetupVideoPlayback();

        // 播放影片
        videoPlayer.Play();
    }

    private void ShowStaticImage()
    {
        if (staticImage == null)
        {
            Debug.LogWarning("[UI_PreviewTarget] Static Image 未設定!");
            return;
        }

        // 隱藏影片
        if (videoRawImage != null)
        {
            videoRawImage.gameObject.SetActive(false);
        }

        // 顯示靜態圖片
        staticImage.gameObject.SetActive(true);
        staticImage.sprite = previewImage;
    }

    private void HideAllElements()
    {
        // 隱藏影片元件
        if (videoRawImage != null)
        {
            videoRawImage.gameObject.SetActive(false);
        }

        // 隱藏靜態圖片元件
        if (staticImage != null)
        {
            staticImage.gameObject.SetActive(false);
        }

        // 隱藏文字元件
        if (textDisplay != null)
        {
            textDisplay.gameObject.SetActive(false);
        }

        // 停止影片播放
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }

    private void SetupVideoPlayback()
    {
        if (videoPlayer == null || previewVideo == null) return;

        // 如果已經有 RenderTexture 且尺寸不同,則重新創建
        if (videoRenderTexture != null &&
            (videoRenderTexture.width != (int)previewVideo.width ||
             videoRenderTexture.height != (int)previewVideo.height))
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
            videoRenderTexture = null;
        }

        // 創建 RenderTexture
        if (videoRenderTexture == null)
        {
            videoRenderTexture = new RenderTexture(
                (int)previewVideo.width,
                (int)previewVideo.height,
                0
            );
        }

        // 設定 VideoPlayer
        videoPlayer.clip = previewVideo;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.playOnAwake = false;

        // 設定音量
        if (muteVideo)
        {
            videoPlayer.SetDirectAudioMute(0, true);
        }

        // 設定 RawImage
        videoRawImage.texture = videoRenderTexture;
    }

    private void SetupTextStyle()
    {
        if (textDisplay == null) return;

        if (textFont != null)
        {
            textDisplay.font = textFont;
        }

        textDisplay.fontSize = fontSize;
        textDisplay.color = textColor;
        textDisplay.alignment = textAlignment;
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            HidePreview();
            isHovering = false;
        }
    }

    private void OnDestroy()
    {
        // 清理 RenderTexture
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
        }
    }

    // ==================== 公開方法：用於動態調整 ====================

    /// <summary>
    /// 動態設定預覽內容
    /// </summary>
    public void SetPreviewContent(string text, VideoClip video = null, Sprite image = null)
    {
        description = text;
        previewVideo = video;
        previewImage = image;
    }

    /// <summary>
    /// 動態設定文字樣式
    /// </summary>
    public void SetTextStyle(TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment)
    {
        textFont = font;
        fontSize = size;
        textColor = color;
        textAlignment = alignment;
        SetupTextStyle();
    }

    /// <summary>
    /// 動態設定是否顯示影片和文字
    /// </summary>
    public void SetShowOptions(bool showVideo, bool showText)
    {
        showVideoOnHover = showVideo;
        showTextOnHover = showText;
    }

    /// <summary>
    /// 手動顯示預覽
    /// </summary>
    public void ManualShowPreview()
    {
        isHovering = true;
        ShowPreview();
    }

    /// <summary>
    /// 手動隱藏預覽
    /// </summary>
    public void ManualHidePreview()
    {
        isHovering = false;
        HidePreview();
    }
}