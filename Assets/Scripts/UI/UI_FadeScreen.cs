using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_FadeScreen : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private float fadeDuration = 1f;  // 淡入淡出持續時間（需與動畫時長一致）
    

    private static UI_FadeScreen instance;

    void Awake()
    {
        // 單例模式：確保只有一個 FadeScreen 存在
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 切換場景時不銷毀
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // 初始化 Animator
        InitializeAnimator();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次場景載入完成後都自動執行 FadeIn
        StartCoroutine(AutoFadeIn());
        Debug.Log($"[UI_FadeScreen] 場景 '{scene.name}' 載入完成，執行 FadeIn");
    }

    IEnumerator AutoFadeIn()
    {
        yield return new WaitForSeconds(0.1f);
        FadeIn(1f);
    }

    // 初始化或重新獲取 Animator
    private void InitializeAnimator()
    {
        // 如果沒有在 Inspector 中指定，就自動尋找
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        // 如果還是找不到，檢查子物件
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
        // 最終檢查
        if (anim == null)
        {
            Debug.LogError("[UI_FadeScreen] 找不到 Animator 組件！請確保：\n" +
                          "1. 此物件或子物件上有 Animator 組件\n" +
                          "2. 或在 Inspector 中手動指定 Animator\n" +
                          "3. Animator Controller 已設定");
        }
        else
        {
            Debug.Log("[UI_FadeScreen] Animator 初始化成功");
        }
    }

    public void FadeOut(float duration)
    {
        // 使用前再次檢查並嘗試初始化
        if (anim == null)
        {
            Debug.LogWarning("[UI_FadeScreen] Animator 是 null，嘗試重新初始化...");
            InitializeAnimator();
        }
        if (anim != null)
        {
            StartCoroutine(FadeCoroutine("fadeOut", duration));
            Debug.Log("[UI_FadeScreen] 執行 FadeOut");
        }
        else
        {
            Debug.LogError("[UI_FadeScreen] Animator 是 null，無法執行 FadeOut");
        }
    }

    public void FadeIn(float duration)
    {
        // 使用前再次檢查並嘗試初始化
        if (anim == null)
        {
            Debug.LogWarning("[UI_FadeScreen] Animator 是 null，嘗試重新初始化...");
            InitializeAnimator();
        }
        if (anim != null)
        {
            StartCoroutine(FadeCoroutine("fadeIn", duration));
            Debug.Log("[UI_FadeScreen] 執行 FadeIn");
        }
        else
        {
            Debug.LogError("[UI_FadeScreen] Animator 是 null，無法執行 FadeIn");
        }
    }

    // 淡入淡出協程：控制對話觸發器的啟用狀態
    private IEnumerator FadeCoroutine(string triggerName, float duration)
    {
        // 淡入淡出開始時，禁用所有對話觸發器
        DialogueStarter[] allStarters = FindObjectsOfType<DialogueStarter>();
        foreach (var starter in allStarters)
        {
            starter.enabled = false;
        }
        Debug.Log($"[UI_FadeScreen] 開始 {triggerName}，已禁用 {allStarters.Length} 個 DialogueStarter");

        // 觸發動畫
        anim.SetTrigger(triggerName);

        // 等待動畫完成
        yield return new WaitForSeconds(fadeDuration);

        // 淡入淡出結束時，重新啟用所有對話觸發器
        foreach (var starter in allStarters)
        {
            if (starter != null)  // 防止場景切換時物件已被銷毀
            {
                starter.enabled = true;
            }
        }
        Debug.Log($"[UI_FadeScreen] {triggerName} 完成，已重新啟用所有 DialogueStarter");
    }
}