using UnityEngine;

public class CloudController : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("雲朵向右飄動的速度")]
    public float moveSpeed = 2f;

    [Header("邊界設定")]
    [Tooltip("雲朵消失的右邊界 X 座標")]
    public float rightBoundary = 20f;

    private CloudSpawner spawner;

    // 由 CloudSpawner 呼叫來設定參考
    public void Initialize(CloudSpawner _spawner, float _moveSpeed)
    {
        spawner = _spawner;
        moveSpeed = _moveSpeed;
    }

    void Update()
    {
        // 讓雲朵持續向右移動
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        // 檢查是否超過右邊界
        if (transform.position.x > rightBoundary)
        {
            // 通知生成器這朵雲已經消失
            if (spawner != null)
            {
                spawner.OnCloudDestroyed(gameObject);
            }

            // 銷毀這朵雲
            Destroy(gameObject);
        }
    }

    // 在 Scene 視圖中顯示邊界線（方便調整）
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(rightBoundary, -50, 0),
            new Vector3(rightBoundary, 50, 0)
        );
    }
}