using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance;

    private string nextSpawnId = "";
    private string nextSceneName = "";

    private readonly Dictionary<string, Vector3> lastPlayerPositions = new Dictionary<string, Vector3>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetNextSpawnTarget(string sceneName, string spawnId)
    {
        nextSceneName = sceneName;
        nextSpawnId = spawnId;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 玩家生成
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        Player player = FindObjectOfType<Player>();

        if (player != null)
        {
            var spawn = spawnPoints.FirstOrDefault(sp => sp.spawnId == nextSpawnId)
                        ?? spawnPoints.FirstOrDefault(sp => sp.isDefault);

            if (spawn != null)
                player.transform.position = spawn.transform.position;
        }

        //  更新相機邊界
        UpdateCameraConfiner();

        nextSpawnId = "";
        nextSceneName = "";
    }

    /// <summary>
    /// 記錄玩家離開當前場景的位置（供之後返回使用）
    /// </summary>
    public void SaveCurrentScenePlayerPos(Vector3 position)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        lastPlayerPositions[currentScene] = position;
        Debug.Log($"[SceneStateManager] 已記錄玩家離開場景 {currentScene} 的位置：{position}");
    }

    /// <summary>
    /// 嘗試取得玩家上次離開該場景的位置（若無紀錄則回傳 false）
    /// </summary>
    public bool TryGetSavedPlayerPos(string sceneName, out Vector3 position)
    {
        return lastPlayerPositions.TryGetValue(sceneName, out position);
    }

    private void UpdateCameraConfiner()
    {
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam == null) return;

        var confiner = vcam.GetComponent<CinemachineConfiner2D>();
        if (confiner == null) return;

        var bound = GameObject.Find("CameraBound");
        if (bound == null)
        {
            Debug.LogWarning("[SceneStateManager] 沒有找到 CameraBound，使用舊邊界");
            return;
        }

        var collider = bound.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("[SceneStateManager] CameraBound 上沒有 Collider2D");
            return;
        }

        confiner.m_BoundingShape2D = collider;
        confiner.InvalidateCache(); // 必須重置 Confiner 快取
        Debug.Log($"[SceneStateManager] 相機邊界更新為 {bound.name}");
    }
}
