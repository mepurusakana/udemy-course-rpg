using System.Collections;
using UnityEngine;

public class SpiritController : MonoBehaviour
{
    public float detectRadius = 8f;
    public float fireRate = 1f;
    public LayerMask enemyLayer;
    private int damage;
    private float lifeTime;
    private GameObject projectilePrefab;
    private Animator anim;

    private Transform target;
    private bool isIdle = false;

    public void Setup(int _damage, float _lifeTime, GameObject _projectilePrefab)
    {
        damage = _damage;
        lifeTime = _lifeTime;
        projectilePrefab = _projectilePrefab;

        anim = GetComponent<Animator>();
        StartCoroutine(SpiritLifecycle());
    }

    private IEnumerator SpiritLifecycle()
    {
        anim.SetTrigger("Enter"); // 進場動畫
        yield return new WaitForSeconds(0.5f);

        isIdle = true;
        anim.SetTrigger("Idle"); // Idle動畫

        float elapsed = 0f;
        while (elapsed < lifeTime)
        {
            FindTarget();
            if (target != null)
            {
                FireProjectile();
                yield return new WaitForSeconds(fireRate);
            }
            else
            {
                yield return null;
            }
            elapsed += Time.deltaTime;
        }

        anim.SetTrigger("Exit");
        Destroy(gameObject, 0.5f); // 播放退出動畫後刪除
    }

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        float closestDist = Mathf.Infinity;
        target = null;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                target = hit.transform;
            }
        }
    }

    private void FireProjectile()
    {
        if (target == null) return;

        GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 dir = (target.position - transform.position).normalized;
        bullet.GetComponent<SpiritProjectile>().Setup(damage, dir);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
