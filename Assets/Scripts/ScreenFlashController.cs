using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制全屏闪屏效果（黑幕/白幕）- 不遮挡玩家版本
/// 挂载到 Canvas 下的 Image 物件上
/// </summary>
public class ScreenFlashController : MonoBehaviour
{
    public static ScreenFlashController instance;

    [Header("闪屏设置")]
    public Image flashImage; // UI Image 组件
    public Color blackColor = Color.black;
    public Color whiteColor = Color.white;

    [Header("玩家遮罩设置")]
    public Canvas flashCanvas; // 闪屏专用 Canvas
    public int sortOrderBelowPlayer = -1; // 设为比玩家低的层级

    private Coroutine currentFlashCoroutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // 單例檢查
        if (instance == null)
        {
            instance = this;

            // 讓父物件在切換場景時保留
            if (transform.parent != null)
            {
                DontDestroyOnLoad(transform.parent.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (instance != null && instance != this)
        {
            Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
            return;
        }

        // 原有初始化邏輯
        if (flashImage == null)
            flashImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (flashImage != null)
        {
            Color color = flashImage.color;
            color.a = 1f;
            flashImage.color = color;
            flashImage.raycastTarget = false;
        }

        canvasGroup.alpha = 0f;
        SetupCanvas();
    }

    private void SetupCanvas()
    {
        // 如果没有指定 Canvas，尝试获取父物件的 Canvas
        if (flashCanvas == null)
        {
            flashCanvas = GetComponentInParent<Canvas>();
        }

        // 如果还是没有，创建一个新的 Canvas
        if (flashCanvas == null)
        {
            GameObject canvasObj = new GameObject("ScreenFlashCanvas");
            flashCanvas = canvasObj.AddComponent<Canvas>();
            flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 将当前物件移到新 Canvas 下
            transform.SetParent(canvasObj.transform);
        }

        // 設定 Canvas 的 Sorting Layer
        flashCanvas.sortingLayerName = "FlashEffect";
        flashCanvas.sortingOrder = 0;

        //Debug.Log($"[ScreenFlash] Canvas 使用排序層: {flashCanvas.sortingLayerName} (Order: {flashCanvas.sortingOrder})");
    }


    /// <summary>
    /// 立即關閉閃屏（將 alpha 歸 0）
    /// </summary>
    /// <summary>
    /// 延遲關閉閃屏
    /// </summary>
    public void CloseFlash(float delay)
{
    if (currentFlashCoroutine != null)
    {
        StopCoroutine(currentFlashCoroutine);
        currentFlashCoroutine = null;
    }

    // 啟動新的延遲關閉協程
    currentFlashCoroutine = StartCoroutine(CloseFlashCoroutine(delay));
}

private IEnumerator CloseFlashCoroutine(float delay)
{
    // 等待指定秒數（不受 Time.timeScale 影響）
    yield return new WaitForSecondsRealtime(delay);

    if (canvasGroup != null)
    {
        canvasGroup.alpha = 0f;
        Debug.Log($"[ScreenFlash] 延遲 {delay} 秒後關閉閃屏 (Alpha = 0)");
    }

    currentFlashCoroutine = null;
}

    /// <summary>
    /// 黑幕效果（快速闪现）
    /// </summary>
    public void BlackFlash(float duration = 0.1f)
    {
        Flash(blackColor, duration);
    }

    /// <summary>
    /// 白幕效果（快速闪现）
    /// </summary>
    public void WhiteFlash(float duration = 0.1f)
    {
        Flash(whiteColor, duration);
    }

    /// <summary>
    /// 黑幕渐入渐出
    /// </summary>
    public void BlackFade(float fadeInTime = 0.2f, float holdTime = 0.1f, float fadeOutTime = 0.2f)
    {
        FadeEffect(blackColor, fadeInTime, holdTime, fadeOutTime);
    }

    /// <summary>
    /// 白幕渐入渐出
    /// </summary>
    public void WhiteFade(float fadeInTime = 0.2f, float holdTime = 0.1f, float fadeOutTime = 0.2f)
    {
        FadeEffect(whiteColor, fadeInTime, holdTime, fadeOutTime);
    }

    /// <summary>
    /// 基础闪现效果（瞬间满屏然后消失）
    /// </summary>
    private void Flash(Color color, float duration)
    {
        StopCurrentFlash();
        currentFlashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
    }

    /// <summary>
    /// 渐变效果（淡入-持续-淡出）
    /// </summary>
    private void FadeEffect(Color color, float fadeInTime, float holdTime, float fadeOutTime)
    {
        StopCurrentFlash();
        currentFlashCoroutine = StartCoroutine(FadeCoroutine(color, fadeInTime, holdTime, fadeOutTime));
    }

    private IEnumerator FlashCoroutine(Color color, float duration)
    {
        if (flashImage == null || canvasGroup == null) yield break;

        // 设置颜色（不透明）
        color.a = 1f;
        flashImage.color = color;

        // 瞬间显示
        canvasGroup.alpha = 1f;

        // 等待一帧确保显示
        yield return null;

        // 立即全亮
        canvasGroup.alpha = 1f;

        // 維持全亮狀態
        yield return new WaitForSecondsRealtime(duration);

    }



    private IEnumerator FadeCoroutine(Color color, float fadeInTime, float holdTime, float fadeOutTime)
    {
        if (flashImage == null || canvasGroup == null) yield break;

        // 设置颜色（不透明）
        color.a = 1f;
        flashImage.color = color;

        // 阶段 1: 淡入
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }

        // 阶段 2: 保持
        canvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(holdTime);

        // 阶段 3: 淡出
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            yield return null;
        }

        // 确保完全透明
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 停止当前的闪屏效果
    /// </summary>
    private void StopCurrentFlash()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
            currentFlashCoroutine = null;
        }

        // 重置为透明
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 自定义颜色闪屏
    /// </summary>
    public void CustomFlash(Color color, float duration = 0.1f)
    {
        Flash(color, duration);
    }

    /// <summary>
    /// 动态设置 Canvas 排序层级
    /// </summary>
    public void SetCanvasSortOrder(int order)
    {
        if (flashCanvas != null)
        {
            flashCanvas.sortingOrder = order;
            Debug.Log($"[ScreenFlash] Canvas Sort Order 更新为: {order}");
        }
    }
}