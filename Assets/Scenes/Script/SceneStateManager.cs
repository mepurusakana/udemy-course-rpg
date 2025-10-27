using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance;

    [SerializeField] private bool showLogs = false;

    private readonly Dictionary<string, Vector3> lastPositions = new Dictionary<string, Vector3>();
    private string pendingSceneName = null;
    private string pendingSpawnId = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ★ 關鍵：先脫離父物件，確保是根物件，再 DontDestroyOnLoad
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- 其餘既有功能（落點邏輯） ---
    public void SaveCurrentScenePlayerPos(Vector3 playerPos)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        lastPositions[currentScene] = playerPos;
        if (showLogs) Debug.Log($"[SceneState] 記錄 {currentScene} 位置 = {playerPos}");
    }

    public void SetNextSpawnTarget(string targetSceneName, string spawnId)
    {
        pendingSceneName = targetSceneName;
        pendingSpawnId = string.IsNullOrEmpty(spawnId) ? null : spawnId;
        if (showLogs) Debug.Log($"[SceneState] 下一場景落點：scene={pendingSceneName}, spawnId={pendingSpawnId}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(PlacePlayerNextFrame(scene.name));
    }

    private IEnumerator PlacePlayerNextFrame(string sceneName)
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // 1) 指定 SpawnID
        if (!string.IsNullOrEmpty(pendingSceneName) && pendingSceneName == sceneName && !string.IsNullOrEmpty(pendingSpawnId))
        {
            var spawns = GameObject.FindObjectsOfType<SpawnPoint>();
            SpawnPoint target = null;
            foreach (var sp in spawns)
            {
                if (sp != null && sp.spawnId == pendingSpawnId) { target = sp; break; }
            }
            if (target != null)
            {
                player.transform.position = target.transform.position;
                pendingSceneName = null; pendingSpawnId = null;
                yield break;
            }
            // 找不到就落到次優先
            pendingSceneName = null; pendingSpawnId = null;
        }

        // 2) 回到上次離開的位置
        if (lastPositions.TryGetValue(sceneName, out var pos))
        {
            player.transform.position = pos;
            yield break;
        }

        // 3) 預設出生點
        var all = GameObject.FindObjectsOfType<SpawnPoint>();
        if (all != null && all.Length > 0)
        {
            Vector3? targetPos = null;
            foreach (var sp in all) if (sp.isDefault) { targetPos = sp.transform.position; break; }
            if (!targetPos.HasValue) targetPos = all[0].transform.position;
            player.transform.position = targetPos.Value;
        }
    }

    public void ClearSavedPositionForScene(string sceneName)
    {
        if (lastPositions.Remove(sceneName) && showLogs)
            Debug.Log($"[SceneState] 已清除 {sceneName} 的記錄位置。");
    }
}
