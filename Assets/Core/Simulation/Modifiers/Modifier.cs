using System.Collections.Generic;
using UnityEngine;

namespace TWK.Modifiers
{
    /// <summary>
    /// Represents a collection of effects that modify gameplay.
    /// Used by culture pillars, religion tenets, buildings, events, etc.
    /// </summary>
    [System.Serializable]
    public class Modifier
    {
        [Header("Identity")]
        [Tooltip("Name of this modifier (e.g., 'Fertile Lands', 'Divine Blessing')")]
        public string Name;

        [TextArea(2, 4)]
        [Tooltip("Human-readable description of what this modifier does")]
        public string Description;

        [Header("Duration")]
        [Tooltip("How long does this modifier last?")]
        public ModifierDuration Duration = ModifierDuration.Permanent;

        [Tooltip("Duration in days (-1 for permanent)")]
        public int DurationDays = -1;

        [Header("Effects")]
        [Tooltip("List of effects this modifier provides")]
        public List<ModifierEffect> Effects = new List<ModifierEffect>();

        // ========== RUNTIME DATA (NOT SERIALIZED) ==========

        /// <summary>
        /// When was this modifier applied? (Used for timed modifiers)
        /// </summary>
        [System.NonSerialized]
        public int AppliedOnDay = -1;

        /// <summary>
        /// Source ID - what entity provided this modifier?
        /// (CultureID, ReligionID, BuildingID, EventID, etc.)
        /// </summary>
        [System.NonSerialized]
        public int SourceID = -1;

        /// <summary>
        /// Source type - what kind of entity provided this modifier?
        /// </summary>
        [System.NonSerialized]
        public string SourceType = "";

        // ========== METHODS ==========

        /// <summary>
        /// Create a new modifier instance with the given parameters.
        /// </summary>
        public Modifier(string name, string description, ModifierDuration duration = ModifierDuration.Permanent)
        {
            Name = name;
            Description = description;
            Duration = duration;
            DurationDays = duration == ModifierDuration.Permanent ? -1 : 0;
        }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Modifier()
        {
        }

        /// <summary>
        /// Add an effect to this modifier.
        /// </summary>
        public Modifier AddEffect(ModifierEffect effect)
        {
            Effects.Add(effect);
            return this; // Fluent interface for chaining
        }

        /// <summary>
        /// Is this modifier still active?
        /// </summary>
        public bool IsActive(int currentDay)
        {
            if (Duration == ModifierDuration.Permanent)
                return true;

            if (Duration == ModifierDuration.Timed)
            {
                if (AppliedOnDay == -1)
                    return true; // Not yet applied, treat as active

                return currentDay < AppliedOnDay + DurationDays;
            }

            // Conditional modifiers always check their condition (future)
            return true;
        }

        /// <summary>
        /// Get all effects of a specific type.
        /// </summary>
        public List<ModifierEffect> GetEffectsOfType(ModifierEffectType effectType)
        {
            var results = new List<ModifierEffect>();
            foreach (var effect in Effects)
            {
                if (effect.EffectType == effectType)
                    results.Add(effect);
            }
            return results;
        }

        /// <summary>
        /// Get all effects targeting a specific scope.
        /// </summary>
        public List<ModifierEffect> GetEffectsForTarget(ModifierTarget target)
        {
            var results = new List<ModifierEffect>();
            foreach (var effect in Effects)
            {
                if (effect.Target == target)
                    results.Add(effect);
            }
            return results;
        }

        /// <summary>
        /// Get a full description including all effects.
        /// </summary>
        public string GetFullDescription()
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;

            // Auto-generate description from effects
            var lines = new List<string>();
            foreach (var effect in Effects)
            {
                lines.Add(effect.GetDescription());
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Clone this modifier (useful for instancing timed modifiers).
        /// </summary>
        public Modifier Clone()
        {
            var clone = new Modifier(Name, Description, Duration);
            clone.DurationDays = DurationDays;

            foreach (var effect in Effects)
            {
                clone.Effects.Add(effect); // Note: Shallow copy of effects
            }

            return clone;
        }
    }
}
