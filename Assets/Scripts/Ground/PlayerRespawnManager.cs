using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager instance;

    [Header("安全位置設定")]
    [SerializeField] private Vector3 lastSafePosition;
    [SerializeField] private float groundCheckInterval = 0.1f;
    [SerializeField] private float minimumSafeHeight = 0.5f; // 最小安全高度，避免記錄在尖刺上

    [Header("除錯顯示")]
    [SerializeField] private bool showDebugInfo = true;

    private Player player;
    private float checkTimer;
    private Vector3 previousPosition;

    private void Awake()
    {
        // 單例模式
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();

        if (player != null)
        {
            // 初始化為玩家當前位置
            lastSafePosition = player.transform.position;
            previousPosition = player.transform.position;

            Debug.Log($"PlayerRespawnManager 初始化完成，初始安全位置：{lastSafePosition}");
        }
        else
        {
            Debug.LogError("找不到Player物件！");
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 定時檢查玩家是否在地面
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0)
        {
            checkTimer = groundCheckInterval;

            // 檢查條件：
            // 1. 玩家在地面上
            // 2. 玩家不在busy狀態（沒有被控制）
            // 3. 玩家的Y軸位置足夠高（不在深坑底部）
            // 4. 玩家不在被擊退狀態
            if (player.IsGroundDetected() &&
                !player.isBusy &&
                !player.isKnocked &&
                player.transform.position.y > minimumSafeHeight)
            {
                // 確保位置有變化才記錄（避免在原地不動時重複記錄）
                if (Vector3.Distance(player.transform.position, previousPosition) > 0.1f)
                {
                    lastSafePosition = player.transform.position;
                    previousPosition = player.transform.position;

                    if (showDebugInfo)
                    {
                        Debug.Log($"更新安全位置：{lastSafePosition}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 獲取最後的安全位置
    /// </summary>
    public Vector3 GetLastSafePosition()
    {
        return lastSafePosition;
    }

    /// <summary>
    /// 手動設置安全位置（可用於檢查點系統）
    /// </summary>
    public void SetSafePosition(Vector3 position)
    {
        lastSafePosition = position;
        previousPosition = position;

        if (showDebugInfo)
        {
            Debug.Log($"手動設置安全位置：{lastSafePosition}");
        }
    }

    /// <summary>
    /// 強制更新當前位置為安全位置
    /// </summary>
    public void ForceUpdateSafePosition()
    {
        if (player != null)
        {
            lastSafePosition = player.transform.position;
            previousPosition = player.transform.position;

            if (showDebugInfo)
            {
                Debug.Log($"強制更新安全位置：{lastSafePosition}");
            }
        }
    }

    /// <summary>
    /// 重置到初始位置
    /// </summary>
    public void ResetToInitialPosition(Vector3 initialPosition)
    {
        lastSafePosition = initialPosition;
        if (player != null)
        {
            player.transform.position = initialPosition;
        }
    }

    // 在Scene視圖顯示最後安全位置
    private void OnDrawGizmos()
    {
        // 繪製安全位置標記
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSafePosition, 1f);
        Gizmos.DrawLine(lastSafePosition, lastSafePosition + Vector3.up * 2f);

        // 繪製安全區域參考線
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawLine(
            new Vector3(-100, minimumSafeHeight, 0),
            new Vector3(100, minimumSafeHeight, 0)
        );

        // 在編輯器中顯示文字標籤
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            lastSafePosition + Vector3.up * 2.5f,
            "安全重生點"
        );
#endif
    }
}