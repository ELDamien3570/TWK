using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.UI.ViewModels;
using TWK.Government;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for government UI.
    /// Displays government information using GovernmentViewModel.
    /// </summary>
    public class GovernmentUIController : MonoBehaviour
    {
        [Header("Realm Reference")]
        [SerializeField] private int targetRealmID = 1;
        [Tooltip("If set, will override targetRealmID")]
        [SerializeField] private TWK.Realms.Realm targetRealm;

        [Header("Main Info")]
        [SerializeField] private TextMeshProUGUI governmentNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image governmentIcon;
        [SerializeField] private TextMeshProUGUI regimeSummaryText;
        [SerializeField] private TextMeshProUGUI lawsSummaryText;

        [Header("Regime Details")]
        [SerializeField] private TextMeshProUGUI regimeFormText;
        [SerializeField] private TextMeshProUGUI stateStructureText;
        [SerializeField] private TextMeshProUGUI successionLawText;
        [SerializeField] private TextMeshProUGUI administrationText;
        [SerializeField] private TextMeshProUGUI mobilityText;

        [Header("Laws")]
        [SerializeField] private TextMeshProUGUI militaryServiceText;
        [SerializeField] private TextMeshProUGUI taxationText;
        [SerializeField] private TextMeshProUGUI tradeText;
        [SerializeField] private TextMeshProUGUI justiceText;

        [Header("Stats")]
        [SerializeField] private Slider legitimacySlider;
        [SerializeField] private TextMeshProUGUI legitimacyText;
        [SerializeField] private Image legitimacyFillImage;

        [SerializeField] private Slider capacitySlider;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private Image capacityFillImage;

        [SerializeField] private Slider revoltRiskSlider;
        [SerializeField] private TextMeshProUGUI revoltRiskText;
        [SerializeField] private Image revoltRiskFillImage;

        [Header("Institutions")]
        [SerializeField] private Transform institutionsContainer;
        [SerializeField] private GameObject institutionItemPrefab;
        [SerializeField] private TextMeshProUGUI institutionCountText;

        [Header("Modifiers")]
        [SerializeField] private TWK.UI.Common.ModifierDisplayPanel modifierDisplayPanel;

        [Header("Actions")]
        [SerializeField] private Button reformButton;
        [SerializeField] private Button enactEdictButton;
        [SerializeField] private Button manageBureaucracyButton;

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 1f;

        private GovernmentViewModel viewModel;
        private List<GameObject> institutionItems = new List<GameObject>();
        private float timeSinceLastRefresh = 0f;

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
                viewModel = ViewModelService.Instance.GetGovernmentViewModel(targetRealmID);
            }
            else
            {
                viewModel = new GovernmentViewModel(targetRealmID);
            }

            // Setup button listeners
            SetupButtons();

            // Subscribe to events
            SubscribeToEvents();

            // Initial refresh
            RefreshUI();
        }

        private void SetupButtons()
        {
            reformButton?.onClick.RemoveAllListeners();
            enactEdictButton?.onClick.RemoveAllListeners();
            manageBureaucracyButton?.onClick.RemoveAllListeners();

            reformButton?.onClick.AddListener(OnReformButtonClicked);
            enactEdictButton?.onClick.AddListener(OnEnactEdictClicked);
            manageBureaucracyButton?.onClick.AddListener(OnManageBureaucracyClicked);
        }

        private void SubscribeToEvents()
        {
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnGovernmentViewModelUpdated += OnGovernmentViewModelUpdated;
            }

            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnGovernmentChanged += OnGovernmentChanged;
                GovernmentManager.Instance.OnLegitimacyChanged += OnLegitimacyChanged;
                GovernmentManager.Instance.OnCapacityChanged += OnCapacityChanged;
                GovernmentManager.Instance.OnEdictEnacted += OnEdictEnacted;
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
                ViewModelService.Instance.OnGovernmentViewModelUpdated -= OnGovernmentViewModelUpdated;
            }

            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnGovernmentChanged -= OnGovernmentChanged;
                GovernmentManager.Instance.OnLegitimacyChanged -= OnLegitimacyChanged;
                GovernmentManager.Instance.OnCapacityChanged -= OnCapacityChanged;
                GovernmentManager.Instance.OnEdictEnacted -= OnEdictEnacted;
            }

            // Clean up listeners
            reformButton?.onClick.RemoveAllListeners();
            enactEdictButton?.onClick.RemoveAllListeners();
            manageBureaucracyButton?.onClick.RemoveAllListeners();
        }

        public void RefreshUI()
        {
            if (viewModel == null) return;

            viewModel.Refresh();

            RefreshMainInfo();
            RefreshRegimeDetails();
            RefreshLaws();
            RefreshStats();
            RefreshInstitutions();
            RefreshModifiers();
        }

        private void RefreshMainInfo()
        {
            if (governmentNameText != null)
                governmentNameText.text = viewModel.GovernmentName;

            if (descriptionText != null)
                descriptionText.text = viewModel.Description ?? "";

            if (governmentIcon != null && viewModel.Icon != null)
            {
                governmentIcon.sprite = viewModel.Icon;
                governmentIcon.color = Color.white;
            }

            if (regimeSummaryText != null)
                regimeSummaryText.text = viewModel.GetRegimeSummary();

            if (lawsSummaryText != null)
                lawsSummaryText.text = viewModel.GetLawsSummary();
        }

        private void RefreshRegimeDetails()
        {
            if (regimeFormText != null)
                regimeFormText.text = $"Regime: {viewModel.RegimeFormDisplay}";

            if (stateStructureText != null)
                stateStructureText.text = $"Structure: {viewModel.StateStructureDisplay}";

            if (successionLawText != null)
                successionLawText.text = $"Succession: {viewModel.SuccessionLawDisplay}";

            if (administrationText != null)
                administrationText.text = $"Administration: {viewModel.AdministrationDisplay}";

            if (mobilityText != null)
                mobilityText.text = $"Mobility: {viewModel.MobilityDisplay}";
        }

        private void RefreshLaws()
        {
            if (militaryServiceText != null)
                militaryServiceText.text = $"Military: {viewModel.MilitaryServiceDisplay}";

            if (taxationText != null)
                taxationText.text = $"Taxation: {viewModel.TaxationDisplay}";

            if (tradeText != null)
                tradeText.text = $"Trade: {viewModel.TradeDisplay}";

            if (justiceText != null)
                justiceText.text = $"Justice: {viewModel.JusticeDisplay}";
        }

        private void RefreshStats()
        {
            // Legitimacy
            if (legitimacySlider != null)
            {
                legitimacySlider.value = viewModel.Legitimacy / 100f;
            }

            if (legitimacyText != null)
            {
                legitimacyText.text = $"Legitimacy: {viewModel.LegitimacyDisplay} ({viewModel.LegitimacyStatus})";
            }

            if (legitimacyFillImage != null)
            {
                legitimacyFillImage.color = viewModel.LegitimacyColor;
            }

            // Capacity
            if (capacitySlider != null)
            {
                capacitySlider.value = viewModel.Capacity / 100f;
            }

            if (capacityText != null)
            {
                capacityText.text = $"Capacity: {viewModel.CapacityDisplay} ({viewModel.CapacityStatus})";
            }

            if (capacityFillImage != null)
            {
                capacityFillImage.color = viewModel.CapacityColor;
            }

            // Revolt Risk
            if (revoltRiskSlider != null)
            {
                revoltRiskSlider.value = viewModel.RevoltRisk / 100f;
            }

            if (revoltRiskText != null)
            {
                revoltRiskText.text = $"Revolt Risk: {viewModel.RevoltRiskDisplay} ({viewModel.RevoltRiskStatus})";
            }

            if (revoltRiskFillImage != null)
            {
                revoltRiskFillImage.color = viewModel.RevoltRiskColor;
            }
        }

        private void RefreshInstitutions()
        {
            // Clear existing items
            foreach (var item in institutionItems)
            {
                if (item != null)
                    Destroy(item);
            }
            institutionItems.Clear();

            if (institutionsContainer == null || institutionItemPrefab == null)
                return;

            // Create new items
            foreach (var institution in viewModel.Institutions)
            {
                var item = Instantiate(institutionItemPrefab, institutionsContainer);

                // Set institution name
                var nameText = item.transform.Find("InstitutionName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = institution.Name;

                // Set description
                var descText = item.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                    descText.text = institution.Description ?? "";

                // Set category
                var categoryText = item.transform.Find("Category")?.GetComponent<TextMeshProUGUI>();
                if (categoryText != null)
                    categoryText.text = institution.Category;

                // Set icon
                var icon = item.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && institution.Icon != null)
                {
                    icon.sprite = institution.Icon;
                    icon.color = Color.white;
                }

                institutionItems.Add(item);
            }

            // Update count
            if (institutionCountText != null)
            {
                institutionCountText.text = $"Institutions: {viewModel.InstitutionCount}/3";
            }
        }

        private void RefreshModifiers()
        {
            if (modifierDisplayPanel == null)
                return;

            // Get all active modifiers for this government
            var allModifiers = new List<TWK.Modifiers.Modifier>(viewModel.ActiveModifiers);

            // Tag modifiers with their source type for better display
            foreach (var modifier in allModifiers)
            {
                // The viewModel already has source info, use it if available
                if (string.IsNullOrEmpty(modifier.SourceType))
                {
                    modifier.SourceType = modifier.Source;
                }
            }

            // Display modifiers grouped by category
            // Separate modifiers by source for better organization
            var edictModifiers = allModifiers.FindAll(m => m.Source?.Contains("Edict") == true || m.SourceType?.Contains("Edict") == true);
            var buildingModifiers = allModifiers.FindAll(m => m.Source?.Contains("Building") == true || m.SourceType?.Contains("Building") == true);
            var cultureModifiers = allModifiers.FindAll(m => m.Source?.Contains("Culture") == true || m.SourceType?.Contains("Culture") == true);
            var otherModifiers = allModifiers.FindAll(m =>
                !edictModifiers.Contains(m) &&
                !buildingModifiers.Contains(m) &&
                !cultureModifiers.Contains(m));

            modifierDisplayPanel.DisplayModifiersByCategory(
                cultureModifiers: cultureModifiers.Count > 0 ? cultureModifiers : null,
                religionModifiers: null, // Religion modifiers could be added later
                eventModifiers: edictModifiers.Count > 0 ? edictModifiers : null,
                buildingModifiers: buildingModifiers.Count > 0 ? buildingModifiers : otherModifiers.Count > 0 ? otherModifiers : buildingModifiers
            );
        }

        // ========== EVENT HANDLERS ==========

        private void OnGovernmentViewModelUpdated(int realmID)
        {
            if (realmID == targetRealmID)
            {
                RefreshUI();
            }
        }

        private void OnGovernmentChanged(int realmID, GovernmentData newGovernment)
        {
            if (realmID == targetRealmID)
            {
                RefreshUI();
            }
        }

        private void OnLegitimacyChanged(int realmID, float newLegitimacy)
        {
            if (realmID == targetRealmID)
            {
                RefreshStats();
            }
        }

        private void OnCapacityChanged(int realmID, float newCapacity)
        {
            if (realmID == targetRealmID)
            {
                RefreshStats();
            }
        }

        private void OnEdictEnacted(int realmID, Edict edict)
        {
            if (realmID == targetRealmID)
            {
                RefreshModifiers();
            }
        }

        // ========== BUTTON HANDLERS ==========
        // DESIGN NOTE: Legitimacy, Capacity, and Revolt Risk are READ-ONLY stats.
        // They are NOT directly purchasable - they change through gameplay:
        // - Buildings provide modifiers
        // - Edicts cost gold and affect stats/loyalty
        // - Culture/religion alignment affects legitimacy
        // - Contracts and policies affect capacity
        // - Reforms change government structure
        // The UI displays these stats for information only, no direct modification!

        private void OnReformButtonClicked()
        {
            Debug.Log($"[GovernmentUIController] Reform button clicked for realm {targetRealmID}");
            // TODO: Open reform selection panel with dropdown selectors for:
            // - RegimeForm (Autocratic, Pluralist, Chiefdom, Confederation)
            // - StateStructure (Territorial, Tribal, CityState)
            // - SuccessionLaw (Hereditary, Election, Appointment, etc.)
            // - Administration, Mobility, etc.
            // All as dropdowns, not text fields
        }

        private void OnEnactEdictClicked()
        {
            Debug.Log($"[GovernmentUIController] Enact edict button clicked for realm {targetRealmID}");
            // TODO: Open edict selection panel
            // Edicts modify stats indirectly through modifiers and loyalty effects
        }

        private void OnManageBureaucracyClicked()
        {
            Debug.Log($"[GovernmentUIController] Manage bureaucracy button clicked for realm {targetRealmID}");
            // TODO: Open bureaucracy management panel
            // Offices increase capacity through efficiency, not direct purchase
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
                viewModel = ViewModelService.Instance.GetGovernmentViewModel(targetRealmID);
            }
            else
            {
                viewModel = new GovernmentViewModel(targetRealmID);
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
            Debug.Log("[GovernmentUIController] Force refresh completed");
        }
    }
}
