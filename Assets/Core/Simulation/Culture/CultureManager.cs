using System.Collections.Generic;
using System;
using UnityEngine;
using TWK.Realms;
using TWK.Realms.Demographics;
using TWK.Economy;
using System.Linq;
using TWK.Core;
using TWK.Simulation;

namespace TWK.Cultures
{
    /// <summary>
    /// Manager for all culture systems: ownership, tech trees, XP accumulation, and assimilation.
    /// </summary>
    public class CultureManager : MonoBehaviour, ISimulationAgent
    {
        public static CultureManager Instance { get; private set; }

        // ========== EVENTS ==========
        /// <summary>
        /// Fired when a city's main culture changes.
        /// Args: cityID, oldCultureID, newCultureID
        /// </summary>
        public event Action<int, int, int> OnCityCultureChanged;

        /// <summary>
        /// Fired when a culture unlocks new buildings (via tech tree or pillars).
        /// Args: cultureID
        /// </summary>
        public event Action<int> OnCultureBuildingsChanged;

        /// <summary>
        /// Fired when XP is added to a culture's tech tree.
        /// Args: cultureID, treeType, xpAmount
        /// </summary>
        public event Action<int, TreeType, float> OnCultureXPAdded;

        // ========== CULTURE DATA ==========
        [Header("Cultures")]
        [SerializeField] private List<CultureData> allCultures = new List<CultureData>();

        private Dictionary<int, CultureData> cultureLookup = new Dictionary<int, CultureData>();

        // ========== CITY CULTURES ==========
        // Maps city ID -> culture ID (based on most prevalent culture)
        private Dictionary<int, int> cityCultures = new Dictionary<int, int>();

        // ========== REALM LEADER CULTURES ==========
        // Maps realm ID -> culture ID (culture of the realm's primary leader)
        private Dictionary<int, int> realmLeaderCultures = new Dictionary<int, int>();

        // ========== XP ACCUMULATION ==========
        [Header("XP Settings")]
        [Tooltip("Days per month for XP generation")]
        [SerializeField] private int daysPerMonth = 30;

        private int daysSinceLastXPTick = 0;

        // ========== ASSIMILATION ==========
        [Header("Assimilation Settings")]
        [Tooltip("Base rate of cultural assimilation per year (can be modified)")]
        [SerializeField] private float baseAssimilationRate = 0.02f; // 2% per year

        // ========== REALM+CULTURE POPULATION CACHE ==========
        // Cache realm+culture populations for efficient ownership calculations
        // Key: (realmID, cultureID), Value: total population
        private Dictionary<(int realmID, int cultureID), int> realmCulturePopulations = new Dictionary<(int, int), int>();
        private bool realmCulturePopulationsDirty = true;

        // ========== TIME MANAGEMENT ==========
        private WorldTimeManager worldTimeManager;

        // ========== INITIALIZATION ==========

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            this.worldTimeManager = worldTimeManager;
            worldTimeManager.OnDayTick += AdvanceDay;

            Debug.Log("[CultureManager] Initialized and registered with WorldTimeManager");
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeCultures();

            // Subscribe to population changes in Awake to ensure we catch all events
            // even if populations are created in other managers' Start() methods
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnCityPopulationChanged += HandleCityPopulationChanged;
            }
            else
            {
                Debug.LogWarning("[CultureManager] PopulationManager not found during Awake. Culture updates may not work.");
            }
        }

        private void Start()
        {
            // Event subscription moved to Awake to ensure proper initialization order
        }

        private void OnDestroy()
        {
            // Unsubscribe from population changes
            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnCityPopulationChanged -= HandleCityPopulationChanged;
            }
        }

        /// <summary>
        /// Handle population changes in cities - recalculate culture if needed.
        /// </summary>
        private void HandleCityPopulationChanged(int cityID)
        {
            // Invalidate realm+culture population cache
            realmCulturePopulationsDirty = true;

            // Recalculate city culture (this will fire OnCityCultureChanged if culture changed)
            CalculateCityCulture(cityID);
        }

        private void InitializeCultures()
        {
            cultureLookup.Clear();

            foreach (var culture in allCultures)
            {
                if (culture != null)
                {
                    culture.InitializeTechTrees();
                    cultureLookup[culture.GetCultureID()] = culture;
                }
            }

            Debug.Log($"[CultureManager] Initialized {allCultures.Count} cultures");
        }

        // ========== CULTURE ACCESS ==========

        public CultureData GetCulture(int cultureID)
        {
            return cultureLookup.GetValueOrDefault(cultureID, null);
        }

        public List<CultureData> GetAllCultures()
        {
            return new List<CultureData>(allCultures);
        }

        public void AddCulture(CultureData culture)
        {
            if (!allCultures.Contains(culture))
            {
                allCultures.Add(culture);
                cultureLookup[culture.GetCultureID()] = culture;
                culture.InitializeTechTrees();
            }
        }

        // ========== REALM LEADER CULTURES ==========

        /// <summary>
        /// Set the culture of a realm's primary leader.
        /// Call this when a realm leader changes or when a leader changes culture.
        /// </summary>
        public void SetRealmLeaderCulture(int realmID, int cultureID)
        {
            int oldCulture = realmLeaderCultures.GetValueOrDefault(realmID, -1);
            if (oldCulture != cultureID)
            {
                realmLeaderCultures[realmID] = cultureID;
                Debug.Log($"[CultureManager] Realm {realmID} leader culture changed to {cultureID}");
            }
        }

        /// <summary>
        /// Get the culture of a realm's primary leader.
        /// Returns -1 if not set.
        /// </summary>
        public int GetRealmLeaderCulture(int realmID)
        {
            return realmLeaderCultures.GetValueOrDefault(realmID, -1);
        }

        // ========== CULTURE OWNERSHIP ==========

        /// <summary>
        /// Update tech tree ownership for all cultures.
        /// Owner is the realm leader with the largest population of that culture.
        /// </summary>
        public void UpdateCultureOwnership()
        {
            foreach (var culture in allCultures)
            {
                foreach (TreeType treeType in System.Enum.GetValues(typeof(TreeType)))
                {
                    var tree = culture.GetTechTree(treeType);
                    if (tree == null) continue;

                    // Find realm with largest population of this culture
                    int newOwner = GetRealmWithLargestCulturePopulation(culture.GetCultureID());

                    if (newOwner != tree.OwnerRealmID)
                    {
                        Debug.Log($"[CultureManager] {culture.CultureName} {treeType} tree ownership changed from Realm {tree.OwnerRealmID} to Realm {newOwner}");
                        tree.OwnerRealmID = newOwner;
                    }
                }
            }
        }

        /// <summary>
        /// Rebuild the realm+culture population cache from all population groups.
        /// Called when cache is dirty (populations changed).
        /// </summary>
        private void RebuildRealmCulturePopulationCache()
        {
            realmCulturePopulations.Clear();

            foreach (var popGroup in PopulationManager.Instance.GetAllPopulationGroups())
            {
                if (popGroup.Culture == null) continue;

                // Get realm ID from city
                var city = PopulationManager.Instance.GetCityByID(popGroup.OwnerCityID);
                if (city == null) continue;

                int realmID = city.Data.OwnerRealmID;
                int cultureID = popGroup.Culture.GetCultureID();
                var key = (realmID, cultureID);

                if (!realmCulturePopulations.ContainsKey(key))
                    realmCulturePopulations[key] = 0;

                realmCulturePopulations[key] += popGroup.PopulationCount;
            }

            realmCulturePopulationsDirty = false;
        }

        private int GetRealmWithLargestCulturePopulation(int cultureID)
        {
            // Rebuild cache if dirty
            if (realmCulturePopulationsDirty)
            {
                RebuildRealmCulturePopulationCache();
            }

            // Find realm with largest population for this culture using cached data
            int largestRealmID = -1;
            int largestPopulation = 0;

            foreach (var kvp in realmCulturePopulations)
            {
                if (kvp.Key.cultureID == cultureID && kvp.Value > largestPopulation)
                {
                    largestPopulation = kvp.Value;
                    largestRealmID = kvp.Key.realmID;
                }
            }

            return largestRealmID;
        }

        // ========== CITY CULTURE CALCULATION ==========

        /// <summary>
        /// Calculate and update the dominant culture for a city (most prevalent culture).
        /// Returns the culture ID, or -1 if the city has no population.
        /// </summary>
        public int CalculateCityCulture(int cityID)
        {
            var popGroups = PopulationManager.Instance.GetPopulationsByCity(cityID);
            var culturePops = new Dictionary<int, int>();
            int totalPop = 0;

            foreach (var popGroup in popGroups)
            {
                if (popGroup.Culture == null) continue;

                int cultureID = popGroup.Culture.GetCultureID();
                if (!culturePops.ContainsKey(cultureID))
                    culturePops[cultureID] = 0;

                culturePops[cultureID] += popGroup.PopulationCount;
                totalPop += popGroup.PopulationCount;
            }

            if (totalPop == 0)
                return -1;

            // Find culture with highest population (most prevalent)
            int dominantCultureID = -1;
            int highestPopulation = 0;

            foreach (var kvp in culturePops)
            {
                if (kvp.Value > highestPopulation)
                {
                    highestPopulation = kvp.Value;
                    dominantCultureID = kvp.Key;
                }
            }

            // Update city culture if it changed
            if (dominantCultureID != -1)
            {
                int oldCulture = cityCultures.GetValueOrDefault(cityID, -1);
                int newCulture = dominantCultureID;

                if (oldCulture != newCulture)
                {
                    cityCultures[cityID] = newCulture;
                    FireCityCultureChangedEvent(cityID, oldCulture, newCulture);
                }

                return dominantCultureID;
            }

            // Should not reach here if totalPop > 0, but handle edge case
            return -1;
        }

        /// <summary>
        /// Get the dominant culture for a city.
        /// </summary>
        public int GetCityCulture(int cityID)
        {
            return cityCultures.GetValueOrDefault(cityID, -1);
        }

        /// <summary>
        /// Called when a city's dominant culture changes.
        /// Fires the OnCityCultureChanged event for listeners.
        /// </summary>
        private void FireCityCultureChangedEvent(int cityID, int oldCultureID, int newCultureID)
        {
           // Debug.Log($"[CultureManager] City {cityID} culture changed from {oldCultureID} to {newCultureID}");

            // Fire event for cities and other systems to react
            OnCityCultureChanged?.Invoke(cityID, oldCultureID, newCultureID);
        }

        // ========== SIMULATION INTERFACE ==========

        /// <summary>
        /// Called every day by WorldTimeManager.
        /// Accumulates XP monthly from buildings.
        /// </summary>
        public void AdvanceDay()
        {
            daysSinceLastXPTick++;

            if (daysSinceLastXPTick >= daysPerMonth)
            {
                ProcessMonthlyXP();
                daysSinceLastXPTick = 0;
            }
        }

        public void AdvanceSeason()
        {
            // Culture assimilation could happen here seasonally if desired
        }

        public void AdvanceYear()
        {
            // Process yearly culture assimilation
            ProcessCultureAssimilation();
        }

        private void ProcessMonthlyXP()
        {
            // Group buildings by city culture and tree type
            var xpByCultureAndTree = new Dictionary<int, Dictionary<TreeType, float>>();

            //Test assigment


            foreach (var instanceData in BuildingManager.Instance.GetAllBuildings())
            {
                if (!instanceData.IsActive || !instanceData.IsCompleted)
                    continue;

                // Get building definition
                var definition = BuildingManager.Instance.GetDefinition(instanceData.BuildingDefinitionID);
                if (definition == null) continue;

                // Get city


                var city = PopulationManager.Instance.GetCityByID(instanceData.CityID); //Using Population to query cities right now, this is TEST LOGIC
                if (city == null) continue;

                // Get city's culture (calculate if not set yet)
                int cityCultureID = GetCityCulture(instanceData.CityID);
                if (cityCultureID == -1)
                {
                    // City culture not calculated yet, calculate it now
                    cityCultureID = CalculateCityCulture(instanceData.CityID);
                    if (cityCultureID == -1)
                    {
                        // City has no population, skip this building
                        continue;
                    }
                }

                // Calculate XP (same as production calculation)
                float averageEfficiency = CalculateAverageWorkerEfficiency(instanceData, definition);
                float xp = definition.CalculateMonthlyXP(instanceData.TotalWorkers, averageEfficiency);

                if (xp <= 0f) continue;

                // Add to culture's tech tree
                if (!xpByCultureAndTree.ContainsKey(cityCultureID))
                    xpByCultureAndTree[cityCultureID] = new Dictionary<TreeType, float>();

                if (!xpByCultureAndTree[cityCultureID].ContainsKey(definition.BuildingCategory))
                    xpByCultureAndTree[cityCultureID][definition.BuildingCategory] = 0f;

                xpByCultureAndTree[cityCultureID][definition.BuildingCategory] += xp;
            }

            // Apply XP to cultures
            foreach (var cultureKvp in xpByCultureAndTree)
            {
                int cultureID = cultureKvp.Key;
                var culture = GetCulture(cultureID);
                if (culture == null) continue;

                foreach (var treeKvp in cultureKvp.Value)
                {
                    TreeType treeType = treeKvp.Key;
                    float xp = treeKvp.Value;

                    var tree = culture.GetTechTree(treeType);
                    if (tree != null)
                    {
                        tree.AddXP(xp);
                        Debug.Log($"[CultureManager] {culture.CultureName} gained {xp:F2} XP in {treeType} tree");

                        // Fire event for UI updates
                        OnCultureXPAdded?.Invoke(cultureID, treeType, xp);
                    }
                }
            }
        }

        private float CalculateAverageWorkerEfficiency(BuildingInstanceData instance, BuildingDefinition definition)
        {
            if (instance.TotalWorkers == 0)
                return 0f;

            float totalEfficiency = 0f;
            foreach (var kvp in instance.AssignedWorkers)
            {
                float archetypeEfficiency = definition.GetWorkerEfficiency(kvp.Key);
                totalEfficiency += archetypeEfficiency * kvp.Value;
            }

            return totalEfficiency / instance.TotalWorkers;
        }

        // ========== CULTURE ASSIMILATION ==========

        /// <summary>
        /// Call this from SimulationManager every year.
        /// Cities with culture different from their leader assimilate slowly.
        /// </summary>
        public void ProcessCultureAssimilation()
        {
            foreach (var city in PopulationManager.Instance.GetAllCities())
            {
                ProcessCityAssimilation(city);
            }
        }

        private void ProcessCityAssimilation(City city)
        {
            // Get city's dominant culture
            int cityCultureID = CalculateCityCulture(city.CityID);
            if (cityCultureID == -1) return;

            // Get realm leader's culture
            int leaderCultureID = GetRealmLeaderCulture(city.Data.OwnerRealmID);
            if (leaderCultureID == -1)
            {
                // No realm leader culture set, skip assimilation
                return;
            }

            if (leaderCultureID == cityCultureID)
            {
                // City already matches leader's culture, no assimilation needed
                return;
            }

            // Get the target culture to assimilate toward
            var targetCulture = GetCulture(leaderCultureID);
            if (targetCulture == null)
            {
                Debug.LogWarning($"[CultureManager] Leader culture {leaderCultureID} not found for assimilation");
                return;
            }

            // Assimilate a small percentage of each pop group toward leader's culture
            var popGroups = PopulationManager.Instance.GetPopulationsByCity(city.CityID).ToList();

            foreach (var popGroup in popGroups)
            {
                // Skip if already the target culture
                if (popGroup.Culture != null && popGroup.Culture.GetCultureID() == leaderCultureID)
                    continue;

                // Skip if no culture set
                if (popGroup.Culture == null)
                    continue;

                // Calculate how many people to convert
                int populationToConvert = Mathf.FloorToInt(popGroup.PopulationCount * baseAssimilationRate);

                if (populationToConvert <= 0)
                    continue;

                // Don't convert more than the population
                populationToConvert = Mathf.Min(populationToConvert, popGroup.PopulationCount);

                // Convert by splitting off a new population group
                SplitPopulationGroupForCultureConversion(
                    popGroup,
                    populationToConvert,
                    targetCulture
                );

                Debug.Log($"[CultureManager] City {city.Name}: {populationToConvert} {popGroup.Archetype} converted from {popGroup.Culture.CultureName} to {targetCulture.CultureName}");
            }

            // Recalculate city culture after assimilation
            CalculateCityCulture(city.CityID);
        }

        /// <summary>
        /// Split off a portion of a population group for culture conversion.
        /// Creates a new PopulationGroup with the target culture.
        /// </summary>
        private void SplitPopulationGroupForCultureConversion(
            PopulationGroup sourceGroup,
            int populationToConvert,
            CultureData targetCulture)
        {
            // Reduce source population
            float scaleFactor = (sourceGroup.PopulationCount - populationToConvert) / (float)sourceGroup.PopulationCount;
            sourceGroup.Demographics.Scale(scaleFactor);

            // Create new population group with converted population
            var newGroup = PopulationManager.Instance.CreatePopulationGroup(
                sourceGroup.OwnerCityID,
                sourceGroup.Archetype,
                populationToConvert,
                targetCulture,
                sourceGroup.AverageAge
            );

            // Copy economic and cultural properties from source
            newGroup.Wealth = sourceGroup.Wealth;
            newGroup.Education = sourceGroup.Education;
            newGroup.Fervor = sourceGroup.Fervor;
            newGroup.CurrentReligion = sourceGroup.CurrentReligion;
            newGroup.Loyalty = sourceGroup.Loyalty;
            newGroup.Happiness = sourceGroup.Happiness;
            newGroup.GrowthModifier = sourceGroup.GrowthModifier;
        }

        // ========== TECH NODE UNLOCKING ==========

        /// <summary>
        /// Attempt to unlock a tech node for a culture.
        /// Only the culture owner (realm leader) can unlock nodes.
        /// </summary>
        public bool UnlockTechNode(int cultureID, TechNode node, int realmID)
        {
            var culture = GetCulture(cultureID);
            if (culture == null)
            {
                Debug.LogWarning($"[CultureManager] Culture {cultureID} not found");
                return false;
            }

            var tree = culture.GetTechTree(node.TreeType);
            if (tree == null)
            {
                Debug.LogWarning($"[CultureManager] Tech tree {node.TreeType} not found for culture {culture.CultureName}");
                return false;
            }

            // Check if this realm owns the tech tree
            if (tree.OwnerRealmID != realmID)
            {
                Debug.LogWarning($"[CultureManager] Realm {realmID} does not own {culture.CultureName}'s {node.TreeType} tree");
                return false;
            }

            // Attempt to unlock
            bool success = tree.UnlockNode(node);

            if (success)
            {
                OnTechNodeUnlocked(culture, node);
            }

            return success;
        }

        private void OnTechNodeUnlocked(CultureData culture, TechNode node)
        {
            Debug.Log($"[CultureManager] {culture.CultureName} unlocked tech node: {node.NodeName}");

            // Check if this node unlocks any buildings
            if (node.UnlockedBuildings != null && node.UnlockedBuildings.Count > 0)
            {
                // Fire event so cities with this culture can refresh their available buildings
                OnCultureBuildingsChanged?.Invoke(culture.GetCultureID());
            }

            // TODO: Apply modifiers to all realms/cities/agents of this culture
            // This will be implemented in the modifier application system
        }

        // ========== HYBRID CULTURE CREATION ==========

        /// <summary>
        /// Create a hybrid culture from two parent cultures.
        /// Cost should be deducted before calling this.
        /// </summary>
        public CultureData CreateHybridCulture(
            string name,
            string description,
            CultureData parent1,
            CultureData parent2,
            List<TreeType> treesFromParent1,
            List<TreeType> treesFromParent2)
        {
            var hybrid = CultureData.CreateHybrid(
                name,
                description,
                parent1,
                parent2,
                treesFromParent1,
                treesFromParent2
            );

            AddCulture(hybrid);

            Debug.Log($"[CultureManager] Created hybrid culture: {name} from {parent1.CultureName} and {parent2.CultureName}");

            return hybrid;
        }
    }
}
