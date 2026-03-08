using UnityEngine;
using ArcheageLike.Data;

namespace ArcheageLike.Housing
{
    /// <summary>
    /// Component for buildings that have been placed in the world.
    /// Handles health, interaction, and ownership.
    /// </summary>
    public class PlacedBuilding : MonoBehaviour
    {
        [Header("Runtime Data")]
        [SerializeField] private string _buildingName;
        [SerializeField] private float _currentHealth;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _buildProgress; // 0 to 1
        [SerializeField] private bool _isConstructed;

        private BuildingData _data;

        public string BuildingName => _buildingName;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float BuildProgress => _buildProgress;
        public bool IsConstructed => _isConstructed;
        public BuildingData Data => _data;

        public void Initialize(BuildingData data)
        {
            _data = data;
            _buildingName = data.buildingName;
            _maxHealth = data.maxHealth;
            _currentHealth = data.maxHealth;
            _buildProgress = 0f;
            _isConstructed = data.buildTime <= 0f;

            if (!_isConstructed)
            {
                // Start construction coroutine
                StartCoroutine(ConstructionProcess(data.buildTime));
            }
        }

        private System.Collections.IEnumerator ConstructionProcess(float buildTime)
        {
            float elapsed = 0f;
            while (elapsed < buildTime)
            {
                elapsed += Time.deltaTime;
                _buildProgress = elapsed / buildTime;

                // Scale up as it builds (simple visual feedback)
                float scale = Mathf.Lerp(0.1f, 1f, _buildProgress);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            _buildProgress = 1f;
            _isConstructed = true;
            transform.localScale = Vector3.one;
            Debug.Log($"[Housing] {_buildingName} construction complete!");
        }

        public void TakeDamage(float amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0f)
            {
                OnDestroyed();
            }
        }

        public void Repair(float amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }

        private void OnDestroyed()
        {
            Debug.Log($"[Housing] {_buildingName} destroyed!");
            // TODO: Drop materials, play destruction VFX
            Destroy(gameObject, 1f);
        }
    }
}
