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

    [Header("開始鈕消失/出場效果（按下後）")]
    [Tooltip("開始鈕淡出+位移動畫時間（秒），也同時用於『初次喚醒淡入+上浮』")]
    public float startFadeSeconds = 0.5f;
    [Tooltip("位移像素量：正數=向下（按下後），初次喚醒則反向向上")]
    public float startMoveDownPixels = 80f;

    [Header("通用")]
    [Tooltip("使用 UnscaledDeltaTime（不受 Time.timeScale 影響）")]
    public bool useUnscaledTime = true;
    [Tooltip("動畫結束後是否把開始鈕物件關閉（按下後）")]
    public bool deactivateStartButtonGO = true;

    [Header("初次喚醒：開始鈕淡入+上浮")]
    [Tooltip("此 UI 第一次啟用時，開始鈕由下方淡入並上浮到定位")]
    public bool playIntroOnFirstEnable = true;

    private bool switched;                 // 是否已按過開始
    private bool introPlayed;              // 是否已播放過初次喚醒動畫
    private CanvasGroup bottomCG;

    // 開始鈕快取
    private RectTransform _startRT;
    private CanvasGroup _startCG;
    private Vector2 _startOriPos;
    private bool _startPosCaptured;

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

            // 初始：隱藏且不可互動（保持啟用方便淡入）
            bottomButtonsRoot.gameObject.SetActive(true);
            bottomCG.alpha = 0f;
            bottomCG.interactable = false;
            bottomCG.blocksRaycasts = false;
        }

        CacheStartRefs();
    }

    void OnEnable()
    {
        CacheStartRefs();

        // 第一次喚醒 → 開始鈕「淡入＋上浮」
        if (playIntroOnFirstEnable && !introPlayed && _startRT != null && _startCG != null)
        {
            // 準備起始狀態：從「原位置的下方」開始，alpha=0，禁互動
            if (!_startPosCaptured)
            {
                _startOriPos = _startRT.anchoredPosition;
                _startPosCaptured = true;
            }
            Vector2 fromPos = _startOriPos + new Vector2(0f, -Mathf.Abs(startMoveDownPixels)); // 由下往上
            _startRT.anchoredPosition = fromPos;

            _startCG.alpha = 0f;
            _startCG.interactable = false;
            _startCG.blocksRaycasts = false;

            StartCoroutine(IntroAppear());
        }
        else
        {
            // 非初次或不播放 intro → 確保開始鈕可見且可互動
            if (_startCG != null)
            {
                _startCG.alpha = 1f;
                _startCG.interactable = true;
                _startCG.blocksRaycasts = true;
            }
            if (_startRT != null && _startPosCaptured)
                _startRT.anchoredPosition = _startOriPos;
        }
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

    IEnumerator IntroAppear()
    {
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

        introPlayed = true;
    }

    public void OnStartClicked()
    {
        if (switched) return;
        switched = true;

        if (startButton) startButton.interactable = false;

        // 依序：1) 開始鈕淡出+下滑 → 2) 顯示四顆按鈕
        StartCoroutine(HideStartThenShowBottom());
    }

    IEnumerator HideStartThenShowBottom()
    {
        // --- 1) 開始鈕動畫（淡出 + 向下位移） ---
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

        // --- 2) 顯示底部四鍵（淡入或立即） ---
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
