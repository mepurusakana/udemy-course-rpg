using System.Collections;
using UnityEngine;

public class SpikeTrapWithRespawn : MonoBehaviour
{
    [Header("傷害設定")]
    [SerializeField] private int damage = 10;

    [Header("回彈力道設定")]
    [SerializeField] private Vector2 bounceForce = new Vector2(8f, 12f);

    [Header("畫面淡出設定")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float pauseDuration = 0.3f;
    [SerializeField] private float hurtStateDuration = 0.4f; // 受擊狀態持續時間

    [Header("無敵時間設定")]
    [SerializeField] private float invincibilityAfterRespawn = 2f;
    [SerializeField] private bool canTriggerDuringInvincibility = true;

    private UI_FadeScreen fadeScreen;

    private void Start()
    {
        fadeScreen = FindObjectOfType<UI_FadeScreen>();

        if (fadeScreen == null)
        {
            Debug.LogError("找不到 UI_FadeScreen！請確保場景中有淡入淡出UI物件");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Player player = collision.GetComponent<Player>();
        player.stats.MakeInvincible(false);
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 檢查是否允許在無敵時觸發
                //if (!canTriggerDuringInvincibility && playerStats.isInvincible)
                //{
                //    return;
                //}
                StartCoroutine(HandleSpikeTrapSequence(player, playerStats));
            }
        }
    }

    private IEnumerator HandleSpikeTrapSequence(Player player, PlayerStats playerStats)
    {
        // === 階段 1：造成傷害和進入受擊狀態 === //
        playerStats.TakeDamage(damage);

        // 設置擊退力道並進入受擊狀態
        // HurtState 會自動處理：回彈、僵直、禁止操作
        player.TakeDamageAndEnterHurtState(transform, bounceForce);

        // 等待受擊狀態完成（玩家回彈和僵直）
        yield return new WaitForSeconds(hurtStateDuration);

        // === 階段 2：畫面淡出 === //

        // 停止所有移動並凍結玩家
        player.SetZeroVelocity();
        player.rb.gravityScale = 0;
        player.isBusy = true;

        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();
        }
        yield return new WaitForSeconds(fadeOutDuration);

        // === 階段 3：回溯位置（黑屏期間） === //

        Vector3 lastSafePosition = PlayerRespawnManager.instance.GetLastSafePosition();
        player.transform.position = lastSafePosition;

        // 重置玩家狀態
        player.stateMachine.ChangeState(player.idleState);
        player.SetZeroVelocity();
        player.rb.gravityScale = 20;

        // 黑屏停留
        yield return new WaitForSeconds(pauseDuration);

        // === 階段 4：無敵保護和特效 === //

        // 給予無敵狀態
        playerStats.MakeInvincible(true);

        // 啟動無敵特效
        PlayerFX playerFX = player.fx as PlayerFX;
        if (playerFX != null)
        {
            playerFX.StartPlayerInvincibilityEffect();
        }
        else
        {
            player.fx.StartInvincibilityEffect();
        }

        // === 階段 5：畫面淡入 === //

        if (fadeScreen != null)
        {
            fadeScreen.FadeIn();
        }
        yield return new WaitForSeconds(fadeInDuration);

        // 恢復玩家控制
        player.isBusy = false;

        // === 階段 6：無敵時間倒數 === //

        // 計算無敵剩餘時間
        float warningTime = 0.5f;
        float remainingTime = invincibilityAfterRespawn - fadeInDuration - warningTime;

        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);

            // 播放警告特效（無敵即將結束）
            if (playerFX != null)
            {
                playerFX.PlayInvincibilityEndWarning();
            }

            yield return new WaitForSeconds(warningTime);
        }
        else
        {
            yield return new WaitForSeconds(invincibilityAfterRespawn - fadeInDuration);
        }

        // === 階段 7：結束無敵 === //

        playerStats.MakeInvincible(false);

        if (playerFX != null)
        {
            playerFX.StopPlayerInvincibilityEffect();
        }
        else
        {
            player.fx.StopInvincibilityEffect();
        }


        Debug.Log("尖刺陷阱序列完成");
    }

    private void OnDrawGizmos()
    {
        // 繪製尖刺危險區域
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }

        // 繪製回彈方向參考線
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(bounceForce.x, bounceForce.y, 0) * 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-bounceForce.x, bounceForce.y, 0) * 0.1f);

        // 繪製尖刺圖示
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            "尖刺陷阱",
            new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } }
        );
#endif
    }
}