using UnityEngine;
using System.Collections.Generic;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms.Demographics;
using TWK.Economy;

namespace TWK.Realms
{
    /// <summary>
    /// City MonoBehaviour - the "View" in MVVM.
    /// Holds CityData and coordinates Unity-specific behavior.
    /// Delegates simulation logic to CitySimulation static functions.
    /// </summary>
    public class City : MonoBehaviour, ISimulationAgent
    {
        // ========== DATA MODEL ==========
        [SerializeField] private CityData cityData = new CityData();

        // ========== INSPECTOR CONFIGURATION (for initialization) ==========
        [Header("City Configuration")]
        [SerializeField] private string cityName = "NewCity";
        [SerializeField] private int cityID = 0;
        [SerializeField] private float growthRate = 1f;
        [SerializeField] private float territoryRadius = 10f;

        // ========== PROPERTIES (redirect to cityData) ==========
        public string Name
        {
            get => cityData.Name;
            set => cityData.Name = value;
        }

        public int CityID
        {
            get => cityData.CityID;
            set => cityData.CityID = value;
        }

        public float GrowthRate
        {
            get => cityData.GrowthRate;
            set => cityData.GrowthRate = value;
        }

        public Vector3 Location
        {
            get => cityData.Location;
            set => cityData.Location = value;
        }

        public float TerritoryRadius
        {
            get => cityData.TerritoryRadius;
            set => cityData.TerritoryRadius = value;
        }

        public CityEconomySnapshot EconomySnapshot => cityData.EconomySnapshot;

        // ========== DATA ACCESS ==========
        /// <summary>
        /// Direct access to the underlying data model for ViewModels.
        /// </summary>
        public CityData Data => cityData;

        // ========== RUNTIME DEPENDENCIES ==========
        private ResourceLedger dailyLedger;
        private ResourceManager resourceManager;
        private WorldTimeManager worldTimeManager;

        // ========== LIFECYCLE ==========
        private void Awake()
        {
            // Initialize cityData from Inspector values
            if (string.IsNullOrEmpty(cityData.Name))
            {
                cityData.Name = cityName;
                cityData.CityID = cityID;
                cityData.Location = transform.position;
                cityData.GrowthRate = growthRate;
                cityData.TerritoryRadius = territoryRadius;
            }
        }

        public void Initialize(WorldTimeManager timeManager)
        {
            worldTimeManager = timeManager;
            resourceManager = ResourceManager.Instance;

            // Subscribe to time events
            worldTimeManager.OnDayTick += AdvanceDay;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;

            // Initialize ledger
            dailyLedger = new ResourceLedger(CityID);
            resourceManager.RegisterCity(CityID);

            // Register with PopulationManager
            PopulationManager.Instance.RegisterCity(this);

            Debug.Log($"[City] Initialized: {Name} (ID: {CityID})");
        }

        private void OnDestroy()
        {
            // Unsubscribe from time events
            if (worldTimeManager != null)
            {
                worldTimeManager.OnDayTick -= AdvanceDay;
                worldTimeManager.OnSeasonTick -= AdvanceSeason;
                worldTimeManager.OnYearTick -= AdvanceYear;
            }
        }

        // ========== SIMULATION TICKS ==========
        public void AdvanceDay()
        {
            dailyLedger.ClearDailyChange();

            // Delegate to pure simulation logic
            CitySimulation.SimulateDay(cityData, dailyLedger);

            // Apply results
            resourceManager.ApplyLedger(CityID, dailyLedger);
            CitySimulation.UpdateEconomySnapshot(cityData, dailyLedger);

            // Debug
            PrintDailyEconomyReport();
        }

        public void AdvanceSeason()
        {
            CitySimulation.SimulateSeason(cityData);
        }

        public void AdvanceYear()
        {
            CitySimulation.SimulateYear(cityData);
        }

        // ========== PUBLIC API ==========
        /// <summary>
        /// Build a building in this city (test helper).
        /// </summary>
        public void BuildFarm(FarmBuildingData farmData, Vector3 position)
        {
            var instance = BuildingManager.Instance.ConstructBuilding(CityID, farmData, position);
            cityData.BuildingIDs.Add(instance.ID);
            Debug.Log($"[City] {Name} started construction on a Farm at {position}");
        }

        /// <summary>
        /// Get total population for this city from PopulationManager.
        /// </summary>
        public int GetTotalPopulation()
        {
            return CitySimulation.GetTotalPopulation(CityID);
        }

        /// <summary>
        /// Get population breakdown by archetype.
        /// </summary>
        public Dictionary<PopulationArchetypes, int> GetPopulationBreakdown()
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

        // ========== DEPRECATED (for backward compatibility) ==========
        /// <summary>
        /// Deprecated: Use cityData.BuildingIDs instead.
        /// </summary>
        public List<BuildingInstance> Buildings
        {
            get
            {
                var buildings = new List<BuildingInstance>();
                foreach (int id in cityData.BuildingIDs)
                {
                    var building = BuildingManager.Instance.GetBuildingByID(id);
                    if (building.HasValue)
                        buildings.Add(building.Value);
                }
                return buildings;
            }
        }

        // ========== DEBUG & DIAGNOSTICS ==========
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
    }
}
