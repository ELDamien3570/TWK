using UnityEngine;
using System.Collections.Generic;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms.Demographics;
using TWK.Economy;

namespace TWK.Realms
{
    public struct CityEconomySnapshot
    {
        public Dictionary<ResourceType, int> Production;
        public Dictionary<ResourceType, int> Consumption;
        public Dictionary<ResourceType, int> Net;
    }

    public class City : MonoBehaviour, ISimulationAgent
    {
        public string Name;
        public int CityID;
        public float GrowthRate;
        public Vector3 Location;
        public float TerritoryRadius;

        public ResourceLedger DailyLedger { get; private set; }
        public List<BuildingInstance> Buildings { get; private set; } = new();
        public CityEconomySnapshot EconomySnapshot;

        private ResourceManager resourceManager;
        private WorldTimeManager worldTimeManager;

        public void Initialize(WorldTimeManager timeManager)
        {
            worldTimeManager = timeManager;
            resourceManager = ResourceManager.Instance;

            worldTimeManager.OnDayTick += AdvanceDay;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;

            DailyLedger = new ResourceLedger(CityID);
            resourceManager.RegisterCity(CityID);
            
            // Register this city with the PopulationManager
            PopulationManager.Instance.RegisterCity(this);
        }

        #region Simulation Ticks
        public void AdvanceDay()
        {
            DailyLedger.ClearDailyChange();

            // Sync building states with BuildingManager first
            SyncBuildingStates();
            
            SimulateBuildings();
            SimulatePopulation();

            resourceManager.ApplyLedger(CityID, DailyLedger);
            UpdateEconomySnapshot();

            // For debugging purposes
            PrintDailyEconomyReport();
        }

        public void AdvanceSeason() { }
        public void AdvanceYear() { }
        #endregion

        #region Simulation Helpers
        private void SyncBuildingStates()
        {
            // Update building states from BuildingManager
            for (int i = 0; i < Buildings.Count; i++)
            {
                var currentBuilding = Buildings[i];
                var updatedBuilding = BuildingManager.Instance.GetBuildingByID(currentBuilding.ID);
                
                if (updatedBuilding.HasValue)
                {
                    Buildings[i] = updatedBuilding.Value;
                }
            }
        }

        private void SimulateBuildings()
        {
            foreach (var building in Buildings)
            {
                if (building.ConstructionState == BuildingConstructionState.Completed)
                {
                    building.Simulate(DailyLedger);
                    Debug.Log($"[City] Simulating completed building: {building.BuildingData.BuildingName} (ID: {building.ID})");
                }
            }
        }

        private void SimulatePopulation() // Test simulation of population food consumption
        {
            int totalPop = GetTotalPopulation();

            // Food consumption (City-level responsibility)
            int foodConsumption = totalPop; // placeholder
             DailyLedger.Subtract(ResourceType.Food, foodConsumption);
        }

        private void UpdateEconomySnapshot()
        {
            EconomySnapshot.Production = new Dictionary<ResourceType, int>(DailyLedger.DailyProduction);
            EconomySnapshot.Consumption = new Dictionary<ResourceType, int>(DailyLedger.DailyConsumption);
            EconomySnapshot.Net = new Dictionary<ResourceType, int>(DailyLedger.DailyChange);
        }

        #endregion


        #region Test helpers
        public void BuildFarm(FarmBuildingData farmData, Vector3 position) //Quick test method to build a farm
        {
            var instance = BuildingManager.Instance.ConstructBuilding(CityID, farmData, position);
            Buildings.Add(instance);
            Debug.Log($"City {Name} started construction on a Farm at {position}");
        }

        public int GetTotalPopulation() //Returns total population count for the city
        {
            int total = 0;
            foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(CityID))
                total += pop.PopulationCount;
            return total;
        }

        public Dictionary<PopulationArchetypes, int> GetPopulationBreakdown() //Returns population breakdown by archetype
        {
            var breakdown = new Dictionary<PopulationArchetypes, int>();
            foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(CityID))
            {
                if (!breakdown.ContainsKey(pop.Archetype))
                    breakdown[pop.Archetype] = 0;
                breakdown[pop.Archetype] += pop.PopulationCount;
            }
            return breakdown;
        }
        #endregion    

    #region Debug & Diagnostics
        private void PrintDailyEconomyReport()
        {
            Debug.Log($"--- [City: {Name}] Daily Economy Report ---");

            foreach (var kvp in EconomySnapshot.Net)
            {
                var resource = kvp.Key;
                var net = kvp.Value;

                EconomySnapshot.Production.TryGetValue(resource, out var produced);
                EconomySnapshot.Consumption.TryGetValue(resource, out var consumed);

                string status = net < 0 ? "! DEFICIT" : "^ SURPLUS";
                Debug.Log($"{status} | {resource}: Net {net}, Produced {produced}, Consumed {consumed}");
            }

            Debug.Log($"--------------------------------------------");
        }
        #endregion
    }
}
