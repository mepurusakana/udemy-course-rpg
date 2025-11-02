using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    public static UI_InGame instance;

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill; // HP 紅條

    [Header("MP UI")]
    [SerializeField] private Slider mpSlider;
    [SerializeField] private Image mpFill; // MP 藍條

    [Header("Optional Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI mpText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindAndSubscribeToPlayer();
    }

    private void OnEnable()
    {
        FindAndSubscribeToPlayer();
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.onHealthChanged -= UpdateHealthUI;
            // 如果之後你加入 onMPChanged，也可以一併取消訂閱
        }
    }

    private void FindAndSubscribeToPlayer()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
        {
            Invoke(nameof(FindAndSubscribeToPlayer), 0.5f);
            return;
        }

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Invoke(nameof(FindAndSubscribeToPlayer), 0.5f);
            return;
        }

        // 綁定事件
        playerStats.onHealthChanged += UpdateHealthUI;

        // 初始化 UI
        UpdateHealthUI();
        UpdateMPUI(playerStats.currentMP, playerStats.GetMaxMPValue());
    }

    private void Update()
    {
        // 若 PlayerStats 在更新 MP 時沒呼叫 UI 更新（例如自然回魔），也可在這同步
        if (playerStats != null)
            UpdateMPUI(playerStats.currentMP, playerStats.GetMaxMPValue());
    }

    /// <summary>更新血量條</summary>
    private void UpdateHealthUI()
    {
        if (playerStats == null) return;

        int maxHealth = playerStats.GetMaxHealthValue();
        healthSlider.maxValue = maxHealth;
        healthSlider.value = playerStats.currentHealth;

        if (healthText != null)
            healthText.text = $"{playerStats.currentHealth}/{maxHealth}";

        if (healthFill != null)
            healthFill.fillAmount = (float)playerStats.currentHealth / maxHealth;
    }

    /// <summary>更新魔力條</summary>
    public void UpdateMPUI(int currentMP, int maxMP)
    {
        if (mpSlider != null)
        {
            mpSlider.maxValue = maxMP;
            mpSlider.value = currentMP;
        }

        if (mpText != null)
            mpText.text = $"{currentMP}/{maxMP}";

        if (mpFill != null)
            mpFill.fillAmount = (float)currentMP / maxMP;
    }
}
