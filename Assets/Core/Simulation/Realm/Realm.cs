using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;
using TWK.Agents;
using TWK.Core;
using TWK.Simulation;

namespace TWK.Realms
{
    /// <summary>
    /// Realm MonoBehaviour - the "View" in MVVM.
    /// Holds RealmData and coordinates Unity-specific behavior.
    /// Manages realm treasury and coordinates tax/tribute collection.
    /// Follows the same pattern as City.cs.
    /// </summary>
    public class Realm : MonoBehaviour, ISimulationAgent
    {
        // ========== DATA MODEL ==========
        [SerializeField] private RealmData realmData = new RealmData();

        // ========== INSPECTOR CONFIGURATION (for initialization) ==========
        [Header("Realm Configuration")]
        [SerializeField] private string realmName = "NewRealm";
        [SerializeField] private int realmID = 0;
        [SerializeField] private int cultureID = -1;

        // ========== PROPERTIES (redirect to realmData) ==========
        public string RealmName
        {
            get => realmData.RealmName;
            set => realmData.RealmName = value;
        }

        public int RealmID
        {
            get => realmData.RealmID;
            set => realmData.RealmID = value;
        }

        public List<City> Cities
        {
            get
            {
                var cities = new List<City>();
                foreach (int cityID in realmData.DirectlyOwnedCityIDs)
                {
                    // Find city GameObject (this is a temporary solution)
                    // TODO: Replace with proper city lookup via CityManager
                    City[] cityObjects = FindObjectsByType<City>(FindObjectsSortMode.None);
                    foreach (City city in cityObjects)
                    {
                        if (city.CityID == cityID)
                        {
                            cities.Add(city);
                            break;
                        }
                    }
                }
                return cities;
            }
        }

        public List<Agent> Leaders
        {
            get
            {
                var leaders = new List<Agent>();
                foreach (int agentID in realmData.LeaderIDs)
                {
                    // TODO: Replace with proper agent lookup via AgentManager when Phase 7B is implemented
                    // For now, return empty list
                }
                return leaders;
            }
        }

        public CultureData RealmCulture
        {
            get
            {
                if (realmData.RealmCultureID == -1)
                    return null;

                // Get culture from CultureManager
                return CultureManager.Instance?.GetCulture(realmData.RealmCultureID);
            }
            set
            {
                if (value != null)
                {
                    realmData.RealmCultureID = value.GetCultureID();
                }
            }
        }

        // ========== DATA ACCESS ==========
        /// <summary>
        /// Direct access to the underlying data model for ViewModels.
        /// </summary>
        public RealmData Data => realmData;

        // ========== RUNTIME DEPENDENCIES ==========
        private RealmTreasury _treasury;
        private WorldTimeManager _worldTimeManager;

        /// <summary>
        /// Access to the realm's treasury.
        /// </summary>
        public RealmTreasury Treasury => _treasury;

        // ========== LIFECYCLE ==========
        private void Awake()
        {
            // Initialize realmData from Inspector values
            if (string.IsNullOrEmpty(realmData.RealmName))
            {
                realmData.RealmName = realmName;
                realmData.RealmID = realmID;
                realmData.RealmCultureID = cultureID;
            }
        }

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            _worldTimeManager = worldTimeManager;

            // Initialize treasury
            _treasury = new RealmTreasury(realmData);

            // Subscribe to time events
            _worldTimeManager.OnDayTick += AdvanceDay;
            _worldTimeManager.OnSeasonTick += AdvanceSeason;
            _worldTimeManager.OnYearTick += AdvanceYear;

            // Register with RealmManager
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.RegisterRealm(this);
            }
            else
            {
                Debug.LogWarning($"[Realm] RealmManager not found when initializing {RealmName}");
            }

            Debug.Log($"[Realm] Initialized: {RealmName} (ID: {RealmID})");
        }

        private void OnDestroy()
        {
            // Unsubscribe from time events
            if (_worldTimeManager != null)
            {
                _worldTimeManager.OnDayTick -= AdvanceDay;
                _worldTimeManager.OnSeasonTick -= AdvanceSeason;
                _worldTimeManager.OnYearTick -= AdvanceYear;
            }

            // Unregister from RealmManager
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.UnregisterRealm(RealmID);
            }
        }

        // ========== SIMULATION TICKS ==========
        public void AdvanceDay()
        {
            // Daily realm logic
            // Tax/tribute collection is handled by RealmManager.CollectAllRealmRevenue()
            // to ensure proper order of operations

            UpdateRealmStatistics();
        }

        public void AdvanceSeason()
        {
            // Seasonal realm logic
        }

        public void AdvanceYear()
        {
            // Yearly realm logic
        }

        // ========== REALM MANAGEMENT ==========

        /// <summary>
        /// Add a city to this realm's direct control.
        /// </summary>
        public void AddCity(int cityID)
        {
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.AddCityToRealm(RealmID, cityID);
            }
            else
            {
                realmData.AddCity(cityID);
            }
        }

        /// <summary>
        /// Remove a city from this realm's direct control.
        /// </summary>
        public void RemoveCity(int cityID)
        {
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.RemoveCityFromRealm(RealmID, cityID);
            }
            else
            {
                realmData.RemoveCity(cityID);
            }
        }

        /// <summary>
        /// Add a leader to this realm.
        /// </summary>
        public void AddLeader(int agentID)
        {
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.AddRealmLeader(RealmID, agentID);
            }
            else
            {
                realmData.AddLeader(agentID);
            }
        }

        /// <summary>
        /// Remove a leader from this realm.
        /// </summary>
        public void RemoveLeader(int agentID)
        {
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.RemoveRealmLeader(RealmID, agentID);
            }
            else
            {
                realmData.RemoveLeader(agentID);
            }
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Update cached realm statistics (population, military, etc).
        /// </summary>
        private void UpdateRealmStatistics()
        {
            // Calculate total population from all directly owned cities
            int totalPop = 0;
            foreach (int cityID in realmData.DirectlyOwnedCityIDs)
            {
                // TODO: Get city population via CityManager
                // For now, just leave at 0
            }
            realmData.TotalPopulation = totalPop;

            // TODO: Calculate total military strength
            realmData.TotalMilitaryStrength = 0;
        }

        /// <summary>
        /// Get total population across all directly owned cities.
        /// </summary>
        public int GetTotalPopulation()
        {
            return realmData.TotalPopulation;
        }

        /// <summary>
        /// Get total military strength.
        /// </summary>
        public int GetTotalMilitaryStrength()
        {
            return realmData.TotalMilitaryStrength;
        }

        // ========== DEBUG ==========

        [ContextMenu("Print Realm Info")]
        public void PrintRealmInfo()
        {
            Debug.Log($"=== Realm: {RealmName} (ID: {RealmID}) ===");
            Debug.Log($"Culture: {RealmCulture?.CultureName ?? "None"}");
            Debug.Log($"Leaders: {realmData.LeaderIDs.Count}");
            Debug.Log($"Direct Cities: {realmData.DirectlyOwnedCityIDs.Count}");
            Debug.Log($"Vassals: {realmData.VassalContractIDs.Count}");
            Debug.Log($"Independent: {realmData.IsIndependent}");
            Debug.Log($"Treasury Gold: {_treasury?.GetResource(TWK.Economy.ResourceType.Gold) ?? 0}");
        }
    }
}
