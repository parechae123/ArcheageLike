using UnityEngine;
using UnityEngine.UI;
using ArcheageLike.Core;

namespace ArcheageLike.UI
{
    /// <summary>
    /// Floating damage number popup.
    /// Spawned when damage is dealt, floats upward and fades.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _lifetime = 1.5f;
        [SerializeField] private float _fadeSpeed = 2f;
        [SerializeField] private Color _physicalColor = Color.white;
        [SerializeField] private Color _magicalColor = new Color(0.5f, 0.5f, 1f);
        [SerializeField] private Color _critColor = Color.yellow;
        [SerializeField] private Color _healColor = Color.green;

        private float _timer;
        private Vector3 _velocity;
        private Camera _cam;

        public void Setup(float amount, DamageType type, bool isCrit = false, bool isHeal = false)
        {
            _cam = Camera.main;
            _timer = _lifetime;

            if (_text != null)
            {
                _text.text = isHeal ? $"+{Mathf.CeilToInt(amount)}" : $"{Mathf.CeilToInt(amount)}";

                if (isHeal)
                    _text.color = _healColor;
                else if (isCrit)
                {
                    _text.color = _critColor;
                    _text.fontSize = Mathf.RoundToInt(_text.fontSize * 1.5f);
                    _text.text = $"{Mathf.CeilToInt(amount)}!";
                }
                else
                    _text.color = type == DamageType.Physical ? _physicalColor : _magicalColor;
            }

            // Random horizontal offset
            _velocity = new Vector3(Random.Range(-0.5f, 0.5f), _floatSpeed, 0f);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            // Float upward
            transform.position += _velocity * Time.deltaTime;

            // Face camera (billboard)
            if (_cam != null)
                transform.rotation = _cam.transform.rotation;

            // Fade
            if (_text != null)
            {
                float alpha = Mathf.Clamp01(_timer / _lifetime);
                var color = _text.color;
                color.a = alpha;
                _text.color = color;
            }

            if (_timer <= 0f)
                Destroy(gameObject);
        }
    }
}
