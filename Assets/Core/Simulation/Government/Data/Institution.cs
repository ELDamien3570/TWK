using System.Collections.Generic;
using UnityEngine;
using TWK.Modifiers;
using TWK.Economy;

namespace TWK.Government
{
    /// <summary>
    /// ScriptableObject defining a government institution.
    /// Institutions are powerful government capabilities that provide modifiers,
    /// unlock buildings, and grant special abilities.
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Government/Institution", fileName = "New Institution")]
    public class Institution : ScriptableObject
    {
        // ========== IDENTITY ==========
        [Header("Identity")]
        [Tooltip("Name of this institution (e.g., 'Standing Army', 'Royal Roads')")]
        public string InstitutionName;

        [TextArea(3, 6)]
        [Tooltip("Description of what this institution provides")]
        public string Description;

        [Tooltip("Category for organization")]
        public InstitutionCategory Category = InstitutionCategory.Diplomacy;

        [Tooltip("Icon representing this institution")]
        public Sprite Icon;

        // ========== MODIFIERS ==========
        [Header("Effects")]
        [Tooltip("Modifiers provided by this institution")]
        public List<Modifier> Modifiers = new List<Modifier>();

        // ========== UNLOCKS ==========
        [Header("Unlocks")]
        [Tooltip("Buildings unlocked by adopting this institution")]
        public List<BuildingDefinition> BuildingUnlocks = new List<BuildingDefinition>();

        [Tooltip("Special abilities granted by this institution")]
        public InstitutionAbilities SpecialAbilities = InstitutionAbilities.None;

        // ========== COSTS ==========
        [Header("Costs")]
        [Tooltip("Gold cost to add this institution during government reform")]
        public int ReformCost = 1000;

        // ========== STABLE ID ==========
        private int? cachedStableID = null;

        /// <summary>
        /// Get a stable ID for this institution based on its name.
        /// Used for save/load and modifier tracking.
        /// </summary>
        public int GetStableInstitutionID()
        {
            if (cachedStableID.HasValue)
                return cachedStableID.Value;

            cachedStableID = InstitutionName.GetHashCode();
            return cachedStableID.Value;
        }

        /// <summary>
        /// Get all modifiers from this institution, properly tagged with source info.
        /// </summary>
        public List<Modifier> GetTaggedModifiers()
        {
            var taggedModifiers = new List<Modifier>();
            int institutionID = GetStableInstitutionID();

            foreach (var modifier in Modifiers)
            {
                if (modifier != null)
                {
                    // Clone the modifier to avoid modifying the template
                    var tagged = modifier.Clone();
                    if (tagged.SourceID == -1)
                    {
                        tagged.SourceID = institutionID;
                        tagged.SourceType = "Institution";
                    }
                    taggedModifiers.Add(tagged);
                }
            }

            return taggedModifiers;
        }

        /// <summary>
        /// Check if this institution has a specific special ability.
        /// </summary>
        public bool HasAbility(InstitutionAbilities ability)
        {
            return (SpecialAbilities & ability) != 0;
        }

        /// <summary>
        /// Get a formatted description including abilities and unlocks.
        /// </summary>
        public string GetFullDescription()
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(Description))
                parts.Add(Description);

            // Special abilities
            if (SpecialAbilities != InstitutionAbilities.None)
            {
                parts.Add("\n<b>Special Abilities:</b>");
                var abilities = System.Enum.GetValues(typeof(InstitutionAbilities));
                foreach (InstitutionAbilities ability in abilities)
                {
                    if (ability != InstitutionAbilities.None && HasAbility(ability))
                    {
                        parts.Add($"• {ability}");
                    }
                }
            }

            // Building unlocks
            if (BuildingUnlocks != null && BuildingUnlocks.Count > 0)
            {
                parts.Add("\n<b>Unlocks Buildings:</b>");
                foreach (var building in BuildingUnlocks)
                {
                    if (building != null)
                        parts.Add($"• {building.BuildingName}");
                }
            }

            // Modifiers
            if (Modifiers != null && Modifiers.Count > 0)
            {
                parts.Add("\n<b>Effects:</b>");
                foreach (var modifier in Modifiers)
                {
                    if (modifier != null)
                        parts.Add($"• {modifier.Name}");
                }
            }

            return string.Join("\n", parts);
        }
    }
}
