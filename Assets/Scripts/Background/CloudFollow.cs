using UnityEngine;
using UnityEngine.SceneManagement;

public class CloudFollow : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;  // 主攝影機 Transform
    [SerializeField] private float followSpeed = 2f;  // 跟隨速度
    private float fixedY;                             // 固定的 y 軸座標

    private void Awake()
    {
        // 嘗試尋找現有攝影機
        FindCamera();

        // 註冊場景切換事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        fixedY = transform.position.y; // 記錄初始的 y 軸位置
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            FindCamera();
            return;
        }

        // 目標位置：x 跟攝影機一致，y 保持固定
        Vector3 targetPos = new Vector3(targetCamera.position.x, fixedY, transform.position.z);

        // 平滑跟隨
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 嘗試尋找主攝影機（支援 DontDestroyOnLoad）
    /// </summary>
    private void FindCamera()
    {
        if (targetCamera != null) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            targetCamera = cam.transform;
            return;
        }

        // 若 Camera.main 為 null，手動搜尋有 Camera 組件的物件
        Camera foundCam = Object.FindObjectOfType<Camera>();
        if (foundCam != null)
        {
            targetCamera = foundCam.transform;
        }
    }

    /// <summary>
    /// 當新場景載入時自動重新尋找攝影機
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCamera();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
