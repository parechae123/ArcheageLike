using UnityEngine;
using UnityEngine.Events;

namespace ArcheageLike.Character
{
    /// <summary>
    /// Manages character stats (HP, MP, Stamina, etc.) with ArcheAge-like attributes.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float _maxHealth = 1000f;
        [SerializeField] private float _maxMana = 500f;
        [SerializeField] private float _maxStamina = 200f;

        [Header("Combat Stats")]
        [SerializeField] private float _strength = 10f;
        [SerializeField] private float _agility = 10f;
        [SerializeField] private float _intelligence = 10f;
        [SerializeField] private float _spirit = 10f;
        [SerializeField] private float _stamina = 10f;

        [Header("Derived Stats")]
        [SerializeField] private float _physicalAttack = 50f;
        [SerializeField] private float _magicAttack = 50f;
        [SerializeField] private float _physicalDefense = 30f;
        [SerializeField] private float _magicDefense = 30f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _attackSpeed = 1f;
        [SerializeField] private float _critRate = 0.05f;
        [SerializeField] private float _critDamage = 1.5f;

        // Runtime values
        public float CurrentHealth { get; private set; }
        public float CurrentMana { get; private set; }
        public float CurrentStamina { get; private set; }

        // Properties
        public float MaxHealth => _maxHealth;
        public float MaxMana => _maxMana;
        public float MaxStamina => _maxStamina;
        public float MoveSpeed => _moveSpeed;
        public float AttackSpeed => _attackSpeed;
        public float PhysicalAttack => _physicalAttack;
        public float MagicAttack => _magicAttack;
        public float PhysicalDefense => _physicalDefense;
        public float MagicDefense => _magicDefense;
        public float CritRate => _critRate;
        public float CritDamage => _critDamage;
        public bool IsDead => CurrentHealth <= 0f;

        // Events
        public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>(); // current, max
        public UnityEvent<float, float> OnManaChanged = new UnityEvent<float, float>();
        public UnityEvent<float, float> OnStaminaChanged = new UnityEvent<float, float>();
        public UnityEvent OnDeath = new UnityEvent();

        private void Awake()
        {
            RecalculateStats();
            CurrentHealth = _maxHealth;
            CurrentMana = _maxMana;
            CurrentStamina = _maxStamina;
        }

        public void RecalculateStats()
        {
            _physicalAttack = _strength * 3f + _agility * 1f;
            _magicAttack = _intelligence * 3f + _spirit * 1f;
            _physicalDefense = _stamina * 2f + _agility * 0.5f;
            _magicDefense = _spirit * 2f + _intelligence * 0.5f;
            _maxHealth = 500f + _stamina * 50f;
            _maxMana = 200f + _intelligence * 30f;
        }

        public void TakeDamage(float amount, Core.DamageType type = Core.DamageType.Physical)
        {
            if (IsDead) return;

            float defense = type == Core.DamageType.Physical ? _physicalDefense : _magicDefense;
            float reduction = defense / (defense + 100f); // diminishing returns formula
            float finalDamage = type == Core.DamageType.True ? amount : amount * (1f - reduction);

            CurrentHealth = Mathf.Max(0f, CurrentHealth - finalDamage);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (CurrentHealth <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        public bool UseMana(float amount)
        {
            if (CurrentMana < amount) return false;
            CurrentMana -= amount;
            OnManaChanged?.Invoke(CurrentMana, _maxMana);
            return true;
        }

        public void RestoreMana(float amount)
        {
            CurrentMana = Mathf.Min(_maxMana, CurrentMana + amount);
            OnManaChanged?.Invoke(CurrentMana, _maxMana);
        }

        public bool UseStamina(float amount)
        {
            if (CurrentStamina < amount) return false;
            CurrentStamina -= amount;
            OnStaminaChanged?.Invoke(CurrentStamina, _maxStamina);
            return true;
        }

        public void RestoreStamina(float amount)
        {
            CurrentStamina = Mathf.Min(_maxStamina, CurrentStamina + amount);
            OnStaminaChanged?.Invoke(CurrentStamina, _maxStamina);
        }
    }
}
