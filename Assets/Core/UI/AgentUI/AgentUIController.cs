using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Agents;
using TWK.Cultures;
using TWK.UI.ViewModels;
using TWK.UI.Common;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for the Agent UI.
    /// Displays agent information across multiple tabs: Main, Equipment, Lands, Relationships.
    /// </summary>
    public class AgentUIController : MonoBehaviour
    {
        // ========== REFERENCES ==========
        [Header("Agent Selection")]
        [SerializeField] private TMP_Dropdown agentDropdown;

        [Header("Tab Buttons")]
        [SerializeField] private Button mainTabButton;
        [SerializeField] private Button equipmentTabButton;
        [SerializeField] private Button landsTabButton;
        [SerializeField] private Button relationshipsTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject mainTabPanel;
        [SerializeField] private GameObject equipmentTabPanel;
        [SerializeField] private GameObject landsTabPanel;
        [SerializeField] private GameObject relationshipsTabPanel;

        // ========== MAIN TAB ==========
        [Header("Main Tab - Identity")]
        [SerializeField] private Image agentIconImage;
        [SerializeField] private TextMeshProUGUI agentNameText;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI genderText;
        [SerializeField] private TextMeshProUGUI sexualityText;
        [SerializeField] private TextMeshProUGUI lifeStatusText;
        [SerializeField] private TextMeshProUGUI cultureText;
        [SerializeField] private TextMeshProUGUI religionText;

        [Header("Main Tab - Reputation")]
        [SerializeField] private TextMeshProUGUI prestigeText;
        [SerializeField] private TextMeshProUGUI moralityText;
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private TextMeshProUGUI reputationLevelText;

        [Header("Main Tab - Relationships")]
        [SerializeField] private TextMeshProUGUI familySummaryText;
        [SerializeField] private TextMeshProUGUI socialSummaryText;

        [Header("Main Tab - Wealth")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI wealthStatusText;
        [SerializeField] private TextMeshProUGUI propertySummaryText;

        [Header("Main Tab - Traits")]
        [SerializeField] private Transform traitsContainer;
        [SerializeField] private GameObject traitItemPrefab;

        [Header("Main Tab - Skills")]
        [SerializeField] private Button warfareSkillButton;
        [SerializeField] private Button politicsSkillButton;
        [SerializeField] private Button economicsSkillButton;
        [SerializeField] private Button scienceSkillButton;
        [SerializeField] private Button religionSkillButton;
        [SerializeField] private TextMeshProUGUI warfareSkillText;
        [SerializeField] private TextMeshProUGUI politicsSkillText;
        [SerializeField] private TextMeshProUGUI economicsSkillText;
        [SerializeField] private TextMeshProUGUI scienceSkillText;
        [SerializeField] private TextMeshProUGUI religionSkillText;

        [Header("Main Tab - Modifiers")]
        [SerializeField] private ModifierDisplayPanel modifierDisplayPanel;

        // ========== EQUIPMENT TAB ==========
        [Header("Equipment Tab - Slots")]
        [SerializeField] private Transform weaponSlotsContainer;
        [SerializeField] private GameObject weaponSlotPrefab;
        [SerializeField] private Image headSlotImage;
        [SerializeField] private Image bodySlotImage;
        [SerializeField] private Image legsSlotImage;
        [SerializeField] private Image shieldSlotImage;
        [SerializeField] private Image mountSlotImage;

        [Header("Equipment Tab - Combat Stats")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI leadershipText;
        [SerializeField] private TextMeshProUGUI moraleText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI agilityText;
        [SerializeField] private TextMeshProUGUI meleeAttackText;
        [SerializeField] private TextMeshProUGUI meleeArmorText;
        [SerializeField] private TextMeshProUGUI missileAttackText;
        [SerializeField] private TextMeshProUGUI missileDefenseText;

        // ========== LANDS TAB ==========
        [Header("Lands Tab")]
        [SerializeField] private Transform buildingsContainer;
        [SerializeField] private Transform citiesContainer;
        [SerializeField] private Transform caravansContainer;
        [SerializeField] private GameObject buildingItemPrefab; // Reuse from Realm UI
        [SerializeField] private GameObject cityItemPrefab; // Reuse from Realm UI
        [SerializeField] private TextMeshProUGUI totalIncomeText;

        // ========== RELATIONSHIPS TAB ==========
        [Header("Relationships Tab")]
        [SerializeField] private Transform relationshipsContainer;
        [SerializeField] private GameObject relationshipItemPrefab;
        [SerializeField] private Button dynastyTreeButton;
        [SerializeField] private TMP_Dropdown relationshipFilterDropdown;

        // ========== STATE ==========
        private AgentViewModel currentViewModel;
        private int currentAgentID = -1;
        private TabType currentTab = TabType.Main;

        private enum TabType
        {
            Main,
            Equipment,
            Lands,
            Relationships
        }

        // ========== INITIALIZATION ==========

        private void Start()
        {
            SetupTabButtons();
            SetupSkillButtons();
            SetupDynastyButton();
            SubscribeToEvents();

            // Delay agent dropdown setup to ensure ViewModelService is initialized
            StartCoroutine(DelayedInitialization());

            ShowTab(TabType.Main);
        }

        private System.Collections.IEnumerator DelayedInitialization()
        {
            // Wait one frame to ensure ViewModelService has registered agents
            yield return null;

            SetupAgentDropdown();

            if (agentDropdown != null && agentDropdown.options.Count > 0)
            {
                OnAgentChanged(0);
            }
            else
            {
                Debug.LogWarning("[AgentUIController] No agents available in dropdown");
            }
        }

        private void SubscribeToEvents()
        {
            // Subscribe to ViewModelService events
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnAgentViewModelUpdated += OnAgentViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated += RefreshCurrentAgent;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnAgentViewModelUpdated -= OnAgentViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated -= RefreshCurrentAgent;
            }
        }

        // ========== SETUP ==========

        private void SetupAgentDropdown()
        {
            if (agentDropdown == null) return;

            agentDropdown.ClearOptions();

            var agents = AgentManager.Instance?.GetAllAgents();
            if (agents == null || agents.Count == 0)
            {
                Debug.LogWarning("[AgentUIController] No agents found");
                return;
            }

            var options = new List<string>();
            foreach (var agent in agents)
            {
                options.Add($"{agent.Data.AgentName} (ID: {agent.Data.AgentID})");
            }

            agentDropdown.AddOptions(options);
            agentDropdown.onValueChanged.AddListener(OnAgentChanged);
        }

        private void SetupTabButtons()
        {
            mainTabButton?.onClick.AddListener(() => ShowTab(TabType.Main));
            equipmentTabButton?.onClick.AddListener(() => ShowTab(TabType.Equipment));
            landsTabButton?.onClick.AddListener(() => ShowTab(TabType.Lands));
            relationshipsTabButton?.onClick.AddListener(() => ShowTab(TabType.Relationships));
        }

        private void SetupSkillButtons()
        {
            warfareSkillButton?.onClick.AddListener(() => OpenSkillTree(TreeType.Warfare));
            politicsSkillButton?.onClick.AddListener(() => OpenSkillTree(TreeType.Politics));
            economicsSkillButton?.onClick.AddListener(() => OpenSkillTree(TreeType.Economics));
            scienceSkillButton?.onClick.AddListener(() => OpenSkillTree(TreeType.Science));
            religionSkillButton?.onClick.AddListener(() => OpenSkillTree(TreeType.Religion));
        }

        private void SetupDynastyButton()
        {
            dynastyTreeButton?.onClick.AddListener(OpenDynastyTree);
        }

        // ========== TAB MANAGEMENT ==========

        private void ShowTab(TabType tab)
        {
            currentTab = tab;

            // Hide all panels
            mainTabPanel?.SetActive(false);
            equipmentTabPanel?.SetActive(false);
            landsTabPanel?.SetActive(false);
            relationshipsTabPanel?.SetActive(false);

            // Show selected panel
            switch (tab)
            {
                case TabType.Main:
                    mainTabPanel?.SetActive(true);
                    RefreshMainTab();
                    break;
                case TabType.Equipment:
                    equipmentTabPanel?.SetActive(true);
                    RefreshEquipmentTab();
                    break;
                case TabType.Lands:
                    landsTabPanel?.SetActive(true);
                    RefreshLandsTab();
                    break;
                case TabType.Relationships:
                    relationshipsTabPanel?.SetActive(true);
                    RefreshRelationshipsTab();
                    break;
            }
        }

        // ========== AGENT SELECTION ==========

        private void OnAgentChanged(int dropdownIndex)
        {
            var agents = AgentManager.Instance?.GetAllAgents();
            if (agents == null || dropdownIndex >= agents.Count)
            {
                Debug.LogWarning($"[AgentUIController] Invalid agent selection: index {dropdownIndex}");
                return;
            }

            var selectedAgent = agents[dropdownIndex];
            currentAgentID = selectedAgent.Data.AgentID;

            Debug.Log($"[AgentUIController] Agent changed to: {selectedAgent.Data.AgentName} (ID: {currentAgentID})");

            // Check if ViewModelService is available
            if (ViewModelService.Instance == null)
            {
                Debug.LogError("[AgentUIController] ViewModelService.Instance is null!");
                return;
            }

            // Get or create ViewModel
            currentViewModel = ViewModelService.Instance.GetAgentViewModel(currentAgentID);
            if (currentViewModel == null)
            {
                Debug.LogError($"[AgentUIController] Failed to get ViewModel for agent {currentAgentID}. Agent may not be registered in ViewModelService.");
                return;
            }

            Debug.Log($"[AgentUIController] ViewModel retrieved successfully for {selectedAgent.Data.AgentName}");
            RefreshCurrentTab();
        }

        private void OnAgentViewModelUpdated(int agentID)
        {
            if (agentID == currentAgentID)
            {
                RefreshCurrentTab();
            }
        }

        private void RefreshCurrentAgent()
        {
            RefreshCurrentTab();
        }

        private void RefreshCurrentTab()
        {
            ShowTab(currentTab);
        }

        /// <summary>
        /// Public method to set the displayed agent by ID.
        /// Useful for external code that wants to open the agent UI to a specific agent.
        /// </summary>
        public void SetAgent(int agentID)
        {
            // Find the agent in the dropdown
            var agents = AgentManager.Instance?.GetAllAgents();
            if (agents == null) return;

            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].Data.AgentID == agentID)
                {
                    agentDropdown.value = i;
                    OnAgentChanged(i);
                    return;
                }
            }

            Debug.LogWarning($"[AgentUIController] Agent {agentID} not found in agent list");
        }

        // ========== MAIN TAB ==========

        private void RefreshMainTab()
        {
            if (currentViewModel == null)
            {
                Debug.LogWarning("[AgentUIController] RefreshMainTab called but currentViewModel is null");
                return;
            }

            Debug.Log($"[AgentUIController] Refreshing main tab for agent {currentViewModel.AgentName}");

            // Identity
            if (agentNameText != null) agentNameText.text = currentViewModel.AgentName;
            if (ageText != null) ageText.text = $"Age: {currentViewModel.Age}";
            if (genderText != null) genderText.text = currentViewModel.Gender;
            if (sexualityText != null) sexualityText.text = currentViewModel.Sexuality;
            if (lifeStatusText != null) lifeStatusText.text = currentViewModel.LifeStatus;
            if (cultureText != null) cultureText.text = $"Culture: {currentViewModel.CultureName}";
            if (religionText != null) religionText.text = $"Religion: {currentViewModel.ReligionName}";

            // Reputation
            if (prestigeText != null) prestigeText.text = $"Prestige: {currentViewModel.Prestige:F1}";
            if (moralityText != null) moralityText.text = $"Morality: {currentViewModel.Morality:F1}";
            if (reputationText != null) reputationText.text = $"Reputation: {currentViewModel.Reputation:F1}";
            if (reputationLevelText != null) reputationLevelText.text = currentViewModel.ReputationLevel;

            // Relationships
            if (familySummaryText != null) familySummaryText.text = currentViewModel.FamilySummary;
            if (socialSummaryText != null) socialSummaryText.text = currentViewModel.SocialSummary;

            // Wealth
            if (goldText != null) goldText.text = $"Gold: {currentViewModel.Gold}";
            if (wealthStatusText != null) wealthStatusText.text = currentViewModel.WealthStatus;
            if (propertySummaryText != null) propertySummaryText.text = currentViewModel.PropertySummary;

            // Traits
            RefreshTraits();

            // Skills
            RefreshSkills();

            // Modifiers
            RefreshModifiers();
        }

        private void RefreshTraits()
        {
            if (traitsContainer == null || traitItemPrefab == null) return;

            // Clear existing traits
            foreach (Transform child in traitsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create trait items
            foreach (var trait in currentViewModel.Traits)
            {
                var traitItem = Instantiate(traitItemPrefab, traitsContainer);
                var traitText = traitItem.GetComponentInChildren<TextMeshProUGUI>();
                if (traitText != null)
                {
                    traitText.text = trait.ToString();
                }
            }
        }

        private void RefreshSkills()
        {
            if (currentViewModel.SkillNodeCounts.TryGetValue(TreeType.Warfare, out int warfareNodes))
                if (warfareSkillText != null) warfareSkillText.text = warfareNodes.ToString();

            if (currentViewModel.SkillNodeCounts.TryGetValue(TreeType.Politics, out int politicsNodes))
                if (politicsSkillText != null) politicsSkillText.text = politicsNodes.ToString();

            if (currentViewModel.SkillNodeCounts.TryGetValue(TreeType.Economics, out int economicsNodes))
                if (economicsSkillText != null) economicsSkillText.text = economicsNodes.ToString();

            if (currentViewModel.SkillNodeCounts.TryGetValue(TreeType.Science, out int scienceNodes))
                if (scienceSkillText != null) scienceSkillText.text = scienceNodes.ToString();

            if (currentViewModel.SkillNodeCounts.TryGetValue(TreeType.Religion, out int religionNodes))
                if (religionSkillText != null) religionSkillText.text = religionNodes.ToString();
        }

        private void RefreshModifiers()
        {
            if (modifierDisplayPanel == null) return;

            // Use ModifierDisplayPanel to display all active modifiers
            modifierDisplayPanel.DisplayModifiers(currentViewModel.ActiveModifiers);
        }

        // ========== EQUIPMENT TAB ==========

        private void RefreshEquipmentTab()
        {
            if (currentViewModel == null) return;

            var agent = AgentManager.Instance?.GetAgent(currentAgentID);
            if (agent == null) return;

            var stats = agent.Data.CombatStats;

            // Combat Stats
            if (healthText != null) healthText.text = $"Health: {stats.Health:F0}/{stats.MaxHealth:F0}";
            if (strengthText != null) strengthText.text = $"Strength: {stats.Strength:F0}";
            if (leadershipText != null) leadershipText.text = $"Leadership: {stats.Leadership:F0}";
            if (moraleText != null) moraleText.text = $"Morale: {stats.Morale:F0}";
            if (speedText != null) speedText.text = $"Speed: {stats.Speed:F0}";
            if (agilityText != null) agilityText.text = $"Agility: {stats.Agility:F0}";
            if (meleeAttackText != null) meleeAttackText.text = $"Melee Attack: {stats.MeleeAttack:F0}";
            if (meleeArmorText != null) meleeArmorText.text = $"Melee Armor: {stats.MeleeArmor:F0}";
            if (missileAttackText != null) missileAttackText.text = $"Missile Attack: {stats.MissileAttack:F0}";
            if (missileDefenseText != null) missileDefenseText.text = $"Missile Defense: {stats.MissileDefense:F0}";

            // Equipment Slots (TODO: Display actual equipment icons/names)
            RefreshWeaponSlots(stats);
        }

        private void RefreshWeaponSlots(SoldierStats stats)
        {
            if (weaponSlotsContainer == null || weaponSlotPrefab == null) return;

            // Clear existing slots
            foreach (Transform child in weaponSlotsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create weapon slot items
            for (int i = 0; i < stats.WeaponSlots; i++)
            {
                var slotItem = Instantiate(weaponSlotPrefab, weaponSlotsContainer);
                var slotText = slotItem.GetComponentInChildren<TextMeshProUGUI>();

                if (i < stats.EquippedWeaponIDs.Count)
                {
                    // Weapon equipped
                    if (slotText != null) slotText.text = $"Weapon {stats.EquippedWeaponIDs[i]}";
                }
                else
                {
                    // Empty slot
                    if (slotText != null) slotText.text = "Empty";
                }
            }
        }

        // ========== LANDS TAB ==========

        private void RefreshLandsTab()
        {
            if (currentViewModel == null) return;

            var agent = AgentManager.Instance?.GetAgent(currentAgentID);
            if (agent == null) return;

            // Buildings
            RefreshOwnedBuildings(agent.Data.OwnedBuildingIDs);

            // Cities
            RefreshControlledCities(agent.Data.ControlledCityIDs);

            // Caravans
            RefreshOwnedCaravans(agent.Data.OwnedCaravanIDs);

            // Total income (TODO: Calculate from properties)
            if (totalIncomeText != null) totalIncomeText.text = "Total Income: TBD";
        }

        private void RefreshOwnedBuildings(List<int> buildingIDs)
        {
            if (buildingsContainer == null) return;

            // Clear existing
            foreach (Transform child in buildingsContainer)
            {
                Destroy(child.gameObject);
            }

            // TODO: Create building items using buildingItemPrefab
            // For now, just display IDs
            foreach (int buildingID in buildingIDs)
            {
                // var buildingItem = Instantiate(buildingItemPrefab, buildingsContainer);
                // Configure building item with ID
            }
        }

        private void RefreshControlledCities(List<int> cityIDs)
        {
            if (citiesContainer == null) return;

            // Clear existing
            foreach (Transform child in citiesContainer)
            {
                Destroy(child.gameObject);
            }

            // TODO: Create city items using cityItemPrefab
            foreach (int cityID in cityIDs)
            {
                // var cityItem = Instantiate(cityItemPrefab, citiesContainer);
                // Configure city item with ID
            }
        }

        private void RefreshOwnedCaravans(List<int> caravanIDs)
        {
            if (caravansContainer == null) return;

            // Clear existing
            foreach (Transform child in caravansContainer)
            {
                Destroy(child.gameObject);
            }

            // TODO: Create caravan items
            foreach (int caravanID in caravanIDs)
            {
                // Display caravan info
            }
        }

        // ========== RELATIONSHIPS TAB ==========

        private void RefreshRelationshipsTab()
        {
            if (currentViewModel == null || relationshipsContainer == null) return;

            // Clear existing
            foreach (Transform child in relationshipsContainer)
            {
                Destroy(child.gameObject);
            }

            // Get all relationships for this agent
            var relationships = RelationshipManager.Instance?.GetAgentRelationships(currentAgentID);
            if (relationships == null) return;

            // Create relationship items
            foreach (var relationship in relationships)
            {
                CreateRelationshipItem(relationship);
            }
        }

        private void CreateRelationshipItem(RelationshipData relationship)
        {
            if (relationshipItemPrefab == null) return;

            var relItem = Instantiate(relationshipItemPrefab, relationshipsContainer);

            // Get the other agent
            int otherAgentID = relationship.GetOtherAgent(currentAgentID);
            var otherAgent = AgentManager.Instance?.GetAgent(otherAgentID);
            string otherName = otherAgent?.Data.AgentName ?? "Unknown";

            // Display relationship info
            var relTypeText = relItem.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
            var relNameText = relItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var relStrengthText = relItem.transform.Find("StrengthText")?.GetComponent<TextMeshProUGUI>();

            if (relTypeText != null) relTypeText.text = relationship.Type.ToString();
            if (relNameText != null) relNameText.text = otherName;
            if (relStrengthText != null) relStrengthText.text = relationship.GetStrengthLabel();
        }

        // ========== ACTIONS ==========

        private void OpenSkillTree(TreeType treeType)
        {
            // TODO: Open skill tree UI at the specified tree
            Debug.Log($"[AgentUIController] Opening {treeType} skill tree for agent {currentAgentID}");
        }

        private void OpenDynastyTree()
        {
            // TODO: Open dynasty/family tree view
            Debug.Log($"[AgentUIController] Opening dynasty tree for agent {currentAgentID}");
        }

        // ========== DEBUG ==========

        [ContextMenu("Refresh UI")]
        public void ForceRefresh()
        {
            RefreshCurrentTab();
        }

        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            Debug.Log("===== AgentUIController Debug State =====");
            Debug.Log($"Current Agent ID: {currentAgentID}");
            Debug.Log($"Current ViewModel: {(currentViewModel != null ? "EXISTS" : "NULL")}");
            Debug.Log($"ViewModelService.Instance: {(ViewModelService.Instance != null ? "EXISTS" : "NULL")}");
            Debug.Log($"AgentManager.Instance: {(AgentManager.Instance != null ? "EXISTS" : "NULL")}");

            if (AgentManager.Instance != null)
            {
                var agents = AgentManager.Instance.GetAllAgents();
                Debug.Log($"Total Agents in AgentManager: {agents?.Count ?? 0}");
            }

            if (ViewModelService.Instance != null)
            {
                var allViewModels = ViewModelService.Instance.GetAllAgentViewModels();
                int count = 0;
                foreach (var vm in allViewModels) count++;
                Debug.Log($"Total Agent ViewModels in ViewModelService: {count}");
            }

            if (currentViewModel != null)
            {
                Debug.Log($"ViewModel Agent Name: {currentViewModel.AgentName}");
                Debug.Log($"ViewModel Age: {currentViewModel.Age}");
                Debug.Log($"ViewModel Culture: {currentViewModel.CultureName}");
                Debug.Log($"ViewModel Religion: {currentViewModel.ReligionName}");
            }

            Debug.Log("=========================================");
        }
    }
}
