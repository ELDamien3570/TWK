using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Cultures;
using TWK.Economy;
using TWK.Modifiers;

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

        // ========== INNOVATION COST SCALING ==========
        [Header("Innovation Cost Scaling (Per Tree)")]
        [Tooltip("Base XP cost for the first innovation in this tree")]
        public float BaseNodeCost = 1000f;

        [Tooltip("XP cost increase per node unlocked (linear scaling). Example: 500 means each node costs 500 more XP")]
        public float CostIncreasePerNode = 500f;

        // ========== CONSTRUCTOR ==========
        public CultureTechTree(TreeType treeType)
        {
            TreeType = treeType;
            AllNodes = new List<TechNode>();
            UnlockedNodeIDs = new List<int>();
        }

        // ========== INITIALIZATION ==========

        /// <summary>
        /// Synchronize the runtime IsUnlocked flags with the persisted UnlockedNodeIDs list.
        /// Must be called after deserialization to restore unlock state.
        /// </summary>
        public void SyncNodeUnlockStates()
        {
            // First, reset all nodes to locked
            foreach (var node in AllNodes)
            {
                if (node != null)
                    node.IsUnlocked = false;
            }

            // Then set unlocked state for nodes in UnlockedNodeIDs
            foreach (var node in AllNodes)
            {
                if (node != null && UnlockedNodeIDs.Contains(node.GetNodeID()))
                {
                    node.IsUnlocked = true;
                }
            }

           /// Debug.Log($"[CultureTechTree] Synced {UnlockedNodeIDs.Count} unlocked nodes in {TreeType} tree");
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
        /// Calculate the XP cost for the next innovation based on how many nodes are already unlocked.
        /// Cost scales per tree: unlocking Warfare nodes doesn't affect Economics costs.
        /// Formula: BaseNodeCost + (NodesUnlocked * CostIncreasePerNode)
        /// Example: BaseNodeCost=1000, CostIncreasePerNode=500
        ///   - 1st node = 1000 XP
        ///   - 2nd node = 1500 XP
        ///   - 10th node = 5500 XP
        ///   - 19th node = 10,000 XP
        /// </summary>
        public float CalculateNextNodeCost()
        {
            int nodesUnlocked = UnlockedNodeIDs.Count;
            return BaseNodeCost + (nodesUnlocked * CostIncreasePerNode);
        }

        /// <summary>
        /// Check if there's enough XP to unlock the next node.
        /// Uses dynamic cost calculation based on nodes already unlocked in this tree.
        /// </summary>
        public bool CanAffordNode(TechNode node)
        {
            float cost = CalculateNextNodeCost();
            return AccumulatedXP >= cost;
        }

        /// <summary>
        /// Get the XP cost for unlocking a specific node (same as next node cost).
        /// All nodes in a tree cost the same based on unlock count, regardless of which node.
        /// </summary>
        public float GetNodeCost(TechNode node)
        {
            return CalculateNextNodeCost();
        }

        /// <summary>
        /// Spend XP to unlock a node.
        /// Returns true if successful.
        /// </summary>
        public bool UnlockNode(TechNode node)
        {
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

            // Calculate dynamic cost based on nodes already unlocked
            float cost = CalculateNextNodeCost();

            if (AccumulatedXP < cost)
            {
                Debug.LogWarning($"[CultureTechTree] Not enough XP to unlock {node.NodeName}. Need {cost:F0} XP, have {AccumulatedXP:F0} XP");
                return false;
            }

            // Spend XP
            AccumulatedXP -= cost;

            // Mark as unlocked
            node.IsUnlocked = true;
            UnlockedNodeIDs.Add(node.GetNodeID());

            Debug.Log($"[CultureTechTree] Unlocked {node.NodeName} in {TreeType} tree for {cost:F0} XP. Next node will cost {CalculateNextNodeCost():F0} XP.");
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
        /// Get all modifiers from unlocked nodes (unified modifier system).
        /// </summary>
        public List<Modifier> GetActiveModifiers()
        {
            var modifiers = new List<Modifier>();
            foreach (var node in GetUnlockedNodes())
            {
                modifiers.AddRange(node.Modifiers);
            }
            return modifiers;
        }

        /// <summary>
        /// Get all legacy modifiers from unlocked nodes.
        /// For backwards compatibility during migration.
        /// </summary>
        public List<CultureModifier> GetActiveLegacyModifiers()
        {
            var modifiers = new List<CultureModifier>();
            foreach (var node in GetUnlockedNodes())
            {
                modifiers.AddRange(node.LegacyModifiers);
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
