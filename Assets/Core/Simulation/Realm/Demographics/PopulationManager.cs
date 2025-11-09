using System;
using System.Collections.Generic;
using TWK.Core;
using TWK.Economy;
using TWK.Simulation;
using UnityEngine;

namespace TWK.Realms.Demographics
{
    public class PopulationManager : MonoBehaviour, ISimulationAgent
    {
        public static PopulationManager Instance { get; private set; }

        [SerializeField] private WorldTimeManager worldTimeManager;
        
        // Add this dictionary to track cities
        private Dictionary<int, City> cityLookup = new();

        private List<PopulationGroups> populationGroups = new();
        private Dictionary<int, int> populationIndexById = new();
        private Dictionary<int, List<int>> populationsByCity = new();
        private Dictionary<int, float> accumulatedGrowth = new(); // Add fractional growth tracking
        private int nextID = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(WorldTimeManager timeManager)
        {
            worldTimeManager = timeManager;

            worldTimeManager.OnDayTick += AdvanceDay;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;
        }

        // Add this method to register cities
        public void RegisterCity(City city)
        {
            cityLookup[city.CityID] = city;
        }

        // Add this method to get city data
        private City GetCity(int cityId)
        {
            return cityLookup.TryGetValue(cityId, out var city) ? city : null;
        }

        #region Simulation Ticks
        public void AdvanceDay()
        {
            for (int i = 0; i < populationGroups.Count; i++)
            {
                var pop = populationGroups[i];

                // Daily growth calculation (placeholder)
                pop.populationCount += CalculateDailyGrowth(pop);
                pop.populationHappiness = Mathf.Clamp01(pop.populationHappiness - 0.0005f);

                populationGroups[i] = pop;
            }
        }

        public void AdvanceSeason()
        {
            // TODO: seasonal modifiers (food, festivals, events)
        }

        public void AdvanceYear()
        {
            // TODO: yearly updates (census, aging, policies)
        }
        #endregion

        #region Population Management
        public int RegisterPopulation(int cityId, PopulationArchetypes archetype, int count)
        {
            var pop = new PopulationGroups(nextID++, cityId, archetype, count);
            populationGroups.Add(pop);
            populationIndexById[pop.id] = populationGroups.Count - 1;

            if (!populationsByCity.ContainsKey(cityId))
                populationsByCity[cityId] = new List<int>();
            populationsByCity[cityId].Add(pop.id);

            return pop.id;
        }

        public void ModifyPopulation(int id, Func<PopulationGroups, PopulationGroups> modify)
        {
            if (populationIndexById.TryGetValue(id, out int index))
            {
                populationGroups[index] = modify(populationGroups[index]);
                return;
            }

            throw new Exception($"Population with ID {id} not found!");
        }

        public PopulationGroups GetPopulationData(int id)
        {
            if (populationIndexById.TryGetValue(id, out int index))
                return populationGroups[index];
            throw new Exception($"Population with ID {id} not found!");
        }

        public IEnumerable<PopulationGroups> GetPopulationsByCity(int cityId)
        {
            if (!populationsByCity.TryGetValue(cityId, out var popIds))
                yield break;

            foreach (int id in popIds)
                yield return populationGroups[populationIndexById[id]];
        }

        public int GetIntPopulationByCity(int cityId)
        {
            int total = 0;
            foreach (var pop in populationGroups)
                if (pop.ownerCityId == cityId)
                    total += pop.populationCount;
            return total;
        }
        #endregion

        #region Helpers
        private int CalculateDailyGrowth(PopulationGroups pop)
        {
                // Get the city this population belongs to
            var city = GetCity(pop.ownerCityId);
            if (city == null)
            {
                Debug.LogWarning($"City {pop.ownerCityId} not found for population {pop.id}");
                return 0;
            }

            // Get city context for resource availability
            var cityFood = TWK.Economy.ResourceManager.Instance.GetResource(pop.ownerCityId, ResourceType.Food);
            var totalCityPop = GetIntPopulationByCity(pop.ownerCityId);
            
            // Calculate growth components
            float baseGrowthRate = GetArchetypeBaseGrowthRate(pop.archetype);
            float cityGrowthModifier = city.GrowthRate; // Use the city's growth rate
            float carryingCapacityFactor = CalculateCarryingCapacityFactor(totalCityPop, cityFood);
            float happinessFactor = CalculateHappinessFactor(pop.populationHappiness);
            float resourceFactor = CalculateResourceFactor(cityFood, totalCityPop);
            
            // Combined growth rate (now includes city growth rate)
            float effectiveGrowthRate = baseGrowthRate * cityGrowthModifier * carryingCapacityFactor * happinessFactor * resourceFactor * pop.growthModifier;
            
            // Calculate exact growth
            float exactGrowth = pop.populationCount * effectiveGrowthRate;
            
            // Accumulate fractional growth
            if (!accumulatedGrowth.ContainsKey(pop.id))
                accumulatedGrowth[pop.id] = 0f;
            
            accumulatedGrowth[pop.id] += exactGrowth;
            
            // Extract whole number growth
            int growthToApply = Mathf.FloorToInt(accumulatedGrowth[pop.id]);
            accumulatedGrowth[pop.id] -= growthToApply;
            
            return growthToApply;
        }

        private float GetArchetypeBaseGrowthRate(PopulationArchetypes archetype)
        {
            return archetype switch
            {
                PopulationArchetypes.Laborer => 0.0008f,
                PopulationArchetypes.Artisan => 0.0006f,
                PopulationArchetypes.Merchant => 0.0007f,
                PopulationArchetypes.Noble => 0.0003f,
                PopulationArchetypes.Clergy => 0.0002f,
                PopulationArchetypes.Slave => 0.0004f,
                _ => 0.0006f
            };
        }

        private float CalculateCarryingCapacityFactor(int totalPopulation, int cityFood)
        {
            float carryingCapacity = cityFood * 2f;
            if (carryingCapacity <= 0) return 0.1f;
            
            float populationRatio = totalPopulation / carryingCapacity;
            return Mathf.Max(0.1f, 1f - populationRatio);
        }

        private float CalculateHappinessFactor(float happiness)
        {
            return Mathf.Pow(happiness, 2f) * 2f;
        }

        private float CalculateResourceFactor(int cityFood, int totalPopulation)
        {
            if (totalPopulation == 0) return 1f;
            
            float foodPerPerson = (float)cityFood / totalPopulation;
            
            if (foodPerPerson < 0.5f)      return 0.2f;
            else if (foodPerPerson < 1f)   return 0.6f;
            else if (foodPerPerson < 2f)   return 1f;
            else if (foodPerPerson < 4f)   return 1.3f;
            else                           return 1.5f;
        }
        #endregion
    }
}
