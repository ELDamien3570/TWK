using System.Collections.Generic;
using System.Linq;
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

        [Header("Building Tab - Build Menu")]
        [SerializeField] private TMP_Dropdown buildingSelectionDropdown;
        [SerializeField] private TextMeshProUGUI selectedBuildingInfoText;
        [SerializeField] private Button constructBuildingButton;
        [SerializeField] private TextMeshProUGUI constructButtonText;

        [Header("Culture Tab")]
        [SerializeField] private TextMeshProUGUI cultureBreakdownText;
        [SerializeField] private TextMeshProUGUI mainCultureInfoText;

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f; // Refresh UI every second

        private CityViewModel viewModel;
        private List<GameObject> currentPopGroupItems = new List<GameObject>();
        private List<GameObject> currentBuildingItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;

        // Build menu state
        private List<BuildingDefinition> availableBuildings = new List<BuildingDefinition>();
        private BuildingDefinition selectedBuildingDefinition = null;

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

            // Subscribe to culture events
            SubscribeToCultureEvents();

            // Show default tab
            ShowTab(populationTab);

            // Refresh UI
            RefreshUI();
        }

        private void SubscribeToCultureEvents()
        {
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCultureBuildingsChanged += HandleCultureBuildingsChanged;
            }
        }

        /// <summary>
        /// Handle culture building unlocks - refresh build menu if it affects this city.
        /// </summary>
        private void HandleCultureBuildingsChanged(int cultureID)
        {
            if (targetCity == null) return;

            // Check if this building change affects our city's culture
            int cityCultureID = CultureManager.Instance.GetCityCulture(targetCity.CityID);

            if (cityCultureID == cultureID)
            {
                // This city's culture unlocked new buildings - refresh the dropdown
                RefreshBuildMenu();
                Debug.Log($"[CityUIController] Culture {cultureID} unlocked new buildings - refreshing build menu for {targetCity.Name}");
            }
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
            constructBuildingButton?.onClick.RemoveAllListeners();

            // Add listeners
            populationTabButton?.onClick.AddListener(() => ShowTab(populationTab));
            popGroupsTabButton?.onClick.AddListener(() => ShowTab(popGroupsTab));
            militaryTabButton?.onClick.AddListener(() => ShowTab(militaryTab));
            economicTabButton?.onClick.AddListener(() => ShowTab(economicTab));
            buildingTabButton?.onClick.AddListener(() => ShowTab(buildingTab));
            cultureTabButton?.onClick.AddListener(() => ShowTab(cultureTab));
            religionTabButton?.onClick.AddListener(() => ShowTab(religionTab));

            // Setup build menu
            if (buildingSelectionDropdown != null)
                buildingSelectionDropdown.onValueChanged.AddListener(OnBuildingSelectionChanged);
            constructBuildingButton?.onClick.AddListener(OnConstructBuildingClicked);
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
            // Unsubscribe from culture events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCultureBuildingsChanged -= HandleCultureBuildingsChanged;
            }

            // Clean up listeners
            populationTabButton?.onClick.RemoveAllListeners();
            popGroupsTabButton?.onClick.RemoveAllListeners();
            militaryTabButton?.onClick.RemoveAllListeners();
            economicTabButton?.onClick.RemoveAllListeners();
            buildingTabButton?.onClick.RemoveAllListeners();
            cultureTabButton?.onClick.RemoveAllListeners();
            religionTabButton?.onClick.RemoveAllListeners();
            constructBuildingButton?.onClick.RemoveAllListeners();

            if (buildingSelectionDropdown != null)
                buildingSelectionDropdown.onValueChanged.RemoveAllListeners();
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

            // Always refresh build menu when building tab is shown
            // This ensures newly unlocked buildings appear immediately
            RefreshBuildMenu();
        }

        private void RefreshBuildMenu()
        {
            if (targetCity == null) return;

            // Get available buildings from city's culture
            availableBuildings = new List<BuildingDefinition>(targetCity.GetAvailableBuildings());

            // Populate dropdown
            if (buildingSelectionDropdown != null)
            {
                // Save current selection index to try to preserve it
                int currentSelection = buildingSelectionDropdown.value;

                buildingSelectionDropdown.ClearOptions();

                var options = new List<string>();

                foreach (var building in availableBuildings)
                {
                    if (building != null)
                    {
                        // Color building name by TreeType for easy visual categorization
                        string coloredName = GetColoredBuildingName(building);
                        options.Add(coloredName);
                    }
                }

                if (options.Count == 0)
                {
                    options.Add("No buildings available");
                }

                buildingSelectionDropdown.AddOptions(options);

                // Try to preserve selection if still valid
                if (currentSelection < options.Count)
                    buildingSelectionDropdown.value = currentSelection;
                else
                    buildingSelectionDropdown.value = 0;
            }

            // Update selection based on dropdown value
            OnBuildingSelectionChanged(buildingSelectionDropdown != null ? buildingSelectionDropdown.value : 0);
        }

        /// <summary>
        /// Get building name with color based on TreeType (Economics, Warfare, etc.).
        /// Uses TextMeshPro rich text color tags for simple visual categorization.
        /// </summary>
        private string GetColoredBuildingName(BuildingDefinition building)
        {
            string colorHex = GetTreeTypeColorHex(building.BuildingCategory);
            return $"<color={colorHex}>{building.BuildingName}</color>";
        }

        /// <summary>
        /// Get color hex code for each TreeType.
        /// Simple color scheme for temporary UI.
        /// </summary>
        private string GetTreeTypeColorHex(TreeType treeType)
        {
            switch (treeType)
            {
                case TreeType.Economics:
                    return "#FFD700";  // Gold
                case TreeType.Warfare:
                    return "#DC143C";  // Crimson
                case TreeType.Religion:
                    return "#9370DB";  // Medium Purple
                case TreeType.Politics:
                    return "#4169E1";  // Royal Blue
                case TreeType.Science:
                    return "#20B2AA";  // Light Sea Green
                default:
                    return "#FFFFFF";  // White
            }
        }

        private void OnBuildingSelectionChanged(int index)
        {
            // Direct mapping - no placeholder offset
            if (index >= 0 && index < availableBuildings.Count)
            {
                selectedBuildingDefinition = availableBuildings[index];
            }
            else
            {
                selectedBuildingDefinition = null;
            }

            UpdateBuildingInfoDisplay();
        }

        private void UpdateBuildingInfoDisplay()
        {
            if (selectedBuildingInfoText == null) return;

            if (selectedBuildingDefinition == null)
            {
                selectedBuildingInfoText.text = "<i>Select a building from the dropdown to see details</i>";

                if (constructBuildingButton != null)
                    constructBuildingButton.interactable = false;
                if (constructButtonText != null)
                    constructButtonText.text = "Select Building";

                return;
            }

            var def = selectedBuildingDefinition;

            // Build the info display
            string info = $"<b><size=16>{def.BuildingName}</size></b>\n";
            info += $"<i>{def.BuildingCategory}</i>\n\n";

            // Construction info
            info += $"<b>Construction Time:</b> {def.ConstructionTimeDays} days\n";
            info += $"<b>Base Efficiency:</b> {def.BaseEfficiency * 100f:F0}%\n\n";

            // Build costs
            info += "<b>Build Cost:</b>\n";
            if (def.BaseBuildCost.Count > 0)
            {
                foreach (var cost in def.BaseBuildCost)
                {
                    int currentAmount = ResourceManager.Instance.GetResource(targetCity.CityID, cost.ResourceType);
                    string colorTag = currentAmount >= cost.Amount ? "<color=green>" : "<color=red>";
                    info += $"  {colorTag}{cost.ResourceType}: {cost.Amount} (have: {currentAmount})</color>\n";
                }
            }
            else
            {
                info += "  <i>Free</i>\n";
            }
            info += "\n";

            // Worker requirements
            if (def.RequiresWorkers)
            {
                info += $"<b>Worker Requirements:</b>\n";
                int minWorkers = def.WorkerSlots.GetTotalMinWorkers();
                int optimalWorkers = def.WorkerSlots.GetTotalMaxWorkers();
                info += $"  Min Workers: {minWorkers}\n";
                info += $"  Optimal Workers: {(optimalWorkers == int.MaxValue ? "Unlimited" : optimalWorkers.ToString())}\n";

                if (def.WorkerSlots != null && def.WorkerSlots.Count > 0)
                {
                    info += "  Worker Slots:\n";
                    foreach (var slot in def.WorkerSlots)
                    {
                        info += $"    {slot.Archetype}: {slot.MinCount}-{(slot.HasMaxLimit ? slot.MaxCount.ToString() : "∞")} (x{slot.EfficiencyMultiplier:F1})\n";
                    }
                }
                info += "\n";
            }
            else
            {
                info += "<b>Workers:</b> Not required\n\n";
            }

            // Production
            if (def.BaseProduction.Count > 0 || def.MaxProduction.Count > 0)
            {
                info += "<b>Production:</b>\n";
                foreach (var prod in def.MaxProduction)
                {
                    int baseAmount = def.BaseProduction
                    .Where(p => p.ResourceType == prod.ResourceType)
                    .Select(p => (int?)p.Amount)
                    .FirstOrDefault() ?? 0;
                    info += $"  {prod.ResourceType}: {baseAmount} - {prod.Amount} per day\n";
                }
                info += "\n";
            }

            // Maintenance
            if (def.BaseMaintenanceCost.Count > 0)
            {
                info += "<b>Maintenance Cost:</b>\n";
                foreach (var cost in def.BaseMaintenanceCost)
                {
                    info += $"  {cost.ResourceType}: {cost.Amount} per day\n";
                }
                info += "\n";
            }

            // Hub/Hublet info
            if (def.IsHub)
            {
                info += $"<b>Hub Building:</b> Can support {def.HubletSlots} hublets\n\n";
            }
            if (def.IsHublet)
            {
                info += "<b>Hublet:</b> Requires attachment to hub (";
                info += string.Join(", ", def.RequiredHubTypes);
                info += ")\n\n";
            }

            // Population effects (now per worker slot)
            bool hasPopEffects = def.PopulationGrowthBonus > 0;
            if (!hasPopEffects && def.WorkerSlots != null)
            {
                foreach (var slot in def.WorkerSlots)
                {
                    if (slot.EducationGrowthPerWorker > 0 || slot.WealthGrowthPerWorker > 0)
                    {
                        hasPopEffects = true;
                        break;
                    }
                }
            }

            if (hasPopEffects)
            {
                info += "<b>Population Effects:</b>\n";

                // Show per-archetype effects from worker slots
                if (def.WorkerSlots != null)
                {
                    foreach (var slot in def.WorkerSlots)
                    {
                        if (slot.EducationGrowthPerWorker > 0)
                            info += $"  {slot.Archetype} Education: +{slot.EducationGrowthPerWorker:F2} per worker/day\n";
                        if (slot.WealthGrowthPerWorker > 0)
                            info += $"  {slot.Archetype} Wealth: +{slot.WealthGrowthPerWorker:F2} per worker/day\n";
                    }
                }

                // Show building-wide growth bonus
                if (def.PopulationGrowthBonus > 0)
                    info += $"  Growth Bonus: +{def.PopulationGrowthBonus:F1}%\n";
            }

            selectedBuildingInfoText.text = info;

            // Update construct button
            bool canAfford = CanAffordBuilding(def);
            if (constructBuildingButton != null)
                constructBuildingButton.interactable = canAfford;

            if (constructButtonText != null)
            {
                if (canAfford)
                    constructButtonText.text = $"Construct {def.BuildingName}";
                else
                    constructButtonText.text = "Insufficient Resources";
            }
        }

        private bool CanAffordBuilding(BuildingDefinition def)
        {
            if (def == null) return false;

            foreach (var cost in def.BaseBuildCost)
            {
                int currentAmount = ResourceManager.Instance.GetResource(targetCity.CityID, cost.ResourceType);
                if (currentAmount < cost.Amount)
                    return false;
            }

            return true;
        }

        private void OnConstructBuildingClicked()
        {
            if (targetCity == null || selectedBuildingDefinition == null)
            {
                Debug.LogWarning("[CityUIController] Cannot construct - no target city or building selected");
                return;
            }

            // Build at random position near city center
            Vector3 buildPosition = targetCity.Data.Location + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0f,
                UnityEngine.Random.Range(-5f, 5f)
            );

            // Construct the building
            targetCity.BuildBuilding(selectedBuildingDefinition, buildPosition);

            Debug.Log($"[CityUIController] {selectedBuildingDefinition.BuildingName} construction requested at {buildPosition}");

            // Don't reset dropdown - let user build multiple of same building if desired
            // Just refresh to update building list
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
                    mainCultureInfoText.text = "<b>Main Culture:</b>\n<i>No dominant culture (city has no population)</i>";
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
