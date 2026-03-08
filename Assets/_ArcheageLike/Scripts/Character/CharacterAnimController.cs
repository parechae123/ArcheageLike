using UnityEngine;

namespace ArcheageLike.Character
{
    /// <summary>
    /// Bridges character state to Animator parameters.
    /// Works with a standard humanoid Animator Controller.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimController : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _cc;
        private ThirdPersonController _controller;

        // Animator parameter hashes
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int IsSwimming = Animator.StringToHash("IsSwimming");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int SkillIndex = Animator.StringToHash("SkillIndex");
        private static readonly int UseSkill = Animator.StringToHash("UseSkill");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int Hit = Animator.StringToHash("Hit");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _cc = GetComponent<CharacterController>();
            _controller = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            if (_animator == null || _cc == null) return;

            float speed = new Vector3(_cc.velocity.x, 0, _cc.velocity.z).magnitude;
            _animator.SetFloat(Speed, speed, 0.1f, Time.deltaTime);
            _animator.SetBool(IsGrounded, _controller?.IsGrounded ?? true);
            _animator.SetBool(IsSwimming, _controller?.IsSwimming ?? false);
        }

        public void PlayJump() => _animator.SetTrigger(Jump);
        public void PlayAttack() => _animator.SetTrigger(Attack);

        public void PlaySkill(int index)
        {
            _animator.SetInteger(SkillIndex, index);
            _animator.SetTrigger(UseSkill);
        }

        public void PlayHit() => _animator.SetTrigger(Hit);
        public void SetDead(bool dead) => _animator.SetBool(IsDead, dead);
    }
}
