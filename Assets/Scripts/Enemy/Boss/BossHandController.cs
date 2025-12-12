using UnityEngine;

public class BossHandController : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;
    public LayerMask playerLayer;

    private Animator anim;
    private EnemyBoss parentBoss;

    private void Start()
    {
        anim = GetComponent<Animator>();
        parentBoss = GetComponentInParent<EnemyBoss>();
    }

    // 在動畫事件中調用
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    // 對玩家造成傷害
                    playerStats.TakeDamage(damage,_attacker);
                    Debug.Log("手部攻擊命中玩家！");
                }
            }
        }
    }

    // 動畫結束時重置
    public void OnAttackFinished()
    {
        anim.SetBool("LeftSword", false);
        anim.SetBool("LeftProjectile", false);
        anim.SetBool("RightSword", false);
        anim.SetBool("RightProjectile", false);
    }
}
