using UnityEngine;
using TWK.Simulation;

namespace TWK.Simulation
{
    public class TimeDebug : MonoBehaviour
    {
        [SerializeField] private TimeSystem timeSystem;

        private void OnEnable()
        {
            timeSystem.OnDayAdvanced += LogDay;
            timeSystem.OnSeasonChanged += LogSeason;
            timeSystem.OnYearChanged += LogYear;
        }

        private void OnDisable()
        {
            timeSystem.OnDayAdvanced -= LogDay;
            timeSystem.OnSeasonChanged -= LogSeason;
            timeSystem.OnYearChanged -= LogYear;
        }

        // Updated to use GameYear
        private void LogDay(int day, GameYear year, string season)
        {
            Debug.Log($"Day {day} of {season}, Year {year}");
        }

        private void LogSeason(string season, GameYear year)
        {
            Debug.Log($"Season changed to {season}, Year {year}");
        }

        private void LogYear(GameYear year)
        {
            Debug.Log($"Year {year} started");
        }
    }
}
