using System;
using UnityEngine;
using TWK.Economy;
using TWK.Realms.Demographics;

namespace TWK.Utils
{
    /// <summary>
    /// Serializable dictionary for ResourceType to int mappings.
    /// Used for build costs, maintenance costs, and production values.
    /// </summary>
    [Serializable]
    public class ResourceIntDictionary : SerializableDictionary<ResourceType, int>
    {
    }

    /// <summary>
    /// Serializable dictionary for PopulationArchetypes to int mappings.
    /// Used for required worker counts by type.
    /// </summary>
    [Serializable]
    public class PopulationIntDictionary : SerializableDictionary<PopulationArchetypes, int>
    {
    }

    /// <summary>
    /// Serializable dictionary for PopulationArchetypes to float mappings.
    /// Used for worker efficiency and penalty multipliers.
    /// </summary>
    [Serializable]
    public class PopulationFloatDictionary : SerializableDictionary<PopulationArchetypes, float>
    {
    }
}
