using System.Collections;
using UnityEngine;

public class BossEnergyBall : MonoBehaviour
{
    [Header("Projectile Data")]
    public ProjectileData projectileData;

    [Header("Fire Timing")]
    public float delayBeforeFire = 1f; //  進場後延遲
    public float fireDuration = 3f;       // 發射持續時間

    public int damage = 1;

    private Rigidbody2D rb;
    private CircleCollider2D col;

    private bool isFiring;
    private float runtimeRotationZ;

    private void Start()
    {
        if (projectileData == null)
        {
            Debug.LogError("ProjectileData is missing!");
            return;
        }

        runtimeRotationZ = projectileData.R_Offset.z;

        //  唯一入口
        StartCoroutine(FireLifecycle());
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = rb.GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        runtimeRotationZ += projectileData.RotationSpeed * Time.deltaTime;
    }

    // ============================
    //  發射生命週期總控
    // ============================
    private IEnumerator FireLifecycle()
    {
        // 1️進場後等待 0.6 秒
        yield return new WaitForSeconds(delayBeforeFire);

        // 2️開始發射
        isFiring = true;
        Coroutine fireRoutine = StartCoroutine(FireRoutine());

        // 3️持續發射 3 秒
        yield return new WaitForSeconds(fireDuration);

        // 4️停止發射
        isFiring = false;
        StopCoroutine(fireRoutine);
    }

    private IEnumerator FireRoutine()
    {
        while (isFiring)
        {
            FireOnce();
            yield return new WaitForSeconds(projectileData.CdTime);
        }
    }

    private void FireOnce()
    {
        int count = projectileData.Count;
        float angleStep = projectileData.Angle;
        float startAngle = -(count - 1) * angleStep * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;

            Quaternion rotation =
                Quaternion.Euler(0, 0, angle + runtimeRotationZ) *
                Quaternion.Euler(projectileData.R_Offset);

            Vector3 offset =
                rotation * Vector3.right * projectileData.CenterDis +
                projectileData.P_Offset;

            GameObject bullet = Instantiate(
                projectileData.Prefab,
                transform.position + offset,
                rotation
            );

            BossProjectile bulletScript = bullet.GetComponent<BossProjectile>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(
                    projectileData.Speed,
                    projectileData.LifeTime,
                    projectileData.SelfRotation
                );
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        // 碰到玩家
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage, this.transform);
            }
            //PlayHitAnimation();
        }
    }
}
