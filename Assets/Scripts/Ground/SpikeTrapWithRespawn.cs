using System.Collections;
using UnityEngine;

public class SpikeTrapWithRespawn : MonoBehaviour
{
    [Header("傷害設定")]
    [SerializeField] private int damage = 10;

    [Header("畫面淡出設定")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float pauseDuration = 0.3f; // 黑屏停留時間

    [Header("受擊僵直時間")]
    [SerializeField] private float stunDuration = 0.3f;

    private UI_FadeScreen fadeScreen;

    private void Start()
    {
        // 尋找場景中的淡入淡出UI
        fadeScreen = FindObjectOfType<UI_FadeScreen>();

        if (fadeScreen == null)
        {
            Debug.LogError("找不到 UI_FadeScreen！請確保場景中有淡入淡出UI物件");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            // 確保不會重複觸發
            if (playerStats != null && !playerStats.isInvincible)
            {
                StartCoroutine(HandleSpikeTrapSequence(player, playerStats));
            }
        }
    }

    private IEnumerator HandleSpikeTrapSequence(Player player, PlayerStats playerStats)
    {
        // 給予無敵避免重複觸發
        playerStats.MakeInvincible(true);

        // 1. 造成傷害
        playerStats.TakeDamage(damage);

        // 2. 播放受擊動畫並僵直玩家
        player.isBusy = true; // 防止玩家操作
        player.SetZeroVelocity(); // 停止移動

        // 等待僵直時間
        yield return new WaitForSeconds(stunDuration);

        // 3. 畫面淡出
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
        }
        yield return new WaitForSeconds(fadeOutDuration);

        // 4. 黑屏期間回溯玩家位置
        Vector3 lastSafePosition = PlayerRespawnManager.instance.GetLastSafePosition();
        player.transform.position = lastSafePosition;

        // 重置玩家狀態機到Idle
        player.stateMachine.ChangeState(player.idleState);
        player.SetZeroVelocity();

        // 黑屏停留
        yield return new WaitForSeconds(pauseDuration);

        // 5. 畫面淡入
        if (fadeScreen != null)
        {
            fadeScreen.FadeIn();
        }
        yield return new WaitForSeconds(fadeInDuration);

        // 6. 恢復玩家控制
        player.isBusy = false;
        playerStats.MakeInvincible(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
    }
}