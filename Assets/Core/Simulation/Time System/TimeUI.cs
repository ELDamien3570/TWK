using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace TWK.Simulation
{
    public class TimeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TimeSystem timeSystem;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speed1xButton;
        [SerializeField] private Button speed2xButton;
        [SerializeField] private Button speed4xButton;
        [SerializeField] private Button speed8xButton;

        [Header("Day/Night Visual")]
        [SerializeField] private Image dayNightOverlay; // assign the UI Image you want to tint
        [SerializeField] private Color dayColor = new Color(1f, 1f, 1f, 0f); // fully transparent during day
        [SerializeField] private Color nightColor = new Color(0f, 0f, 0.2f, 0.5f); // semi-dark at night

        private void OnEnable()
        {
            if (timeSystem == null)
            {
                Debug.LogError("TimeSystem reference missing on TimeUI.");
                return;
            }

            // Subscribe to time events
            timeSystem.OnDayAdvanced += UpdateTimeDisplay;
            timeSystem.OnSeasonChanged += UpdateTimeDisplay;
            timeSystem.OnYearChanged += UpdateTimeDisplay;

            // Wire up the speed control buttons
            pauseButton.onClick.AddListener(() => timeSystem.SetTimeScale(0));
            speed1xButton.onClick.AddListener(() => timeSystem.SetTimeScale(1));
            speed2xButton.onClick.AddListener(() => timeSystem.SetTimeScale(2));
            speed4xButton.onClick.AddListener(() => timeSystem.SetTimeScale(4));
            speed8xButton.onClick.AddListener(() => timeSystem.SetTimeScale(8));

            UpdateTimeDisplay();
        }

        private void OnDisable()
        {
            if (timeSystem == null) return;

            timeSystem.OnDayAdvanced -= UpdateTimeDisplay;
            timeSystem.OnSeasonChanged -= UpdateTimeDisplay;
            timeSystem.OnYearChanged -= UpdateTimeDisplay;
        }

        private void Update()
        {
            UpdateDayNightOverlay();
        }

        private string GetSeasonColor(string season)
        {
            return season switch
            {
                "Spring" => "#00FF00",
                "Summer" => "#FFD700",
                "Autumn" => "#FF8C00",
                "Winter" => "#ADD8E6",
                _ => "#FFFFFF"
            };
        }

        private void UpdateTimeDisplay()
        {
            string color = GetSeasonColor(timeSystem.CurrentSeason);
            string styledSeason = $"<color={color}><b>{timeSystem.CurrentSeason}</b></color>";
            timeText.text = $"{timeSystem.CurrentYear}  {styledSeason}  Day {timeSystem.CurrentDay}";


        }

        private void UpdateDayNightOverlay()
        {
            if (dayNightOverlay == null) return;

            float t = timeSystem.TimeOfDayNormalized;

            Color targetColor;

            if (t < 0.25f) // morning: fade from night to day
                targetColor = Color.Lerp(nightColor, dayColor, t / 0.25f);
            else if (t < 0.75f) // day: fully dayColor
                targetColor = dayColor;
            else // evening: fade from day to night
                targetColor = Color.Lerp(dayColor, nightColor, (t - 0.75f) / 0.25f);

            dayNightOverlay.color = targetColor;
        }

        // Event overloads
        private void UpdateTimeDisplay(int day, GameYear year, string season) => UpdateTimeDisplay();
        private void UpdateTimeDisplay(string season, GameYear year) => UpdateTimeDisplay();
        private void UpdateTimeDisplay(GameYear year) => UpdateTimeDisplay();
    }
}
