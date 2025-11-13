using UnityEngine;
using TWK.Realms.Demographics;
using TWK.Cultures;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a single population group's data.
    /// </summary>
    public class PopulationGroupViewModel : BaseViewModel
    {
        private PopulationGroup _source;

        // ========== IDENTITY ==========
        public int ID { get; private set; }
        public int CityID { get; private set; }
        public PopulationArchetypes Archetype { get; private set; }
        public string ArchetypeName { get; private set; }

        public CultureData Culture { get; private set; }

        // ========== POPULATION ==========
        public int TotalPopulation { get; private set; }
        public float AverageAge { get; private set; }
        public float MalePercentage { get; private set; }
        public float FemalePercentage { get; private set; }

        // ========== DEMOGRAPHICS BREAKDOWN ==========
        public int YoungMales { get; private set; }
        public int YoungFemales { get; private set; }
        public int AdultMales { get; private set; }
        public int AdultFemales { get; private set; }
        public int MiddleMales { get; private set; }
        public int MiddleFemales { get; private set; }
        public int ElderlyMales { get; private set; }
        public int ElderlyFemales { get; private set; }

        public int TotalYouth { get; private set; }
        public int TotalAdults { get; private set; }
        public int TotalMiddleAge { get; private set; }
        public int TotalElderly { get; private set; }

        // ========== ECONOMIC ==========
        public float Wealth { get; private set; }
        public string WealthFormatted { get; private set; }
        public float Education { get; private set; }
        public string EducationFormatted { get; private set; }

        // ========== LABOR ==========
        public int AvailableWorkers { get; private set; }
        public int EmployedCount { get; private set; }
        public int UnemployedCount { get; private set; }
        public float UnemploymentRate { get; private set; }
        public string UnemploymentRateFormatted { get; private set; }

        public int MilitaryEligible { get; private set; }
        public float MilitaryEligiblePercentage { get; private set; }

        // ========== CULTURAL ==========
        public float Fervor { get; private set; }
        public string FervorFormatted { get; private set; }
        public string ReligionName { get; private set; }
        public float Loyalty { get; private set; }
        public string LoyaltyFormatted { get; private set; }

        // ========== LEGACY ==========
        public float Happiness { get; private set; }
        public string HappinessFormatted { get; private set; }
        public float GrowthModifier { get; private set; }

        // ========== CONSTRUCTOR ==========
        public PopulationGroupViewModel(PopulationGroup source)
        {
            _source = source;
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_source == null) return;

            // Identity
            ID = _source.ID;
            CityID = _source.OwnerCityID;
            Archetype = _source.Archetype;
            ArchetypeName = _source.Archetype.ToString();
            Culture = _source.Culture;

            // Population
            TotalPopulation = _source.PopulationCount;
            AverageAge = _source.AverageAge;
            MalePercentage = _source.MalePercentage;
            FemalePercentage = _source.FemalePercentage;

            // Demographics breakdown
            var demo = _source.Demographics;
            YoungMales = demo.YoungMales;
            YoungFemales = demo.YoungFemales;
            AdultMales = demo.AdultMales;
            AdultFemales = demo.AdultFemales;
            MiddleMales = demo.MiddleMales;
            MiddleFemales = demo.MiddleFemales;
            ElderlyMales = demo.ElderlyMales;
            ElderlyFemales = demo.ElderlyFemales;

            TotalYouth = YoungMales + YoungFemales;
            TotalAdults = AdultMales + AdultFemales;
            TotalMiddleAge = MiddleMales + MiddleFemales;
            TotalElderly = ElderlyMales + ElderlyFemales;

            // Economic
            Wealth = _source.Wealth;
            WealthFormatted = $"{Wealth:F1}";
            Education = _source.Education;
            EducationFormatted = $"{Education:F1}";

            // Labor
            AvailableWorkers = _source.AvailableWorkers;
            EmployedCount = _source.EmployedCount;
            UnemployedCount = _source.UnemployedCount;
            UnemploymentRate = AvailableWorkers > 0 ? (UnemployedCount / (float)AvailableWorkers) * 100f : 0f;
            UnemploymentRateFormatted = $"{UnemploymentRate:F1}%";

            MilitaryEligible = _source.MilitaryEligible;
            MilitaryEligiblePercentage = TotalPopulation > 0 ? (MilitaryEligible / (float)TotalPopulation) * 100f : 0f;

            // Cultural
            Fervor = _source.Fervor;
            FervorFormatted = $"{Fervor:F1}";
            ReligionName = _source.CurrentReligion?.Name ?? "None";
            Loyalty = _source.Loyalty;
            LoyaltyFormatted = $"{Loyalty:F1}";

            // Legacy
            Happiness = _source.Happiness;
            HappinessFormatted = $"{Happiness * 100f:F1}%";
            GrowthModifier = _source.GrowthModifier;

            NotifyPropertyChanged();
        }

        // ========== HELPER METHODS ==========
        public string GetDemographicSummary()
        {
            return $"Youth: {TotalYouth} | Adults: {TotalAdults} | Middle: {TotalMiddleAge} | Elderly: {TotalElderly}";
        }

        public string GetGenderSummary()
        {
            return $"Male: {MalePercentage:F1}% | Female: {FemalePercentage:F1}%";
        }

        public string GetLaborSummary()
        {
            return $"Available: {AvailableWorkers} | Employed: {EmployedCount} | Unemployed: {UnemployedCount}";
        }

        public string GetPromotionStatus()
        {
            // Based on promotion requirements from PopulationManager
            return Archetype switch
            {
                PopulationArchetypes.Slave => "Cannot promote (policy required)",
                PopulationArchetypes.Laborer => Education >= 40f && Wealth >= 30f ? "✓ Eligible for promotion to Artisan" : $"Need Edu: {Mathf.Max(0, 40f - Education):F0} | Wealth: {Mathf.Max(0, 30f - Wealth):F0}",
                PopulationArchetypes.Artisan => Education >= 60f && Wealth >= 50f ? "✓ Eligible for promotion to Merchant" : $"Need Edu: {Mathf.Max(0, 60f - Education):F0} | Wealth: {Mathf.Max(0, 50f - Wealth):F0}",
                PopulationArchetypes.Merchant => Education >= 80f && Wealth >= 75f ? "✓ Eligible for promotion to Noble" : $"Need Edu: {Mathf.Max(0, 80f - Education):F0} | Wealth: {Mathf.Max(0, 75f - Wealth):F0}",
                PopulationArchetypes.Noble => "Top tier",
                PopulationArchetypes.Clergy => "Special branch",
                _ => "Unknown"
            };
        }

        public string GetDemotionRisk()
        {
            if (Wealth < 20f)
                return "⚠ At risk of demotion (low wealth)";
            if (Archetype == PopulationArchetypes.Clergy && (Wealth < 15f || Fervor < 30f))
                return "⚠ At risk of demotion (low wealth or fervor)";
            return "Stable";
        }
    }
}
