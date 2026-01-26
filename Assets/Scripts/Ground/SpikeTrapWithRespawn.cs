using System.Collections;
using UnityEngine;

public class SpikeTrapWithRespawn : MonoBehaviour
{
    public Player player;

    [Header("傷害設定")]
    [SerializeField] private int damage = 10;

    [Header("回彈力道設定")]
    [SerializeField] private Vector2 bounceForce = new Vector2(8f, 12f);

    [Header("畫面淡出設定")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float pauseDuration = 1f;

    [Header("無敵時間設定")]
    [SerializeField] private float invincibilityAfterRespawn = 2f;

    [SerializeField] private bool canTriggerDuringInvincibility = true;
    private UI_FadeScreen fadeScreen;
    private bool isProcessing; // 防止連續觸發

    private void Start()
    {
        fadeScreen = FindObjectOfType<UI_FadeScreen>();

        if (fadeScreen == null)
            Debug.LogError("找不到 UI_FadeScreen！請確保場景中有淡入淡出UI物件");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isProcessing) return;

        Player player = collision.GetComponent<Player>();
        if (player == null) return;

        // 立刻鎖，避免同一幀吃輸入
        player.LockCompletely();

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        StartCoroutine(HandleSpikeTrapSequence(player, stats));
    }

    private IEnumerator HandleSpikeTrapSequence(Player player, PlayerStats stats)
    {
        isProcessing = true;

        // --- 階段 1：傷害 + 完全鎖定 ---
        stats.MakeInvincible(false);
        stats.TakeDamage(damage, transform);

        player.TakeDamageAndEnterHurtState(transform, bounceForce);

        //  完全鎖死
        player.LockCompletely();

        // 強制回 Idle（避免 Attack / Air State）
        player.stateMachine.ChangeState(player.idleState);

        // --- 階段 2：黑屏漸入 ---
        if (fadeScreen != null)
            fadeScreen.FadeOut();

        //Player.instance.GetOnBusy();
        //player.stateMachine.ChangeState(player.idleState);
        //player.rb.gravityScale = 0f;
        //player.SetZeroVelocity();

        yield return new WaitForSeconds(fadeOutDuration);
        yield return new WaitForSeconds(pauseDuration);

        // --- 階段 3：傳送玩家到安全點 ---
        Vector3 safePos = PlayerRespawnManager.instance.GetLastSafePosition();
        player.transform.position = safePos;
        player.SetZeroVelocity();


        // --- 階段 4：給予無敵狀態 ---
        stats.MakeInvincible(true);
        if (player.fx != null)
            player.fx.StartInvincibilityEffect();

        isProcessing = false;

        // --- 階段 5：黑屏漸出 ---
        if (fadeScreen != null)
            fadeScreen.FadeIn();
        yield return new WaitForSeconds(fadeInDuration);

        // --- 階段 6：恢復控制與重力 ---
        player.stateMachine.ChangeState(player.idleState);
        player.UnlockCompletely();
        //player.stateMachine.ChangeState(player.idleState);
        //player.rb.gravityScale = player.defaultGravity;
        //player.isBusy = false;

        // --- 階段 7：等待無敵結束 ---
        yield return new WaitForSeconds(invincibilityAfterRespawn);
        stats.MakeInvincible(false);

        if (player.fx != null)
            player.fx.StopInvincibilityEffect();

        Debug.Log("<color=yellow>[陷阱流程完成]</color>");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
    }
}
