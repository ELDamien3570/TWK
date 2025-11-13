using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// Simple debug UI for displaying city information using ViewModels.
    /// Attach this to a UI panel with TextMeshPro components.
    /// </summary>
    public class CityInfoDebugUI : MonoBehaviour
    {
        [Header("Target City")]
        [SerializeField] private int cityId = 0;
        [SerializeField] private bool autoRefresh = true;
        [SerializeField] private float refreshInterval = 1f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cityNameText;
        [SerializeField] private TextMeshProUGUI populationSummaryText;
        [SerializeField] private TextMeshProUGUI populationBreakdownText;
        [SerializeField] private TextMeshProUGUI demographicsText;
        [SerializeField] private TextMeshProUGUI laborText;
        [SerializeField] private TextMeshProUGUI economyText;
        [SerializeField] private TextMeshProUGUI socialMobilityText;
        [SerializeField] private TextMeshProUGUI populationGroupsText;

        private CityViewModel currentCityViewModel;
        private float lastRefreshTime;

        private void Start()
        {
            // Subscribe to ViewModel updates
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnCityViewModelUpdated += OnCityViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated += OnViewModelsUpdated;
            }

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnCityViewModelUpdated -= OnCityViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated -= OnViewModelsUpdated;
            }
        }

        private void Update()
        {
            if (autoRefresh && Time.time - lastRefreshTime > refreshInterval)
            {
                RefreshUI();
                lastRefreshTime = Time.time;
            }
        }

        private void OnCityViewModelUpdated(int updatedCityId)
        {
            if (updatedCityId == cityId)
            {
                RefreshUI();
            }
        }

        private void OnViewModelsUpdated()
        {
            RefreshUI();
        }

        [ContextMenu("Refresh UI")]
        public void RefreshUI()
        {
            if (ViewModelService.Instance == null)
            {
                Debug.LogWarning("[CityInfoDebugUI] ViewModelService not found");
                return;
            }

            currentCityViewModel = ViewModelService.Instance.GetCityViewModel(cityId);

            if (currentCityViewModel == null)
            {
                DisplayNoDataMessage();
                return;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            var vm = currentCityViewModel;

            // City Name
            if (cityNameText != null)
                cityNameText.text = $"<b>{vm.CityName}</b> (ID: {vm.CityID})";

            // Population Summary
            if (populationSummaryText != null)
            {
                populationSummaryText.text = $"<b>POPULATION: {vm.TotalPopulation:N0}</b>\n" +
                    $"Growth Rate: {vm.GrowthRate:F2}\n" +
                    $"Average Age: {vm.AverageAge:F1} years\n" +
                    $"Gender: {vm.MalePercentage:F1}% M / {vm.FemalePercentage:F1}% F";
            }

            // Population Breakdown by Archetype
            if (populationBreakdownText != null)
            {
                populationBreakdownText.text = $"<b>POPULATION BY CLASS</b>\n" +
                    $"Slaves:    {vm.SlaveCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Slave):F1}%)\n" +
                    $"Laborers:  {vm.LaborerCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Laborer):F1}%)\n" +
                    $"Artisans:  {vm.ArtisanCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Artisan):F1}%)\n" +
                    $"Merchants: {vm.MerchantCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Merchant):F1}%)\n" +
                    $"Nobles:    {vm.NobleCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Noble):F1}%)\n" +
                    $"Clergy:    {vm.ClergyCount,6:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Clergy):F1}%)";
            }

            // Demographics
            if (demographicsText != null)
            {
                float youthPct = vm.TotalPopulation > 0 ? (vm.TotalYouth / (float)vm.TotalPopulation) * 100f : 0f;
                float adultPct = vm.TotalPopulation > 0 ? (vm.TotalAdults / (float)vm.TotalPopulation) * 100f : 0f;
                float middlePct = vm.TotalPopulation > 0 ? (vm.TotalMiddleAge / (float)vm.TotalPopulation) * 100f : 0f;
                float elderlyPct = vm.TotalPopulation > 0 ? (vm.TotalElderly / (float)vm.TotalPopulation) * 100f : 0f;

                demographicsText.text = $"<b>DEMOGRAPHICS</b>\n" +
                    $"Youth (0-17):   {vm.TotalYouth,6:N0}  ({youthPct:F1}%)\n" +
                    $"Adults (18-45): {vm.TotalAdults,6:N0}  ({adultPct:F1}%)\n" +
                    $"Middle (46-60): {vm.TotalMiddleAge,6:N0}  ({middlePct:F1}%)\n" +
                    $"Elderly (60+):  {vm.TotalElderly,6:N0}  ({elderlyPct:F1}%)\n\n" +
                    $"<b>AGGREGATE STATS</b>\n" +
                    $"Avg Wealth:     {vm.AverageWealth:F1}\n" +
                    $"Avg Education:  {vm.AverageEducation:F1}\n" +
                    $"Avg Fervor:     {vm.AverageFervor:F1}\n" +
                    $"Avg Loyalty:    {vm.AverageLoyalty:F1}\n" +
                    $"Avg Happiness:  {vm.AverageHappiness * 100f:F1}%";
            }

            // Labor & Military
            if (laborText != null)
            {
                laborText.text = $"<b>LABOR FORCE</b>\n" +
                    $"Available Workers: {vm.TotalAvailableWorkers:N0}\n" +
                    $"Employed:          {vm.TotalEmployed:N0}\n" +
                    $"Unemployed:        {vm.TotalUnemployed:N0}\n" +
                    $"Unemployment Rate: {vm.UnemploymentRateFormatted}\n\n" +
                    $"<b>MILITARY</b>\n" +
                    $"Eligible:          {vm.TotalMilitaryEligible:N0} ({vm.MilitaryEligiblePercentage:F1}%)";
            }

            // Economy
            if (economyText != null)
            {
                string foodStatus = vm.DailyFoodNet >= 0 ? "SURPLUS" : "DEFICIT";
                economyText.text = $"<b>ECONOMY</b>\n" +
                    $"Food:        {vm.FoodStockpile,8:N0}  ({vm.DailyFoodNet:+#;-#;0}/day) {foodStatus}\n" +
                    $"Gold:        {vm.GoldStockpile,8:N0}\n" +
                    $"Buildings:   {vm.BuildingCount}";
            }

            // Social Mobility
            if (socialMobilityText != null)
            {
                socialMobilityText.text = $"<b>SOCIAL MOBILITY</b>\n{vm.GetSocialMobilitySummary()}";
            }

            // Population Groups Detail
            if (populationGroupsText != null)
            {
                string groupsText = $"<b>POPULATION GROUPS ({vm.PopulationGroups.Count})</b>\n";

                foreach (var popVM in vm.PopulationGroups)
                {
                    groupsText += $"\n<b>{popVM.ArchetypeName}</b> (ID: {popVM.ID})\n";
                    groupsText += $"  Pop: {popVM.TotalPopulation:N0} | Age: {popVM.AverageAge:F0} | " +
                                 $"Workers: {popVM.AvailableWorkers}/{popVM.EmployedCount}\n";
                    groupsText += $"  Edu: {popVM.EducationFormatted} | Wealth: {popVM.WealthFormatted} | " +
                                 $"Happy: {popVM.HappinessFormatted}\n";
                    groupsText += $"  {popVM.GetPromotionStatus()}\n";
                }

                populationGroupsText.text = groupsText;
            }
        }

        private void DisplayNoDataMessage()
        {
            string message = $"No data available for City ID: {cityId}";

            if (cityNameText != null) cityNameText.text = message;
            if (populationSummaryText != null) populationSummaryText.text = "";
            if (populationBreakdownText != null) populationBreakdownText.text = "";
            if (demographicsText != null) demographicsText.text = "";
            if (laborText != null) laborText.text = "";
            if (economyText != null) economyText.text = "";
            if (socialMobilityText != null) socialMobilityText.text = "";
            if (populationGroupsText != null) populationGroupsText.text = "";
        }

        // ========== PUBLIC METHODS ==========
        public void SetCityId(int newCityId)
        {
            cityId = newCityId;
            RefreshUI();
        }
    }
}
