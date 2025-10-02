using System.Collections.Generic;
using UnityEngine;

public class SpearController : MonoBehaviour
{
    [Header("Flight")]
    public float speed = 12f;
    public float travelDistance = 15f;

    private int damage;
    private float direction;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform visual; // 預覽用子物件（含 SpriteRenderer / Animator）

    private Vector3 startPos;
    private bool isFired = false;
    private bool outroStarted = false;

    private HashSet<int> hitIds = new HashSet<int>(); // 防同一敵人被重複計算

    public bool HasFinishedIntro { get; private set; } = false;
    public bool HasFinishedOutro { get; private set; } = false;

    public void Setup(int _damage, float _direction)
    {
        damage = _damage;
        direction = Mathf.Sign(_direction) == 0 ? 1f : Mathf.Sign(_direction);

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visual = anim != null ? anim.transform : transform;

        startPos = transform.position;
        isFired = false;
        outroStarted = false;
        HasFinishedIntro = false;
        HasFinishedOutro = false;
        hitIds.Clear();

        // 翻轉子物件的顯示（不去改父物件 scale）
        Vector3 vScale = visual.localScale;
        visual.localScale = new Vector3(Mathf.Abs(vScale.x) * direction, vScale.y, vScale.z);

        // 初始不移動（等 intro 播完再 Fire）
        if (rb != null) rb.velocity = Vector2.zero;

        // 播 intro（如果 Animator 有設定 trigger）
        if (anim != null)
        {
            anim.SetTrigger("Intro");
        }
    }

    // 由子物件的 Animation Event 呼叫（或其他程式直接呼叫）
    public void OnIntroFinished()
    {
        HasFinishedIntro = true;
    }

    public void Fire()
    {
        if (isFired) return;
        isFired = true;

        if (rb != null)
        {
            rb.velocity = new Vector2(speed * direction, 0f);
        }

        if (anim != null)
            anim.SetTrigger("Fire");
    }

    private void Update()
    {
        if (isFired && !outroStarted)
        {
            // 用 X 距離來判定 travelled
            if (Mathf.Abs(transform.position.x - startPos.x) >= travelDistance)
            {
                if (anim != null) anim.SetTrigger("Outro");
                outroStarted = true;
            }
        }
    }

    // 由子物件的 Animation Event 呼叫（或其他程式直接呼叫）
    public void OnOutroFinished()
    {
        HasFinishedOutro = true;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFired) return;

        if (collision.CompareTag("Enemy"))
        {
            var enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats == null) return;

            int id = collision.gameObject.GetInstanceID();
            if (hitIds.Contains(id)) return; // 已命中過就跳過
            hitIds.Add(id);

            enemyStats.TakeDamage(damage);

            // 穿透設計 -> 不 Destroy(gameObject)
        }
    }
}
