using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Cultures;
using TWK.Economy;

namespace TWK.Cultures
{
    /// <summary>
    /// Represents a tech tree for a specific TreeType within a culture.
    /// Tracks XP accumulation and node unlocks.
    /// </summary>
    [System.Serializable]
    public class CultureTechTree
    {
        // ========== IDENTITY ==========
        public TreeType TreeType;

        // ========== XP TRACKING ==========
        [Tooltip("Accumulated XP (tech points) available to spend")]
        public float AccumulatedXP = 0f;

        [Tooltip("Total XP earned over the culture's lifetime")]
        public float TotalXPEarned = 0f;

        // ========== NODES ==========
        [Tooltip("All nodes in this tech tree")]
        public List<TechNode> AllNodes = new List<TechNode>();

        [Tooltip("IDs of unlocked nodes")]
        public List<int> UnlockedNodeIDs = new List<int>();

        // ========== OWNERSHIP ==========
        [Tooltip("Realm ID of the culture leader (largest population)")]
        public int OwnerRealmID = -1;

        // ========== CONSTRUCTOR ==========
        public CultureTechTree(TreeType treeType)
        {
            TreeType = treeType;
            AllNodes = new List<TechNode>();
            UnlockedNodeIDs = new List<int>();
        }

        // ========== XP METHODS ==========

        /// <summary>
        /// Add XP to this tech tree (from buildings, culture leader bonuses, etc.).
        /// </summary>
        public void AddXP(float amount)
        {
            AccumulatedXP += amount;
            TotalXPEarned += amount;
        }

        /// <summary>
        /// Check if there's enough XP to unlock a node.
        /// </summary>
        public bool CanAffordNode(TechNode node)
        {
            return AccumulatedXP >= node.Cost;
        }

        /// <summary>
        /// Spend XP to unlock a node.
        /// Returns true if successful.
        /// </summary>
        public bool UnlockNode(TechNode node)
        {
            if (!CanAffordNode(node))
            {
                Debug.LogWarning($"[CultureTechTree] Not enough XP to unlock {node.NodeName}");
                return false;
            }

            if (!node.ArePrerequisitesMet())
            {
                Debug.LogWarning($"[CultureTechTree] Prerequisites not met for {node.NodeName}");
                return false;
            }

            if (IsNodeUnlocked(node))
            {
                Debug.LogWarning($"[CultureTechTree] Node {node.NodeName} already unlocked");
                return false;
            }

            // Spend XP
            AccumulatedXP -= node.Cost;

            // Mark as unlocked
            node.IsUnlocked = true;
            UnlockedNodeIDs.Add(node.GetNodeID());

            Debug.Log($"[CultureTechTree] Unlocked {node.NodeName} in {TreeType} tree");
            return true;
        }

        /// <summary>
        /// Check if a node is unlocked.
        /// </summary>
        public bool IsNodeUnlocked(TechNode node)
        {
            return UnlockedNodeIDs.Contains(node.GetNodeID());
        }

        /// <summary>
        /// Get all nodes that are currently available to unlock.
        /// </summary>
        public List<TechNode> GetAvailableNodes()
        {
            return AllNodes
                .Where(node => !IsNodeUnlocked(node) && node.ArePrerequisitesMet())
                .ToList();
        }

        /// <summary>
        /// Get all unlocked nodes.
        /// </summary>
        public List<TechNode> GetUnlockedNodes()
        {
            return AllNodes
                .Where(node => IsNodeUnlocked(node))
                .ToList();
        }

        /// <summary>
        /// Get all modifiers from unlocked nodes.
        /// </summary>
        public List<CultureModifier> GetActiveModifiers()
        {
            var modifiers = new List<CultureModifier>();
            foreach (var node in GetUnlockedNodes())
            {
                modifiers.AddRange(node.Modifiers);
            }
            return modifiers;
        }

        /// <summary>
        /// Get all building definitions unlocked by this tree.
        /// </summary>
        public HashSet<BuildingDefinition> GetUnlockedBuildings()
        {
            var buildings = new HashSet<BuildingDefinition>();
            foreach (var node in GetUnlockedNodes())
            {
                foreach (var building in node.UnlockedBuildings)
                {
                    if (building != null)
                        buildings.Add(building);
                }
            }
            return buildings;
        }
    }
}
