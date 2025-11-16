using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Religion;

namespace TWK.UI
{
    /// <summary>
    /// UI component for displaying tenet information.
    /// Attach to tenet item prefab and assign UI references in Inspector.
    /// </summary>
    public class TenetUIItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image tenetIcon;
        [SerializeField] private TextMeshProUGUI tenetNameText;
        [SerializeField] private TextMeshProUGUI tenetDetailsText; // Content inside scrollview

        [Header("Fallback")]
        [SerializeField] private Sprite defaultIcon;

        private Tenet currentTenet;
        private Color religionColor = Color.white;

        /// <summary>
        /// Initialize this UI item with tenet data.
        /// Call this after instantiating the prefab.
        /// </summary>
        public void Initialize(Tenet tenet, Color religionColor)
        {
            if (tenet == null)
            {
                Debug.LogWarning("[TenetUIItem] Attempted to initialize with null tenet");
                return;
            }

            currentTenet = tenet;
            this.religionColor = religionColor;
            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the display with current tenet data.
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentTenet == null)
                return;

            // Set icon
            if (tenetIcon != null)
            {
                tenetIcon.sprite = currentTenet.Icon != null ? currentTenet.Icon : defaultIcon;
                tenetIcon.enabled = tenetIcon.sprite != null;

                // Set icon color to religion's color
                tenetIcon.color = religionColor;
            }

            // Set name with category
            if (tenetNameText != null)
            {
                tenetNameText.text = $"<b>{currentTenet.Name}</b>\n<i>{currentTenet.Category}</i>";
            }

            // Set detailed information in scrollview
            if (tenetDetailsText != null)
            {
                string details = BuildDetailsText();
                tenetDetailsText.text = details;
            }
        }

        /// <summary>
        /// Build the detailed text for description, modifiers, and special rules.
        /// </summary>
        private string BuildDetailsText()
        {
            var text = new System.Text.StringBuilder();

            // Description section
            if (!string.IsNullOrEmpty(currentTenet.Description))
            {
                text.AppendLine("<b>Doctrine:</b>");
                text.AppendLine(currentTenet.Description);
                text.AppendLine();
            }

            // Modifiers section
            if (currentTenet.Modifiers != null && currentTenet.Modifiers.Count > 0)
            {
                text.AppendLine("<b>Effects:</b>");
                foreach (var modifier in currentTenet.Modifiers)
                {
                    if (modifier.Effects != null)
                    {
                        foreach (var effect in modifier.Effects)
                        {
                            text.AppendLine($"  • {effect.GetDescription()}");
                        }
                    }
                }
                text.AppendLine();
            }

            // Special rules section
            if (currentTenet.HasSpecialRules && !string.IsNullOrEmpty(currentTenet.SpecialRulesDescription))
            {
                text.AppendLine("<b><color=yellow>Special Rules:</color></b>");
                text.AppendLine(currentTenet.SpecialRulesDescription);
            }

            return text.ToString().TrimEnd();
        }

        /// <summary>
        /// Get the currently displayed tenet.
        /// </summary>
        public Tenet GetTenet()
        {
            return currentTenet;
        }
    }
}
