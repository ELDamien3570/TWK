using System.Collections.Generic;
using TWK.Core;
using TWK.Simulation;
using UnityEngine;

namespace TWK.Economy
{
    public class ResourceManager : MonoBehaviour, ISimulationAgent
    {
        public static ResourceManager Instance { get; private set; }

        private readonly Dictionary<int, Dictionary<ResourceType, int>> cityResources = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(WorldTimeManager worldTimeManager) { }

        public void RegisterCity(int cityId)
        {
            if (cityResources.ContainsKey(cityId)) return;

            cityResources[cityId] = new Dictionary<ResourceType, int>
            {
                { ResourceType.Food, 1000 },
                { ResourceType.Gold, 1000 }
            };
        }

        public void ApplyLedger(int cityId, ResourceLedger ledger)
        {
            if (!cityResources.TryGetValue(cityId, out var resources))
                throw new System.Exception($"City {cityId} not registered.");

            foreach (var kvp in ledger.DailyChange)
            {
                if (!resources.ContainsKey(kvp.Key))
                    resources[kvp.Key] = 0;

                resources[kvp.Key] += kvp.Value;
            }
            
           //Debug.Log($"[ResourceManager] Applied ledger for City {cityId}.");
            GetCityResources(cityId);
        }

        public int GetResource(int cityId, ResourceType type)
        {
            return cityResources.TryGetValue(cityId, out var dict) && dict.TryGetValue(type, out var val)
                ? val : 0;
        }

        private void GetCityResources(int cityID)
        {
            if (cityResources.TryGetValue(cityID, out var resources))
            {
                foreach (var kvp in resources)
                {
                    Debug.Log($"[ResourceManager] City {cityID} - {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                Debug.LogWarning($"[ResourceManager] City {cityID} not found.");
            }        
            
        }
    }
}
