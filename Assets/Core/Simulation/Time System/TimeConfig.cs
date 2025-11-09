using UnityEngine;

namespace TWK.Simulation
{
    [CreateAssetMenu(menuName = "Simulation/Time/Time Config", fileName = "TimeConfig", order = 1)]

    public class TimeConfig : ScriptableObject
    {
        [Header("Time Ratios")]
        [Tooltip("Real-time minutes that pass per in-game day at 1x speed.")]
        public float realMinutesPerGameDay = 15f;

        [Tooltip("Number of in-game days per season.")]
        public int daysPerSeason = 90;

        [Tooltip("Number of seasons per in-game year.")]
        public int seasonsPerYear = 4;

        [Header("Calendar Labels")]
        public string[] seasonNames = { "Spring", "Summer", "Autumn", "Winter" };

        [Header("Starting Date")]
        public GameYear startingYear = new GameYear { year = 343, era = Era.BC };
        public int startingSeasonIndex = 0;
        public int startingDay = 1;

        [Header("Speed Multipliers")]
        public float[] timeMultipliers = { 1f, 2f, 4f, 10f, 60f };

        [Tooltip("Maximum real-time multiplier (for accelerated sim).")]
        public float maxSpeedMultiplier = 60f;
    }
}
