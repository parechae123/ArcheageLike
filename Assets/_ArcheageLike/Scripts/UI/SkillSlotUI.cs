using UnityEngine;
using UnityEngine.UI;
using ArcheageLike.Data;

namespace ArcheageLike.UI
{
    /// <summary>
    /// Individual skill slot on the hotbar.
    /// Shows icon, cooldown overlay, and keybind.
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private Text _keybindText;
        [SerializeField] private Text _cooldownText;

        private SkillData _currentSkill;

        public void UpdateSlot(SkillData skill, float cooldownProgress)
        {
            _currentSkill = skill;

            if (_iconImage != null)
            {
                _iconImage.enabled = true;
                _iconImage.sprite = skill.icon;
            }

            if (_cooldownOverlay != null)
            {
                bool onCooldown = cooldownProgress < 1f;
                _cooldownOverlay.enabled = onCooldown;
                _cooldownOverlay.fillAmount = 1f - cooldownProgress;
            }

            if (_cooldownText != null)
            {
                float remaining = (1f - cooldownProgress) * skill.cooldown;
                _cooldownText.enabled = cooldownProgress < 1f;
                _cooldownText.text = remaining > 0 ? $"{remaining:F1}" : "";
            }
        }

        public void ClearSlot()
        {
            _currentSkill = null;
            if (_iconImage != null) _iconImage.enabled = false;
            if (_cooldownOverlay != null) _cooldownOverlay.enabled = false;
            if (_cooldownText != null) _cooldownText.enabled = false;
        }

        public void SetKeybind(string keybind)
        {
            if (_keybindText != null)
                _keybindText.text = keybind;
        }
    }
}
