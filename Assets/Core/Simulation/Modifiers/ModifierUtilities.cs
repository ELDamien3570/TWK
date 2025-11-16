using System.Collections.Generic;
using TWK.Economy;
using TWK.Cultures;
using TWK.Realms.Demographics;

namespace TWK.Modifiers
{
    /// <summary>
    /// Utility methods and extensions for working with modifiers.
    /// </summary>
    public static class ModifierUtilities
    {
        // ========== RESOURCE MODIFIERS ==========

        /// <summary>
        /// Calculate modified resource production from a building.
        /// Applies modifiers from culture, religion, building bonuses, and timed effects.
        /// </summary>
        /// <param name="baseProduction">Base production value</param>
        /// <param name="resourceType">Type of resource being produced</param>
        /// <param name="buildingDef">The building definition to check for specific targeting</param>
        /// <param name="modifiers">List of applicable modifiers</param>
        /// <returns>Modified production value</returns>
        public static int CalculateModifiedProduction(
            int baseProduction,
            ResourceType resourceType,
            BuildingDefinition buildingDef,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.ResourceProduction);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this resource
                    if (effect.ResourceType != resourceType)
                        continue;

                    // Check if this effect applies to this specific building or category
                    if (!effect.AppliesTo(buildingDef))
                        continue;

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

            return UnityEngine.Mathf.RoundToInt((baseProduction + flatBonus) * percentageMultiplier);
        }

        /// <summary>
        /// Calculate modified resource production from a building (legacy overload using TreeType).
        /// </summary>
        public static int CalculateModifiedProduction(
            int baseProduction,
            ResourceType resourceType,
            TreeType buildingCategory,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.ResourceProduction);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this resource
                    if (effect.ResourceType != resourceType)
                        continue;

                    // Check if this effect applies to this building category
                    if (!effect.AppliesTo(buildingCategory))
                        continue;

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

            return UnityEngine.Mathf.RoundToInt((baseProduction + flatBonus) * percentageMultiplier);
        }

        // ========== POPULATION MODIFIERS ==========

        /// <summary>
        /// Calculate modified education growth for a population group.
        /// </summary>
        public static float CalculateModifiedEducationGrowth(
            float baseGrowth,
            PopulationArchetypes archetype,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.PopulationEducation);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this archetype
                    if (!effect.AppliesTo(archetype))
                        continue;

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

            return (baseGrowth + flatBonus) * percentageMultiplier;
        }

        /// <summary>
        /// Calculate modified population growth rate.
        /// </summary>
        public static float CalculateModifiedPopulationGrowth(
            float baseGrowth,
            List<Modifier> modifiers)
        {
            return ModifierManager.CalculateModifiedValue(
                modifiers,
                ModifierEffectType.PopulationGrowth,
                baseGrowth
            );
        }

        /// <summary>
        /// Calculate modified income growth for a population group.
        /// </summary>
        public static float CalculateModifiedIncomeGrowth(
            float baseGrowth,
            PopulationArchetypes archetype,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.PopulationIncomeGrowth);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this archetype
                    if (!effect.AppliesTo(archetype))
                        continue;

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

            return (baseGrowth + flatBonus) * percentageMultiplier;
        }

        // ========== BUILDING MODIFIERS ==========

        /// <summary>
        /// Calculate modified building efficiency.
        /// </summary>
        public static float CalculateModifiedBuildingEfficiency(
            float baseEfficiency,
            TreeType buildingCategory,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.BuildingEfficiency);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this building category
                    if (!effect.AppliesTo(buildingCategory))
                        continue;

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

            return (baseEfficiency + flatBonus) * percentageMultiplier;
        }

        // ========== CULTURE MODIFIERS ==========

        /// <summary>
        /// Calculate modified culture XP gain.
        /// </summary>
        public static float CalculateModifiedCultureXP(
            float baseXP,
            TreeType treeType,
            List<Modifier> modifiers)
        {
            float flatBonus = 0f;
            float percentageMultiplier = 1f;

            foreach (var modifier in modifiers)
            {
                var effects = modifier.GetEffectsOfType(ModifierEffectType.CultureXPGain);
                foreach (var effect in effects)
                {
                    // Check if this effect applies to this tech tree
                    if (!effect.AppliesToTechTree(treeType))
                        continue;

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

            return (baseXP + flatBonus) * percentageMultiplier;
        }

        // ========== CITY MODIFIERS ==========

        /// <summary>
        /// Calculate modified city growth rate.
        /// </summary>
        public static float CalculateModifiedCityGrowth(
            float baseGrowth,
            List<Modifier> modifiers)
        {
            return ModifierManager.CalculateModifiedValue(
                modifiers,
                ModifierEffectType.CityGrowthRate,
                baseGrowth
            );
        }

        /// <summary>
        /// Calculate modified city stability.
        /// </summary>
        public static float CalculateModifiedCityStability(
            float baseStability,
            List<Modifier> modifiers)
        {
            return ModifierManager.CalculateModifiedValue(
                modifiers,
                ModifierEffectType.CityStability,
                baseStability
            );
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Combine multiple modifier lists into one.
        /// Useful for aggregating modifiers from multiple sources (culture + religion + building + timed).
        /// </summary>
        public static List<Modifier> CombineModifiers(params List<Modifier>[] modifierLists)
        {
            var combined = new List<Modifier>();
            foreach (var list in modifierLists)
            {
                if (list != null)
                    combined.AddRange(list);
            }
            return combined;
        }

        /// <summary>
        /// Get all modifiers affecting a specific target.
        /// </summary>
        public static List<ModifierEffect> GetEffectsForTarget(
            List<Modifier> modifiers,
            ModifierTarget target)
        {
            var effects = new List<ModifierEffect>();
            foreach (var modifier in modifiers)
            {
                effects.AddRange(modifier.GetEffectsForTarget(target));
            }
            return effects;
        }
    }
}
