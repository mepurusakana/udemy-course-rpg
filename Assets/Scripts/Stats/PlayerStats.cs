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
    public override void TakeDamage(int _damage, Transform _attacker)
    {
        if (isInvincible || isDead) return;

        // 紀錄攻擊者
        player.lastAttacker = _attacker;

        // 計算擊退方向
        Vector2 direction = (player.transform.position - _attacker.position).normalized;
        Vector2 knockbackForce = new Vector2(direction.x * 1f, 2f);
        player.SetupKnockbackPower(knockbackForce);

        // 扣血
        base.TakeDamage(_damage, _attacker);

        if (isDead) return;

        // 調整面向方向：面朝敵人
        int faceDir = (_attacker.position.x > player.transform.position.x) ? 1 : -1;
        player.FlipController(faceDir);

        // 進入受擊狀態
        player.TakeDamageAndEnterHurtState(_attacker);
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
