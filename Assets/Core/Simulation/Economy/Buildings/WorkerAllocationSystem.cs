using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Realms.Demographics;

namespace TWK.Economy
{
    /// <summary>
    /// Pure static functions for automatic worker allocation across buildings.
    /// Uses first-come-first-serve based on building construction order (building ID).
    /// </summary>
    public static class WorkerAllocationSystem
    {
        // ========== AUTOMATIC ALLOCATION ==========

        /// <summary>
        /// Automatically allocate workers to all buildings in a city.
        /// Uses first-come-first-serve based on building construction order (building ID ascending).
        /// </summary>
        /// <param name="cityId">City to allocate workers for</param>
        /// <param name="buildings">List of building instances (will be sorted by ID)</param>
        /// <param name="definitions">Lookup for building definitions</param>
        public static void AllocateWorkersForCity(
            int cityId,
            List<BuildingInstanceData> buildings,
            Dictionary<int, BuildingDefinition> definitions)
        {
            // Get all population groups for this city
            var populationGroups = PopulationManager.Instance.GetPopulationsByCity(cityId);

            // Calculate available workers by archetype
            var availableWorkers = CalculateAvailableWorkers(populationGroups);

            // Clear all current assignments
            ClearAllWorkerAssignments(buildings);

            // Sort buildings by ID (construction order - first built = lowest ID)
            var sortedBuildings = buildings
                .Where(b => b.CityID == cityId && b.IsActive && b.IsCompleted)
                .OrderBy(b => b.ID)
                .ToList();

            // Phase 1: Assign required workers (buildings with RequiredWorkers_ByType)
            AllocateRequiredWorkers(sortedBuildings, definitions, availableWorkers);

            // Phase 2: Fill buildings to minimum workers
            AllocateToMinimumWorkers(sortedBuildings, definitions, availableWorkers);

            // Phase 3: Fill buildings to optimal workers (first-come-first-serve)
            AllocateToOptimalWorkers(sortedBuildings, definitions, availableWorkers);

            // Update employment counts in population groups
            UpdatePopulationEmployment(populationGroups, buildings);

            Debug.Log($"[WorkerAllocationSystem] Allocated workers for City {cityId}. " +
                      $"Remaining: {string.Join(", ", availableWorkers.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
        }

        /// <summary>
        /// Deallocate workers from buildings when population is lost.
        /// Uses reverse construction order (last built = first to lose workers).
        /// </summary>
        public static void DeallocateWorkersForCity(
            int cityId,
            List<BuildingInstanceData> buildings,
            Dictionary<int, BuildingDefinition> definitions,
            int workersLost)
        {
            // Sort buildings by ID descending (reverse construction order - last built first)
            var sortedBuildings = buildings
                .Where(b => b.CityID == cityId && b.TotalWorkers > 0)
                .OrderByDescending(b => b.ID)
                .ToList();

            int remainingToRemove = workersLost;

            foreach (var building in sortedBuildings)
            {
                if (remainingToRemove <= 0)
                    break;

                var definition = definitions[building.BuildingDefinitionID];

                // Remove workers from this building (prioritize non-required workers)
                int removed = RemoveWorkersFromBuilding(building, definition, remainingToRemove);
                remainingToRemove -= removed;
            }

            // Update employment counts
            var populationGroups = PopulationManager.Instance.GetPopulationsByCity(cityId);
            UpdatePopulationEmployment(populationGroups, buildings);

            Debug.Log($"[WorkerAllocationSystem] Deallocated {workersLost - remainingToRemove} workers from City {cityId}");
        }

        // ========== PHASE 1: REQUIRED WORKERS ==========

        private static void AllocateRequiredWorkers(
            List<BuildingInstanceData> buildings,
            Dictionary<int, BuildingDefinition> definitions,
            Dictionary<PopulationArchetypes, int> availableWorkers)
        {
            foreach (var building in buildings)
            {
                if (!definitions.ContainsKey(building.BuildingDefinitionID))
                    continue;

                var definition = definitions[building.BuildingDefinitionID];

                var requiredSlots = definition.WorkerSlots.GetRequiredSlots();
                if (!definition.RequiresWorkers || requiredSlots.Count == 0)
                    continue;

                // Assign each required worker type
                foreach (var slot in requiredSlots)
                {
                    PopulationArchetypes archetype = slot.Archetype;
                    int required = slot.MinCount;

                    if (!availableWorkers.ContainsKey(archetype))
                        continue;

                    int toAssign = Mathf.Min(required, availableWorkers[archetype]);
                    if (toAssign > 0)
                    {
                        building.AssignWorker(archetype, toAssign);
                        availableWorkers[archetype] -= toAssign;
                    }
                }
            }
        }

        // ========== PHASE 2: MINIMUM WORKERS ==========

        private static void AllocateToMinimumWorkers(
            List<BuildingInstanceData> buildings,
            Dictionary<int, BuildingDefinition> definitions,
            Dictionary<PopulationArchetypes, int> availableWorkers)
        {
            foreach (var building in buildings)
            {
                if (!definitions.ContainsKey(building.BuildingDefinitionID))
                    continue;

                var definition = definitions[building.BuildingDefinitionID];

                if (!definition.RequiresWorkers)
                    continue;

                int minWorkers = definition.WorkerSlots.GetTotalMinWorkers();
                int needed = minWorkers - building.TotalWorkers;
                if (needed <= 0)
                    continue;

                // Try to assign from allowed worker types (any slot archetype)
                foreach (var slot in definition.WorkerSlots)
                {
                    if (needed <= 0)
                        break;

                    PopulationArchetypes archetype = slot.Archetype;

                    if (!availableWorkers.ContainsKey(archetype) || availableWorkers[archetype] <= 0)
                        continue;

                    int toAssign = Mathf.Min(needed, availableWorkers[archetype]);
                    building.AssignWorker(archetype, toAssign);
                    availableWorkers[archetype] -= toAssign;
                    needed -= toAssign;
                }
            }
        }

        // ========== PHASE 3: OPTIMAL WORKERS ==========

        private static void AllocateToOptimalWorkers(
            List<BuildingInstanceData> buildings,
            Dictionary<int, BuildingDefinition> definitions,
            Dictionary<PopulationArchetypes, int> availableWorkers)
        {
            foreach (var building in buildings)
            {
                if (!definitions.ContainsKey(building.BuildingDefinitionID))
                    continue;

                var definition = definitions[building.BuildingDefinitionID];

                if (!definition.RequiresWorkers)
                    continue;

                int optimalWorkers = definition.WorkerSlots.GetTotalMaxWorkers();
                int canAccept = optimalWorkers - building.TotalWorkers;
                if (canAccept <= 0)
                    continue;

                // Prioritize worker types by efficiency (highest efficiency first)
                var sortedSlots = definition.WorkerSlots
                    .OrderByDescending(s => s.EfficiencyMultiplier)
                    .ToList();

                foreach (var slot in sortedSlots)
                {
                    if (canAccept <= 0)
                        break;

                    PopulationArchetypes archetype = slot.Archetype;

                    if (!availableWorkers.ContainsKey(archetype) || availableWorkers[archetype] <= 0)
                        continue;

                    int toAssign = Mathf.Min(canAccept, availableWorkers[archetype]);
                    building.AssignWorker(archetype, toAssign);
                    availableWorkers[archetype] -= toAssign;
                    canAccept -= toAssign;
                }
            }
        }

        // ========== WORKER REMOVAL ==========

        private static int RemoveWorkersFromBuilding(
            BuildingInstanceData building,
            BuildingDefinition definition,
            int maxToRemove)
        {
            int totalRemoved = 0;

            // Create a copy of assigned workers to iterate safely
            var workersCopy = new Dictionary<PopulationArchetypes, int>(building.AssignedWorkers);

            // Prioritize removing non-required workers first
            foreach (var kvp in workersCopy.OrderBy(w => IsRequiredWorker(definition, w.Key) ? 1 : 0))
            {
                if (totalRemoved >= maxToRemove)
                    break;

                PopulationArchetypes archetype = kvp.Key;
                int currentCount = kvp.Value;

                // Determine how many we can remove (keep required workers if possible)
                var slot = definition.WorkerSlots.GetSlot(archetype);
                int requiredCount = slot != null && slot.IsRequired ? slot.MinCount : 0;
                int removable = Mathf.Max(0, currentCount - requiredCount);

                int toRemove = Mathf.Min(removable, maxToRemove - totalRemoved);

                if (toRemove > 0)
                {
                    building.RemoveWorker(archetype, toRemove);
                    totalRemoved += toRemove;
                }
            }

            // If we still need to remove more, remove required workers
            if (totalRemoved < maxToRemove)
            {
                foreach (var kvp in workersCopy)
                {
                    if (totalRemoved >= maxToRemove)
                        break;

                    PopulationArchetypes archetype = kvp.Key;
                    int remaining = building.GetWorkerCount(archetype);

                    int toRemove = Mathf.Min(remaining, maxToRemove - totalRemoved);

                    if (toRemove > 0)
                    {
                        building.RemoveWorker(archetype, toRemove);
                        totalRemoved += toRemove;
                    }
                }
            }

            return totalRemoved;
        }

        // ========== HELPER METHODS ==========

        private static Dictionary<PopulationArchetypes, int> CalculateAvailableWorkers(
            IEnumerable<PopulationGroup> populationGroups)
        {
            var available = new Dictionary<PopulationArchetypes, int>();

            foreach (var group in populationGroups)
            {
                if (!available.ContainsKey(group.Archetype))
                    available[group.Archetype] = 0;

                available[group.Archetype] += group.AvailableWorkers;
            }

            return available;
        }

        private static void ClearAllWorkerAssignments(List<BuildingInstanceData> buildings)
        {
            foreach (var building in buildings)
            {
                building.AssignedWorkers.Clear();
                building.TotalWorkers = 0;
            }
        }

        private static void UpdatePopulationEmployment(
            IEnumerable<PopulationGroup> populationGroups,
            List<BuildingInstanceData> buildings)
        {
            // Reset employment counts
            foreach (var group in populationGroups)
            {
                group.EmployedCount = 0;
            }

            // Count employed workers from buildings
            var employedByArchetype = new Dictionary<PopulationArchetypes, int>();

            foreach (var building in buildings)
            {
                foreach (var kvp in building.AssignedWorkers)
                {
                    if (!employedByArchetype.ContainsKey(kvp.Key))
                        employedByArchetype[kvp.Key] = 0;

                    employedByArchetype[kvp.Key] += kvp.Value;
                }
            }

            // Distribute employment to population groups
            // This is a simplified approach - in reality you might want to track which specific groups work where
            foreach (var group in populationGroups)
            {
                if (employedByArchetype.ContainsKey(group.Archetype))
                {
                    int totalEmployed = employedByArchetype[group.Archetype];
                    int available = group.AvailableWorkers;

                    // Simple proportional assignment
                    if (available > 0)
                    {
                        float proportion = available / (float)GetTotalAvailableForArchetype(populationGroups, group.Archetype);
                        group.EmployedCount = Mathf.FloorToInt(totalEmployed * proportion);
                    }
                }
            }
        }

        private static int GetTotalAvailableForArchetype(IEnumerable<PopulationGroup> groups, PopulationArchetypes archetype)
        {
            return groups.Where(g => g.Archetype == archetype).Sum(g => g.AvailableWorkers);
        }

        private static bool IsRequiredWorker(BuildingDefinition definition, PopulationArchetypes archetype)
        {
            var slot = definition.WorkerSlots.GetSlot(archetype);
            return slot != null && slot.IsRequired;
        }

        // ========== UI PLACEHOLDER METHODS ==========
        // These will be called from UI controls when player manually assigns workers

        /// <summary>
        /// PLACEHOLDER: Manual worker assignment from UI.
        /// Call this when player uses slider/UI to assign workers to a specific building.
        /// This will override automatic allocation for this building.
        /// </summary>
        /// <param name="buildingId">Building to assign workers to</param>
        /// <param name="archetype">Worker type</param>
        /// <param name="count">Number of workers</param>
        /// <param name="isManualOverride">If true, marks this building as manually managed</param>
        public static void AssignWorkerManual(int buildingId, PopulationArchetypes archetype, int count, bool isManualOverride = true)
        {
            // TODO: Implement manual worker assignment
            // - Check if worker type is allowed
            // - Check if count doesn't exceed optimal workers
            // - Check if workers are available
            // - Mark building as manually managed (skip in automatic allocation)
            // - Update BuildingManager
            Debug.LogWarning($"[WorkerAllocationSystem] Manual worker assignment not yet implemented: Building {buildingId}, {archetype}, {count}");
        }

        /// <summary>
        /// PLACEHOLDER: Remove manual worker assignment from UI.
        /// Call this when player uses slider/UI to remove workers from a specific building.
        /// </summary>
        public static void RemoveWorkerManual(int buildingId, PopulationArchetypes archetype, int count)
        {
            // TODO: Implement manual worker removal
            // - Remove workers from building
            // - Update BuildingManager
            // - Optionally trigger reallocation of freed workers
            Debug.LogWarning($"[WorkerAllocationSystem] Manual worker removal not yet implemented: Building {buildingId}, {archetype}, {count}");
        }

        /// <summary>
        /// PLACEHOLDER: Set worker allocation for a building via UI sliders.
        /// Call this when player adjusts sliders for worker distribution.
        /// Each slider affects other sliders' max values (total can't exceed optimal).
        /// </summary>
        /// <param name="buildingId">Building to configure</param>
        /// <param name="workerDistribution">Desired worker counts by archetype</param>
        public static void SetWorkerDistribution(int buildingId, Dictionary<PopulationArchetypes, int> workerDistribution)
        {
            // TODO: Implement UI-driven worker distribution
            // - Validate total doesn't exceed optimal workers
            // - Validate each archetype is allowed
            // - Validate workers are available
            // - Apply distribution
            // - Mark building as manually managed
            Debug.LogWarning($"[WorkerAllocationSystem] Worker distribution UI not yet implemented for Building {buildingId}");
        }

        /// <summary>
        /// PLACEHOLDER: Reset building to automatic worker allocation.
        /// Call this when player wants to return building to automatic management.
        /// </summary>
        public static void ResetToAutomaticAllocation(int buildingId, int cityId)
        {
            // TODO: Implement reset to automatic
            // - Unmark building as manually managed
            // - Trigger reallocation for the city
            Debug.LogWarning($"[WorkerAllocationSystem] Reset to automatic not yet implemented for Building {buildingId}");
        }

        /// <summary>
        /// PLACEHOLDER: Get worker slider constraints for UI.
        /// Returns max values for each slider based on current assignments and available workers.
        /// </summary>
        /// <param name="buildingId">Building to get constraints for</param>
        /// <param name="archetype">Worker type being adjusted</param>
        /// <returns>Maximum value for this slider</returns>
        public static int GetWorkerSliderMax(int buildingId, PopulationArchetypes archetype)
        {
            // TODO: Implement slider max calculation
            // - Get building's optimal workers
            // - Subtract currently assigned workers of other types
            // - Add available workers of this type
            // - Return min(result, available workers of this type)
            Debug.LogWarning($"[WorkerAllocationSystem] Slider max calculation not yet implemented for Building {buildingId}, {archetype}");
            return 0;
        }
    }
}
