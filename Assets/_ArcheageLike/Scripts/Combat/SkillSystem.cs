using UnityEngine;
using System.Collections.Generic;
using ArcheageLike.Core;
using ArcheageLike.Data;
using ArcheageLike.Character;

namespace ArcheageLike.Combat
{
    /// <summary>
    /// ArcheAge-style skill system with cooldowns, combos, and GCD.
    /// Each character can equip skills from multiple skill trees.
    /// </summary>
    public class SkillSystem : MonoBehaviour
    {
        [Header("Skill Slots (Hotbar)")]
        [SerializeField] private List<SkillData> _equippedSkills = new List<SkillData>();

        [Header("Settings")]
        [SerializeField] private float _globalCooldown = 1.0f;

        private CharacterStats _stats;
        private TargetingSystem _targeting;
        private CharacterAnimController _animController;

        private Dictionary<int, float> _cooldownTimers = new Dictionary<int, float>();
        private float _gcdTimer;
        private bool _isCasting;
        private float _castTimer;
        private SkillData _castingSkill;

        // Combo tracking
        private SkillData _lastUsedSkill;
        private float _comboTimer;

        public List<SkillData> EquippedSkills => _equippedSkills;
        public bool IsCasting => _isCasting;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _targeting = GetComponent<TargetingSystem>();
            _animController = GetComponent<CharacterAnimController>();
        }

        private void Update()
        {
            UpdateTimers();
            HandleInput();
        }

        private void UpdateTimers()
        {
            // GCD
            if (_gcdTimer > 0) _gcdTimer -= Time.deltaTime;

            // Individual cooldowns
            var keys = new List<int>(_cooldownTimers.Keys);
            foreach (var key in keys)
            {
                _cooldownTimers[key] -= Time.deltaTime;
                if (_cooldownTimers[key] <= 0f)
                    _cooldownTimers.Remove(key);
            }

            // Combo timer
            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                    _lastUsedSkill = null;
            }

            // Cast timer
            if (_isCasting)
            {
                _castTimer -= Time.deltaTime;
                if (_castTimer <= 0f)
                {
                    ExecuteSkill(_castingSkill);
                    _isCasting = false;
                    _castingSkill = null;
                }
            }
        }

        private void HandleInput()
        {
            var input = GameInputManager.Instance;
            if (input == null) return;

            if (input.Skill1Pressed && _equippedSkills.Count > 0) TryUseSkill(0);
            if (input.Skill2Pressed && _equippedSkills.Count > 1) TryUseSkill(1);
            if (input.Skill3Pressed && _equippedSkills.Count > 2) TryUseSkill(2);
            if (input.Skill4Pressed && _equippedSkills.Count > 3) TryUseSkill(3);
            if (input.AttackPressed) TryBasicAttack();
        }

        public bool TryUseSkill(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _equippedSkills.Count) return false;

            var skill = _equippedSkills[slotIndex];
            if (skill == null) return false;

            // Check for combo override
            if (_lastUsedSkill?.comboNextSkill != null && _comboTimer > 0)
            {
                // If this slot's skill is the combo follow-up, use it
                if (skill == _lastUsedSkill.comboNextSkill)
                {
                    skill = _lastUsedSkill.comboNextSkill;
                }
            }

            return TryUseSkill(skill);
        }

        public bool TryUseSkill(SkillData skill)
        {
            if (_stats.IsDead || _isCasting) return false;

            // GCD check
            if (_gcdTimer > 0) return false;

            // Cooldown check
            if (_cooldownTimers.ContainsKey(skill.skillId)) return false;

            // Mana check
            if (!_stats.UseMana(skill.manaCost)) return false;

            // Range check for targeted skills
            if (skill.targetType == SkillTargetType.SingleEnemy || skill.targetType == SkillTargetType.AOEEnemy)
            {
                if (_targeting != null && _targeting.CurrentTarget != null)
                {
                    float dist = _targeting.GetDistanceToTarget();
                    if (dist > skill.range)
                    {
                        Debug.Log($"[Skill] {skill.skillName}: Target out of range ({dist:F1}/{skill.range})");
                        return false;
                    }
                }
                else if (skill.targetType == SkillTargetType.SingleEnemy)
                {
                    Debug.Log($"[Skill] {skill.skillName}: No target selected");
                    return false;
                }
            }

            // Start cast or instant
            if (skill.castTime > 0f)
            {
                StartCasting(skill);
            }
            else
            {
                ExecuteSkill(skill);
            }

            // Apply GCD and cooldown
            _gcdTimer = _globalCooldown;
            _cooldownTimers[skill.skillId] = skill.cooldown;

            // Combo tracking
            _lastUsedSkill = skill;
            _comboTimer = skill.comboWindow;

            // Animation
            _animController?.PlaySkill(skill.skillId % 4);

            // Event
            EventBus.Publish(new SkillUsedEvent { Caster = gameObject, SkillId = skill.skillId });

            return true;
        }

        private void StartCasting(SkillData skill)
        {
            _isCasting = true;
            _castTimer = skill.castTime;
            _castingSkill = skill;

            // Spawn cast VFX
            if (skill.castVFXPrefab != null)
            {
                Instantiate(skill.castVFXPrefab, transform.position + Vector3.up, Quaternion.identity);
            }

            Debug.Log($"[Skill] Casting {skill.skillName}... ({skill.castTime}s)");
        }

        private void ExecuteSkill(SkillData skill)
        {
            float attackStat = skill.damageType == DamageType.Physical
                ? _stats.PhysicalAttack
                : _stats.MagicAttack;

            float totalDamage = skill.baseDamage + attackStat * skill.scalingFactor;

            // Crit
            if (Random.value < _stats.CritRate)
            {
                totalDamage *= _stats.CritDamage;
                Debug.Log("[Skill] CRITICAL HIT!");
            }

            switch (skill.targetType)
            {
                case SkillTargetType.SingleEnemy:
                    ApplyDamageToTarget(totalDamage, skill);
                    break;

                case SkillTargetType.AOEEnemy:
                    ApplyAOEDamage(totalDamage, skill);
                    break;

                case SkillTargetType.Self:
                case SkillTargetType.SingleAlly:
                    _stats.Heal(totalDamage);
                    break;

                case SkillTargetType.Ground:
                case SkillTargetType.Directional:
                    ApplyAOEDamage(totalDamage, skill);
                    break;
            }

            Debug.Log($"[Skill] {skill.skillName} executed! Damage: {totalDamage:F0}");
        }

        private void ApplyDamageToTarget(float damage, SkillData skill)
        {
            if (_targeting?.CurrentTarget == null) return;

            var targetStats = _targeting.CurrentTarget.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage, skill.damageType);

                EventBus.Publish(new DamageEvent
                {
                    Source = gameObject,
                    Target = _targeting.CurrentTarget.gameObject,
                    Amount = damage,
                    Type = skill.damageType
                });

                // Hit VFX
                if (skill.hitVFXPrefab != null)
                {
                    Instantiate(skill.hitVFXPrefab, _targeting.CurrentTarget.position + Vector3.up, Quaternion.identity);
                }
            }
        }

        private void ApplyAOEDamage(float damage, SkillData skill)
        {
            Vector3 center = _targeting?.CurrentTarget != null
                ? _targeting.CurrentTarget.position
                : transform.position + transform.forward * skill.range * 0.5f;

            var colliders = Physics.OverlapSphere(center, skill.aoeRadius);
            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;
                var stats = col.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.TakeDamage(damage, skill.damageType);
                }
            }
        }

        private void TryBasicAttack()
        {
            if (_stats.IsDead || _isCasting) return;
            if (_targeting?.CurrentTarget == null) return;
            if (_gcdTimer > 0) return;

            float damage = _stats.PhysicalAttack;
            var targetStats = _targeting.CurrentTarget.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damage, DamageType.Physical);
                _animController?.PlayAttack();
                _gcdTimer = 1f / _stats.AttackSpeed;

                EventBus.Publish(new DamageEvent
                {
                    Source = gameObject,
                    Target = _targeting.CurrentTarget.gameObject,
                    Amount = damage,
                    Type = DamageType.Physical
                });
            }
        }

        public float GetCooldownRemaining(int skillId)
        {
            return _cooldownTimers.ContainsKey(skillId) ? _cooldownTimers[skillId] : 0f;
        }

        public float GetCooldownProgress(int skillId)
        {
            var skill = _equippedSkills.Find(s => s != null && s.skillId == skillId);
            if (skill == null) return 1f;

            float remaining = GetCooldownRemaining(skillId);
            if (remaining <= 0f) return 1f;
            return 1f - (remaining / skill.cooldown);
        }
    }
}
