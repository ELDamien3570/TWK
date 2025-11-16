using System.Collections.Generic;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Economy;
using TWK.Realms.Demographics;
using TWK.Cultures;

namespace TWK.Modifiers
{
    /// <summary>
    /// Manages timed modifiers and provides utility methods for calculating modifier totals.
    /// Permanent modifiers (from culture, religion, buildings) are queried directly from their sources.
    /// </summary>
    public class ModifierManager : MonoBehaviour, ISimulationAgent
    {
        public static ModifierManager Instance { get; private set; }

        // ========== TIMED MODIFIERS ==========
        // Stores modifiers that expire after X days (from events, temporary effects, etc.)

        /// <summary>
        /// Timed modifiers affecting cities.
        /// Key: CityID, Value: List of active timed modifiers
        /// </summary>
        private Dictionary<int, List<Modifier>> cityTimedModifiers = new Dictionary<int, List<Modifier>>();

        /// <summary>
        /// Timed modifiers affecting population groups.
        /// Key: PopGroupID, Value: List of active timed modifiers
        /// </summary>
        private Dictionary<int, List<Modifier>> popGroupTimedModifiers = new Dictionary<int, List<Modifier>>();

        /// <summary>
        /// Timed modifiers affecting buildings.
        /// Key: BuildingID, Value: List of active timed modifiers
        /// </summary>
        private Dictionary<int, List<Modifier>> buildingTimedModifiers = new Dictionary<int, List<Modifier>>();

        /// <summary>
        /// Global timed modifiers affecting the entire realm.
        /// </summary>
        private List<Modifier> globalTimedModifiers = new List<Modifier>();

        private WorldTimeManager worldTimeManager;

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

            Debug.Log("[ModifierManager] Initialized");
        }

        // ========== SIMULATION TICKS ==========

        public void AdvanceDay()
        {
            int currentDay = worldTimeManager.CurrentDay;

            // Clean up expired city modifiers
            CleanupExpiredModifiers(cityTimedModifiers, currentDay);

            // Clean up expired pop group modifiers
            CleanupExpiredModifiers(popGroupTimedModifiers, currentDay);

            // Clean up expired building modifiers
            CleanupExpiredModifiers(buildingTimedModifiers, currentDay);

            // Clean up expired global modifiers
            globalTimedModifiers.RemoveAll(m => !m.IsActive(currentDay));
        }

        public void AdvanceSeason() { }
        public void AdvanceYear() { }

        // ========== ADDING TIMED MODIFIERS ==========

        /// <summary>
        /// Add a timed modifier to a city.
        /// </summary>
        public void AddTimedModifier(int cityID, Modifier modifier)
        {
            if (modifier.Duration != ModifierDuration.Timed)
            {
                Debug.LogWarning($"[ModifierManager] Trying to add non-timed modifier '{modifier.Name}' as timed modifier");
                return;
            }

            var instance = modifier.Clone();
            instance.AppliedOnDay = worldTimeManager.CurrentDay;

            if (!cityTimedModifiers.ContainsKey(cityID))
                cityTimedModifiers[cityID] = new List<Modifier>();

            cityTimedModifiers[cityID].Add(instance);

            Debug.Log($"[ModifierManager] Added timed modifier '{modifier.Name}' to city {cityID} for {modifier.DurationDays} days");
        }

        /// <summary>
        /// Add a timed modifier to a population group.
        /// </summary>
        public void AddTimedModifier(int popGroupID, Modifier modifier, bool isPopGroup)
        {
            if (modifier.Duration != ModifierDuration.Timed)
            {
                Debug.LogWarning($"[ModifierManager] Trying to add non-timed modifier '{modifier.Name}' as timed modifier");
                return;
            }

            var instance = modifier.Clone();
            instance.AppliedOnDay = worldTimeManager.CurrentDay;

            if (!popGroupTimedModifiers.ContainsKey(popGroupID))
                popGroupTimedModifiers[popGroupID] = new List<Modifier>();

            popGroupTimedModifiers[popGroupID].Add(instance);

            Debug.Log($"[ModifierManager] Added timed modifier '{modifier.Name}' to pop group {popGroupID} for {modifier.DurationDays} days");
        }

        /// <summary>
        /// Add a global timed modifier affecting the entire realm.
        /// </summary>
        public void AddGlobalTimedModifier(Modifier modifier)
        {
            if (modifier.Duration != ModifierDuration.Timed)
            {
                Debug.LogWarning($"[ModifierManager] Trying to add non-timed modifier '{modifier.Name}' as timed modifier");
                return;
            }

            var instance = modifier.Clone();
            instance.AppliedOnDay = worldTimeManager.CurrentDay;

            globalTimedModifiers.Add(instance);

            Debug.Log($"[ModifierManager] Added global timed modifier '{modifier.Name}' for {modifier.DurationDays} days");
        }

        // ========== QUERYING MODIFIERS ==========

        /// <summary>
        /// Get all active timed modifiers for a city.
        /// </summary>
        public List<Modifier> GetCityTimedModifiers(int cityID)
        {
            if (!cityTimedModifiers.TryGetValue(cityID, out var modifiers))
                return new List<Modifier>();

            return new List<Modifier>(modifiers);
        }

        /// <summary>
        /// Get all active timed modifiers for a population group.
        /// </summary>
        public List<Modifier> GetPopGroupTimedModifiers(int popGroupID)
        {
            if (!popGroupTimedModifiers.TryGetValue(popGroupID, out var modifiers))
                return new List<Modifier>();

            return new List<Modifier>(modifiers);
        }

        /// <summary>
        /// Get all active global timed modifiers.
        /// </summary>
        public List<Modifier> GetGlobalTimedModifiers()
        {
            return new List<Modifier>(globalTimedModifiers);
        }

        // ========== UTILITY METHODS ==========

        /// <summary>
        /// Calculate the total modifier value for a specific effect type.
        /// Combines both flat and percentage modifiers.
        /// </summary>
        /// <param name="modifiers">List of modifiers to calculate from</param>
        /// <param name="effectType">Type of effect to calculate</param>
        /// <param name="baseValue">Base value before modifiers</param>
        /// <returns>Final value after applying all modifiers</returns>
        public static float CalculateModifiedValue(List<Modifier> modifiers, ModifierEffectType effectType, float baseValue)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(effectType);
                foreach (var effect in effects)
                {
                    if (effect.IsPercentage)
                    {
                        percentageMultiplier += effect.Value / 100f;
                    }
                    else
                    {
                        flatBonus += effect.Value;
                    }
                }
            }

            // Apply: (base + flat) * percentage
            return (baseValue + flatBonus) * percentageMultiplier;
        }

        /// <summary>
        /// Calculate the total flat modifier value for a specific effect type.
        /// </summary>
        public static float CalculateFlatModifier(List<Modifier> modifiers, ModifierEffectType effectType)
        {
            float total = 0f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(effectType);
                foreach (var effect in effects)
                {
                    if (!effect.IsPercentage)
                    {
                        total += effect.Value;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// Calculate the total percentage multiplier for a specific effect type.
        /// Returns the multiplier (1.0 = no change, 1.5 = +50%, 0.8 = -20%)
        /// </summary>
        public static float CalculatePercentageMultiplier(List<Modifier> modifiers, ModifierEffectType effectType)
        {
            float multiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(effectType);
                foreach (var effect in effects)
                {
                    if (effect.IsPercentage)
                    {
                        multiplier += effect.Value / 100f;
                    }
                }
            }

            return multiplier;
        }

        // ========== CLEANUP ==========

        private void CleanupExpiredModifiers<T>(Dictionary<T, List<Modifier>> modifierDict, int currentDay)
        {
            var keysToRemove = new List<T>();

            foreach (var kvp in modifierDict)
            {
                // Remove expired modifiers
                kvp.Value.RemoveAll(m => !m.IsActive(currentDay));

                // Mark empty lists for removal
                if (kvp.Value.Count == 0)
                    keysToRemove.Add(kvp.Key);
            }

            // Remove empty entries
            foreach (var key in keysToRemove)
            {
                modifierDict.Remove(key);
            }
        }
    }
}
