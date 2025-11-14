using System;
using System.Collections.Generic;

namespace TWK.Realms.Demographics
{
    /// <summary>
    /// Represents a worker requirement for a specific population archetype.
    /// Used in BuildingDefinition for Unity Inspector editing.
    /// </summary>
    [Serializable]
    public struct WorkerRequirement
    {
        public PopulationArchetypes Archetype;
        public int Count;

        public WorkerRequirement(PopulationArchetypes archetype, int count)
        {
            Archetype = archetype;
            Count = count;
        }
    }

    /// <summary>
    /// Represents a worker efficiency modifier for a specific population archetype.
    /// Used in BuildingDefinition for Unity Inspector editing.
    /// </summary>
    [Serializable]
    public struct WorkerEfficiencyModifier
    {
        public PopulationArchetypes Archetype;
        public float Multiplier;

        public WorkerEfficiencyModifier(PopulationArchetypes archetype, float multiplier)
        {
            Archetype = archetype;
            Multiplier = multiplier;
        }
    }

    /// <summary>
    /// Extension methods for working with worker-related lists.
    /// </summary>
    public static class WorkerExtensions
    {
        /// <summary>
        /// Convert a list of WorkerRequirement to a Dictionary.
        /// </summary>
        public static Dictionary<PopulationArchetypes, int> ToDictionary(this List<WorkerRequirement> list)
        {
            var dict = new Dictionary<PopulationArchetypes, int>();
            foreach (var item in list)
            {
                if (!dict.ContainsKey(item.Archetype))
                {
                    dict[item.Archetype] = item.Count;
                }
            }
            return dict;
        }

        /// <summary>
        /// Convert a list of WorkerEfficiencyModifier to a Dictionary.
        /// </summary>
        public static Dictionary<PopulationArchetypes, float> ToDictionary(this List<WorkerEfficiencyModifier> list)
        {
            var dict = new Dictionary<PopulationArchetypes, float>();
            foreach (var item in list)
            {
                if (!dict.ContainsKey(item.Archetype))
                {
                    dict[item.Archetype] = item.Multiplier;
                }
            }
            return dict;
        }

        /// <summary>
        /// Get the count for a specific archetype from the list.
        /// </summary>
        public static int GetCount(this List<WorkerRequirement> list, PopulationArchetypes archetype)
        {
            foreach (var item in list)
            {
                if (item.Archetype == archetype)
                    return item.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get the multiplier for a specific archetype from the list.
        /// </summary>
        public static float GetMultiplier(this List<WorkerEfficiencyModifier> list, PopulationArchetypes archetype, float defaultValue = 1f)
        {
            foreach (var item in list)
            {
                if (item.Archetype == archetype)
                    return item.Multiplier;
            }
            return defaultValue;
        }
    }
}
