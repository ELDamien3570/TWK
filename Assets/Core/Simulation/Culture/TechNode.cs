using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;

namespace TWK.Culture
{
    /// <summary>
    /// Represents a node in a culture's tech tree.
    /// Nodes can unlock buildings and grant modifiers.
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Culture/Tech Node", fileName = "New Tech Node")]
    public class TechNode : ScriptableObject
    {
        // ========== IDENTITY ==========
        [Header("Identity")]
        public string NodeName;
        [TextArea(3, 6)]
        public string Description;
        public Sprite Icon;

        // ========== TREE TYPE ==========
        [Header("Tech Tree")]
        [Tooltip("Which tech tree this node belongs to")]
        public TreeType TreeType;

        // ========== COST ==========
        [Header("Cost")]
        [Tooltip("Cost is always 1 point from the tech tree")]
        public int Cost = 1;

        // ========== PREREQUISITES ==========
        [Header("Prerequisites")]
        [Tooltip("Nodes that must be unlocked before this node becomes available")]
        public List<TechNode> Prerequisites = new List<TechNode>();

        // ========== UNLOCKS ==========
        [Header("Building Unlocks")]
        [Tooltip("Building definitions unlocked by this node")]
        public List<int> UnlockedBuildingDefinitionIDs = new List<int>();

        [Header("Modifiers")]
        [Tooltip("Modifiers granted when this node is unlocked")]
        public List<CultureModifier> Modifiers = new List<CultureModifier>();

        // ========== STATE (Runtime) ==========
        [System.NonSerialized]
        public bool IsUnlocked = false;

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Check if this node's prerequisites are met.
        /// </summary>
        public bool ArePrerequisitesMet()
        {
            foreach (var prereq in Prerequisites)
            {
                if (!prereq.IsUnlocked)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get unique ID for this node (uses Unity's instance ID).
        /// </summary>
        public int GetNodeID()
        {
            return GetInstanceID();
        }
    }
}
