using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using TWK.Modifiers;

namespace TWK.UI.Common
{
    /// <summary>
    /// Reusable UI component for displaying a list of active modifiers.
    /// Can be used in city UI, realm UI, character UI, religion UI, etc.
    /// </summary>
    public class ModifierDisplayPanel : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private TextMeshProUGUI modifierListText;

      //  [Tooltip("If true, groups modifiers by source (culture, religion, events)")]  
       // [SerializeField] private bool groupBySource = true;

        [Tooltip("If true, shows the source of each modifier")]
        [SerializeField] private bool showSource = true;

        [Tooltip("Maximum number of modifiers to display (0 = unlimited)")]
        [SerializeField] private int maxDisplay = 0;

        [Header("Empty State")]
        [SerializeField] private string emptyMessage = "No active modifiers";

        /// <summary>
        /// Display a list of modifiers with optional source information.
        /// </summary>
        public void DisplayModifiers(List<Modifier> modifiers)
        {
            if (modifierListText == null)
            {
                Debug.LogWarning("[ModifierDisplayPanel] ModifierListText not assigned!");
                return;
            }

            if (modifiers == null || modifiers.Count == 0)
            {
                modifierListText.text = emptyMessage;
                return;
            }

            var sb = new StringBuilder();
            int displayCount = maxDisplay > 0 ? Mathf.Min(modifiers.Count, maxDisplay) : modifiers.Count;

            for (int i = 0; i < displayCount; i++)
            {
                var modifier = modifiers[i];

                // Modifier name
                sb.Append($"<b>{modifier.Name}</b>");

                // Source (if enabled)
                if (showSource && !string.IsNullOrEmpty(modifier.SourceType))
                {
                    sb.Append($" <color=#888888>[{modifier.SourceType}]</color>");
                }

                sb.AppendLine();

                // Effects
                foreach (var effect in modifier.Effects)
                {
                    sb.AppendLine($"  • {effect.GetDescription()}");
                }

                // Add spacing between modifiers
                if (i < displayCount - 1)
                    sb.AppendLine();
            }

            // Show truncation message if there are more modifiers
            if (maxDisplay > 0 && modifiers.Count > maxDisplay)
            {
                sb.AppendLine($"\n<color=#888888>... and {modifiers.Count - maxDisplay} more</color>");
            }

            modifierListText.text = sb.ToString();
        }

        /// <summary>
        /// Display modifiers grouped by source type.
        /// </summary>
        public void DisplayModifiersGrouped(List<Modifier> modifiers)
        {
            if (modifierListText == null)
            {
                Debug.LogWarning("[ModifierDisplayPanel] ModifierListText not assigned!");
                return;
            }

            if (modifiers == null || modifiers.Count == 0)
            {
                modifierListText.text = emptyMessage;
                return;
            }

            // Group modifiers by source type
            var groupedModifiers = new Dictionary<string, List<Modifier>>();

            foreach (var modifier in modifiers)
            {
                string sourceType = string.IsNullOrEmpty(modifier.SourceType) ? "Other" : modifier.SourceType;

                if (!groupedModifiers.ContainsKey(sourceType))
                    groupedModifiers[sourceType] = new List<Modifier>();

                groupedModifiers[sourceType].Add(modifier);
            }

            // Build display string
            var sb = new StringBuilder();
            bool first = true;

            foreach (var group in groupedModifiers)
            {
                if (!first)
                    sb.AppendLine();

                // Group header
                sb.AppendLine($"<b><color=#FFA500>{group.Key}</color></b>");

                // Modifiers in this group
                foreach (var modifier in group.Value)
                {
                    sb.AppendLine($"  <b>{modifier.Name}</b>");

                    foreach (var effect in modifier.Effects)
                    {
                        sb.AppendLine($"    • {effect.GetDescription()}");
                    }
                }

                first = false;
            }

            modifierListText.text = sb.ToString();
        }

        /// <summary>
        /// Display modifiers from multiple sources with category headers.
        /// </summary>
        public void DisplayModifiersByCategory(
            List<Modifier> cultureModifiers = null,
            List<Modifier> religionModifiers = null,
            List<Modifier> eventModifiers = null,
            List<Modifier> buildingModifiers = null)
        {
            if (modifierListText == null)
            {
                Debug.LogWarning("[ModifierDisplayPanel] ModifierListText not assigned!");
                return;
            }

            var sb = new StringBuilder();
            bool hasAny = false;

            // Culture modifiers
            if (cultureModifiers != null && cultureModifiers.Count > 0)
            {
                sb.AppendLine("<b><color=#4A90E2>Culture</color></b>");
                AppendModifierList(sb, cultureModifiers);
                hasAny = true;
            }

            // Religion modifiers
            if (religionModifiers != null && religionModifiers.Count > 0)
            {
                if (hasAny) sb.AppendLine();
                sb.AppendLine("<b><color=#FFD700>Religion</color></b>");
                AppendModifierList(sb, religionModifiers);
                hasAny = true;
            }

            // Event modifiers (timed)
            if (eventModifiers != null && eventModifiers.Count > 0)
            {
                if (hasAny) sb.AppendLine();
                sb.AppendLine("<b><color=#FF6B6B>Events</color></b>");
                AppendModifierList(sb, eventModifiers);
                hasAny = true;
            }

            // Building modifiers
            if (buildingModifiers != null && buildingModifiers.Count > 0)
            {
                if (hasAny) sb.AppendLine();
                sb.AppendLine("<b><color=#50C878>Buildings</color></b>");
                AppendModifierList(sb, buildingModifiers);
                hasAny = true;
            }

            modifierListText.text = hasAny ? sb.ToString() : emptyMessage;
        }

        /// <summary>
        /// Helper method to append a list of modifiers to a StringBuilder.
        /// </summary>
        private void AppendModifierList(StringBuilder sb, List<Modifier> modifiers)
        {
            foreach (var modifier in modifiers)
            {
                sb.AppendLine($"  <b>{modifier.Name}</b>");

                foreach (var effect in modifier.Effects)
                {
                    sb.AppendLine($"    • {effect.GetDescription()}");
                }
            }
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        public void Clear()
        {
            if (modifierListText != null)
                modifierListText.text = emptyMessage;
        }
    }
}
