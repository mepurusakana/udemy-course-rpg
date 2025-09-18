using System.Collections;
using UnityEngine;

public class SummonSpirit : MonoBehaviour
{
    public float detectionRadius = 8f;
    public float attackCooldown = 5f;
    public Transform firePoint;
    public GameObject bulletPrefab;

    private Animator anim;
    private Transform targetEnemy;
    private bool canAttack = true;
    private int facingDir = 1;

    private enum State { Enter, Idle, Attack }
    private State currentState;

    private void Start()
    {
        anim = GetComponent<Animator>();
        currentState = State.Enter;
        anim.SetTrigger("Enter");
    }

    public void Setup(int _facingDir)
    {
        facingDir = _facingDir;
        transform.localScale = new Vector3(facingDir, 1, 1);
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Enter:
                // 等待動畫事件回調進入 Idle
                break;
            case State.Idle:
                FindTarget();
                break;
            case State.Attack:
                // 攻擊動作動畫控制
                break;
        }
    }

    private void FindTarget()
    {
        if (!canAttack) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                targetEnemy = hit.transform;
                StartCoroutine(AttackRoutine());
                break;
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        currentState = State.Attack;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f); // 等動畫前搖

        if (targetEnemy != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<SpiritBullet>().Setup(targetEnemy.position);
        }

        yield return new WaitForSeconds(0.1f);
        currentState = State.Idle;

        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // 動畫事件用
    public void EnterToIdle()
    {
        currentState = State.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
