using System;
using System.Collections.Generic;
using UnityEngine;  

namespace TWK.Realms.Demographics
{
    /// <summary>
    /// Unified worker slot definition for buildings.
    /// Consolidates all worker requirements and modifiers for a single archetype.
    /// </summary>
    [Serializable]
    public class WorkerSlot
    {
        [Header("Archetype")]
        public PopulationArchetypes Archetype;

        [Header("Count Requirements")]
        [Tooltip("Minimum workers of this type required (0 = optional)")]
        public int MinCount = 0;

        [Tooltip("Maximum workers of this type allowed (0 = no limit)")]
        public int MaxCount = 0;

        [Header("Efficiency")]
        [Tooltip("Efficiency multiplier for this archetype (1.0 = normal, 1.5 = 50% more efficient, 0.5 = 50% penalty)")]
        public float EfficiencyMultiplier = 1.0f;

        [Header("Slot Type")]
        [Tooltip("Is this archetype required for the building to function?")]
        public bool IsRequired = false;

        [Header("Population Effects")]
        [Tooltip("Education increase per day per worker of this type")]
        public float EducationGrowthPerWorker = 0f;

        [Tooltip("Wealth increase per day per worker of this type")]
        public float WealthGrowthPerWorker = 0f;

        /// <summary>
        /// Check if this slot is optional (not required).
        /// </summary>
        public bool IsOptional => !IsRequired;

        /// <summary>
        /// Check if this slot has a capacity limit.
        /// </summary>
        public bool HasMaxLimit => MaxCount > 0;

        /// <summary>
        /// Get the actual max count (returns int.MaxValue if no limit).
        /// </summary>
        public int GetEffectiveMaxCount()
        {
            return HasMaxLimit ? MaxCount : int.MaxValue;
        }
    }

    // ========== EXTENSION METHODS ==========

    /// <summary>
    /// Extension methods for working with worker slots.
    /// </summary>
    public static class WorkerSlotExtensions
    {
        /// <summary>
        /// Get total minimum workers required across all slots.
        /// </summary>
        public static int GetTotalMinWorkers(this List<WorkerSlot> slots)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                total += slot.MinCount;
            }
            return total;
        }

        /// <summary>
        /// Get total maximum workers allowed across all slots.
        /// Returns int.MaxValue if any slot has no limit.
        /// </summary>
        public static int GetTotalMaxWorkers(this List<WorkerSlot> slots)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (!slot.HasMaxLimit)
                    return int.MaxValue; // No limit
                total += slot.MaxCount;
            }
            return total;
        }

        /// <summary>
        /// Get the worker slot for a specific archetype.
        /// </summary>
        public static WorkerSlot GetSlot(this List<WorkerSlot> slots, PopulationArchetypes archetype)
        {
            foreach (var slot in slots)
            {
                if (slot.Archetype == archetype)
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Check if an archetype is allowed in any worker slot.
        /// </summary>
        public static bool IsArchetypeAllowed(this List<WorkerSlot> slots, PopulationArchetypes archetype)
        {
            return slots.GetSlot(archetype) != null;
        }

        /// <summary>
        /// Get efficiency multiplier for an archetype from worker slots.
        /// </summary>
        public static float GetEfficiency(this List<WorkerSlot> slots, PopulationArchetypes archetype, float defaultValue = 1f)
        {
            var slot = slots.GetSlot(archetype);
            return slot != null ? slot.EfficiencyMultiplier : defaultValue;
        }

        /// <summary>
        /// Get all required worker slots.
        /// </summary>
        public static List<WorkerSlot> GetRequiredSlots(this List<WorkerSlot> slots)
        {
            var required = new List<WorkerSlot>();
            foreach (var slot in slots)
            {
                if (slot.IsRequired)
                    required.Add(slot);
            }
            return required;
        }

        /// <summary>
        /// Get all optional worker slots.
        /// </summary>
        public static List<WorkerSlot> GetOptionalSlots(this List<WorkerSlot> slots)
        {
            var optional = new List<WorkerSlot>();
            foreach (var slot in slots)
            {
                if (slot.IsOptional)
                    optional.Add(slot);
            }
            return optional;
        }
    }
}
