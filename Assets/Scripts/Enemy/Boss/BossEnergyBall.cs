using System.Collections;
using UnityEngine;

public class BossEnergyBall : MonoBehaviour
{
    [Header("Projectile Data")]
    public ProjectileData projectileData;

    private float fireTimer;
    private bool isFiring = true;

    private float runtimeRotationZ;

    private void Start()
    {
        if (projectileData == null)
        {
            Debug.LogError("ProjectileData is missing!");
            return;
        }

        runtimeRotationZ = projectileData.R_Offset.z;

        // 啟動彈幕
        StartCoroutine(FireRoutine());

        // EnergyBall 本體的存活時間
        //Destroy(gameObject, projectileData.LifeTime);
    }

    private void Update()
    {
        runtimeRotationZ += projectileData.RotationSpeed * Time.deltaTime;
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

            // 設定子彈行為
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
}
