using UnityEngine;
using ArcheageLike.Core;

namespace ArcheageLike.Sailing
{
    /// <summary>
    /// Handles player-ship interaction (boarding, exiting).
    /// Attach to the ship GameObject.
    /// </summary>
    public class ShipInteraction : MonoBehaviour
    {
        [SerializeField] private float _interactionRange = 5f;
        [SerializeField] private string _interactionPrompt = "[F] 승선하기";

        private ShipController _shipController;
        private bool _playerInRange;
        private GameObject _nearbyPlayer;

        public bool PlayerInRange => _playerInRange;
        public string InteractionPrompt => _interactionPrompt;

        private void Awake()
        {
            _shipController = GetComponent<ShipController>();
        }

        private void Update()
        {
            CheckPlayerDistance();

            if (_playerInRange && !_shipController.IsPlayerControlled)
            {
                var input = GameInputManager.Instance;
                if (input != null && input.InteractPressed)
                {
                    _shipController.BoardShip(_nearbyPlayer);
                }
            }

            // Exit ship with F key while sailing
            if (_shipController.IsPlayerControlled)
            {
                var input = GameInputManager.Instance;
                if (input != null && input.InteractPressed)
                {
                    _shipController.ExitShip();
                }
            }
        }

        private void CheckPlayerDistance()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                _playerInRange = false;
                return;
            }

            float dist = Vector3.Distance(
                _shipController.BoardingPosition != null
                    ? _shipController.BoardingPosition.position
                    : transform.position,
                player.transform.position
            );

            _playerInRange = dist <= _interactionRange;
            _nearbyPlayer = _playerInRange ? player : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var center = _shipController?.BoardingPosition != null
                ? _shipController.BoardingPosition.position
                : transform.position;
            Gizmos.DrawWireSphere(center, _interactionRange);
        }
    }
}
