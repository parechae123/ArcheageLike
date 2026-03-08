using UnityEngine;

namespace ArcheageLike.Data
{
    /// <summary>
    /// ScriptableObject for ship definitions.
    /// ArcheAge has various ship types: rowboat, clipper, merchant ship, galleon, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShip", menuName = "ArcheageLike/Ship Data")]
    public class ShipData : ScriptableObject
    {
        [Header("Basic Info")]
        public string shipName;
        [TextArea] public string description;
        public ShipType shipType;
        public Sprite icon;

        [Header("Stats")]
        public float maxHealth = 5000f;
        public float maxSpeed = 10f;
        public float acceleration = 3f;
        public float turnSpeed = 30f;
        public float brakeForce = 5f;

        [Header("Capacity")]
        public int maxPassengers = 1;
        public int cargoSlots = 0;

        [Header("Combat (Optional)")]
        public int cannonSlots = 0;
        public float cannonDamage = 100f;
        public float cannonRange = 50f;
        public float cannonCooldown = 3f;

        [Header("Prefab")]
        public GameObject shipPrefab;
    }

    public enum ShipType
    {
        Rowboat,
        Clipper,
        MerchantShip,
        Galleon,
        FishingBoat
    }
}
