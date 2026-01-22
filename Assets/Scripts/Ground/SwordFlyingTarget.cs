using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SwordFlyingTarget : MonoBehaviour
{
    [Header("Hit Event")]
    public UnityEvent onHit;

    [Header("Move Settings")]
    public float moveAmplitude = 1.5f;   // 上下移動高度
    public float moveSpeed = 2f;          // 移動速度

    [Header("Return Settings")]
    public float hitPauseTime = 0.5f;
    public float returnDuration = 0.6f;

    private Vector3 startPosition;
    private bool triggered = false;
    private bool canMove = true;

    private BoxCollider2D box;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!canMove) return;

        float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveAmplitude;
        transform.position = startPosition + Vector3.down * offsetY;
    }

    //public void Hit(FlyingSwordController sword)
    //{
    //    if (triggered) return;

    //    triggered = true;
    //    canMove = false;

    //    // 回到初始位置
    //    transform.position = startPosition;

    //    onHit.Invoke();
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FlyingSwordController sword = collision.GetComponent<FlyingSwordController>();
        if (sword == null) return;
        else
        {
            onHit.Invoke();

            canMove = false;
            transform.position = startPosition;
        }
    }
    //Player player = collision.GetComponent<Player>();
    //if (player == null) return;

    //player.lastAttacker = transform; // 取消無敵 player.stats.MakeInvincible(false);

    //PlayerStats stats = player.GetComponent<PlayerStats>();
    //if (stats == null) return;

    //StartCoroutine(HandleSpikeTrapSequence(player, stats));

    private IEnumerator ReturnToStartCoroutine()
    {
        // 停頓
        yield return new WaitForSeconds(hitPauseTime);

        // 緩緩回到起始位置
        Vector3 from = transform.position;
        float timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float t = timer / returnDuration;

            // 平滑插值（比較好看）
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(from, startPosition, t);
            yield return null;
        }

        transform.position = startPosition;
    }



    /// <summary>
    /// 如果你之後需要重置標靶（例如重新挑戰）
    /// </summary>
    //public void ResetTarget()
    //{
    //    triggered = false;
    //    canMove = true;
    //    transform.position = startPosition;
    //}
}
