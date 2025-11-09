using System.Collections.Generic;
using UnityEngine;

namespace TWK.Economy
{
    public class ResourceLedger
    {
        public int OwnerID;
        public Dictionary<ResourceType, int> DailyChange;
        public Dictionary<ResourceType, int> DailyProduction;
        public Dictionary<ResourceType, int> DailyConsumption;

        public ResourceLedger(int ownerID)
        {
            OwnerID = ownerID;
            DailyChange = new Dictionary<ResourceType, int>();
            DailyProduction = new Dictionary<ResourceType, int>();
            DailyConsumption = new Dictionary<ResourceType, int>();
        }

        public void Add(ResourceType type, int amount)
        {
            if (!DailyChange.ContainsKey(type))
                DailyChange[type] = 0;
            if (!DailyProduction.ContainsKey(type))
                DailyProduction[type] = 0;

            DailyChange[type] += amount;
            DailyProduction[type] += amount;
        }

        public void Subtract(ResourceType type, int amount)
        {
            if (!DailyChange.ContainsKey(type))
                DailyChange[type] = 0;
            if (!DailyConsumption.ContainsKey(type))
                DailyConsumption[type] = 0;

                DailyChange[type] -= amount;
                 DailyConsumption[type] += amount;
        }

        public void RecordChange(ResourceType type, int delta) => Add(type, delta);

        public void ClearDailyChange()
        {
            DailyChange.Clear();
            DailyProduction.Clear();
            DailyConsumption.Clear();
        }
    }
}
