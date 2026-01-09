using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWave : MonoBehaviour
{
    public int damage = 40;

    private HashSet<PlayerStats> damagedTargets = new HashSet<PlayerStats>();

    private BoxCollider2D col;
    public Transform target;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerStats playerStats=collision.GetComponent<PlayerStats>();

        if (playerStats == null)
            return;

        if (damagedTargets.Contains(playerStats))
            return;

        if (playerStats != null)
        {
            playerStats.TakeDamage(damage, this.transform);

            damagedTargets.Add(playerStats);
        }
    }
}
