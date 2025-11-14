using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Realms;
using TWK.UI.ViewModels;
using TWK.Economy;
using TWK.Realms.Demographics;
using TWK.Cultures;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for city UI.
    /// Displays city information using CityViewModel and provides tab navigation.
    /// </summary>
    public class CityUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private City targetCity;

        [Header("Main Info")]
        [SerializeField] private TextMeshProUGUI cityNameText;
        [SerializeField] private TextMeshProUGUI economyOverviewText;
        [SerializeField] private TextMeshProUGUI populationOverviewText;
        [SerializeField] private TextMeshProUGUI militaryEligibilityText;

        [Header("Tabs")]
        [SerializeField] private GameObject populationTab;
        [SerializeField] private GameObject popGroupsTab;
        [SerializeField] private GameObject militaryTab;
        [SerializeField] private GameObject economicTab;
        [SerializeField] private GameObject buildingTab;
        [SerializeField] private GameObject cultureTab;
        [SerializeField] private GameObject religionTab;

        [Header("Tab Buttons")]
        [SerializeField] private Button populationTabButton;
        [SerializeField] private Button popGroupsTabButton;
        [SerializeField] private Button militaryTabButton;
        [SerializeField] private Button economicTabButton;
        [SerializeField] private Button buildingTabButton;
        [SerializeField] private Button cultureTabButton;
        [SerializeField] private Button religionTabButton;

        [Header("Population Overview Tab")]
        [SerializeField] private TextMeshProUGUI popOverviewDetailsText;
        [SerializeField] private TextMeshProUGUI popDemographicsText;
        [SerializeField] private TextMeshProUGUI popGenderText;
        [SerializeField] private TextMeshProUGUI popSocialMobilityText;
        [SerializeField] private TextMeshProUGUI popArchetypeBreakdownText;
        [SerializeField] private TextMeshProUGUI popLaborText;

        [Header("Pop Groups Tab")]
        [SerializeField] private Transform popGroupListContainer;
        [SerializeField] private GameObject popGroupItemPrefab;

        [Header("Military Tab")]
        [SerializeField] private TextMeshProUGUI militaryTotalText;
        [SerializeField] private TextMeshProUGUI militaryByArchetypeText;

        [Header("Economic Tab")]
        [SerializeField] private TextMeshProUGUI economicResourcesText;
        [SerializeField] private TextMeshProUGUI economicProductionText;
        [SerializeField] private TextMeshProUGUI economicConsumptionText;

        [Header("Building Tab")]
        [SerializeField] private Transform buildingListContainer;
        [SerializeField] private GameObject buildingItemPrefab;
        [SerializeField] private Button buildFarmButton;
        [SerializeField] private TextMeshProUGUI buildFarmCostText;

        [Header("Culture Tab")]
        [SerializeField] private TextMeshProUGUI cultureBreakdownText;
        [SerializeField] private TextMeshProUGUI mainCultureInfoText;

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f; // Refresh UI every second

        private CityViewModel viewModel;
        private List<GameObject> currentPopGroupItems = new List<GameObject>();
        private List<GameObject> currentBuildingItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;

        private void Start()
        {
            if (targetCity == null)
            {
                Debug.LogError("[CityUIController] No target city assigned!");
                return;
            }

            // Initialize ViewModel
            viewModel = new CityViewModel(targetCity);

            // Setup tab buttons
            SetupTabButtons();

            // Show default tab
            ShowTab(populationTab);

            // Refresh UI
            RefreshUI();
        }

        private void SetupTabButtons()
        {
            // Remove all existing listeners to prevent double-registration
            populationTabButton?.onClick.RemoveAllListeners();
            popGroupsTabButton?.onClick.RemoveAllListeners();
            militaryTabButton?.onClick.RemoveAllListeners();
            economicTabButton?.onClick.RemoveAllListeners();
            buildingTabButton?.onClick.RemoveAllListeners();
            cultureTabButton?.onClick.RemoveAllListeners();
            religionTabButton?.onClick.RemoveAllListeners();
            buildFarmButton?.onClick.RemoveAllListeners();

            // Add listeners
            populationTabButton?.onClick.AddListener(() => ShowTab(populationTab));
            popGroupsTabButton?.onClick.AddListener(() => ShowTab(popGroupsTab));
            militaryTabButton?.onClick.AddListener(() => ShowTab(militaryTab));
            economicTabButton?.onClick.AddListener(() => ShowTab(economicTab));
            buildingTabButton?.onClick.AddListener(() => ShowTab(buildingTab));
            cultureTabButton?.onClick.AddListener(() => ShowTab(cultureTab));
            religionTabButton?.onClick.AddListener(() => ShowTab(religionTab));

            // Setup build button
            buildFarmButton?.onClick.AddListener(OnBuildFarmClicked);
        }

        private void Update()
        {
            // Periodic refresh
            timeSinceLastRefresh += Time.deltaTime;
            if (timeSinceLastRefresh >= refreshInterval)
            {
                timeSinceLastRefresh = 0f;
                RefreshUI();
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            populationTabButton?.onClick.RemoveAllListeners();
            popGroupsTabButton?.onClick.RemoveAllListeners();
            militaryTabButton?.onClick.RemoveAllListeners();
            economicTabButton?.onClick.RemoveAllListeners();
            buildingTabButton?.onClick.RemoveAllListeners();
            cultureTabButton?.onClick.RemoveAllListeners();
            religionTabButton?.onClick.RemoveAllListeners();
            buildFarmButton?.onClick.RemoveAllListeners();
        }

        private void ShowTab(GameObject tab)
        {
            // Hide all tabs
            populationTab?.SetActive(false);
            popGroupsTab?.SetActive(false);
            militaryTab?.SetActive(false);
            economicTab?.SetActive(false);
            buildingTab?.SetActive(false);
            cultureTab?.SetActive(false);
            religionTab?.SetActive(false);

            // Show selected tab
            tab?.SetActive(true);

            // Refresh tab content
            RefreshCurrentTab(tab);
        }

        public void RefreshUI()
        {
            if (viewModel == null) return;

            viewModel.Refresh();

            RefreshMainInfo();
            RefreshCurrentTab(GetActiveTab());
        }

        private void RefreshMainInfo()
        {
            // City Name
            if (cityNameText != null)
                cityNameText.text = viewModel.CityName;

            // Economy Overview
            if (economyOverviewText != null)
                economyOverviewText.text = viewModel.GetEconomySummary();

            // Population Overview
            if (populationOverviewText != null)
            {
                populationOverviewText.text = $"Population: {viewModel.TotalPopulation}\n" +
                                             $"Workers: {viewModel.TotalAvailableWorkers} " +
                                             $"(Employed: {viewModel.TotalEmployed}, " +
                                             $"Unemployed: {viewModel.TotalUnemployed})";
            }

            // Military Eligibility
            if (militaryEligibilityText != null)
            {
                militaryEligibilityText.text = $"Military Eligible: {viewModel.TotalMilitaryEligible} " +
                                              $"({viewModel.MilitaryEligiblePercentage:F1}%)";
            }
        }

        private void RefreshCurrentTab(GameObject activeTab)
        {
            if (activeTab == populationTab)
                RefreshPopulationTab();
            else if (activeTab == popGroupsTab)
                RefreshPopGroupsTab();
            else if (activeTab == militaryTab)
                RefreshMilitaryTab();
            else if (activeTab == economicTab)
                RefreshEconomicTab();
            else if (activeTab == buildingTab)
                RefreshBuildingTab();
            else if (activeTab == cultureTab)
                RefreshCultureTab();
            // Religion tab is blank for now
        }

        private GameObject GetActiveTab()
        {
            if (populationTab != null && populationTab.activeSelf) return populationTab;
            if (popGroupsTab != null && popGroupsTab.activeSelf) return popGroupsTab;
            if (militaryTab != null && militaryTab.activeSelf) return militaryTab;
            if (economicTab != null && economicTab.activeSelf) return economicTab;
            if (buildingTab != null && buildingTab.activeSelf) return buildingTab;
            if (cultureTab != null && cultureTab.activeSelf) return cultureTab;
            if (religionTab != null && religionTab.activeSelf) return religionTab;
            return null;
        }

        // ========== POPULATION OVERVIEW TAB ==========

        private void RefreshPopulationTab()
        {
            if (popOverviewDetailsText != null)
            {
                popOverviewDetailsText.text = $"<b>Total Population:</b> {viewModel.TotalPopulation}\n" +
                                             $"<b>Average Age:</b> {viewModel.AverageAge:F1} years\n" +
                                             $"<b>Average Wealth:</b> {viewModel.AverageWealth:F1}\n" +
                                             $"<b>Average Education:</b> {viewModel.AverageEducation:F1}\n" +
                                             $"<b>Average Fervor:</b> {viewModel.AverageFervor:F1}\n" +
                                             $"<b>Average Loyalty:</b> {viewModel.AverageLoyalty:F1}";
            }

            if (popDemographicsText != null)
            {
                popDemographicsText.text = $"<b>Demographics:</b>\n{viewModel.GetDemographicSummary()}";
            }

            if (popGenderText != null)
            {
                popGenderText.text = $"<b>Gender:</b>\n{viewModel.GetGenderSummary()}";
            }

            if (popSocialMobilityText != null)
            {
                popSocialMobilityText.text = $"<b>Social Mobility:</b>\n{viewModel.GetSocialMobilitySummary()}";
            }

            if (popArchetypeBreakdownText != null)
            {
                popArchetypeBreakdownText.text = $"<b>By Class:</b>\n{viewModel.GetPopulationBreakdownSummary()}";
            }

            if (popLaborText != null)
            {
                popLaborText.text = $"<b>Labor:</b>\n{viewModel.GetLaborSummary()}";
            }
        }

        // ========== POP GROUPS TAB ==========

        private void RefreshPopGroupsTab()
        {
            // Clear existing items
            foreach (var item in currentPopGroupItems)
                Destroy(item);
            currentPopGroupItems.Clear();

            if (popGroupListContainer == null || popGroupItemPrefab == null)
                return;

            // Create item for each population group
            foreach (var popGroupVM in viewModel.PopulationGroups)
            {
                GameObject item = Instantiate(popGroupItemPrefab, popGroupListContainer);
                PopulationGroupUIItem uiItem = item.GetComponent<PopulationGroupUIItem>();

                if (uiItem != null)
                {
                    uiItem.SetData(popGroupVM);
                }

                currentPopGroupItems.Add(item);
            }
        }

        // ========== MILITARY TAB ==========

        private void RefreshMilitaryTab()
        {
            if (militaryTotalText != null)
            {
                militaryTotalText.text = $"<b>Total Military Eligible:</b> {viewModel.TotalMilitaryEligible}\n" +
                                        $"<b>Percentage of Population:</b> {viewModel.MilitaryEligiblePercentage:F1}%\n" +
                                        $"{viewModel.GetMilitarySummary()}";
            }

            if (militaryByArchetypeText != null)
            {
                string breakdown = "<b>Eligibility by Class:</b>\n";

                foreach (var popGroup in viewModel.PopulationGroups)
                {
                    if (popGroup.MilitaryEligible > 0)
                    {
                        breakdown += $"{popGroup.ArchetypeName}: {popGroup.MilitaryEligible} " +
                                   $"({popGroup.MilitaryEligiblePercentage:F1}% of their population)\n";
                    }
                }

                militaryByArchetypeText.text = breakdown;
            }
        }

        // ========== ECONOMIC TAB ==========

        private void RefreshEconomicTab()
        {
            if (economicResourcesText != null)
            {
                string resources = "<b>Current Resources:</b>\n";
                foreach (var kvp in viewModel.CurrentResources)
                {
                    resources += $"{kvp.Key}: {kvp.Value}\n";
                }
                economicResourcesText.text = resources;
            }

            if (economicProductionText != null)
            {
                string production = "<b>Daily Production:</b>\n";
                var snapshot = targetCity.Data.EconomySnapshot;
                foreach (var kvp in snapshot.Production)
                {
                    production += $"{kvp.Key}: +{kvp.Value}\n";
                }
                economicProductionText.text = production;
            }

            if (economicConsumptionText != null)
            {
                string consumption = "<b>Daily Consumption:</b>\n";
                var snapshot = targetCity.Data.EconomySnapshot;
                foreach (var kvp in snapshot.Consumption)
                {
                    consumption += $"{kvp.Key}: -{kvp.Value}\n";
                }
                economicConsumptionText.text = consumption;
            }
        }

        // ========== BUILDING TAB ==========

        private void RefreshBuildingTab()
        {
            // Clear existing items
            foreach (var item in currentBuildingItems)
                Destroy(item);
            currentBuildingItems.Clear();

            if (buildingListContainer == null || buildingItemPrefab == null)
                return;

            // Create item for each building
            foreach (int buildingId in viewModel.BuildingIDs)
            {
                GameObject item = Instantiate(buildingItemPrefab, buildingListContainer);
                BuildingUIItem uiItem = item.GetComponent<BuildingUIItem>();

                if (uiItem != null)
                {
                    var buildingVM = new BuildingViewModel(buildingId);
                    uiItem.SetData(buildingVM);
                }

                currentBuildingItems.Add(item);
            }

            // Update build farm button
            RefreshBuildFarmButton();
        }

        private void RefreshBuildFarmButton()
        {
            if (buildFarmCostText != null)
            {
                // TODO: Get actual farm costs from BuildingDefinition when implemented
                buildFarmCostText.text = "Cost: 50 Gold";
            }
        }

        private void OnBuildFarmClicked()
        {
            if (targetCity == null)
            {
                Debug.LogWarning("[CityUIController] Cannot build - no target city");
                return;
            }

            // Load farm BuildingDefinition from Resources
            var farmDefinition = Resources.Load<BuildingDefinition>("Buildings/FarmDefinition");

            if (farmDefinition == null)
            {
                Debug.LogError("[CityUIController] Could not find FarmDefinition in Resources/Buildings/. Please create a BuildingDefinition asset for Farm.");
                return;
            }

            // Build at city center (placeholder position)
            Vector3 buildPosition = targetCity.Data.Location + new Vector3(
                Random.Range(-5f, 5f),
                0f,
                Random.Range(-5f, 5f)
            );

            // Use new building system
            targetCity.BuildBuilding(farmDefinition, buildPosition);

            Debug.Log($"[CityUIController] Farm construction requested at {buildPosition}");

            // Refresh UI
            RefreshUI();
        }

        // ========== CULTURE TAB ==========

        private void RefreshCultureTab()
        {
            if (targetCity == null) return;

            // Get culture breakdown from city
            var cultureBreakdown = targetCity.GetCultureBreakdown();
            var mainCulture = targetCity.GetMainCulture();

            // Display culture breakdown
            if (cultureBreakdownText != null)
            {
                if (cultureBreakdown.Count == 0)
                {
                    cultureBreakdownText.text = "<b>Population by Culture:</b>\n<i>No population data available</i>";
                }
                else
                {
                    string breakdown = "<b>Population by Culture:</b>\n\n";

                    // Sort by population count (descending)
                    var sortedCultures = cultureBreakdown.OrderByDescending(kvp => kvp.Value.count);

                    foreach (var kvp in sortedCultures)
                    {
                        var culture = kvp.Key;
                        var (count, percentage) = kvp.Value;

                        // Highlight main culture
                        string prefix = (mainCulture != null && culture == mainCulture) ? "<b>★ " : "  ";
                        string suffix = (mainCulture != null && culture == mainCulture) ? " (Main Culture)</b>" : "";

                        breakdown += $"{prefix}{culture.CultureName}: {count:N0} ({percentage:F1}%){suffix}\n";
                    }

                    cultureBreakdownText.text = breakdown;
                }
            }

            // Display main culture info
            if (mainCultureInfoText != null)
            {
                if (mainCulture == null)
                {
                    mainCultureInfoText.text = "<b>Main Culture:</b>\n<i>No dominant culture (requires >50% of population)</i>";
                }
                else
                {
                    string info = $"<b>Main Culture:</b> {mainCulture.CultureName}\n\n";
                    info += "<b>Cultural Progress (XP Applied):</b>\n";

                    // Display XP for each tree type
                    foreach (TreeType treeType in System.Enum.GetValues(typeof(TreeType)))
                    {
                        var techTree = mainCulture.GetTechTree(treeType);
                        if (techTree != null)
                        {
                            int xpApplied = Mathf.FloorToInt(techTree.TotalXPEarned);
                            info += $"  {treeType}: {xpApplied} XP\n";
                        }
                    }

                    mainCultureInfoText.text = info;
                }
            }
        }

        // ========== PUBLIC API ==========

        public void SetTargetCity(City city)
        {
            targetCity = city;
            if (viewModel != null)
                viewModel = new CityViewModel(targetCity);
            RefreshUI();
        }

        private void OnEnable()
        {
            RefreshUI();
        }
    }
}
