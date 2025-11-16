using System.Collections.Generic;
using UnityEngine;
using TWK.Modifiers;
using TWK.Realms.Demographics;

namespace TWK.Government
{
    /// <summary>
    /// Represents a temporary law or decree enacted by a government.
    /// Edicts provide powerful modifiers but affect population loyalty by archetype.
    /// </summary>
    [System.Serializable]
    public class Edict
    {
        // ========== IDENTITY ==========
        [Tooltip("Name of this edict (e.g., 'Forced Labor', 'Tax Relief')")]
        public string EdictName;

        [TextArea(3, 6)]
        [Tooltip("Description of what this edict does")]
        public string Description;

        // ========== MODIFIERS ==========
        [Header("Effects")]
        [Tooltip("Standard modifiers applied realm-wide")]
        public List<Modifier> Modifiers = new List<Modifier>();

        // ========== LOYALTY EFFECTS ==========
        [Header("Loyalty Effects by Archetype")]
        [Tooltip("Loyalty impact for Laborers")]
        public float LaborerLoyalty = 0f;

        [Tooltip("Loyalty impact for Artisans")]
        public float ArtisanLoyalty = 0f;

        [Tooltip("Loyalty impact for Nobles")]
        public float NobleLoyalty = 0f;

        [Tooltip("Loyalty impact for Clergy")]
        public float ClergyLoyalty = 0f;

        [Tooltip("Loyalty impact for Slaves")]
        public float SlaveLoyalty = 0f;

        [Tooltip("Loyalty impact for Merchants")]
        public float MerchantLoyalty = 0f;

        // ========== COSTS ==========
        [Header("Costs")]
        [Tooltip("One-time gold cost to enact this edict")]
        public int EnactmentCost = 500;

        [Tooltip("Monthly gold cost to maintain this edict")]
        public int MonthlyMaintenance = 50;

        // ========== DURATION ==========
        [Header("Duration")]
        [Tooltip("Is this edict permanent or temporary?")]
        public bool IsPermanent = false;

        [Tooltip("Duration in months (if not permanent)")]
        public int DurationMonths = 12;

        // ========== RUNTIME DATA ==========
        // These are not serialized in the template, only in active instances

        /// <summary>
        /// Month when this edict was enacted (for expiration tracking).
        /// </summary>
        [System.NonSerialized]
        public int EnactedOnMonth = -1;

        /// <summary>
        /// Realm ID where this edict is active.
        /// </summary>
        [System.NonSerialized]
        public int TargetRealmID = -1;

        // ========== CONSTRUCTORS ==========

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Edict()
        {
        }

        /// <summary>
        /// Create a new edict with basic configuration.
        /// </summary>
        public Edict(string name, string description)
        {
            EdictName = name;
            Description = description;
        }

        // ========== LOYALTY IMPACT ==========

        /// <summary>
        /// Get loyalty impact for a specific population archetype.
        /// </summary>
        public float GetLoyaltyImpact(PopulationArchetypes archetype)
        {
            return archetype switch
            {
                PopulationArchetypes.Laborer => LaborerLoyalty,
                PopulationArchetypes.Artisan => ArtisanLoyalty,
                PopulationArchetypes.Noble => NobleLoyalty,
                PopulationArchetypes.Clergy => ClergyLoyalty,
                PopulationArchetypes.Slave => SlaveLoyalty,
                PopulationArchetypes.Merchant => MerchantLoyalty,
                _ => 0f
            };
        }

        /// <summary>
        /// Set loyalty impact for a specific population archetype.
        /// </summary>
        public void SetLoyaltyImpact(PopulationArchetypes archetype, float impact)
        {
            switch (archetype)
            {
                case PopulationArchetypes.Laborer:
                    LaborerLoyalty = impact;
                    break;
                case PopulationArchetypes.Artisan:
                    ArtisanLoyalty = impact;
                    break;
                case PopulationArchetypes.Noble:
                    NobleLoyalty = impact;
                    break;
                case PopulationArchetypes.Clergy:
                    ClergyLoyalty = impact;
                    break;
                case PopulationArchetypes.Slave:
                    SlaveLoyalty = impact;
                    break;
                case PopulationArchetypes.Merchant:
                    MerchantLoyalty = impact;
                    break;
            }
        }

        /// <summary>
        /// Get all archetypes affected by this edict (loyalty != 0).
        /// </summary>
        public List<PopulationArchetypes> GetAffectedArchetypes()
        {
            var affected = new List<PopulationArchetypes>();

            if (Mathf.Abs(LaborerLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Laborer);
            if (Mathf.Abs(ArtisanLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Artisan);
            if (Mathf.Abs(NobleLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Noble);
            if (Mathf.Abs(ClergyLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Clergy);
            if (Mathf.Abs(SlaveLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Slave);
            if (Mathf.Abs(MerchantLoyalty) > 0.01f)
                affected.Add(PopulationArchetypes.Merchant);

            return affected;
        }

        /// <summary>
        /// Get average loyalty impact across all archetypes.
        /// </summary>
        public float GetAverageLoyaltyImpact()
        {
            float total = LaborerLoyalty + ArtisanLoyalty + NobleLoyalty +
                         ClergyLoyalty + SlaveLoyalty + MerchantLoyalty;
            return total / 6f;
        }

        // ========== ACTIVE STATE ==========

        /// <summary>
        /// Is this edict currently active?
        /// </summary>
        public bool IsActive(int currentMonth)
        {
            // Not enacted yet
            if (EnactedOnMonth == -1)
                return false;

            // Permanent edicts never expire
            if (IsPermanent)
                return true;

            // Check if still within duration
            return currentMonth < EnactedOnMonth + DurationMonths;
        }

        /// <summary>
        /// Mark this edict as enacted.
        /// </summary>
        public void Enact(int currentMonth, int realmID)
        {
            EnactedOnMonth = currentMonth;
            TargetRealmID = realmID;
        }

        /// <summary>
        /// Get months remaining until expiration.
        /// </summary>
        public int GetMonthsRemaining(int currentMonth)
        {
            if (IsPermanent)
                return -1;

            if (EnactedOnMonth == -1)
                return DurationMonths;

            int remaining = (EnactedOnMonth + DurationMonths) - currentMonth;
            return Mathf.Max(0, remaining);
        }

        // ========== COST CALCULATIONS ==========

        /// <summary>
        /// Get total cost for the edict's full duration.
        /// </summary>
        public int GetTotalCost()
        {
            if (IsPermanent)
                return EnactmentCost; // Can't calculate infinite maintenance

            return EnactmentCost + (MonthlyMaintenance * DurationMonths);
        }

        /// <summary>
        /// Get cost per month (including amortized enactment cost).
        /// </summary>
        public float GetCostPerMonth()
        {
            if (IsPermanent)
                return MonthlyMaintenance;

            return (float)EnactmentCost / DurationMonths + MonthlyMaintenance;
        }

        // ========== DISPLAY HELPERS ==========

        /// <summary>
        /// Get a formatted display string for loyalty effects.
        /// </summary>
        public string GetLoyaltyEffectsDescription()
        {
            var effects = new List<string>();

            if (Mathf.Abs(LaborerLoyalty) > 0.01f)
                effects.Add($"Laborers: {LaborerLoyalty:+0.#;-0.#}");
            if (Mathf.Abs(ArtisanLoyalty) > 0.01f)
                effects.Add($"Artisans: {ArtisanLoyalty:+0.#;-0.#}");
            if (Mathf.Abs(NobleLoyalty) > 0.01f)
                effects.Add($"Nobles: {NobleLoyalty:+0.#;-0.#}");
            if (Mathf.Abs(ClergyLoyalty) > 0.01f)
                effects.Add($"Clergy: {ClergyLoyalty:+0.#;-0.#}");
            if (Mathf.Abs(SlaveLoyalty) > 0.01f)
                effects.Add($"Slaves: {SlaveLoyalty:+0.#;-0.#}");
            if (Mathf.Abs(MerchantLoyalty) > 0.01f)
                effects.Add($"Merchants: {MerchantLoyalty:+0.#;-0.#}");

            if (effects.Count == 0)
                return "No loyalty effects";

            return string.Join(", ", effects);
        }

        /// <summary>
        /// Get duration description.
        /// </summary>
        public string GetDurationDescription()
        {
            if (IsPermanent)
                return "Permanent";

            return $"{DurationMonths} months";
        }

        /// <summary>
        /// Clone this edict for instancing (creating active copy from template).
        /// </summary>
        public Edict Clone()
        {
            var clone = new Edict(EdictName, Description);

            // Copy all properties
            clone.LaborerLoyalty = LaborerLoyalty;
            clone.ArtisanLoyalty = ArtisanLoyalty;
            clone.NobleLoyalty = NobleLoyalty;
            clone.ClergyLoyalty = ClergyLoyalty;
            clone.SlaveLoyalty = SlaveLoyalty;
            clone.MerchantLoyalty = MerchantLoyalty;

            clone.EnactmentCost = EnactmentCost;
            clone.MonthlyMaintenance = MonthlyMaintenance;
            clone.IsPermanent = IsPermanent;
            clone.DurationMonths = DurationMonths;

            // Clone modifiers
            foreach (var modifier in Modifiers)
            {
                if (modifier != null)
                    clone.Modifiers.Add(modifier.Clone());
            }

            return clone;
        }
    }
}
