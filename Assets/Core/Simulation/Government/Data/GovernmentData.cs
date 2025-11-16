using System.Collections.Generic;
using UnityEngine;
using TWK.Modifiers;

namespace TWK.Government
{
    /// <summary>
    /// ScriptableObject defining a complete government configuration.
    /// This serves as a template that can be assigned to realms.
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Government/Government Data", fileName = "New Government")]
    public class GovernmentData : ScriptableObject
    {
        // ========== IDENTITY ==========
        [Header("Identity")]
        [Tooltip("Name of this government form (e.g., 'Feudal Monarchy', 'Tribal Chiefdom')")]
        public string GovernmentName;

        [TextArea(3, 6)]
        [Tooltip("Description of this government system")]
        public string Description;

        [Tooltip("Icon representing this government")]
        public Sprite Icon;

        [Tooltip("Theme color for UI display")]
        public Color GovernmentColor = Color.white;

        // ========== REGIME STRUCTURE ==========
        [Header("Regime")]
        [Tooltip("Fundamental structure of political authority")]
        public RegimeForm RegimeForm = RegimeForm.Autocratic;

        [Tooltip("Territorial organization of the state")]
        public StateStructure StateStructure = StateStructure.Territorial;

        [Tooltip("How leadership transitions occur")]
        public SuccessionLaw SuccessionLaw = SuccessionLaw.Hereditary;

        [Tooltip("How the bureaucracy is organized")]
        public Administration Administration = Administration.Patrimonial;

        [Tooltip("Population mobility restrictions")]
        public Mobility Mobility = Mobility.Sedentary;

        // ========== INSTITUTIONS ==========
        [Header("Institutions (Max 3)")]
        [Tooltip("Core institutions that define government capabilities")]
        public List<Institution> Institutions = new List<Institution>();

        // ========== LAWS ==========
        [Header("Laws")]
        [Tooltip("How military forces are raised (can select multiple)")]
        public MilitaryServiceLaw MilitaryService = MilitaryServiceLaw.Levies;

        [Tooltip("How taxes are collected")]
        public TaxationLaw TaxationLaw = TaxationLaw.TaxCollectors;

        [Tooltip("Government control over trade")]
        public TradeLaw TradeLaw = TradeLaw.NoControl;

        [Tooltip("Severity of criminal punishment")]
        public JusticeLaw JusticeLaw = JusticeLaw.Fair;

        // ========== CAPACITY & LEGITIMACY ==========
        [Header("Government Stats")]
        [Range(0, 100)]
        [Tooltip("Base government capacity (administrative efficiency)")]
        public float BaseCapacity = 30f;

        [Range(0, 100)]
        [Tooltip("Base government legitimacy (public support)")]
        public float BaseLegitimacy = 50f;

        // ========== TRANSITION REQUIREMENTS ==========
        [Header("Transition Requirements")]
        [Tooltip("Minimum number of provinces required to adopt this government")]
        public int MinimumProvinces = 1;

        [Tooltip("Minimum capacity required to adopt this government")]
        [Range(0, 100)]
        public float MinimumCapacity = 0f;

        [Tooltip("Required institutions to transition to this government")]
        public List<Institution> RequiredInstitutions = new List<Institution>();

        // ========== STABLE ID ==========
        private int? cachedStableID = null;

        /// <summary>
        /// Get a stable ID for this government based on its name.
        /// Used for save/load and modifier tracking.
        /// </summary>
        public int GetStableGovernmentID()
        {
            if (cachedStableID.HasValue)
                return cachedStableID.Value;

            if (string.IsNullOrEmpty(GovernmentName))
            {
                Debug.LogError("[GovernmentData] Cannot get ID - GovernmentName is null or empty");
                return 0;
            }

            cachedStableID = GovernmentName.GetHashCode();
            return cachedStableID.Value;
        }

        // ========== MODIFIER ACCESS ==========

        /// <summary>
        /// Get all modifiers from all institutions, properly tagged with government source.
        /// </summary>
        public List<Modifier> GetAllModifiers()
        {
            var allModifiers = new List<Modifier>();
            int governmentID = GetStableGovernmentID();

            foreach (var institution in Institutions)
            {
                if (institution != null)
                {
                    var institutionModifiers = institution.GetTaggedModifiers();
                    if (institutionModifiers != null)
                    {
                        foreach (var modifier in institutionModifiers)
                        {
                            // Re-tag with government ID as primary source
                            modifier.SourceID = governmentID;
                            modifier.SourceType = "Government";
                            allModifiers.Add(modifier);
                        }
                    }
                }
            }

            return allModifiers;
        }

        // ========== REGIME FORM HELPERS ==========

        /// <summary>
        /// Is this an autocratic government? (Single ruler)
        /// </summary>
        public bool IsAutocratic() => RegimeForm == RegimeForm.Autocratic;

        /// <summary>
        /// Is this a pluralist government? (Bureaucratic council)
        /// </summary>
        public bool IsPluralist() => RegimeForm == RegimeForm.Pluralist;

        /// <summary>
        /// Is this a chiefdom government? (Chief + assemblies)
        /// </summary>
        public bool IsChiefdom() => RegimeForm == RegimeForm.Chiefdom;

        /// <summary>
        /// Is this a confederation? (League of cities)
        /// </summary>
        public bool IsConfederation() => RegimeForm == RegimeForm.Confederation;

        /// <summary>
        /// Is this a league? (Trade league)
        /// </summary>
        public bool IsLeague() => RegimeForm == RegimeForm.League;

        // ========== STATE STRUCTURE HELPERS ==========

        /// <summary>
        /// Is this a territorial state? (Multi-province)
        /// </summary>
        public bool IsTerritorial() => StateStructure == StateStructure.Territorial;

        /// <summary>
        /// Is this a city-state? (Single city)
        /// </summary>
        public bool IsCityState() => StateStructure == StateStructure.CityState;

        /// <summary>
        /// Is this a nomadic state? (Mobile camps)
        /// </summary>
        public bool IsNomadic() => StateStructure == StateStructure.Nomadic;

        // ========== MILITARY SERVICE HELPERS ==========

        /// <summary>
        /// Can this government raise levies?
        /// </summary>
        public bool HasLevies() => (MilitaryService & MilitaryServiceLaw.Levies) != 0;

        /// <summary>
        /// Can this government use warbands?
        /// </summary>
        public bool HasWarbands() => (MilitaryService & MilitaryServiceLaw.Warbands) != 0;

        /// <summary>
        /// Can this government maintain professional armies?
        /// </summary>
        public bool HasProfessionalArmies() => (MilitaryService & MilitaryServiceLaw.ProfessionalArmies) != 0;

        // ========== INSTITUTION HELPERS ==========

        /// <summary>
        /// Check if this government has a specific institution.
        /// </summary>
        public bool HasInstitution(Institution institution)
        {
            return Institutions.Contains(institution);
        }

        /// <summary>
        /// Check if this government has an institution with a specific ability.
        /// </summary>
        public bool HasInstitutionAbility(InstitutionAbilities ability)
        {
            foreach (var institution in Institutions)
            {
                if (institution != null && institution.HasAbility(ability))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get all institutions of a specific category.
        /// </summary>
        public List<Institution> GetInstitutionsByCategory(InstitutionCategory category)
        {
            var results = new List<Institution>();
            foreach (var institution in Institutions)
            {
                if (institution != null && institution.Category == category)
                    results.Add(institution);
            }
            return results;
        }

        // ========== VALIDATION ==========

        /// <summary>
        /// Validate this government configuration.
        /// </summary>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(GovernmentName))
                errors.Add("Government must have a name");

            if (Institutions.Count > 3)
                errors.Add("Government cannot have more than 3 institutions");

            // Validate succession law matches regime form
            if (IsAutocratic())
            {
                if (SuccessionLaw == SuccessionLaw.Oligarchical ||
                    SuccessionLaw == SuccessionLaw.Republican ||
                    SuccessionLaw == SuccessionLaw.Democratic)
                {
                    errors.Add("Autocratic governments cannot use pluralist succession laws");
                }
            }

            if (IsPluralist())
            {
                if (SuccessionLaw == SuccessionLaw.Hereditary ||
                    SuccessionLaw == SuccessionLaw.Chosen)
                {
                    errors.Add("Pluralist governments should use appropriate succession laws");
                }
            }

            return errors;
        }

        /// <summary>
        /// Is this government configuration valid?
        /// </summary>
        public bool IsValid()
        {
            return GetValidationErrors().Count == 0;
        }

        // ========== EDITOR HELPERS ==========

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp institutions to max 3
            if (Institutions.Count > 3)
            {
                Debug.LogWarning($"[GovernmentData] {GovernmentName}: Institutions limited to 3. Removing excess.");
                Institutions.RemoveRange(3, Institutions.Count - 3);
            }

            // Log validation errors
            var errors = GetValidationErrors();
            if (errors.Count > 0)
            {
                Debug.LogWarning($"[GovernmentData] {GovernmentName} validation errors:\n" +
                                string.Join("\n", errors));
            }
        }
#endif
    }
}
