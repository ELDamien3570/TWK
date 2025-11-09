using System.Collections.Generic;
using UnityEngine;

namespace TWK.Economy
{
    [System.Serializable]
    public struct BuildingInstance
    {
        public int ID;
        public int CityID;
        public BuildingData BuildingData;
        public Vector3 Position;

        public float EfficiencyMultiplier;
        public bool IsActive;
        
        // Construction tracking
        public BuildingConstructionState ConstructionState;
        public int ConstructionDaysRemaining;
        public int TotalConstructionDays;
        public bool HasPaidConstructionCost;

        public BuildingInstance(int id, int cityId, BuildingData data, Vector3 position)
        {
            ID = id;
            CityID = cityId;
            BuildingData = data;
            Position = position;
            EfficiencyMultiplier = data.BaseEfficiency;

            TotalConstructionDays = data.ConstructionTimeDays;
            ConstructionDaysRemaining = data.ConstructionTimeDays;
            ConstructionState = data.ConstructionTimeDays > 0 ? BuildingConstructionState.UnderConstruction : BuildingConstructionState.Completed;
            IsActive = ConstructionState == BuildingConstructionState.Completed;
            HasPaidConstructionCost = false;
        }   

        public void Simulate(ResourceLedger ledger)
        {
            // Only simulate production/consumption if building is completed and active
            if (!IsActive || BuildingData == null || ConstructionState != BuildingConstructionState.Completed) 
                return;

            // Production (only when completed)
            foreach (var prod in BuildingData.BaseProduction)
            {
                ledger.Add(prod.Key, Mathf.RoundToInt(prod.Value * EfficiencyMultiplier));
            }

            // Maintenance costs (only when completed)
            foreach (var cost in BuildingData.BaseMaintenanceCost)
            {
                ledger.Subtract(cost.Key, Mathf.RoundToInt(cost.Value));
            }
        }

        // Make this method public and return bool to indicate if construction was advanced
        public bool AdvanceConstruction()
        {
            Debug.Log("AdvanceConstruction called for BuildingInstance ID: " + ID); 
            if (ConstructionState != BuildingConstructionState.UnderConstruction)
                return false;

            ConstructionDaysRemaining--;
            Debug.Log($"[BuildingInstance] {BuildingData.BuildingName} (ID: {ID}) construction progress: {TotalConstructionDays - ConstructionDaysRemaining}/{TotalConstructionDays} days");

            if (ConstructionDaysRemaining <= 0)
            {
                CompleteConstruction();
                return true; // Construction completed
            }
            
            return true; // Construction advanced
        }

        private void CompleteConstruction()
        {
            ConstructionState = BuildingConstructionState.Completed;
            ConstructionDaysRemaining = 0;
            IsActive = true;
            
            Debug.Log($"[BuildingInstance] {BuildingData.BuildingName} (ID: {ID}) construction completed!");
        }

        // Helper method to get construction costs for refund purposes
        public Dictionary<ResourceType, int> GetConstructionCosts()
        {
            return new Dictionary<ResourceType, int>(BuildingData.BaseBuildCost);
        }

        // Helper properties
        public bool IsUnderConstruction => ConstructionState == BuildingConstructionState.UnderConstruction;
        public bool CanBeCancelled => IsUnderConstruction && HasPaidConstructionCost;
        public float ConstructionProgress => TotalConstructionDays > 0 ? 
            (float)(TotalConstructionDays - ConstructionDaysRemaining) / TotalConstructionDays : 1f;
    }
}
