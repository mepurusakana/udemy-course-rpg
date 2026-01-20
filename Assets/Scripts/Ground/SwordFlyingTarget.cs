using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SwordFlyingTarget : MonoBehaviour
{
    [Header("Hit Event")]
    public UnityEvent onHit;

    [Header("Move Settings")]
    public float moveAmplitude = 1.5f;   // 上下移動高度
    public float moveSpeed = 2f;          // 移動速度

    private Vector3 startPosition;
    private bool triggered = false;
    private bool canMove = true;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!canMove) return;

        float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveAmplitude;
        transform.position = startPosition + Vector3.down * offsetY;
    }

    public void Hit(FlyingSwordController sword)
    {
        if (triggered) return;

        triggered = true;
        canMove = false;

        // 回到初始位置
        transform.position = startPosition;

        onHit.Invoke();
    }

    /// <summary>
    /// 如果你之後需要重置標靶（例如重新挑戰）
    /// </summary>
    public void ResetTarget()
    {
        triggered = false;
        canMove = true;
        transform.position = startPosition;
    }
}
