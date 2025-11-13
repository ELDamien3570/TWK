using System.Collections.Generic;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms;

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

        // Definition lookup (BuildingData.name -> BuildingDefinition)
        // In real implementation, this would be populated from ScriptableObjects
        private Dictionary<string, BuildingDefinition> definitionLookup = new Dictionary<string, BuildingDefinition>();

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
            // Process construction for all buildings
            foreach (var building in buildings)
            {
                if (building.IsUnderConstruction)
                {
                    BuildingSimulation.AdvanceConstruction(building);
                }
            }
        }

        public void AdvanceSeason() { }
        public void AdvanceYear() { }

        // ========== CONSTRUCTION ==========

        /// <summary>
        /// Construct a new building (backward compatible with old BuildingData).
        /// </summary>
        public BuildingInstance ConstructBuilding(int cityID, BuildingData data, Vector3 position)
        {
            // Create instance data
            var instanceData = new BuildingInstanceData(
                nextBuildingID++,
                cityID,
                data.GetInstanceID(), // Use BuildingData's instance ID as definition ID for now
                position,
                data.ConstructionTimeDays,
                data.BaseEfficiency
            );

            // Deduct construction costs
            if (DeductConstructionCosts(cityID, data.BaseBuildCost))
            {
                instanceData.HasPaidConstructionCost = true;
                buildings.Add(instanceData);
                buildingLookup[instanceData.ID] = instanceData;

                if (instanceData.IsUnderConstruction)
                {
                    Debug.Log($"[BuildingManager] Started construction of {data.BuildingName} - {data.ConstructionTimeDays} days remaining. Costs deducted immediately.");
                }
                else
                {
                    Debug.Log($"[BuildingManager] {data.BuildingName} completed immediately. Costs deducted.");
                }

                // Return backward-compatible BuildingInstance struct
                return ConvertToLegacyInstance(instanceData, data);
            }
            else
            {
                Debug.LogWarning($"[BuildingManager] Insufficient resources to build {data.BuildingName} in city {cityID}");
                return default;
            }
        }

        /// <summary>
        /// Construct a new building using BuildingDefinition (new way).
        /// </summary>
        public BuildingInstanceData ConstructBuildingNew(int cityID, BuildingDefinition definition, Vector3 position)
        {
            var instanceData = new BuildingInstanceData(
                nextBuildingID++,
                cityID,
                definition.GetInstanceID(),
                position,
                definition.ConstructionTimeDays,
                definition.BaseEfficiency
            );

            // Deduct construction costs
            if (DeductConstructionCosts(cityID, definition.BaseBuildCost))
            {
                instanceData.HasPaidConstructionCost = true;
                buildings.Add(instanceData);
                buildingLookup[instanceData.ID] = instanceData;

                Debug.Log($"[BuildingManager] Created {definition.BuildingName} (ID: {instanceData.ID})");
                return instanceData;
            }
            else
            {
                Debug.LogWarning($"[BuildingManager] Insufficient resources to build {definition.BuildingName} in city {cityID}");
                return null;
            }
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

            Debug.Log($"[BuildingManager] Cancelled construction of building ID {buildingID}");
            return true;
        }

        private float CalculateRefundPercentage(BuildingInstanceData building)
        {
            float progressMade = building.ConstructionProgress;
            return Mathf.Lerp(1.0f, 0.5f, progressMade);
        }

        // ========== QUERIES ==========

        public IEnumerable<BuildingInstance> GetBuildingsForCity(int cityID)
        {
            // TODO: This requires BuildingData lookup
            // For now, return empty - needs refactor when BuildingDefinitions are loaded
            yield break;
        }

        public IEnumerable<BuildingInstanceData> GetBuildingsForCityNew(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID)
                    yield return b;
        }

        public IEnumerable<BuildingInstance> GetCompletedBuildingsForCity(int cityID)
        {
            // TODO: Backward compatibility wrapper
            yield break;
        }

        public IEnumerable<BuildingInstanceData> GetCompletedBuildingsForCityNew(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID && b.IsCompleted)
                    yield return b;
        }

        public IEnumerable<BuildingInstance> GetBuildingsUnderConstruction(int cityID)
        {
            // TODO: Backward compatibility wrapper
            yield break;
        }

        public IEnumerable<BuildingInstanceData> GetBuildingsUnderConstructionNew(int cityID)
        {
            foreach (var b in buildings)
                if (b.CityID == cityID && b.IsUnderConstruction)
                    yield return b;
        }

        public BuildingInstance? GetBuildingByID(int buildingID)
        {
            // TODO: Backward compatibility wrapper - needs BuildingData
            return null;
        }

        public BuildingInstanceData GetInstanceData(int buildingID)
        {
            return buildingLookup.GetValueOrDefault(buildingID, null);
        }

        public BuildingDefinition GetDefinition(int definitionID)
        {
            // TODO: Implement when definitions are loaded
            return null;
        }

        // ========== WORKER MANAGEMENT ==========

        public void AssignWorkerToBuilding(int buildingID, Realms.Demographics.PopulationArchetypes archetype, int count)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.AssignWorker(archetype, count);
                Debug.Log($"[BuildingManager] Assigned {count} {archetype} workers to building {buildingID}");
            }
        }

        public void RemoveWorkerFromBuilding(int buildingID, Realms.Demographics.PopulationArchetypes archetype, int count)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.RemoveWorker(archetype, count);
                Debug.Log($"[BuildingManager] Removed {count} {archetype} workers from building {buildingID}");
            }
        }

        public void ClearWorkersFromBuilding(int buildingID)
        {
            if (buildingLookup.TryGetValue(buildingID, out var building))
            {
                building.ClearWorkers();
                Debug.Log($"[BuildingManager] Cleared all workers from building {buildingID}");
            }
        }

        // ========== HUB/HUBLET SYSTEM ==========

        public void AttachHubletToHub(int hubletID, int hubID)
        {
            if (buildingLookup.TryGetValue(hubletID, out var hublet) &&
                buildingLookup.TryGetValue(hubID, out var hub))
            {
                hublet.AttachedToHubID = hubID;
                hub.AttachedHubletIDs.Add(hubletID);
                Debug.Log($"[BuildingManager] Attached hublet {hubletID} to hub {hubID}");
            }
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

        // ========== BACKWARD COMPATIBILITY ==========

        private BuildingInstance ConvertToLegacyInstance(BuildingInstanceData data, BuildingData buildingData)
        {
            // Convert new data format to old struct format
            return new BuildingInstance(data.ID, data.CityID, buildingData, data.Position);
        }

        // ========== DEFINITION LOADING (TODO) ==========

        private void LoadBuildingDefinitions()
        {
            // TODO: Load all BuildingDefinition ScriptableObjects from Resources
            // var definitions = Resources.LoadAll<BuildingDefinition>("Buildings");
            // foreach (var def in definitions)
            // {
            //     definitionLookup[def.BuildingName] = def;
            // }
        }
    }
}
