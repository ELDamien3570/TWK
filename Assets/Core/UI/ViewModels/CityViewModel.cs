using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Realms;
using TWK.Realms.Demographics;
using TWK.Economy;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying city data with aggregated population statistics.
    /// </summary>
    public class CityViewModel : BaseViewModel
    {
        private City _source;

        // ========== CITY INFO ==========
        public string CityName { get; private set; }
        public int CityID { get; private set; }
        public float GrowthRate { get; private set; }
        public Vector3 Location { get; private set; }

        // ========== POPULATION SUMMARY ==========
        public int TotalPopulation { get; private set; }
        public int TotalAvailableWorkers { get; private set; }
        public int TotalEmployed { get; private set; }
        public int TotalUnemployed { get; private set; }
        public float UnemploymentRate { get; private set; }
        public string UnemploymentRateFormatted { get; private set; }

        public int TotalMilitaryEligible { get; private set; }
        public float MilitaryEligiblePercentage { get; private set; }

        // ========== POPULATION BY ARCHETYPE ==========
        public Dictionary<PopulationArchetypes, int> PopulationByArchetype { get; private set; }
        public int SlaveCount { get; private set; }
        public int LaborerCount { get; private set; }
        public int ArtisanCount { get; private set; }
        public int MerchantCount { get; private set; }
        public int NobleCount { get; private set; }
        public int ClergyCount { get; private set; }

        // ========== DEMOGRAPHICS ==========
        public int TotalYouth { get; private set; }
        public int TotalAdults { get; private set; }
        public int TotalMiddleAge { get; private set; }
        public int TotalElderly { get; private set; }

        public int TotalMales { get; private set; }
        public int TotalFemales { get; private set; }
        public float MalePercentage { get; private set; }
        public float FemalePercentage { get; private set; }

        public float AverageAge { get; private set; }

        // ========== AGGREGATE STATS ==========
        public float AverageWealth { get; private set; }
        public float AverageEducation { get; private set; }
        public float AverageFervor { get; private set; }
        public float AverageLoyalty { get; private set; }
        public float AverageHappiness { get; private set; }

        // ========== ECONOMY ==========
        public Dictionary<ResourceType, int> CurrentResources { get; private set; }
        public int FoodStockpile { get; private set; }
        public int GoldStockpile { get; private set; }
        public int DailyFoodProduction { get; private set; }
        public int DailyFoodConsumption { get; private set; }
        public int DailyFoodNet { get; private set; }
        public int BuildingCount { get; private set; }

        // ========== POPULATION GROUPS ==========
        public List<PopulationGroupViewModel> PopulationGroups { get; private set; }

        // ========== CONSTRUCTOR ==========
        public CityViewModel(City source)
        {
            _source = source;
            PopulationGroups = new List<PopulationGroupViewModel>();
            PopulationByArchetype = new Dictionary<PopulationArchetypes, int>();
            CurrentResources = new Dictionary<ResourceType, int>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_source == null) return;

            // City info
            CityName = _source.Name;
            CityID = _source.CityID;
            GrowthRate = _source.GrowthRate;
            Location = _source.Location;

            // Refresh population groups
            RefreshPopulationGroups();

            // Calculate aggregates
            CalculatePopulationSummary();
            CalculateDemographics();
            CalculateAggregateStats();

            // Economy
            RefreshEconomy();

            NotifyPropertyChanged();
        }

        private void RefreshPopulationGroups()
        {
            PopulationGroups.Clear();

            foreach (var popGroup in PopulationManager.Instance.GetPopulationsByCity(CityID))
            {
                var viewModel = new PopulationGroupViewModel(popGroup);
                PopulationGroups.Add(viewModel);
            }
        }

        private void CalculatePopulationSummary()
        {
            TotalPopulation = 0;
            TotalAvailableWorkers = 0;
            TotalEmployed = 0;
            TotalUnemployed = 0;
            TotalMilitaryEligible = 0;

            PopulationByArchetype.Clear();
            SlaveCount = 0;
            LaborerCount = 0;
            ArtisanCount = 0;
            MerchantCount = 0;
            NobleCount = 0;
            ClergyCount = 0;

            foreach (var popVM in PopulationGroups)
            {
                TotalPopulation += popVM.TotalPopulation;
                TotalAvailableWorkers += popVM.AvailableWorkers;
                TotalEmployed += popVM.EmployedCount;
                TotalUnemployed += popVM.UnemployedCount;
                TotalMilitaryEligible += popVM.MilitaryEligible;

                // Count by archetype
                if (!PopulationByArchetype.ContainsKey(popVM.Archetype))
                    PopulationByArchetype[popVM.Archetype] = 0;
                PopulationByArchetype[popVM.Archetype] += popVM.TotalPopulation;

                // Individual counts for easy access
                switch (popVM.Archetype)
                {
                    case PopulationArchetypes.Slave: SlaveCount += popVM.TotalPopulation; break;
                    case PopulationArchetypes.Laborer: LaborerCount += popVM.TotalPopulation; break;
                    case PopulationArchetypes.Artisan: ArtisanCount += popVM.TotalPopulation; break;
                    case PopulationArchetypes.Merchant: MerchantCount += popVM.TotalPopulation; break;
                    case PopulationArchetypes.Noble: NobleCount += popVM.TotalPopulation; break;
                    case PopulationArchetypes.Clergy: ClergyCount += popVM.TotalPopulation; break;
                }
            }

            UnemploymentRate = TotalAvailableWorkers > 0 ? (TotalUnemployed / (float)TotalAvailableWorkers) * 100f : 0f;
            UnemploymentRateFormatted = $"{UnemploymentRate:F1}%";

            MilitaryEligiblePercentage = TotalPopulation > 0 ? (TotalMilitaryEligible / (float)TotalPopulation) * 100f : 0f;
        }

        private void CalculateDemographics()
        {
            TotalYouth = 0;
            TotalAdults = 0;
            TotalMiddleAge = 0;
            TotalElderly = 0;
            TotalMales = 0;
            TotalFemales = 0;

            float totalAge = 0f;
            int ageCount = 0;

            foreach (var popVM in PopulationGroups)
            {
                TotalYouth += popVM.TotalYouth;
                TotalAdults += popVM.TotalAdults;
                TotalMiddleAge += popVM.TotalMiddleAge;
                TotalElderly += popVM.TotalElderly;

                TotalMales += popVM.YoungMales + popVM.AdultMales + popVM.MiddleMales + popVM.ElderlyMales;
                TotalFemales += popVM.YoungFemales + popVM.AdultFemales + popVM.MiddleFemales + popVM.ElderlyFemales;

                totalAge += popVM.AverageAge * popVM.TotalPopulation;
                ageCount += popVM.TotalPopulation;
            }

            MalePercentage = TotalPopulation > 0 ? (TotalMales / (float)TotalPopulation) * 100f : 50f;
            FemalePercentage = 100f - MalePercentage;

            AverageAge = ageCount > 0 ? totalAge / ageCount : 30f;
        }

        private void CalculateAggregateStats()
        {
            float totalWealth = 0f;
            float totalEducation = 0f;
            float totalFervor = 0f;
            float totalLoyalty = 0f;
            float totalHappiness = 0f;

            foreach (var popVM in PopulationGroups)
            {
                int weight = popVM.TotalPopulation;
                totalWealth += popVM.Wealth * weight;
                totalEducation += popVM.Education * weight;
                totalFervor += popVM.Fervor * weight;
                totalLoyalty += popVM.Loyalty * weight;
                totalHappiness += popVM.Happiness * weight;
            }

            if (TotalPopulation > 0)
            {
                AverageWealth = totalWealth / TotalPopulation;
                AverageEducation = totalEducation / TotalPopulation;
                AverageFervor = totalFervor / TotalPopulation;
                AverageLoyalty = totalLoyalty / TotalPopulation;
                AverageHappiness = totalHappiness / TotalPopulation;
            }
            else
            {
                AverageWealth = 0f;
                AverageEducation = 0f;
                AverageFervor = 50f;
                AverageLoyalty = 0f;
                AverageHappiness = 0.5f;
            }
        }

        private void RefreshEconomy()
        {
            CurrentResources.Clear();

            // Get current resources
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int amount = ResourceManager.Instance.GetResource(CityID, resourceType);
                CurrentResources[resourceType] = amount;
            }

            FoodStockpile = CurrentResources.GetValueOrDefault(ResourceType.Food, 0);
            GoldStockpile = CurrentResources.GetValueOrDefault(ResourceType.Gold, 0);

            // Get daily economy snapshot
            var snapshot = _source.EconomySnapshot;
            DailyFoodProduction = snapshot.Production.GetValueOrDefault(ResourceType.Food, 0);
            DailyFoodConsumption = snapshot.Consumption.GetValueOrDefault(ResourceType.Food, 0);
            DailyFoodNet = snapshot.Net.GetValueOrDefault(ResourceType.Food, 0);

            BuildingCount = _source.Buildings.Count;
        }

        // ========== HELPER METHODS ==========
        public string GetPopulationBreakdownSummary()
        {
            return $"Slaves: {SlaveCount} | Laborers: {LaborerCount} | Artisans: {ArtisanCount} | " +
                   $"Merchants: {MerchantCount} | Nobles: {NobleCount} | Clergy: {ClergyCount}";
        }

        public string GetDemographicSummary()
        {
            return $"Youth: {TotalYouth} ({GetPercentage(TotalYouth)}%) | " +
                   $"Adults: {TotalAdults} ({GetPercentage(TotalAdults)}%) | " +
                   $"Middle: {TotalMiddleAge} ({GetPercentage(TotalMiddleAge)}%) | " +
                   $"Elderly: {TotalElderly} ({GetPercentage(TotalElderly)}%)";
        }

        public string GetGenderSummary()
        {
            return $"Males: {TotalMales} ({MalePercentage:F1}%) | Females: {TotalFemales} ({FemalePercentage:F1}%)";
        }

        public string GetLaborSummary()
        {
            return $"Available: {TotalAvailableWorkers} | Employed: {TotalEmployed} | " +
                   $"Unemployed: {TotalUnemployed} ({UnemploymentRateFormatted})";
        }

        public string GetMilitarySummary()
        {
            return $"Eligible: {TotalMilitaryEligible} ({MilitaryEligiblePercentage:F1}% of total pop)";
        }

        public string GetEconomySummary()
        {
            return $"Food: {FoodStockpile} ({DailyFoodNet:+#;-#;0}/day) | Gold: {GoldStockpile} | Buildings: {BuildingCount}";
        }

        public string GetSocialMobilitySummary()
        {
            int eligibleForPromotion = 0;
            int atRiskOfDemotion = 0;

            foreach (var popVM in PopulationGroups)
            {
                // Check promotion eligibility
                bool canPromote = popVM.Archetype switch
                {
                    PopulationArchetypes.Laborer => popVM.Education >= 40f && popVM.Wealth >= 30f,
                    PopulationArchetypes.Artisan => popVM.Education >= 60f && popVM.Wealth >= 50f,
                    PopulationArchetypes.Merchant => popVM.Education >= 80f && popVM.Wealth >= 75f,
                    _ => false
                };
                if (canPromote) eligibleForPromotion++;

                // Check demotion risk
                bool atRisk = popVM.Wealth < 20f ||
                             (popVM.Archetype == PopulationArchetypes.Clergy && (popVM.Wealth < 15f || popVM.Fervor < 30f));
                if (atRisk) atRiskOfDemotion++;
            }

            return $"Eligible for promotion: {eligibleForPromotion} groups | At risk of demotion: {atRiskOfDemotion} groups";
        }

        private float GetPercentage(int value)
        {
            return TotalPopulation > 0 ? (value / (float)TotalPopulation) * 100f : 0f;
        }

        public List<PopulationGroupViewModel> GetGroupsByArchetype(PopulationArchetypes archetype)
        {
            return PopulationGroups.Where(p => p.Archetype == archetype).ToList();
        }

        public float GetArchetypePercentage(PopulationArchetypes archetype)
        {
            if (TotalPopulation == 0) return 0f;
            int count = PopulationByArchetype.GetValueOrDefault(archetype, 0);
            return (count / (float)TotalPopulation) * 100f;
        }
    }
}
