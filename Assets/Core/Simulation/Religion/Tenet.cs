using System.Collections.Generic;
using UnityEngine;
using TWK.Modifiers;

namespace TWK.Religion
{
    /// <summary>
    /// Represents a religious doctrine or tenet.
    /// Tenets define the beliefs and rules of a religion, and provide modifiers.
    /// Examples: "Monastic Tradition" (+education, -happiness), "Warrior Faith" (+military power)
    /// </summary>
    [System.Serializable]
    public class Tenet
    {
        [Header("Identity")]
        [Tooltip("Name of the tenet")]
        public string Name;

        [Tooltip("Description of what this tenet teaches")]
        [TextArea(3, 5)]
        public string Description;

        [Tooltip("Category for organization")]
        public TenetCategory Category;

        [Header("Modifiers")]
        [Tooltip("Gameplay effects of following this tenet")]
        public List<Modifier> Modifiers = new List<Modifier>();

        [Header("Special Rules")]
        [Tooltip("Does this tenet have special scripted effects? (e.g., 'Cannot marry outside faith')")]
        public bool HasSpecialRules = false;

        [Tooltip("Description of special rules")]
        [TextArea(2, 4)]
        public string SpecialRulesDescription = "";

        [Header("Visual")]
        [Tooltip("Icon for this tenet")]
        public Sprite Icon;

        /// <summary>
        /// Get all modifiers from this tenet.
        /// </summary>
        public List<Modifier> GetModifiers()
        {
            var mods = new List<Modifier>(Modifiers);

            // Tag modifiers with their source
            foreach (var mod in mods)
            {
                if (string.IsNullOrEmpty(mod.SourceType))
                {
                    mod.SourceType = "Tenet";
                }
            }

            return mods;
        }

        /// <summary>
        /// Get a summary of this tenet's effects.
        /// </summary>
        public string GetEffectSummary()
        {
            if (Modifiers == null || Modifiers.Count == 0)
            {
                if (HasSpecialRules)
                    return SpecialRulesDescription;
                return "No mechanical effects";
            }

            var summary = new System.Text.StringBuilder();

            foreach (var modifier in Modifiers)
            {
                foreach (var effect in modifier.Effects)
                {
                    summary.AppendLine(effect.GetDescription());
                }
            }

            if (HasSpecialRules)
            {
                summary.AppendLine();
                summary.AppendLine(SpecialRulesDescription);
            }

            return summary.ToString().TrimEnd();
        }
    }
}
