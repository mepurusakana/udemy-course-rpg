using UnityEngine;
using System.Collections;

public class BossHand : MonoBehaviour
{
    [Header("Hand Settings")]
    public bool isLeftHand = true;
    public float sweepSpeed;
    public float swordStabSpeed;
    public int damage;

    [Header("References")]
    public EnemyBoss boss;
    public GameObject swordPrefab;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Sweep Settings")]
    public Vector3 sweepStartPos; // 橫掃開始位置
    public float sweepDistance; // 橫掃距離

    [Header("Impact Effect")]
    public GameObject groundImpactPrefab; // 地面衝擊特效

    [Header("Default (local)")]
    public Vector3 defaultLocalPos = Vector3.zero; // 可在 Inspector 設定

    private PolygonCollider2D handCollider;
    public bool isAttacking { get; private set; } = false;


    private void Awake()
    {
        handCollider = GetComponent<PolygonCollider2D>();
        if (handCollider != null)
            handCollider.enabled = false; // 初始關閉碰撞
    }

    private void Start()
    {
        if (defaultLocalPos == Vector3.zero)
            defaultLocalPos = transform.localPosition;
    }

    // 劍攻擊
    public void PerformSwordAttack()
    {
        if (isAttacking) return;
        StartCoroutine(SwordAttackRoutine());
    }

    private IEnumerator SwordAttackRoutine()
    {
        isAttacking = true; // 鎖定

        // 1. 移動到空中位置（你可以改成從 boss anchor 讀取）
        Vector3 airPosition = new Vector3(isLeftHand ? 3f : -3f, 5f, 0f);
        yield return StartCoroutine(MoveToPosition(airPosition, 1f));

        // 2. 生成劍
        GameObject sword = Instantiate(swordPrefab, transform);
        sword.transform.localPosition = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        // 3. 向下刺（使用 localPosition）
        bool hitGround = false;
        float stabDistance = 0f;
        float maxStabDistance = 100f;

        while (!hitGround && stabDistance < maxStabDistance)
        {
            transform.localPosition += Vector3.down * swordStabSpeed * Time.deltaTime;
            stabDistance += swordStabSpeed * Time.deltaTime;

            Vector2 worldPos = transform.position;
            if (Physics2D.Raycast(worldPos, Vector2.down, 0.5f, groundLayer))
            {
                hitGround = true;
            }

            yield return null;
        }

        // 4. 釋放地面衝擊
        if (groundImpactPrefab != null)
        {
            Vector3 impactPos = transform.position + Vector3.down * 1f;
            GameObject impact = Instantiate(groundImpactPrefab, impactPos, Quaternion.identity);
            Destroy(impact, 2f);
        }

        // 5. 銷毀劍
        Destroy(sword);
        yield return new WaitForSeconds(0.2f);

        // 回到預設位置
        yield return StartCoroutine(MoveToPosition(defaultLocalPos, 0.6f));

        isAttacking = false;
    }

    // 橫掃攻擊
    public void PerformSweepAttack()
    {
        if (isAttacking) return;
        StartCoroutine(SweepAttackRoutine());
    }

    private IEnumerator SweepAttackRoutine()
    {
        isAttacking = true;

        // 1. 移動到起始位置（對角）
        Vector3 startPos = new Vector3(isLeftHand ? 6f : -6f, -6f, 0f);
        yield return StartCoroutine(MoveToPosition(sweepStartPos, 0.6f));

        // 2. 啟用碰撞
        if (handCollider != null) handCollider.enabled = true;

        // 3. 快速橫掃（使用 localPosition）
        float sweepDir = isLeftHand ? -1f : 1f;
        float sweptDistance = 0f;

        while (sweptDistance < sweepDistance)
        {
            float moveAmount = sweepSpeed * Time.deltaTime;
            transform.localPosition += new Vector3(sweepDir * moveAmount, 0f, 0f);
            sweptDistance += Mathf.Abs(moveAmount);
            yield return null;
        }

        // 4. 關閉碰撞
        if (handCollider != null) handCollider.enabled = false;

        // 回到預設位置
        yield return StartCoroutine(MoveToPosition(defaultLocalPos, 0.4f));

        isAttacking = false;
    }

    // MoveToPosition (local)
    private IEnumerator MoveToPosition(Vector3 targetLocalPos, float duration)
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t);
            yield return null;
        }

        transform.localPosition = targetLocalPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking) return;

        PlayerStats player = collision.GetComponent<PlayerStats>();
        if (player != null)
        {
            boss.stats.DoDamage(player, transform);
        }
    }
}
