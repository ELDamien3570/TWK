using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Cultures;

namespace TWK.UI
{
    /// <summary>
    /// UI item representing a single tech node in the tech tree.
    /// Shows node status (locked, available, unlocked) and handles selection.
    /// </summary>
    public class TechNodeUIItem : MonoBehaviour
    {
        // ========== UI REFERENCES ==========
        [Header("Node Display")]
        [SerializeField] private TextMeshProUGUI nodeNameText;
        [SerializeField] private Image nodeIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button selectButton;

        [Header("Status Indicators")]
        [SerializeField] private GameObject unlockedIndicator;
        [SerializeField] private GameObject availableIndicator;
        [SerializeField] private GameObject lockedIndicator;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
        [SerializeField] private Color availableColor = new Color(0.8f, 0.8f, 0.2f, 1f); // Yellow
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

        // ========== STATE ==========
        private TechNode node;
        private CultureTechTree tree;
        private Action<TechNode> onSelected;

        // ========== INITIALIZATION ==========

        public void Initialize(TechNode techNode, CultureTechTree techTree, Action<TechNode> onNodeSelected)
        {
            node = techNode;
            tree = techTree;
            onSelected = onNodeSelected;

            SetupButton();
            RefreshDisplay();
        }

        private void SetupButton()
        {
            selectButton?.onClick.RemoveAllListeners();
            selectButton?.onClick.AddListener(OnClicked);
        }

        // ========== DISPLAY ==========

        private void RefreshDisplay()
        {
            if (node == null)
                return;

            // Set node name
            nodeNameText.text = node.NodeName;

            // Set icon if available
            if (nodeIcon != null && node.Icon != null)
            {
                nodeIcon.sprite = node.Icon;
                nodeIcon.gameObject.SetActive(true);
            }
            else if (nodeIcon != null)
            {
                nodeIcon.gameObject.SetActive(false);
            }

            // Update status
            UpdateNodeStatus();
        }

        private void UpdateNodeStatus()
        {
            bool isUnlocked = tree != null && tree.IsNodeUnlocked(node);
            bool isAvailable = !isUnlocked && node.ArePrerequisitesMet();
            bool isLocked = !isUnlocked && !isAvailable;

            // Update indicators
            if (unlockedIndicator != null)
                unlockedIndicator.SetActive(isUnlocked);

            if (availableIndicator != null)
                availableIndicator.SetActive(isAvailable);

            if (lockedIndicator != null)
                lockedIndicator.SetActive(isLocked);

            // Update background color
            if (backgroundImage != null)
            {
                if (isUnlocked)
                    backgroundImage.color = unlockedColor;
                else if (isAvailable)
                    backgroundImage.color = availableColor;
                else
                    backgroundImage.color = lockedColor;
            }
        }

        // ========== INTERACTION ==========

        private void OnClicked()
        {
            onSelected?.Invoke(node);
        }

        // ========== PUBLIC API ==========

        /// <summary>
        /// Refresh the display (call after tech tree changes).
        /// </summary>
        public void Refresh()
        {
            RefreshDisplay();
        }

        // ========== CLEANUP ==========

        private void OnDestroy()
        {
            selectButton?.onClick.RemoveAllListeners();
        }
    }
}
