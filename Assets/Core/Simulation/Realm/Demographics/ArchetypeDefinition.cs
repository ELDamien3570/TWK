using UnityEngine;

namespace TWK.Realms.Demographics
{
    /// <summary>
    /// Defines the rules, requirements, and behavior for a population archetype.
    /// This creates a rigid social hierarchy with education and wealth-driven mobility.
    /// </summary>
    [CreateAssetMenu(fileName = "ArchetypeDefinition", menuName = "TWK/Demographics/Archetype Definition")]
    public class ArchetypeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public PopulationArchetypes Archetype;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Hierarchy")]
        [Tooltip("Social tier: Slave=0, Laborer=1, Artisan=2, Merchant=3, Noble=4, Clergy=Special")]
        public int SocialTier;

        [Header("Promotion Requirements")]
        [Tooltip("Minimum education level to promote to next tier (0-100)")]
        public float PromotionEducationThreshold = 50f;

        [Tooltip("Minimum wealth level to promote to next tier (0-100)")]
        public float PromotionWealthThreshold = 50f;

        [Tooltip("Base chance per year for eligible pops to promote (0-1)")]
        public float BasePromotionChance = 0.05f;

        [Tooltip("Which archetype this promotes to (if any)")]
        public PopulationArchetypes? PromotesTo = null;

        [Header("Demotion Requirements")]
        [Tooltip("Below this wealth, pops risk demotion (0-100)")]
        public float DemotionWealthThreshold = 20f;

        [Tooltip("Base chance per year for struggling pops to demote (0-1)")]
        public float BaseDemotionChance = 0.02f;

        [Tooltip("Which archetype this demotes to (if any)")]
        public PopulationArchetypes? DemotesTo = null;

        [Header("Special Mobility Rules")]
        [Tooltip("Can this archetype become Clergy? (Laborer/Artisan only)")]
        public bool CanBecomeCergy = false;

        [Tooltip("Base chance to enslave from this class (default 0, set by policy)")]
        public float BaseEnslavementChance = 0f;

        [Header("Economic Properties")]
        [Tooltip("Base wealth income per day per person")]
        public float BaseWealthIncome = 1f;

        [Tooltip("Base education growth per day per person")]
        public float BaseEducationGrowth = 0.01f;

        [Tooltip("Base consumption multiplier (slaves consume less, nobles more)")]
        public float ConsumptionMultiplier = 1f;

        [Header("Growth & Demographics")]
        [Tooltip("Base population growth rate modifier")]
        public float BaseGrowthRate = 0.0008f;

        [Tooltip("Average starting age for this class")]
        public float TypicalAge = 30f;

        [Header("Cultural Properties")]
        [Tooltip("Base fervor/religious conviction (0-100)")]
        public float BaseFervor = 50f;

        [Tooltip("Loyalty modifier to the state (-100 to 100, slaves are negative)")]
        public float LoyaltyModifier = 0f;

        [Tooltip("Stability impact per 1000 people (slaves cause instability)")]
        public float StabilityImpactPer1000 = 0f;

        [Header("Labor Properties")]
        [Tooltip("Can this archetype work in any building? (false = restricted by building requirements)")]
        public bool CanWorkAnywhere = false;

        [Tooltip("Base labor efficiency multiplier (0-2)")]
        public float BaseLaborEfficiency = 1f;
    }
}
