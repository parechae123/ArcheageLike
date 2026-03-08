using UnityEngine;
using UnityEngine.InputSystem;
using ArcheageLike.Core;

namespace ArcheageLike.Character
{
    /// <summary>
    /// ArcheAge-style 3rd person character controller.
    /// Click-to-move + WASD movement, with camera-relative direction.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterStats))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _jumpForce = 8f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Swimming")]
        [SerializeField] private float _swimSpeed = 3.5f;
        [SerializeField] private float _waterSurfaceY = 0f;
        [SerializeField] private bool _isSwimming;

        [Header("Click to Move")]
        [SerializeField] private LayerMask _clickMoveLayer;
        [SerializeField] private float _clickMoveStopDistance = 0.3f;

        private CharacterController _cc;
        private CharacterStats _stats;
        private Transform _cameraTransform;
        private Vector3 _velocity;
        private Vector3 _clickMoveTarget;
        private bool _isClickMoving;
        private bool _isGrounded;

        public bool IsMoving => _cc.velocity.magnitude > 0.1f;
        public bool IsGrounded => _isGrounded;
        public bool IsSwimming => _isSwimming;

        private void Start()
        {
            _cc = GetComponent<CharacterController>();
            _stats = GetComponent<CharacterStats>();
            _cameraTransform = Camera.main?.transform;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.UI ||
                GameManager.Instance.CurrentState == GameState.Dialogue)
                return;

            if (_stats.IsDead) return;

            CheckGround();
            CheckSwimming();
            HandleMovement();
            HandleClickToMove();
            ApplyGravity();
        }

        private void CheckGround()
        {
            _isGrounded = Physics.CheckSphere(
                transform.position + Vector3.down * 0.1f,
                _groundCheckRadius,
                _groundLayer
            );
        }

        private void CheckSwimming()
        {
            _isSwimming = transform.position.y < _waterSurfaceY;
        }

        private void HandleMovement()
        {
            var input = GameInputManager.Instance;
            if (input == null) return;

            Vector2 moveInput = input.MoveInput;
            if (moveInput.sqrMagnitude < 0.01f) return;

            // Cancel click-to-move when WASD is pressed
            _isClickMoving = false;

            float speed = input.IsRunning ? _runSpeed : _walkSpeed;
            if (_isSwimming) speed = _swimSpeed;

            // Camera-relative movement
            Vector3 forward = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            Vector3 right = _cameraTransform != null ? _cameraTransform.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
            _cc.Move(moveDir * speed * Time.deltaTime);

            // Rotate character to face movement direction
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }

            // Stamina consumption while running
            if (input.IsRunning && moveInput.sqrMagnitude > 0.01f)
            {
                _stats.UseStamina(10f * Time.deltaTime);
            }

            // Jump
            if (input.JumpPressed && _isGrounded && !_isSwimming)
            {
                _velocity.y = _jumpForce;
            }
        }

        private void HandleClickToMove()
        {
            var input = GameInputManager.Instance;
            if (input == null) return;

            // Right click to set destination (New Input System)
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame && !input.RightMouseHeld)
            {
                Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, _clickMoveLayer))
                {
                    _clickMoveTarget = hit.point;
                    _isClickMoving = true;
                }
            }

            if (!_isClickMoving) return;

            Vector3 direction = _clickMoveTarget - transform.position;
            direction.y = 0f;

            if (direction.magnitude < _clickMoveStopDistance)
            {
                _isClickMoving = false;
                return;
            }

            direction.Normalize();
            float speed = _isSwimming ? _swimSpeed : _walkSpeed;
            _cc.Move(direction * speed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_isSwimming)
            {
                // Buoyancy in water
                float buoyancy = (_waterSurfaceY - transform.position.y) * 5f;
                _velocity.y = Mathf.Clamp(buoyancy, -2f, 2f);
            }
            else if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }

            _cc.Move(_velocity * Time.deltaTime);
        }

        public void Teleport(Vector3 position)
        {
            _cc.enabled = false;
            transform.position = position;
            _cc.enabled = true;
            _isClickMoving = false;
        }
    }
}
