using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [Header("Cinemachine Impulse Source")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 若你需要跨場景：取消註解
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 觸發相機震動（可帶方向）
    /// direction：受力方向（通常是 從攻擊者 → 受擊者 的方向）
    /// force：震動倍率（1=預設）
    /// </summary>
    public void Shake(Vector2 direction, float force = 1f)
    {
        if (impulseSource == null) return;

        Vector3 dir3 = new Vector3(direction.x, direction.y, 0f) * force;
        impulseSource.GenerateImpulse(dir3);
    }
}
