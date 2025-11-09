using System.Collections.Generic;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms;

namespace TWK.Economy
{
    public class BuildingManager : MonoBehaviour, ISimulationAgent
    {
        public static BuildingManager Instance { get; private set; }

        private int nextBuildingID = 0;
        private readonly List<BuildingInstance> buildings = new();
        private WorldTimeManager worldTimeManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            this.worldTimeManager = worldTimeManager;
            worldTimeManager.OnDayTick += AdvanceDay;
        }

        public void AdvanceDay()
        {
            // Process construction for all buildings
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].IsUnderConstruction)
                {
                    Debug.Log("Attempting to advance construction for building ID: " + buildings[i].ID);
                    // Get a copy, modify it, then put it back
                    var building = buildings[i];
                    building.AdvanceConstruction();
                    buildings[i] = building;
                }
            }
        }

        public void AdvanceSeason() { }
        public void AdvanceYear() { }

        public BuildingInstance ConstructBuilding(int cityID, BuildingData data, Vector3 position)
        {
            var instance = new BuildingInstance(nextBuildingID++, cityID, data, position);
            
            // Immediately deduct construction costs from city
            if (DeductConstructionCosts(cityID, instance))
            {
                // Mark that construction costs have been paid
                instance.HasPaidConstructionCost = true;
                buildings.Add(instance);
                
                if (instance.IsUnderConstruction)
                {
                    Debug.Log($"[BuildingManager] Started construction of {data.BuildingName} - {data.ConstructionTimeDays} days remaining. Costs deducted immediately.");
                }
                else
                {
                    Debug.Log($"[BuildingManager] {data.BuildingName} completed immediately. Costs deducted.");
                }
                
                return instance;
            }
            else
            {
                Debug.LogWarning($"[BuildingManager] Insufficient resources to build {data.BuildingName} in city {cityID}");
                return default; // Return default instance if construction failed
            }
        }

        private bool DeductConstructionCosts(int cityID, BuildingInstance building)
        {
            var resourceManager = ResourceManager.Instance;
            
            // Check if city has enough resources
            foreach (var cost in building.BuildingData.BaseBuildCost)
            {
                int available = resourceManager.GetResource(cityID, cost.Key);
                if (available < cost.Value)
                {
                    Debug.LogWarning($"[BuildingManager] City {cityID} lacks {cost.Key}: has {available}, needs {cost.Value}");
                    return false;
                }
            }
            
            // Deduct costs using a temporary ledger
            var costLedger = new ResourceLedger(cityID);
            foreach (var cost in building.BuildingData.BaseBuildCost)
            {
                costLedger.Subtract(cost.Key, cost.Value);
            }
            
            resourceManager.ApplyLedger(cityID, costLedger);
            Debug.Log($"[BuildingManager] Deducted construction costs for {building.BuildingData.BuildingName}");
            return true;
        }

        public bool CancelConstruction(int buildingID, int cityID)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (building.ID == buildingID && building.CityID == cityID)
                {
                    if (!building.CanBeCancelled)
                    {
                        Debug.LogWarning($"[BuildingManager] Cannot cancel building {buildingID} - either not under construction or costs not paid");
                        return false;
                    }

                    // Calculate refund (could implement partial refund based on progress)
                    float refundPercentage = CalculateRefundPercentage(building);
                    RefundConstructionCosts(cityID, building, refundPercentage);
                    
                    // Remove building from list
                    buildings.RemoveAt(i);
                    
                    Debug.Log($"[BuildingManager] Cancelled construction of {building.BuildingData.BuildingName} (ID: {buildingID}). Refunded {refundPercentage:P0} of costs.");
                    return true;
                }
            }
            
            Debug.LogWarning($"[BuildingManager] Building {buildingID} not found for cancellation");
            return false;
        }

        private float CalculateRefundPercentage(BuildingInstance building)
        {
            // Options for refund calculation:
            
            // Option 1: Full refund regardless of progress
            // return 1.0f;
            
            // Option 2: Refund based on remaining construction time
            float progressMade = building.ConstructionProgress;
            return Mathf.Lerp(1.0f, 0.5f, progressMade); // 100% refund at start, 50% when nearly complete
            
            // Option 3: Fixed percentage
            // return 0.75f; // Always 75% refund
        }

        private void RefundConstructionCosts(int cityID, BuildingInstance building, float refundPercentage)
        {
            var refundLedger = new ResourceLedger(cityID);
            
            foreach (var cost in building.BuildingData.BaseBuildCost)
            {
                int refundAmount = Mathf.RoundToInt(cost.Value * refundPercentage);
                refundLedger.Add(cost.Key, refundAmount);
            }
            
            ResourceManager.Instance.ApplyLedger(cityID, refundLedger);
            Debug.Log($"[BuildingManager] Refunded construction costs to city {cityID}");
        }

        public IEnumerable<BuildingInstance> GetBuildingsForCity(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID)
                    yield return b;
        }

        public IEnumerable<BuildingInstance> GetCompletedBuildingsForCity(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID && b.ConstructionState == BuildingConstructionState.Completed)
                    yield return b;
        }

        public IEnumerable<BuildingInstance> GetBuildingsUnderConstruction(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID && b.IsUnderConstruction)
                    yield return b;
        }

        public BuildingInstance? GetBuildingByID(int buildingID)
        {
            foreach (var building in buildings)
            {
                if (building.ID == buildingID)
                    return building;
            }
            return null;
        }
    }
}
