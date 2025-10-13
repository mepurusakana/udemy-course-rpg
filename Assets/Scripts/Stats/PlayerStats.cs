using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    private Player player;
    
    protected override void Start()
    {
        base.Start();

        player= GetComponent<Player>();
    }

    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);
    }

    protected override void Die()
    {
        base.Die();
        player.Die();

        GameManager.instance.lostCurrencyAmount = PlayerManager.instance.currency;
        PlayerManager.instance.currency = 0;
    }

    protected override void DecreaseHealthBy(int _damage)
    {
        base.DecreaseHealthBy(_damage);

        if (isDead)
            return;

        if (_damage > GetMaxHealthValue() * .3f )
        {
            player.SetupKnockbackPower(new Vector2(10,6));
            player.fx.ScreenShake(player.fx.shakeHighDamage);


            int randomSound = Random.Range(34, 35);
            AudioManager.instance.PlaySFX(randomSound, null);
            
        }
    }

    public void SetHealth(int newHealth)
    {
        currentHealth=Mathf.Clamp(newHealth, 0, GetMaxHealthValue());

        if (onHealthChanged != null)
            onHealthChanged();

        if (currentHealth <= 0 && !isDead)
            Die();
    }
}
