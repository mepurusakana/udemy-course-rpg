using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager instance;

    [Header("安全位置設定")]
    [SerializeField] private Vector3 lastSafePosition;
    [SerializeField] private float groundCheckInterval = 0.1f; // 檢查間隔

    private Player player;
    private float checkTimer;

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
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();

        if (player != null)
        {
            // 初始化為玩家當前位置
            lastSafePosition = player.transform.position;
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

            // 如果玩家在地面且不在busy狀態，記錄位置
            if (player.IsGroundDetected() && !player.isBusy)
            {
                lastSafePosition = player.transform.position;
            }
        }
    }

    public Vector3 GetLastSafePosition()
    {
        return lastSafePosition;
    }

    // 手動設置安全位置（可用於檢查點）
    public void SetSafePosition(Vector3 position)
    {
        lastSafePosition = position;
    }

    // 在Scene視圖顯示最後安全位置
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSafePosition, 1f);
        Gizmos.DrawLine(lastSafePosition, lastSafePosition + Vector3.up * 2f);
    }
}