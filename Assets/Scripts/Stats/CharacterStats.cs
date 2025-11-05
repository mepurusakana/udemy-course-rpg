using System.Collections;
using UnityEngine;
using System;

public enum StatType
{
    damage,
    health,
}

public class CharacterStats : MonoBehaviour
{
    private EntityFX fx;

    [Header("Offensive stats")]
    public Stat damage; // 攻擊力

    [Header("Defensive stats")]
    public Stat maxHealth;
    public Stat maxMP;

    public int currentHealth;
    public int currentMP;

    public float mpRegenPerSecond = 1f;

    public bool isDead { get; protected set; }
    public bool isInvincible { get; private set; }

    private SpikeTrapWithRespawn trap;

    

    public System.Action onHealthChanged;
    // 事件：当MP改变时触发
    public System.Action onMPChanged;


    protected virtual void Start()
    {
        currentHealth = GetMaxHealthValue();
        currentMP = GetMaxMPValue();

        fx = GetComponent<EntityFX>();
    }

    protected virtual void Update()
    {
        RegenerateMP();
    }

    // 提升屬性暫時性 buff
    public virtual void IncreaseStatBy(int _modifier, float _duration, Stat _statToModify)
    {
        StartCoroutine(StatModCoroutine(_modifier, _duration, _statToModify));
    }

    private IEnumerator StatModCoroutine(int _modifier, float _duration, Stat _statToModify)
    {
        _statToModify.AddModifier(_modifier);
        yield return new WaitForSeconds(_duration);
        _statToModify.RemoveModifier(_modifier);
    }

    // 傷害邏輯
    public virtual void DoDamage(CharacterStats _targetStats, Transform _attacker)
    {
        if (_targetStats == null || _targetStats.isInvincible)
            return;

        Entity targetEntity = _targetStats.GetComponent<Entity>();
        if (targetEntity != null)
            targetEntity.SetupKnockbackDir(_attacker);

        int totalDamage = damage.GetValue();

        Player player = _targetStats.GetComponent<Player>();
        if (player != null)
            player.lastAttacker = _attacker;

        _targetStats.TakeDamage(totalDamage, _attacker);
    }

    public virtual void TakeDamage(int _damage, Transform _attacker)
    {
        if (isInvincible)
            return;

        DecreaseHealthBy(_damage);

        GetComponent<Entity>().DamageImpact();
        fx.StartCoroutine("FlashFX");

        if (currentHealth <= 0 && !isDead)
            Die();
    }

    public virtual void IncreaseHealthBy(int _amount)
    {
        currentHealth += _amount;
        currentHealth = Mathf.Min(currentHealth, GetMaxHealthValue());
        onHealthChanged?.Invoke();
    }

    protected virtual void DecreaseHealthBy(int _damage)
    {
        currentHealth -= _damage;
        if (_damage > 0)
            fx.CreatePopUpText(_damage.ToString());
        onHealthChanged?.Invoke();
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

    public int GetMaxHealthValue() => maxHealth.GetValue();
    public int GetMaxMPValue() => maxMP.GetValue();

    public Stat GetStat(StatType _statType)
    {
        if (_statType == StatType.damage) return damage;
        if (_statType == StatType.health) return maxHealth;
        return null;
    }

    private void RegenerateMP()
    {
        //  maxMP 是 Stat，要用 .GetValue() 取數值
        int maxMPValue = maxMP.GetValue();

        if (currentMP < maxMPValue)
        {
            currentMP = Mathf.Min(
                maxMPValue,
                currentMP + Mathf.FloorToInt(mpRegenPerSecond * Time.deltaTime)
            );

        }
    }

    public bool ConsumeMP(int amount)
    {
        int maxMPValue = maxMP.GetValue();

        if (currentMP >= amount)
        {
            currentMP -= amount;


            return true;
        }

        return false;
    }

    public void AddMP(int amount)
    {
        int maxMPValue = maxMP.GetValue();
        currentMP = Mathf.Min(maxMPValue, currentMP + amount);

    }
}
