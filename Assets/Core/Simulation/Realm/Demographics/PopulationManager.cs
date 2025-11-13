using System;
using System.Collections.Generic;
using TWK.Core;
using TWK.Cultures;
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

        private List<PopulationGroup> populationGroups = new();
        private Dictionary<int, int> populationIndexById = new();
        private Dictionary<int, List<int>> populationsByCity = new();
        private Dictionary<int, float> accumulatedGrowth = new(); // Add fractional growth tracking
        private int nextID = 0;

        // Archetype definitions for promotion/demotion rules (will be loaded from ScriptableObjects)
        private Dictionary<PopulationArchetypes, ArchetypeDefinition> archetypeDefinitions = new();

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

        /// <summary>
        /// Get city by ID (public accessor for managers).
        /// </summary>
        public City GetCityByID(int cityId)
        {
            return cityLookup.TryGetValue(cityId, out var city) ? city : null;
        }

        /// <summary>
        /// Get all registered cities.
        /// </summary>
        public List<City> GetAllCities()
        {
            return new List<City>(cityLookup.Values);
        }

        #region Simulation Ticks
        public void AdvanceDay()
        {
            foreach (var pop in populationGroups)
            {
                // Calculate and apply daily growth
                int growth = CalculateDailyGrowth(pop);
                if (growth != 0)
                {
                    // Apply growth proportionally to demographics
                    float growthMultiplier = 1f + (growth / (float)pop.PopulationCount);
                    pop.ApplyGrowth(growthMultiplier);
                }

                // Happiness naturally decays slightly
                pop.Happiness = Mathf.Clamp01(pop.Happiness - 0.0005f);

                // Education grows slowly from baseline (buildings and policies will add more)
                float educationGrowth = 0.001f; // Base education growth per day
                pop.Education = Mathf.Clamp(pop.Education + educationGrowth, 0f, 100f);

                // Wealth grows from employment (to be enhanced with building income)
                if (pop.EmployedCount > 0)
                {
                    float wealthGrowth = 0.01f; // Base wealth growth for employed
                    pop.Wealth = Mathf.Clamp(pop.Wealth + wealthGrowth, 0f, 100f);
                }
            }
        }

        public void AdvanceSeason()
        {
            // TODO: seasonal modifiers (food, festivals, events)
        }

        public void AdvanceYear()
        {
            foreach (var pop in populationGroups)
            {
                // Age the population demographics
                pop.AdvanceYear();

                // Check for promotion/demotion opportunities
                CheckSocialMobility(pop);
            }
        }
        #endregion

        #region Population Management
        public int RegisterPopulation(int cityId, PopulationArchetypes archetype, int count,  CultureData inCulture, float averageAge = 30f)
        {
            var pop = new PopulationGroup(nextID++, cityId, archetype, count, inCulture, averageAge);
            populationGroups.Add(pop);
            populationIndexById[pop.ID] = populationGroups.Count - 1;

            if (!populationsByCity.ContainsKey(cityId))
                populationsByCity[cityId] = new List<int>();
            populationsByCity[cityId].Add(pop.ID);

            return pop.ID;
        }

        public PopulationGroup GetPopulationData(int id)
        {
            if (populationIndexById.TryGetValue(id, out int index))
                return populationGroups[index];
            throw new Exception($"Population with ID {id} not found!");
        }

        public IEnumerable<PopulationGroup> GetPopulationsByCity(int cityId)
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
                if (pop.OwnerCityID == cityId)
                    total += pop.PopulationCount;
            return total;
        }

        /// <summary>
        /// Get total available workers in a city (all working-age population)
        /// </summary>
        public int GetAvailableWorkersByCity(int cityId)
        {
            int total = 0;
            foreach (var pop in GetPopulationsByCity(cityId))
                total += pop.AvailableWorkers;
            return total;
        }

        /// <summary>
        /// Get military eligible population in a city
        /// </summary>
        public int GetMilitaryEligibleByCity(int cityId)
        {
            int total = 0;
            foreach (var pop in GetPopulationsByCity(cityId))
                total += pop.MilitaryEligible;
            return total;
        }
        #endregion

        #region Social Mobility
        /// <summary>
        /// Check if a population group should promote or demote based on education, wealth, and chance
        /// </summary>
        private void CheckSocialMobility(PopulationGroup pop)
        {
            // Check for promotion
            if (CanPromote(pop))
            {
                float promotionChance = CalculatePromotionChance(pop);
                if (UnityEngine.Random.value < promotionChance)
                {
                    PromotePopulation(pop);
                }
            }

            // Check for demotion
            if (CanDemote(pop))
            {
                float demotionChance = CalculateDemotionChance(pop);
                if (UnityEngine.Random.value < demotionChance)
                {
                    DemotePopulation(pop);
                }
            }
        }

        private bool CanPromote(PopulationGroup pop)
        {
            // Define promotion paths
            switch (pop.Archetype)
            {
                case PopulationArchetypes.Slave:
                    return false; // Slaves cannot promote without policy (TODO: add emancipation policy)
                case PopulationArchetypes.Laborer:
                    return pop.Education >= 40f && pop.Wealth >= 30f;
                case PopulationArchetypes.Artisan:
                    return pop.Education >= 60f && pop.Wealth >= 50f;
                case PopulationArchetypes.Merchant:
                    return pop.Education >= 80f && pop.Wealth >= 75f;
                case PopulationArchetypes.Noble:
                    return false; // Nobles are top tier
                case PopulationArchetypes.Clergy:
                    return false; // Clergy is special branch
                default:
                    return false;
            }
        }

        private bool CanDemote(PopulationGroup pop)
        {
            // Demotion occurs when wealth falls too low
            switch (pop.Archetype)
            {
                case PopulationArchetypes.Slave:
                    return false; // Can't go lower
                case PopulationArchetypes.Laborer:
                    // Laborers can only become slaves via policy (default 0% chance)
                    return false; // TODO: add enslavement policy
                case PopulationArchetypes.Artisan:
                case PopulationArchetypes.Merchant:
                case PopulationArchetypes.Noble:
                    return pop.Wealth < 20f; // Poverty threshold
                case PopulationArchetypes.Clergy:
                    return pop.Wealth < 15f || pop.Fervor < 30f; // Loss of faith or poverty
                default:
                    return false;
            }
        }

        private float CalculatePromotionChance(PopulationGroup pop)
        {
            // Base 5% chance per year if requirements are met
            float baseChance = 0.05f;

            // Education bonus (higher education = faster promotion)
            float educationBonus = (pop.Education - 50f) / 100f; // 0 to +0.5

            // Wealth bonus
            float wealthBonus = (pop.Wealth - 50f) / 100f; // 0 to +0.5

            // Happiness bonus
            float happinessBonus = (pop.Happiness - 0.5f) * 0.1f; // -0.05 to +0.05

            return Mathf.Clamp01(baseChance + educationBonus + wealthBonus + happinessBonus);
        }

        private float CalculateDemotionChance(PopulationGroup pop)
        {
            // Base 2% chance per year if in poverty
            float baseChance = 0.02f;

            // Wealth penalty (lower wealth = higher demotion chance)
            float wealthPenalty = (20f - pop.Wealth) / 50f; // 0 to +0.4

            // Unhappiness penalty
            float unhappinessPenalty = (0.5f - pop.Happiness) * 0.2f; // 0 to +0.1

            return Mathf.Clamp01(baseChance + wealthPenalty + unhappinessPenalty);
        }

        private void PromotePopulation(PopulationGroup pop)
        {
            PopulationArchetypes newArchetype = pop.Archetype switch
            {
                PopulationArchetypes.Laborer => PopulationArchetypes.Artisan,
                PopulationArchetypes.Artisan => PopulationArchetypes.Merchant,
                PopulationArchetypes.Merchant => PopulationArchetypes.Noble,
                _ => pop.Archetype
            };

            if (newArchetype != pop.Archetype)
            {
                // Calculate how many promote (10-30% of eligible population)
                int eligibleCount = Mathf.RoundToInt(pop.PopulationCount * 0.2f);
                eligibleCount = Mathf.Max(1, eligibleCount); // At least 1 person

                // Split the population
                if (eligibleCount < pop.PopulationCount)
                {
                    SplitPopulation(pop, eligibleCount, newArchetype);
                }
                else
                {
                    // Entire group promotes
                    pop.Archetype = newArchetype;
                    Debug.Log($"Population group {pop.ID} fully promoted from {pop.Archetype} to {newArchetype}");
                }
            }
        }

        private void DemotePopulation(PopulationGroup pop)
        {
            PopulationArchetypes newArchetype = pop.Archetype switch
            {
                PopulationArchetypes.Noble => PopulationArchetypes.Merchant,
                PopulationArchetypes.Merchant => PopulationArchetypes.Artisan,
                PopulationArchetypes.Artisan => PopulationArchetypes.Laborer,
                PopulationArchetypes.Clergy => PopulationArchetypes.Laborer,
                _ => pop.Archetype
            };

            if (newArchetype != pop.Archetype)
            {
                // Calculate how many demote (10-20% of struggling population)
                int strugglingCount = Mathf.RoundToInt(pop.PopulationCount * 0.15f);
                strugglingCount = Mathf.Max(1, strugglingCount);

                // Split the population
                if (strugglingCount < pop.PopulationCount)
                {
                    SplitPopulation(pop, strugglingCount, newArchetype);
                }
                else
                {
                    // Entire group demotes
                    pop.Archetype = newArchetype;
                    Debug.Log($"Population group {pop.ID} fully demoted from {pop.Archetype} to {newArchetype}");
                }
            }
        }

        /// <summary>
        /// Split a population group, moving some to a different archetype
        /// </summary>
        private void SplitPopulation(PopulationGroup sourcePop, int countToSplit, PopulationArchetypes newArchetype)
        {
            if (countToSplit >= sourcePop.PopulationCount)
            {
                // Just change the archetype if splitting the entire population
                sourcePop.Archetype = newArchetype;
                return;
            }

            // Check if there's already a group of this archetype in the city
            PopulationGroup targetGroup = null;
            foreach (var pop in GetPopulationsByCity(sourcePop.OwnerCityID))
            {
                if (pop.Archetype == newArchetype)
                {
                    targetGroup = pop;
                    break;
                }
            }

            // Create new group if needed
            if (targetGroup == null)
            {
                int newId = RegisterPopulation(sourcePop.OwnerCityID, newArchetype, 0, sourcePop.Culture, sourcePop.AverageAge);
                targetGroup = GetPopulationData(newId);

                // Initialize with source properties
                targetGroup.Wealth = sourcePop.Wealth;
                targetGroup.Education = sourcePop.Education;
                targetGroup.Fervor = sourcePop.Fervor;
                targetGroup.CurrentReligion = sourcePop.CurrentReligion;
                targetGroup.Happiness = sourcePop.Happiness;
            }

            // Transfer population proportionally from demographics
            float transferRatio = countToSplit / (float)sourcePop.PopulationCount;

            // Scale down source
            sourcePop.Demographics.Scale(1f - transferRatio);

            // Scale up or create target demographics
            var transferDemographics = AgeDistribution.CreateNormalized(countToSplit, sourcePop.AverageAge);
            targetGroup.Demographics.YoungMales += transferDemographics.YoungMales;
            targetGroup.Demographics.YoungFemales += transferDemographics.YoungFemales;
            targetGroup.Demographics.AdultMales += transferDemographics.AdultMales;
            targetGroup.Demographics.AdultFemales += transferDemographics.AdultFemales;
            targetGroup.Demographics.MiddleMales += transferDemographics.MiddleMales;
            targetGroup.Demographics.MiddleFemales += transferDemographics.MiddleFemales;
            targetGroup.Demographics.ElderlyMales += transferDemographics.ElderlyMales;
            targetGroup.Demographics.ElderlyFemales += transferDemographics.ElderlyFemales;

            Debug.Log($"Split {countToSplit} people from {sourcePop.Archetype} to {newArchetype}");
        }

        /// <summary>
        /// Get all population groups across all cities.
        /// </summary>
        public List<PopulationGroup> GetAllPopulationGroups()
        {
            var allGroups = new List<PopulationGroup>();

            foreach (var city in GetAllCities())
            {
                var cityGroups = GetPopulationsByCity(city.CityID);
                allGroups.AddRange(cityGroups);
            }

            return allGroups;
        }
        #endregion

        #region Helpers
        private int CalculateDailyGrowth(PopulationGroup pop)
        {
                // Get the city this population belongs to
            var city = GetCity(pop.OwnerCityID);
            if (city == null)
            {
                Debug.LogWarning($"City {pop.OwnerCityID} not found for population {pop.ID}");
                return 0;
            }

            // Get city context for resource availability
            var cityFood = TWK.Economy.ResourceManager.Instance.GetResource(pop.OwnerCityID, ResourceType.Food);
            var totalCityPop = GetIntPopulationByCity(pop.OwnerCityID);

            // Calculate growth components
            float baseGrowthRate = GetArchetypeBaseGrowthRate(pop.Archetype);
            float cityGrowthModifier = city.GrowthRate; // Use the city's growth rate
            float carryingCapacityFactor = CalculateCarryingCapacityFactor(totalCityPop, cityFood);
            float happinessFactor = CalculateHappinessFactor(pop.Happiness);
            float resourceFactor = CalculateResourceFactor(cityFood, totalCityPop);

            // Combined growth rate (now includes city growth rate)
            float effectiveGrowthRate = baseGrowthRate * cityGrowthModifier * carryingCapacityFactor * happinessFactor * resourceFactor * pop.GrowthModifier;

            // Calculate exact growth
            float exactGrowth = pop.PopulationCount * effectiveGrowthRate;

            // Accumulate fractional growth
            if (!accumulatedGrowth.ContainsKey(pop.ID))
                accumulatedGrowth[pop.ID] = 0f;

            accumulatedGrowth[pop.ID] += exactGrowth;

            // Extract whole number growth
            int growthToApply = Mathf.FloorToInt(accumulatedGrowth[pop.ID]);
            accumulatedGrowth[pop.ID] -= growthToApply;

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
