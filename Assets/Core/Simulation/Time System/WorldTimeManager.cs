using UnityEngine;
using System;
using TWK.Realms;

namespace TWK.Simulation
{
    public class WorldTimeManager : MonoBehaviour
    {
        public static WorldTimeManager Instance { get; private set; }

        public TimeSystem timeSystem;

        // Events that subsystems can subscribe to
        public event Action OnDayTick;
        public event Action OnSeasonTick;
        public event Action OnYearTick;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);

                if (timeSystem == null)
                {                  
                    timeSystem = FindFirstObjectByType<TimeSystem>();
                    Initialize(timeSystem);                     
                }
                else
                {
                    Initialize(timeSystem);
                }
            }          
        }

        void Initialize(TimeSystem ts)
        {
            timeSystem = ts;
            // Subscribe to TimeSystem events
            timeSystem.OnDayAdvanced += HandleDayAdvanced;
            timeSystem.OnSeasonChanged += HandleSeasonChanged;
            timeSystem.OnYearChanged += HandleYearChanged;
        }

        private void HandleDayAdvanced(int day, GameYear year, string season)
        {
           //Debug.Log("Day tick triggered");
            OnDayTick?.Invoke();
            ProcessDailyWorldUpdates();
        }

        private void HandleSeasonChanged(string season, GameYear year)
        {
            OnSeasonTick?.Invoke();
            ProcessSeasonalUpdates();
        }

        private void HandleYearChanged(GameYear year)
        {
            OnYearTick?.Invoke();
            ProcessYearlyUpdates();
        }

        private void ProcessDailyWorldUpdates()
        {
            // Collect realm taxes and tribute daily
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.CollectAllRealmRevenue();
            }
        }

        private void ProcessSeasonalUpdates()
        {
            // Pay office salaries quarterly (4 seasons per year)
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.PayAllOfficeSalaries();
            }
        }

        private void ProcessYearlyUpdates()
        {
            // Yearly world updates can go here
        }

        // ========== TIME CONTROL HELPERS ==========

        public void AdvanceDay()
        {
            if (timeSystem != null)
            {
                // Manually trigger day advance
                HandleDayAdvanced(timeSystem.CurrentDay + 1, timeSystem.CurrentYear, timeSystem.CurrentSeason);
            }
        }

        public void SetPaused(bool paused)
        {
            if (timeSystem != null)
            {
                timeSystem.SetTimeScale(paused ? 0f : 1f);
            }
        }

        public void SetTimeScale(float scale)
        {
            if (timeSystem != null)
            {
                timeSystem.SetTimeScale(scale);
            }
        }

        public bool IsPaused => timeSystem != null && timeSystem.CurrentMultiplier == 0f;

        public float GetTimeScale()
        {
            return timeSystem?.CurrentMultiplier ?? 0f;
        }

        public int CurrentDay => timeSystem?.CurrentDay ?? 0;
        public string CurrentSeason => timeSystem?.CurrentSeason ?? "Spring";
        public int CurrentYear => timeSystem?.CurrentYear.Year ?? 0;
    }
}

