using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 全域場景轉場管理：黑幕淡入淡出 + 非同步載入（自動常駐）。
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("轉場參數")]
    [SerializeField] private float fadeOutDuration = 0.35f; // 到全黑
    [SerializeField] private float fadeInDuration = 0.30f; // 從黑回畫面
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("是否淡化整體音量（AudioListener.volume）")]
    [SerializeField] private bool fadeAudio = true;

    [Header("（可選）顯示簡易載入中文字")]
    [SerializeField] private bool showLoadingHint = false;

    [Header("（可選）指定字型（若不指定，會自動尋找）")]
    [SerializeField] private Font overrideFont;

    private CanvasGroup fader;
    private Text loadingText;
    private bool isTransitioning = false;
    private float originalAudioVol = 1f;

    /// 目前是否在轉場（讓 SceneGate 查，避免重入）
    public bool IsBusy => isTransitioning;

    // 第一次載入任何場景前，確保自己存在
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("SceneTransitionManager (Auto)");
            go.AddComponent<SceneTransitionManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlayIfNeeded();
    }

    private void BuildOverlayIfNeeded()
    {
        if (fader != null) return;

        // Root Canvas
        var root = new GameObject("__TransitionCanvas");
        root.layer = LayerMask.NameToLayer("UI");
        DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        // 黑幕
        var imgGO = new GameObject("Fade");
        imgGO.transform.SetParent(root.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 用 CanvasGroup 控制透明度
        fader = root.AddComponent<CanvasGroup>();
        fader.alpha = 0f;
        fader.blocksRaycasts = false;
        fader.interactable = false;

        if (showLoadingHint)
        {
            var textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(root.transform, false);
            loadingText = textGO.AddComponent<Text>();
            loadingText.text = "";
            loadingText.alignment = TextAnchor.LowerRight;
            loadingText.fontSize = 18;
            loadingText.color = new Color(1, 1, 1, 0.9f);

            var trt = loadingText.rectTransform;
            trt.anchorMin = new Vector2(1, 0);
            trt.anchorMax = new Vector2(1, 0);
            trt.pivot = new Vector2(1, 0);
            trt.anchoredPosition = new Vector2(-16, 16);

            loadingText.font = ResolveFontSafe(overrideFont);
        }
    }

    private Font ResolveFontSafe(Font userOverride)
    {
        if (userOverride != null) return userOverride;

        try { var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); if (f) return f; } catch { }
        try { var f = Resources.GetBuiltinResource<Font>("Arial.ttf"); if (f) return f; } catch { }
        try { var f = Font.CreateDynamicFontFromOSFont("Arial", 18); if (f) return f; } catch { }
        try
        {
            var names = Font.GetOSInstalledFontNames();
            if (names != null && names.Length > 0)
            {
                var f = Font.CreateDynamicFontFromOSFont(names[0], 18);
                if (f) return f;
            }
        }
        catch { }

        Debug.LogWarning("[SceneTransitionManager] 無法取得字型，Loading 文字可能不顯示。");
        return null;
    }

    /// 以淡入淡出進行場景切換；可帶 SpawnID（交給 SceneStateManager 處理落點）
    public void TransitionToScene(string sceneName, string targetSpawnId = null)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(sceneName, targetSpawnId));
    }

    private IEnumerator DoTransition(string sceneName, string targetSpawnId)
    {
        isTransitioning = true;

        // 1) 切場景前：記錄當前位置 & 指定下個場景落點
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player && SceneStateManager.Instance != null)
        {
            SceneStateManager.Instance.SaveCurrentScenePlayerPos(player.transform.position);
            SceneStateManager.Instance.SetNextSpawnTarget(sceneName, targetSpawnId);
        }

        // 2) 淡到全黑
        yield return Fade(1f, fadeOutDuration);

        // 3) 非同步載入
        if (loadingText) loadingText.text = "載入中…";
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null; // 等切換完成
        yield return null;                     // 再等一兩幀讓落點邏輯跑完
        yield return null;

        if (loadingText) loadingText.text = "";

        // 4) 從黑幕淡回
        yield return Fade(0f, fadeInDuration);

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fader == null) BuildOverlayIfNeeded();

        fader.blocksRaycasts = true;

        float start = fader.alpha;
        float t = 0f;

        if (fadeAudio) originalAudioVol = AudioListener.volume;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float k = fadeCurve.Evaluate(p);
            fader.alpha = Mathf.Lerp(start, targetAlpha, k);

            if (fadeAudio)
            {
                float targetVol = (targetAlpha >= 0.5f) ? 0f : originalAudioVol;
                AudioListener.volume = Mathf.Lerp(AudioListener.volume, targetVol, k);
            }

            yield return null;
        }

        fader.alpha = targetAlpha;
        fader.blocksRaycasts = targetAlpha > 0.001f;

        if (fadeAudio)
            AudioListener.volume = (targetAlpha <= 0.001f) ? originalAudioVol : 0f;
    }
}
