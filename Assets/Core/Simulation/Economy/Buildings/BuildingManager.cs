using System.Collections.Generic;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms;
using TWK.Cultures;

namespace TWK.Economy
{
    /// <summary>
    /// Manages all building instances in the game.
    /// Uses BuildingInstanceData (MVVM data layer) internally.
    /// </summary>
    public class BuildingManager : MonoBehaviour, ISimulationAgent
    {
        public static BuildingManager Instance { get; private set; }

        private int nextBuildingID = 0;
        private List<BuildingInstanceData> buildings = new List<BuildingInstanceData>();
        private Dictionary<int, BuildingInstanceData> buildingLookup = new Dictionary<int, BuildingInstanceData>();

        // City-indexed building cache for faster city-based queries
        private Dictionary<int, List<int>> buildingsByCity = new Dictionary<int, List<int>>();

        // Definition lookup (BuildingDefinition.GetInstanceID() -> BuildingDefinition)
        // In real implementation, this would be populated from ScriptableObjects
        private Dictionary<int, BuildingDefinition> definitionLookup = new Dictionary<int, BuildingDefinition>();

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

            // TODO: Load all BuildingDefinition ScriptableObjects
            // LoadBuildingDefinitions();
        }

        // ========== SIMULATION ==========

        public void AdvanceDay()
        {
            // Track which cities have completed buildings (to allocate workers)
            var citiesWithCompletedBuildings = new HashSet<int>();

            // Process construction for all buildings
            foreach (var building in buildings)
            {
                if (building.IsUnderConstruction)
                {
                    bool justCompleted = BuildingSimulation.AdvanceConstruction(building);

                    // If building just completed, mark city for worker allocation
                    if (justCompleted && building.IsCompleted)
                    {
                        citiesWithCompletedBuildings.Add(building.CityID);
                        Debug.Log($"[BuildingManager] Building {building.ID} completed in city {building.CityID}, will allocate workers");
                    }
                }
            }

            // Allocate workers for cities with newly completed buildings
            foreach (var cityId in citiesWithCompletedBuildings)
            {
                AllocateWorkersForCity(cityId);
            }
        }

        public void AdvanceSeason() { }
        public void AdvanceYear() { }

        // ========== CONSTRUCTION ==========

        /// <summary>
        /// Construct a new building using BuildingDefinition.
        /// </summary>
        public BuildingInstanceData ConstructBuilding(int cityID, BuildingDefinition definition, Vector3 position)
        {
            // Check if city's culture has unlocked this building
            if (!IsBuildingUnlockedForCity(cityID, definition))
            {
                Debug.LogWarning($"[BuildingManager] {definition.BuildingName} not unlocked for city {cityID}'s culture");
                return null;
            }

            // Use definition's stable ID instead of GetInstanceID()
            // GetInstanceID() can change between sessions, but stable ID is persistent
            int definitionID = definition.GetStableDefinitionID();

            var instanceData = new BuildingInstanceData(
                nextBuildingID++,
                cityID,
                definitionID,
                position,
                definition.ConstructionTimeDays,
                definition.BaseEfficiency
            );

            // Deduct construction costs
            if (DeductConstructionCosts(cityID, definition.BaseBuildCost.ToDictionary()))
            {
                instanceData.HasPaidConstructionCost = true;
                buildings.Add(instanceData);
                buildingLookup[instanceData.ID] = instanceData;

                // Update city index cache
                if (!buildingsByCity.ContainsKey(cityID))
                    buildingsByCity[cityID] = new List<int>();
                buildingsByCity[cityID].Add(instanceData.ID);

                // Register the definition in the lookup if not already registered
                // This allows BuildingViewModel to find the definition later
                if (!definitionLookup.ContainsKey(definitionID))
                {
                    definitionLookup[definitionID] = definition;
                    Debug.Log($"[BuildingManager] Registered definition for {definition.BuildingName} (DefID: {definitionID})");
                }

                Debug.Log($"[BuildingManager] Created {definition.BuildingName} (ID: {instanceData.ID}, DefID: {definitionID})");
                return instanceData;
            }
            else
            {
                Debug.LogWarning($"[BuildingManager] Insufficient resources to build {definition.BuildingName} in city {cityID}");
                return null;
            }
        }

        /// <summary>
        /// Check if a building is unlocked for a city based on the city's culture.
        /// </summary>
        public bool IsBuildingUnlockedForCity(int cityID, BuildingDefinition definition)
        {
            // Get city's dominant culture
            int cityCultureID = CultureManager.Instance.GetCityCulture(cityID);
            if (cityCultureID == -1)
            {
                // City has no dominant culture, allow all buildings for now
                // TODO: Decide default behavior
                return true;
            }

            // Get culture
            var culture = CultureManager.Instance.GetCulture(cityCultureID);
            if (culture == null)
            {
                Debug.LogWarning($"[BuildingManager] Culture {cityCultureID} not found");
                return false;
            }

            // Check if building is unlocked
            return culture.IsBuildingUnlocked(definition);
        }

        private bool DeductConstructionCosts(int cityID, Dictionary<ResourceType, int> costs)
        {
            var resourceManager = ResourceManager.Instance;

            // Check if city has enough resources
            foreach (var cost in costs)
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
            foreach (var cost in costs)
            {
                costLedger.Subtract(cost.Key, cost.Value);
            }

            resourceManager.ApplyLedger(cityID, costLedger);
            Debug.Log($"[BuildingManager] Deducted construction costs");
            return true;
        }

        // ========== CANCELLATION ==========

        public bool CancelConstruction(int buildingID, int cityID)
        {
            if (!buildingLookup.TryGetValue(buildingID, out var building))
            {
                Debug.LogWarning($"[BuildingManager] Building {buildingID} not found for cancellation");
                return false;
            }

            if (building.CityID != cityID || !building.CanBeCancelled)
            {
                Debug.LogWarning($"[BuildingManager] Cannot cancel building {buildingID}");
                return false;
            }

            // Calculate refund
            float refundPercentage = CalculateRefundPercentage(building);

            // Refund costs (need to get BuildingDefinition or BuildingData for costs)
            // For now, skip refund logic - TODO: implement when definitions are loaded

            // Remove building
            buildings.Remove(building);
            buildingLookup.Remove(buildingID);

            // Update city index cache
            if (buildingsByCity.ContainsKey(building.CityID))
            {
                buildingsByCity[building.CityID].Remove(buildingID);
            }

            Debug.Log($"[BuildingManager] Cancelled construction of building ID {buildingID}");
            return true;
        }

        private float CalculateRefundPercentage(BuildingInstanceData building)
        {
            float progressMade = building.ConstructionProgress;
            return Mathf.Lerp(1.0f, 0.5f, progressMade);
        }

        // ========== QUERIES ==========

        public IEnumerable<BuildingInstanceData> GetBuildingsForCity(int cityID)
        {
            // Use cached city index instead of iterating all buildings
            if (!buildingsByCity.TryGetValue(cityID, out var buildingIds))
                yield break;

            foreach (int id in buildingIds)
                yield return buildingLookup[id];
        }

        public IEnumerable<BuildingInstanceData> GetCompletedBuildingsForCity(int cityID)
        {
            // Use cached city index instead of iterating all buildings
            if (!buildingsByCity.TryGetValue(cityID, out var buildingIds))
                yield break;

            foreach (int id in buildingIds)
            {
                var building = buildingLookup[id];
                if (building.IsCompleted)
                    yield return building;
            }
        }

        public IEnumerable<BuildingInstanceData> GetBuildingsUnderConstruction(int cityID)
        {
            // Use cached city index instead of iterating all buildings
            if (!buildingsByCity.TryGetValue(cityID, out var buildingIds))
                yield break;

            foreach (int id in buildingIds)
            {
                var building = buildingLookup[id];
                if (building.IsUnderConstruction)
                    yield return building;
            }
        }

        public BuildingInstanceData GetInstanceData(int buildingID)
        {
            return buildingLookup.GetValueOrDefault(buildingID, null);
        }

        public BuildingDefinition GetDefinition(int definitionID)
        {
            return definitionLookup.GetValueOrDefault(definitionID, null);
        }

        public IEnumerable<BuildingInstanceData> GetAllBuildings()
        {
            foreach (var building in buildings)
            {
                    yield return building;
            }
        }

        // ========== WORKER MANAGEMENT ==========

        /// <summary>
        /// Manually assign workers to a specific building (called from code or UI).
        /// </summary>
        public void AssignWorkerToBuilding(int buildingID, Realms.Demographics.PopulationArchetypes archetype, int count)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.AssignWorker(archetype, count);
                Debug.Log($"[BuildingManager] Assigned {count} {archetype} workers to building {buildingID}");
            }
        }

        /// <summary>
        /// Manually remove workers from a specific building (called from code or UI).
        /// </summary>
        public void RemoveWorkerFromBuilding(int buildingID, Realms.Demographics.PopulationArchetypes archetype, int count)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.RemoveWorker(archetype, count);
                Debug.Log($"[BuildingManager] Removed {count} {archetype} workers from building {buildingID}");
            }
        }

        /// <summary>
        /// Clear all workers from a building.
        /// </summary>
        public void ClearWorkersFromBuilding(int buildingID)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.ClearWorkers();
                Debug.Log($"[BuildingManager] Cleared all workers from building {buildingID}");
            }
        }

        // ========== AUTOMATIC WORKER ALLOCATION ==========

        /// <summary>
        /// Automatically allocate workers to all buildings in a city.
        /// Uses first-come-first-serve based on construction order (building ID).
        /// Call this after: building completion, population changes, or manual reallocation request.
        /// </summary>
        public void AllocateWorkersForCity(int cityId)
        {
            // Get buildings for this city using cached index
            var cityBuildings = new List<BuildingInstanceData>(GetBuildingsForCity(cityId));

            // Call allocation system
            WorkerAllocationSystem.AllocateWorkersForCity(cityId, cityBuildings, definitionLookup);

            Debug.Log($"[BuildingManager] Completed automatic worker allocation for city {cityId}");
        }

        /// <summary>
        /// Deallocate workers when population is lost.
        /// Removes workers in reverse construction order (last built = first to lose workers).
        /// </summary>
        public void DeallocateWorkersForCity(int cityId, int workersLost)
        {
            // Get buildings for this city using cached index
            var cityBuildings = new List<BuildingInstanceData>(GetBuildingsForCity(cityId));

            WorkerAllocationSystem.DeallocateWorkersForCity(cityId, cityBuildings, definitionLookup, workersLost);

            Debug.Log($"[BuildingManager] Deallocated {workersLost} workers from city {cityId}");
        }

        // ========== HUB/HUBLET SYSTEM ==========

        /// <summary>
        /// Attach a hublet to a hub.
        /// Validates hublet slot limits (placeholder for world-based validation).
        /// </summary>
        public bool AttachHubletToHub(int hubletID, int hubID)
        {
            if (!buildingLookup.TryGetValue(hubletID, out var hublet))
            {
                Debug.LogWarning($"[BuildingManager] Hublet {hubletID} not found");
                return false;
            }

            if (!buildingLookup.TryGetValue(hubID, out var hub))
            {
                Debug.LogWarning($"[BuildingManager] Hub {hubID} not found");
                return false;
            }

            // Get definitions to validate
            var hubletDef = GetDefinition(hublet.BuildingDefinitionID);
            var hubDef = GetDefinition(hub.BuildingDefinitionID);

            if (hubletDef == null || hubDef == null)
            {
                Debug.LogWarning($"[BuildingManager] Could not find definitions for hublet or hub");
                return false;
            }

            // Validate this is actually a hublet and hub
            if (!hubletDef.IsHublet)
            {
                Debug.LogWarning($"[BuildingManager] Building {hubletID} is not a hublet");
                return false;
            }

            if (!hubDef.IsHub)
            {
                Debug.LogWarning($"[BuildingManager] Building {hubID} is not a hub");
                return false;
            }

            // NOTE: Hublet slot validation is a placeholder.
            // In the future, this will be replaced with world-based spatial validation.
            // Check if hub has available hublet slots
            if (hub.OccupiedHubletSlots >= hubDef.HubletSlots)
            {
                Debug.LogWarning($"[BuildingManager] Hub {hubID} has no available hublet slots " +
                                $"({hub.OccupiedHubletSlots}/{hubDef.HubletSlots} occupied)");
                return false;
            }

            // Validate hub type compatibility
            if (!BuildingSimulation.CanHubletAttachToHub(hubletDef, hubDef))
            {
                Debug.LogWarning($"[BuildingManager] Hublet {hubletID} cannot attach to hub type {hubDef.BuildingCategory}");
                return false;
            }

            // Perform attachment
            hublet.AttachedToHubID = hubID;
            hub.AttachedHubletIDs.Add(hubletID);

            Debug.Log($"[BuildingManager] Attached hublet {hubletID} to hub {hubID} " +
                     $"({hub.OccupiedHubletSlots}/{hubDef.HubletSlots} slots occupied)");
            return true;
        }

        public void DetachHubletFromHub(int hubletID)
        {
            if (buildingLookup.TryGetValue(hubletID, out var hublet))
            {
                int oldHubID = hublet.AttachedToHubID;
                if (oldHubID >= 0 && buildingLookup.TryGetValue(oldHubID, out var hub))
                {
                    hub.AttachedHubletIDs.Remove(hubletID);
                }
                hublet.AttachedToHubID = -1;
                Debug.Log($"[BuildingManager] Detached hublet {hubletID} from hub {oldHubID}");
            }
        }

        public List<int> GetHubletsForHub(int hubID)
        {
            if (buildingLookup.TryGetValue(hubID, out var hub))
            {
                return new List<int>(hub.AttachedHubletIDs);
            }
            return new List<int>();
        }

        /// <summary>
        /// Check if a hub can accept more hublets.
        /// NOTE: This uses slot-based validation (placeholder for world-based validation).
        /// </summary>
        public bool CanHubAcceptHublet(int hubID)
        {
            if (!buildingLookup.TryGetValue(hubID, out var hub))
                return false;

            var hubDef = GetDefinition(hub.BuildingDefinitionID);
            if (hubDef == null || !hubDef.IsHub)
                return false;

            // NOTE: Placeholder slot-based check - will be replaced with world-based spatial validation
            return hub.OccupiedHubletSlots < hubDef.HubletSlots;
        }

        /// <summary>
        /// Get available hublet slots for a hub.
        /// NOTE: Placeholder for world-based validation.
        /// </summary>
        public int GetAvailableHubletSlots(int hubID)
        {
            if (!buildingLookup.TryGetValue(hubID, out var hub))
                return 0;

            var hubDef = GetDefinition(hub.BuildingDefinitionID);
            if (hubDef == null || !hubDef.IsHub)
                return 0;

            return Mathf.Max(0, hubDef.HubletSlots - hub.OccupiedHubletSlots);
        }

        // ========== DEFINITION LOADING (TODO) ==========

        private void LoadBuildingDefinitions()
        {
            // TODO: Load all BuildingDefinition ScriptableObjects from Resources
            // var definitions = Resources.LoadAll<BuildingDefinition>("Buildings");
            // foreach (var def in definitions)
            // {
            //     definitionLookup[def.GetInstanceID()] = def;
            // }
        }
    }
}
