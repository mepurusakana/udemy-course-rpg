using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_InGame : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider slider;


    [Header("Souls info")]
    [SerializeField] private TextMeshProUGUI currentSouls;
    [SerializeField] private float soulsAmount;
    [SerializeField] private float increaseRate = 100;

    void Start()
    {
        FindAndSubscribeToPlayer();
    }

    private void OnEnable()
    {
        // 每次啟用時都重新尋找玩家（場景切換後）
        FindAndSubscribeToPlayer();
    }

    private void OnDisable()
    {
        // 取消訂閱
        if (playerStats != null)
            playerStats.onHealthChanged -= UpdateHealthUI;
    }

    public void FindAndSubscribeToPlayer()
    {
        // 先取消舊的訂閱
        if (playerStats != null)
            playerStats.onHealthChanged -= UpdateHealthUI;

        // 尋找玩家
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.onHealthChanged += UpdateHealthUI;
                UpdateHealthUI(); // 立即更新一次
            }
        }
        else
        {
            // 如果 PlayerManager 還沒準備好，稍後再試
            Invoke(nameof(FindAndSubscribeToPlayer), 0.5f);
        }
    }

    void Update()
    {
        UpdateSoulsUI();
    }

    private void UpdateSoulsUI()
    {


        //currentSouls.text = ((int)soulsAmount).ToString();
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = playerStats.GetMaxHealthValue();
        slider.value = playerStats.currentHealth;
    }


    private void SetCooldownOf(Image _image)
    {
        if (_image.fillAmount <= 0)
            _image.fillAmount = 1;
    }

    private void CheckCooldownOf(Image _image, float _cooldown)
    {
        if (_image.fillAmount > 0)
            _image.fillAmount -= 1 / _cooldown * Time.deltaTime;
    }


}
