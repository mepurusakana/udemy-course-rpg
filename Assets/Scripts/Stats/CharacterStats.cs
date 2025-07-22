using System.Collections;
using UnityEngine;


public enum StatType
{
    damage,
    health,
}

public class CharacterStats : MonoBehaviour
{
    private EntityFX fx;

    [Header("Offensive stats")]
    public Stat damage;          // default value 150%

    [Header("Defensive stats")]
    public Stat maxHealth;

    public int currentHealth;

    public System.Action onHealthChanged;
    public bool isDead { get; private set; }
    public bool isInvincible { get; private set; }

    protected virtual void Start()
    {
        currentHealth = GetMaxHealthValue();

        fx = GetComponent<EntityFX>();
    }

    protected virtual void Update()
    {

    }



    public virtual void IncreaseStatBy(int _modifier, float _duration, Stat _statToModify)
    {
        // start corototuine for stat increase
        StartCoroutine(StatModCoroutine(_modifier, _duration, _statToModify));
    }

    private IEnumerator StatModCoroutine(int _modifier, float _duration, Stat _statToModify)
    {
        _statToModify.AddModifier(_modifier);

        yield return new WaitForSeconds(_duration);

        _statToModify.RemoveModifier(_modifier);
    }
    

    public virtual void DoDamage(CharacterStats _targetStats)
    {


        if (_targetStats.isInvincible)
            return;


        _targetStats.GetComponent<Entity>().SetupKnockbackDir(transform);

        int totalDamage = damage.GetValue();

        _targetStats.TakeDamage(totalDamage);
    }

    private void HitNearestTargetWithShockStrike()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 25);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && Vector2.Distance(transform.position, hit.transform.position) > 1)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);

                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }

            if (closestEnemy == null)            // delete if you don't want shocked target to be hit by shock strike
                closestEnemy = transform;
        }

    }


    public virtual void TakeDamage(int _damage)
    {

        if (isInvincible)
            return;

        DecreaseHealthBy(_damage);



        GetComponent<Entity>().DamageImpact();
        fx.StartCoroutine("FlashFX");

        if (currentHealth < 0 && !isDead)
            Die();


    }


    public virtual void IncreaseHealthBy(int _amount)
    {
        currentHealth += _amount;

        if (currentHealth > GetMaxHealthValue())
            currentHealth = GetMaxHealthValue();

        if(onHealthChanged != null)
            onHealthChanged();
    }


    protected virtual void DecreaseHealthBy(int _damage)
    {

        currentHealth -= _damage;

        if (_damage > 0)
            fx.CreatePopUpText(_damage.ToString());

        if (onHealthChanged != null)
            onHealthChanged();
    }

    protected virtual void Die()
    {
        isDead = true;
    }

    public void KillEntity()
    {
        if (!isDead)
            Die();
    }

    public void MakeInvincible(bool _invincible) => isInvincible = _invincible;

    public int GetMaxHealthValue()
    {
        return maxHealth.GetValue();
    }

    public Stat GetStat(StatType _statType)
    {
        if (_statType == StatType.damage) return damage;
        else if (_statType == StatType.health) return maxHealth;

        return null;
    }
}
