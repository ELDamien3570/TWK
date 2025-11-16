using System;
using UnityEngine;

namespace TWK.Simulation
{
    public class TimeSystem : MonoBehaviour
    {
        [SerializeField] private TimeConfig config;

        private int dayOfSeason;
        private int currentSeasonIndex;
        private GameYear currentYear;

        private float elapsedRealSeconds = 0f;
        private float SecondsPerDay => config.realMinutesPerGameDay * 60f;

        private float currentMultiplier = 1f;

        public string CurrentSeason => config.seasonNames[currentSeasonIndex];
        public int CurrentSeasonIndex => currentSeasonIndex;
        public int CurrentDay => dayOfSeason;
        public GameYear CurrentYear => currentYear;

        public event Action<int, GameYear, string> OnDayAdvanced;
        public event Action<string, GameYear> OnSeasonChanged;
        public event Action<GameYear> OnYearChanged;
        public event Action<float> OnTimeScaleChanged;

        /// <summary>
        /// Returns the fraction of the current day that has passed.
        /// 0 = start of day, 1 = end of day (rolls over to next day)
        /// </summary>
        public float TimeOfDayNormalized => Mathf.Clamp01(elapsedRealSeconds / SecondsPerDay);
        public bool IsDay => TimeOfDayNormalized < 0.5f;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            dayOfSeason = Mathf.Clamp(config.startingDay, 1, config.daysPerSeason);
            currentSeasonIndex = Mathf.Clamp(config.startingSeasonIndex, 0, config.seasonsPerYear - 1);
            currentYear = config.startingYear;

            OnDayAdvanced?.Invoke(dayOfSeason, currentYear, CurrentSeason);
            OnSeasonChanged?.Invoke(CurrentSeason, currentYear);
            OnYearChanged?.Invoke(currentYear);
        }

        private void Update()
        {
            elapsedRealSeconds += Time.deltaTime * currentMultiplier;

            while (elapsedRealSeconds >= SecondsPerDay)
            {
                elapsedRealSeconds -= SecondsPerDay;
                AdvanceDay();
            }

            //Debug.Log($"Elapsed seconds: {elapsedRealSeconds}, currentMultiplier: {currentMultiplier}");
        }

        private void AdvanceDay()
        {
            dayOfSeason++;

            if (dayOfSeason > config.daysPerSeason)
            {
                dayOfSeason = 1;
                currentSeasonIndex++;

                if (currentSeasonIndex >= config.seasonsPerYear)
                {
                    currentSeasonIndex = 0;
                    currentYear.AdvanceYear();
                    OnYearChanged?.Invoke(currentYear);
                }

                OnSeasonChanged?.Invoke(CurrentSeason, currentYear);
            }

            OnDayAdvanced?.Invoke(dayOfSeason, currentYear, CurrentSeason);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            currentMultiplier = Mathf.Clamp(multiplier, 0.1f, config.maxSpeedMultiplier);
            OnTimeScaleChanged?.Invoke(currentMultiplier);
        }

        public void SetTimeScale(float newScale)
        {
            currentMultiplier = (newScale == 0f) ? 0f : Mathf.Clamp(newScale, 0.1f, config.maxSpeedMultiplier);
            OnTimeScaleChanged?.Invoke(currentMultiplier);
        }
    }
}
