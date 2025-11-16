using System.Collections.Generic;
using UnityEngine;
using TWK.Realms.Demographics;
using TWK.Modifiers;
using TWK.Cultures;
using TWK.Simulation;

namespace TWK.Economy
{
    /// <summary>
    /// Pure static functions for building simulation logic.
    /// Operates on BuildingInstanceData and BuildingDefinition without side effects.
    /// This is the "Logic" layer - pure functions that transform data.
    /// </summary>
    public static class BuildingSimulation
    {
        // ========== SIMULATION ==========

        /// <summary>
        /// Simulate one day for a building (production, maintenance, population effects).
        /// </summary>
        public static void SimulateDay(BuildingInstanceData instance, BuildingDefinition definition, ResourceLedger ledger, int cityId, List<Modifier> modifiers = null)
        {
            // Only simulate if completed and active
            if (!instance.IsActive || instance.ConstructionState != BuildingConstructionState.Completed)
                return;

            // Production (with modifiers)
            var production = CalculateProduction(instance, definition, modifiers);
            foreach (var kvp in production)
                ledger.Add(kvp.Key, kvp.Value);

            // Maintenance costs
            var maintenance = definition.CalculateMaintenanceCost(instance.TotalWorkers);
            foreach (var kvp in maintenance)
                ledger.Subtract(kvp.Key, kvp.Value);

            // Population effects (education, wealth)
            ApplyPopulationEffects(instance, definition, cityId);
        }

        /// <summary>
        /// Calculate production based on workers, efficiency, and modifiers.
        /// </summary>
        public static Dictionary<ResourceType, int> CalculateProduction(BuildingInstanceData instance, BuildingDefinition definition, List<Modifier> modifiers = null)
        {
            // Get base production from definition
            Dictionary<ResourceType, int> production;

            if (!definition.RequiresWorkers)
            {
                // Buildings that don't require workers produce at base rate
                production = definition.BaseProduction.ToDictionary();
            }
            else
            {
                // Calculate average worker efficiency
                float totalEfficiency = 0f;
                int totalWorkers = 0;

                foreach (var kvp in instance.AssignedWorkers)
                {
                    PopulationArchetypes archetype = kvp.Key;
                    int workerCount = kvp.Value;

                    float archetypeEfficiency = definition.GetWorkerEfficiency(archetype);
                    totalEfficiency += archetypeEfficiency * workerCount;
                    totalWorkers += workerCount;
                }

                float averageEfficiency = totalWorkers > 0 ? totalEfficiency / totalWorkers : 0f;

                // Use definition's calculation method
                production = definition.CalculateProduction(instance.TotalWorkers, averageEfficiency);
            }

            // Apply seasonal production modifier
            production = ApplySeasonalModifier(production, definition);

            // Apply culture/religion/event modifiers to production
            if (modifiers != null && modifiers.Count > 0)
            {
                production = ApplyProductionModifiers(production, definition, modifiers);
            }

            return production;
        }

        /// <summary>
        /// Apply seasonal production modifier to building production.
        /// </summary>
        private static Dictionary<ResourceType, int> ApplySeasonalModifier(
            Dictionary<ResourceType, int> baseProduction,
            BuildingDefinition definition)
        {
            // Get current season from TimeSystem
            if (WorldTimeManager.Instance?.timeSystem == null)
                return baseProduction; // No time system available, return unmodified

            int currentSeason = WorldTimeManager.Instance.timeSystem.CurrentSeasonIndex;
            float seasonalMultiplier = definition.GetSeasonalProductionModifier(currentSeason);

            // If seasonal multiplier is 1.0, no need to modify
            if (Mathf.Approximately(seasonalMultiplier, 1f))
                return baseProduction;

            // Apply seasonal multiplier to all production
            var modifiedProduction = new Dictionary<ResourceType, int>();
            foreach (var kvp in baseProduction)
            {
                int modifiedValue = Mathf.RoundToInt(kvp.Value * seasonalMultiplier);
                modifiedProduction[kvp.Key] = modifiedValue;
            }

            return modifiedProduction;
        }

        /// <summary>
        /// Apply production modifiers from culture/religion/events to building production.
        /// </summary>
        private static Dictionary<ResourceType, int> ApplyProductionModifiers(
            Dictionary<ResourceType, int> baseProduction,
            BuildingDefinition definition,
            List<Modifier> modifiers)
        {
            var modifiedProduction = new Dictionary<ResourceType, int>(baseProduction);

            // For each resource type in production
            foreach (var kvp in baseProduction)
            {
                ResourceType resourceType = kvp.Key;
                int baseValue = kvp.Value;

                // Use ModifierUtilities to calculate modified value
                int modifiedValue = ModifierUtilities.CalculateModifiedProduction(
                    baseValue,
                    resourceType,
                    definition,
                    modifiers
                );

                modifiedProduction[resourceType] = modifiedValue;
            }

            return modifiedProduction;
        }

        /// <summary>
        /// Apply population effects (education, wealth increases).
        /// Effects are now defined per worker slot.
        /// </summary>
        private static void ApplyPopulationEffects(BuildingInstanceData instance, BuildingDefinition definition, int cityId)
        {
            if (instance.TotalWorkers == 0)
                return;

            // Get all population groups for this city
            var populationGroups = PopulationManager.Instance.GetPopulationsByCity(cityId);

            // Process each archetype with assigned workers
            foreach (var kvp in instance.AssignedWorkers)
            {
                PopulationArchetypes archetype = kvp.Key;
                int workerCount = kvp.Value;

                if (workerCount == 0)
                    continue;

                // Get the worker slot for this archetype
                var slot = definition.WorkerSlots.GetSlot(archetype);
                if (slot == null)
                    continue;

                // Find the population group for this archetype
                PopulationGroup popGroup = null;
                foreach (var group in populationGroups)
                {
                    if (group.Archetype == archetype)
                    {
                        popGroup = group;
                        break;
                    }
                }

                if (popGroup == null)
                    continue;

                // Apply education growth from this slot
                if (slot.EducationGrowthPerWorker > 0)
                {
                    float educationGain = slot.EducationGrowthPerWorker * workerCount;
                    popGroup.Education = Mathf.Clamp(popGroup.Education + educationGain, 0f, 100f);
                }

                // Apply wealth growth from this slot
                if (slot.WealthGrowthPerWorker > 0)
                {
                    float wealthGain = slot.WealthGrowthPerWorker * workerCount;
                    popGroup.Wealth = Mathf.Clamp(popGroup.Wealth + wealthGain, 0f, 100f);
                }
            }
        }

        // ========== CONSTRUCTION ==========

        /// <summary>
        /// Advance construction by one day.
        /// Returns true if construction was advanced or completed.
        /// </summary>
        public static bool AdvanceConstruction(BuildingInstanceData instance)
        {
            if (instance.ConstructionState != BuildingConstructionState.UnderConstruction)
                return false;

            instance.ConstructionDaysRemaining--;

            if (instance.ConstructionDaysRemaining <= 0)
            {
                CompleteConstruction(instance);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Complete construction.
        /// </summary>
        private static void CompleteConstruction(BuildingInstanceData instance)
        {
            instance.ConstructionState = BuildingConstructionState.Completed;
            instance.ConstructionDaysRemaining = 0;
            instance.IsActive = true;

            Debug.Log($"[BuildingSimulation] Building ID {instance.ID} construction completed!");
        }

        // ========== WORKER MANAGEMENT ==========

        /// <summary>
        /// Check if a building can accept more workers.
        /// </summary>
        public static bool CanAcceptWorkers(BuildingInstanceData instance, BuildingDefinition definition, int additionalWorkers)
        {
            if (!definition.RequiresWorkers)
                return false;

            int optimalWorkers = definition.WorkerSlots.GetTotalMaxWorkers();
            return (instance.TotalWorkers + additionalWorkers) <= optimalWorkers;
        }

        /// <summary>
        /// Get the production efficiency percentage (0-1).
        /// </summary>
        public static float GetProductionEfficiency(BuildingInstanceData instance, BuildingDefinition definition)
        {
            if (!definition.RequiresWorkers)
                return 1f;

            int minWorkers = definition.WorkerSlots.GetTotalMinWorkers();
            int optimalWorkers = definition.WorkerSlots.GetTotalMaxWorkers();

            if (instance.TotalWorkers < minWorkers)
                return 0f;

            if (optimalWorkers == 0)
                return 1f;

            return Mathf.Clamp01(instance.TotalWorkers / (float)optimalWorkers);
        }

        /// <summary>
        /// Calculate required workers that aren't met.
        /// Returns dictionary of archetype -> deficit count.
        /// </summary>
        public static Dictionary<PopulationArchetypes, int> GetWorkerDeficit(BuildingInstanceData instance, BuildingDefinition definition)
        {
            var deficit = new Dictionary<PopulationArchetypes, int>();

            foreach (var slot in definition.WorkerSlots.GetRequiredSlots())
            {
                PopulationArchetypes archetype = slot.Archetype;
                int required = slot.MinCount;
                int assigned = instance.GetWorkerCount(archetype);

                if (assigned < required)
                    deficit[archetype] = required - assigned;
            }

            return deficit;
        }

        // ========== HUB/HUBLET VALIDATION ==========

        /// <summary>
        /// Check if a hublet can be placed adjacent to a hub.
        /// </summary>
        public static bool CanHubletAttachToHub(BuildingDefinition hubletDef, BuildingDefinition hubDef)
        {
            if (!hubletDef.IsHublet || !hubDef.IsHub)
                return false;

            // If hublet has no required hub types, it can attach to any hub
            if (hubletDef.RequiredHubTypes.Count == 0)
                return true;

            // Check if hub's category matches hublet's requirements
            return hubletDef.RequiredHubTypes.Contains(hubDef.BuildingCategory);
        }

        /// <summary>
        /// Check if a position is adjacent to another building.
        /// </summary>
        public static bool IsAdjacentTo(Vector3 position1, Vector3 position2, float maxDistance = 5f)
        {
            return Vector3.Distance(position1, position2) <= maxDistance;
        }

        /// <summary>
        /// Find the nearest hub to a position from a list of building instances.
        /// </summary>
        public static BuildingInstanceData FindNearestHub(Vector3 position, List<BuildingInstanceData> buildings, Dictionary<int, BuildingDefinition> definitions)
        {
            BuildingInstanceData nearestHub = null;
            float nearestDistance = float.MaxValue;

            foreach (var building in buildings)
            {
                if (!definitions.ContainsKey(building.BuildingDefinitionID))
                    continue;

                var definition = definitions[building.BuildingDefinitionID];
                if (!definition.IsHub)
                    continue;

                float distance = Vector3.Distance(position, building.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestHub = building;
                }
            }

            return nearestHub;
        }

        // ========== POPULATION GROWTH EFFECTS ==========

        /// <summary>
        /// Calculate population growth bonus from buildings (for eco hubs).
        /// </summary>
        public static float CalculatePopulationGrowthBonus(List<BuildingInstanceData> cityBuildings, Dictionary<int, BuildingDefinition> definitions)
        {
            float totalBonus = 0f;

            foreach (var building in cityBuildings)
            {
                if (!building.IsActive || building.ConstructionState != BuildingConstructionState.Completed)
                    continue;

                if (!definitions.ContainsKey(building.BuildingDefinitionID))
                    continue;

                var definition = definitions[building.BuildingDefinitionID];
                totalBonus += definition.PopulationGrowthBonus;
            }

            return totalBonus;
        }
    }
}
