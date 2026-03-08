using UnityEngine;

namespace ArcheageLike.Data
{
    /// <summary>
    /// ScriptableObject for building/furniture definitions.
    /// ArcheAge housing allows placing structures on designated housing zones.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuilding", menuName = "ArcheageLike/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Basic Info")]
        public int buildingId;
        public string buildingName;
        [TextArea] public string description;
        public Sprite icon;
        public BuildingCategory category;

        [Header("Placement")]
        public GameObject prefab;
        public GameObject ghostPrefab; // semi-transparent preview
        public Vector3 size = Vector3.one;
        public bool requiresFlatGround = true;
        public bool canRotate = true;
        public float rotationStep = 15f;

        [Header("Construction")]
        public float buildTime = 5f;
        public BuildingMaterial[] requiredMaterials;

        [Header("Properties")]
        public float maxHealth = 500f;
        public int storageSlots = 0;
        public bool isHouse = false;
        public bool isFarm = false;
    }

    public enum BuildingCategory
    {
        House,
        Farm,
        Crafting,
        Storage,
        Decoration,
        Fence,
        Workshop
    }

    [System.Serializable]
    public struct BuildingMaterial
    {
        public string materialName;
        public int amount;
    }
}
