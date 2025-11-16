namespace TWK.Modifiers
{
    /// <summary>
    /// What scope does this modifier affect?
    /// </summary>
    public enum ModifierTarget
    {
        Global,          // Affects entire realm
        City,            // Affects cities with the modifier
        PopulationGroup, // Affects population groups with the modifier
        Building,        // Affects buildings with the modifier
        Character        // Affects characters with the modifier (future)
    }

    /// <summary>
    /// What type of effect does this modifier have?
    /// </summary>
    public enum ModifierEffectType
    {
        // ========== RESOURCE EFFECTS ==========
        ResourceProduction,      // Increases/decreases resource production
        ResourceMaintenance,     // Increases/decreases resource maintenance costs

        // ========== POPULATION EFFECTS ==========
        PopulationGrowth,        // Affects population growth rate
        PopulationEducation,     // Affects education growth for pop groups
        PopulationHappiness,     // Affects happiness
        PopulationFervor,        // Affects religious fervor
        PopulationIncomeGrowth,  // Affects wealth growth for pop groups

        // ========== CITY/REALM EFFECTS ==========
        CityStability,           // Affects city stability
        CityGrowthRate,          // Affects city growth modifier

        // ========== BUILDING EFFECTS ==========
        BuildingEfficiency,      // Affects building efficiency multiplier
        BuildingConstructionSpeed, // Affects construction speed

        // ========== CULTURE/TECH EFFECTS ==========
        CultureXPGain,           // Affects culture tech XP generation

        // ========== FUTURE CHARACTER EFFECTS ==========
        CharacterPiety,          // Affects piety gain
        CharacterPrestige,       // Affects prestige gain

        // ========== MILITARY EFFECTS (Future) ==========
        MilitaryPower,           // Affects military strength
        RecruitmentSpeed         // Affects troop recruitment speed
    }

    /// <summary>
    /// How long does this modifier last?
    /// </summary>
    public enum ModifierDuration
    {
        Permanent,    // Lasts forever (culture pillars, building bonuses)
        Timed,        // Lasts for specific number of days (events)
        Conditional   // Active while condition is met (future - e.g., "while at war")
    }

    /// <summary>
    /// What building category does this modifier filter by?
    /// Used for targeted building modifiers like "5% more food from economic hublets"
    /// </summary>
    public enum BuildingCategoryFilter
    {
        All,          // All buildings
        Economic,     // Economic category buildings
        Agricultural, // Agricultural category buildings
        Military,     // Military category buildings
        Cultural,     // Cultural category buildings
        Religious,    // Religious category buildings
        Scientific    // Scientific category buildings
    }
}
