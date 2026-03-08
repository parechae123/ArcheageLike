using UnityEngine;

namespace ArcheageLike.Sailing
{
    /// <summary>
    /// Simple scrolling water plane for prototyping.
    /// For production, replace with a proper ocean shader (e.g., Stylized Water, Crest).
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class SimpleWater : MonoBehaviour
    {
        [Header("Wave Animation")]
        [SerializeField] private float _waveSpeed = 0.5f;
        [SerializeField] private float _waveScale = 1f;
        [SerializeField] private Vector2 _scrollDirection = new Vector2(1f, 0.5f);

        [Header("Visual")]
        [SerializeField] private Color _shallowColor = new Color(0.1f, 0.5f, 0.8f, 0.7f);
        [SerializeField] private Color _deepColor = new Color(0.02f, 0.1f, 0.3f, 0.9f);

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _mpb;
        private Vector2 _offset;

        private static readonly int MainTex_ST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void Update()
        {
            _offset += _scrollDirection.normalized * _waveSpeed * Time.deltaTime;

            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetVector(MainTex_ST, new Vector4(_waveScale, _waveScale, _offset.x, _offset.y));
                _renderer.SetPropertyBlock(_mpb);
            }
        }

        /// <summary>
        /// Get wave height at a world position.
        /// Used by ships and characters for water interaction.
        /// </summary>
        public float GetWaveHeight(Vector3 worldPosition)
        {
            float wave = Mathf.Sin(worldPosition.x * 0.5f + Time.time * _waveSpeed) * 0.3f;
            wave += Mathf.Cos(worldPosition.z * 0.3f + Time.time * _waveSpeed * 0.7f) * 0.2f;
            return transform.position.y + wave;
        }
    }
}
