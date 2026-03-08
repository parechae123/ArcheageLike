using UnityEngine;

namespace ArcheageLike.UI
{
    /// <summary>
    /// Simple minimap using a secondary camera rendering to a RenderTexture.
    /// </summary>
    public class Minimap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Camera _minimapCamera;
        [SerializeField] private Transform _player;
        [SerializeField] private float _height = 50f;
        [SerializeField] private float _size = 30f;
        [SerializeField] private bool _rotateWithPlayer = true;

        private void Start()
        {
            if (_player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    _player = playerObj.transform;
            }

            if (_minimapCamera != null)
            {
                _minimapCamera.orthographic = true;
                _minimapCamera.orthographicSize = _size;
            }
        }

        private void LateUpdate()
        {
            if (_player == null || _minimapCamera == null) return;

            Vector3 pos = _player.position;
            pos.y = _height;
            _minimapCamera.transform.position = pos;

            if (_rotateWithPlayer)
            {
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, _player.eulerAngles.y, 0f);
            }
            else
            {
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        public void SetZoom(float size)
        {
            _size = size;
            if (_minimapCamera != null)
                _minimapCamera.orthographicSize = _size;
        }
    }
}
