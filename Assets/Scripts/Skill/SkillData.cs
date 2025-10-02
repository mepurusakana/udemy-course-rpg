using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("基本設定")]
    public string skillName;
    public KeyCode activationKey;
    public float cooldown = 2f;

    [Header("技能預製體")]
    public GameObject skillPrefab;

    [Header("動畫設定")]
    public string animationBoolName;

    [Header("技能屬性")]
    public int damageAmount = 20;
    public float skillDuration = 1f;

    [Header("使用條件")]
    public bool requiresAirborne = false;
    public bool requiresGrounded = false;

    [Header("技能類型")]
    public bool isProjectile = false;
    public Transform spawnPoint;
    public bool isSummon = false;
    public bool isFlyingSword = false;
    public bool isDimensionGun = false;
}