using UnityEngine;
using ArcheageLike.Data;
using ArcheageLike.Core;

namespace ArcheageLike.Housing
{
    /// <summary>
    /// ArcheAge-style farm plot. Plant seeds, wait for growth, harvest crops.
    /// Consumes "Labor Points" (stamina) for planting/harvesting.
    /// </summary>
    public class FarmPlot : MonoBehaviour
    {
        public enum CropState { Empty, Growing, Ready, Withered }

        [Header("State")]
        [SerializeField] private CropState _state = CropState.Empty;
        [SerializeField] private ItemData _plantedSeed;
        [SerializeField] private float _growthTimer;
        [SerializeField] private float _growthDuration;
        [SerializeField] private float _witherTime = 300f; // 5 min after ready

        [Header("Visual")]
        private GameObject _cropVisual;

        public CropState State => _state;
        public float GrowthProgress => _growthDuration > 0 ? Mathf.Clamp01(_growthTimer / _growthDuration) : 0f;

        private void Update()
        {
            switch (_state)
            {
                case CropState.Growing:
                    _growthTimer += Time.deltaTime;
                    UpdateCropVisual();
                    if (_growthTimer >= _growthDuration)
                    {
                        _state = CropState.Ready;
                        _growthTimer = 0f;
                        Debug.Log($"[Farm] {_plantedSeed.itemName} is ready to harvest!");
                    }
                    break;

                case CropState.Ready:
                    _growthTimer += Time.deltaTime;
                    if (_growthTimer >= _witherTime)
                    {
                        _state = CropState.Withered;
                        UpdateCropVisual();
                        Debug.Log($"[Farm] {_plantedSeed.itemName} has withered!");
                    }
                    break;
            }
        }

        /// <summary>
        /// Plant a seed on this plot.
        /// </summary>
        public bool Plant(ItemData seed)
        {
            if (_state != CropState.Empty) return false;
            if (seed == null || !seed.isSeed) return false;

            _plantedSeed = seed;
            _growthDuration = seed.growTime;
            _growthTimer = 0f;
            _state = CropState.Growing;

            CreateCropVisual();
            Debug.Log($"[Farm] Planted {seed.itemName}. Growth time: {_growthDuration}s");
            return true;
        }

        /// <summary>
        /// Harvest the crop. Returns the item and amount.
        /// </summary>
        public bool Harvest(out ItemData harvestItem, out int harvestAmount)
        {
            harvestItem = null;
            harvestAmount = 0;

            if (_state != CropState.Ready)
            {
                if (_state == CropState.Withered)
                {
                    // Can still clear withered crops
                    ClearPlot();
                }
                return false;
            }

            harvestItem = _plantedSeed.harvestResult != null ? _plantedSeed.harvestResult : _plantedSeed;
            harvestAmount = _plantedSeed.harvestAmount;

            Debug.Log($"[Farm] Harvested {harvestAmount}x {harvestItem.itemName}!");
            ClearPlot();
            return true;
        }

        private void ClearPlot()
        {
            _state = CropState.Empty;
            _plantedSeed = null;
            _growthTimer = 0f;
            _growthDuration = 0f;

            if (_cropVisual != null)
                Destroy(_cropVisual);
        }

        private void CreateCropVisual()
        {
            if (_cropVisual != null) Destroy(_cropVisual);

            _cropVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cropVisual.name = "Crop";
            _cropVisual.transform.SetParent(transform);
            _cropVisual.transform.localPosition = Vector3.up * 0.1f;
            _cropVisual.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);

            // Remove collider
            var col = _cropVisual.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var renderer = _cropVisual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.5f, 0.1f);
                renderer.material = mat;
            }
        }

        private void UpdateCropVisual()
        {
            if (_cropVisual == null) return;

            float progress = GrowthProgress;
            float height = Mathf.Lerp(0.1f, 0.8f, progress);
            _cropVisual.transform.localScale = new Vector3(0.2f + progress * 0.2f, height, 0.2f + progress * 0.2f);

            var renderer = _cropVisual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color c = _state == CropState.Withered
                    ? new Color(0.5f, 0.4f, 0.1f)
                    : Color.Lerp(new Color(0.2f, 0.5f, 0.1f), new Color(0.1f, 0.8f, 0.1f), progress);
                renderer.material.color = c;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _state switch
            {
                CropState.Empty => Color.gray,
                CropState.Growing => Color.yellow,
                CropState.Ready => Color.green,
                CropState.Withered => new Color(0.5f, 0.3f, 0.1f),
                _ => Color.white
            };
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.1f, 1f));
        }
    }
}
