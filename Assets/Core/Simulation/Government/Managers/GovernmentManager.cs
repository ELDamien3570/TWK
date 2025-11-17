using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Modifiers;
using TWK.Realms.Demographics;

namespace TWK.Government
{
    /// <summary>
    /// Central manager for all government-related operations.
    /// Manages government types, bureaucracy, offices, edicts, legitimacy, capacity, and reforms.
    /// </summary>
    public class GovernmentManager : MonoBehaviour, ISimulationAgent
    {
        public static GovernmentManager Instance { get; private set; }

        // ========== GOVERNMENT REGISTRY ==========
        /// <summary>
        /// All available government templates.
        /// Key: Government stable ID
        /// </summary>
        private Dictionary<int, GovernmentData> governmentRegistry = new Dictionary<int, GovernmentData>();

        // ========== ACTIVE GOVERNMENTS ==========
        /// <summary>
        /// Active governments per realm.
        /// Key: Realm ID, Value: Government Data
        /// </summary>
        private Dictionary<int, GovernmentData> activeGovernments = new Dictionary<int, GovernmentData>();

        // ========== BUREAUCRACY STATE ==========
        /// <summary>
        /// Bureaucracy state per realm.
        /// Key: Realm ID, Value: List of offices
        /// </summary>
        private Dictionary<int, List<Office>> realmBureaucracies = new Dictionary<int, List<Office>>();

        // ========== REALM STATS ==========
        /// <summary>
        /// Legitimacy per realm (0-100)
        /// </summary>
        private Dictionary<int, float> realmLegitimacy = new Dictionary<int, float>();

        /// <summary>
        /// Capacity per realm (0-100)
        /// </summary>
        private Dictionary<int, float> realmCapacity = new Dictionary<int, float>();

        // ========== EDICTS ==========
        /// <summary>
        /// Active edicts per realm
        /// </summary>
        private Dictionary<int, List<Edict>> realmEdicts = new Dictionary<int, List<Edict>>();

        // ========== TIME TRACKING ==========
        private WorldTimeManager worldTimeManager;
        private int currentMonthCounter = 0;
        private int currentDayCounter = 0;

        // ========== EVENTS ==========
        public event Action<int, GovernmentData> OnGovernmentChanged;
        public event Action<int, Office> OnOfficeAssigned;
        public event Action<int, Office> OnOfficeCreated;
        public event Action<int, Edict> OnEdictEnacted;
        public event Action<int, Edict> OnEdictExpired;
        public event Action<int, RevoltType> OnRevoltTriggered;
        public event Action<int, float> OnLegitimacyChanged;
        public event Action<int, float> OnCapacityChanged;

        // ========== INITIALIZATION ==========
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

            Debug.Log("[GovernmentManager] Initialized");
        }

        private void OnDestroy()
        {
            if (worldTimeManager != null)
            {
                worldTimeManager.OnDayTick -= AdvanceDay;
                worldTimeManager.OnSeasonTick -= AdvanceSeason;
                worldTimeManager.OnYearTick -= AdvanceYear;
            }
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get all population groups belonging to a realm.
        /// </summary>
        private List<PopulationGroup> GetRealmPopulations(int realmID)
        {
            if (PopulationManager.Instance == null)
                return new List<PopulationGroup>();

            var allPops = PopulationManager.Instance.GetAllPopulationGroups();
            return allPops.Where(p =>
            {
                var city = PopulationManager.Instance.GetCityByID(p.OwnerCityID);
                if (city == null)
                    return false;
                var realm = city.GetComponent<TWK.Realms.Realm>();
                return realm != null && realm.RealmID == realmID;
            }).ToList();
        }

        // ========== GOVERNMENT REGISTRATION ==========

        /// <summary>
        /// Register a government template in the registry.
        /// </summary>
        public void RegisterGovernment(GovernmentData government)
        {
            if (government == null)
            {
                Debug.LogError("[GovernmentManager] Cannot register null government");
                return;
            }

            int govID = government.GetStableGovernmentID();
            if (governmentRegistry.ContainsKey(govID))
            {
                Debug.LogWarning($"[GovernmentManager] Government '{government.GovernmentName}' already registered");
                return;
            }

            governmentRegistry[govID] = government;
            Debug.Log($"[GovernmentManager] Registered government: {government.GovernmentName} (ID: {govID})");
        }

        /// <summary>
        /// Get a government template by ID.
        /// </summary>
        public GovernmentData GetGovernmentTemplate(int governmentID)
        {
            if (governmentRegistry.TryGetValue(governmentID, out var government))
                return government;

            Debug.LogWarning($"[GovernmentManager] Government template with ID {governmentID} not found");
            return null;
        }

        /// <summary>
        /// Get all registered government templates.
        /// </summary>
        public List<GovernmentData> GetAllGovernmentTemplates()
        {
            return new List<GovernmentData>(governmentRegistry.Values);
        }

        // ========== REALM GOVERNMENT ==========

        /// <summary>
        /// Set a realm's government.
        /// </summary>
        public void SetRealmGovernment(int realmID, GovernmentData government)
        {
            if (government == null)
            {
                Debug.LogError("[GovernmentManager] Cannot set null government");
                return;
            }

            var previousGovernment = GetRealmGovernment(realmID);
            activeGovernments[realmID] = government;

            // Initialize government stats if this is a new realm
            if (!realmLegitimacy.ContainsKey(realmID))
            {
                realmLegitimacy[realmID] = government.BaseLegitimacy;
                realmCapacity[realmID] = government.BaseCapacity;
            }

            // Initialize bureaucracy if needed
            if (!realmBureaucracies.ContainsKey(realmID))
            {
                realmBureaucracies[realmID] = new List<Office>();
            }

            // Initialize edicts if needed
            if (!realmEdicts.ContainsKey(realmID))
            {
                realmEdicts[realmID] = new List<Edict>();
            }

            OnGovernmentChanged?.Invoke(realmID, government);
            Debug.Log($"[GovernmentManager] Realm {realmID} government set to {government.GovernmentName}");
        }

        /// <summary>
        /// Get a realm's active government.
        /// </summary>
        public GovernmentData GetRealmGovernment(int realmID)
        {
            if (activeGovernments.TryGetValue(realmID, out var government))
                return government;

            return null;
        }

        /// <summary>
        /// Check if a realm has a government assigned.
        /// </summary>
        public bool HasGovernment(int realmID)
        {
            return activeGovernments.ContainsKey(realmID);
        }

        // ========== BUREAUCRACY MANAGEMENT ==========

        /// <summary>
        /// Create a new office for a realm.
        /// </summary>
        public Office CreateOffice(int realmID, string officeName, TWK.Cultures.TreeType skillTree, OfficePurpose purpose = OfficePurpose.None)
        {
            if (!realmBureaucracies.ContainsKey(realmID))
            {
                realmBureaucracies[realmID] = new List<Office>();
            }

            var office = new Office(officeName, skillTree, purpose);
            realmBureaucracies[realmID].Add(office);

            OnOfficeCreated?.Invoke(realmID, office);
            Debug.Log($"[GovernmentManager] Created office '{officeName}' for realm {realmID}");

            return office;
        }

        /// <summary>
        /// Assign an agent to an office.
        /// </summary>
        public void AssignOffice(int realmID, Office office, int agentID)
        {
            if (office == null)
            {
                Debug.LogError("[GovernmentManager] Cannot assign null office");
                return;
            }

            office.AssignAgent(agentID);
            RecalculateOfficeEfficiency(office, agentID);

            OnOfficeAssigned?.Invoke(realmID, office);
            Debug.Log($"[GovernmentManager] Agent {agentID} assigned to office '{office.OfficeName}' in realm {realmID}");
        }

        /// <summary>
        /// Recalculate office efficiency based on agent skill.
        /// </summary>
        private void RecalculateOfficeEfficiency(Office office, int agentID)
        {
            if (agentID == -1)
            {
                office.CurrentEfficiency = 0f;
                return;
            }

            // TODO: Get agent from Agent system when AgentManager is implemented
            // For now, use base efficiency with a placeholder skill level
            // Expected integration: AgentManager.Instance.GetAgent(agentID).SkillLevels[office.SkillTree]
            float agentSkillLevel = 50f; // Placeholder: moderate skill
            office.UpdateEfficiency(agentSkillLevel);
        }

        /// <summary>
        /// Get all offices for a realm.
        /// </summary>
        public List<Office> GetRealmOffices(int realmID)
        {
            if (realmBureaucracies.TryGetValue(realmID, out var offices))
                return new List<Office>(offices);

            return new List<Office>();
        }

        /// <summary>
        /// Get filled offices for a realm.
        /// </summary>
        public List<Office> GetFilledOffices(int realmID)
        {
            return GetRealmOffices(realmID).Where(o => o.IsFilled()).ToList();
        }

        /// <summary>
        /// Get vacant offices for a realm.
        /// </summary>
        public List<Office> GetVacantOffices(int realmID)
        {
            return GetRealmOffices(realmID).Where(o => o.IsVacant()).ToList();
        }

        /// <summary>
        /// Calculate the cost of adding a new office.
        /// </summary>
        public int CalculateOfficeCost(int realmID)
        {
            int existingCount = GetRealmOffices(realmID).Count;
            // Base cost: 100, increases by 50% per existing office
            return 100 + (existingCount * 50);
        }

        /// <summary>
        /// Get total monthly office costs for a realm.
        /// </summary>
        public int GetMonthlyOfficeCosts(int realmID)
        {
            var offices = GetRealmOffices(realmID);
            return offices.Sum(o => o.GetMonthlyCost());
        }

        // ========== OFFICE AUTOMATION ==========

        /// <summary>
        /// Process all office automation tasks.
        /// </summary>
        private void ProcessOfficeAutomation()
        {
            foreach (var kvp in realmBureaucracies)
            {
                int realmID = kvp.Key;
                var offices = kvp.Value;

                foreach (var office in offices)
                {
                    if (office.CanExecuteNow(currentDayCounter))
                    {
                        ExecuteOfficePurpose(realmID, office);
                        office.MarkExecuted(currentDayCounter);
                    }
                }
            }
        }

        /// <summary>
        /// Execute the automation logic for an office based on its purpose.
        /// </summary>
        private void ExecuteOfficePurpose(int realmID, Office office)
        {
            switch (office.Purpose)
            {
                case OfficePurpose.RaiseLoyaltyInSlaveGroups:
                    ExecuteLoyaltyBoost(realmID, office);
                    break;

                case OfficePurpose.ManageTaxCollection:
                    ExecuteTaxOptimization(realmID, office);
                    break;

                case OfficePurpose.MigratePeoples:
                    ExecuteMigration(realmID, office);
                    break;

                // Add more office purposes as needed
                default:
                    Debug.Log($"[GovernmentManager] Office '{office.OfficeName}' purpose {office.Purpose} not yet implemented");
                    break;
            }
        }

        private void ExecuteLoyaltyBoost(int realmID, Office office)
        {
            if (office.TargetCityID == -1)
            {
                Debug.LogWarning($"[GovernmentManager] Office '{office.OfficeName}' has no target city for loyalty boost");
                return;
            }

            if (PopulationManager.Instance == null)
                return;

            var popGroups = PopulationManager.Instance.GetPopulationsByCity(office.TargetCityID);
            if (popGroups == null)
                return;

            var slavePops = popGroups.Where(p => p.Archetype == PopulationArchetypes.Slave).ToList();
            if (slavePops.Count == 0)
                return;

            float loyaltyGain = 1f * office.CurrentEfficiency;

            foreach (var pop in slavePops)
            {
                pop.Loyalty = Mathf.Min(100f, pop.Loyalty + loyaltyGain);
            }

            Debug.Log($"[GovernmentManager] Office '{office.OfficeName}' boosted slave loyalty by {loyaltyGain:F1} in city {office.TargetCityID}");
        }

        private void ExecuteTaxOptimization(int realmID, Office office)
        {
            // TODO: Implement tax optimization logic
            Debug.Log($"[GovernmentManager] Office '{office.OfficeName}' optimized tax collection (not yet implemented)");
        }

        private void ExecuteMigration(int realmID, Office office)
        {
            // TODO: Implement migration logic
            Debug.Log($"[GovernmentManager] Office '{office.OfficeName}' executed migration (not yet implemented)");
        }

        // ========== EDICTS ==========

        /// <summary>
        /// Enact an edict in a realm.
        /// </summary>
        public bool EnactEdict(int realmID, Edict edictTemplate)
        {
            if (edictTemplate == null)
            {
                Debug.LogError("[GovernmentManager] Cannot enact null edict");
                return false;
            }

            // Check if realm has enough gold for enactment cost
            if (!CanAffordEdictCost(realmID, edictTemplate.EnactmentCost))
            {
                Debug.LogWarning($"[GovernmentManager] Realm {realmID} cannot afford edict '{edictTemplate.EdictName}' (Cost: {edictTemplate.EnactmentCost})");
                return false;
            }

            if (!realmEdicts.ContainsKey(realmID))
            {
                realmEdicts[realmID] = new List<Edict>();
            }

            // Clone the template to create an active instance
            var edict = edictTemplate.Clone();
            edict.Enact(currentMonthCounter, realmID);
            realmEdicts[realmID].Add(edict);

            // Deduct enactment cost
            DeductRealmGold(realmID, edict.EnactmentCost, $"Edict: {edict.EdictName}");

            // Apply loyalty effects to populations
            ApplyEdictLoyaltyEffects(realmID, edict);

            // Register edict modifiers with ModifierManager
            RegisterEdictModifiers(realmID, edict);

            OnEdictEnacted?.Invoke(realmID, edict);
            Debug.Log($"[GovernmentManager] Edict '{edict.EdictName}' enacted in realm {realmID}");

            return true;
        }

        /// <summary>
        /// Register edict modifiers with ModifierManager as global timed modifiers.
        /// </summary>
        private void RegisterEdictModifiers(int realmID, Edict edict)
        {
            if (ModifierManager.Instance == null || edict.Modifiers == null || edict.Modifiers.Count == 0)
                return;

            // Calculate expiration in days (1 month = 30 days)
            int durationDays = edict.IsPermanent ? -1 : edict.DurationMonths * 30;

            foreach (var modifier in edict.Modifiers)
            {
                if (modifier != null)
                {
                    var modifierCopy = modifier.Clone();
                    modifierCopy.SourceID = realmID;
                    modifierCopy.SourceType = $"Edict:{edict.EdictName}";
                    modifierCopy.DurationDays = durationDays;
                    modifierCopy.AppliedOnDay = currentDayCounter;

                    // Add as global modifier (affects entire realm)
                    ModifierManager.Instance.AddGlobalTimedModifier(modifierCopy);
                }
            }

            Debug.Log($"[GovernmentManager] Registered {edict.Modifiers.Count} modifiers for edict '{edict.EdictName}' (Duration: {(edict.IsPermanent ? "permanent" : $"{durationDays} days")})");
        }

        /// <summary>
        /// Apply loyalty effects from an edict to all populations in a realm.
        /// </summary>
        private void ApplyEdictLoyaltyEffects(int realmID, Edict edict)
        {
            var realmPops = GetRealmPopulations(realmID);

            foreach (var pop in realmPops)
            {
                float loyaltyImpact = edict.GetLoyaltyImpact(pop.Archetype);
                if (Mathf.Abs(loyaltyImpact) > 0.01f)
                {
                    pop.Loyalty = Mathf.Clamp(pop.Loyalty + loyaltyImpact, 0f, 100f);
                }
            }

            if (realmPops.Count > 0)
            {
                Debug.Log($"[GovernmentManager] Applied edict '{edict.EdictName}' loyalty effects to {realmPops.Count} population groups");
            }
        }

        /// <summary>
        /// Get active edicts for a realm.
        /// </summary>
        public List<Edict> GetActiveEdicts(int realmID)
        {
            if (!realmEdicts.TryGetValue(realmID, out var edicts))
                return new List<Edict>();

            return edicts.Where(e => e.IsActive(currentMonthCounter)).ToList();
        }

        /// <summary>
        /// Get all edicts for a realm (including expired).
        /// </summary>
        public List<Edict> GetAllEdicts(int realmID)
        {
            if (realmEdicts.TryGetValue(realmID, out var edicts))
                return new List<Edict>(edicts);

            return new List<Edict>();
        }

        /// <summary>
        /// Process edict expiration and cleanup.
        /// </summary>
        private void ProcessEdictExpiration()
        {
            foreach (var kvp in realmEdicts)
            {
                int realmID = kvp.Key;
                var edicts = kvp.Value;

                // ToList() needed here because we're removing from the collection
                var expiredEdicts = edicts.Where(e => !e.IsActive(currentMonthCounter)).ToList();

                foreach (var edict in expiredEdicts)
                {
                    edicts.Remove(edict);
                    OnEdictExpired?.Invoke(realmID, edict);
                    Debug.Log($"[GovernmentManager] Edict '{edict.EdictName}' expired in realm {realmID}");
                }
            }
        }

        // ========== ECONOMY INTEGRATION ==========

        /// <summary>
        /// Check if a realm can afford a cost.
        /// </summary>
        private bool CanAffordEdictCost(int realmID, int cost)
        {
            if (cost <= 0)
                return true;

            // TODO: Implement proper realm treasury system
            // For now, get the capital city's gold (requires realm->city mapping)
            // Placeholder: always allow for testing
            return true;
        }

        /// <summary>
        /// Deduct gold from a realm's treasury.
        /// </summary>
        private void DeductRealmGold(int realmID, int amount, string reason)
        {
            if (amount <= 0)
                return;

            // TODO: Implement proper realm treasury system
            // Expected implementation:
            // 1. Get realm's capital city ID
            // 2. Use ResourceManager to deduct gold from that city
            // Example:
            // int capitalCityID = RealmManager.Instance.GetCapitalCity(realmID);
            // if (ResourceManager.Instance != null)
            // {
            //     var ledger = new ResourceLedger(capitalCityID);
            //     ledger.DailyChange[ResourceType.Gold] = -amount;
            //     ResourceManager.Instance.ApplyLedger(capitalCityID, ledger);
            // }

            Debug.Log($"[GovernmentManager] Deducted {amount} gold from realm {realmID} for {reason}");
        }

        /// <summary>
        /// Add gold to a realm's treasury.
        /// </summary>
        private void AddRealmGold(int realmID, int amount, string reason)
        {
            if (amount <= 0)
                return;

            // TODO: Implement proper realm treasury system
            // Expected implementation:
            // int capitalCityID = RealmManager.Instance.GetCapitalCity(realmID);
            // if (ResourceManager.Instance != null)
            // {
            //     var ledger = new ResourceLedger(capitalCityID);
            //     ledger.DailyChange[ResourceType.Gold] = amount;
            //     ResourceManager.Instance.ApplyLedger(capitalCityID, ledger);
            // }

            Debug.Log($"[GovernmentManager] Added {amount} gold to realm {realmID} from {reason}");
        }

        // ========== LEGITIMACY & CAPACITY ==========
        // DESIGN NOTE: Legitimacy and Capacity are NOT directly purchasable by the player!
        // They change through gameplay mechanics:
        // - Buildings provide permanent modifiers
        // - Edicts provide temporary modifiers (cost gold to enact)
        // - Culture/religion alignment affects legitimacy automatically
        // - Office efficiency affects capacity
        // - Reforms change base values but cost gold and may reduce legitimacy
        // - Wars, events, and policies all indirectly affect these stats
        // ModifyLegitimacy/ModifyCapacity are INTERNAL methods called by game systems only!

        /// <summary>
        /// Get a realm's legitimacy (0-100).
        /// </summary>
        public float GetRealmLegitimacy(int realmID)
        {
            if (realmLegitimacy.TryGetValue(realmID, out var legitimacy))
                return legitimacy;

            return 50f; // Default
        }

        /// <summary>
        /// Modify a realm's legitimacy.
        /// </summary>
        public void ModifyLegitimacy(int realmID, float delta)
        {
            float current = GetRealmLegitimacy(realmID);
            float newValue = Mathf.Clamp(current + delta, 0f, 100f);
            realmLegitimacy[realmID] = newValue;

            OnLegitimacyChanged?.Invoke(realmID, newValue);

            if (Mathf.Abs(delta) > 0.1f)
            {
                Debug.Log($"[GovernmentManager] Realm {realmID} legitimacy changed by {delta:F1} to {newValue:F1}");
            }
        }

        /// <summary>
        /// Get a realm's capacity (0-100).
        /// </summary>
        public float GetRealmCapacity(int realmID)
        {
            if (realmCapacity.TryGetValue(realmID, out var capacity))
                return capacity;

            return 30f; // Default
        }

        /// <summary>
        /// Modify a realm's capacity.
        /// </summary>
        public void ModifyCapacity(int realmID, float delta)
        {
            float current = GetRealmCapacity(realmID);
            float newValue = Mathf.Clamp(current + delta, 0f, 100f);
            realmCapacity[realmID] = newValue;

            OnCapacityChanged?.Invoke(realmID, newValue);

            if (Mathf.Abs(delta) > 0.1f)
            {
                Debug.Log($"[GovernmentManager] Realm {realmID} capacity changed by {delta:F1} to {newValue:F1}");
            }
        }

        // ========== CULTURE & RELIGION INTEGRATION ==========

        /// <summary>
        /// Calculate legitimacy bonus from culture alignment between ruler and population.
        /// </summary>
        private float CalculateCultureLegitimacyBonus(int realmID)
        {
            if (TWK.Cultures.CultureManager.Instance == null)
                return 0f;

            // Get realm leader's culture
            int leaderCultureID = TWK.Cultures.CultureManager.Instance.GetRealmLeaderCulture(realmID);
            if (leaderCultureID == -1)
                return 0f;

            // Get all population groups in realm
            var realmPops = GetRealmPopulations(realmID);
            if (realmPops.Count == 0)
                return 0f;

            // Calculate culture match percentage
            long totalPops = realmPops.Sum(p => (long)p.PopulationCount);
            long matchingPops = realmPops
                .Where(p => p.Culture != null && p.Culture.GetCultureID() == leaderCultureID)
                .Sum(p => (long)p.PopulationCount);

            float matchPercentage = totalPops > 0 ? (float)matchingPops / totalPops : 0f;

            // Legitimacy bonus: +10 for perfect match, -10 for complete mismatch
            float bonus = (matchPercentage - 0.5f) * 20f;

            return bonus;
        }

        /// <summary>
        /// Calculate legitimacy bonus from clergy happiness (important for theocracies).
        /// </summary>
        private float CalculateClergyLegitimacyBonus(int realmID)
        {
            var government = GetRealmGovernment(realmID);
            if (government == null)
                return 0f;

            // Check if this is a theocratic government
            bool isTheocracy = government.RegimeForm == RegimeForm.Autocratic &&
                              government.Institutions.Any(i => i != null &&
                                  !string.IsNullOrEmpty(i.InstitutionName) &&
                                  i.InstitutionName.Contains("Theocracy"));

            // Get clergy population groups in realm
            var realmPops = GetRealmPopulations(realmID);
            var clergyPops = realmPops.Where(p => p.Archetype == PopulationArchetypes.Clergy).ToList();

            if (clergyPops.Count == 0)
                return 0f;

            // Calculate average clergy happiness
            float avgHappiness = clergyPops.Average(p => p.Happiness);

            // Theocracies get stronger bonus/penalty from clergy happiness
            float multiplier = isTheocracy ? 0.2f : 0.1f;

            // Happiness ranges from -100 to 100, so this gives -20 to +20 for theocracy, -10 to +10 for others
            float bonus = avgHappiness * multiplier;

            return bonus;
        }

        /// <summary>
        /// Apply periodic legitimacy modifiers from culture and religion.
        /// Called yearly.
        /// </summary>
        private void ApplyCultureAndReligionLegitimacy()
        {
            foreach (var realmID in activeGovernments.Keys)
            {
                float cultureBonus = CalculateCultureLegitimacyBonus(realmID);
                float clergyBonus = CalculateClergyLegitimacyBonus(realmID);

                float totalBonus = cultureBonus + clergyBonus;

                if (Mathf.Abs(totalBonus) > 0.1f)
                {
                    ModifyLegitimacy(realmID, totalBonus);
                    Debug.Log($"[GovernmentManager] Realm {realmID} received culture/religion legitimacy: {totalBonus:F1} (culture: {cultureBonus:F1}, clergy: {clergyBonus:F1})");
                }
            }
        }

        // ========== REFORM SYSTEM ==========

        /// <summary>
        /// Calculate the cost of reforming to a new government.
        /// </summary>
        public int CalculateReformCost(int realmID, GovernmentData targetGovernment)
        {
            var currentGov = GetRealmGovernment(realmID);
            if (currentGov == null || targetGovernment == null)
                return 0;

            int baseCost = 1000;

            // Institution changes
            int institutionCost = CalculateInstitutionChangeCost(currentGov, targetGovernment);

            // Law distance
            int lawCost = CalculateLawDistanceCost(currentGov, targetGovernment);

            // Local resistance based on legitimacy
            float legitimacy = GetRealmLegitimacy(realmID);
            float resistanceMultiplier = (100f - legitimacy) / 100f;
            int resistanceCost = (int)(baseCost * resistanceMultiplier);

            int totalCost = baseCost + institutionCost + lawCost + resistanceCost;

            return totalCost;
        }

        private int CalculateInstitutionChangeCost(GovernmentData current, GovernmentData target)
        {
            int cost = 0;

            // Institutions being added
            foreach (var institution in target.Institutions)
            {
                if (institution != null && !current.Institutions.Contains(institution))
                {
                    cost += institution.ReformCost;
                }
            }

            return cost;
        }

        private int CalculateLawDistanceCost(GovernmentData current, GovernmentData target)
        {
            int distance = 0;

            if (current.RegimeForm != target.RegimeForm) distance += 2;
            if (current.StateStructure != target.StateStructure) distance += 3;
            if (current.SuccessionLaw != target.SuccessionLaw) distance += 1;
            if (current.Administration != target.Administration) distance += 1;
            if (current.MilitaryService != target.MilitaryService) distance += 1;
            if (current.TaxationLaw != target.TaxationLaw) distance += 1;
            if (current.TradeLaw != target.TradeLaw) distance += 1;
            if (current.JusticeLaw != target.JusticeLaw) distance += 1;

            return distance * 100; // 100 gold per "step"
        }

        /// <summary>
        /// Check if a realm can reform to a target government.
        /// </summary>
        public bool CanReform(int realmID, GovernmentData targetGovernment)
        {
            if (targetGovernment == null)
                return false;

            // Check minimum provinces (cities)
            // TODO: Get realm cities properly
            // For now, assume check passes

            // Check capacity requirement
            float capacity = GetRealmCapacity(realmID);
            if (capacity < targetGovernment.MinimumCapacity)
                return false;

            // Check required institutions
            var currentGov = GetRealmGovernment(realmID);
            if (currentGov != null)
            {
                foreach (var requiredInst in targetGovernment.RequiredInstitutions)
                {
                    if (requiredInst != null && !currentGov.Institutions.Contains(requiredInst))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reform a realm's government.
        /// </summary>
        public bool ReformGovernment(int realmID, GovernmentData targetGovernment)
        {
            if (!CanReform(realmID, targetGovernment))
            {
                Debug.LogWarning($"[GovernmentManager] Realm {realmID} cannot reform to {targetGovernment.GovernmentName}");
                return false;
            }

            int cost = CalculateReformCost(realmID, targetGovernment);

            // TODO: Check if realm has enough resources
            // TODO: Deduct resources

            SetRealmGovernment(realmID, targetGovernment);

            Debug.Log($"[GovernmentManager] Realm {realmID} reformed to {targetGovernment.GovernmentName} (Cost: {cost})");
            return true;
        }

        // ========== REVOLT SYSTEM ==========

        /// <summary>
        /// Calculate revolt risk for a realm (0-100).
        /// </summary>
        public float CalculateRevoltRisk(int realmID)
        {
            float risk = 0f;

            // Low legitimacy increases risk
            float legitimacy = GetRealmLegitimacy(realmID);
            risk += (100f - legitimacy) * 0.5f; // Max 50% from legitimacy

            // TODO: Add more factors:
            // - Low population loyalty
            // - Cultural/religious mismatch
            // - Harsh edicts
            // - Exploitative contracts

            return Mathf.Clamp(risk, 0f, 100f);
        }

        /// <summary>
        /// Trigger a revolt in a realm.
        /// </summary>
        public void TriggerRevolt(int realmID, RevoltType revoltType)
        {
            OnRevoltTriggered?.Invoke(realmID, revoltType);
            Debug.Log($"[GovernmentManager] {revoltType} triggered in realm {realmID}!");

            // Immediate legitimacy penalty
            ModifyLegitimacy(realmID, -10f);

            // TODO: Implement revolt consequences
            // - Create rebel faction
            // - Spawn rebel armies
            // - Economic disruption
        }

        private RevoltType DetermineRevoltType(GovernmentData government)
        {
            if (government == null)
                return RevoltType.CivicTumult;

            return government.RegimeForm switch
            {
                RegimeForm.Autocratic => RevoltType.PretenderWar,
                RegimeForm.Pluralist => RevoltType.CivicTumult,
                RegimeForm.Chiefdom => RevoltType.ClanRising,
                RegimeForm.Confederation => RevoltType.MemberSecession,
                _ => RevoltType.CivicTumult
            };
        }

        /// <summary>
        /// Process revolt checks for all realms.
        /// </summary>
        private void ProcessRevoltChecks()
        {
            foreach (var realmID in activeGovernments.Keys)
            {
                float risk = CalculateRevoltRisk(realmID);

                if (risk > 75f)
                {
                    // High chance of revolt
                    if (UnityEngine.Random.value < 0.3f) // 30% chance per season
                    {
                        var gov = GetRealmGovernment(realmID);
                        RevoltType type = DetermineRevoltType(gov);
                        TriggerRevolt(realmID, type);
                    }
                }
            }
        }

        // ========== MODIFIER ACCESS ==========

        /// <summary>
        /// Get all modifiers from a realm's government.
        /// </summary>
        public List<Modifier> GetGovernmentModifiers(int realmID)
        {
            var modifiers = new List<Modifier>();

            // Government institution modifiers
            var government = GetRealmGovernment(realmID);
            if (government != null)
            {
                modifiers.AddRange(government.GetAllModifiers());
            }

            // Active edict modifiers
            var edicts = GetActiveEdicts(realmID);
            foreach (var edict in edicts)
            {
                foreach (var modifier in edict.Modifiers)
                {
                    if (modifier != null)
                    {
                        var tagged = modifier.Clone();
                        tagged.SourceID = realmID;
                        tagged.SourceType = "Edict";
                        modifiers.Add(tagged);
                    }
                }
            }

            return modifiers;
        }

        // ========== SIMULATION TICKS ==========

        public void AdvanceDay()
        {
            currentDayCounter++;

            // Process office automation daily
            ProcessOfficeAutomation();
        }

        public void AdvanceSeason()
        {
            currentMonthCounter += 3; // Season = 3 months

            // Process edict expiration
            ProcessEdictExpiration();

            // Check for revolts
            ProcessRevoltChecks();
        }

        public void AdvanceYear()
        {
            // Apply culture and religion legitimacy effects
            ApplyCultureAndReligionLegitimacy();

            // Yearly legitimacy decay
            // ToList() needed because ModifyLegitimacy could potentially add new keys
            foreach (var realmID in realmLegitimacy.Keys.ToList())
            {
                ModifyLegitimacy(realmID, -2f); // Slow natural decay
            }
        }
    }
}
