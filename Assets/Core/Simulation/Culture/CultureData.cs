using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Cultures;
using TWK.Economy;

namespace TWK.Cultures
{
    /// <summary>
    /// Represents a culture with its own tech trees, pillars, and identity.
    /// Cultures can be original or hybrid (created from two parent cultures).
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Culture/Culture Data", fileName = "New Culture")]
    public class CultureData : ScriptableObject
    {
        // ========== IDENTITY ==========
        [Header("Identity")]
        public string CultureName;
        [TextArea(3, 6)]
        public string Description;
        public Color CultureColor = Color.white;
        public Sprite Icon;

        // ========== CULTURAL PILLARS ==========
        [Header("Cultural Pillars")]
        [Tooltip("Defining characteristics of this culture (more powerful than tech nodes)")]
        public List<CulturePillar> Pillars = new List<CulturePillar>();

        // ========== TECH TREES ==========
        [Header("Tech Trees")]
        [Tooltip("One tech tree per TreeType")]
        public List<CultureTechTree> TechTrees = new List<CultureTechTree>();

        // ========== DEFAULT UNLOCKS ==========
        [Header("Starting Unlocks")]
        [Tooltip("Building definitions unlocked by default (starting buildings)")]
        public List<BuildingDefinition> DefaultBuildingUnlocks = new List<BuildingDefinition>();

        // ========== HYBRID CULTURE DATA ==========
        [Header("Hybrid Culture (Optional)")]
        [Tooltip("Is this a hybrid culture created from two parents?")]
        public bool IsHybrid = false;

        [Tooltip("Parent cultures (for player inspection only)")]
        public List<CultureData> ParentCultures = new List<CultureData>();

        // ========== RUNTIME DATA ==========
        [System.NonSerialized]
        private Dictionary<TreeType, CultureTechTree> _techTreeLookup;

        // ========== INITIALIZATION ==========

        private void OnEnable()
        {
            InitializeTechTreeLookup();
        }

        private void InitializeTechTreeLookup()
        {
            _techTreeLookup = new Dictionary<TreeType, CultureTechTree>();
            foreach (var tree in TechTrees)
            {
                _techTreeLookup[tree.TreeType] = tree;
            }
        }

        /// <summary>
        /// Initialize tech trees for all TreeTypes if they don't exist.
        /// </summary>
        public void InitializeTechTrees()
        {
            if (TechTrees == null)
                TechTrees = new List<CultureTechTree>();

            // Create a tech tree for each TreeType
            foreach (TreeType treeType in System.Enum.GetValues(typeof(TreeType)))
            {
                if (!TechTrees.Any(t => t.TreeType == treeType))
                {
                    TechTrees.Add(new CultureTechTree(treeType));
                }
            }

            InitializeTechTreeLookup();

            // Sync node unlock states from persisted data
            // This restores the IsUnlocked flags after game reload
            foreach (var tree in TechTrees)
            {
                tree.SyncNodeUnlockStates();
            }
        }

        // ========== TECH TREE ACCESS ==========

        /// <summary>
        /// Get tech tree for a specific TreeType.
        /// </summary>
        public CultureTechTree GetTechTree(TreeType treeType)
        {
            if (_techTreeLookup == null)
                InitializeTechTreeLookup();

            return _techTreeLookup.GetValueOrDefault(treeType, null);
        }

        /// <summary>
        /// Get all unlocked building definitions from all sources.
        /// </summary>
        public HashSet<BuildingDefinition> GetAllUnlockedBuildings()
        {
            var buildings = new HashSet<BuildingDefinition>();

            // Add default unlocks
            foreach (var building in DefaultBuildingUnlocks)
            {
                if (building != null)
                    buildings.Add(building);
            }

            // Add buildings from cultural pillars
            foreach (var pillar in Pillars)
            {
                if (pillar == null) continue;
                foreach (var building in pillar.UnlockedBuildings)
                {
                    if (building != null)
                        buildings.Add(building);
                }
            }

            // Add buildings from tech trees
            foreach (var tree in TechTrees)
            {
                if (tree == null) continue;
                foreach (var building in tree.GetUnlockedBuildings())
                {
                    if (building != null)
                        buildings.Add(building);
                }
            }

            return buildings;
        }

        /// <summary>
        /// Get all active modifiers from pillars and unlocked tech nodes.
        /// </summary>
        public List<CultureModifier> GetAllModifiers()
        {
            var modifiers = new List<CultureModifier>();

            // Add modifiers from cultural pillars
            foreach (var pillar in Pillars)
            {
                modifiers.AddRange(pillar.Modifiers);
            }

            // Add modifiers from tech trees
            foreach (var tree in TechTrees)
            {
                modifiers.AddRange(tree.GetActiveModifiers());
            }

            return modifiers;
        }

        /// <summary>
        /// Check if a building is unlocked by this culture.
        /// </summary>
        public bool IsBuildingUnlocked(BuildingDefinition building)
        {
            if (building == null)
                return false;

            return GetAllUnlockedBuildings().Contains(building);
        }

        // ========== HYBRID CULTURE CREATION ==========

        /// <summary>
        /// Create a hybrid culture from two parent cultures.
        /// Inherits selected tech trees wholesale from each parent.
        /// </summary>
        public static CultureData CreateHybrid(
            string name,
            string description,
            CultureData parent1,
            CultureData parent2,
            List<TreeType> treesFromParent1,
            List<TreeType> treesFromParent2)
        {
            var hybrid = CreateInstance<CultureData>();
            hybrid.CultureName = name;
            hybrid.Description = description;
            hybrid.IsHybrid = true;
            hybrid.ParentCultures = new List<CultureData> { parent1, parent2 };

            // Initialize tech trees
            hybrid.TechTrees = new List<CultureTechTree>();

            // Inherit trees from parent 1
            foreach (var treeType in treesFromParent1)
            {
                var parentTree = parent1.GetTechTree(treeType);
                if (parentTree != null)
                {
                    hybrid.TechTrees.Add(CloneTechTree(parentTree));
                }
            }

            // Inherit trees from parent 2
            foreach (var treeType in treesFromParent2)
            {
                var parentTree = parent2.GetTechTree(treeType);
                if (parentTree != null)
                {
                    hybrid.TechTrees.Add(CloneTechTree(parentTree));
                }
            }

            // Blend colors
            hybrid.CultureColor = Color.Lerp(parent1.CultureColor, parent2.CultureColor, 0.5f);

            hybrid.InitializeTechTreeLookup();
            return hybrid;
        }

        private static CultureTechTree CloneTechTree(CultureTechTree source)
        {
            var clone = new CultureTechTree(source.TreeType);
            clone.AccumulatedXP = source.AccumulatedXP;
            clone.TotalXPEarned = source.TotalXPEarned;
            clone.AllNodes = new List<TechNode>(source.AllNodes);
            clone.UnlockedNodeIDs = new List<int>(source.UnlockedNodeIDs);
            clone.OwnerRealmID = source.OwnerRealmID;
            return clone;
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get unique ID for this culture (uses Unity's instance ID).
        /// </summary>
        public int GetCultureID()
        {
            return GetInstanceID();
        }
    }
}
