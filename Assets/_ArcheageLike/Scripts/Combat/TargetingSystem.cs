using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ArcheageLike.Core;
using System.Collections.Generic;
using System.Linq;

namespace ArcheageLike.Combat
{
    /// <summary>
    /// ArcheAge-style tab targeting system with click-to-target support.
    /// </summary>
    public class TargetingSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _maxTargetRange = 30f;
        [SerializeField] private LayerMask _targetableLayers;
        [SerializeField] private float _tabTargetFOV = 90f;

        [Header("UI")]
        [SerializeField] private GameObject _targetIndicatorPrefab;

        private Transform _currentTarget;
        private GameObject _targetIndicator;
        private List<Transform> _nearbyTargets = new List<Transform>();
        private int _tabIndex = 0;

        public Transform CurrentTarget => _currentTarget;
        public UnityEvent<Transform> OnTargetChanged = new UnityEvent<Transform>();

        private void Update()
        {
            var input = GameInputManager.Instance;
            if (input == null) return;

            // Tab targeting
            if (input.TabTargetPressed)
            {
                CycleTarget();
            }

            // Click targeting (New Input System)
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                TryClickTarget();
            }

            // Validate current target
            if (_currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, _currentTarget.position);
                if (distance > _maxTargetRange || !_currentTarget.gameObject.activeInHierarchy)
                {
                    ClearTarget();
                }

                UpdateTargetIndicator();
            }

            // Escape to clear target
            if (input.EscapePressed)
            {
                ClearTarget();
            }
        }

        private void CycleTarget()
        {
            RefreshNearbyTargets();

            if (_nearbyTargets.Count == 0)
            {
                ClearTarget();
                return;
            }

            _tabIndex = (_tabIndex + 1) % _nearbyTargets.Count;
            SetTarget(_nearbyTargets[_tabIndex]);
        }

        private void TryClickTarget()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _maxTargetRange, _targetableLayers))
            {
                // Look for targetable component on hit object or parent
                var targetable = hit.collider.GetComponentInParent<Targetable>();
                if (targetable != null)
                {
                    SetTarget(targetable.transform);
                    return;
                }
            }
        }

        private void RefreshNearbyTargets()
        {
            _nearbyTargets.Clear();
            var colliders = Physics.OverlapSphere(transform.position, _maxTargetRange, _targetableLayers);

            foreach (var col in colliders)
            {
                var targetable = col.GetComponentInParent<Targetable>();
                if (targetable == null || targetable.transform == transform) continue;
                if (targetable.GetComponent<Character.CharacterStats>()?.IsDead == true) continue;

                // Check if in front of player (within FOV)
                Vector3 dirToTarget = (targetable.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                if (angle <= _tabTargetFOV * 0.5f)
                {
                    _nearbyTargets.Add(targetable.transform);
                }
            }

            // Sort by distance
            _nearbyTargets = _nearbyTargets
                .OrderBy(t => Vector3.Distance(transform.position, t.position))
                .ToList();
        }

        public void SetTarget(Transform target)
        {
            if (_currentTarget == target) return;
            _currentTarget = target;
            _tabIndex = _nearbyTargets.IndexOf(target);
            OnTargetChanged?.Invoke(_currentTarget);

            Debug.Log($"[Targeting] Target set: {target?.name ?? "None"}");
        }

        public void ClearTarget()
        {
            _currentTarget = null;
            _tabIndex = -1;
            OnTargetChanged?.Invoke(null);

            if (_targetIndicator != null)
                _targetIndicator.SetActive(false);
        }

        private void UpdateTargetIndicator()
        {
            if (_targetIndicatorPrefab == null || _currentTarget == null) return;

            if (_targetIndicator == null)
            {
                _targetIndicator = Instantiate(_targetIndicatorPrefab);
            }

            _targetIndicator.SetActive(true);
            _targetIndicator.transform.position = _currentTarget.position + Vector3.up * 0.1f;
        }

        public float GetDistanceToTarget()
        {
            if (_currentTarget == null) return float.MaxValue;
            return Vector3.Distance(transform.position, _currentTarget.position);
        }
    }
}
