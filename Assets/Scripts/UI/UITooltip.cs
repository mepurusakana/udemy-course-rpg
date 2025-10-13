using UnityEngine;
using UnityEngine.UI;      // RawImage, LayoutElement, VerticalLayoutGroup
using UnityEngine.Video;   // VideoPlayer
using TMPro;               // TMP_Text

public class UITooltip : MonoBehaviour
{
    public static UITooltip Instance { get; private set; }

    // ====== UI 元件 ======
    [Header("UI 元件")]
    [SerializeField] private Canvas rootCanvas;          // 外層 Canvas（自動抓也可）
    [SerializeField] private RectTransform panel;        // Tooltip 容器（需掛 VLG + CSF）
    [SerializeField] private CanvasGroup cg;             // 穿透 & 透明度
    [SerializeField] private TMP_Text descriptionText;   // 文字（永遠在上方）
    [SerializeField] private RawImage videoSurface;      // 影片（永遠在下方）

    // ====== 版面排版 ======
    [Header("版面排版 (Panel 需掛 VerticalLayoutGroup + ContentSizeFitter)")]
    [SerializeField] private VerticalLayoutGroup vlg;    // Panel 上的 VLG
    [SerializeField] private LayoutElement videoSurfaceLE;// VideoSurface 的 LayoutElement
    [SerializeField] private float layoutSpacing = 4f;   // 文字與影片的間距

    // ====== 影片播放器 ======
    [Header("影片播放器")]
    [SerializeField] private VideoPlayer videoPlayer;

    // ====== 行為設定 ======
    [Header("顯示與跟隨")]
    [SerializeField] private bool followMouse = true;
    [SerializeField] private Vector2 pixelOffset = new Vector2(16f, -16f);
    [SerializeField] private float screenSafe = 8f;

    [Header("尺寸設定")]
    [Tooltip("固定顯示尺寸（推薦開啟）。開啟後忽略影片實際寬高。")]
    [SerializeField] private bool forceConstantVideoSize = true;
    [SerializeField] private Vector2 constantVideoSize = new Vector2(640, 360);
    [Tooltip("若未固定尺寸，取不到影片寬高時的保底尺寸")]
    [SerializeField] private Vector2 fallbackVideoSize = new Vector2(640, 360);

    [Header("偵錯")]
    [SerializeField] private bool debugLog = false;

    // ====== 內部狀態 ======
    private RenderTexture rt;
    private string curDesc;
    private VideoClip curClip;

    private bool playerReady = false;
    private bool preparing = false;

    // ========================== Unity 生命週期 ==========================
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

        if (cg != null)
        {
            cg.ignoreParentGroups = true;
            cg.alpha = 1f;
            cg.interactable = false;   // 不可互動
            cg.blocksRaycasts = false; // 不吃事件（避免滑到 Tooltip 被判定離開圖片）
        }

        if (vlg != null)
        {
            vlg.spacing = layoutSpacing;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        }

        EnsureLayoutParentsAndOrder();   // 文字在上、影片在下
        FixVideoRectTransform();         // 避免 Stretch
        EnsureRenderTexture();           // 綁好 RT

        gameObject.SetActive(false);
        curDesc = null; curClip = null;
    }

    private void Update()
    {
        if (followMouse && gameObject.activeSelf)
            SetScreenPosition(Input.mousePosition);
    }

    // ============================ Public API ============================
    /// <summary>顯示 Tooltip（文字永遠在上、影片永遠在下；不重疊、尺寸固定）</summary>
    public void Show(Vector2 screenPos, string desc, VideoClip clip, bool _ignoredVideoOnTop)
    {
        // 影片位置永遠在文字下方，故參數 _ignoredVideoOnTop 直接忽略
        EnsureLayoutParentsAndOrder();
        FixVideoRectTransform();

        bool needUpdate = (curDesc != desc) || (curClip != clip);

        if (needUpdate)
        {
            curDesc = desc;
            curClip = clip;

            // 文字
            if (descriptionText != null)
            {
                descriptionText.text = curDesc ?? string.Empty;
                var col = descriptionText.color; col.a = 1f;
                descriptionText.color = col;
                descriptionText.enabled = true;
                descriptionText.gameObject.SetActive(true);
            }
        }

        // 確保影片節點可見與在正確位置（panel 的第二個孩子）
        if (videoSurface != null && panel != null)
        {
            if (videoSurface.rectTransform.parent != panel)
                videoSurface.rectTransform.SetParent(panel, false);

            // 保證層級：0=文字, 1=影片
            if (descriptionText) descriptionText.rectTransform.SetSiblingIndex(0);
            videoSurface.rectTransform.SetSiblingIndex(1);

            bool hasClip = (clip != null);
            videoSurface.gameObject.SetActive(hasClip);
        }

        // 綁定 RT + 播放
        EnsureRenderTexture();
        if (videoPlayer != null)
        {
            if (clip != null)
            {
                if (videoPlayer.clip != clip || !videoPlayer.isPlaying)
                    PlayClipSafely(clip);
            }
            else
            {
                StopAndClear();
            }
        }

        // 每次顯示都鎖尺寸（確保一致且讓 VLG 正確排版）
        ApplyVideoSize();

        // 顯示與定位
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        SetScreenPosition(screenPos);
        if (panel != null) panel.SetAsLastSibling();

        if (debugLog) Debug.Log("[UITooltip] Show done", this);
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;

        CancelInvoke(nameof(FallbackPlay));
        preparing = false;
        playerReady = false;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        gameObject.SetActive(false);

        if (debugLog) Debug.Log("[UITooltip] Hide", this);
    }

    /// <summary>設定 Tooltip 的螢幕座標（自動避開邊界）</summary>
    public void SetScreenPosition(Vector2 screenPos)
    {
        if (rootCanvas == null || panel == null) return;

        // 更新 Layout 拿到正確尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector2 localMouse
        );

        Vector2 size = panel.rect.size;
        Rect canvasBounds = canvasRect.rect;

        // 預設放右下
        Vector2 preferredPos = localMouse + pixelOffset;

        // 動態 pivot 避免超出
        Vector2 newPivot = panel.pivot;
        if (preferredPos.x + size.x * (1f - panel.pivot.x) > canvasBounds.xMax - screenSafe) newPivot.x = 1f;
        else newPivot.x = 0f;

        if (preferredPos.y + size.y * (1f - panel.pivot.y) > canvasBounds.yMax - screenSafe) newPivot.y = 1f;
        else newPivot.y = 1f;

        panel.pivot = newPivot;

        // 夾在安全邊界內
        float left = canvasBounds.xMin + screenSafe + size.x * panel.pivot.x;
        float right = canvasBounds.xMax - screenSafe - size.x * (1f - panel.pivot.x);
        float bottom = canvasBounds.yMin + screenSafe + size.y * panel.pivot.y;
        float top = canvasBounds.yMax - screenSafe - size.y * (1f - panel.pivot.y);

        preferredPos.x = Mathf.Clamp(preferredPos.x, left, right);
        preferredPos.y = Mathf.Clamp(preferredPos.y, bottom, top);

        panel.anchoredPosition = preferredPos;
    }

    // ============================ 私有方法 ============================
    /// <summary>保證 0=文字、1=影片 的同層順序（VLG 會垂直排版，不重疊）</summary>
    private void EnsureLayoutParentsAndOrder()
    {
        if (panel == null) return;

        if (descriptionText && descriptionText.rectTransform.parent != panel)
            descriptionText.rectTransform.SetParent(panel, false);
        if (videoSurface && videoSurface.rectTransform.parent != panel)
            videoSurface.rectTransform.SetParent(panel, false);

        if (descriptionText) descriptionText.rectTransform.SetSiblingIndex(0);
        if (videoSurface) videoSurface.rectTransform.SetSiblingIndex(1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
    }

    /// <summary>把 VideoSurface 的錨點固定於中心，避免 Stretch 與 Layout 打架</summary>
    private void FixVideoRectTransform()
    {
        if (videoSurface == null) return;
        var rtVideo = videoSurface.rectTransform;

        rtVideo.anchorMin = rtVideo.anchorMax = new Vector2(0.5f, 0.5f);
        rtVideo.pivot = new Vector2(0.5f, 0.5f);
        rtVideo.anchoredPosition = Vector2.zero;
    }

    /// <summary>建立/維護 RenderTexture，並綁定到 RawImage 與 VideoPlayer</summary>
    private void EnsureRenderTexture()
    {
        if (videoSurface == null) return;

        // 選 RT 尺寸：固定 or 影片寬高 or 保底
        int w = Mathf.RoundToInt(forceConstantVideoSize ? constantVideoSize.x : fallbackVideoSize.x);
        int h = Mathf.RoundToInt(forceConstantVideoSize ? constantVideoSize.y : fallbackVideoSize.y);

        if (!forceConstantVideoSize && videoPlayer != null && videoPlayer.clip != null)
        {
            if (videoPlayer.clip.width > 0) w = (int)videoPlayer.clip.width;
            if (videoPlayer.clip.height > 0) h = (int)videoPlayer.clip.height;
        }

        bool needNew = (rt == null) || videoSurface.texture == null || !(videoSurface.texture is RenderTexture);
        if (needNew || (rt != null && (rt.width != w || rt.height != h)))
        {
            if (rt != null)
            {
                if (videoPlayer != null && videoPlayer.targetTexture == rt) videoPlayer.targetTexture = null;
                rt.Release(); Destroy(rt);
            }

            rt = new RenderTexture(Mathf.Max(2, w), Mathf.Max(2, h), 0, RenderTextureFormat.ARGB32)
            {
                name = "UITooltip_RT"
            };
            rt.Create();

            videoSurface.texture = rt;

            if (videoPlayer != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = rt;
            }
        }

        ApplyVideoSize();
    }

    /// <summary>鎖定影片顯示尺寸（每次顯示必呼叫）</summary>
    private void ApplyVideoSize()
    {
        if (videoSurface == null) return;

        int w, h;
        if (forceConstantVideoSize)
        {
            w = Mathf.RoundToInt(Mathf.Max(2f, constantVideoSize.x));
            h = Mathf.RoundToInt(Mathf.Max(2f, constantVideoSize.y));
        }
        else
        {
            int rw = (rt != null && rt.width > 0) ? rt.width : Mathf.RoundToInt(fallbackVideoSize.x);
            int rh = (rt != null && rt.height > 0) ? rt.height : Mathf.RoundToInt(fallbackVideoSize.y);
            w = Mathf.Max(2, rw);
            h = Mathf.Max(2, rh);
        }

        // 用 LayoutElement 鎖住尺寸，VLG 會依此排版
        if (videoSurfaceLE != null)
        {
            videoSurfaceLE.minWidth = videoSurfaceLE.preferredWidth = w;
            videoSurfaceLE.minHeight = videoSurfaceLE.preferredHeight = h;
            videoSurfaceLE.flexibleWidth = 0f;
            videoSurfaceLE.flexibleHeight = 0f;
        }

        // 同步 RectTransform（雙重保險）
        var rtVideo = videoSurface.rectTransform;
        rtVideo.anchorMin = rtVideo.anchorMax = new Vector2(0.5f, 0.5f);
        rtVideo.pivot = new Vector2(0.5f, 0.5f);
        rtVideo.anchoredPosition = Vector2.zero;
        rtVideo.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        rtVideo.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // 保證可見
        var c = videoSurface.color; c.a = 1f; videoSurface.color = c;

        // 立刻重排，確保與文字上下排列、不重疊
        if (panel != null) LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        if (debugLog) Debug.Log($"[UITooltip] ApplyVideoSize -> {w}x{h}", this);
    }

    /// <summary>安全播放（含 Prepare 與備援）</summary>
    private void PlayClipSafely(VideoClip clip)
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying) videoPlayer.Stop();
        CancelInvoke(nameof(FallbackPlay));
        preparing = true;
        playerReady = false;

        EnsureRenderTexture();

        // 避免重複訂閱
        videoPlayer.prepareCompleted -= OnPreparedPlay;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.prepareCompleted += OnPreparedPlay;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.url = string.Empty; // 比 null 更穩

        videoPlayer.Prepare();

        // 預防個別平台不回呼 prepareCompleted
        Invoke(nameof(FallbackPlay), 0.5f);

        videoSurface.gameObject.SetActive(true);
        ApplyVideoSize();

        if (debugLog) Debug.Log($"[UITooltip] Prepare clip={clip.name}", this);
    }

    private void OnPreparedPlay(VideoPlayer vp)
    {
        preparing = false; playerReady = true; vp.Play();
        if (debugLog) Debug.Log("[UITooltip] prepareCompleted -> Play()", this);
    }

    private void FallbackPlay()
    {
        if (videoPlayer == null) return;
        if (!playerReady && preparing)
        {
            videoPlayer.Play();
            if (debugLog) Debug.Log("[UITooltip] FallbackPlay() -> Play()", this);
        }
    }

    private void OnVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogError($"[UITooltip] Video error: {msg}", this);
        StopAndClear();
    }

    private void StopAndClear()
    {
        CancelInvoke(nameof(FallbackPlay));
        preparing = false;
        playerReady = false;

        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            videoPlayer.prepareCompleted -= OnPreparedPlay;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.clip = null;
            videoPlayer.url = string.Empty;
        }

        if (videoSurface != null)
            videoSurface.gameObject.SetActive(false);
    }
}
