using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 傳送門（2D）：玩家進入範圍並按鍵（或直接靠近）後切換場景，
/// 並把「要用哪個 Spawn ID 落地」告訴 SceneStateManager。
/// </summary>

//常見配置範例
//A001 場景
//SpawnPoint: spawnId = "FromB001"（建議同場景另有 isDefault = true）
//A→B 的門：targetSceneName="B001", targetSpawnId = "FromA001"
//B001 場景
//SpawnPoint: spawnId = "FromA001"
//B→A 的門：targetSceneName="A001", targetSpawnId = "FromB001"

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SceneGate : MonoBehaviour
{
    [Header("要切換到的場景名稱（需加入 Build Settings）")]
    [Tooltip("和檔名一致（不含 .unity），大小寫需相同")]
    public string targetSceneName = "A001";

    [Header("到目標場景時，要落地的 Spawn ID（對應 SpawnPoint.spawnId）")]
    [Tooltip("留空則改用「上次離開該場景的位置」或預設出生點")]
    public string targetSpawnId = "";

    [Header("是否需要按鍵才切換（否則進入範圍就切）")]
    public bool requireKeyPress = true;

    [Header("互動按鍵（舊輸入系統）")]
    public KeyCode activationKey = KeyCode.E;

    [Header("玩家的 Tag 名稱")]
    public string playerTag = "Player";

    [Header("（可選）靠近時顯示的提示 UI")]
    public GameObject promptUI;

    [Header("除錯")]
    [SerializeField] private bool showLogs = false;

    private bool playerInRange = false;
    private Transform playerRoot; // 真的帶 Player Tag 的根節點
    private bool isLoading = false; // 本門的防抖旗標

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryGetPlayerRoot(other.transform, out var root))
        {
            playerInRange = true;
            playerRoot = root;
            if (promptUI) promptUI.SetActive(true);
            if (showLogs) Debug.Log($"[SceneGate] 玩家進入：{name}", this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerRoot != null && IsSameHierarchy(other.transform, playerRoot))
        {
            playerInRange = false;
            playerRoot = null;
            if (promptUI) promptUI.SetActive(false);
            if (showLogs) Debug.Log($"[SceneGate] 玩家離開：{name}", this);
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        bool pressed = !requireKeyPress;
        if (requireKeyPress && Input.GetKeyDown(activationKey))
            pressed = true;

        if (pressed) DoTransfer();
    }

    private void DoTransfer()
    {
        // 全域轉場管理器忙碌中 → 忽略（避免重入）
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsBusy)
            return;

        // 本門正在處理 → 忽略（本門防抖）
        if (isLoading) return;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneGate] targetSceneName 是空的！");
            return;
        }

        isLoading = true;           // 鎖本門，避免一門連觸
        if (promptUI) promptUI.SetActive(false);

        // ===== 首選：使用轉場管理器（有黑幕淡入淡出 + 非同步載入） =====
        if (SceneTransitionManager.Instance != null)
        {
            // 由管理器負責：記錄離開位置 + 設定 SpawnID + 淡出/載入/淡入
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetSpawnId);
            if (showLogs) Debug.Log($"[SceneGate] TransitionToScene → {targetSceneName} (spawnId=\"{targetSpawnId}\")", this);
            return;
        }

        // ===== 保險：沒有轉場管理器 → 走同步載入（沒有淡出） =====
        if (SceneStateManager.Instance != null)
        {
            // 記錄當前場景位置，供之後回來時使用
            if (playerRoot != null)
                SceneStateManager.Instance.SaveCurrentScenePlayerPos(playerRoot.position);

            // 指定下一場景的落地 SpawnID（可為空）
            SceneStateManager.Instance.SetNextSpawnTarget(targetSceneName, targetSpawnId);
        }
        else if (showLogs)
        {
            Debug.LogWarning("[SceneGate] 找不到 SceneStateManager.Instance，將做純切場景（不設定落點）。", this);
        }

        if (showLogs)
            Debug.Log($"[SceneGate] LoadScene（無轉場管理器）→ {targetSceneName}", this);

        SceneManager.LoadScene(targetSceneName);
    }

    private bool TryGetPlayerRoot(Transform t, out Transform rootWithTag)
    {
        Transform cur = t;
        while (cur != null)
        {
            if (cur.CompareTag(playerTag))
            {
                rootWithTag = cur;
                return true;
            }
            cur = cur.parent;
        }
        rootWithTag = null;
        return false;
    }

    private bool IsSameHierarchy(Transform a, Transform targetRoot)
    {
        Transform cur = a;
        while (cur != null)
        {
            if (cur == targetRoot) return true;
            cur = cur.parent;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        UnityEditor.Handles.color = Color.cyan;
        string label = $"→ {targetSceneName}";
        if (!string.IsNullOrEmpty(targetSpawnId)) label += $" : {targetSpawnId}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.35f, label);
    }
#endif
}
