using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_FadeScreen : MonoBehaviour
{
    [SerializeField] private Animator anim;  // ← 改成可在 Inspector 中指定
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
        FadeIn();
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

    public void FadeOut()
    {
        // 使用前再次檢查並嘗試初始化
        if (anim == null)
        {
            Debug.LogWarning("[UI_FadeScreen] Animator 是 null，嘗試重新初始化...");
            InitializeAnimator();
        }

        if (anim != null)
        {
            anim.SetTrigger("fadeOut");
            Debug.Log("[UI_FadeScreen] 執行 FadeOut");
        }
        else
        {
            Debug.LogError("[UI_FadeScreen] Animator 是 null，無法執行 FadeOut");
        }
    }

    public void FadeIn()
    {
        // 使用前再次檢查並嘗試初始化
        if (anim == null)
        {
            Debug.LogWarning("[UI_FadeScreen] Animator 是 null，嘗試重新初始化...");
            InitializeAnimator();
        }

        if (anim != null)
        {
            anim.SetTrigger("fadeIn");
            Debug.Log("[UI_FadeScreen] 執行 FadeIn");
        }
        else
        {
            Debug.LogError("[UI_FadeScreen] Animator 是 null，無法執行 FadeIn");
        }
    }
}