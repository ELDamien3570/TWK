using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.UI.ViewModels;
using TWK.Realms;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for realm management UI.
    /// Displays realm information using RealmViewModel across 7 tabs.
    /// </summary>
    public class RealmUIController : MonoBehaviour
    {
        [Header("Realm Reference")]
        [SerializeField] private int targetRealmID = 1;
        [Tooltip("If set, will override targetRealmID")]
        [SerializeField] private Realm targetRealm;

        [Header("Tab System")]
        [SerializeField] private GameObject mainTab;
        [SerializeField] private GameObject landsTab;
        [SerializeField] private GameObject economicsTab;
        [SerializeField] private GameObject governmentTab;
        [SerializeField] private GameObject cultureReligionTab;
        [SerializeField] private GameObject militaryTab;
        [SerializeField] private GameObject diplomacyTab;

        [Header("Tab Buttons")]
        [SerializeField] private Button mainTabButton;
        [SerializeField] private Button landsTabButton;
        [SerializeField] private Button economicsTabButton;
        [SerializeField] private Button governmentTabButton;
        [SerializeField] private Button cultureReligionTabButton;
        [SerializeField] private Button militaryTabButton;
        [SerializeField] private Button diplomacyTabButton;

        // ========== TAB 1: MAIN INFO ==========
        [Header("Main Tab - Basic Info")]
        [SerializeField] private TextMeshProUGUI realmNameText;
        [SerializeField] private TextMeshProUGUI leaderNamesText;
        [SerializeField] private Transform leaderIconsContainer;
        [SerializeField] private GameObject leaderIconPrefab;
        [SerializeField] private TextMeshProUGUI governmentNameText;

        [Header("Main Tab - Stability & Prestige")]
        [SerializeField] private Slider stabilitySlider;
        [SerializeField] private TextMeshProUGUI stabilityText;
        [SerializeField] private Image stabilityFillImage;
        [SerializeField] private Slider prestigeSlider;
        [SerializeField] private TextMeshProUGUI prestigeText;

        [Header("Main Tab - Income/Expense")]
        [SerializeField] private TextMeshProUGUI totalIncomeText;
        [SerializeField] private TextMeshProUGUI totalExpensesText;
        [SerializeField] private TextMeshProUGUI netIncomeText;
        [SerializeField] private TextMeshProUGUI treasuryGoldText;

        [Header("Main Tab - Navigation Buttons")]
        [SerializeField] private Button bureaucracyButton;
        [SerializeField] private Button contractsButton;
        [SerializeField] private Button governmentButton;
        [SerializeField] private Button cultureButton;
        [SerializeField] private Button religionButton;

        // ========== TAB 2: REALM LANDS ==========
        [Header("Lands Tab")]
        [SerializeField] private TextMeshProUGUI holdingsSummaryText;
        [SerializeField] private Transform holdingsListContainer;
        [SerializeField] private GameObject holdingItemPrefab;
        [SerializeField] private TMP_Dropdown holdingFilterDropdown;

        // ========== TAB 3: ECONOMICS ==========
        [Header("Economics Tab")]
        [SerializeField] private TextMeshProUGUI economicBreakdownText;
        [SerializeField] private Transform topCitiesContainer;
        [SerializeField] private GameObject cityEconomicItemPrefab;
        [SerializeField] private Transform topBuildingsContainer;
        [SerializeField] private GameObject buildingEconomicItemPrefab;
        [SerializeField] private TextMeshProUGUI resourceProductionText;
        [SerializeField] private TextMeshProUGUI resourceConsumptionText;

        // ========== TAB 4: GOVERNMENT OVERVIEW ==========
        [Header("Government Tab")]
        [SerializeField] private TextMeshProUGUI stabilityBreakdownText;
        [SerializeField] private Transform officersContainer;
        [SerializeField] private GameObject officerItemPrefab;
        [SerializeField] private TextMeshProUGUI officeStatsText;
        [SerializeField] private Transform stablePopulationsContainer;
        [SerializeField] private GameObject populationItemPrefab;
        [SerializeField] private Transform unstablePopulationsContainer;
        [SerializeField] private Transform loyalVassalsContainer;
        [SerializeField] private GameObject vassalItemPrefab;
        [SerializeField] private Transform disloyalVassalsContainer;

        // ========== TAB 5: CULTURE/RELIGION ==========
        [Header("Culture/Religion Tab")]
        [SerializeField] private TMP_Dropdown cultureReligionToggle;
        [SerializeField] private TextMeshProUGUI dominantCultureReligionText;
        [SerializeField] private TextMeshProUGUI unityText;
        [SerializeField] private Transform byCityContainer;
        [SerializeField] private GameObject demographicItemPrefab;
        [SerializeField] private Transform byPopulationContainer;
        [SerializeField] private Transform byClassContainer;

        // ========== TAB 6: MILITARY ==========
        [Header("Military Tab")]
        [SerializeField] private TextMeshProUGUI militarySummaryText;
        [SerializeField] private Transform warPartiesContainer;
        [SerializeField] private GameObject warPartyItemPrefab;

        // ========== TAB 7: DIPLOMACY ==========
        [Header("Diplomacy Tab")]
        [SerializeField] private TextMeshProUGUI diplomacyPlaceholderText;

        // ========== SETTINGS ==========
        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f;

        private RealmViewModel viewModel;
        private List<GameObject> spawnedItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;

        // ========== LIFECYCLE ==========

        private void Start()
        {
            // Determine realm ID
            if (targetRealm != null)
            {
                targetRealmID = targetRealm.RealmID;
            }

            // Initialize ViewModel
            viewModel = new RealmViewModel(targetRealmID);

            // Setup tab buttons
            SetupTabButtons();

            // Setup navigation buttons
            SetupNavigationButtons();

            // Show default tab
            ShowTab(mainTab);

            // Initial refresh
            RefreshUI();
        }

        private void SetupTabButtons()
        {
            mainTabButton?.onClick.RemoveAllListeners();
            landsTabButton?.onClick.RemoveAllListeners();
            economicsTabButton?.onClick.RemoveAllListeners();
            governmentTabButton?.onClick.RemoveAllListeners();
            cultureReligionTabButton?.onClick.RemoveAllListeners();
            militaryTabButton?.onClick.RemoveAllListeners();
            diplomacyTabButton?.onClick.RemoveAllListeners();

            mainTabButton?.onClick.AddListener(() => ShowTab(mainTab));
            landsTabButton?.onClick.AddListener(() => ShowTab(landsTab));
            economicsTabButton?.onClick.AddListener(() => ShowTab(economicsTab));
            governmentTabButton?.onClick.AddListener(() => ShowTab(governmentTab));
            cultureReligionTabButton?.onClick.AddListener(() => ShowTab(cultureReligionTab));
            militaryTabButton?.onClick.AddListener(() => ShowTab(militaryTab));
            diplomacyTabButton?.onClick.AddListener(() => ShowTab(diplomacyTab));
        }

        private void SetupNavigationButtons()
        {
            bureaucracyButton?.onClick.RemoveAllListeners();
            contractsButton?.onClick.RemoveAllListeners();
            governmentButton?.onClick.RemoveAllListeners();
            cultureButton?.onClick.RemoveAllListeners();
            religionButton?.onClick.RemoveAllListeners();

            bureaucracyButton?.onClick.AddListener(OnBureaucracyClicked);
            contractsButton?.onClick.AddListener(OnContractsClicked);
            governmentButton?.onClick.AddListener(OnGovernmentClicked);
            cultureButton?.onClick.AddListener(OnCultureClicked);
            religionButton?.onClick.AddListener(OnReligionClicked);

            // Setup dropdowns
            if (holdingFilterDropdown != null)
            {
                holdingFilterDropdown.ClearOptions();
                holdingFilterDropdown.AddOptions(new List<string> { "All", "Direct Cities", "Vassals" });
                holdingFilterDropdown.onValueChanged.AddListener(OnHoldingFilterChanged);
            }

            if (cultureReligionToggle != null)
            {
                cultureReligionToggle.ClearOptions();
                cultureReligionToggle.AddOptions(new List<string> { "Culture", "Religion" });
                cultureReligionToggle.onValueChanged.AddListener(OnCultureReligionToggleChanged);
            }
        }

        private void Update()
        {
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
            mainTabButton?.onClick.RemoveAllListeners();
            landsTabButton?.onClick.RemoveAllListeners();
            economicsTabButton?.onClick.RemoveAllListeners();
            governmentTabButton?.onClick.RemoveAllListeners();
            cultureReligionTabButton?.onClick.RemoveAllListeners();
            militaryTabButton?.onClick.RemoveAllListeners();
            diplomacyTabButton?.onClick.RemoveAllListeners();

            bureaucracyButton?.onClick.RemoveAllListeners();
            contractsButton?.onClick.RemoveAllListeners();
            governmentButton?.onClick.RemoveAllListeners();
            cultureButton?.onClick.RemoveAllListeners();
            religionButton?.onClick.RemoveAllListeners();

            if (holdingFilterDropdown != null)
                holdingFilterDropdown.onValueChanged.RemoveAllListeners();
            if (cultureReligionToggle != null)
                cultureReligionToggle.onValueChanged.RemoveAllListeners();
        }

        // ========== TAB MANAGEMENT ==========

        private void ShowTab(GameObject tab)
        {
            // Hide all tabs
            mainTab?.SetActive(false);
            landsTab?.SetActive(false);
            economicsTab?.SetActive(false);
            governmentTab?.SetActive(false);
            cultureReligionTab?.SetActive(false);
            militaryTab?.SetActive(false);
            diplomacyTab?.SetActive(false);

            // Show selected tab
            tab?.SetActive(true);

            // Refresh tab content
            RefreshCurrentTab(tab);
        }

        private void RefreshCurrentTab(GameObject activeTab)
        {
            if (activeTab == mainTab)
                RefreshMainTab();
            else if (activeTab == landsTab)
                RefreshLandsTab();
            else if (activeTab == economicsTab)
                RefreshEconomicsTab();
            else if (activeTab == governmentTab)
                RefreshGovernmentTab();
            else if (activeTab == cultureReligionTab)
                RefreshCultureReligionTab();
            else if (activeTab == militaryTab)
                RefreshMilitaryTab();
            else if (activeTab == diplomacyTab)
                RefreshDiplomacyTab();
        }

        private GameObject GetActiveTab()
        {
            if (mainTab != null && mainTab.activeSelf) return mainTab;
            if (landsTab != null && landsTab.activeSelf) return landsTab;
            if (economicsTab != null && economicsTab.activeSelf) return economicsTab;
            if (governmentTab != null && governmentTab.activeSelf) return governmentTab;
            if (cultureReligionTab != null && cultureReligionTab.activeSelf) return cultureReligionTab;
            if (militaryTab != null && militaryTab.activeSelf) return militaryTab;
            if (diplomacyTab != null && diplomacyTab.activeSelf) return diplomacyTab;
            return null;
        }

        // ========== REFRESH UI ==========

        public void RefreshUI()
        {
            if (viewModel == null) return;

            viewModel.Refresh();
            RefreshCurrentTab(GetActiveTab());
        }

        // ========== TAB 1: MAIN INFO ==========

        private void RefreshMainTab()
        {
            // Basic info
            if (realmNameText != null)
                realmNameText.text = viewModel.RealmName;

            if (leaderNamesText != null)
                leaderNamesText.text = viewModel.LeaderNamesDisplay;

            if (governmentNameText != null)
                governmentNameText.text = $"{viewModel.GovernmentName}\n<size=12>{viewModel.GovernmentTypeDisplay}</size>";

            // Stability
            if (stabilitySlider != null)
                stabilitySlider.value = viewModel.Stability / 100f;

            if (stabilityText != null)
                stabilityText.text = $"Stability: {viewModel.StabilityDisplay}";

            if (stabilityFillImage != null)
                stabilityFillImage.color = viewModel.StabilityColor;

            // Prestige
            if (prestigeSlider != null)
                prestigeSlider.value = viewModel.Prestige / 100f;

            if (prestigeText != null)
                prestigeText.text = $"Prestige: {viewModel.PrestigeDisplay}";

            // Income/Expense
            if (totalIncomeText != null)
                totalIncomeText.text = $"<b>Income:</b>\n{viewModel.IncomeDisplayText}";

            if (totalExpensesText != null)
                totalExpensesText.text = $"<b>Expenses:</b>\n{viewModel.ExpenseDisplayText}";

            if (netIncomeText != null)
                netIncomeText.text = $"<b>Net Income (per day):</b>\n{viewModel.NetIncomeDisplayText}";

            if (treasuryGoldText != null)
                treasuryGoldText.text = $"Treasury: {viewModel.TreasuryGoldDisplay}";

            // Leader icons (optional)
            RefreshLeaderIcons();
        }

        private void RefreshLeaderIcons()
        {
            if (leaderIconsContainer == null || leaderIconPrefab == null)
                return;

            // Clear existing icons
            ClearContainer(leaderIconsContainer);

            // Create icons for each leader
            foreach (var leader in viewModel.Leaders)
            {
                var icon = Instantiate(leaderIconPrefab, leaderIconsContainer);

                // Set icon sprite if available
                var iconImage = icon.GetComponent<Image>();
                if (iconImage != null && leader.Icon != null)
                {
                    iconImage.sprite = leader.Icon;
                }

                // Set tooltip or name text
                var nameText = icon.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = $"{leader.Name}\nAge {leader.Age}";
                }

                spawnedItems.Add(icon);
            }
        }

        // ========== TAB 2: REALM LANDS ==========

        private void RefreshLandsTab()
        {
            if (holdingsSummaryText != null)
                holdingsSummaryText.text = viewModel.HoldingsSummary;

            RefreshHoldingsList();
        }

        private void RefreshHoldingsList()
        {
            if (holdingsListContainer == null || holdingItemPrefab == null)
                return;

            ClearContainer(holdingsListContainer);

            // Get filter
            int filter = holdingFilterDropdown != null ? holdingFilterDropdown.value : 0;

            foreach (var holding in viewModel.Holdings)
            {
                // Apply filter
                if (filter == 1 && !holding.IsDirectlyOwned) continue; // Direct only
                if (filter == 2 && holding.IsDirectlyOwned) continue;  // Vassals only

                var item = Instantiate(holdingItemPrefab, holdingsListContainer);

                // Set holding name
                var nameText = item.transform.Find("HoldingName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = holding.Name;

                // Set type
                var typeText = item.transform.Find("Type")?.GetComponent<TextMeshProUGUI>();
                if (typeText != null)
                    typeText.text = holding.Type;

                // Set population
                var popText = item.transform.Find("Population")?.GetComponent<TextMeshProUGUI>();
                if (popText != null)
                    popText.text = $"Pop: {holding.Population:N0}";

                // Set income
                var incomeText = item.transform.Find("Income")?.GetComponent<TextMeshProUGUI>();
                if (incomeText != null)
                {
                    if (holding.IsDirectlyOwned)
                        incomeText.text = $"Tax: {holding.Income:N0}g";
                    else
                        incomeText.text = $"Tribute: {holding.Income:N0}g ({holding.TributeRate:F0}%)";
                }

                spawnedItems.Add(item);
            }
        }

        private void OnHoldingFilterChanged(int value)
        {
            RefreshHoldingsList();
        }

        // ========== TAB 3: ECONOMICS ==========

        private void RefreshEconomicsTab()
        {
            if (economicBreakdownText != null)
                economicBreakdownText.text = viewModel.EconomicBreakdown;

            RefreshTopCities();
            RefreshResourceProduction();
        }

        private void RefreshTopCities()
        {
            if (topCitiesContainer == null || cityEconomicItemPrefab == null)
                return;

            ClearContainer(topCitiesContainer);

            foreach (var city in viewModel.TopEarningCities)
            {
                var item = Instantiate(cityEconomicItemPrefab, topCitiesContainer);

                var nameText = item.transform.Find("CityName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = city.CityName;

                var taxText = item.transform.Find("TaxIncome")?.GetComponent<TextMeshProUGUI>();
                if (taxText != null)
                    taxText.text = $"Tax: {city.TaxIncome:N0}g";

                var prodText = item.transform.Find("Production")?.GetComponent<TextMeshProUGUI>();
                if (prodText != null)
                    prodText.text = $"Production: {city.GoldProduction:N0}g";

                var popText = item.transform.Find("Population")?.GetComponent<TextMeshProUGUI>();
                if (popText != null)
                    popText.text = $"Pop: {city.Population:N0}";

                spawnedItems.Add(item);
            }
        }

        private void RefreshResourceProduction()
        {
            if (resourceProductionText != null)
            {
                var text = "<b>Production:</b>\n";
                foreach (var kvp in viewModel.TotalProduction)
                {
                    if (kvp.Value > 0)
                        text += $"{kvp.Key}: +{kvp.Value:N0}\n";
                }
                resourceProductionText.text = text;
            }

            if (resourceConsumptionText != null)
            {
                var text = "<b>Consumption:</b>\n";
                foreach (var kvp in viewModel.TotalConsumption)
                {
                    if (kvp.Value > 0)
                        text += $"{kvp.Key}: -{kvp.Value:N0}\n";
                }
                resourceConsumptionText.text = text;
            }
        }

        // ========== TAB 4: GOVERNMENT ==========

        private void RefreshGovernmentTab()
        {
            if (stabilityBreakdownText != null)
                stabilityBreakdownText.text = viewModel.StabilityBreakdown;

            if (officeStatsText != null)
                officeStatsText.text = $"Offices: {viewModel.FilledOffices}/{viewModel.TotalOffices} filled";

            RefreshOfficers();
            RefreshVassalLoyalty();
        }

        private void RefreshOfficers()
        {
            if (officersContainer == null || officerItemPrefab == null)
            {
                Debug.LogWarning($"[RealmUIController] Cannot refresh officers - Container: {(officersContainer == null ? "NULL" : "OK")}, Prefab: {(officerItemPrefab == null ? "NULL" : "OK")}");
                return;
            }

            ClearContainer(officersContainer);

            Debug.Log($"[RealmUIController] Spawning {viewModel.Officers.Count} officer UI items");

            foreach (var officer in viewModel.Officers)
            {
                var item = Instantiate(officerItemPrefab, officersContainer);

                var nameText = item.transform.Find("OfficeName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = officer.OfficeName;

                var holderText = item.transform.Find("Holder")?.GetComponent<TextMeshProUGUI>();
                if (holderText != null)
                {
                    holderText.text = officer.IsFilled ? officer.HolderName : "<i>Vacant</i>";
                    holderText.color = officer.IsFilled ? Color.white : Color.gray;
                }

                var salaryText = item.transform.Find("Salary")?.GetComponent<TextMeshProUGUI>();
                if (salaryText != null)
                    salaryText.text = $"Salary: {officer.Salary:N0}g";

                spawnedItems.Add(item);
                Debug.Log($"[RealmUIController] Spawned officer UI: {officer.OfficeName}, Holder: {officer.HolderName}");
            }
        }

        private void RefreshVassalLoyalty()
        {
            if (loyalVassalsContainer != null && vassalItemPrefab != null)
            {
                ClearContainer(loyalVassalsContainer);

                foreach (var vassal in viewModel.LoyalVassals)
                {
                    var item = Instantiate(vassalItemPrefab, loyalVassalsContainer);

                    var nameText = item.transform.Find("VassalName")?.GetComponent<TextMeshProUGUI>();
                    if (nameText != null)
                        nameText.text = vassal.VassalName;

                    var loyaltyText = item.transform.Find("Loyalty")?.GetComponent<TextMeshProUGUI>();
                    if (loyaltyText != null)
                        loyaltyText.text = $"{vassal.LoyaltyStatus} ({vassal.Loyalty:F0}%)";

                    var tributeText = item.transform.Find("Tribute")?.GetComponent<TextMeshProUGUI>();
                    if (tributeText != null)
                        tributeText.text = $"Tribute: {vassal.TributeRate:F0}%";

                    spawnedItems.Add(item);
                }
            }

            if (disloyalVassalsContainer != null && vassalItemPrefab != null)
            {
                ClearContainer(disloyalVassalsContainer);

                foreach (var vassal in viewModel.DisloyalVassals)
                {
                    var item = Instantiate(vassalItemPrefab, disloyalVassalsContainer);

                    var nameText = item.transform.Find("VassalName")?.GetComponent<TextMeshProUGUI>();
                    if (nameText != null)
                        nameText.text = vassal.VassalName;

                    var loyaltyText = item.transform.Find("Loyalty")?.GetComponent<TextMeshProUGUI>();
                    if (loyaltyText != null)
                    {
                        loyaltyText.text = $"{vassal.LoyaltyStatus} ({vassal.Loyalty:F0}%)";
                        loyaltyText.color = new Color(0.9f, 0.2f, 0.2f); // Red for disloyal
                    }

                    var tributeText = item.transform.Find("Tribute")?.GetComponent<TextMeshProUGUI>();
                    if (tributeText != null)
                        tributeText.text = $"Tribute: {vassal.TributeRate:F0}%";

                    spawnedItems.Add(item);
                }
            }
        }

        // ========== TAB 5: CULTURE/RELIGION ==========

        private void RefreshCultureReligionTab()
        {
            bool showingCulture = cultureReligionToggle == null || cultureReligionToggle.value == 0;

            if (showingCulture)
                RefreshCultureView();
            else
                RefreshReligionView();
        }

        private void RefreshCultureView()
        {
            if (dominantCultureReligionText != null)
                dominantCultureReligionText.text = $"<b>Dominant Culture:</b> {viewModel.DominantCulture}";

            if (unityText != null)
                unityText.text = $"<b>Cultural Unity:</b> {viewModel.CulturalUnity:F0}%";

            Debug.Log($"[RealmUIController] Refreshing culture view - Dominant: {viewModel.DominantCulture}, Unity: {viewModel.CulturalUnity:F0}%");

            // Clear containers
            if (byCityContainer != null)
                ClearContainer(byCityContainer);
            if (byPopulationContainer != null)
                ClearContainer(byPopulationContainer);
            if (byClassContainer != null)
                ClearContainer(byClassContainer);

            // Spawn demographic items for cultures by city
            if (byCityContainer != null && demographicItemPrefab != null && viewModel.CulturesByCity != null)
            {
                Debug.Log($"[RealmUIController] Spawning {viewModel.CulturesByCity.Count} culture by city items");
                foreach (var item in viewModel.CulturesByCity)
                {
                    SpawnDemographicItem(byCityContainer, item.CityName, item.CultureName, item.Population, item.Percentage);
                }
            }

            // Spawn demographic items for cultures by population size
            if (byPopulationContainer != null && demographicItemPrefab != null && viewModel.CulturesByPopulation != null)
            {
                foreach (var item in viewModel.CulturesByPopulation)
                {
                    string classMame = ""; // Need to add helpper method to get population class name when searching culture by population size
                    SpawnDemographicItem(byPopulationContainer, item.CultureName, classMame, item.TotalPopulation, item.Percentage);
                }
            }

            // Spawn demographic items for cultures by class
            if (byClassContainer != null && demographicItemPrefab != null && viewModel.CulturesByClass != null)
            {
                foreach (var item in viewModel.CulturesByClass)
                {
                    SpawnDemographicItem(byClassContainer, item.CultureName, item.ClassName, item.Population, item.Percentage);
                }
            }
        }

        private void RefreshReligionView()
        {
            if (dominantCultureReligionText != null)
                dominantCultureReligionText.text = $"<b>Dominant Religion:</b> {viewModel.DominantReligion}";

            if (unityText != null)
                unityText.text = $"<b>Religious Unity:</b> {viewModel.ReligiousUnity:F0}%";

            Debug.Log($"[RealmUIController] Refreshing religion view - Dominant: {viewModel.DominantReligion}, Unity: {viewModel.ReligiousUnity:F0}%");

            // Clear containers
            if (byCityContainer != null)
                ClearContainer(byCityContainer);
            if (byPopulationContainer != null)
                ClearContainer(byPopulationContainer);
            if (byClassContainer != null)
                ClearContainer(byClassContainer);

            // Spawn demographic items for religions by city
            if (byCityContainer != null && demographicItemPrefab != null && viewModel.ReligionsByCity != null)
            {
                foreach (var item in viewModel.ReligionsByCity)
                {
                    SpawnDemographicItem(byCityContainer, item.CityName, item.ReligionName, item.Population, item.Percentage);
                }
            }

            // Spawn demographic items for religions by population size
            if (byPopulationContainer != null && demographicItemPrefab != null && viewModel.ReligionsByPopulation != null)
            {
                foreach (var item in viewModel.ReligionsByPopulation)
                {
                    string popClass = ""; // Need to add helpper method to get population class name when searching religion by population size
                    SpawnDemographicItem(byPopulationContainer, item.ReligionName, popClass, item.TotalPopulation, item.Percentage);
                }
            }

            // Spawn demographic items for religions by class
            if (byClassContainer != null && demographicItemPrefab != null && viewModel.ReligionsByClass != null)
            {
                foreach (var item in viewModel.ReligionsByClass)
                {
                    SpawnDemographicItem(byClassContainer, item.ReligionName, item.ClassName, item.Population, item.Percentage);
                }
            }
        }

        private void SpawnDemographicItem(Transform container, string primaryText, string secondaryText, int population, float percentage)
        {
            var item = Instantiate(demographicItemPrefab, container);

            var primaryTextComponent = item.transform.Find("PrimaryText")?.GetComponent<TextMeshProUGUI>();
            if (primaryTextComponent != null)
                primaryTextComponent.text = primaryText;

            var secondaryTextComponent = item.transform.Find("SecondaryText")?.GetComponent<TextMeshProUGUI>();
            if (secondaryTextComponent != null)
                secondaryTextComponent.text = secondaryText;

            var populationText = item.transform.Find("Population")?.GetComponent<TextMeshProUGUI>();
            if (populationText != null)
                populationText.text = $"{population:N0}";

            var percentageText = item.transform.Find("Percentage")?.GetComponent<TextMeshProUGUI>();
            if (percentageText != null)
                percentageText.text = $"{percentage:F1}%";

            spawnedItems.Add(item);
        }

        private void OnCultureReligionToggleChanged(int value)
        {
            RefreshCultureReligionTab();
        }

        // ========== TAB 6: MILITARY ==========

        private void RefreshMilitaryTab()
        {
            // Summary text
            if (militarySummaryText != null)
            {
                militarySummaryText.text = "<b>Military Overview</b>\n\n" +
                    "War Parties: Coming Soon\n" +
                    "Total Forces: TBD\n" +
                    "Active Campaigns: TBD";
            }

            // War parties list (placeholder - will be implemented when war party system exists)
            if (warPartiesContainer != null && warPartyItemPrefab != null)
            {
                ClearContainer(warPartiesContainer);

                // TODO: When war party system is implemented:
                // 1. Get war parties from realm/military manager
                // 2. Instantiate warPartyItemPrefab for each party
                // 3. Display party name, size, location, status, commander, etc.
                // 4. Add click handlers to view/manage parties
            }
        }

        // ========== TAB 7: DIPLOMACY ==========

        private void RefreshDiplomacyTab()
        {
            if (diplomacyPlaceholderText != null)
                diplomacyPlaceholderText.text = "Diplomacy features coming soon...\n\n" +
                    "Future features:\n" +
                    "- Wars & Alliances\n" +
                    "- Treaties & Agreements\n" +
                    "- Trade Routes\n" +
                    "- Diplomatic Relations";
        }

        // ========== NAVIGATION BUTTON HANDLERS ==========

        private void OnBureaucracyClicked()
        {
            Debug.Log($"[RealmUIController] Opening bureaucracy menu for realm {targetRealmID}");
            // TODO: Open bureaucracy UI (offices, appointments)
        }

        private void OnContractsClicked()
        {
            Debug.Log($"[RealmUIController] Opening contracts menu for realm {targetRealmID}");
            // TODO: Open contracts UI (vassal management)
        }

        private void OnGovernmentClicked()
        {
            Debug.Log($"[RealmUIController] Opening government menu for realm {targetRealmID}");
            // TODO: Open government UI (reforms, edicts)
        }

        private void OnCultureClicked()
        {
            Debug.Log($"[RealmUIController] Opening culture menu for realm {targetRealmID}");
            // TODO: Open culture UI
        }

        private void OnReligionClicked()
        {
            Debug.Log($"[RealmUIController] Opening religion menu for realm {targetRealmID}");
            // TODO: Open religion UI
        }

        // ========== HELPER METHODS ==========

        private void ClearContainer(Transform container)
        {
            foreach (var item in spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }
            spawnedItems.Clear();
        }

        /// <summary>
        /// Change the target realm being displayed.
        /// </summary>
        public void SetTargetRealm(int realmID)
        {
            targetRealmID = realmID;
            viewModel = new RealmViewModel(targetRealmID);
            RefreshUI();
        }

        /// <summary>
        /// Force an immediate refresh (useful for debugging).
        /// </summary>
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            RefreshUI();
            Debug.Log("[RealmUIController] Force refresh completed");
        }
    }
}
