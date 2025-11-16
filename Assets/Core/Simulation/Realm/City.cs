using UnityEngine;
using System.Collections.Generic;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms.Demographics;
using TWK.Economy;
using TWK.Cultures;

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
        [SerializeField] private Realm parentRealm;
        private int OwnerRealmID => parentRealm != null ? parentRealm.RealmID : -1; 

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

        // ========== CULTURE CACHING ==========
        private int _cachedMainCultureID = -1;
        private HashSet<BuildingDefinition> _cachedAvailableBuildings = new HashSet<BuildingDefinition>();

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
                cityData.OwnerRealmID = OwnerRealmID;
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

            // Subscribe to culture events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCityCultureChanged += HandleCityCultureChanged;
                CultureManager.Instance.OnCultureBuildingsChanged += HandleCultureBuildingsChanged;

                // Initialize culture cache
                RefreshCultureAndBuildings();
            }

            // Initialize ledger
            dailyLedger = new ResourceLedger(CityID);
            resourceManager.RegisterCity(CityID);

            // Register with PopulationManager
            PopulationManager.Instance.RegisterCity(this);

          // Debug.Log($"[City] Initialized: {Name} (ID: {CityID})");
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

            // Unsubscribe from culture events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCityCultureChanged -= HandleCityCultureChanged;
                CultureManager.Instance.OnCultureBuildingsChanged -= HandleCultureBuildingsChanged;
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
        /// Build a building in this city using BuildingDefinition.
        /// </summary>
        public void BuildBuilding(BuildingDefinition definition, Vector3 position)
        {
            var instanceData = BuildingManager.Instance.ConstructBuilding(CityID, definition, position);
            if (instanceData != null)
            {
                cityData.BuildingIDs.Add(instanceData.ID);
                Debug.Log($"[City] {Name} started construction on {definition.BuildingName} at {position}");
            }
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

        /// <summary>
        /// Get population breakdown by culture with counts and percentages.
        /// </summary>
        public Dictionary<CultureData, (int count, float percentage)> GetCultureBreakdown()
        {
            var culturePopulations = new Dictionary<CultureData, int>();
            int totalPopulation = 0;

            foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(CityID))
            {
                if (pop.Culture == null) continue;

                if (!culturePopulations.ContainsKey(pop.Culture))
                    culturePopulations[pop.Culture] = 0;

                culturePopulations[pop.Culture] += pop.PopulationCount;
                totalPopulation += pop.PopulationCount;
            }

            var breakdown = new Dictionary<CultureData, (int count, float percentage)>();
            foreach (var kvp in culturePopulations)
            {
                float percentage = totalPopulation > 0 ? (kvp.Value / (float)totalPopulation) * 100f : 0f;
                breakdown[kvp.Key] = (kvp.Value, percentage);
            }

            return breakdown;
        }

        /// <summary>
        /// Get the city's main (dominant) culture.
        /// Returns the culture with the highest population, or null if the city has no population.
        /// </summary>
        public CultureData GetMainCulture()
        {
            if (_cachedMainCultureID == -1)
                return null;

            return CultureManager.Instance.GetCulture(_cachedMainCultureID);
        }

        /// <summary>
        /// Get population breakdown by religion with counts and percentages.
        /// </summary>
        public Dictionary<TWK.Religion.ReligionData, (int count, float percentage)> GetReligionBreakdown()
        {
            var religionPopulations = new Dictionary<TWK.Religion.ReligionData, int>();
            int totalPopulation = 0;

            foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(CityID))
            {
                if (pop.CurrentReligion == null) continue;

                if (!religionPopulations.ContainsKey(pop.CurrentReligion))
                    religionPopulations[pop.CurrentReligion] = 0;

                religionPopulations[pop.CurrentReligion] += pop.PopulationCount;
                totalPopulation += pop.PopulationCount;
            }

            var breakdown = new Dictionary<TWK.Religion.ReligionData, (int count, float percentage)>();
            foreach (var kvp in religionPopulations)
            {
                float percentage = totalPopulation > 0 ? (kvp.Value / (float)totalPopulation) * 100f : 0f;
                breakdown[kvp.Key] = (kvp.Value, percentage);
            }

            return breakdown;
        }

        /// <summary>
        /// Get the city's main (dominant) religion.
        /// Returns the religion with the highest population, or null if the city has no population.
        /// </summary>
        public TWK.Religion.ReligionData GetMainReligion()
        {
            var religionBreakdown = GetReligionBreakdown();

            if (religionBreakdown.Count == 0)
                return null;

            // Find religion with highest population
            TWK.Religion.ReligionData dominantReligion = null;
            int maxPopulation = 0;

            foreach (var kvp in religionBreakdown)
            {
                if (kvp.Value.count > maxPopulation)
                {
                    maxPopulation = kvp.Value.count;
                    dominantReligion = kvp.Key;
                }
            }

            return dominantReligion;
        }

        /// <summary>
        /// Get all buildings available to construct in this city based on its culture.
        /// </summary>
        public HashSet<BuildingDefinition> GetAvailableBuildings()
        {
            return new HashSet<BuildingDefinition>(_cachedAvailableBuildings);
        }

        // ========== CULTURE EVENT HANDLERS ==========

        /// <summary>
        /// Handle culture change events from CultureManager.
        /// </summary>
        private void HandleCityCultureChanged(int cityID, int oldCultureID, int newCultureID)
        {
            // Only respond to events for this city
            if (cityID != this.CityID) return;

            Debug.Log($"[City] {Name} culture changed from {oldCultureID} to {newCultureID}");

            // Refresh culture and buildings cache
            RefreshCultureAndBuildings();
        }

        /// <summary>
        /// Handle culture buildings changed events from CultureManager.
        /// </summary>
        private void HandleCultureBuildingsChanged(int cultureID)
        {
            // Only respond if this culture affects our city
            if (cultureID != _cachedMainCultureID) return;

            Debug.Log($"[City] {Name} refreshing buildings due to culture {cultureID} unlocking new buildings");

            // Refresh available buildings
            RefreshAvailableBuildings();
        }

        /// <summary>
        /// Refresh the cached main culture and available buildings.
        /// Called when city culture changes or population shifts.
        /// </summary>
        private void RefreshCultureAndBuildings()
        {
            if (CultureManager.Instance == null) return;

            // Recalculate main culture
            int newCultureID = CultureManager.Instance.CalculateCityCulture(CityID);
            _cachedMainCultureID = newCultureID;

            // Refresh available buildings
            RefreshAvailableBuildings();
        }

        /// <summary>
        /// Refresh the cached available buildings from the city's main culture.
        /// Called when culture changes or when culture unlocks new buildings.
        /// </summary>
        private void RefreshAvailableBuildings()
        {
            _cachedAvailableBuildings.Clear();

            if (_cachedMainCultureID == -1)
            {
                Debug.Log($"[City] {Name} has no main culture (no population)");
                return;
            }

            var culture = CultureManager.Instance.GetCulture(_cachedMainCultureID);
            if (culture == null)
            {
                Debug.LogWarning($"[City] {Name} culture ID {_cachedMainCultureID} not found");
                return;
            }

            _cachedAvailableBuildings = culture.GetAllUnlockedBuildings();
            Debug.Log($"[City] {Name} has {_cachedAvailableBuildings.Count} buildings available from culture {culture.CultureName}");
        }

        // ========== DEBUG & DIAGNOSTICS ==========
        private void PrintDailyEconomyReport()
        {
            //Debug.Log($"--- [City: {Name}] Daily Economy Report ---");

            //foreach (var kvp in EconomySnapshot.Net)
            //{
            //    var resource = kvp.Key;
            //    var net = kvp.Value;

            //    EconomySnapshot.Production.TryGetValue(resource, out var produced);
            //    EconomySnapshot.Consumption.TryGetValue(resource, out var consumed);

            //    string status = net < 0 ? "! DEFICIT" : "^ SURPLUS";
            //    Debug.Log($"{status} | {resource}: Net {net}, Produced {produced}, Consumed {consumed}");
            //}

            //Debug.Log($"--------------------------------------------");
        }
    }
}
