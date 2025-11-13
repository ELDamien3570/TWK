using UnityEngine;
using TWK.Culture;

namespace TWK.Realms.Demographics
{
    /// <summary>
    /// Represents a population group within a city.
    /// Refactored from struct to class to support complex state management,
    /// including demographics, employment, education, and cultural properties.
    /// </summary>
    public class PopulationGroup
    {
        // ========== IDENTITY ==========
        public int ID { get; private set; }
        public int OwnerCityID { get; set; }
        public PopulationArchetypes Archetype { get; set; }

        // ========== DEMOGRAPHICS ==========
        /// <summary>
        /// Age and gender distribution for this population group
        /// </summary>
        public AgeDistribution Demographics { get; private set; }

        /// <summary>
        /// Total population count (derived from demographics)
        /// </summary>
        public int PopulationCount => Demographics.TotalPopulation;

        /// <summary>
        /// Average age of this population group
        /// </summary>
        public float AverageAge => Demographics.AverageAge;

        /// <summary>
        /// Percentage of males in population (0-100)
        /// </summary>
        public float MalePercentage => Demographics.MalePercentage;

        /// <summary>
        /// Percentage of females in population (0-100)
        /// </summary>
        public float FemalePercentage => Demographics.FemalePercentage;

        // ========== ECONOMIC ==========
        /// <summary>
        /// Wealth level of this population group (0-100)
        /// Affects promotion/demotion and consumption patterns
        /// </summary>
        public float Wealth { get; set; }

        /// <summary>
        /// Education level of this population group (0-100)
        /// Primary driver of social mobility (promotions)
        /// </summary>
        public float Education { get; set; }

        /// <summary>
        /// Number of people currently employed in buildings
        /// </summary>
        public int EmployedCount { get; set; }

        /// <summary>
        /// Number of people unemployed (consume resources without producing)
        /// </summary>
        public int UnemployedCount => Mathf.Max(0, AvailableWorkers - EmployedCount);

        // ========== LABOR ==========
        /// <summary>
        /// Working age population available for employment
        /// </summary>
        public int AvailableWorkers => Mathf.FloorToInt(Demographics.WorkingAgePopulation);

        /// <summary>
        /// Military eligible population (adult males)
        /// </summary>
        public int MilitaryEligible => Demographics.MilitaryEligiblePopulation;

        // ========== CULTURAL ==========
        /// <summary>
        /// Culture of this population group
        /// NOTE: PopulationGroups can only belong to one culture at a time.
        /// Culture conversions create new PopulationGroups.
        /// </summary>
        public CultureData Culture { get; set; }

        /// <summary>
        /// Religious fervor - resistance to religious conversion (0-100)
        /// </summary>
        public float Fervor { get; set; }

        /// <summary>
        /// Current religion of this population group
        /// </summary>
        public Religion CurrentReligion { get; set; }

        /// <summary>
        /// Loyalty to the state (-100 to 100)
        /// Affected by culture, policies (slavery), and treatment
        /// </summary>
        public float Loyalty { get; set; }

        // ========== LEGACY PROPERTIES (maintained for compatibility) ==========
        /// <summary>
        /// Overall happiness/satisfaction (0-1)
        /// </summary>
        public float Happiness { get; set; }

        /// <summary>
        /// Growth rate modifier (multiplied with base growth)
        /// </summary>
        public float GrowthModifier { get; set; }

        // ========== CONSTRUCTOR ==========
        public PopulationGroup(int id, int ownerCityId, PopulationArchetypes archetype, int initialPopulation, CultureData culture = null, float averageAge = 30f)
        {
            this.ID = id;
            this.OwnerCityID = ownerCityId;
            this.Archetype = archetype;

            // Initialize demographics with normalized distribution
            this.Demographics = AgeDistribution.CreateNormalized(initialPopulation, averageAge);

            // Initialize economic properties
            this.Wealth = GetDefaultWealthForArchetype(archetype);
            this.Education = GetDefaultEducationForArchetype(archetype);
            this.EmployedCount = 0;

            // Initialize cultural properties
            this.Culture = culture; // Can be set later if not provided
            this.Fervor = 50f; // Medium fervor by default
            this.CurrentReligion = null; // Set by city/realm
            this.Loyalty = GetDefaultLoyaltyForArchetype(archetype);

            // Initialize legacy properties
            this.Happiness = 0.75f;
            this.GrowthModifier = 1f;
        }

        // ========== HELPER METHODS ==========
        private float GetDefaultWealthForArchetype(PopulationArchetypes archetype)
        {
            return archetype switch
            {
                PopulationArchetypes.Slave => 5f,
                PopulationArchetypes.Laborer => 25f,
                PopulationArchetypes.Artisan => 50f,
                PopulationArchetypes.Merchant => 75f,
                PopulationArchetypes.Noble => 90f,
                PopulationArchetypes.Clergy => 60f,
                _ => 25f
            };
        }

        private float GetDefaultEducationForArchetype(PopulationArchetypes archetype)
        {
            return archetype switch
            {
                PopulationArchetypes.Slave => 5f,
                PopulationArchetypes.Laborer => 20f,
                PopulationArchetypes.Artisan => 45f,
                PopulationArchetypes.Merchant => 70f,
                PopulationArchetypes.Noble => 85f,
                PopulationArchetypes.Clergy => 80f,
                _ => 20f
            };
        }

        private float GetDefaultLoyaltyForArchetype(PopulationArchetypes archetype)
        {
            return archetype switch
            {
                PopulationArchetypes.Slave => -30f, // Slaves have low loyalty
                PopulationArchetypes.Laborer => 0f,
                PopulationArchetypes.Artisan => 10f,
                PopulationArchetypes.Merchant => 20f,
                PopulationArchetypes.Noble => 50f,
                PopulationArchetypes.Clergy => 30f,
                _ => 0f
            };
        }

        // ========== SIMULATION METHODS ==========
        /// <summary>
        /// Age the population by one year
        /// </summary>
        public void AdvanceYear()
        {
            Demographics.AdvanceYear();
        }

        /// <summary>
        /// Apply military casualties to this population group
        /// </summary>
        public void ApplyMilitaryCasualties(int casualties)
        {
            Demographics.ApplyCasualties(casualties);
        }

        /// <summary>
        /// Add new births to this population group
        /// </summary>
        public void AddBirths(int births)
        {
            Demographics.AddBirths(births);
        }

        /// <summary>
        /// Scale population by growth/decline factor
        /// </summary>
        public void ApplyGrowth(float growthMultiplier)
        {
            Demographics.Scale(growthMultiplier);
        }
    }

    /// <summary>
    /// Placeholder for Religion system (to be implemented)
    /// </summary>
    public class Religion
    {
        public string Name { get; set; }
        public int ID { get; set; }

        public Religion(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
