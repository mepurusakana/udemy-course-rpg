using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
public class StartToButtonsUI : MonoBehaviour
{
    [Header("開始按鈕")]
    public Button startButton;

    [Header("底部四鍵容器（需有 CanvasGroup）")]
    public RectTransform bottomButtonsRoot;

    [Header("切換後預設聚焦（可選）")]
    public Button firstButtonOnBottom;

    [Header("底部四鍵出現效果")]
    [Tooltip("四顆按鈕出現的淡入時間；0 = 立即出現")]
    public float appearFadeSeconds = 0f;

    [Header("開始鈕出/入場通用參數")]
    [Tooltip("開始鈕淡出+位移動畫時間（秒），也用於『喚醒時淡入+上浮』")]
    public float startFadeSeconds = 0.5f;
    [Tooltip("位移像素量：正數=向下（按下後）；喚醒時則反向向上")]
    public float startMoveDownPixels = 80f;

    [Header("時間/行為")]
    [Tooltip("使用 UnscaledDeltaTime（不受 Time.timeScale 影響）")]
    public bool useUnscaledTime = true;
    [Tooltip("按下後動畫結束是否把開始鈕物件關閉")]
    public bool deactivateStartButtonGO = true;

    [Header("喚醒：開始鈕淡入+上浮")]
    [Tooltip("喚醒時，先延遲這麼久才開始淡入+上浮（秒）")]
    public float startIntroDelay = 0.25f;

    private bool switched;                 // 是否已按過開始（每次喚醒會重置）
    private CanvasGroup bottomCG;

    // 開始鈕快取
    private RectTransform _startRT;
    private CanvasGroup _startCG;
    private Vector2 _startOriPos;
    private bool _startPosCaptured;

    private Coroutine _introCo;

    void Reset()
    {
        if (startButton == null)
        {
            var go = GameObject.Find("StartButton");
            if (go) startButton = go.GetComponent<Button>();
        }
        if (bottomButtonsRoot == null)
        {
            var go = GameObject.Find("BottomButtons");
            if (go) bottomButtonsRoot = go.GetComponent<RectTransform>();
        }
    }

    void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (bottomButtonsRoot != null)
        {
            bottomCG = bottomButtonsRoot.GetComponent<CanvasGroup>();
            if (bottomCG == null) bottomCG = bottomButtonsRoot.gameObject.AddComponent<CanvasGroup>();
            bottomButtonsRoot.gameObject.SetActive(true); // 方便淡入
        }

        CacheStartRefs();
    }

    void OnEnable()
    {
        CacheStartRefs();

        // 每次喚醒都重置狀態
        switched = false;

        // 1) 開始鈕：確保可見並重置到「由下開始、透明」
        if (startButton != null)
            startButton.gameObject.SetActive(true); // 若上次按下後被關掉，這裡打開

        if (_introCo != null) { StopCoroutine(_introCo); _introCo = null; }

        if (_startRT != null && _startCG != null)
        {
            if (!_startPosCaptured)
            {
                _startOriPos = _startRT.anchoredPosition;
                _startPosCaptured = true;
            }

            Vector2 fromPos = _startOriPos + new Vector2(0f, -Mathf.Abs(startMoveDownPixels));
            _startRT.anchoredPosition = fromPos;

            _startCG.alpha = 0f;
            _startCG.interactable = false;
            _startCG.blocksRaycasts = false;

            _introCo = StartCoroutine(IntroAppearWithDelay());
        }

        // 2) 底部四鍵：每次喚醒先隱藏與關互動
        if (bottomCG != null)
        {
            bottomCG.alpha = 0f;
            bottomCG.interactable = false;
            bottomCG.blocksRaycasts = false;
            bottomButtonsRoot.gameObject.SetActive(true); // 保持啟用以利之後淡入
        }
    }

    void OnDisable()
    {
        if (_introCo != null) { StopCoroutine(_introCo); _introCo = null; }
    }

    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
    }

    void CacheStartRefs()
    {
        if (startButton == null) return;
        if (_startRT == null) _startRT = startButton.transform as RectTransform;
        if (_startRT != null && !_startPosCaptured)
        {
            _startOriPos = _startRT.anchoredPosition;
            _startPosCaptured = true;
        }
        if (_startCG == null)
        {
            _startCG = startButton.GetComponent<CanvasGroup>();
            if (_startCG == null) _startCG = startButton.gameObject.AddComponent<CanvasGroup>();
        }
    }

    IEnumerator IntroAppearWithDelay()
    {
        // 延遲
        if (startIntroDelay > 0f)
        {
            float tDelay = 0f;
            float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (tDelay < startIntroDelay) { tDelay += Dt(); yield return null; }
        }

        // 淡入 + 上浮
        float dur = Mathf.Max(0.0001f, startFadeSeconds);
        float t = 0f;
        float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        Vector2 fromPos = _startOriPos + new Vector2(0f, -Mathf.Abs(startMoveDownPixels));
        Vector2 toPos = _startOriPos;

        while (t < dur)
        {
            t += dt();
            float k = Mathf.Clamp01(t / dur);
            float eased = k * k * (3f - 2f * k); // SmoothStep
            _startCG.alpha = eased;              // 0 → 1
            _startRT.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);
            yield return null;
        }

        _startCG.alpha = 1f;
        _startRT.anchoredPosition = toPos;
        _startCG.interactable = true;
        _startCG.blocksRaycasts = true;

        _introCo = null;
    }

    public void OnStartClicked()
    {
        if (switched) return;
        switched = true;

        if (startButton) startButton.interactable = false;

        StartCoroutine(HideStartThenShowBottom());
    }

    IEnumerator HideStartThenShowBottom()
    {
        // --- 1) 開始鈕：淡出 + 向下位移 ---
        if (_startRT != null && _startCG != null)
        {
            _startCG.blocksRaycasts = true; // 動畫期間擋點擊
            _startCG.interactable = false;

            Vector2 fromPos = _startPosCaptured ? _startOriPos : _startRT.anchoredPosition;
            Vector2 toPos = fromPos + new Vector2(0f, -Mathf.Abs(startMoveDownPixels));

            float dur = Mathf.Max(0.0001f, startFadeSeconds);
            float t = 0f;
            float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            while (t < dur)
            {
                t += dt();
                float k = Mathf.Clamp01(t / dur);
                float eased = k * k * (3f - 2f * k);

                _startCG.alpha = 1f - eased; // 1 → 0
                _startRT.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);

                yield return null;
            }

            _startCG.alpha = 0f;
            _startRT.anchoredPosition = toPos;

            if (deactivateStartButtonGO)
                startButton.gameObject.SetActive(false);
            else
            {
                _startCG.blocksRaycasts = false;
                _startCG.interactable = false;
            }
        }

        // --- 2) 底部四鍵：淡入或立即 ---
        yield return StartCoroutine(ShowBottomButtons());
    }

    IEnumerator ShowBottomButtons()
    {
        if (bottomCG == null) yield break;

        if (!bottomButtonsRoot.gameObject.activeSelf)
            bottomButtonsRoot.gameObject.SetActive(true);

        bottomCG.interactable = false;
        bottomCG.blocksRaycasts = false;

        float dur = Mathf.Max(0f, appearFadeSeconds);
        if (dur <= 0f)
        {
            bottomCG.alpha = 1f;
        }
        else
        {
            float t = 0f;
            float fromA = bottomCG.alpha; // 應為 0
            float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            while (t < dur)
            {
                t += dt();
                float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, dur));
                bottomCG.alpha = Mathf.Lerp(fromA, 1f, k);
                yield return null;
            }
            bottomCG.alpha = 1f;
        }

        bottomCG.interactable = true;
        bottomCG.blocksRaycasts = true;

        if (firstButtonOnBottom != null)
            EventSystem.current?.SetSelectedGameObject(firstButtonOnBottom.gameObject);
    }
}
