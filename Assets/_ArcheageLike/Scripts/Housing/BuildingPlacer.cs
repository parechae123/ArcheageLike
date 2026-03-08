using UnityEngine;
using UnityEngine.InputSystem;
using ArcheageLike.Core;
using ArcheageLike.Data;

namespace ArcheageLike.Housing
{
    /// <summary>
    /// ArcheAge-style building placement system.
    /// Shows a ghost preview that follows the mouse, snaps to grid,
    /// and validates placement before confirming.
    /// </summary>
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask _placementLayer;
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private float _maxPlacementDistance = 20f;
        [SerializeField] private float _maxSlopeAngle = 15f;

        [Header("Visual")]
        [SerializeField] private Material _validPlacementMat;
        [SerializeField] private Material _invalidPlacementMat;

        private BuildingData _currentBuilding;
        private GameObject _ghostObject;
        private bool _isPlacing;
        private bool _isValidPlacement;
        private float _currentRotation;

        public bool IsPlacing => _isPlacing;

        private void Update()
        {
            if (!_isPlacing) return;

            var input = GameInputManager.Instance;
            if (input == null) return;

            UpdateGhostPosition();

            // Rotate building
            if (input.RotateBuildingPressed && _currentBuilding.canRotate)
            {
                _currentRotation += _currentBuilding.rotationStep;
                if (_currentRotation >= 360f) _currentRotation -= 360f;
            }

            // Place building
            if (input.PlaceBuildingPressed && _isValidPlacement)
            {
                PlaceBuilding();
            }

            // Cancel
            if (input.CancelBuildingPressed || input.EscapePressed)
            {
                CancelPlacement();
            }
        }

        /// <summary>
        /// Start placing a building. Called from UI/inventory.
        /// </summary>
        public void StartPlacement(BuildingData buildingData)
        {
            if (_isPlacing) CancelPlacement();

            _currentBuilding = buildingData;
            _currentRotation = 0f;
            _isPlacing = true;

            // Create ghost preview
            var prefab = buildingData.ghostPrefab != null ? buildingData.ghostPrefab : buildingData.prefab;
            if (prefab != null)
            {
                _ghostObject = Instantiate(prefab);
                _ghostObject.name = $"Ghost_{buildingData.buildingName}";

                // Disable colliders on ghost
                foreach (var col in _ghostObject.GetComponentsInChildren<Collider>())
                    col.enabled = false;

                // Remove scripts from ghost
                foreach (var mb in _ghostObject.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (mb != null && mb.GetType() != typeof(Transform))
                        Destroy(mb);
                }
            }

            GameManager.Instance.ChangeState(GameState.Housing);
            Debug.Log($"[Housing] Started placing: {buildingData.buildingName}");
        }

        private void UpdateGhostPosition()
        {
            if (_ghostObject == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _maxPlacementDistance, _placementLayer))
            {
                // Snap to grid
                Vector3 position = hit.point;
                if (_gridSize > 0)
                {
                    position.x = Mathf.Round(position.x / _gridSize) * _gridSize;
                    position.z = Mathf.Round(position.z / _gridSize) * _gridSize;
                }

                _ghostObject.transform.position = position;
                _ghostObject.transform.rotation = Quaternion.Euler(0f, _currentRotation, 0f);

                // Validate placement
                _isValidPlacement = ValidatePlacement(position, hit.normal);
                UpdateGhostMaterial(_isValidPlacement);
            }
            else
            {
                _isValidPlacement = false;
                UpdateGhostMaterial(false);
            }
        }

        private bool ValidatePlacement(Vector3 position, Vector3 groundNormal)
        {
            // Check slope
            if (_currentBuilding.requiresFlatGround)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
                if (slopeAngle > _maxSlopeAngle)
                    return false;
            }

            // Check for overlapping buildings
            Vector3 halfExtents = _currentBuilding.size * 0.5f;
            var overlaps = Physics.OverlapBox(
                position + Vector3.up * halfExtents.y,
                halfExtents,
                Quaternion.Euler(0f, _currentRotation, 0f)
            );

            foreach (var col in overlaps)
            {
                if (col.GetComponent<PlacedBuilding>() != null)
                    return false;
            }

            // Check if within housing zone
            var zone = GetHousingZone(position);
            if (zone != null && !zone.CanPlace())
                return false;

            return true;
        }

        private void UpdateGhostMaterial(bool valid)
        {
            if (_ghostObject == null) return;

            Material mat = valid ? _validPlacementMat : _invalidPlacementMat;
            if (mat == null) return;

            foreach (var renderer in _ghostObject.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = mat;
                renderer.materials = mats;
            }
        }

        private void PlaceBuilding()
        {
            if (_currentBuilding == null || _ghostObject == null) return;

            // TODO: Check and consume required materials

            Vector3 position = _ghostObject.transform.position;
            Quaternion rotation = _ghostObject.transform.rotation;

            // Spawn actual building
            var building = Instantiate(_currentBuilding.prefab, position, rotation);
            var placedBuilding = building.AddComponent<PlacedBuilding>();
            placedBuilding.Initialize(_currentBuilding);

            EventBus.Publish(new BuildingPlacedEvent
            {
                Building = building,
                Position = position
            });

            Debug.Log($"[Housing] Placed: {_currentBuilding.buildingName} at {position}");

            CancelPlacement();
        }

        public void CancelPlacement()
        {
            if (_ghostObject != null)
                Destroy(_ghostObject);

            _ghostObject = null;
            _currentBuilding = null;
            _isPlacing = false;

            if (GameManager.Instance.CurrentState == GameState.Housing)
                GameManager.Instance.ChangeState(GameState.FreeRoam);
        }

        private HousingZone GetHousingZone(Vector3 position)
        {
            var colliders = Physics.OverlapSphere(position, 1f);
            foreach (var col in colliders)
            {
                var zone = col.GetComponent<HousingZone>();
                if (zone != null) return zone;
            }
            return null;
        }
    }
}
