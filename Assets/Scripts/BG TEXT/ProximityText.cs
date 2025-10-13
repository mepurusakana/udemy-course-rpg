using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ProximityText_ByTextBounds : MonoBehaviour
{
    [Header("玩家與顯示物件")]
    [Tooltip("玩家 Transform；留空會自動找 Tag=Player")]
    public Transform player;
    [Tooltip("要控制透明度的 CanvasGroup（掛在文字的 Canvas 上）")]
    public CanvasGroup targetGroup;

    [Header("距離判定（基於文字框 Bounds）")]
    [Tooltip("<= 這個距離 → 文字全亮 (alpha=1)")]
    public float minDistanceForFullAlpha = 0.5f;
    [Tooltip(">= 這個距離 → 文字全隱 (alpha=0)")]
    public float maxDistanceForZeroAlpha = 5f;
    [Tooltip("透明度曲線，X=距離比例(0~1)，Y=透明度(0~1)")]
    public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("其他設定")]
    public string playerTag = "Player";
    [Tooltip("更新頻率（秒）；0=每幀更新")]
    [Range(0f, 0.2f)] public float updateInterval = 0.05f;
    public bool debugLog = false;

    private RectTransform rect;
    private float timer;

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
            targetGroup.alpha = 0f; // 一開始隱藏
            targetGroup.blocksRaycasts = false;
            targetGroup.interactable = false;
        }
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

        // 套用透明度
        targetGroup.alpha = a;
        targetGroup.blocksRaycasts = false;
        targetGroup.interactable = false;

        if (debugLog) Debug.Log($"[ProximityText_ByTextBounds] d={dist:F2}, alpha={a:F2}");
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
