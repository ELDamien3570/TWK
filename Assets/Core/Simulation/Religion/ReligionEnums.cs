namespace TWK.Religion
{
    /// <summary>
    /// Tradition type - how is religious knowledge transmitted?
    /// </summary>
    public enum ReligionTradition
    {
        Written,    // Has holy texts, more stable, harder to reform
        Oral        // Passed down through stories, more flexible, easier to reform
    }

    /// <summary>
    /// Centralization level - how organized is the religious structure?
    /// </summary>
    public enum ReligionCentralization
    {
        Decentralized,  // No central authority, local interpretation
        Centralized     // Strong hierarchy, Head of Faith has more power
    }

    /// <summary>
    /// Evangelism stance - how does the religion view conversion?
    /// </summary>
    public enum ReligionEvangelism
    {
        Isolationist,   // Conversion is discouraged or forbidden
        Evangelical     // Actively seeks to convert others
    }

    /// <summary>
    /// Syncretism stance - how open is the religion to absorbing other beliefs?
    /// </summary>
    public enum ReligionSyncretism
    {
        Rigid,      // Rejects foreign beliefs, maintains purity
        Syncretic   // Absorbs and adapts foreign beliefs
    }

    /// <summary>
    /// Religion type - scope and social acceptance
    /// </summary>
    public enum ReligionType
    {
        MainlineReligion,   // Socially accepted, mainstream
        Cult                // Fringe belief, social penalties, secretive
    }

    /// <summary>
    /// Tenet categories for organization
    /// </summary>
    public enum TenetCategory
    {
        Virtue,         // What behaviors are encouraged
        Sin,            // What behaviors are forbidden
        Marriage,       // Marriage and family rules
        Gender,         // Gender roles and equality
        Crime,          // Views on crime and punishment
        Clerical,       // Rules for clergy
        Death,          // Death rites and afterlife
        Special         // Unique doctrines specific to this religion
    }

    /// <summary>
    /// Festival frequency
    /// </summary>
    public enum FestivalFrequency
    {
        Yearly,
        Seasonal,
        Monthly
    }

    /// <summary>
    /// Holy land importance level
    /// </summary>
    public enum HolyLandImportance
    {
        Sacred,     // Most important, pilgrimage destination
        Blessed,    // Important but not critical
        Historical  // Historically significant
    }

    /// <summary>
    /// Head of Faith powers (for centralized religions)
    /// </summary>
    [System.Flags]
    public enum HeadOfFaithPowers
    {
        None = 0,
        Excommunication = 1 << 0,       // Can excommunicate rulers
        GrantDivorce = 1 << 1,          // Can grant divorces
        GrantClaims = 1 << 2,           // Can grant claims on titles
        CallCrusade = 1 << 3,           // Can call religious wars
        InvestClergy = 1 << 4,          // Controls clergy appointments
        TaxClergy = 1 << 5,             // Can tax religious holdings
        GrantIndulgences = 1 << 6       // Can grant sins forgiveness for gold
    }
}
