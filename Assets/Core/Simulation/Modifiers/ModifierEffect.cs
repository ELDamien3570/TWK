using UnityEngine;
using TWK.Economy;
using TWK.Realms.Demographics;
using TWK.Cultures;
using System.Collections.Generic;

namespace TWK.Modifiers
{
    /// <summary>
    /// Represents a single effect within a modifier.
    /// A modifier can have multiple effects (e.g., +10 food AND +5% happiness)
    /// </summary>
    [System.Serializable]
    public class ModifierEffect
    {
        [Header("Effect Type")]
        [Tooltip("What does this modifier affect?")]
        public ModifierTarget Target;

        [Tooltip("What type of effect is this?")]
        public ModifierEffectType EffectType;

        [Header("Value")]
        [Tooltip("The magnitude of the effect")]
        public float Value;

        [Tooltip("Is this a percentage modifier? (true = 5%, false = +5 flat)")]
        public bool IsPercentage;

        [Header("Resource-Specific")]
        [Tooltip("Which resource does this affect? (Only for Resource* effect types)")]
        public ResourceType ResourceType;

        [Header("Filters")]
        [Tooltip("Filter by building category (Only for Building target)")]
        public BuildingCategoryFilter BuildingFilter = BuildingCategoryFilter.All;

        [Tooltip("Filter by specific building definitions (Only for Building target)")]
        public List<BuildingDefinition> SpecificBuildings = new List<BuildingDefinition>();

        [Tooltip("Should this filter by specific buildings? If true, only affects buildings in SpecificBuildings list")]
        public bool FilterBySpecificBuildings = false;

        [Tooltip("Filter by population archetype (Only for PopulationGroup target)")]
        public PopulationArchetypes PopGroupFilter = PopulationArchetypes.Laborer;

        [Tooltip("Should this filter by pop archetype? If false, affects all pop groups")]
        public bool FilterByPopArchetype = false;

        [Tooltip("Filter by culture tech tree type (Only for CultureXPGain effect)")]
        public TreeType TechTreeFilter;

        [Tooltip("Should this filter by tech tree? If false, affects all trees")]
        public bool FilterByTechTree = false;

        /// <summary>
        /// Get a human-readable description of this effect.
        /// </summary>
        public string GetDescription()
        {
            string valueStr = IsPercentage ? $"{Value:F1}%" : $"{Value:F0}";
            string sign = Value >= 0 ? "+" : "";

            switch (EffectType)
            {
                case ModifierEffectType.ResourceProduction:
                    string buildingDesc = BuildingFilter != BuildingCategoryFilter.All
                        ? $" from {BuildingFilter} buildings"
                        : "";
                    return $"{sign}{valueStr} {ResourceType}{buildingDesc}";

                case ModifierEffectType.ResourceMaintenance:
                    return $"{sign}{valueStr} {ResourceType} maintenance";

                case ModifierEffectType.PopulationGrowth:
                    return $"{sign}{valueStr} population growth";

                case ModifierEffectType.PopulationEducation:
                    string popDesc = FilterByPopArchetype ? $" for {PopGroupFilter}" : "";
                    return $"{sign}{valueStr} education growth{popDesc}";

                case ModifierEffectType.PopulationHappiness:
                    return $"{sign}{valueStr} happiness";

                case ModifierEffectType.PopulationFervor:
                    return $"{sign}{valueStr} fervor";

                case ModifierEffectType.PopulationIncomeGrowth:
                    string incomePopDesc = FilterByPopArchetype ? $" for {PopGroupFilter}" : "";
                    return $"{sign}{valueStr} income growth{incomePopDesc}";

                case ModifierEffectType.CityStability:
                    return $"{sign}{valueStr} stability";

                case ModifierEffectType.CityGrowthRate:
                    return $"{sign}{valueStr} city growth";

                case ModifierEffectType.BuildingEfficiency:
                    string effBuildingDesc = BuildingFilter != BuildingCategoryFilter.All
                        ? $" for {BuildingFilter} buildings"
                        : "";
                    return $"{sign}{valueStr} building efficiency{effBuildingDesc}";

                case ModifierEffectType.BuildingConstructionSpeed:
                    return $"{sign}{valueStr} construction speed";

                case ModifierEffectType.CultureXPGain:
                    string treeDesc = FilterByTechTree ? $" for {TechTreeFilter}" : "";
                    return $"{sign}{valueStr} culture XP{treeDesc}";

                case ModifierEffectType.CharacterPiety:
                    return $"{sign}{valueStr} piety gain";

                case ModifierEffectType.CharacterPrestige:
                    return $"{sign}{valueStr} prestige gain";

                case ModifierEffectType.MilitaryPower:
                    return $"{sign}{valueStr} military power";

                case ModifierEffectType.RecruitmentSpeed:
                    return $"{sign}{valueStr} recruitment speed";

                default:
                    return $"{sign}{valueStr} {EffectType}";
            }
        }

        /// <summary>
        /// Does this effect apply to the given building category?
        /// </summary>
        public bool AppliesTo(TreeType buildingCategory)
        {
            if (Target != ModifierTarget.Building)
                return false;

            // If filtering by specific buildings, category filter doesn't apply
            if (FilterBySpecificBuildings)
                return false;

            if (BuildingFilter == BuildingCategoryFilter.All)
                return true;

            // Map TreeType to BuildingCategoryFilter
            return buildingCategory switch
            {
                TreeType.Economics => BuildingFilter == BuildingCategoryFilter.Economic,
                TreeType.Warfare => BuildingFilter == BuildingCategoryFilter.Military,
                TreeType.Politics => BuildingFilter == BuildingCategoryFilter.Political,
                TreeType.Science => BuildingFilter == BuildingCategoryFilter.Scientific,
                TreeType.Religion => BuildingFilter == BuildingCategoryFilter.Religious,
                _ => false
            };
        }

        /// <summary>
        /// Does this effect apply to the given specific building definition?
        /// </summary>
        public bool AppliesTo(BuildingDefinition buildingDef)
        {
            if (Target != ModifierTarget.Building)
                return false;

            // If filtering by specific buildings, check if this building is in the list
            if (FilterBySpecificBuildings)
            {
                if (buildingDef == null)
                    return false;

                foreach (var specificBuilding in SpecificBuildings)
                {
                    if (specificBuilding != null && specificBuilding.GetStableDefinitionID() == buildingDef.GetStableDefinitionID())
                        return true;
                }
                return false;
            }

            // Otherwise, check category filter
            return AppliesTo(buildingDef.BuildingCategory);
        }

        /// <summary>
        /// Does this effect apply to the given population archetype?
        /// </summary>
        public bool AppliesTo(PopulationArchetypes archetype)
        {
            if (Target != ModifierTarget.PopulationGroup)
                return false;

            if (!FilterByPopArchetype)
                return true;

            return PopGroupFilter == archetype;
        }

        /// <summary>
        /// Does this effect apply to the given tech tree?
        /// </summary>
        public bool AppliesToTechTree(TreeType treeType)
        {
            if (EffectType != ModifierEffectType.CultureXPGain)
                return false;

            if (!FilterByTechTree)
                return true;

            return TechTreeFilter == treeType;
        }
    }
}
