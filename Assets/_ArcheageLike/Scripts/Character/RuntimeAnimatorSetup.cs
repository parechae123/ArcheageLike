using UnityEngine;

namespace ArcheageLike.Character
{
    /// <summary>
    /// Creates a basic AnimatorController at runtime for capsule prototype.
    /// Since we can't create AnimatorController at runtime,
    /// this drives scale/color changes as visual feedback instead.
    /// </summary>
    public class RuntimeAnimatorSetup : MonoBehaviour
    {
        private CharacterStats _stats;
        private ThirdPersonController _controller;
        private MeshRenderer _renderer;
        private Vector3 _baseScale;
        private Color _baseColor;
        private float _attackAnimTimer;
        private float _hitAnimTimer;
        private float _bobPhase;

        private void Start()
        {
            _stats = GetComponent<CharacterStats>();
            _controller = GetComponent<ThirdPersonController>();
            _renderer = GetComponent<MeshRenderer>();

            _baseScale = transform.localScale;
            if (_renderer != null)
                _baseColor = _renderer.material.color;
        }

        private void Update()
        {
            if (_stats != null && _stats.IsDead)
            {
                // Death: fall over (scale X)
                transform.localScale = new Vector3(_baseScale.x * 2f, _baseScale.y * 0.2f, _baseScale.z);
                return;
            }

            // Movement bob animation
            if (_controller != null && _controller.IsMoving)
            {
                _bobPhase += Time.deltaTime * 10f;
                float bob = Mathf.Sin(_bobPhase) * 0.05f;
                transform.localScale = _baseScale + new Vector3(0, bob, 0);
            }
            else
            {
                _bobPhase = 0;
                transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, Time.deltaTime * 5f);
            }

            // Swimming visual
            if (_controller != null && _controller.IsSwimming)
            {
                var s = transform.localScale;
                s.y = _baseScale.y * 0.6f;
                transform.localScale = s;
            }

            // Attack animation
            if (_attackAnimTimer > 0)
            {
                _attackAnimTimer -= Time.deltaTime;
                float t = _attackAnimTimer / 0.3f;
                transform.localScale = _baseScale * (1f + t * 0.2f);
            }

            // Hit flash
            if (_hitAnimTimer > 0)
            {
                _hitAnimTimer -= Time.deltaTime;
                if (_renderer != null)
                    _renderer.material.color = Color.Lerp(_baseColor, Color.red, _hitAnimTimer / 0.15f);
            }
        }

        public void PlayAttackAnim()
        {
            _attackAnimTimer = 0.3f;
        }

        public void PlayHitAnim()
        {
            _hitAnimTimer = 0.15f;
        }
    }
}
