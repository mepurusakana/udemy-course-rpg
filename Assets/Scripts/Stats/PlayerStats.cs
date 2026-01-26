using System.Collections;
using System.Reflection;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    private Player player;

    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
    }

    // 玩家被打才會進這裡（正確）
    public override void TakeDamage(int _damage, Transform _attacker)
    {
        if (isInvincible || isDead) return;

        // 真正有傷害才做受擊反饋
        if (_damage > 0 && player != null)
        {
            // 紀錄攻擊者
            if (_attacker != null)
                player.lastAttacker = _attacker;

            // 計算擊退方向（如果沒有 attacker 就用預設方向）
            Vector2 attackerPos = (_attacker != null) ? (Vector2)_attacker.position : (Vector2)player.transform.position + Vector2.left;
            Vector2 direction = ((Vector2)player.transform.position - attackerPos).normalized;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;

            Vector2 knockbackForce = new Vector2(direction.x * 1f, 2f);
            player.SetupKnockbackPower(knockbackForce);
        }

        // 扣血（基底內會進 DecreaseHealthBy）
        base.TakeDamage(_damage, _attacker);

        if (isDead || player == null) return;

        // 面朝敵人（有 attacker 才做）
        if (_attacker != null)
        {
            int faceDir = (_attacker.position.x > player.transform.position.x) ? 1 : -1;
            player.FlipController(faceDir);

            // 進入受擊狀態
            player.TakeDamageAndEnterHurtState(_attacker);
        }
        else
        {
            // 沒 attacker（陷阱等）也能進受擊狀態（看你 Player 的函式簽名）
            player.TakeDamageAndEnterHurtState(player.transform);
        }
    }

    protected override void DecreaseHealthBy(int _damage)
    {
        currentHealth -= _damage;

        if (_damage > 0 && player != null)
        {
            // 鏡頭晃動
            if (player.fx != null)
                player.fx.ScreenShake(1f);

            // 受傷模糊
            if (CameraPostFXController.instance != null)
                CameraPostFXController.instance.PlayHitBlur(0.35f, 0.12f);
        }
    }

    private static void TryScreenShake(Player player, string preferredFieldName, string fallbackFieldName)
    {
        if (player == null) return;

        // player.fx 可能是你自定義的特效系統（例如 PlayerFX）
        object fxObj = player.fx;
        if (fxObj == null) return;

        var fxType = fxObj.GetType();

        // 找 ScreenShake(???) 方法（通常是 ScreenShake(profile) 或 ScreenShake(setting)）
        MethodInfo screenShakeMethod = fxType.GetMethod("ScreenShake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (screenShakeMethod == null) return;

        // 先找 preferred 的震動設定（shakeLowDamage），找不到就 fallback（shakeHighDamage）
        object shakeSetting = GetFieldOrPropertyValue(fxObj, fxType, preferredFieldName)
                           ?? GetFieldOrPropertyValue(fxObj, fxType, "shakeDamage")
                           ?? GetFieldOrPropertyValue(fxObj, fxType, "shakeOnHit")
                           ?? GetFieldOrPropertyValue(fxObj, fxType, fallbackFieldName);

        if (shakeSetting == null) return;

        // 呼叫 ScreenShake(設定)
        var parameters = screenShakeMethod.GetParameters();
        if (parameters.Length == 1)
        {
            screenShakeMethod.Invoke(fxObj, new object[] { shakeSetting });
        }
    }

    private static object GetFieldOrPropertyValue(object obj, System.Type type, string name)
    {
        // Field
        var f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null) return f.GetValue(obj);

        // Property
        var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null) return p.GetValue(obj);

        return null;
    }

    private IEnumerator NotifyDeathToGameManager()
    {
        yield return new WaitForSeconds(0.2f);

        if (GameManager.instance != null)
            GameManager.instance.RespawnPlayer();
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, GetMaxHealthValue());
        onHealthChanged?.Invoke();
    }

    public void ResetOnRespawn()
    {
        isDead = false;
        currentHealth = GetMaxHealthValue();
        currentMP = GetMaxMPValue();

        player.chantCharges = 3;
        player.UpdateChantUI();

        onHealthChanged?.Invoke();
    }
}
