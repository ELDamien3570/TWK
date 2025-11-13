using UnityEngine;
using TMPro;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// UI component for displaying a single building's information.
    /// </summary>
    public class BuildingUIItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI workerText;
        [SerializeField] private TextMeshProUGUI productionText;
        [SerializeField] private TextMeshProUGUI hubStatusText;

        private BuildingViewModel viewModel;

        public void SetData(BuildingViewModel vm)
        {
            viewModel = vm;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (viewModel == null) return;

            // Building Name
            if (buildingNameText != null)
            {
                buildingNameText.text = $"<b>{viewModel.BuildingName}</b> ({viewModel.CategoryName})";
            }

            // Status
            if (statusText != null)
            {
                if (viewModel.IsUnderConstruction)
                {
                    statusText.text = $"<color=yellow>{viewModel.GetConstructionStatus()}</color>";
                }
                else if (!viewModel.IsActive)
                {
                    statusText.text = "<color=red>Inactive</color>";
                }
                else
                {
                    statusText.text = $"<color=green>{viewModel.GetStatusSummary()}</color>";
                }
            }

            // Workers
            if (workerText != null)
            {
                if (viewModel.RequiresWorkers)
                {
                    workerText.text = $"Workers: {viewModel.TotalWorkers}/{viewModel.OptimalWorkers} " +
                                     $"({viewModel.WorkerRatioFormatted})\n" +
                                     $"{viewModel.GetWorkerBreakdown()}";
                }
                else
                {
                    workerText.text = "No workers required";
                }
            }

            // Production
            if (productionText != null)
            {
                if (viewModel.IsCompleted && viewModel.IsActive)
                {
                    productionText.text = $"Production: {viewModel.ProductionSummary}\n" +
                                         $"Efficiency: {viewModel.GetProductionEfficiency()}";

                    if (viewModel.HasPopulationEffects)
                    {
                        productionText.text += $"\n<i>{viewModel.GetPopulationEffectsSummary()}</i>";
                    }
                }
                else
                {
                    productionText.text = "Not producing";
                }
            }

            // Hub Status
            if (hubStatusText != null)
            {
                hubStatusText.text = viewModel.GetHubStatus();
            }
        }
    }
}
