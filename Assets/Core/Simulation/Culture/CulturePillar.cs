using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;

namespace TWK.Cultures
{
    /// <summary>
    /// Cultural Pillars are defining characteristics of a culture.
    /// They are more powerful than regular tech nodes and define the culture's identity.
    /// </summary>
    [CreateAssetMenu(menuName = "TWK/Culture/Culture Pillar", fileName = "New Culture Pillar")]
    public class CulturePillar : ScriptableObject
    {
        // ========== IDENTITY ==========
        [Header("Identity")]
        public string PillarName;
        [TextArea(3, 6)]
        public string Description;
        public Sprite Icon;

        // ========== MODIFIERS ==========
        [Header("Powerful Modifiers")]
        [Tooltip("Pillars grant more powerful modifiers than regular tech nodes")]
        public List<CultureModifier> Modifiers = new List<CultureModifier>();

        // ========== BUILDING UNLOCKS ==========
        [Header("Building Unlocks")]
        [Tooltip("Buildings unlocked by this pillar (usually unique cultural buildings)")]
        public List<BuildingDefinition> UnlockedBuildings = new List<BuildingDefinition>();

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get unique ID for this pillar (uses Unity's instance ID).
        /// </summary>
        public int GetPillarID()
        {
            return GetInstanceID();
        }
    }
}
