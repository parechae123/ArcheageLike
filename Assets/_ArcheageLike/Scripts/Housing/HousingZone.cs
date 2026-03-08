using UnityEngine;
using System.Collections.Generic;

namespace ArcheageLike.Housing
{
    /// <summary>
    /// Defines a housing zone where players can place buildings.
    /// ArcheAge uses designated zones with limited plots.
    /// </summary>
    public class HousingZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private string _zoneName = "Housing Zone";
        [SerializeField] private int _maxBuildings = 10;
        [SerializeField] private Vector3 _zoneSize = new Vector3(50f, 10f, 50f);

        [Header("Permissions")]
        [SerializeField] private bool _publicZone = true;

        private List<PlacedBuilding> _buildings = new List<PlacedBuilding>();

        public string ZoneName => _zoneName;
        public int CurrentBuildingCount => _buildings.Count;
        public int MaxBuildings => _maxBuildings;

        public bool CanPlace()
        {
            return _buildings.Count < _maxBuildings;
        }

        public void RegisterBuilding(PlacedBuilding building)
        {
            if (!_buildings.Contains(building))
                _buildings.Add(building);
        }

        public void UnregisterBuilding(PlacedBuilding building)
        {
            _buildings.Remove(building);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawCube(transform.position, _zoneSize);
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawWireCube(transform.position, _zoneSize);
        }
    }
}
