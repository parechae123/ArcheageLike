using UnityEngine;

namespace ArcheageLike.Combat
{
    /// <summary>
    /// Marks a GameObject as targetable by the targeting system.
    /// </summary>
    public class Targetable : MonoBehaviour
    {
        public enum TargetFaction
        {
            Player,
            Friendly,
            Neutral,
            Hostile
        }

        [SerializeField] private TargetFaction _faction = TargetFaction.Hostile;
        [SerializeField] private string _displayName = "Unknown";

        public TargetFaction Faction => _faction;
        public string DisplayName => _displayName;
    }
}
