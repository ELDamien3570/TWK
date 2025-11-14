using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Cultures;

namespace TWK.UI
{
    /// <summary>
    /// Layout information for a single tech node in the tree.
    /// </summary>
    public class TechNodeLayoutInfo
    {
        public TechNode Node;
        public int Tier;
        public Vector2 Position;
        public GameObject UIObject;
        public RectTransform RectTransform;
    }

    /// <summary>
    /// Calculates tiered layout positions for tech tree nodes.
    /// Uses BFS traversal to assign tiers and positions nodes in columns.
    /// </summary>
    public class TechTreeLayoutCalculator
    {
        // ========== LAYOUT SETTINGS ==========
        // Adjust these values to change the tree appearance and spacing

        // Node dimensions (should match your prefab size)
        private const float NODE_WIDTH = 200f;
        private const float NODE_HEIGHT = 100f;

        // Spacing between elements
        private const float HORIZONTAL_SPACING = 150f;  // Space between tier columns
        private const float VERTICAL_SPACING = 50f;      // Space between nodes in same tier

        // Scroll view content size settings
        // These are initial estimates - adjust based on your expected tree size during testing
        private const int EXPECTED_MAX_TIER = 5;        // Estimated maximum tier depth
        private const int EXPECTED_NODES_PER_TIER = 10; // Estimated max nodes per tier

        // ========== TIER CALCULATION ==========

        /// <summary>
        /// Calculate tier assignments for all nodes using BFS traversal.
        /// Root nodes (no prerequisites) start at tier 0.
        /// Each subsequent tier contains nodes whose prerequisites are all in earlier tiers.
        /// </summary>
        public static Dictionary<TechNode, int> CalculateTiers(List<TechNode> allNodes)
        {
            var tierAssignments = new Dictionary<TechNode, int>();
            var queue = new Queue<TechNode>();

            // Find root nodes (nodes with no prerequisites)
            var rootNodes = allNodes.Where(n => n.Prerequisites == null || n.Prerequisites.Count == 0).ToList();

            // Assign tier 0 to all root nodes
            foreach (var root in rootNodes)
            {
                tierAssignments[root] = 0;
                queue.Enqueue(root);
            }

            // BFS to assign tiers to remaining nodes
            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                int currentTier = tierAssignments[currentNode];

                // Find all nodes that list currentNode as a prerequisite
                var dependentNodes = allNodes.Where(n =>
                    n.Prerequisites != null &&
                    n.Prerequisites.Contains(currentNode)
                ).ToList();

                foreach (var dependent in dependentNodes)
                {
                    // Skip if already assigned a tier
                    if (tierAssignments.ContainsKey(dependent))
                        continue;

                    // Check if ALL prerequisites have been assigned tiers
                    bool allPrereqsAssigned = dependent.Prerequisites.All(prereq =>
                        tierAssignments.ContainsKey(prereq)
                    );

                    if (allPrereqsAssigned)
                    {
                        // Tier = max(prerequisite tiers) + 1
                        int maxPrereqTier = dependent.Prerequisites.Max(prereq =>
                            tierAssignments[prereq]
                        );

                        tierAssignments[dependent] = maxPrereqTier + 1;
                        queue.Enqueue(dependent);
                    }
                }
            }

            // Handle orphaned nodes (nodes with unmet prerequisites or circular dependencies)
            foreach (var node in allNodes)
            {
                if (!tierAssignments.ContainsKey(node))
                {
                    Debug.LogWarning($"[TechTreeLayout] Node {node.NodeName} has no tier assignment (orphaned or circular dependency). Assigning to tier 0.");
                    tierAssignments[node] = 0;
                }
            }

            return tierAssignments;
        }

        // ========== POSITION CALCULATION ==========

        /// <summary>
        /// Calculate X,Y positions for all nodes based on their tier assignments.
        /// Nodes are arranged in vertical columns (tiers) from left to right.
        /// </summary>
        public static Dictionary<TechNode, Vector2> CalculatePositions(
            List<TechNode> allNodes,
            Dictionary<TechNode, int> tierAssignments)
        {
            var positions = new Dictionary<TechNode, Vector2>();

            // Group nodes by tier
            var nodesByTier = allNodes
                .GroupBy(n => tierAssignments[n])
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Calculate positions for each tier
            foreach (var tierGroup in nodesByTier)
            {
                int tier = tierGroup.Key;
                var nodesInTier = tierGroup.Value;

                // Calculate X position (horizontal - based on tier)
                float xPos = tier * (NODE_WIDTH + HORIZONTAL_SPACING);

                // Calculate Y positions (vertical - evenly spaced within tier)
                for (int i = 0; i < nodesInTier.Count; i++)
                {
                    float yPos = i * (NODE_HEIGHT + VERTICAL_SPACING);
                    positions[nodesInTier[i]] = new Vector2(xPos, -yPos); // Negative Y for top-down layout
                }
            }

            return positions;
        }

        // ========== FULL LAYOUT CALCULATION ==========

        /// <summary>
        /// Calculate complete layout information for all nodes.
        /// Returns a dictionary mapping each node to its layout info.
        /// </summary>
        public static Dictionary<TechNode, TechNodeLayoutInfo> CalculateLayout(List<TechNode> allNodes)
        {
            var tierAssignments = CalculateTiers(allNodes);
            var positions = CalculatePositions(allNodes, tierAssignments);

            var layoutInfo = new Dictionary<TechNode, TechNodeLayoutInfo>();

            foreach (var node in allNodes)
            {
                layoutInfo[node] = new TechNodeLayoutInfo
                {
                    Node = node,
                    Tier = tierAssignments[node],
                    Position = positions[node]
                };
            }

            return layoutInfo;
        }

        // ========== CONTENT SIZE CALCULATION ==========

        /// <summary>
        /// Calculate the required ScrollView content size based on the layout.
        /// This ensures the content area is large enough to hold all nodes.
        /// </summary>
        public static Vector2 CalculateContentSize(Dictionary<TechNode, TechNodeLayoutInfo> layout)
        {
            if (layout.Count == 0)
            {
                // Return default size based on expected values
                return new Vector2(
                    EXPECTED_MAX_TIER * (NODE_WIDTH + HORIZONTAL_SPACING) + NODE_WIDTH,
                    EXPECTED_NODES_PER_TIER * (NODE_HEIGHT + VERTICAL_SPACING) + NODE_HEIGHT
                );
            }

            // Find the maximum tier (rightmost column)
            int maxTier = layout.Values.Max(info => info.Tier);

            // Find the node count in the largest tier (tallest column)
            var nodesByTier = layout.Values.GroupBy(info => info.Tier);
            int maxNodesInTier = nodesByTier.Max(group => group.Count());

            // Calculate required width and height
            float width = (maxTier + 1) * (NODE_WIDTH + HORIZONTAL_SPACING) + HORIZONTAL_SPACING;
            float height = maxNodesInTier * (NODE_HEIGHT + VERTICAL_SPACING) + VERTICAL_SPACING;

            return new Vector2(width, height);
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get all nodes that should have connection lines drawn to a given node.
        /// Returns the node's prerequisites.
        /// </summary>
        public static List<TechNode> GetConnectionSources(TechNode node)
        {
            if (node.Prerequisites == null || node.Prerequisites.Count == 0)
                return new List<TechNode>();

            return new List<TechNode>(node.Prerequisites);
        }
    }
}
