using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Religion;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for the religion data UI.
    /// Allows viewing detailed information about religions including deities, tenets, identity, etc.
    /// </summary>
    public class ReligionDataUIController : MonoBehaviour
    {
        // ========== REFERENCES ==========
        [Header("Religion Selection")]
        [SerializeField] private TMP_Dropdown religionDropdown;
        [SerializeField] private TextMeshProUGUI religionNameText;
        [SerializeField] private TextMeshProUGUI religionDescriptionText;

        [Header("Identity Info")]
        [SerializeField] private TextMeshProUGUI religionTypeText;
        [SerializeField] private TextMeshProUGUI traditionText;
        [SerializeField] private TextMeshProUGUI centralizationText;
        [SerializeField] private TextMeshProUGUI evangelismText;
        [SerializeField] private TextMeshProUGUI syncretismText;
        [SerializeField] private TextMeshProUGUI headOfFaithText;
        [SerializeField] private TextMeshProUGUI fervorInfoText;

        [Header("Deities Display")]
        [SerializeField] private Transform deitiesContainer;
        [SerializeField] private GameObject deityItemPrefab;
        [SerializeField] private TextMeshProUGUI deitiesHeaderText;

        [Header("Tenets Display")]
        [SerializeField] private Transform tenetsContainer;
        [SerializeField] private GameObject tenetItemPrefab;
        [SerializeField] private TextMeshProUGUI tenetsHeaderText;

        [Header("Holy Lands Display")]
        [SerializeField] private TextMeshProUGUI holyLandsText;

        [Header("Festivals Display")]
        [SerializeField] private TextMeshProUGUI festivalsText;

        [Header("Rituals Display")]
        [SerializeField] private TextMeshProUGUI ritualsText;

        // ========== STATE ==========
        private ReligionData currentReligion;
        private List<GameObject> currentDeityItems = new List<GameObject>();
        private List<GameObject> currentTenetItems = new List<GameObject>();
        private bool isInitialized = false;

        // ========== INITIALIZATION ==========

        private void Start()
        {
            SetupReligionDropdown();

            // Subscribe to ViewModelService events
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnReligionViewModelUpdated += OnReligionViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated += OnViewModelsUpdated;
            }

            // Also keep the ReligionManager event for when new religions are registered
            if (ReligionManager.Instance != null)
            {
                ReligionManager.Instance.newReligionRegistered += RefreshReligionDropdown;
            }

            // Mark as initialized and do the first refresh
            isInitialized = true;
            RefreshReligionDropdown();
        }

        private void OnDestroy()
        {
            // Unsubscribe from ViewModelService events
            if (ViewModelService.Instance != null)
            {
                ViewModelService.Instance.OnReligionViewModelUpdated -= OnReligionViewModelUpdated;
                ViewModelService.Instance.OnViewModelsUpdated -= OnViewModelsUpdated;
            }

            // Unsubscribe from ReligionManager events
            if (ReligionManager.Instance != null)
            {
                ReligionManager.Instance.newReligionRegistered -= RefreshReligionDropdown;
            }
        }

        private void OnReligionViewModelUpdated(int updatedReligionId)
        {
            // Refresh the dropdown when a religion ViewModel is updated
            RefreshReligionDropdown();
        }

        private void OnViewModelsUpdated()
        {
            // Refresh the dropdown when all ViewModels are updated
            RefreshReligionDropdown();
        }

        private void OnEnable()
        {
            // Only refresh if we've already initialized (prevents early OnEnable calls before Start)
            if (isInitialized)
            {
                RefreshReligionDropdown();
            }
        }

        private void SetupReligionDropdown()
        {
            religionDropdown?.onValueChanged.AddListener(OnReligionChanged);
        }

        /// <summary>
        /// Refresh the religion dropdown options from ViewModelService.
        /// Called automatically on OnEnable, or can be called manually/via button.
        /// </summary>
        public void RefreshReligionDropdown()
        {
            if (religionDropdown == null)
                return;

            // Silently return if ViewModelService isn't ready yet (can happen during early initialization)
            if (ViewModelService.Instance == null)
                return;

            // Store current selection
            int currentIndex = religionDropdown.value;
            string currentlySelectedName = null;

            if (currentIndex >= 0 && currentIndex < religionDropdown.options.Count)
            {
                currentlySelectedName = religionDropdown.options[currentIndex].text;
            }

            // Clear and rebuild dropdown
            religionDropdown.ClearOptions();

            // Get religion ViewModels from ViewModelService
            var allReligionViewModels = ViewModelService.Instance.GetAllReligionViewModels().ToList();
            var options = allReligionViewModels.Select(vm => new TMP_Dropdown.OptionData(vm.ReligionName)).ToList();

            religionDropdown.AddOptions(options);

            // Determine which index to select and display
            int indexToSelect = 0;

            // Restore selection if the religion still exists
            if (!string.IsNullOrEmpty(currentlySelectedName))
            {
                int newIndex = allReligionViewModels.FindIndex(vm => vm.ReligionName == currentlySelectedName);
                if (newIndex >= 0)
                {
                    indexToSelect = newIndex;
                }
            }

            // Set the value and trigger display update
            if (allReligionViewModels.Count > 0)
            {
                religionDropdown.SetValueWithoutNotify(indexToSelect);
                OnReligionChanged(indexToSelect); // Manually trigger to update display
            }
        }

        // ========== RELIGION SELECTION ==========

        private void OnReligionChanged(int index)
        {
            if (ReligionManager.Instance == null)
            {
                Debug.LogWarning("[ReligionDataUIController] ReligionManager instance is null in OnReligionChanged");
                return;
            }

            var religions = ReligionManager.Instance.GetAllReligions();
            if (index < 0 || index >= religions.Count)
            {
                Debug.LogWarning($"[ReligionDataUIController] Invalid index {index} (total religions: {religions.Count})");
                return;
            }

            currentReligion = religions[index];
            Debug.Log($"[ReligionDataUIController] Selected religion: {currentReligion.ReligionName}");
            RefreshReligionDisplay();
        }

        private void RefreshReligionDisplay()
        {
            if (currentReligion == null)
            {
                Debug.LogWarning("[ReligionDataUIController] currentReligion is null in RefreshReligionDisplay");
                return;
            }

            Debug.Log($"[ReligionDataUIController] Refreshing display for {currentReligion.ReligionName}");
            RefreshBasicInfo();
            RefreshIdentityInfo();
            RefreshDeities();
            RefreshTenets();
            RefreshHolyLands();
            RefreshFestivals();
            RefreshRituals();
        }

        // ========== BASIC INFO ==========

        private void RefreshBasicInfo()
        {
            if (religionNameText != null)
                religionNameText.text = currentReligion.ReligionName;
            else
                Debug.LogWarning("[ReligionDataUIController] religionNameText is null - not assigned in inspector?");

            if (religionDescriptionText != null)
                religionDescriptionText.text = currentReligion.Description;
            else
                Debug.LogWarning("[ReligionDataUIController] religionDescriptionText is null - not assigned in inspector?");
        }

        // ========== IDENTITY INFO ==========

        private void RefreshIdentityInfo()
        {
            if (religionTypeText != null)
                religionTypeText.text = $"<b>Type:</b> {currentReligion.ReligionType}";

            if (traditionText != null)
                traditionText.text = $"<b>Tradition:</b> {currentReligion.Tradition}";

            if (centralizationText != null)
                centralizationText.text = $"<b>Organization:</b> {currentReligion.Centralization}";

            if (evangelismText != null)
                evangelismText.text = $"<b>Evangelism:</b> {currentReligion.Evangelism}";

            if (syncretismText != null)
                syncretismText.text = $"<b>Syncretism:</b> {currentReligion.Syncretism}";

            // Head of Faith (if centralized)
            if (headOfFaithText != null)
            {
                if (currentReligion.IsCentralized())
                {
                    string headInfo = $"<b>Head of Faith:</b> {currentReligion.HeadOfFaithTitle}\n";

                    if (currentReligion.HeadPowers != HeadOfFaithPowers.None)
                    {
                        headInfo += "<b>Powers:</b>\n";

                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.Excommunication) != 0)
                            headInfo += "  • Excommunication\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.GrantDivorce) != 0)
                            headInfo += "  • Grant Divorce\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.GrantClaims) != 0)
                            headInfo += "  • Grant Claims\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.CallCrusade) != 0)
                            headInfo += "  • Call Crusade\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.InvestClergy) != 0)
                            headInfo += "  • Invest Clergy\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.TaxClergy) != 0)
                            headInfo += "  • Tax Clergy\n";
                        if ((currentReligion.HeadPowers & HeadOfFaithPowers.GrantIndulgences) != 0)
                            headInfo += "  • Grant Indulgences\n";
                    }

                    headOfFaithText.text = headInfo;
                }
                else
                {
                    headOfFaithText.text = "<b>Head of Faith:</b> Decentralized (No central authority)";
                }
            }

            // Fervor info
            if (fervorInfoText != null)
            {
                string fervorInfo = $"<b>Fervor:</b>\n";
                fervorInfo += $"  Base: {currentReligion.BaseFervor:F0}\n";
                fervorInfo += $"  Decay Rate: {currentReligion.FervorDecayRate:F1}/season\n";
                fervorInfo += $"  Conversion Resistance: {currentReligion.ConversionResistance:F0}%\n";
                fervorInfo += $"  Conversion Speed: {currentReligion.ConversionSpeed:F1}x";

                fervorInfoText.text = fervorInfo;
            }
        }

        // ========== DEITIES ==========

        private void RefreshDeities()
        {
            // Clear existing deity items
            foreach (var item in currentDeityItems)
            {
                if (item != null)
                    Destroy(item);
            }
            currentDeityItems.Clear();

            if (deitiesHeaderText != null)
            {
                int deityCount = currentReligion.Deities?.Count ?? 0;
                string pantheonType = deityCount == 0 ? "No Deities" : deityCount == 1 ? "Monotheistic" : "Polytheistic";
                deitiesHeaderText.text = $"<b>Deities ({deityCount} - {pantheonType}):</b>";
            }

            if (currentReligion.Deities == null || currentReligion.Deities.Count == 0)
                return;

            // Create deity items using the DeityUIItem component
            if (deityItemPrefab != null && deitiesContainer != null)
            {
                foreach (var deity in currentReligion.Deities)
                {
                    if (deity == null) continue;

                    var deityItemObj = Instantiate(deityItemPrefab, deitiesContainer);
                    currentDeityItems.Add(deityItemObj);

                    // Initialize the deity item with data
                    var deityUIItem = deityItemObj.GetComponent<DeityUIItem>();
                    if (deityUIItem != null)
                    {
                        deityUIItem.Initialize(deity);
                    }
                    else
                    {
                        Debug.LogWarning("[ReligionDataUIController] DeityUIItem component not found on prefab. Add DeityUIItem script to the deity prefab.");
                    }
                }
            }
        }

        // ========== TENETS ==========

        private void RefreshTenets()
        {
            // Clear existing tenet items
            foreach (var item in currentTenetItems)
            {
                if (item != null)
                    Destroy(item);
            }
            currentTenetItems.Clear();

            if (tenetsHeaderText != null)
            {
                int tenetCount = currentReligion.Tenets?.Count ?? 0;
                tenetsHeaderText.text = $"<b>Tenets ({tenetCount}):</b>";
            }

            if (currentReligion.Tenets == null || currentReligion.Tenets.Count == 0)
                return;

            // Create tenet items using the TenetUIItem component
            if (tenetItemPrefab != null && tenetsContainer != null)
            {
                foreach (var tenet in currentReligion.Tenets)
                {
                    if (tenet == null) continue;

                    var tenetItemObj = Instantiate(tenetItemPrefab, tenetsContainer);
                    currentTenetItems.Add(tenetItemObj);

                    // Initialize the tenet item with data
                    var tenetUIItem = tenetItemObj.GetComponent<TenetUIItem>();
                    if (tenetUIItem != null)
                    {
                        tenetUIItem.Initialize(tenet);
                    }
                    else
                    {
                        Debug.LogWarning("[ReligionDataUIController] TenetUIItem component not found on prefab. Add TenetUIItem script to the tenet prefab.");
                    }
                }
            }
        }

        // ========== HOLY LANDS ==========

        private void RefreshHolyLands()
        {
            if (holyLandsText == null)
                return;

            var holyLands = currentReligion.GetAllHolyLands();

            if (holyLands == null || holyLands.Count == 0)
            {
                holyLandsText.text = "<b>Holy Lands:</b>\n<i>None</i>";
                return;
            }

            string text = "<b>Holy Lands:</b>\n\n";
            foreach (var holyLand in holyLands)
            {
                if (holyLand == null) continue;

                text += $"<b>{holyLand.Name}</b> ({holyLand.Importance})\n";
                if (!string.IsNullOrEmpty(holyLand.Significance))
                    text += $"  {holyLand.Significance}\n";
                text += $"  Fervor Bonus: +{holyLand.FervorBonus:F0}\n";
                text += $"  Piety/Season: +{holyLand.PietyPerSeason}\n\n";
            }

            holyLandsText.text = text;
        }

        // ========== FESTIVALS ==========

        private void RefreshFestivals()
        {
            if (festivalsText == null)
                return;

            var festivals = currentReligion.GetAllFestivals();

            if (festivals == null || festivals.Count == 0)
            {
                festivalsText.text = "<b>Festivals:</b>\n<i>None</i>";
                return;
            }

            string text = "<b>Festivals:</b>\n\n";
            foreach (var festival in festivals)
            {
                if (festival == null) continue;

                text += $"<b>{festival.Name}</b> ({festival.Frequency})\n";
                if (!string.IsNullOrEmpty(festival.Description))
                    text += $"  {festival.Description}\n";
                text += $"  Season: {GetSeasonName(festival.Season)}\n";
                text += $"  Happiness: +{festival.HappinessBonus:F0}, Fervor: +{festival.FervorBonus:F0}\n";
                text += $"  Cost: {festival.GoldCost} gold\n\n";
            }

            festivalsText.text = text;
        }

        // ========== RITUALS ==========

        private void RefreshRituals()
        {
            if (ritualsText == null)
                return;

            var rituals = currentReligion.GetAllRituals();

            if (rituals == null || rituals.Count == 0)
            {
                ritualsText.text = "<b>Rituals:</b>\n<i>None</i>";
                return;
            }

            string text = "<b>Rituals:</b>\n\n";
            foreach (var ritual in rituals)
            {
                if (ritual == null) continue;

                text += $"<b>{ritual.Name}</b>\n";
                if (!string.IsNullOrEmpty(ritual.Description))
                    text += $"  {ritual.Description}\n";

                if (ritual.Effect != null)
                {
                    text += "  <i>Effects:</i>";
                    if (ritual.Effect.FervorGain > 0)
                        text += $" Fervor +{ritual.Effect.FervorGain:F0},";
                    if (ritual.Effect.HappinessGain > 0)
                        text += $" Happiness +{ritual.Effect.HappinessGain:F0},";
                    if (ritual.Effect.StabilityGain > 0)
                        text += $" Stability +{ritual.Effect.StabilityGain:F0},";
                    if (ritual.Effect.PrestigeGain > 0)
                        text += $" Prestige +{ritual.Effect.PrestigeGain}";
                    text += "\n";
                }

                text += $"  Cost: {ritual.PietyCost} piety\n";
                text += $"  Cooldown: {ritual.Cooldown} days\n\n";
            }

            ritualsText.text = text;
        }

        // ========== HELPERS ==========

        private string GetSeasonName(int season)
        {
            return season switch
            {
                0 => "Spring",
                1 => "Summer",
                2 => "Fall",
                3 => "Winter",
                _ => "Unknown"
            };
        }

        // ========== PUBLIC API ==========

        /// <summary>
        /// Refresh the currently displayed religion.
        /// Call this if religion data is modified at runtime.
        /// </summary>
        public void RefreshDisplay()
        {
            RefreshReligionDisplay();
        }

        /// <summary>
        /// Set the displayed religion by index in the dropdown.
        /// </summary>
        public void SetReligion(int index)
        {
            if (religionDropdown != null)
            {
                religionDropdown.value = index;
            }
        }

        /// <summary>
        /// Set the displayed religion by ReligionData reference.
        /// </summary>
        public void SetReligion(ReligionData religion)
        {
            if (religion == null || ReligionManager.Instance == null)
                return;

            var religions = ReligionManager.Instance.GetAllReligions();
            int index = religions.IndexOf(religion);

            if (index >= 0)
            {
                SetReligion(index);
            }
        }
    }
}
