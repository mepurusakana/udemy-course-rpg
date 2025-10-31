using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    private Player player;
    
    protected override void Start()
    {
        base.Start();

        player= GetComponent<Player>();
    }

    // PlayerStats.cs
    public override void TakeDamage(int _damage)
    {
        if (isInvincible || isDead) return;

        Vector2 knockbackDirection = Vector2.zero;
        if (player != null && player.lastAttacker != null)
        {
            knockbackDirection = (player.transform.position - player.lastAttacker.position).normalized;
            player.SetupKnockbackPower(knockbackDirection * new Vector2(8, 12));
        }

        base.TakeDamage(_damage);

        if (isDead) return;

        if (player != null && player.stateMachine != null)
        {
            player.TakeDamageAndEnterHurtState(player.lastAttacker);
        }
    }



    protected override void DecreaseHealthBy(int _damage)
    {
        currentHealth -= _damage;

        if (_damage > 0 && _damage > GetMaxHealthValue() * .3f)
        {
            if (player != null)
            {
                player.SetupKnockbackPower(new Vector2(10, 6));
                player.fx.ScreenShake(player.fx.shakeHighDamage);

                int randomSound = Random.Range(34, 35);
                AudioManager.instance.PlaySFX(randomSound, null);
            }
        }

        // 如果血量 <= 0
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;

            // 進入 HurtState
            if (player != null)
            {
                player.TakeDamageAndEnterHurtState(player.transform, Vector2.zero);

                // 延遲呼叫 GameManager 重生
                player.StartCoroutine(NotifyDeathToGameManager());
            }
        }
    }

    private IEnumerator NotifyDeathToGameManager()
    {
        // 等受擊動畫或短暫延遲
        yield return new WaitForSeconds(0.2f);

        // 呼叫 GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.RespawnPlayer();
        }
    }

    public void SetHealth(int newHealth)
    {
        currentHealth=Mathf.Clamp(newHealth, 0, GetMaxHealthValue());

        if (onHealthChanged != null)
            onHealthChanged();
    }

    public void ResetHealthOnRespawn()
    {
        isDead = false;
        currentHealth = GetMaxHealthValue();

        if (onHealthChanged != null)
            onHealthChanged();
    }
}
