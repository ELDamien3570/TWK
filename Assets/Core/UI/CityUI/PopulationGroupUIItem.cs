using UnityEngine;
using TMPro;
using TWK.UI.ViewModels;
using System.Linq;

namespace TWK.UI
{
    /// <summary>
    /// UI component for displaying a single population group's information.
    /// </summary>
    public class PopulationGroupUIItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI archetypeNameText;
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private TextMeshProUGUI wealthEducationText;
        [SerializeField] private TextMeshProUGUI laborText;
        [SerializeField] private TextMeshProUGUI demographicsText;
        [SerializeField] private TextMeshProUGUI promotionStatusText;

        private PopulationGroupViewModel viewModel;

        public void SetData(PopulationGroupViewModel vm)
        {
            viewModel = vm;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (viewModel == null) return;

            // Archetype Name
            if (archetypeNameText != null)
            {
                string cultureName = viewModel.Culture.ToString().Split("(").First();
                string religionText = !string.IsNullOrEmpty(viewModel.ReligionName) && viewModel.ReligionName != "None"
                    ? $" - <color=yellow>{viewModel.ReligionName}</color>"
                    : "";
                archetypeNameText.text = $"<b><color=green>{cultureName}</color> {viewModel.ArchetypeName}{religionText}</b>";
            }

            // Population
            if (populationText != null)
            {
                populationText.text = $"Population: {viewModel.TotalPopulation}\n" +
                                     $"Avg Age: {viewModel.AverageAge:F1} | " +
                                     $"Gender: {viewModel.MalePercentage:F0}% M / {viewModel.FemalePercentage:F0}% F";
            }

            // Wealth & Education
            if (wealthEducationText != null)
            {
                wealthEducationText.text = $"Wealth: {viewModel.WealthFormatted} | " +
                                          $"Education: {viewModel.EducationFormatted} | " +
                                          $"Fervor: {viewModel.FervorFormatted} | " +
                                          $"Loyalty: {viewModel.LoyaltyFormatted}";
            }

            // Labor
            if (laborText != null)
            {
                laborText.text = $"Labor: {viewModel.EmployedCount}/{viewModel.AvailableWorkers} employed " +
                                $"({viewModel.UnemploymentRateFormatted} unemployment) | " +
                                $"Military Eligible: {viewModel.MilitaryEligible}";
            }

            // Demographics
            if (demographicsText != null)
            {
                demographicsText.text = viewModel.GetDemographicSummary();
            }

            // Promotion Status
            if (promotionStatusText != null)
            {
                string promotionStatus = viewModel.GetPromotionStatus();
                string demotionRisk = viewModel.GetDemotionRisk();

                promotionStatusText.text = $"<i>{promotionStatus}</i>";

                if (!demotionRisk.Contains("Stable"))
                {
                    promotionStatusText.text += $"\n<color=red>{demotionRisk}</color>";
                }
            }
        }
    }
}
