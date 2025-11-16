using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Religion;

namespace TWK.UI
{
    /// <summary>
    /// UI component for displaying deity information.
    /// Attach to deity item prefab and assign UI references in Inspector.
    /// </summary>
    public class DeityUIItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image deityIcon;
        [SerializeField] private TextMeshProUGUI deityNameText;
        [SerializeField] private TextMeshProUGUI deityDetailsText; // Content inside scrollview

        [Header("Fallback")]
        [SerializeField] private Sprite defaultIcon;

        private Deity currentDeity;

        /// <summary>
        /// Initialize this UI item with deity data.
        /// Call this after instantiating the prefab.
        /// </summary>
        public void Initialize(Deity deity)
        {
            if (deity == null)
            {
                Debug.LogWarning("[DeityUIItem] Attempted to initialize with null deity");
                return;
            }

            currentDeity = deity;
            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the display with current deity data.
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentDeity == null)
                return;

            // Set icon
            if (deityIcon != null)
            {
                deityIcon.sprite = currentDeity.Icon != null ? currentDeity.Icon : defaultIcon;
                deityIcon.enabled = deityIcon.sprite != null;

                // Set icon color to deity's color
                deityIcon.color = currentDeity.DeityColor;
            }

            // Set name (with title if available)
            if (deityNameText != null)
            {
                string nameDisplay = $"<b>{currentDeity.Name}</b>";
                if (!string.IsNullOrEmpty(currentDeity.Title))
                {
                    nameDisplay += $"\n<i>{currentDeity.Title}</i>";
                }
                deityNameText.text = nameDisplay;
            }

            // Set detailed information in scrollview
            if (deityDetailsText != null)
            {
                string details = BuildDetailsText();
                deityDetailsText.text = details;
            }
        }

        /// <summary>
        /// Build the detailed text for mythos, description, and traits.
        /// </summary>
        private string BuildDetailsText()
        {
            var text = new System.Text.StringBuilder();

            // Mythos section
            if (!string.IsNullOrEmpty(currentDeity.Mythos))
            {
                text.AppendLine("<b>Mythos:</b>");
                text.AppendLine(currentDeity.Mythos);
                text.AppendLine();
            }

            // Description section
            if (!string.IsNullOrEmpty(currentDeity.Description))
            {
                text.AppendLine("<b>Domain:</b>");
                text.AppendLine(currentDeity.Description);
                text.AppendLine();
            }

            // Traits section
            if (currentDeity.FamousTraits != null && currentDeity.FamousTraits.Count > 0)
            {
                text.AppendLine("<b>Famous Traits:</b>");
                foreach (var trait in currentDeity.FamousTraits)
                {
                    text.AppendLine($"  • {trait}");
                }
            }

            return text.ToString().TrimEnd();
        }

        /// <summary>
        /// Get the currently displayed deity.
        /// </summary>
        public Deity GetDeity()
        {
            return currentDeity;
        }
    }
}
