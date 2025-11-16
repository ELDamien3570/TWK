using UnityEngine;
using System;

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

        }

        private void ProcessSeasonalUpdates()
        {

        }

        private void ProcessYearlyUpdates()
        {

        }
    }
}

