using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Cultures;

namespace TWK.UI
{
    /// <summary>
    /// Main controller for the culture tech tree UI.
    /// Allows viewing and unlocking tech nodes for different cultures and tree types.
    /// </summary>
    public class CultureTechTreeUIController : MonoBehaviour
    {
        // ========== REFERENCES ==========
        [Header("Culture Selection")]
        [SerializeField] private TMP_Dropdown cultureDropdown;
        [SerializeField] private TextMeshProUGUI cultureNameText;
        [SerializeField] private TextMeshProUGUI cultureDescriptionText;

        [Header("Tree Type Tabs")]
        [SerializeField] private Button economicsTabButton;
        [SerializeField] private Button warfareTabButton;
        [SerializeField] private Button religionTabButton;
        [SerializeField] private Button politicsTabButton;
        [SerializeField] private Button scienceTabButton;

        [Header("Tree Info")]
        [SerializeField] private TextMeshProUGUI treeNameText;
        [SerializeField] private TextMeshProUGUI availableXPText;
        [SerializeField] private TextMeshProUGUI totalXPText;
        [SerializeField] private TextMeshProUGUI ownerRealmText;

        [Header("Node Display")]
        [SerializeField] private RectTransform nodeListContainer;
        [SerializeField] private GameObject nodeItemPrefab;

        [Header("Connection Lines")]
        [SerializeField] private Transform connectionLineContainer;
        [SerializeField] private GameObject connectionLinePrefab;

        [Header("Selected Node Details")]
        [SerializeField] private GameObject nodeDetailsPanel;
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private TextMeshProUGUI nodeDescriptionText;
        [SerializeField] private TextMeshProUGUI nodeCostText;
        [SerializeField] private TextMeshProUGUI nodePrerequisitesText;
        [SerializeField] private TextMeshProUGUI nodeUnlocksText;
        [SerializeField] private TextMeshProUGUI nodeModifiersText;
        [SerializeField] private Button unlockNodeButton;
        [SerializeField] private TextMeshProUGUI unlockButtonText;

        [Header("Settings")]
        [SerializeField] private int playerRealmID = 0; // Set this to the player's realm ID

        // ========== STATE ==========
        private CultureData currentCulture;
        private TreeType currentTreeType = TreeType.Economics;
        private TechNode selectedNode;
        private List<TechNodeUIItem> nodeUIItems = new List<TechNodeUIItem>();
        private List<TechTreeConnectionLine> connectionLines = new List<TechTreeConnectionLine>();
        private Dictionary<TechNode, TechNodeLayoutInfo> currentLayout = new Dictionary<TechNode, TechNodeLayoutInfo>();

        // ========== INITIALIZATION ==========

        private void Start()
        {
            SetupCultureDropdown();
            SetupTabButtons();
            SetupUnlockButton();
            SubscribeToEvents();

            if (cultureDropdown.options.Count > 0)
            {
                OnCultureChanged(0);
            }

            nodeDetailsPanel?.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            // Subscribe to XP changes to auto-refresh the UI
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCultureXPAdded += HandleCultureXPAdded;
            }
        }

        private void HandleCultureXPAdded(int cultureID, TreeType treeType, float xpAmount)
        {
            // Only refresh if we're currently viewing this culture and tree
            if (currentCulture != null &&
                currentCulture.GetCultureID() == cultureID &&
                currentTreeType == treeType)
            {
                RefreshTreeInfo();

                // Also refresh node details if a node is selected (to update unlock button state)
                if (selectedNode != null)
                {
                    RefreshUnlockButton();
                }
            }
        }

        private void SetupCultureDropdown()
        {
            cultureDropdown?.ClearOptions();

            var cultures = CultureManager.Instance.GetAllCultures();
            var options = cultures.Select(c => new TMP_Dropdown.OptionData(c.CultureName)).ToList();

            cultureDropdown?.AddOptions(options);
            cultureDropdown?.onValueChanged.AddListener(OnCultureChanged);
        }

        private void SetupTabButtons()
        {
            economicsTabButton?.onClick.AddListener(() => ShowTreeType(TreeType.Economics));
            warfareTabButton?.onClick.AddListener(() => ShowTreeType(TreeType.Warfare));
            religionTabButton?.onClick.AddListener(() => ShowTreeType(TreeType.Religion));
            politicsTabButton?.onClick.AddListener(() => ShowTreeType(TreeType.Politics));
            scienceTabButton?.onClick.AddListener(() => ShowTreeType(TreeType.Science));
        }

        private void SetupUnlockButton()
        {
            unlockNodeButton?.onClick.AddListener(OnUnlockNodeClicked);
        }

        // ========== CULTURE SELECTION ==========

        private void OnCultureChanged(int index)
        {
            var cultures = CultureManager.Instance.GetAllCultures();
            if (index < 0 || index >= cultures.Count)
                return;

            currentCulture = cultures[index];
            RefreshCultureInfo();
            ShowTreeType(currentTreeType);
        }

        private void RefreshCultureInfo()
        {
            if (currentCulture == null)
                return;

            cultureNameText.text = currentCulture.CultureName;
            cultureDescriptionText.text = currentCulture.Description;
        }

        // ========== TREE TYPE TABS ==========

        private void ShowTreeType(TreeType treeType)
        {
            currentTreeType = treeType;
            RefreshTreeInfo();
            RefreshNodeList();
        }

        private void RefreshTreeInfo()
        {
            if (currentCulture == null)
                return;

            var tree = currentCulture.GetTechTree(currentTreeType);
            if (tree == null)
            {
                Debug.LogWarning($"[CultureTechTreeUI] No tech tree found for {currentTreeType}");
                return;
            }

            treeNameText.text = $"{currentTreeType} Tech Tree";
            availableXPText.text = $"Available XP: {tree.AccumulatedXP:F0}";
            totalXPText.text = $"Total XP Earned: {tree.TotalXPEarned:F0}";

            if (tree.OwnerRealmID >= 0)
            {
                ownerRealmText.text = $"Controlled by Realm {tree.OwnerRealmID}";
            }
            else
            {
                ownerRealmText.text = "No Owner";
            }
        }

        // ========== NODE LIST ==========

        private void RefreshNodeList()
        {
            // Clear existing node UI items
            foreach (var item in nodeUIItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            nodeUIItems.Clear();

            // Clear existing connection lines
            foreach (var line in connectionLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            connectionLines.Clear();

            currentLayout.Clear();

            if (currentCulture == null)
                return;

            var tree = currentCulture.GetTechTree(currentTreeType);
            if (tree == null || tree.AllNodes.Count == 0)
            {
                Debug.Log($"[CultureTechTreeUI] No nodes in {currentTreeType} tree");
                return;
            }

            // Calculate layout for all nodes
            currentLayout = TechTreeLayoutCalculator.CalculateLayout(tree.AllNodes);

            // Set ScrollView content size based on layout
            if (nodeListContainer != null)
            {
                Vector2 contentSize = TechTreeLayoutCalculator.CalculateContentSize(currentLayout);
                nodeListContainer.sizeDelta = contentSize;
            }

            // Create connection lines first (so they appear behind nodes)
            CreateConnectionLines(tree);

            // Create and position node UI items
            foreach (var layoutInfo in currentLayout.Values)
            {
                if (layoutInfo.Node == null || nodeItemPrefab == null || nodeListContainer == null)
                    continue;

                var nodeObj = Instantiate(nodeItemPrefab, nodeListContainer);
                var nodeItem = nodeObj.GetComponent<TechNodeUIItem>();

                if (nodeItem != null)
                {
                    nodeItem.Initialize(layoutInfo.Node, tree, OnNodeSelected);
                    nodeUIItems.Add(nodeItem);

                    // Position the node
                    var rectTransform = nodeObj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = layoutInfo.Position;
                        rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    }

                    // Store references for connection line positioning
                    layoutInfo.UIObject = nodeObj;
                    layoutInfo.RectTransform = rectTransform;
                }
            }
        }

        /// <summary>
        /// Create connection lines between nodes based on prerequisites.
        /// </summary>
        private void CreateConnectionLines(CultureTechTree tree)
        {
            if (connectionLinePrefab == null)
            {
                Debug.LogWarning("[CultureTechTreeUI] Connection line prefab not assigned. Skipping line creation.");
                return;
            }

            Transform lineParent = connectionLineContainer != null ? connectionLineContainer : nodeListContainer;

            foreach (var layoutInfo in currentLayout.Values)
            {
                var targetNode = layoutInfo.Node;
                var targetPos = layoutInfo.Position;

                // Get all prerequisite nodes (sources for connection lines)
                var prerequisites = TechTreeLayoutCalculator.GetConnectionSources(targetNode);

                foreach (var sourceNode in prerequisites)
                {
                    if (!currentLayout.ContainsKey(sourceNode))
                        continue;

                    var sourcePos = currentLayout[sourceNode].Position;

                    // Create line GameObject
                    var lineObj = Instantiate(connectionLinePrefab, lineParent);
                    var line = lineObj.GetComponent<TechTreeConnectionLine>();

                    if (line == null)
                    {
                        line = lineObj.AddComponent<TechTreeConnectionLine>();
                    }

                    // Initialize the line
                    line.Initialize(sourcePos, targetPos, sourceNode, targetNode, tree);
                    connectionLines.Add(line);
                }
            }
        }

        // ========== NODE SELECTION ==========

        private void OnNodeSelected(TechNode node)
        {
            selectedNode = node;
            RefreshNodeDetails();
            nodeDetailsPanel?.SetActive(true);
        }

        private void RefreshNodeDetails()
        {
            if (selectedNode == null)
                return;

            nodeNameText.text = selectedNode.NodeName;
            nodeDescriptionText.text = selectedNode.Description;
            nodeCostText.text = $"Cost: {selectedNode.Cost} XP";

            // Prerequisites
            if (selectedNode.Prerequisites.Count > 0)
            {
                var prereqNames = selectedNode.Prerequisites.Select(p => p.NodeName).ToList();
                nodePrerequisitesText.text = $"Requires: {string.Join(", ", prereqNames)}";
            }
            else
            {
                nodePrerequisitesText.text = "No Prerequisites";
            }

            // Building unlocks
            if (selectedNode.UnlockedBuildings.Count > 0)
            {
                var buildingNames = selectedNode.UnlockedBuildings
                    .Where(b => b != null)
                    .Select(b => b.BuildingName)
                    .ToList();

                if (buildingNames.Count > 0)
                {
                    nodeUnlocksText.text = $"Unlocks:\n• {string.Join("\n• ", buildingNames)}";
                }
                else
                {
                    nodeUnlocksText.text = "No Building Unlocks";
                }
            }
            else
            {
                nodeUnlocksText.text = "No Building Unlocks";
            }

            // Modifiers
            if (selectedNode.Modifiers.Count > 0)
            {
                var modifierDescriptions = selectedNode.Modifiers.Select(m =>
                    $"• {m.Name}: {FormatModifierValue(m)}").ToList();
                nodeModifiersText.text = string.Join("\n", modifierDescriptions);
            }
            else
            {
                nodeModifiersText.text = "No Modifiers";
            }

            // Unlock button state
            RefreshUnlockButton();
        }

        private string FormatModifierValue(CultureModifier modifier)
        {
            if (modifier.ValueType == ModifierValueType.Percentage)
            {
                return $"+{modifier.BonusValue * 100:F0}%";
            }
            else
            {
                return $"+{modifier.BonusValue:F0}";
            }
        }

        private void RefreshUnlockButton()
        {
            if (selectedNode == null || currentCulture == null)
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = "Select a Node";
                return;
            }

            var tree = currentCulture.GetTechTree(currentTreeType);
            if (tree == null)
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = "Error";
                return;
            }

            // Check if already unlocked
            if (tree.IsNodeUnlocked(selectedNode))
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = "Already Unlocked";
                return;
            }

            // Check if prerequisites met
            if (!selectedNode.ArePrerequisitesMet())
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = "Prerequisites Not Met";
                return;
            }

            // Check if player controls this tree
            if (tree.OwnerRealmID != playerRealmID)
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = "You Don't Control This Tree";
                return;
            }

            // Check if enough XP
            if (!tree.CanAffordNode(selectedNode))
            {
                unlockNodeButton.interactable = false;
                unlockButtonText.text = $"Need {selectedNode.Cost} XP";
                return;
            }

            // All checks passed
            unlockNodeButton.interactable = true;
            unlockButtonText.text = $"Unlock ({selectedNode.Cost} XP)";
        }

        // ========== NODE UNLOCKING ==========

        private void OnUnlockNodeClicked()
        {
            if (selectedNode == null || currentCulture == null)
                return;

            bool success = CultureManager.Instance.UnlockTechNode(
                currentCulture.GetCultureID(),
                selectedNode,
                playerRealmID
            );

            if (success)
            {
                Debug.Log($"[CultureTechTreeUI] Successfully unlocked {selectedNode.NodeName}");
                RefreshTreeInfo();
                RefreshNodeList();
                RefreshNodeDetails();
            }
            else
            {
                Debug.LogWarning($"[CultureTechTreeUI] Failed to unlock {selectedNode.NodeName}");
            }
        }

        // ========== PUBLIC API ==========

        /// <summary>
        /// Set the player's realm ID for unlock permissions.
        /// </summary>
        public void SetPlayerRealmID(int realmID)
        {
            playerRealmID = realmID;
            RefreshUnlockButton();
        }

        /// <summary>
        /// Select a specific culture to view.
        /// </summary>
        public void SelectCulture(CultureData culture)
        {
            var cultures = CultureManager.Instance.GetAllCultures();
            int index = cultures.IndexOf(culture);

            if (index >= 0)
            {
                cultureDropdown.value = index;
            }
        }

        // ========== CLEANUP ==========

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCultureXPAdded -= HandleCultureXPAdded;
            }

            // Remove UI listeners
            cultureDropdown?.onValueChanged.RemoveAllListeners();
            economicsTabButton?.onClick.RemoveAllListeners();
            warfareTabButton?.onClick.RemoveAllListeners();
            religionTabButton?.onClick.RemoveAllListeners();
            politicsTabButton?.onClick.RemoveAllListeners();
            scienceTabButton?.onClick.RemoveAllListeners();
            unlockNodeButton?.onClick.RemoveAllListeners();
        }
    }
}
