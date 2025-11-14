using System;
using System.Collections.Generic;

namespace TWK.Economy
{
    /// <summary>
    /// Represents an amount of a specific resource.
    /// Used for costs, production, and consumption in a Unity Inspector-friendly format.
    /// </summary>
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType ResourceType;
        public int Amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            ResourceType = type;
            Amount = amount;
        }
    }

    /// <summary>
    /// Extension methods for working with ResourceAmount lists.
    /// </summary>
    public static class ResourceAmountExtensions
    {
        /// <summary>
        /// Convert a list of ResourceAmount to a Dictionary.
        /// </summary>
        public static Dictionary<ResourceType, int> ToDictionary(this List<ResourceAmount> list)
        {
            var dict = new Dictionary<ResourceType, int>();
            foreach (var item in list)
            {
                if (!dict.ContainsKey(item.ResourceType))
                {
                    dict[item.ResourceType] = item.Amount;
                }
            }
            return dict;
        }

        /// <summary>
        /// Get the amount of a specific resource type from the list.
        /// </summary>
        public static int GetAmount(this List<ResourceAmount> list, ResourceType type)
        {
            foreach (var item in list)
            {
                if (item.ResourceType == type)
                    return item.Amount;
            }
            return 0;
        }
    }
}
