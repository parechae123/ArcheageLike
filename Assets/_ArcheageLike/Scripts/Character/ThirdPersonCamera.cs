using UnityEngine;
using ArcheageLike.Core;

namespace ArcheageLike.Character
{
    /// <summary>
    /// ArcheAge-style third person camera.
    /// - Right mouse hold to rotate camera
    /// - Scroll wheel to zoom
    /// - Collision detection to prevent clipping
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Distance")]
        [SerializeField] private float _defaultDistance = 8f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 20f;
        [SerializeField] private float _zoomSpeed = 3f;
        [SerializeField] private float _zoomSmoothSpeed = 10f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 3f;
        [SerializeField] private float _minVerticalAngle = -30f;
        [SerializeField] private float _maxVerticalAngle = 70f;

        [Header("Collision")]
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private LayerMask _collisionLayers;

        private float _currentDistance;
        private float _targetDistance;
        private float _yaw;
        private float _pitch = 20f;
        private Vector3 _smoothVelocity;

        private void Start()
        {
            _currentDistance = _defaultDistance;
            _targetDistance = _defaultDistance;

            if (_target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _target = player.transform;
            }

            // Initialize rotation from current camera angle
            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;

            Cursor.lockState = CursorLockMode.None;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            var input = GameInputManager.Instance;
            if (input == null) return;

            HandleZoom(input);
            HandleRotation(input);
            UpdateCameraPosition();
        }

        private void HandleZoom(GameInputManager input)
        {
            if (Mathf.Abs(input.ScrollValue) > 0.01f)
            {
                _targetDistance -= input.ScrollValue * _zoomSpeed * 0.1f;
                _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            }

            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, _zoomSmoothSpeed * Time.deltaTime);
        }

        private void HandleRotation(GameInputManager input)
        {
            // Only rotate when right mouse is held (ArcheAge style)
            if (input.RightMouseHeld)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _yaw += input.LookInput.x * _rotationSpeed;
                _pitch -= input.LookInput.y * _rotationSpeed;
                _pitch = Mathf.Clamp(_pitch, _minVerticalAngle, _maxVerticalAngle);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void UpdateCameraPosition()
        {
            Vector3 targetPosition = _target.position + _targetOffset;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 direction = rotation * Vector3.back;

            // Collision detection
            float adjustedDistance = _currentDistance;
            if (Physics.SphereCast(targetPosition, _collisionRadius, direction, out RaycastHit hit,
                _currentDistance, _collisionLayers))
            {
                adjustedDistance = hit.distance - _collisionRadius;
                adjustedDistance = Mathf.Max(adjustedDistance, _minDistance * 0.5f);
            }

            Vector3 desiredPosition = targetPosition + direction * adjustedDistance;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _smoothVelocity, 0.05f);
            transform.LookAt(targetPosition);
        }

        /// <summary>
        /// Rotate camera to face the same direction as the target (for combat lock-on)
        /// </summary>
        public void SnapBehindTarget()
        {
            if (_target != null)
            {
                _yaw = _target.eulerAngles.y;
            }
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
