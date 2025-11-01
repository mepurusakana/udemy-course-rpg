using UnityEngine;

public class Explosion : MonoBehaviour
{
    private int damage;

    public void Setup(int _damage)
    {
        damage = _damage;
        Destroy(gameObject, 0.5f); // 爆炸0.5秒後刪除
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Explosion hit: {collision.name}, Tag: {collision.tag}");
        if (collision.CompareTag("Enemy"))
        {
            CharacterStats enemyStats = collision.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(damage, this.transform);
                Debug.Log("敵人受傷成功！");
            }
        }
    }
}

