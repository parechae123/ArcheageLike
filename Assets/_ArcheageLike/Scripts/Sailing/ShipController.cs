using UnityEngine;
using ArcheageLike.Core;
using ArcheageLike.Data;

namespace ArcheageLike.Sailing
{
    /// <summary>
    /// ArcheAge-style ship controller.
    /// Ships float on water, respond to wind, and can be steered by the helm.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        [Header("Ship Data")]
        [SerializeField] private ShipData _shipData;

        [Header("Water Settings")]
        [SerializeField] private float _waterLevel = 0f;
        [SerializeField] private float _buoyancyForce = 10f;
        [SerializeField] private float _waterDrag = 1f;
        [SerializeField] private float _waterAngularDrag = 2f;

        [Header("Buoyancy Points")]
        [SerializeField] private Transform[] _buoyancyPoints;

        [Header("Wave Settings")]
        [SerializeField] private float _waveAmplitude = 0.5f;
        [SerializeField] private float _waveFrequency = 1f;
        [SerializeField] private float _waveSpeed = 1f;

        [Header("Interaction")]
        [SerializeField] private Transform _helmPosition;
        [SerializeField] private Transform _boardingPosition;
        [SerializeField] private Transform[] _passengerPositions;

        private Rigidbody _rb;
        private float _currentSpeed;
        private float _currentTurnInput;
        private bool _isPlayerControlled;
        private GameObject _pilot;
        private float _health;

        public bool IsPlayerControlled => _isPlayerControlled;
        public float CurrentSpeed => _currentSpeed;
        public float Health => _health;
        public float MaxHealth => _shipData != null ? _shipData.maxHealth : 1000f;
        public ShipData ShipData => _shipData;
        public Transform HelmPosition => _helmPosition;
        public Transform BoardingPosition => _boardingPosition;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.linearDamping = _waterDrag;
            _rb.angularDamping = _waterAngularDrag;
            _rb.mass = 500f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            if (_shipData != null)
                _health = _shipData.maxHealth;
        }

        private void FixedUpdate()
        {
            ApplyBuoyancy();

            if (_isPlayerControlled)
            {
                HandleSteering();
            }
        }

        private void ApplyBuoyancy()
        {
            if (_buoyancyPoints == null || _buoyancyPoints.Length == 0)
            {
                // Simple single-point buoyancy
                ApplyBuoyancyAtPoint(transform.position);
                return;
            }

            // Multi-point buoyancy for realistic tilting
            foreach (var point in _buoyancyPoints)
            {
                if (point != null)
                    ApplyBuoyancyAtPoint(point.position);
            }
        }

        private void ApplyBuoyancyAtPoint(Vector3 point)
        {
            float waveHeight = GetWaveHeight(point);
            float depth = waveHeight - point.y;

            if (depth > 0f)
            {
                float force = _buoyancyForce * depth;
                force = Mathf.Min(force, _buoyancyForce * 3f); // clamp
                _rb.AddForceAtPosition(Vector3.up * force, point, ForceMode.Force);
            }
        }

        private float GetWaveHeight(Vector3 position)
        {
            // Simple Gerstner-like wave approximation
            float wave = Mathf.Sin((position.x * _waveFrequency + Time.time * _waveSpeed)) * _waveAmplitude;
            wave += Mathf.Sin((position.z * _waveFrequency * 0.7f + Time.time * _waveSpeed * 1.3f)) * _waveAmplitude * 0.5f;
            return _waterLevel + wave;
        }

        private void HandleSteering()
        {
            var input = GameInputManager.Instance;
            if (input == null) return;

            float throttle = input.MoveInput.y;
            float steering = input.MoveInput.x;

            // Acceleration / Deceleration
            float maxSpeed = _shipData != null ? _shipData.maxSpeed : 10f;
            float accel = _shipData != null ? _shipData.acceleration : 3f;

            if (Mathf.Abs(throttle) > 0.1f)
            {
                _currentSpeed += throttle * accel * Time.fixedDeltaTime;
                _currentSpeed = Mathf.Clamp(_currentSpeed, -maxSpeed * 0.3f, maxSpeed);
            }
            else
            {
                // Natural deceleration
                float brakeForce = _shipData != null ? _shipData.brakeForce : 5f;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, brakeForce * Time.fixedDeltaTime * 0.3f);
            }

            // Apply forward force
            Vector3 forwardForce = transform.forward * _currentSpeed;
            _rb.AddForce(forwardForce, ForceMode.Acceleration);

            // Turning (only when moving)
            if (Mathf.Abs(_currentSpeed) > 0.5f)
            {
                float turnSpeed = _shipData != null ? _shipData.turnSpeed : 30f;
                float turnAmount = steering * turnSpeed * Time.fixedDeltaTime;
                turnAmount *= Mathf.Clamp01(Mathf.Abs(_currentSpeed) / 3f); // slower turns at low speed
                _rb.AddTorque(Vector3.up * turnAmount, ForceMode.Acceleration);
            }

            // Sails / boost with sprint
            if (input.IsRunning && Mathf.Abs(throttle) > 0.1f)
            {
                _rb.AddForce(transform.forward * accel * 0.5f, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Player boards the ship and takes control.
        /// </summary>
        public void BoardShip(GameObject player)
        {
            _pilot = player;
            _isPlayerControlled = true;

            // Disable player character controller, parent to helm
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.SetParent(_helmPosition != null ? _helmPosition : transform);
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;

            GameManager.Instance.ChangeState(GameState.Sailing);

            EventBus.Publish(new PlayerBoardShipEvent { Player = player, Ship = gameObject });

            Debug.Log($"[Ship] Player boarded {_shipData?.shipName ?? "ship"}");
        }

        /// <summary>
        /// Player exits the ship.
        /// </summary>
        public void ExitShip()
        {
            if (_pilot == null) return;

            _isPlayerControlled = false;
            _pilot.transform.SetParent(null);

            // Re-enable character controller
            var cc = _pilot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;

            // Place player at boarding position
            if (_boardingPosition != null)
                _pilot.transform.position = _boardingPosition.position;
            else
                _pilot.transform.position = transform.position + transform.right * 3f;

            EventBus.Publish(new PlayerExitShipEvent { Player = _pilot, Ship = gameObject });
            GameManager.Instance.ChangeState(GameState.FreeRoam);

            Debug.Log("[Ship] Player exited ship");
            _pilot = null;
        }

        public void TakeDamage(float amount)
        {
            _health -= amount;
            if (_health <= 0f)
            {
                _health = 0f;
                OnShipDestroyed();
            }
        }

        private void OnShipDestroyed()
        {
            Debug.Log($"[Ship] {_shipData?.shipName ?? "Ship"} destroyed!");
            if (_pilot != null)
                ExitShip();

            // TODO: Sink animation, debris spawning
            Destroy(gameObject, 3f);
        }

        private void OnDrawGizmosSelected()
        {
            if (_buoyancyPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var point in _buoyancyPoints)
                {
                    if (point != null)
                        Gizmos.DrawSphere(point.position, 0.3f);
                }
            }
        }
    }
}
