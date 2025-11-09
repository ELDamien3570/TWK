using System.Collections.Generic;
using UnityEngine;


namespace TWK.Economy
{
    [CreateAssetMenu(menuName = "TWK/Buildings/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
            public string UpgradeName;
            public float EfficiencyMultiplier = 1f; // global multiplier for this building
            public Dictionary<ResourceType, float> ProductionMultipliers = new();
            public Dictionary<ResourceType, float> MaintenanceMultipliers = new();
    }
}
