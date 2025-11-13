using System.Collections.Generic;
using TWK.Cultures;
using TWK.Economy;
using UnityEngine;

namespace TWK.Economy
{
    [CreateAssetMenu(menuName = "TWK/Buildings/Farm")]
    public class FarmBuildingData : BuildingData
    {
        private void OnEnable()
        {
            BuildingName = "Farm";
            buildingCategory = TreeType.Economics;

            BaseBuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Gold, 50 },
                { ResourceType.Food, 0 }
            };

            BaseMaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Gold, 1 }
            };

            BaseProduction = new Dictionary<ResourceType, int>
            {
                { ResourceType.Food, 10075 }
            };

            BaseEfficiency = 1f;
            ConstructionTimeDays = 3; // Farm takes 3 days to build
        }
    }
}
