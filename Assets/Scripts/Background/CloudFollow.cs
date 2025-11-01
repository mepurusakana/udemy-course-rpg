using UnityEngine;

public class CloudFollow : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;  // 拖入主攝影機 Transform
    [SerializeField] private float followSpeed = 2f; // 跟隨速度
    private float fixedY; // 固定的 y 軸座標

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = GameObject.FindGameObjectWithTag("MainCamera")?.transform;
        }

        fixedY = transform.position.y; // 記錄初始的 y 軸位置
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // 目標位置：x 跟攝影機一致，y 保持固定
        Vector3 targetPos = new Vector3(targetCamera.position.x, fixedY, transform.position.z);

        // 平滑跟隨
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
