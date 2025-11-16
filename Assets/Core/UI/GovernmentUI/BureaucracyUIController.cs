using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.UI.ViewModels;
using TWK.Government;

namespace TWK.UI
{
    /// <summary>
    /// Controller for bureaucracy and office management UI.
    /// Displays office information using BureaucracyViewModel.
    /// </summary>
    public class BureaucracyUIController : MonoBehaviour
    {
        [Header("Realm Reference")]
        [SerializeField] private int targetRealmID = 1;
        [Tooltip("If set, will override targetRealmID")]
        [SerializeField] private TWK.Realms.Realm targetRealm;

        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI summaryText;
        [SerializeField] private TextMeshProUGUI totalOfficesText;
        [SerializeField] private TextMeshProUGUI filledOfficesText;
        [SerializeField] private TextMeshProUGUI vacantOfficesText;
        [SerializeField] private TextMeshProUGUI monthlyCostText;
        [SerializeField] private TextMeshProUGUI averageEfficiencyText;
        [SerializeField] private Image efficiencyBarFill;

        [Header("Skill Tree Breakdown")]
        [SerializeField] private TextMeshProUGUI economicsCountText;
        [SerializeField] private TextMeshProUGUI warfareCountText;
        [SerializeField] private TextMeshProUGUI politicsCountText;
        [SerializeField] private TextMeshProUGUI religionCountText;
        [SerializeField] private TextMeshProUGUI scienceCountText;
        [SerializeField] private TextMeshProUGUI skillTreeBreakdownText;

        [Header("Office List")]
        [SerializeField] private Transform officeListContainer;
        [SerializeField] private GameObject officeItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;

        [Header("Office Creation")]
        [SerializeField] private Button createOfficeButton;
        [SerializeField] private TextMeshProUGUI createOfficeCostText;
        [SerializeField] private TMP_InputField officeNameInput;
        [SerializeField] private TMP_Dropdown skillTreeDropdown;
        [SerializeField] private TMP_Dropdown purposeDropdown;

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f;

        private BureaucracyViewModel viewModel;
        private List<GameObject> officeItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;
        private string currentFilter = "All";

        private void Start()
        {
            // Determine realm ID
            if (targetRealm != null)
            {
                targetRealmID = targetRealm.RealmID;
            }

            // Initialize ViewModel
            if (ViewModelService.Instance != null)
            {
                viewModel = ViewModelService.Instance.GetBureaucracyViewModel(targetRealmID);
            }
            else
            {
                viewModel = new BureaucracyViewModel(targetRealmID);
            }

            // Setup UI
            SetupButtons();
            SetupDropdowns();
            SubscribeToEvents();

            // Initial refresh
            RefreshUI();
        }

        private void SetupButtons()
        {
            createOfficeButton?.onClick.RemoveAllListeners();
            createOfficeButton?.onClick.AddListener(OnCreateOfficeClicked);
        }

        private void SetupDropdowns()
        {
            // Setup filter dropdown
            if (filterDropdown != null)
            {
                filterDropdown.ClearOptions();
                filterDropdown.AddOptions(new List<string>
                {
                    "All",
                    "Filled",
                    "Vacant",
                    "Economics",
                    "Warfare",
                    "Politics",
                    "Religion",
                    "Science"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }

            // Setup skill tree dropdown
            if (skillTreeDropdown != null)
            {
                skillTreeDropdown.ClearOptions();
                skillTreeDropdown.AddOptions(new List<string>
                {
                    "Economics",
                    "Warfare",
                    "Politics",
                    "Religion",
                    "Science"
                });
            }

            // Setup purpose dropdown
            if (purposeDropdown != null)
            {
                purposeDropdown.ClearOptions();
                var purposes = new List<string>();
                foreach (OfficePurpose purpose in System.Enum.GetValues(typeof(OfficePurpose)))
                {
                    purposes.Add(purpose.ToString());
                }
                purposeDropdown.AddOptions(purposes);
            }
        }

        private void SubscribeToEvents()
        {
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnBureaucracyViewModelUpdated += OnBureaucracyViewModelUpdated;
            }

            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnOfficeCreated += OnOfficeCreated;
                GovernmentManager.Instance.OnOfficeAssigned += OnOfficeAssigned;
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
            // Unsubscribe from events
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnBureaucracyViewModelUpdated -= OnBureaucracyViewModelUpdated;
            }

            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnOfficeCreated -= OnOfficeCreated;
                GovernmentManager.Instance.OnOfficeAssigned -= OnOfficeAssigned;
            }

            // Clean up listeners
            createOfficeButton?.onClick.RemoveAllListeners();
            filterDropdown?.onValueChanged.RemoveAllListeners();
        }

        public void RefreshUI()
        {
            if (viewModel == null) return;

            viewModel.Refresh();

            RefreshSummary();
            RefreshSkillTreeBreakdown();
            RefreshOfficeList();
            RefreshCreateOfficePanel();
        }

        private void RefreshSummary()
        {
            if (summaryText != null)
                summaryText.text = viewModel.GetOfficeSummary();

            if (totalOfficesText != null)
                totalOfficesText.text = $"Total: {viewModel.TotalOffices}";

            if (filledOfficesText != null)
                filledOfficesText.text = $"Filled: {viewModel.FilledOffices}";

            if (vacantOfficesText != null)
                vacantOfficesText.text = $"Vacant: {viewModel.VacantOffices}";

            if (monthlyCostText != null)
                monthlyCostText.text = viewModel.MonthlySalaryCostDisplay;

            if (averageEfficiencyText != null)
                averageEfficiencyText.text = $"Avg Efficiency: {viewModel.AverageEfficiencyDisplay} ({viewModel.EfficiencyStatus})";

            if (efficiencyBarFill != null)
            {
                efficiencyBarFill.fillAmount = viewModel.AverageEfficiency;
                efficiencyBarFill.color = viewModel.EfficiencyColor;
            }
        }

        private void RefreshSkillTreeBreakdown()
        {
            if (economicsCountText != null)
                economicsCountText.text = $"Economics: {viewModel.OfficesByEconomics}";

            if (warfareCountText != null)
                warfareCountText.text = $"Warfare: {viewModel.OfficesByWarfare}";

            if (politicsCountText != null)
                politicsCountText.text = $"Politics: {viewModel.OfficesByPolitics}";

            if (religionCountText != null)
                religionCountText.text = $"Religion: {viewModel.OfficesByReligion}";

            if (scienceCountText != null)
                scienceCountText.text = $"Science: {viewModel.OfficesByScience}";

            if (skillTreeBreakdownText != null)
                skillTreeBreakdownText.text = viewModel.GetSkillTreeBreakdown();
        }

        private void RefreshOfficeList()
        {
            // Clear existing items
            foreach (var item in officeItems)
            {
                if (item != null)
                    Destroy(item);
            }
            officeItems.Clear();

            if (officeListContainer == null || officeItemPrefab == null)
                return;

            // Get filtered offices
            var offices = GetFilteredOffices();

            // Create new items
            foreach (var office in offices)
            {
                var item = Instantiate(officeItemPrefab, officeListContainer);

                // Set office name
                var nameText = item.transform.Find("OfficeName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = office.OfficeName;

                // Set skill tree
                var skillText = item.transform.Find("SkillTree")?.GetComponent<TextMeshProUGUI>();
                if (skillText != null)
                    skillText.text = office.SkillTree;

                // Set purpose
                var purposeText = item.transform.Find("Purpose")?.GetComponent<TextMeshProUGUI>();
                if (purposeText != null)
                    purposeText.text = office.Purpose;

                // Set assigned agent
                var agentText = item.transform.Find("AssignedAgent")?.GetComponent<TextMeshProUGUI>();
                if (agentText != null)
                {
                    agentText.text = office.AssignedAgentName;
                    agentText.color = office.IsFilled ? Color.white : Color.gray;
                }

                // Set efficiency
                var efficiencyText = item.transform.Find("Efficiency")?.GetComponent<TextMeshProUGUI>();
                if (efficiencyText != null)
                {
                    efficiencyText.text = office.EfficiencyPercentage;
                    efficiencyText.color = office.GetStatusColor();
                }

                // Set status
                var statusText = item.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
                if (statusText != null)
                {
                    statusText.text = office.GetStatusText();
                    statusText.color = office.GetStatusColor();
                }

                // Set salary
                var salaryText = item.transform.Find("Salary")?.GetComponent<TextMeshProUGUI>();
                if (salaryText != null)
                    salaryText.text = $"{office.MonthlySalary} Gold/month";

                // Setup assign button
                var assignButton = item.transform.Find("AssignButton")?.GetComponent<Button>();
                if (assignButton != null)
                {
                    assignButton.onClick.RemoveAllListeners();
                    assignButton.onClick.AddListener(() => OnAssignOfficeClicked(office));
                    assignButton.interactable = true; // Always allow reassignment
                }

                // Setup remove button
                var removeButton = item.transform.Find("RemoveButton")?.GetComponent<Button>();
                if (removeButton != null && office.IsFilled)
                {
                    removeButton.onClick.RemoveAllListeners();
                    removeButton.onClick.AddListener(() => OnRemoveAgentClicked(office));
                }

                officeItems.Add(item);
            }
        }

        private List<OfficeDisplay> GetFilteredOffices()
        {
            switch (currentFilter)
            {
                case "Filled":
                    return viewModel.GetFilledOffices();
                case "Vacant":
                    return viewModel.GetVacantOffices();
                case "Economics":
                    return viewModel.GetOfficesBySkillTree(TWK.Cultures.TreeType.Economics);
                case "Warfare":
                    return viewModel.GetOfficesBySkillTree(TWK.Cultures.TreeType.Warfare);
                case "Politics":
                    return viewModel.GetOfficesBySkillTree(TWK.Cultures.TreeType.Politics);
                case "Religion":
                    return viewModel.GetOfficesBySkillTree(TWK.Cultures.TreeType.Religion);
                case "Science":
                    return viewModel.GetOfficesBySkillTree(TWK.Cultures.TreeType.Science);
                default:
                    return viewModel.Offices;
            }
        }

        private void RefreshCreateOfficePanel()
        {
            if (createOfficeCostText != null)
                createOfficeCostText.text = viewModel.NextOfficeCostDisplay;
        }

        // ========== EVENT HANDLERS ==========

        private void OnBureaucracyViewModelUpdated(int realmID)
        {
            if (realmID == targetRealmID)
            {
                RefreshUI();
            }
        }

        private void OnOfficeCreated(int realmID, Office office)
        {
            if (realmID == targetRealmID)
            {
                RefreshUI();
            }
        }

        private void OnOfficeAssigned(int realmID, Office office)
        {
            if (realmID == targetRealmID)
            {
                RefreshUI();
            }
        }

        private void OnFilterChanged(int index)
        {
            if (filterDropdown != null)
            {
                currentFilter = filterDropdown.options[index].text;
                RefreshOfficeList();
            }
        }

        // ========== BUTTON HANDLERS ==========

        private void OnCreateOfficeClicked()
        {
            if (GovernmentManager.Instance == null)
            {
                Debug.LogError("[BureaucracyUIController] GovernmentManager not available");
                return;
            }

            // Get values from UI
            string officeName = officeNameInput != null ? officeNameInput.text : "New Office";
            if (string.IsNullOrEmpty(officeName))
                officeName = "New Office";

            // Get skill tree
            TWK.Cultures.TreeType skillTree = TWK.Cultures.TreeType.Economics;
            if (skillTreeDropdown != null && skillTreeDropdown.value >= 0)
            {
                skillTree = (TWK.Cultures.TreeType)skillTreeDropdown.value;
            }

            // Get purpose
            OfficePurpose purpose = OfficePurpose.None;
            if (purposeDropdown != null && purposeDropdown.value >= 0)
            {
                purpose = (OfficePurpose)purposeDropdown.value;
            }

            // Create the office
            GovernmentManager.Instance.CreateOffice(targetRealmID, officeName, skillTree, purpose);

            // Clear input
            if (officeNameInput != null)
                officeNameInput.text = "";

            Debug.Log($"[BureaucracyUIController] Created office '{officeName}' for realm {targetRealmID}");
        }

        private void OnAssignOfficeClicked(OfficeDisplay office)
        {
            Debug.Log($"[BureaucracyUIController] Assign button clicked for office: {office.OfficeName}");
            // TODO: Open agent selection panel
        }

        private void OnRemoveAgentClicked(OfficeDisplay office)
        {
            Debug.Log($"[BureaucracyUIController] Remove button clicked for office: {office.OfficeName}");
            // TODO: Implement agent removal from office
        }

        // ========== PUBLIC METHODS ==========

        /// <summary>
        /// Change the target realm being displayed.
        /// </summary>
        public void SetTargetRealm(int realmID)
        {
            targetRealmID = realmID;

            if (ViewModelService.Instance != null)
            {
                viewModel = ViewModelService.Instance.GetBureaucracyViewModel(targetRealmID);
            }
            else
            {
                viewModel = new BureaucracyViewModel(targetRealmID);
            }

            RefreshUI();
        }

        /// <summary>
        /// Force an immediate refresh (useful for debugging).
        /// </summary>
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            RefreshUI();
            Debug.Log("[BureaucracyUIController] Force refresh completed");
        }
    }
}
