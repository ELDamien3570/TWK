namespace TWK.Agents
{
    /// <summary>
    /// Gender of an agent.
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    /// <summary>
    /// Sexuality of an agent - affects relationship formation.
    /// </summary>
    public enum Sexuality
    {
        Heterosexual,
        Homosexual,
        Bisexual
    }

    /// <summary>
    /// Types of personality traits that can be assigned to agents.
    /// These can be unlocked through skill trees or assigned at creation.
    /// </summary>
    public enum PersonalityTrait
    {
        // Combat Traits
        Brave,
        Cowardly,
        Aggressive,
        Defensive,
        Tactician,
        Berserker,

        // Social Traits
        Charismatic,
        Shy,
        Diplomatic,
        Blunt,
        Manipulative,
        Honest,

        // Economic Traits
        Greedy,
        Generous,
        Frugal,
        Wasteful,
        Merchant,

        // Leadership Traits
        Just,
        Cruel,
        Ambitious,
        Content,
        Inspiring,
        Tyrannical,

        // Personal Traits
        Lustful,
        Chaste,
        Gluttonous,
        Temperate,
        Wrathful,
        Calm,
        Patient,
        Impatient,
        Humble,
        Proud,

        // Intellectual Traits
        Scholarly,
        Ignorant,
        Curious,
        Stubborn,
        Wise,
        Foolish
    }
}
