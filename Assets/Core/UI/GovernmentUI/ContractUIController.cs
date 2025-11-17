using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.UI.ViewModels;
using TWK.Government;

namespace TWK.UI
{
    /// <summary>
    /// Controller for contract and vassal relationship UI.
    /// Displays contract information using ContractViewModel.
    /// </summary>
    public class ContractUIController : MonoBehaviour
    {
        [Header("Realm Reference")]
        [SerializeField] private int targetRealmID = 1;
        [Tooltip("If set, will override targetRealmID")]
        [SerializeField] private TWK.Realms.Realm targetRealm;

        [Header("Contract List")]
        [SerializeField] private Transform contractListContainer;
        [SerializeField] private GameObject contractItemPrefab;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TextMeshProUGUI contractCountText;

        [Header("Contract Details Panel")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private TextMeshProUGUI contractNameText;
        [SerializeField] private TextMeshProUGUI contractTypeText;
        [SerializeField] private TextMeshProUGUI parentRealmText;
        [SerializeField] private TextMeshProUGUI subjectRealmText;

        [Header("Resource Obligations")]
        [SerializeField] private Slider foodSlider;
        [SerializeField] private TextMeshProUGUI foodPercentageText;
        [SerializeField] private Slider goldSlider;
        [SerializeField] private TextMeshProUGUI goldPercentageText;
        [SerializeField] private Slider pietySlider;
        [SerializeField] private TextMeshProUGUI pietyPercentageText;
        [SerializeField] private Slider prestigeSlider;
        [SerializeField] private TextMeshProUGUI prestigePercentageText;
        [SerializeField] private TextMeshProUGUI totalBurdenText;

        [Header("Manpower Obligations")]
        [SerializeField] private Slider manpowerSlider;
        [SerializeField] private TextMeshProUGUI manpowerPercentageText;

        [Header("Governance")]
        [SerializeField] private TextMeshProUGUI governanceStatusText;
        [SerializeField] private Toggle allowGovernmentToggle;
        [SerializeField] private Toggle canReformToggle;
        [SerializeField] private Toggle parentControlsToggle;

        [Header("Loyalty")]
        [SerializeField] private Slider loyaltySlider;
        [SerializeField] private TextMeshProUGUI loyaltyText;
        [SerializeField] private TextMeshProUGUI loyaltyStatusText;
        [SerializeField] private Image loyaltyFillImage;
        [SerializeField] private Image loyaltyStatusIcon;

        [Header("Subsidies")]
        [SerializeField] private Transform subsidyListContainer;
        [SerializeField] private GameObject subsidyItemPrefab;
        [SerializeField] private TextMeshProUGUI monthlyGoldSubsidyText;

        [Header("Duration")]
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI monthsRemainingText;
        [SerializeField] private Toggle autoRenewToggle;

        [Header("Actions")]
        [SerializeField] private Button modifyTermsButton;
        [SerializeField] private Button terminateContractButton;
        [SerializeField] private Button createContractButton;

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f;

        private List<GameObject> contractItems = new List<GameObject>();
        private List<GameObject> subsidyItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;
        private string currentFilter = "All";
        private ContractViewModel currentContractViewModel;
        private int selectedContractID = -1;

        private void Start()
        {
            // Determine realm ID
            if (targetRealm != null)
            {
                targetRealmID = targetRealm.RealmID;
            }

            // Setup UI
            SetupButtons();
            SetupDropdowns();
            SetupToggles();
            SubscribeToEvents();

            // Hide details panel initially
            if (detailsPanel != null)
                detailsPanel.SetActive(false);

            // Initial refresh
            RefreshUI();
        }

        private void SetupButtons()
        {
            createContractButton?.onClick.RemoveAllListeners();
            createContractButton?.onClick.AddListener(OnCreateContractClicked);

            modifyTermsButton?.onClick.RemoveAllListeners();
            modifyTermsButton?.onClick.AddListener(OnModifyTermsClicked);

            terminateContractButton?.onClick.RemoveAllListeners();
            terminateContractButton?.onClick.AddListener(OnTerminateContractClicked);
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
                    "As Parent",
                    "As Subject",
                    "Governors",
                    "Vassals",
                    "Tributaries",
                    "Warbands",
                    "Loyal",
                    "Disloyal"
                });
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }
        }

        private void SetupToggles()
        {
            // Make toggles non-interactable (display only)
            if (allowGovernmentToggle != null)
                allowGovernmentToggle.interactable = false;
            if (canReformToggle != null)
                canReformToggle.interactable = false;
            if (parentControlsToggle != null)
                parentControlsToggle.interactable = false;
            if (autoRenewToggle != null)
                autoRenewToggle.interactable = false;

            // Make sliders non-interactable (display only)
            if (foodSlider != null) foodSlider.interactable = false;
            if (goldSlider != null) goldSlider.interactable = false;
            if (pietySlider != null) pietySlider.interactable = false;
            if (prestigeSlider != null) prestigeSlider.interactable = false;
            if (manpowerSlider != null) manpowerSlider.interactable = false;
            if (loyaltySlider != null) loyaltySlider.interactable = false;
        }

        private void SubscribeToEvents()
        {
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnContractViewModelUpdated += OnContractViewModelUpdated;
            }

            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated += OnContractCreated;
                ContractManager.Instance.OnContractTerminated += OnContractTerminated;
                ContractManager.Instance.OnLoyaltyChanged += OnLoyaltyChanged;
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
                ViewModelService.Instance.OnContractViewModelUpdated -= OnContractViewModelUpdated;
            }

            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated -= OnContractCreated;
                ContractManager.Instance.OnContractTerminated -= OnContractTerminated;
                ContractManager.Instance.OnLoyaltyChanged -= OnLoyaltyChanged;
            }

            // Clean up listeners
            createContractButton?.onClick.RemoveAllListeners();
            modifyTermsButton?.onClick.RemoveAllListeners();
            terminateContractButton?.onClick.RemoveAllListeners();
            filterDropdown?.onValueChanged.RemoveAllListeners();
        }

        public void RefreshUI()
        {
            RefreshContractList();

            // Refresh selected contract details if one is selected
            if (selectedContractID != -1 && currentContractViewModel != null)
            {
                currentContractViewModel.Refresh();
                RefreshContractDetails();
            }
        }

        private void RefreshContractList()
        {
            // Clear existing items
            foreach (var item in contractItems)
            {
                if (item != null)
                    Destroy(item);
            }
            contractItems.Clear();

            if (contractListContainer == null || contractItemPrefab == null)
                return;

            if (ContractManager.Instance == null)
                return;

            // Get filtered contracts
            var contracts = GetFilteredContracts();

            // Update count
            if (contractCountText != null)
                contractCountText.text = $"Contracts: {contracts.Count}";

            // Create new items
            foreach (var contract in contracts)
            {
                var item = Instantiate(contractItemPrefab, contractListContainer);

                // Set contract type
                var typeText = item.transform.Find("ContractType")?.GetComponent<TextMeshProUGUI>();
                if (typeText != null)
                    typeText.text = contract.Type.ToString();

                // Set parent realm
                var parentText = item.transform.Find("ParentRealm")?.GetComponent<TextMeshProUGUI>();
                if (parentText != null)
                    parentText.text = $"Parent: Realm {contract.ParentRealmID}";

                // Set subject realm
                var subjectText = item.transform.Find("SubjectRealm")?.GetComponent<TextMeshProUGUI>();
                if (subjectText != null)
                {
                    string subjectName = contract.SubjectRealmID != -1
                        ? $"Subject: Realm {contract.SubjectRealmID}"
                        : $"Subject: Agent {contract.SubjectAgentID}";
                    subjectText.text = subjectName;
                }

                // Set loyalty
                var loyaltyText = item.transform.Find("Loyalty")?.GetComponent<TextMeshProUGUI>();
                if (loyaltyText != null)
                {
                    loyaltyText.text = $"Loyalty: {contract.CurrentLoyalty:F0}%";
                    loyaltyText.color = GetLoyaltyColor(contract.CurrentLoyalty);
                }

                // Set status
                var statusText = item.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
                if (statusText != null)
                {
                    statusText.text = contract.GetLoyaltyStatus();
                    statusText.color = GetLoyaltyColor(contract.CurrentLoyalty);
                }

                // Setup select button
                var selectButton = item.GetComponent<Button>();
                if (selectButton != null)
                {
                    int contractID = contract.ContractID;
                    selectButton.onClick.RemoveAllListeners();
                    selectButton.onClick.AddListener(() => OnContractSelected(contractID));
                }

                contractItems.Add(item);
            }
        }

        private List<Contract> GetFilteredContracts()
        {
            var allContracts = new List<Contract>();

            if (ContractManager.Instance == null)
                return allContracts;

            switch (currentFilter)
            {
                case "As Parent":
                    allContracts = ContractManager.Instance.GetContractsAsParent(targetRealmID);
                    break;
                case "As Subject":
                    allContracts = ContractManager.Instance.GetContractsAsSubject(targetRealmID);
                    break;
                case "Governors":
                    allContracts = ContractManager.Instance.GetContractsAsParent(targetRealmID);
                    allContracts.RemoveAll(c => c.Type != ContractType.Governor);
                    break;
                case "Vassals":
                    allContracts = ContractManager.Instance.GetContractsAsParent(targetRealmID);
                    allContracts.RemoveAll(c => c.Type != ContractType.Vassal);
                    break;
                case "Tributaries":
                    allContracts = ContractManager.Instance.GetContractsAsParent(targetRealmID);
                    allContracts.RemoveAll(c => c.Type != ContractType.Tributary);
                    break;
                case "Warbands":
                    allContracts = ContractManager.Instance.GetContractsAsParent(targetRealmID);
                    allContracts.RemoveAll(c => c.Type != ContractType.Warband);
                    break;
                case "Loyal":
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsParent(targetRealmID));
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsSubject(targetRealmID));
                    allContracts.RemoveAll(c => !c.IsLoyal());
                    break;
                case "Disloyal":
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsParent(targetRealmID));
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsSubject(targetRealmID));
                    allContracts.RemoveAll(c => !c.IsDisloyal() && !c.IsRebellious());
                    break;
                default: // "All"
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsParent(targetRealmID));
                    allContracts.AddRange(ContractManager.Instance.GetContractsAsSubject(targetRealmID));
                    break;
            }

            return allContracts;
        }

        private void RefreshContractDetails()
        {
            if (currentContractViewModel == null)
                return;

            // Show details panel
            if (detailsPanel != null)
                detailsPanel.SetActive(true);

            // Basic info
            if (contractNameText != null)
                contractNameText.text = currentContractViewModel.DisplayName;
            if (contractTypeText != null)
                contractTypeText.text = currentContractViewModel.ContractType;
            if (parentRealmText != null)
                parentRealmText.text = $"Parent: {currentContractViewModel.ParentRealmName}";
            if (subjectRealmText != null)
                subjectRealmText.text = $"Subject: {currentContractViewModel.SubjectName}";

            // Resource obligations
            RefreshResourceObligations();

            // Manpower
            if (manpowerSlider != null)
                manpowerSlider.value = currentContractViewModel.ManpowerPercentage / 100f;
            if (manpowerPercentageText != null)
                manpowerPercentageText.text = currentContractViewModel.ManpowerDisplay;

            // Governance
            if (governanceStatusText != null)
                governanceStatusText.text = currentContractViewModel.GovernanceStatus;
            if (allowGovernmentToggle != null)
                allowGovernmentToggle.isOn = currentContractViewModel.AllowSubjectGovernment;
            if (canReformToggle != null)
                canReformToggle.isOn = currentContractViewModel.CanReformGovernment;
            if (parentControlsToggle != null)
                parentControlsToggle.isOn = currentContractViewModel.ParentControlsBureaucracy;

            // Loyalty
            RefreshLoyalty();

            // Subsidies
            RefreshSubsidies();

            // Duration
            if (durationText != null)
                durationText.text = $"Duration: {currentContractViewModel.DurationDisplay}";
            if (monthsRemainingText != null)
                monthsRemainingText.text = $"Remaining: {currentContractViewModel.MonthsRemainingDisplay}";
            if (autoRenewToggle != null)
                autoRenewToggle.isOn = currentContractViewModel.AutoRenew;

            // Button states
            if (modifyTermsButton != null)
                modifyTermsButton.interactable = (currentContractViewModel.ParentRealmID == targetRealmID);
            if (terminateContractButton != null)
                terminateContractButton.interactable = true;
        }

        private void RefreshResourceObligations()
        {
            if (foodSlider != null)
                foodSlider.value = currentContractViewModel.FoodPercentage / 100f;
            if (foodPercentageText != null)
                foodPercentageText.text = $"{currentContractViewModel.FoodPercentage:F0}%";

            if (goldSlider != null)
                goldSlider.value = currentContractViewModel.GoldPercentage / 100f;
            if (goldPercentageText != null)
                goldPercentageText.text = $"{currentContractViewModel.GoldPercentage:F0}%";

            if (pietySlider != null)
                pietySlider.value = currentContractViewModel.PietyPercentage / 100f;
            if (pietyPercentageText != null)
                pietyPercentageText.text = $"{currentContractViewModel.PietyPercentage:F0}%";

            if (prestigeSlider != null)
                prestigeSlider.value = currentContractViewModel.PrestigePercentage / 100f;
            if (prestigePercentageText != null)
                prestigePercentageText.text = $"{currentContractViewModel.PrestigePercentage:F0}%";

            if (totalBurdenText != null)
                totalBurdenText.text = $"Total Burden: {currentContractViewModel.TotalResourceBurdenDisplay}";
        }

        private void RefreshLoyalty()
        {
            if (loyaltySlider != null)
                loyaltySlider.value = currentContractViewModel.Loyalty / 100f;

            if (loyaltyText != null)
                loyaltyText.text = $"Loyalty: {currentContractViewModel.LoyaltyDisplay}";

            if (loyaltyStatusText != null)
            {
                loyaltyStatusText.text = currentContractViewModel.LoyaltyStatus;
                loyaltyStatusText.color = currentContractViewModel.LoyaltyColor;
            }

            if (loyaltyFillImage != null)
                loyaltyFillImage.color = currentContractViewModel.LoyaltyColor;

            // Update status icon
            if (loyaltyStatusIcon != null)
            {
                loyaltyStatusIcon.color = currentContractViewModel.LoyaltyColor;
            }
        }

        private void RefreshSubsidies()
        {
            // Clear existing items
            foreach (var item in subsidyItems)
            {
                if (item != null)
                    Destroy(item);
            }
            subsidyItems.Clear();

            if (subsidyListContainer == null || subsidyItemPrefab == null)
                return;

            // Create subsidy items
            foreach (var subsidy in currentContractViewModel.Subsidies)
            {
                var item = Instantiate(subsidyItemPrefab, subsidyListContainer);
                var text = item.GetComponent<TextMeshProUGUI>();
                if (text != null)
                    text.text = subsidy;
                subsidyItems.Add(item);
            }

            // Update monthly gold subsidy
            if (monthlyGoldSubsidyText != null)
                monthlyGoldSubsidyText.text = $"Monthly Gold: {currentContractViewModel.MonthlyGoldSubsidyDisplay}";
        }

        private Color GetLoyaltyColor(float loyalty)
        {
            if (loyalty >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (loyalty >= 60f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (loyalty >= 40f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (loyalty >= 20f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        // ========== EVENT HANDLERS ==========

        private void OnContractViewModelUpdated(int contractID)
        {
            if (contractID == selectedContractID)
            {
                RefreshContractDetails();
            }
            // Also refresh the list in case any contract changed
            RefreshContractList();
        }

        private void OnContractCreated(Contract contract)
        {
            if (contract.ParentRealmID == targetRealmID || contract.SubjectRealmID == targetRealmID)
            {
                RefreshContractList();
            }
        }

        private void OnContractTerminated(Contract contract)
        {
            if (contract.ContractID == selectedContractID)
            {
                // Hide details panel
                if (detailsPanel != null)
                    detailsPanel.SetActive(false);
                selectedContractID = -1;
                currentContractViewModel = null;
            }
            RefreshContractList();
        }

        private void OnLoyaltyChanged(TWK.Government.Contract contract, float newLoyalty)
        {
            if (contract.ContractID == selectedContractID)
            {
                RefreshLoyalty();
            }
        }

        private void OnFilterChanged(int index)
        {
            if (filterDropdown != null)
            {
                currentFilter = filterDropdown.options[index].text;
                RefreshContractList();
            }
        }

        private void OnContractSelected(int contractID)
        {
            selectedContractID = contractID;

            // Get or create ViewModel
            if (ViewModelService.Instance != null)
            {
                currentContractViewModel = ViewModelService.Instance.GetContractViewModel(contractID);
            }
            else
            {
                // Fallback: create ViewModel directly
                var contract = ContractManager.Instance?.GetContract(contractID);
                if (contract != null)
                {
                    currentContractViewModel = new ContractViewModel(contract);
                }
            }

            RefreshContractDetails();

            Debug.Log($"[ContractUIController] Selected contract {contractID}");
        }

        // ========== BUTTON HANDLERS ==========

        private void OnCreateContractClicked()
        {
            Debug.Log($"[ContractUIController] Create contract button clicked");
            // TODO: Open contract creation panel
        }

        private void OnModifyTermsClicked()
        {
            if (selectedContractID == -1)
                return;

            Debug.Log($"[ContractUIController] Modify terms button clicked for contract {selectedContractID}");
            // TODO: Open contract modification panel
        }

        private void OnTerminateContractClicked()
        {
            if (selectedContractID == -1)
                return;

            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.TerminateContract(selectedContractID);
                Debug.Log($"[ContractUIController] Terminated contract {selectedContractID}");
            }

            // Hide details panel
            if (detailsPanel != null)
                detailsPanel.SetActive(false);
            selectedContractID = -1;
            currentContractViewModel = null;
        }

        // ========== PUBLIC METHODS ==========

        /// <summary>
        /// Change the target realm being displayed.
        /// </summary>
        public void SetTargetRealm(int realmID)
        {
            targetRealmID = realmID;

            // Hide details panel
            if (detailsPanel != null)
                detailsPanel.SetActive(false);
            selectedContractID = -1;
            currentContractViewModel = null;

            RefreshUI();
        }

        /// <summary>
        /// Force an immediate refresh (useful for debugging).
        /// </summary>
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            RefreshUI();
            Debug.Log("[ContractUIController] Force refresh completed");
        }
    }
}
