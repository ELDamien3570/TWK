using UnityEngine;
using TWK.Economy;
using TWK.Realms.Demographics;

namespace TWK.Realms
{
    /// <summary>
    /// Pure static functions for city simulation logic.
    /// Operates on CityData without side effects (besides the data itself).
    /// This is the "Logic" layer - pure functions that transform data.
    /// </summary>
    public static class CitySimulation
    {
        // ========== MAIN SIMULATION ==========

        /// <summary>
        /// Simulate one day for a city.
        /// </summary>
        public static void SimulateDay(CityData city, ResourceLedger ledger)
        {
            SimulateBuildings(city, ledger);
            SimulatePopulation(city, ledger);
        }

        /// <summary>
        /// Simulate one season for a city.
        /// </summary>
        public static void SimulateSeason(CityData city)
        {
            // TODO: Seasonal modifiers (food, festivals, events)
        }

        /// <summary>
        /// Simulate one year for a city.
        /// </summary>
        public static void SimulateYear(CityData city)
        {
            // TODO: Yearly updates (census, policies)
        }

        // ========== BUILDING SIMULATION ==========

        private static void SimulateBuildings(CityData city, ResourceLedger ledger)
        {
            // Get culture modifiers for this city
            var modifiers = GetCityModifiers(city.CityID);

            // Get buildings from BuildingManager and simulate them
            foreach (int buildingId in city.BuildingIDs)
            {
                var instanceData = BuildingManager.Instance.GetInstanceData(buildingId);

                if (instanceData != null && instanceData.IsCompleted && instanceData.IsActive)
                {
                    // Get the building definition
                    var definition = BuildingManager.Instance.GetDefinition(instanceData.BuildingDefinitionID);

                    if (definition != null)
                    {
                        // Use BuildingSimulation for production/maintenance/population effects (with modifiers)
                        BuildingSimulation.SimulateDay(instanceData, definition, ledger, city.CityID, modifiers);
                    }
                    else
                    {
                        Debug.LogWarning($"[CitySimulation] Building definition not found for building {buildingId}");
                    }
                }
            }
        }

        /// <summary>
        /// Get all active modifiers for a city from culture, religion, and timed events.
        /// </summary>
        private static List<TWK.Modifiers.Modifier> GetCityModifiers(int cityID)
        {
            var modifiers = new List<TWK.Modifiers.Modifier>();

            // Get the city's culture modifiers
            int cultureID = CultureManager.Instance.GetCityCulture(cityID);
            if (cultureID != -1)
            {
                var culture = CultureManager.Instance.GetCulture(cultureID);
                if (culture != null)
                {
                    modifiers.AddRange(culture.GetAllModifiers());
                }
            }

            // Get religion modifiers from the city's dominant religion
            // We collect modifiers from all population groups and use the most common religion
            var religionModifiers = GetCityReligionModifiers(cityID);
            modifiers.AddRange(religionModifiers);

            // Get timed modifiers from ModifierManager
            if (TWK.Modifiers.ModifierManager.Instance != null)
            {
                modifiers.AddRange(TWK.Modifiers.ModifierManager.Instance.GetCityTimedModifiers(cityID));
                modifiers.AddRange(TWK.Modifiers.ModifierManager.Instance.GetGlobalTimedModifiers());
            }

            return modifiers;
        }

        /// <summary>
        /// Get religion modifiers for a city based on its population's dominant religion.
        /// </summary>
        private static List<TWK.Modifiers.Modifier> GetCityReligionModifiers(int cityID)
        {
            var modifiers = new List<TWK.Modifiers.Modifier>();

            // Get all population groups in the city
            var populations = PopulationManager.Instance.GetPopulationsByCity(cityID);
            if (populations == null)
                return modifiers;

            // Find the dominant religion (religion with the most population)
            var religionPopCounts = new Dictionary<TWK.Religion.ReligionData, int>();

            foreach (var pop in populations)
            {
                if (pop.CurrentReligion != null)
                {
                    if (!religionPopCounts.ContainsKey(pop.CurrentReligion))
                        religionPopCounts[pop.CurrentReligion] = 0;

                    religionPopCounts[pop.CurrentReligion] += pop.PopulationCount;
                }
            }

            // If no religion found, return empty
            if (religionPopCounts.Count == 0)
                return modifiers;

            // Find the dominant religion
            TWK.Religion.ReligionData dominantReligion = null;
            int maxPopulation = 0;

            foreach (var kvp in religionPopCounts)
            {
                if (kvp.Value > maxPopulation)
                {
                    maxPopulation = kvp.Value;
                    dominantReligion = kvp.Key;
                }
            }

            // Get modifiers from the dominant religion
            if (dominantReligion != null)
            {
                modifiers.AddRange(dominantReligion.GetAllModifiers());
            }

            return modifiers;
        }

        // ========== POPULATION SIMULATION ==========

        private static void SimulatePopulation(CityData city, ResourceLedger ledger)
        {
            int totalPop = GetTotalPopulation(city.CityID);

            // Food consumption (1 food per person per day)
            int foodConsumption = totalPop;
            ledger.Subtract(ResourceType.Food, foodConsumption);
        }

        // ========== HELPER FUNCTIONS ==========

        /// <summary>
        /// Get total population for a city from PopulationManager.
        /// </summary>
        public static int GetTotalPopulation(int cityId)
        {
            int total = 0;
            foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(cityId))
                total += pop.PopulationCount;
            return total;
        }

        /// <summary>
        /// Update the economy snapshot from the ledger.
        /// </summary>
        public static void UpdateEconomySnapshot(CityData city, ResourceLedger ledger)
        {
            city.EconomySnapshot.Production = new System.Collections.Generic.Dictionary<ResourceType, int>(ledger.DailyProduction);
            city.EconomySnapshot.Consumption = new System.Collections.Generic.Dictionary<ResourceType, int>(ledger.DailyConsumption);
            city.EconomySnapshot.Net = new System.Collections.Generic.Dictionary<ResourceType, int>(ledger.DailyChange);
        }

        /// <summary>
        /// Sync building IDs with BuildingManager (add/remove as needed).
        /// </summary>
        public static void SyncBuildingStates(CityData city)
        {
            // This is called to ensure CityData.BuildingIDs matches what BuildingManager has
            // For now, we rely on manual Add/Remove via the public API
        }
    }
}
