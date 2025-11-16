using System;

namespace TWK.Government
{
    // ========== REGIME TYPES ==========

    /// <summary>
    /// Defines the fundamental structure of political authority
    /// </summary>
    public enum RegimeForm
    {
        Autocratic,     // Sole ruler (monarch, dictator, chief executive)
        Pluralist,      // Bureaucratic council or oligarchy
        Chiefdom,       // Chief + tribal assemblies
        Confederation,  // League of cities (future implementation)
        League          // Trade league (future implementation)
    }

    /// <summary>
    /// Defines the territorial organization of the state
    /// </summary>
    public enum StateStructure
    {
        Territorial,    // Multi-province state
        CityState,      // Single city dominance
        Nomadic         // Mobile camps and pastoral organization
    }

    /// <summary>
    /// Defines how leadership transitions occur
    /// </summary>
    public enum SuccessionLaw
    {
        // Autocratic succession types
        Chosen,         // Ruler chooses successor
        Elective,       // Nobles/council elect
        Theocratic,     // Religious authority determines
        Hereditary,     // Bloodline inheritance

        // Pluralist succession types
        Oligarchical,   // Elite families rotate
        Republican,     // Term-limited elections
        Democratic,     // Broad citizen elections

        // Chiefdom succession types
        Kinship,        // Clan-based selection
        Divinity,       // Divine right or blessing
        Strength        // Trial by combat or merit
    }

    /// <summary>
    /// Defines how the bureaucracy is organized
    /// </summary>
    public enum Administration
    {
        Patrimonial,    // Nobles and loyalists hold power
        Rational        // Competency-based meritocracy
    }

    /// <summary>
    /// Defines mobility restrictions for the population
    /// </summary>
    public enum Mobility
    {
        Sedentary,      // Cannot migrate freely
        SemiNomadic,    // Can migrate under pressure (famine, war)
        Nomadic         // Can migrate freely at will
    }

    // ========== LAWS ==========

    /// <summary>
    /// Defines how military forces are raised (can have multiple types)
    /// </summary>
    [Flags]
    public enum MilitaryServiceLaw
    {
        None = 0,
        Levies = 1 << 0,               // Temporary armies conscripted from population
        Warbands = 1 << 1,             // Independent warrior bands (mercenaries)
        ProfessionalArmies = 1 << 2    // Standing armies with full-time soldiers
    }

    /// <summary>
    /// Defines how taxes are collected
    /// </summary>
    public enum TaxationLaw
    {
        Tribute,        // Physical goods transported via caravan (can be raided)
        TaxCollectors   // Administrative collection with capacity-based efficiency
    }

    /// <summary>
    /// Defines government control over trade
    /// </summary>
    public enum TradeLaw
    {
        NoControl,      // Free market - merchants operate freely
        Regulated,      // Government oversight and tariffs
        StateMonopoly   // Only nobles/state can engage in trade
    }

    /// <summary>
    /// Defines severity of criminal punishment
    /// </summary>
    public enum JusticeLaw
    {
        Severe,         // Harsh punishments (executions, mutilation)
        Fair,           // Balanced justice system
        Lenient         // Light punishments (fines, exile)
    }

    // ========== INSTITUTIONS ==========

    /// <summary>
    /// Categories for organizing institutions
    /// </summary>
    public enum InstitutionCategory
    {
        Diplomacy,      // Foreign relations, vassals, contracts
        Warfare,        // Military organization and logistics
        Economics       // Trade, taxation, resource management
    }

    /// <summary>
    /// Special abilities granted by institutions
    /// </summary>
    [Flags]
    public enum InstitutionAbilities
    {
        None = 0,
        StandingArmy = 1 << 0,          // Instant army raise without levy delay
        Coinage = 1 << 1,               // Better taxation efficiency
        RoyalRoads = 1 << 2,            // Trade route bonuses
        Census = 1 << 3,                // Population tracking and bonuses
        ProvincialStatutes = 1 << 4,    // Enhanced provincial governance
        ImperialBureaucracy = 1 << 5,   // Advanced office automation
        NavalDoctrine = 1 << 6,         // Naval military bonuses
        DiplomaticCorps = 1 << 7        // Enhanced diplomatic actions
    }

    // ========== OFFICES ==========

    /// <summary>
    /// Defines the purpose/automation behavior of an office
    /// </summary>
    public enum OfficePurpose
    {
        None,

        // Economics offices
        RaiseLoyaltyInSlaveGroups,
        ManageCityTrade,
        OptimizeBuildingConstruction,
        ManageTaxCollection,
        DistributeGrain,

        // Diplomacy offices
        MigratePeoples,
        ManageVassalRelations,
        ConductDiplomacy,
        NegotiateTreaties,

        // Warfare offices
        TrainGarrisons,
        RecruitMercenaries,
        ManageDefenses,
        OrganizeSupplyLines,

        // Science offices
        DistributeEducation,
        ManageResearch,
        FundSchools,

        // Religion offices
        SpreadFaith,
        ConvertPopulations,
        BuildTemples
    }

    // ========== CONTRACTS ==========

    /// <summary>
    /// Types of contracts between realms/agents
    /// </summary>
    public enum ContractType
    {
        Governor,   // Agent manages city on behalf of parent realm
        Vassal,     // Realm subordinate to parent realm
        Tributary,  // Nomadic vassalage (lighter obligations)
        Warband     // Independent military contractor
    }

    // ========== REVOLTS ==========

    /// <summary>
    /// Types of revolts based on government form
    /// </summary>
    public enum RevoltType
    {
        PretenderWar,   // Autocracy - rival claimant to throne
        CivicTumult,    // Pluralist - factional conflict in government
        ClanRising,     // Chiefdom - clan rebellion against chief
        MemberSecession // Confederation - member state leaves
    }
}
