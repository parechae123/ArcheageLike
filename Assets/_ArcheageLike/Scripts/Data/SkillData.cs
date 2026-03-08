using UnityEngine;
using ArcheageLike.Core;

namespace ArcheageLike.Data
{
    /// <summary>
    /// ScriptableObject defining a skill.
    /// ArcheAge has a tree-based skill system with combo chains.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "ArcheageLike/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Basic Info")]
        public int skillId;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Skill Type")]
        public SkillType skillType = SkillType.Active;
        public SkillTargetType targetType = SkillTargetType.SingleEnemy;
        public DamageType damageType = DamageType.Physical;

        [Header("Values")]
        public float baseDamage = 100f;
        public float scalingFactor = 1.0f; // multiplied by stat
        public float manaCost = 30f;
        public float cooldown = 5f;
        public float castTime = 0f;
        public float range = 10f;
        public float aoeRadius = 0f;

        [Header("Effects")]
        public float duration = 0f; // for DoT/HoT/buffs
        public float tickInterval = 1f;
        public StatusEffect statusEffect = StatusEffect.None;

        [Header("Combo")]
        public SkillData comboNextSkill; // unlocked after using this skill
        public float comboWindow = 3f;

        [Header("VFX/SFX")]
        public GameObject castVFXPrefab;
        public GameObject hitVFXPrefab;
        public AudioClip castSFX;
        public AudioClip hitSFX;
    }

    public enum SkillType
    {
        Active,
        Passive,
        Toggle
    }

    public enum SkillTargetType
    {
        Self,
        SingleEnemy,
        SingleAlly,
        AOEEnemy,
        AOEAlly,
        Directional,
        Ground
    }

    public enum StatusEffect
    {
        None,
        Stun,
        Slow,
        Root,
        Silence,
        Bleed,
        Poison,
        Burn,
        Heal,
        SpeedBuff,
        AttackBuff,
        DefenseBuff,
        Fear,
        Sleep,
        Knockback,
        Knockdown
    }
}
