using System.Collections.Generic;
using TWK.Cultures;
using UnityEngine;

namespace TWK.Economy
{
    [CreateAssetMenu(menuName = "TWK/Economy/Building Data", fileName = "New Building Data")]
    public class BuildingData : ScriptableObject
    {
        public string BuildingName;
        public TreeType buildingCategory;
        public bool isHubBuilding;
        public Sprite Icon;

        [Header("Costs")]
        public Dictionary<ResourceType, int> BaseBuildCost = new();
        public Dictionary<ResourceType, int> BaseMaintenanceCost = new();

        [Header("Production")]
        public Dictionary<ResourceType, int> BaseProduction = new();

        [Header("Modifiers")]
        public float BaseEfficiency = 1f;

        [Header("Construction")]
        public int ConstructionTimeDays = 1; // Add construction time in days
    }
}
