using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager instance;

    [Header("安全位置設定")]
    [SerializeField] private Vector3 lastSafePosition;
    [SerializeField] private float groundCheckInterval = 0.2f; // 增加檢查間隔，避免過於頻繁
    [SerializeField] private float minimumSafeHeight = -10f; // 降低限制，避免過於嚴格
    [SerializeField] private float minimumMoveDistance = 0.5f; // 最小移動距離才記錄

    [Header("除錯顯示")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showDebugGizmos = true;

    private Player player;
    private float checkTimer;
    private Vector3 previousPosition;
    private int safePositionUpdateCount = 0; // 記錄更新次數

    private void Awake()
    {
        // 單例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景保留
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 延遲尋找 Player，確保場景已完全載入
        Invoke(nameof(FindPlayer), 1f);
    }

    private void FindPlayer()
    {
        player = FindObjectOfType<Player>();

        if (player != null)
        {
            // 初始化為玩家當前位置
            lastSafePosition = player.transform.position;
            previousPosition = player.transform.position;

            if (showDebugInfo)
            {

            }
        }
        else
        {
            Debug.LogError("<color=red>[PlayerRespawnManager] 找不到Player物件！</color>");
        }
    }

    private void Update()
    {
        if (player == null)
        {
            // 嘗試重新尋找 Player
            if (Time.frameCount % 60 == 0) // 每60幀嘗試一次
            {
                FindPlayer();
            }
            return;
        }

        // 定時檢查玩家是否在地面
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0)
        {
            checkTimer = groundCheckInterval;
            CheckAndUpdateSafePosition();
        }
    }

    private void CheckAndUpdateSafePosition()
    {
        // 檢查條件（簡化版，更寬鬆）
        bool isGrounded = player.IsGroundDetected();
        bool isNotBusy = !player.isBusy;
        bool isNotKnocked = !player.isKnocked;
        bool isAboveMinHeight = player.transform.position.y > minimumSafeHeight;

        // 只要在地面上且高度足夠就記錄（移除 isBusy 和 isKnocked 的限制）
        if (isGrounded && isAboveMinHeight)
        {
            // 計算移動距離
            float movedDistance = Vector3.Distance(player.transform.position, previousPosition);

            // 如果移動距離足夠或這是首次更新，則記錄
            if (movedDistance >= minimumMoveDistance || safePositionUpdateCount == 0)
            {
                lastSafePosition = player.transform.position;
                previousPosition = player.transform.position;
                safePositionUpdateCount++;

                if (showDebugInfo)
                {
                    //Debug.Log($"<color=cyan>[更新 #{safePositionUpdateCount}]</color> 安全位置：{lastSafePosition:F2}");
                }
            }
        }

        // 除錯資訊（可選）
        if (showDebugInfo && Time.frameCount % 120 == 0) // 每2秒顯示一次狀態
        {
            Debug.Log($"<color=yellow>[狀態檢查]</color> 著地:{isGrounded} | 忙碌:{!isNotBusy} | 擊退:{!isNotKnocked} | 高度OK:{isAboveMinHeight}");
        }
    }

    /// <summary>
    /// 獲取最後的安全位置
    /// </summary>
    public Vector3 GetLastSafePosition()
    {
        if (showDebugInfo)
        {
            //Debug.Log($"<color=magenta>[讀取]</color> 返回安全位置：{lastSafePosition}");
        }
        return lastSafePosition;
    }

    /// <summary>
    /// 手動設置安全位置（可用於檢查點系統）
    /// </summary>
    public void SetSafePosition(Vector3 position)
    {
        lastSafePosition = position;
        previousPosition = position;
        safePositionUpdateCount++;

        if (showDebugInfo)
        {
            Debug.Log($"<color=green>[手動設置]</color> 安全位置：{lastSafePosition}");
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
            safePositionUpdateCount++;

            if (showDebugInfo)
            {
                Debug.Log($"<color=orange>[強制更新]</color> 安全位置：{lastSafePosition}");
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

    /// <summary>
    /// 獲取更新次數（用於除錯）
    /// </summary>
    public int GetUpdateCount()
    {
        return safePositionUpdateCount;
    }

    // 在Scene視圖顯示最後安全位置
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

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

        // 繪製當前玩家位置到安全位置的連線
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.transform.position, lastSafePosition);
            Gizmos.DrawWireSphere(player.transform.position, 0.5f);
        }

        // 在編輯器中顯示文字標籤
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            lastSafePosition + Vector3.up * 2.5f,
            $"安全重生點 (更新:{safePositionUpdateCount}次)",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.green },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            }
        );

        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, lastSafePosition);
            UnityEditor.Handles.Label(
                player.transform.position + Vector3.up * 1.5f,
                $"距離安全點: {distance:F2}m",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.yellow },
                    fontSize = 10
                }
            );
        }
#endif
    }
}