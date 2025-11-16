using System.Collections.Generic;
using UnityEngine;
using TWK.Modifiers;

namespace TWK.Religion
{
    /// <summary>
    /// Defines a complete religion with its beliefs, deities, tenets, and identity.
    /// </summary>
    [CreateAssetMenu(fileName = "New Religion", menuName = "TWK/Religion/Religion Data")]
    public class ReligionData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Name of the religion")]
        public string ReligionName;

        [Tooltip("Short description of the religion's core beliefs")]
        [TextArea(3, 6)]
        public string Description;

        [Tooltip("Is this a mainstream religion or a cult?")]
        public ReligionType ReligionType = ReligionType.MainlineReligion;

        [Header("Deities")]
        [Tooltip("The god(s) worshipped by this religion")]
        public List<Deity> Deities = new List<Deity>();

        [Header("Religious Identity")]
        [Tooltip("How is knowledge transmitted?")]
        public ReligionTradition Tradition = ReligionTradition.Written;

        [Tooltip("How organized is the religion?")]
        public ReligionCentralization Centralization = ReligionCentralization.Decentralized;

        [Tooltip("How does the religion view conversion of others?")]
        public ReligionEvangelism Evangelism = ReligionEvangelism.Isolationist;

        [Tooltip("How open is the religion to foreign beliefs?")]
        public ReligionSyncretism Syncretism = ReligionSyncretism.Rigid;

        [Header("Head of Faith (Centralized Only)")]
        [Tooltip("Title for the religious leader (e.g., 'Pope', 'Caliph', 'High Priest')")]
        public string HeadOfFaithTitle = "High Priest";

        [Tooltip("Powers the Head of Faith has (only if centralized)")]
        public HeadOfFaithPowers HeadPowers = HeadOfFaithPowers.None;

        [Header("Tenets")]
        [Tooltip("Core religious doctrines that provide modifiers and define behavior")]
        public List<Tenet> Tenets = new List<Tenet>();

        [Header("Conversion")]
        [Tooltip("Base conversion resistance (0-100). Higher = harder to convert away from this religion")]
        [Range(0, 100)]
        public float ConversionResistance = 50f;

        [Tooltip("Conversion speed modifier (multiplier for how fast pops convert TO this religion)")]
        [Range(0.1f, 3f)]
        public float ConversionSpeed = 1f;

        [Header("Fervor")]
        [Tooltip("Base fervor level for new converts (0-100)")]
        [Range(0, 100)]
        public float BaseFervor = 50f;

        [Tooltip("Fervor decay rate per season")]
        [Range(0f, 10f)]
        public float FervorDecayRate = 1f;

        [Header("Visual")]
        [Tooltip("Icon representing this religion")]
        public Sprite ReligionIcon;

        [Tooltip("Color associated with this religion")]
        public Color ReligionColor = Color.white;

        // ========== RUNTIME ID ==========
        // We need a stable ID for religions similar to building definitions

        private int? cachedStableID = null;

        /// <summary>
        /// Get a stable, persistent ID for this religion based on its asset name.
        /// This ID remains consistent across play sessions.
        /// </summary>
        public int GetStableReligionID()
        {
            if (cachedStableID.HasValue)
                return cachedStableID.Value;

            // Use asset name hash for persistent ID
            cachedStableID = ReligionName.GetHashCode();
            return cachedStableID.Value;
        }

        // ========== MODIFIER ACCESS ==========

        /// <summary>
        /// Get all modifiers provided by this religion's tenets.
        /// </summary>
        public List<Modifier> GetAllModifiers()
        {
            var allModifiers = new List<Modifier>();

            foreach (var tenet in Tenets)
            {
                if (tenet != null)
                {
                    allModifiers.AddRange(tenet.GetModifiers());
                }
            }

            // Tag all modifiers with religion source
            foreach (var mod in allModifiers)
            {
                if (string.IsNullOrEmpty(mod.SourceID))
                {
                    mod.SourceID = GetStableReligionID().ToString();
                }
            }

            return allModifiers;
        }

        /// <summary>
        /// Get all modifiers of a specific effect type.
        /// </summary>
        public List<ModifierEffect> GetEffectsOfType(ModifierEffectType effectType)
        {
            var effects = new List<ModifierEffect>();

            foreach (var modifier in GetAllModifiers())
            {
                effects.AddRange(modifier.GetEffectsOfType(effectType));
            }

            return effects;
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Is this religion centralized?
        /// </summary>
        public bool IsCentralized()
        {
            return Centralization == ReligionCentralization.Centralized;
        }

        /// <summary>
        /// Is this a cult?
        /// </summary>
        public bool IsCult()
        {
            return ReligionType == ReligionType.Cult;
        }

        /// <summary>
        /// Is this religion evangelical?
        /// </summary>
        public bool IsEvangelical()
        {
            return Evangelism == ReligionEvangelism.Evangelical;
        }

        /// <summary>
        /// Is this religion syncretic?
        /// </summary>
        public bool IsSyncretic()
        {
            return Syncretism == ReligionSyncretism.Syncretic;
        }

        /// <summary>
        /// Get a formatted description of this religion's identity.
        /// </summary>
        public string GetIdentitySummary()
        {
            var summary = new System.Text.StringBuilder();

            summary.AppendLine($"<b>{ReligionName}</b>");
            summary.AppendLine($"<i>{Description}</i>");
            summary.AppendLine();

            summary.AppendLine($"Tradition: {Tradition}");
            summary.AppendLine($"Organization: {Centralization}");
            summary.AppendLine($"Conversion: {Evangelism}");
            summary.AppendLine($"Syncretism: {Syncretism}");

            if (IsCentralized())
            {
                summary.AppendLine($"Head of Faith: {HeadOfFaithTitle}");
            }

            return summary.ToString();
        }

        /// <summary>
        /// Get all holy lands from all deities.
        /// </summary>
        public List<HolyLand> GetAllHolyLands()
        {
            var holyLands = new List<HolyLand>();

            foreach (var deity in Deities)
            {
                if (deity != null && deity.HolyLands != null)
                {
                    holyLands.AddRange(deity.HolyLands);
                }
            }

            return holyLands;
        }

        /// <summary>
        /// Get all festivals from all deities.
        /// </summary>
        public List<Festival> GetAllFestivals()
        {
            var festivals = new List<Festival>();

            foreach (var deity in Deities)
            {
                if (deity != null && deity.Festivals != null)
                {
                    festivals.AddRange(deity.Festivals);
                }
            }

            return festivals;
        }

        /// <summary>
        /// Get all rituals from all deities.
        /// </summary>
        public List<Ritual> GetAllRituals()
        {
            var rituals = new List<Ritual>();

            foreach (var deity in Deities)
            {
                if (deity != null && deity.Rituals != null)
                {
                    rituals.AddRange(deity.Rituals);
                }
            }

            return rituals;
        }
    }
}
