using UnityEngine;
using UnityEngine.UI;
using TWK.Cultures;

namespace TWK.UI
{
    /// <summary>
    /// Renders a connection line between two tech nodes using a UI Image.
    /// The line is stretched and rotated to connect the nodes.
    /// </summary>
    public class TechTreeConnectionLine : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Image lineImage;
        [SerializeField] private float lineThickness = 3f;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f, 1f);     // Green
        [SerializeField] private Color availableColor = new Color(0.8f, 0.8f, 0.2f, 0.7f);  // Yellow (slightly transparent)
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);     // Gray (more transparent)

        private RectTransform rectTransform;
        private TechNode sourceNode;
        private TechNode targetNode;
        private CultureTechTree tree;

        // ========== INITIALIZATION ==========

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (lineImage == null)
            {
                lineImage = GetComponent<Image>();
            }

            // Create image component if it doesn't exist
            if (lineImage == null)
            {
                lineImage = gameObject.AddComponent<Image>();
                lineImage.color = lockedColor;
            }
        }

        /// <summary>
        /// Initialize the connection line between two nodes.
        /// </summary>
        public void Initialize(
            Vector2 startPos,
            Vector2 endPos,
            TechNode source,
            TechNode target,
            CultureTechTree techTree)
        {
            sourceNode = source;
            targetNode = target;
            tree = techTree;

            PositionLine(startPos, endPos);
            UpdateLineAppearance();
        }

        // ========== LINE POSITIONING ==========

        /// <summary>
        /// Position and rotate the line to connect start and end points.
        /// </summary>
        private void PositionLine(Vector2 startPos, Vector2 endPos)
        {
            if (rectTransform == null)
                return;

            // Adjust start position to right edge of source node
            startPos.x += 32f; // Half of NODE_WIDTH (64/2)

            // Adjust end position to left edge of target node
            endPos.x -= 32f; // Half of NODE_WIDTH (64/2)

            // Calculate line center, length, and angle
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set position (center of the line)
            Vector2 centerPos = (startPos + endPos) / 2f;
            rectTransform.anchoredPosition = centerPos;

            // Set size (length x thickness)
            rectTransform.sizeDelta = new Vector2(distance, lineThickness);

            // Set rotation
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Set pivot to center for proper rotation
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        // ========== APPEARANCE ==========

        /// <summary>
        /// Update the line's color based on node unlock status.
        /// </summary>
        private void UpdateLineAppearance()
        {
            if (lineImage == null || sourceNode == null || targetNode == null || tree == null)
                return;

            bool sourceUnlocked = tree.IsNodeUnlocked(sourceNode);
            bool targetUnlocked = tree.IsNodeUnlocked(targetNode);

            // Determine line color based on node states
            Color lineColor;
            if (targetUnlocked)
            {
                // Target is unlocked - show as fully unlocked
                lineColor = unlockedColor;
            }
            else if (sourceUnlocked && targetNode.ArePrerequisitesMet())
            {
                // Source unlocked and target is available - show as available
                lineColor = availableColor;
            }
            else
            {
                // Locked state
                lineColor = lockedColor;
            }

            lineImage.color = lineColor;
        }

        // ========== PUBLIC API ==========

        /// <summary>
        /// Refresh the line's appearance (call after tech tree changes).
        /// </summary>
        public void Refresh()
        {
            UpdateLineAppearance();
        }
    }
}
