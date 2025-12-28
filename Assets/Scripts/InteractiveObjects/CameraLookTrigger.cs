using UnityEngine;
using Cinemachine;

public class CameraLookTrigger : MonoBehaviour
{
    [Header("Camera (Auto)")]
    private CinemachineVirtualCamera vcam;
    private Transform player;
    private CinemachineFramingTransposer transposer;

    [Header("Look Target")]
    public Transform lookTarget;

    [Header("Follow Damping")]
    public float followDamping = 20f;

    [Header("Follow Parameters")]
    public float TargetSoftZoneWidth;
    public float TargetSoftZoneHeight;
    public float TargetDeadZoneWidth;
    public float TargetDeadZoneHeight;
    public float TargetBiasX;
    public float TargetBiasY;

    // ===== 原始參數備份 =====
    private float originalSoftZoneWidth;
    private float originalSoftZoneHeight;
    private float originalDeadZoneWidth;
    private float originalDeadZoneHeight;
    private Vector2 originalBias;

    private void Awake()
    {
        FindReferences();
        CacheOriginalSettings();
    }

    private void FindReferences()
    {
        player = FindObjectOfType<Player>()?.transform;
        vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    private void CacheOriginalSettings()
    {
        if (transposer == null) return;

        originalSoftZoneWidth = transposer.m_SoftZoneWidth;
        originalSoftZoneHeight = transposer.m_SoftZoneHeight;
        originalDeadZoneWidth = transposer.m_DeadZoneWidth;
        originalDeadZoneHeight = transposer.m_DeadZoneHeight;
        originalBias = transposer.m_ScreenX != 0 || transposer.m_ScreenY != 0
            ? new Vector2(transposer.m_ScreenX, transposer.m_ScreenY)
            : new Vector2(0.5f, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform != player) return;
        if (vcam == null || transposer == null) return;

        //  切換 Follow 目標
        vcam.Follow = lookTarget;

        //  調整 Framing
        transposer.m_SoftZoneWidth = 3f;
        transposer.m_SoftZoneHeight = 1.7f;
        transposer.m_DeadZoneWidth = 0f;
        transposer.m_DeadZoneHeight = 0f;

        transposer.m_BiasX = 0f; // BiasX = 0
        transposer.m_BiasY = 0f; // BiasY = 0

        // 平滑跟隨
        transposer.m_XDamping = followDamping;
        transposer.m_YDamping = followDamping;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform != player) return;
        if (vcam == null || transposer == null) return;

        //  回到玩家
        vcam.Follow = player;

        //  還原 Framing
        transposer.m_SoftZoneWidth = originalSoftZoneWidth;
        transposer.m_SoftZoneHeight = originalSoftZoneHeight;
        transposer.m_DeadZoneWidth = originalDeadZoneWidth;
        transposer.m_DeadZoneHeight = originalDeadZoneHeight;

        transposer.m_BiasX = 0f;
        transposer.m_BiasY = -0.299f;

        // 平滑跟隨
        transposer.m_XDamping = followDamping;
        transposer.m_YDamping = followDamping;
    }
}
