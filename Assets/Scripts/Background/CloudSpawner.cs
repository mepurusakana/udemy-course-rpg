using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("雲朵預製物")]
    [Tooltip("拖曳你所有的雲朵 Prefab 到這裡")]
    public List<GameObject> cloudPrefabs = new List<GameObject>();

    [Header("生成位置設定")]
    [Tooltip("雲朵生成的左邊界 X 座標")]
    public float leftBoundary = -20f;

    [Tooltip("雲朵生成的最低 Y 座標")]
    public float minY = 5f;

    [Tooltip("雲朵生成的最高 Y 座標")]
    public float maxY = 15f;

    [Header("速度設定")]
    [Tooltip("雲朵最慢的移動速度")]
    public float minSpeed = 1f;

    [Tooltip("雲朵最快的移動速度")]
    public float maxSpeed = 4f;

    [Header("生成設定")]
    [Tooltip("初始生成的雲朵數量")]
    public int initialCloudCount = 5;

    [Tooltip("雲朵消失的右邊界 X 座標（要和 CloudController 一致）")]
    public float rightBoundary = 20f;

    [Tooltip("生成新雲朵前的隨機延遲時間（秒）")]
    public float minSpawnDelay = 2f;
    public float maxSpawnDelay = 5f;

    private List<GameObject> activeCloudsList = new List<GameObject>();
    private float nextSpawnTime;

    void Start()
    {
        // 初始生成指定數量的雲朵
        for (int i = 0; i < initialCloudCount; i++)
        {
            SpawnCloud(true); // true 代表是初始生成，會隨機分布在整個螢幕
        }

        // 設定下次生成時間
        ScheduleNextSpawn();
    }

    void Update()
    {
        // 檢查是否該生成新雲朵
        if (Time.time >= nextSpawnTime)
        {
            SpawnCloud(false); // false 代表從左邊界生成
            ScheduleNextSpawn();
        }
    }

    // 生成一朵雲
    void SpawnCloud(bool isInitialSpawn)
    {
        // 檢查是否有雲朵預製物
        if (cloudPrefabs.Count == 0)
        {
            Debug.LogWarning("沒有設定雲朵 Prefab！請在 Inspector 中加入雲朵預製物。");
            return;
        }

        // 隨機選擇一種雲朵
        GameObject randomCloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Count)];

        // 決定生成位置
        float spawnX;
        if (isInitialSpawn)
        {
            // 初始生成：在整個螢幕範圍內隨機分布
            spawnX = Random.Range(leftBoundary, rightBoundary);
        }
        else
        {
            // 正常生成：從左邊界生成
            spawnX = leftBoundary;
        }

        float spawnY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        // 生成雲朵
        GameObject newCloud = Instantiate(randomCloudPrefab, spawnPosition, Quaternion.identity);
        newCloud.transform.parent = transform; // 設為 CloudSpawner 的子物件，方便管理

        // 設定雲朵的移動速度
        CloudController cloudController = newCloud.GetComponent<CloudController>();
        if (cloudController == null)
        {
            // 如果雲朵預製物上沒有 CloudController，自動加上
            cloudController = newCloud.AddComponent<CloudController>();
        }

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        cloudController.Initialize(this, randomSpeed);
        cloudController.rightBoundary = rightBoundary;

        // 加入追蹤列表
        activeCloudsList.Add(newCloud);
    }

    // 設定下次生成時間
    void ScheduleNextSpawn()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        nextSpawnTime = Time.time + delay;
    }

    // 當雲朵被銷毀時，從列表中移除
    public void OnCloudDestroyed(GameObject cloud)
    {
        activeCloudsList.Remove(cloud);
    }

    // 在 Scene 視圖中顯示生成區域（方便調整）
    private void OnDrawGizmos()
    {
        // 左邊界線（綠色）
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(leftBoundary, minY, 0),
            new Vector3(leftBoundary, maxY, 0)
        );

        // 右邊界線（紅色）
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(rightBoundary, minY, 0),
            new Vector3(rightBoundary, maxY, 0)
        );

        // 上下邊界線（黃色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(leftBoundary, minY, 0),
            new Vector3(rightBoundary, minY, 0)
        );
        Gizmos.DrawLine(
            new Vector3(leftBoundary, maxY, 0),
            new Vector3(rightBoundary, maxY, 0)
        );
    }
}