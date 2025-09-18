using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public KeyCode activationKey = KeyCode.Alpha1;
    public float cooldown = 2f;
    public GameObject skillPrefab;
    public string animationTriggerName;
    public int damageAmount = 20;   // 斬擊固定傷害，可擴展倍率
    public float skillDuration = 1f; // 斬擊持續時間

    [Header("Usage Conditions")]
    public bool requiresAirborne;
    public bool requiresGrounded; // true = 必須在地面使用
    public bool isProjectile;

    [Header("Spirit Summon Settings")]
    public bool isSummon;
}