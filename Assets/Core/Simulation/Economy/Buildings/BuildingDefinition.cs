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

        [Tooltip("Number of hublet slots this hub provides. NOTE: This is a placeholder - will be replaced with world-based spatial validation")]
        public int HubletSlots = 0;

        [Tooltip("Is this a hublet? Hublets must be adjacent to a hub")]
        public bool IsHublet;

        [Tooltip("If hublet, which hub types can it attach to? Empty = any hub")]
        public List<TreeType> RequiredHubTypes = new List<TreeType>();

        [Header("Costs")]
        public List<ResourceAmount> BaseBuildCost = new List<ResourceAmount>();
        public List<ResourceAmount> BaseMaintenanceCost = new List<ResourceAmount>();

        [Header("Production")]
        public List<ResourceAmount> BaseProduction = new List<ResourceAmount>();

        [Tooltip("Max production when fully staffed")]
        public List<ResourceAmount> MaxProduction = new List<ResourceAmount>();

        [Header("Culture Tech XP")]
        [Tooltip("Base culture tech XP generated per month (varies with worker count like production)")]
        public float BaseMonthlyXP = 0f;

        [Tooltip("Max XP generated per month when fully staffed")]
        public float MaxMonthlyXP = 0f;

        [Header("Worker Requirements")]
        [Tooltip("Does this building require workers to operate?")]
        public bool RequiresWorkers = true;

        [Tooltip("Worker slot definitions - each slot defines an archetype with min/max counts, efficiency, and population effects")]
        public List<WorkerSlot> WorkerSlots = new List<WorkerSlot>();

        [Header("Population Effects")]
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
            // If WorkerSlots is empty, all types can work
            if (WorkerSlots.Count == 0)
                return true;

            return WorkerSlots.IsArchetypeAllowed(archetype);
        }

        /// <summary>
        /// Get the efficiency multiplier for this worker type.
        /// </summary>
        public float GetWorkerEfficiency(PopulationArchetypes archetype)
        {
            return WorkerSlots.GetEfficiency(archetype, 1f);
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
                foreach (var item in BaseProduction)
                    production[item.ResourceType] = item.Amount;
                return production;
            }

            int minWorkers = WorkerSlots.GetTotalMinWorkers();
            int optimalWorkers = WorkerSlots.GetTotalMaxWorkers();

            if (workerCount < minWorkers)
            {
                // Not enough workers = no production
                return production;
            }

            // Calculate production scaling
            float workerRatio = optimalWorkers > 0 ? (workerCount / (float)optimalWorkers) : 1f;
            workerRatio = Mathf.Clamp01(workerRatio); // Cap at 100%

            foreach (var item in MaxProduction)
            {
                int maxProd = item.Amount;
                int baseProd = BaseProduction.GetAmount(item.ResourceType);

                // Scale between base and max based on workers
                int scaledProd = Mathf.RoundToInt(
                    Mathf.Lerp(baseProd, maxProd, workerRatio) * averageWorkerEfficiency * BaseEfficiency
                );

                production[item.ResourceType] = scaledProd;
            }

            // Add any base production that isn't in max production
            foreach (var item in BaseProduction)
            {
                if (!production.ContainsKey(item.ResourceType))
                    production[item.ResourceType] = Mathf.RoundToInt(item.Amount * averageWorkerEfficiency * BaseEfficiency);
            }

            return production;
        }

        /// <summary>
        /// Calculate maintenance costs (can scale with workers if desired).
        /// </summary>
        public Dictionary<ResourceType, int> CalculateMaintenanceCost(int workerCount)
        {
            var costs = new Dictionary<ResourceType, int>();

            foreach (var item in BaseMaintenanceCost)
            {
                // For now, maintenance is flat. Could scale with workers if desired.
                costs[item.ResourceType] = item.Amount;
            }

            return costs;
        }

        /// <summary>
        /// Calculate monthly culture tech XP based on worker count and efficiency.
        /// XP is generated for the building's TreeType (category).
        /// </summary>
        public float CalculateMonthlyXP(int workerCount, float averageWorkerEfficiency)
        {
            if (BaseMonthlyXP <= 0f)
                return 0f;

            if (!RequiresWorkers)
            {
                // Buildings that don't require workers produce at base rate
                return BaseMonthlyXP;
            }

            int minWorkers = WorkerSlots.GetTotalMinWorkers();
            int optimalWorkers = WorkerSlots.GetTotalMaxWorkers();

            if (workerCount < minWorkers)
            {
                // Not enough workers = no XP
                return 0f;
            }

            // Calculate XP scaling (same logic as production)
            float workerRatio = optimalWorkers > 0 ? (workerCount / (float)optimalWorkers) : 1f;
            workerRatio = Mathf.Clamp01(workerRatio); // Cap at 100%

            // Scale between base and max based on workers
            float xp = Mathf.Lerp(BaseMonthlyXP, MaxMonthlyXP, workerRatio) * averageWorkerEfficiency * BaseEfficiency;

            return xp;
        }
    }
}

