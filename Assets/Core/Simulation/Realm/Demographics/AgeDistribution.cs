using UnityEngine;

namespace TWK.Realms.Demographics
{
    /// <summary>
    /// Tracks age and gender distribution for a population group.
    /// Used to calculate available workers, military recruits, and birth rates.
    /// Simulates demographic impacts of war casualties.
    /// </summary>
    [System.Serializable]
    public class AgeDistribution
    {
        [Header("Age Brackets")]
        [Tooltip("Youth (0-17): Not working age, not military age")]
        public int YoungMales;
        public int YoungFemales;

        [Tooltip("Adult (18-45): Prime working and military age")]
        public int AdultMales;
        public int AdultFemales;

        [Tooltip("Middle Age (46-60): Working age, not military age")]
        public int MiddleMales;
        public int MiddleFemales;

        [Tooltip("Elderly (60+): Not working, not military")]
        public int ElderlyMales;
        public int ElderlyFemales;

        // Calculated properties
        public int TotalPopulation => YoungMales + YoungFemales + AdultMales + AdultFemales +
                                       MiddleMales + MiddleFemales + ElderlyMales + ElderlyFemales;

        public int TotalMales => YoungMales + AdultMales + MiddleMales + ElderlyMales;
        public int TotalFemales => YoungFemales + AdultFemales + MiddleFemales + ElderlyFemales;

        public float MalePercentage => TotalPopulation > 0 ? (TotalMales * 100f / TotalPopulation) : 50f;
        public float FemalePercentage => 100f - MalePercentage;

        public float AverageAge
        {
            get
            {
                if (TotalPopulation == 0) return 30f;

                float totalAge = (YoungMales + YoungFemales) * 10f +  // Avg age 10
                                 (AdultMales + AdultFemales) * 32f +  // Avg age 32
                                 (MiddleMales + MiddleFemales) * 53f + // Avg age 53
                                 (ElderlyMales + ElderlyFemales) * 70f; // Avg age 70

                return totalAge / TotalPopulation;
            }
        }

        /// <summary>
        /// Working age population (18-60)
        /// </summary>
        public int WorkingAgePopulation => AdultMales + AdultFemales + MiddleMales + MiddleFemales;

        /// <summary>
        /// Percentage of population that can work (0-1)
        /// </summary>
        public float WorkingAgePercentage => TotalPopulation > 0 ? (WorkingAgePopulation / (float)TotalPopulation) : 0.6f;

        /// <summary>
        /// Military eligible population (18-45 males)
        /// </summary>
        public int MilitaryEligiblePopulation => AdultMales;

        /// <summary>
        /// Breeding age females for birth rate calculations (18-45)
        /// </summary>
        public int BreedingAgeFemales => AdultFemales;

        /// <summary>
        /// Create a normalized distribution for a given total population
        /// </summary>
        public static AgeDistribution CreateNormalized(int totalPopulation, float averageAge = 30f)
        {
            var dist = new AgeDistribution();

            // Standard distribution percentages based on average age
            // Younger average = more youth, older average = more elderly
            float youthFactor = Mathf.Clamp01(1f - (averageAge / 50f));
            float elderlyFactor = Mathf.Clamp01((averageAge - 20f) / 50f);

            // Distribution percentages
            float youngPct = 0.20f + (youthFactor * 0.15f);     // 20-35%
            float adultPct = 0.45f;                              // 45%
            float middlePct = 0.20f;                             // 20%
            float elderlyPct = 0.10f + (elderlyFactor * 0.10f); // 10-20%

            // Normalize
            float total = youngPct + adultPct + middlePct + elderlyPct;
            youngPct /= total;
            adultPct /= total;
            middlePct /= total;
            elderlyPct /= total;

            // Distribute population (assume 51% male, 49% female at start)
            int young = Mathf.RoundToInt(totalPopulation * youngPct);
            int adult = Mathf.RoundToInt(totalPopulation * adultPct);
            int middle = Mathf.RoundToInt(totalPopulation * middlePct);
            int elderly = Mathf.RoundToInt(totalPopulation * elderlyPct);

            dist.YoungMales = Mathf.RoundToInt(young * 0.51f);
            dist.YoungFemales = young - dist.YoungMales;

            dist.AdultMales = Mathf.RoundToInt(adult * 0.51f);
            dist.AdultFemales = adult - dist.AdultMales;

            dist.MiddleMales = Mathf.RoundToInt(middle * 0.51f);
            dist.MiddleFemales = middle - dist.MiddleMales;

            dist.ElderlyMales = Mathf.RoundToInt(elderly * 0.51f);
            dist.ElderlyFemales = elderly - dist.ElderlyMales;

            return dist;
        }

        /// <summary>
        /// Age the population by one year (shift distributions)
        /// </summary>
        public void AdvanceYear()
        {
            // Shift percentages each year
            // ~5.5% of youth become adults each year (18 year span)
            // ~3.5% of adults become middle each year (28 year span)
            // ~7% of middle become elderly each year (14 year span)
            // Elderly gradually die off

            int youthToAdult = Mathf.RoundToInt((YoungMales + YoungFemales) * 0.055f);
            int adultToMiddle = Mathf.RoundToInt((AdultMales + AdultFemales) * 0.035f);
            int middleToElderly = Mathf.RoundToInt((MiddleMales + MiddleFemales) * 0.07f);

            // Apply aging (proportional to gender ratios)
            float youngMaleRatio = (YoungMales + YoungFemales) > 0 ? YoungMales / (float)(YoungMales + YoungFemales) : 0.5f;
            float adultMaleRatio = (AdultMales + AdultFemales) > 0 ? AdultMales / (float)(AdultMales + AdultFemales) : 0.5f;
            float middleMaleRatio = (MiddleMales + MiddleFemales) > 0 ? MiddleMales / (float)(MiddleMales + MiddleFemales) : 0.5f;

            // Youth -> Adult
            int youngMaleToAdult = Mathf.RoundToInt(youthToAdult * youngMaleRatio);
            int youngFemaleToAdult = youthToAdult - youngMaleToAdult;
            YoungMales -= youngMaleToAdult;
            YoungFemales -= youngFemaleToAdult;
            AdultMales += youngMaleToAdult;
            AdultFemales += youngFemaleToAdult;

            // Adult -> Middle
            int adultMaleToMiddle = Mathf.RoundToInt(adultToMiddle * adultMaleRatio);
            int adultFemaleToMiddle = adultToMiddle - adultMaleToMiddle;
            AdultMales -= adultMaleToMiddle;
            AdultFemales -= adultFemaleToMiddle;
            MiddleMales += adultMaleToMiddle;
            MiddleFemales += adultFemaleToMiddle;

            // Middle -> Elderly
            int middleMaleToElderly = Mathf.RoundToInt(middleToElderly * middleMaleRatio);
            int middleFemaleToElderly = middleToElderly - middleMaleToElderly;
            MiddleMales -= middleMaleToElderly;
            MiddleFemales -= middleFemaleToElderly;
            ElderlyMales += middleMaleToElderly;
            ElderlyFemales += middleFemaleToElderly;

            // Clamp negatives
            YoungMales = Mathf.Max(0, YoungMales);
            YoungFemales = Mathf.Max(0, YoungFemales);
            AdultMales = Mathf.Max(0, AdultMales);
            AdultFemales = Mathf.Max(0, AdultFemales);
            MiddleMales = Mathf.Max(0, MiddleMales);
            MiddleFemales = Mathf.Max(0, MiddleFemales);
            ElderlyMales = Mathf.Max(0, ElderlyMales);
            ElderlyFemales = Mathf.Max(0, ElderlyFemales);
        }

        /// <summary>
        /// Apply military casualties (primarily from adult males)
        /// </summary>
        public void ApplyCasualties(int casualties)
        {
            // Casualties come primarily from adult males (90%), some from young males (10%)
            int adultCasualties = Mathf.Min(Mathf.RoundToInt(casualties * 0.9f), AdultMales);
            int youngCasualties = Mathf.Min(casualties - adultCasualties, YoungMales);

            AdultMales -= adultCasualties;
            YoungMales -= youngCasualties;
        }

        /// <summary>
        /// Add new births (distributed as young)
        /// </summary>
        public void AddBirths(int births)
        {
            // 51% male, 49% female
            int maleBirths = Mathf.RoundToInt(births * 0.51f);
            int femaleBirths = births - maleBirths;

            YoungMales += maleBirths;
            YoungFemales += femaleBirths;
        }

        /// <summary>
        /// Scale the entire distribution by a percentage (for growth/decline)
        /// </summary>
        public void Scale(float multiplier)
        {
            YoungMales = Mathf.RoundToInt(YoungMales * multiplier);
            YoungFemales = Mathf.RoundToInt(YoungFemales * multiplier);
            AdultMales = Mathf.RoundToInt(AdultMales * multiplier);
            AdultFemales = Mathf.RoundToInt(AdultFemales * multiplier);
            MiddleMales = Mathf.RoundToInt(MiddleMales * multiplier);
            MiddleFemales = Mathf.RoundToInt(MiddleFemales * multiplier);
            ElderlyMales = Mathf.RoundToInt(ElderlyMales * multiplier);
            ElderlyFemales = Mathf.RoundToInt(ElderlyFemales * multiplier);
        }
    }
}
