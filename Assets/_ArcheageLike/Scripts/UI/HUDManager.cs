using UnityEngine;
using UnityEngine.UI;
using ArcheageLike.Character;
using ArcheageLike.Combat;
using ArcheageLike.Sailing;

namespace ArcheageLike.UI
{
    /// <summary>
    /// Main HUD controller. Manages HP/MP bars, target frame, and skill cooldowns.
    /// Attach to a Canvas GameObject.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Player Bars")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Slider _manaBar;
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _manaText;

        [Header("Target Frame")]
        [SerializeField] private GameObject _targetFrame;
        [SerializeField] private Text _targetNameText;
        [SerializeField] private Slider _targetHealthBar;
        [SerializeField] private Text _targetHealthText;

        [Header("Skill Bar")]
        [SerializeField] private SkillSlotUI[] _skillSlots;

        [Header("Ship HUD")]
        [SerializeField] private GameObject _shipHUD;
        [SerializeField] private Slider _shipHealthBar;
        [SerializeField] private Text _shipSpeedText;

        [Header("State Display")]
        [SerializeField] private Text _gameStateText;
        [SerializeField] private Text _interactionPromptText;

        [Header("Cast Bar")]
        [SerializeField] private GameObject _castBarObject;
        [SerializeField] private Slider _castBar;
        [SerializeField] private Text _castBarText;

        private CharacterStats _playerStats;
        private TargetingSystem _targetingSystem;
        private SkillSystem _skillSystem;

        private void Start()
        {
            // Find player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<CharacterStats>();
                _targetingSystem = player.GetComponent<TargetingSystem>();
                _skillSystem = player.GetComponent<SkillSystem>();

                // Subscribe to events
                if (_playerStats != null)
                {
                    _playerStats.OnHealthChanged.AddListener(UpdateHealthBar);
                    _playerStats.OnManaChanged.AddListener(UpdateManaBar);
                    _playerStats.OnStaminaChanged.AddListener(UpdateStaminaBar);
                }

                if (_targetingSystem != null)
                {
                    _targetingSystem.OnTargetChanged.AddListener(UpdateTargetFrame);
                }
            }

            // Initial state
            if (_targetFrame != null) _targetFrame.SetActive(false);
            if (_shipHUD != null) _shipHUD.SetActive(false);
            if (_castBarObject != null) _castBarObject.SetActive(false);
        }

        private void Update()
        {
            UpdateSkillBar();
            UpdateCastBar();
            UpdateTargetHealth();
            UpdateShipHUD();
            UpdateGameStateDisplay();
            UpdateInteractionPrompt();
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (_healthBar != null) _healthBar.value = current / max;
            if (_healthText != null) _healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void UpdateManaBar(float current, float max)
        {
            if (_manaBar != null) _manaBar.value = current / max;
            if (_manaText != null) _manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void UpdateStaminaBar(float current, float max)
        {
            if (_staminaBar != null) _staminaBar.value = current / max;
        }

        private void UpdateTargetFrame(Transform target)
        {
            if (_targetFrame == null) return;

            if (target == null)
            {
                _targetFrame.SetActive(false);
                return;
            }

            _targetFrame.SetActive(true);

            var targetable = target.GetComponent<Targetable>();
            if (_targetNameText != null && targetable != null)
                _targetNameText.text = targetable.DisplayName;

            UpdateTargetHealth();
        }

        private void UpdateTargetHealth()
        {
            if (_targetingSystem?.CurrentTarget == null) return;

            var targetStats = _targetingSystem.CurrentTarget.GetComponent<CharacterStats>();
            if (targetStats == null) return;

            if (_targetHealthBar != null)
                _targetHealthBar.value = targetStats.CurrentHealth / targetStats.MaxHealth;
            if (_targetHealthText != null)
                _targetHealthText.text = $"{Mathf.CeilToInt(targetStats.CurrentHealth)}/{Mathf.CeilToInt(targetStats.MaxHealth)}";
        }

        private void UpdateSkillBar()
        {
            if (_skillSystem == null || _skillSlots == null) return;

            for (int i = 0; i < _skillSlots.Length; i++)
            {
                if (i < _skillSystem.EquippedSkills.Count && _skillSystem.EquippedSkills[i] != null)
                {
                    var skill = _skillSystem.EquippedSkills[i];
                    float cdProgress = _skillSystem.GetCooldownProgress(skill.skillId);
                    _skillSlots[i].UpdateSlot(skill, cdProgress);
                }
                else
                {
                    _skillSlots[i].ClearSlot();
                }
            }
        }

        private void UpdateCastBar()
        {
            if (_skillSystem == null || _castBarObject == null) return;

            _castBarObject.SetActive(_skillSystem.IsCasting);
        }

        private void UpdateShipHUD()
        {
            if (_shipHUD == null) return;

            var state = Core.GameManager.Instance?.CurrentState;
            if (state != Core.GameState.Sailing)
            {
                _shipHUD.SetActive(false);
                return;
            }

            // Find current ship
            var ship = FindObjectOfType<ShipController>();
            if (ship == null || !ship.IsPlayerControlled)
            {
                _shipHUD.SetActive(false);
                return;
            }

            _shipHUD.SetActive(true);
            if (_shipHealthBar != null) _shipHealthBar.value = ship.Health / ship.MaxHealth;
            if (_shipSpeedText != null) _shipSpeedText.text = $"Speed: {ship.CurrentSpeed:F1} knots";
        }

        private void UpdateGameStateDisplay()
        {
            if (_gameStateText == null) return;
            var gm = Core.GameManager.Instance;
            if (gm != null)
                _gameStateText.text = gm.CurrentState.ToString();
        }

        private void UpdateInteractionPrompt()
        {
            if (_interactionPromptText == null) return;

            // Check for nearby ship interaction
            var shipInteraction = FindObjectOfType<ShipInteraction>();
            if (shipInteraction != null && shipInteraction.PlayerInRange)
            {
                _interactionPromptText.gameObject.SetActive(true);
                _interactionPromptText.text = shipInteraction.InteractionPrompt;
                return;
            }

            _interactionPromptText.gameObject.SetActive(false);
        }
    }
}
