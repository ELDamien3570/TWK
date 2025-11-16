using System.Collections.Generic;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Realms.Demographics;
using TWK.Modifiers;
using System;

namespace TWK.Religion
{
    /// <summary>
    /// Manages religions, conversions, fervor, and religious mechanics.
    /// Handles seasonal conversion, holy land bonuses, festivals, and rituals.
    /// </summary>
    public class ReligionManager : MonoBehaviour, ISimulationAgent
    {
        public static ReligionManager Instance { get; private set; }

        // ========== RELIGION REGISTRY ==========

        /// <summary>
        /// All available religions in the game.
        /// Key: Stable Religion ID (from GetStableReligionID())
        /// </summary>
        private Dictionary<int, ReligionData> religionRegistry = new Dictionary<int, ReligionData>();

        // ========== HOLY LAND TRACKING ==========

        /// <summary>
        /// Tracks which realm controls which holy lands.
        /// Key: Province ID, Value: Controlling Realm ID
        /// </summary>
        private Dictionary<int, int> holyLandControl = new Dictionary<int, int>();

        // ========== FESTIVAL TRACKING ==========

        /// <summary>
        /// Tracks active festivals by city.
        /// Key: City ID, Value: Active festival
        /// </summary>
        private Dictionary<int, Festival> activeFestivals = new Dictionary<int, Festival>();

        // ========== RITUAL COOLDOWNS ==========

        /// <summary>
        /// Tracks ritual cooldowns by religion.
        /// Key: (Religion ID, Ritual Name), Value: Day when cooldown expires
        /// </summary>
        private Dictionary<(int, string), int> ritualCooldowns = new Dictionary<(int, string), int>();

        private WorldTimeManager worldTimeManager;
        private int globalDayCounter = 0;
        private int currentSeason = 0; // 0=Spring, 1=Summer, 2=Fall, 3=Winter

        // ========== EVENTS ==========

        public delegate void ReligionEventHandler(int religionID);
        public delegate void ConversionEventHandler(int popGroupID, int oldReligionID, int newReligionID);
        public delegate void FestivalEventHandler(int cityID, Festival festival);

        public event ReligionEventHandler OnReligionCreated;
        public event ConversionEventHandler OnPopulationConverted;
        public event FestivalEventHandler OnFestivalStarted;
        public event FestivalEventHandler OnFestivalEnded;

        public event Action newReligionRegistered;

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

            Debug.Log("[ReligionManager] Initialized");
        }

        // ========== RELIGION REGISTRATION ==========

        /// <summary>
        /// Register a religion in the game.
        /// </summary>
        public void RegisterReligion(ReligionData religion)
        {
            if (religion == null)
            {
                Debug.LogError("[ReligionManager] Cannot register null religion");
                return;
            }

            int religionID = religion.GetStableReligionID();

            if (religionRegistry.ContainsKey(religionID))
            {
                Debug.LogWarning($"[ReligionManager] Religion '{religion.ReligionName}' already registered");
                return;
            }

            religionRegistry[religionID] = religion;
            OnReligionCreated?.Invoke(religionID);
            newReligionRegistered?.Invoke();

            Debug.Log($"[ReligionManager] Registered religion: {religion.ReligionName} (ID: {religionID})");
        }

        /// <summary>
        /// Get a religion by its stable ID.
        /// </summary>
        public ReligionData GetReligion(int religionID)
        {
            if (religionRegistry.TryGetValue(religionID, out var religion))
                return religion;

            Debug.LogWarning($"[ReligionManager] Religion with ID {religionID} not found");
            return null;
        }

        /// <summary>
        /// Get all registered religions.
        /// </summary>
        public List<ReligionData> GetAllReligions()
        {
            return new List<ReligionData>(religionRegistry.Values);
        }

        // ========== SIMULATION TICKS ==========

        public void AdvanceDay()
        {
            globalDayCounter++;
            ProcessFestivalExpiration();
        }

        public void AdvanceSeason()
        {
            currentSeason = (currentSeason + 1) % 4;
            ProcessSeasonalConversion();
            ProcessFervorDecay();
            CheckForFestivals();
        }

        public void AdvanceYear() { }

        // ========== CONVERSION MECHANICS ==========

        /// <summary>
        /// Process seasonal religious conversion for all population groups.
        /// Called every season.
        /// </summary>
        private void ProcessSeasonalConversion()
        {
            if (PopulationManager.Instance == null)
                return;

            var allPops = PopulationManager.Instance.GetAllPopulationGroups();

            foreach (var pop in allPops)
            {
                ProcessPopulationConversion(pop);
            }
        }

        /// <summary>
        /// Process conversion for a single population group.
        /// Conversion is influenced by:
        /// - City's dominant religion
        /// - Population's current fervor (resistance)
        /// - Religion's conversion speed
        /// - Religion's evangelism stance
        /// </summary>
        private void ProcessPopulationConversion(PopulationGroup pop)
        {
            if (pop == null || pop.CurrentReligion == null)
                return;

            // TODO: Get city's dominant religion
            // For now, we'll skip conversion logic until we have city religion tracking
            // This will be implemented when we add religion tracking to CityData

            // Placeholder for future conversion logic:
            // 1. Check if city has a different dominant religion
            // 2. Calculate conversion chance based on fervor, evangelism, conversion speed
            // 3. Roll for conversion
            // 4. If converting, create new pop group with new religion (similar to culture conversion)
        }

        /// <summary>
        /// Manually convert a population group to a new religion.
        /// Used for scripted conversions (ruler conversion, events, etc.)
        /// </summary>
        public void ConvertPopulation(PopulationGroup pop, ReligionData newReligion)
        {
            if (pop == null || newReligion == null)
                return;

            var oldReligionID = pop.CurrentReligion?.GetStableReligionID() ?? -1;
            var newReligionID = newReligion.GetStableReligionID();

            pop.CurrentReligion = newReligion;
            pop.Fervor = newReligion.BaseFervor; // Reset fervor to new religion's base

            OnPopulationConverted?.Invoke(pop.ID, oldReligionID, newReligionID);

            Debug.Log($"[ReligionManager] Pop {pop.ID} converted to {newReligion.ReligionName}");
        }

        // ========== FERVOR MECHANICS ==========

        /// <summary>
        /// Process seasonal fervor decay for all population groups.
        /// </summary>
        private void ProcessFervorDecay()
        {
            if (PopulationManager.Instance == null)
                return;

            var allPops = PopulationManager.Instance.GetAllPopulationGroups();

            foreach (var pop in allPops)
            {
                if (pop?.CurrentReligion == null)
                    continue;

                // Decay fervor based on religion's decay rate
                float decayRate = pop.CurrentReligion.FervorDecayRate;
                pop.Fervor = Mathf.Max(0f, pop.Fervor - decayRate);
            }
        }

        /// <summary>
        /// Gain fervor for a population group.
        /// Called from events, rituals, festivals, etc.
        /// </summary>
        public void GainFervor(PopulationGroup pop, float amount)
        {
            if (pop == null)
                return;

            pop.Fervor = Mathf.Min(100f, pop.Fervor + amount);
        }

        // ========== HOLY LAND MECHANICS ==========

        /// <summary>
        /// Set control of a holy land province.
        /// </summary>
        public void SetHolyLandControl(int provinceID, int realmID)
        {
            holyLandControl[provinceID] = realmID;
            Debug.Log($"[ReligionManager] Realm {realmID} now controls holy land province {provinceID}");
        }

        /// <summary>
        /// Check if a realm controls a specific holy land.
        /// </summary>
        public bool DoesRealmControlHolyLand(int realmID, HolyLand holyLand)
        {
            if (holyLand.ProvinceID == -1)
                return false;

            if (holyLandControl.TryGetValue(holyLand.ProvinceID, out int controllingRealm))
                return controllingRealm == realmID;

            return false;
        }

        /// <summary>
        /// Get fervor bonus from controlling holy lands for a specific religion.
        /// </summary>
        public float GetHolyLandFervorBonus(int realmID, ReligionData religion)
        {
            if (religion == null)
                return 0f;

            float totalBonus = 0f;

            foreach (var holyLand in religion.GetAllHolyLands())
            {
                if (DoesRealmControlHolyLand(realmID, holyLand))
                {
                    totalBonus += holyLand.FervorBonus;
                }
            }

            return totalBonus;
        }

        // ========== FESTIVAL MECHANICS ==========

        /// <summary>
        /// Check for festivals that should start this season.
        /// </summary>
        private void CheckForFestivals()
        {
            // TODO: Implement when we have city religion tracking
            // For each city:
            // 1. Get city's dominant religion
            // 2. Check if any festivals should occur this season
            // 3. Start festival if applicable
        }

        /// <summary>
        /// Start a festival in a city.
        /// </summary>
        public void StartFestival(int cityID, Festival festival)
        {
            if (festival == null)
                return;

            activeFestivals[cityID] = festival;
            OnFestivalStarted?.Invoke(cityID, festival);

            Debug.Log($"[ReligionManager] Festival '{festival.Name}' started in city {cityID}");
        }

        /// <summary>
        /// End a festival in a city.
        /// </summary>
        public void EndFestival(int cityID)
        {
            if (activeFestivals.TryGetValue(cityID, out var festival))
            {
                activeFestivals.Remove(cityID);
                OnFestivalEnded?.Invoke(cityID, festival);

                Debug.Log($"[ReligionManager] Festival '{festival.Name}' ended in city {cityID}");
            }
        }

        /// <summary>
        /// Check if a city has an active festival.
        /// </summary>
        public bool HasActiveFestival(int cityID)
        {
            return activeFestivals.ContainsKey(cityID);
        }

        /// <summary>
        /// Get the active festival for a city.
        /// </summary>
        public Festival GetActiveFestival(int cityID)
        {
            if (activeFestivals.TryGetValue(cityID, out var festival))
                return festival;

            return null;
        }

        /// <summary>
        /// Process festival expiration (festivals last 1 season).
        /// </summary>
        private void ProcessFestivalExpiration()
        {
            // Festivals are seasonal, so they end automatically at season change
            // This is handled in AdvanceSeason by clearing the activeFestivals dict
        }

        // ========== RITUAL MECHANICS ==========

        /// <summary>
        /// Perform a ritual.
        /// </summary>
        public bool PerformRitual(ReligionData religion, Ritual ritual, int performerID)
        {
            if (religion == null || ritual == null)
                return false;

            int religionID = religion.GetStableReligionID();
            var cooldownKey = (religionID, ritual.Name);

            // Check cooldown
            if (ritualCooldowns.TryGetValue(cooldownKey, out int cooldownEndDay))
            {
                if (globalDayCounter < cooldownEndDay)
                {
                    Debug.Log($"[ReligionManager] Ritual '{ritual.Name}' is on cooldown until day {cooldownEndDay}");
                    return false;
                }
            }

            // TODO: Check piety cost when we have character/realm piety tracking

            // Apply ritual effects
            ApplyRitualEffects(ritual, performerID);

            // Set cooldown
            ritualCooldowns[cooldownKey] = globalDayCounter + ritual.Cooldown;

            Debug.Log($"[ReligionManager] Ritual '{ritual.Name}' performed by {performerID}");
            return true;
        }

        /// <summary>
        /// Apply the effects of a ritual.
        /// </summary>
        private void ApplyRitualEffects(Ritual ritual, int performerID)
        {
            if (ritual.Effect == null)
                return;

            // TODO: Apply ritual effects to appropriate targets
            // - Fervor gain to population
            // - Happiness/stability to city
            // - Prestige to character
            // - Trigger events

            Debug.Log($"[ReligionManager] Applied ritual effects: Fervor +{ritual.Effect.FervorGain}, Happiness +{ritual.Effect.HappinessGain}");
        }

        /// <summary>
        /// Check if a ritual is on cooldown.
        /// </summary>
        public bool IsRitualOnCooldown(ReligionData religion, Ritual ritual)
        {
            if (religion == null || ritual == null)
                return false;

            int religionID = religion.GetStableReligionID();
            var cooldownKey = (religionID, ritual.Name);

            if (ritualCooldowns.TryGetValue(cooldownKey, out int cooldownEndDay))
            {
                return globalDayCounter < cooldownEndDay;
            }

            return false;
        }

        /// <summary>
        /// Get days remaining on a ritual cooldown.
        /// </summary>
        public int GetRitualCooldownDays(ReligionData religion, Ritual ritual)
        {
            if (religion == null || ritual == null)
                return 0;

            int religionID = religion.GetStableReligionID();
            var cooldownKey = (religionID, ritual.Name);

            if (ritualCooldowns.TryGetValue(cooldownKey, out int cooldownEndDay))
            {
                return Mathf.Max(0, cooldownEndDay - globalDayCounter);
            }

            return 0;
        }
    }
}
