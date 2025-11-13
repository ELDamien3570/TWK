using System.Collections.Generic;
using UnityEngine;
using TWK.Realms.Demographics;

namespace TWK.Economy
{
    /// <summary>
    /// Pure data class for a building instance.
    /// Contains no logic, just serializable state.
    /// This is the "Model" in MVVM.
    /// </summary>
    [System.Serializable]
    public class BuildingInstanceData
    {
        // ========== IDENTITY ==========
        public int ID;
        public int CityID;
        public int BuildingDefinitionID; // Reference to BuildingDefinition (store ID or name)

        // ========== LOCATION ==========
        public float PositionX, PositionY, PositionZ;

        public Vector3 Position
        {
            get => new Vector3(PositionX, PositionY, PositionZ);
            set { PositionX = value.x; PositionY = value.y; PositionZ = value.z; }
        }

        // ========== HUB SYSTEM ==========
        [Tooltip("If this is a hublet, what hub is it attached to?")]
        public int AttachedToHubID = -1;

        [Tooltip("If this is a hub, which hublets are attached?")]
        public List<int> AttachedHubletIDs = new List<int>();

        // ========== STATE ==========
        public bool IsActive;
        public float EfficiencyMultiplier;

        // ========== CONSTRUCTION ==========
        public BuildingConstructionState ConstructionState;
        public int ConstructionDaysRemaining;
        public int TotalConstructionDays;
        public bool HasPaidConstructionCost;

        // ========== WORKERS ==========
        [Tooltip("Workers assigned by archetype")]
        public Dictionary<PopulationArchetypes, int> AssignedWorkers = new Dictionary<PopulationArchetypes, int>();

        [Tooltip("Total workers currently assigned")]
        public int TotalWorkers = 0;

        // ========== CONSTRUCTOR ==========
        public BuildingInstanceData()
        {
            // Default constructor for serialization
        }

        public BuildingInstanceData(int id, int cityId, int buildingDefId, Vector3 position, int constructionDays, float baseEfficiency)
        {
            this.ID = id;
            this.CityID = cityId;
            this.BuildingDefinitionID = buildingDefId;
            this.Position = position;
            this.EfficiencyMultiplier = baseEfficiency;

            this.TotalConstructionDays = constructionDays;
            this.ConstructionDaysRemaining = constructionDays;
            this.ConstructionState = constructionDays > 0 ? BuildingConstructionState.UnderConstruction : BuildingConstructionState.Completed;
            this.IsActive = ConstructionState == BuildingConstructionState.Completed;
            this.HasPaidConstructionCost = false;

            this.AssignedWorkers = new Dictionary<PopulationArchetypes, int>();
            this.TotalWorkers = 0;
            this.AttachedHubletIDs = new List<int>();
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Assign workers to this building.
        /// </summary>
        public void AssignWorker(PopulationArchetypes archetype, int count)
        {
            if (!AssignedWorkers.ContainsKey(archetype))
                AssignedWorkers[archetype] = 0;

            AssignedWorkers[archetype] += count;
            RecalculateTotalWorkers();
        }

        /// <summary>
        /// Remove workers from this building.
        /// </summary>
        public void RemoveWorker(PopulationArchetypes archetype, int count)
        {
            if (!AssignedWorkers.ContainsKey(archetype))
                return;

            AssignedWorkers[archetype] -= count;
            if (AssignedWorkers[archetype] <= 0)
                AssignedWorkers.Remove(archetype);

            RecalculateTotalWorkers();
        }

        /// <summary>
        /// Clear all workers.
        /// </summary>
        public void ClearWorkers()
        {
            AssignedWorkers.Clear();
            TotalWorkers = 0;
        }

        /// <summary>
        /// Recalculate total workers from assigned workers dictionary.
        /// </summary>
        private void RecalculateTotalWorkers()
        {
            TotalWorkers = 0;
            foreach (var kvp in AssignedWorkers)
                TotalWorkers += kvp.Value;
        }

        /// <summary>
        /// Get worker count for a specific archetype.
        /// </summary>
        public int GetWorkerCount(PopulationArchetypes archetype)
        {
            return AssignedWorkers.GetValueOrDefault(archetype, 0);
        }

        // ========== STATE PROPERTIES ==========
        public bool IsUnderConstruction => ConstructionState == BuildingConstructionState.UnderConstruction;
        public bool IsCompleted => ConstructionState == BuildingConstructionState.Completed;
        public bool CanBeCancelled => IsUnderConstruction && HasPaidConstructionCost;
        public float ConstructionProgress => TotalConstructionDays > 0 ?
            (float)(TotalConstructionDays - ConstructionDaysRemaining) / TotalConstructionDays : 1f;
    }
}
