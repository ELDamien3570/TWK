using System;
using System.Collections.Generic;
using TWK.Economy;
using TWK.Cultures;

namespace TWK.Culture
{
    /// <summary>
    /// Defines a modifier that can affect realms, cities, agents, or buildings.
    /// Modifiers are granted by tech nodes and cultural pillars.
    /// </summary>
    [Serializable]
    public class CultureModifier
    {
        // ========== IDENTITY ==========
        public string Name;
        public string Description;

        // ========== MODIFIER TYPE ==========
        public ModifierScope Scope; // Realm, City, Agent, Building
        public ModifierValueType ValueType; // Flat or Percentage

        // ========== BONUS VALUE ==========
        public float BonusValue; // e.g., 100 for flat, 0.05 for 5%

        // ========== TARGET ==========
        public ModifierTargetType TargetType; // What this affects
        public ResourceType? TargetResource; // If affecting resource production/consumption
        public TreeType? TargetBuildingCategory; // If affecting specific building category
        public int? TargetBuildingDefinitionID; // If affecting specific building type

        // ========== CONDITIONAL RULES ==========
        // Optional rules for when this modifier applies
        public List<ModifierRule> Rules = new List<ModifierRule>();

        // ========== CONSTRUCTOR ==========
        public CultureModifier()
        {
            Rules = new List<ModifierRule>();
        }

        /// <summary>
        /// Check if this modifier applies given the current context.
        /// </summary>
        public bool AppliesInContext(ModifierContext context)
        {
            // If no rules, always applies
            if (Rules.Count == 0)
                return true;

            // All rules must pass for modifier to apply
            foreach (var rule in Rules)
            {
                if (!rule.Evaluate(context))
                    return false;
            }

            return true;
        }
    }

    // ========== ENUMS ==========

    public enum ModifierScope
    {
        Realm,      // Applies to entire realm
        City,       // Applies to cities of this culture (>50% pop)
        Agent,      // Applies to agents of this culture
        Building    // Applies to specific buildings
    }

    public enum ModifierValueType
    {
        Flat,       // +100 food
        Percentage  // +5% food (0.05)
    }

    public enum ModifierTargetType
    {
        ResourceProduction,     // Affects resource production
        ResourceConsumption,    // Affects resource consumption
        BuildingEfficiency,     // Affects building efficiency multiplier
        PopulationGrowth,       // Affects population growth rate
        ConstructionSpeed,      // Affects building construction time
        MaintenanceCost,        // Affects building maintenance costs
        XPGeneration,           // Affects XP generation rate
        // Placeholders for future world-based modifiers
        ArmyLineOfSight,        // Affects army vision range
        MovementSpeed,          // Affects army movement speed
        Custom                  // Custom modifier type (for future expansion)
    }

    // ========== MODIFIER RULES ==========

    /// <summary>
    /// Defines a conditional rule for when a modifier applies.
    /// </summary>
    [Serializable]
    public class ModifierRule
    {
        public ModifierRuleType RuleType;
        public float Threshold; // e.g., population > 1000

        public bool Evaluate(ModifierContext context)
        {
            switch (RuleType)
            {
                case ModifierRuleType.CityPopulationGreaterThan:
                    return context.CityPopulation > Threshold;

                case ModifierRuleType.CityPopulationLessThan:
                    return context.CityPopulation < Threshold;

                case ModifierRuleType.BuildingWorkerCountGreaterThan:
                    return context.BuildingWorkerCount > Threshold;

                default:
                    return true;
            }
        }
    }

    public enum ModifierRuleType
    {
        CityPopulationGreaterThan,
        CityPopulationLessThan,
        BuildingWorkerCountGreaterThan,
        // More rule types can be added as needed
    }

    // ========== MODIFIER CONTEXT ==========

    /// <summary>
    /// Context information for evaluating modifier rules.
    /// </summary>
    public struct ModifierContext
    {
        public int CityPopulation;
        public int BuildingWorkerCount;
        // Add more context fields as needed
    }
}
