using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    private Enemy enemy;
    //private ItemDrop myDropSystem;
    public Stat soulsDropAmount;

    [Header("Level details")]
    [SerializeField] private int level = 1;

    [Range(0f, 1f)]
    [SerializeField] private float percantageModifier = .4f;

    protected override void Start()
    {
        soulsDropAmount.SetDefaultValue(100);
        ApplyLevelModifiers();

        base.Start();

        enemy = GetComponent<Enemy>();
        //myDropSystem = GetComponent<ItemDrop>();
    }

    private void ApplyLevelModifiers()
    {

        Modify(soulsDropAmount);
    }

    private void Modify(Stat _stat)
    {
        for (int i = 1; i < level; i++)
        {
            float modifier = _stat.GetValue() * percantageModifier;

            _stat.AddModifier(Mathf.RoundToInt(modifier));
        }
    }

    public override void TakeDamage(int _damage, Transform _attacker)
    {
        base.TakeDamage(_damage, _attacker);

        if (isDead) return;

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
            enemy.OnTakeDamage(_attacker);
    }


    protected override void Die()
    {
        base.Die();

        //myDropSystem.GenerateDrop();


        enemy.Die();



        Destroy(gameObject, 5f);
    }
}
