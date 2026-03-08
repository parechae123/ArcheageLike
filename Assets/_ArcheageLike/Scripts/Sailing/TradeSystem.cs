using UnityEngine;
using System.Collections.Generic;
using ArcheageLike.Character;
using ArcheageLike.Data;

namespace ArcheageLike.Sailing
{
    /// <summary>
    /// ArcheAge-style trade system.
    /// Craft trade packs at origin, deliver to distant trade posts for profit.
    /// Longer distance = higher profit. Sea routes pay more but riskier.
    /// </summary>
    public class TradeSystem : MonoBehaviour
    {
        [System.Serializable]
        public class TradeRoute
        {
            public string routeName;
            public Transform origin;
            public Transform destination;
            public float baseGoldReward = 100;
            public float distanceMultiplier = 1f; // calculated at runtime
        }

        [System.Serializable]
        public class TradePost
        {
            public string postName;
            public Transform location;
            public List<TradeRoute> availableRoutes = new List<TradeRoute>();
        }

        [Header("Settings")]
        [SerializeField] private float _profitPerDistance = 0.5f;
        [SerializeField] private float _seaRouteBonus = 1.5f;

        private List<TradePost> _tradePosts = new List<TradePost>();

        /// <summary>
        /// Calculate gold reward for delivering a trade pack.
        /// </summary>
        public int CalculateReward(Vector3 origin, Vector3 destination, bool seaRoute)
        {
            float distance = Vector3.Distance(origin, destination);
            float reward = distance * _profitPerDistance;

            if (seaRoute) reward *= _seaRouteBonus;

            // Time-based demand fluctuation
            float hour = (Time.time / 60f) % 24f;
            float demandMultiplier = 1f + Mathf.Sin(hour * Mathf.PI / 12f) * 0.3f;
            reward *= demandMultiplier;

            return Mathf.RoundToInt(reward);
        }

        /// <summary>
        /// Attempt to deliver a trade pack at the given position.
        /// </summary>
        public bool TryDeliver(GameObject player, Vector3 deliveryPoint, Vector3 originPoint, bool seaRoute)
        {
            var inventory = player.GetComponent<Inventory>();
            if (inventory == null) return false;

            // Check if player has a trade pack
            bool hasTradePack = false;
            foreach (var slot in inventory.Slots)
            {
                if (!slot.IsEmpty && slot.item.isTradePack)
                {
                    hasTradePack = true;
                    inventory.RemoveItem(slot.item, 1);
                    break;
                }
            }

            if (!hasTradePack)
            {
                Debug.Log("[Trade] No trade pack in inventory!");
                return false;
            }

            int reward = CalculateReward(originPoint, deliveryPoint, seaRoute);
            inventory.AddGold(reward);

            Debug.Log($"[Trade] Delivered! Earned {reward} gold.");
            return true;
        }

        public void RegisterTradePost(TradePost post)
        {
            _tradePosts.Add(post);
        }
    }

    /// <summary>
    /// Attach to a GameObject to mark it as a trade post.
    /// Players can deliver trade packs here.
    /// </summary>
    public class TradePostMarker : MonoBehaviour
    {
        [SerializeField] private string _postName = "Trade Post";
        [SerializeField] private float _interactionRange = 5f;
        [SerializeField] private bool _isSeaPort;

        public string PostName => _postName;
        public bool IsSeaPort => _isSeaPort;
        public float InteractionRange => _interactionRange;

        private void OnDrawGizmos()
        {
            Gizmos.color = _isSeaPort ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
}
