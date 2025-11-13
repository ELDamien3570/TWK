using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;
using TWK.Realms.Demographics;

namespace TWK.Economy
{
    /// <summary>
    /// ScriptableObject definition for a building type (template/configuration).
    /// This defines what a building CAN do, not the state of a specific instance.
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Economy/Building Definition", fileName = "New Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string BuildingName;
        public TreeType BuildingCategory;
        public Sprite Icon;

        [Header("Hub System")]
        [Tooltip("Is this a hub? Hubs allow hublets to be built around them")]
        public bool IsHub;

        [Tooltip("Is this a hublet? Hublets must be adjacent to a hub")]
        public bool IsHublet;

        [Tooltip("If hublet, which hub types can it attach to? Empty = any hub")]
        public List<TreeType> RequiredHubTypes = new List<TreeType>();

        [Header("Costs")]
        public Dictionary<ResourceType, int> BaseBuildCost = new Dictionary<ResourceType, int>();
        public Dictionary<ResourceType, int> BaseMaintenanceCost = new Dictionary<ResourceType, int>();

        [Header("Production")]
        public Dictionary<ResourceType, int> BaseProduction = new Dictionary<ResourceType, int>();

        [Tooltip("Max production when fully staffed")]
        public Dictionary<ResourceType, int> MaxProduction = new Dictionary<ResourceType, int>();

        [Header("Worker Requirements")]
        [Tooltip("Does this building require workers to operate?")]
        public bool RequiresWorkers = true;

        [Tooltip("Minimum workers needed to operate at all")]
        public int MinWorkers = 0;

        [Tooltip("Optimal workers for max production")]
        public int OptimalWorkers = 10;

        [Tooltip("Which population archetypes can work here? Empty = any")]
        public List<PopulationArchetypes> AllowedWorkerTypes = new List<PopulationArchetypes>();

        [Tooltip("Required worker types (must have at least one)")]
        public Dictionary<PopulationArchetypes, int> RequiredWorkers_ByType = new Dictionary<PopulationArchetypes, int>();

        [Tooltip("Worker efficiency multipliers by archetype (1.0 = normal)")]
        public Dictionary<PopulationArchetypes, float> WorkerEfficiency = new Dictionary<PopulationArchetypes, float>();

        [Tooltip("Penalties for certain worker types (negative multiplier)")]
        public Dictionary<PopulationArchetypes, float> WorkerPenalties = new Dictionary<PopulationArchetypes, float>();

        [Header("Population Effects")]
        [Tooltip("Education increase per day per worker")]
        public float EducationGrowthPerWorker = 0f;

        [Tooltip("Wealth increase per day per worker")]
        public float WealthGrowthPerWorker = 0f;

        [Tooltip("Affects specific population archetype only? Null = affects all workers")]
        public PopulationArchetypes? AffectsSpecificArchetype = null;

        [Tooltip("Population growth bonus (for eco hubs like markets)")]
        public float PopulationGrowthBonus = 0f;

        [Header("Modifiers")]
        public float BaseEfficiency = 1f;

        [Header("Construction")]
        public int ConstructionTimeDays = 1;

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Can this archetype work in this building?
        /// </summary>
        public bool CanWorkerTypeWork(PopulationArchetypes archetype)
        {
            // If AllowedWorkerTypes is empty, all types can work
            if (AllowedWorkerTypes.Count == 0)
                return true;

            return AllowedWorkerTypes.Contains(archetype);
        }

        /// <summary>
        /// Get the efficiency multiplier for this worker type.
        /// Accounts for both efficiency bonuses and penalties.
        /// </summary>
        public float GetWorkerEfficiency(PopulationArchetypes archetype)
        {
            float efficiency = 1f;

            // Apply efficiency multiplier
            if (WorkerEfficiency.ContainsKey(archetype))
                efficiency *= WorkerEfficiency[archetype];

            // Apply penalties (negative effects)
            if (WorkerPenalties.ContainsKey(archetype))
                efficiency *= (1f - WorkerPenalties[archetype]);

            return efficiency;
        }

        /// <summary>
        /// Calculate total production based on worker count and efficiency.
        /// </summary>
        public Dictionary<ResourceType, int> CalculateProduction(int workerCount, float averageWorkerEfficiency)
        {
            var production = new Dictionary<ResourceType, int>();

            if (!RequiresWorkers)
            {
                // Buildings that don't require workers produce at base rate
                foreach (var kvp in BaseProduction)
                    production[kvp.Key] = kvp.Value;
                return production;
            }

            if (workerCount < MinWorkers)
            {
                // Not enough workers = no production
                return production;
            }

            // Calculate production scaling
            float workerRatio = OptimalWorkers > 0 ? (workerCount / (float)OptimalWorkers) : 1f;
            workerRatio = Mathf.Clamp01(workerRatio); // Cap at 100%

            foreach (var kvp in MaxProduction)
            {
                int maxProd = kvp.Value;
                int baseProd = BaseProduction.GetValueOrDefault(kvp.Key, 0);

                // Scale between base and max based on workers
                int scaledProd = Mathf.RoundToInt(
                    Mathf.Lerp(baseProd, maxProd, workerRatio) * averageWorkerEfficiency * BaseEfficiency
                );

                production[kvp.Key] = scaledProd;
            }

            // Add any base production that isn't in max production
            foreach (var kvp in BaseProduction)
            {
                if (!production.ContainsKey(kvp.Key))
                    production[kvp.Key] = Mathf.RoundToInt(kvp.Value * averageWorkerEfficiency * BaseEfficiency);
            }

            return production;
        }

        /// <summary>
        /// Calculate maintenance costs (can scale with workers if desired).
        /// </summary>
        public Dictionary<ResourceType, int> CalculateMaintenanceCost(int workerCount)
        {
            var costs = new Dictionary<ResourceType, int>();

            foreach (var kvp in BaseMaintenanceCost)
            {
                // For now, maintenance is flat. Could scale with workers if desired.
                costs[kvp.Key] = kvp.Value;
            }

            return costs;
        }
    }
}
