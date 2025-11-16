using System.Collections.Generic;
using UnityEngine;

namespace TWK.Religion
{
    /// <summary>
    /// Represents a deity/god within a religion.
    /// A religion can be monotheistic (1 deity) or polytheistic (multiple deities).
    /// </summary>
    [System.Serializable]
    public class Deity
    {
        [Header("Identity")]
        [Tooltip("Name of the deity")]
        public string Name;

        [Tooltip("Title or epithet (e.g., 'God of War', 'The Merciful')")]
        public string Title;

        [Tooltip("Description of this deity's domain and personality")]
        [TextArea(3, 5)]
        public string Description;

        [Header("Mythos")]
        [Tooltip("The core mythology/story of this deity")]
        [TextArea(3, 8)]
        public string Mythos;

        [Tooltip("Famous traits or aspects of this deity (e.g., 'Vengeful', 'Wise', 'Trickster')")]
        public List<string> FamousTraits = new List<string>();

        [Header("Holy Lands")]
        [Tooltip("Territories considered holy to this deity")]
        public List<HolyLand> HolyLands = new List<HolyLand>();

        [Header("Festivals")]
        [Tooltip("Festivals celebrating this deity")]
        public List<Festival> Festivals = new List<Festival>();

        [Header("Rituals")]
        [Tooltip("Sacred rituals performed for this deity")]
        public List<Ritual> Rituals = new List<Ritual>();

        [Header("Visual")]
        [Tooltip("Icon representing this deity")]
        public Sprite Icon;

        [Tooltip("Color associated with this deity")]
        public Color DeityColor = Color.white;
    }

    /// <summary>
    /// Represents a territory sacred to a deity or religion.
    /// </summary>
    [System.Serializable]
    public class HolyLand
    {
        [Tooltip("Name of the holy land")]
        public string Name;

        [Tooltip("Why is this land sacred?")]
        [TextArea(2, 4)]
        public string Significance;

        [Tooltip("How important is this holy land?")]
        public HolyLandImportance Importance;

        [Tooltip("Province ID of the holy land (if applicable)")]
        public int ProvinceID = -1;

        [Tooltip("Fervor bonus for controlling this holy land")]
        public float FervorBonus = 5f;

        [Tooltip("Piety gained per season for controlling this holy land")]
        public int PietyPerSeason = 10;
    }

    /// <summary>
    /// Represents a religious festival.
    /// </summary>
    [System.Serializable]
    public class Festival
    {
        [Tooltip("Name of the festival")]
        public string Name;

        [Tooltip("Description of what happens during this festival")]
        [TextArea(2, 4)]
        public string Description;

        [Tooltip("How often does this festival occur?")]
        public FestivalFrequency Frequency;

        [Tooltip("Which season does this festival occur in? (0=Spring, 1=Summer, 2=Fall, 3=Winter)")]
        [Range(0, 3)]
        public int Season = 0;

        [Tooltip("Happiness bonus during festival")]
        public float HappinessBonus = 5f;

        [Tooltip("Fervor bonus during festival")]
        public float FervorBonus = 3f;

        [Tooltip("Gold cost to host festival properly")]
        public int GoldCost = 50;
    }

    /// <summary>
    /// Represents a religious ritual.
    /// </summary>
    [System.Serializable]
    public class Ritual
    {
        [Tooltip("Name of the ritual")]
        public string Name;

        [Tooltip("Description of the ritual")]
        [TextArea(2, 4)]
        public string Description;

        [Tooltip("What does this ritual do mechanically?")]
        public RitualEffect Effect;

        [Tooltip("Piety cost to perform")]
        public int PietyCost = 100;

        [Tooltip("How often can this be performed? (in days)")]
        public int Cooldown = 365;
    }

    /// <summary>
    /// Mechanical effects of performing a ritual.
    /// </summary>
    [System.Serializable]
    public class RitualEffect
    {
        [Tooltip("Fervor gained")]
        public float FervorGain = 0f;

        [Tooltip("Happiness gained")]
        public float HappinessGain = 0f;

        [Tooltip("Stability gained")]
        public float StabilityGain = 0f;

        [Tooltip("Prestige gained")]
        public int PrestigeGain = 0;

        [Tooltip("Can this ritual trigger a special event?")]
        public bool CanTriggerEvent = false;

        [Tooltip("Event ID to trigger (if applicable)")]
        public string EventID = "";
    }
}
