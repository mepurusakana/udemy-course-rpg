using UnityEngine;
using UnityEngine.Video; // 需要引用
[RequireComponent(typeof(RectTransform))]
public class ProximityText_ByTextBounds : MonoBehaviour
{
    [Header("玩家與顯示物件")]
    [Tooltip("玩家 Transform；留空會自動找 Tag=Player")]
    public Transform player;
    [Tooltip("要控制透明度的 CanvasGroup（掛在文字的 Canvas 上）")]
    public CanvasGroup targetGroup;

    [Header("影片控制")]
    [Tooltip("可選：同級的 VideoPlayer，用於同步淡入淡出")]
    public VideoPlayer videoPlayer;
    [Tooltip("影片的 Renderer（如果你用 RawImage 或 MeshRenderer 顯示影片）")]
    public Renderer videoRenderer;
    [Tooltip("影片淡入淡出時間（秒）")]
    public float videoFadeSpeed = 3f;

    [Header("距離判定（基於文字框 Bounds）")]
    public float minDistanceForFullAlpha = 0.5f;
    public float maxDistanceForZeroAlpha = 5f;
    public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("其他設定")]
    public string playerTag = "Player";
    [Range(0f, 0.2f)] public float updateInterval = 0.05f;
    public bool debugLog = false;

    private RectTransform rect;
    private float timer;
    private float currentVideoAlpha = 0f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        if (targetGroup != null)
        {
            targetGroup.alpha = 0f;
            targetGroup.blocksRaycasts = false;
            targetGroup.interactable = false;
        }

        // 初始化影片
        if (videoRenderer != null)
            SetVideoAlpha(0f);
    }

    private void Update()
    {
        if (player == null || targetGroup == null) return;

        // 控制更新頻率
        if (updateInterval > 0f)
        {
            timer += Time.deltaTime;
            if (timer < updateInterval) return;
            timer = 0f;
        }

        // 取得文字框的世界空間範圍
        Bounds b = RectTransformToBounds(rect);

        // 算玩家到文字框最近點的距離
        Vector3 closest = b.ClosestPoint(player.position);
        float dist = Vector3.Distance(player.position, closest);

        // 轉換成透明度
        float a;
        if (dist >= maxDistanceForZeroAlpha) a = 0f;
        else if (dist <= minDistanceForFullAlpha) a = 1f;
        else
        {
            float t = 1f - Mathf.InverseLerp(minDistanceForFullAlpha, maxDistanceForZeroAlpha, dist);
            a = falloffCurve.Evaluate(t);
        }

        // 套用文字透明度
        targetGroup.alpha = a;
        targetGroup.blocksRaycasts = false;
        targetGroup.interactable = false;

        // 套用影片淡入淡出
        UpdateVideoFade(a);

        if (debugLog) Debug.Log($"[ProximityText_ByTextBounds] d={dist:F2}, alpha={a:F2}");
    }

    private void UpdateVideoFade(float targetAlpha)
    {
        if (videoRenderer == null) return;

        // 平滑過渡
        currentVideoAlpha = Mathf.MoveTowards(currentVideoAlpha, targetAlpha, videoFadeSpeed * Time.deltaTime);

        // 更新材質顏色透明度
        SetVideoAlpha(currentVideoAlpha);

        // 控制播放/暫停
        if (videoPlayer != null)
        {
            if (currentVideoAlpha > 0.01f && !videoPlayer.isPlaying)
                videoPlayer.Play();
            else if (currentVideoAlpha <= 0.01f && videoPlayer.isPlaying)
                videoPlayer.Pause();
        }
    }

    private void SetVideoAlpha(float alpha)
    {
        if (videoRenderer == null) return;

        if (videoRenderer.material.HasProperty("_Color"))
        {
            Color c = videoRenderer.material.color;
            c.a = alpha;
            videoRenderer.material.color = c;
        }
    }

    /// <summary>
    /// 把 RectTransform 轉換成世界空間 Bounds
    /// </summary>
    private Bounds RectTransformToBounds(RectTransform r)
    {
        Vector3[] corners = new Vector3[4];
        r.GetWorldCorners(corners);
        Bounds b = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; i++) b.Encapsulate(corners[i]);
        return b;
    }
}
