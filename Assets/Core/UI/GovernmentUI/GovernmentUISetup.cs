using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TWK.UI
{
    /// <summary>
    /// Helper script to quickly set up Government UI in the scene.
    /// Use the context menu to create UI elements programmatically.
    /// </summary>
    public class GovernmentUISetup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private int testRealmID = 1;

        [Header("Prefabs (Optional)")]
        [Tooltip("If you have prefabs for these, assign them here")]
        [SerializeField] private GameObject governmentPanelPrefab;
        [SerializeField] private GameObject bureaucracyPanelPrefab;
        [SerializeField] private GameObject contractPanelPrefab;

        private void Start()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }
        }

        [ContextMenu("Setup All UI Panels")]
        public void SetupAllUIPanels()
        {
            if (targetCanvas == null)
            {
                Debug.LogError("[GovernmentUISetup] No canvas found! Assign targetCanvas or add a Canvas to the scene.");
                return;
            }

            SetupGovernmentPanel();
            SetupBureaucracyPanel();
            SetupContractPanel();

            Debug.Log("[GovernmentUISetup] All UI panels created successfully!");
        }

        [ContextMenu("Setup Government Panel")]
        public void SetupGovernmentPanel()
        {
            GameObject panel;

            if (governmentPanelPrefab != null)
            {
                panel = Instantiate(governmentPanelPrefab, targetCanvas.transform);
            }
            else
            {
                panel = CreateBasicPanel("GovernmentPanel", new Vector2(400, 600));
                AddGovernmentUIController(panel);
            }

            panel.name = "GovernmentPanel";
            Debug.Log("[GovernmentUISetup] Government Panel created");
        }

        [ContextMenu("Setup Bureaucracy Panel")]
        public void SetupBureaucracyPanel()
        {
            GameObject panel;

            if (bureaucracyPanelPrefab != null)
            {
                panel = Instantiate(bureaucracyPanelPrefab, targetCanvas.transform);
            }
            else
            {
                panel = CreateBasicPanel("BureaucracyPanel", new Vector2(500, 700));
                AddBureaucracyUIController(panel);
            }

            panel.name = "BureaucracyPanel";
            Debug.Log("[GovernmentUISetup] Bureaucracy Panel created");
        }

        [ContextMenu("Setup Contract Panel")]
        public void SetupContractPanel()
        {
            GameObject panel;

            if (contractPanelPrefab != null)
            {
                panel = Instantiate(contractPanelPrefab, targetCanvas.transform);
            }
            else
            {
                panel = CreateBasicPanel("ContractPanel", new Vector2(600, 700));
                AddContractUIController(panel);
            }

            panel.name = "ContractPanel";
            Debug.Log("[GovernmentUISetup] Contract Panel created");
        }

        private GameObject CreateBasicPanel(string name, Vector2 size)
        {
            // Create panel GameObject
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(targetCanvas.transform, false);

            // Add RectTransform
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add Image (background)
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            // Add ScrollRect for scrolling
            ScrollRect scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Create content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1000); // Tall enough for content

            // Add VerticalLayoutGroup for auto-layout
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;

            // Add ContentSizeFitter for dynamic sizing
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            scrollRect.viewport = rectTransform;

            // Add title text
            CreateTitleText(name, content.transform);

            return panel;
        }

        private void CreateTitleText(string title, Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title.Replace("Panel", " Panel");
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 40);
        }

        private void AddGovernmentUIController(GameObject panel)
        {
            var controller = panel.AddComponent<GovernmentUIController>();

            // Use reflection to set the targetRealmID field
            var field = typeof(GovernmentUIController).GetField("targetRealmID",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, testRealmID);
            }

            Debug.Log($"[GovernmentUISetup] Added GovernmentUIController for realm {testRealmID}");
        }

        private void AddBureaucracyUIController(GameObject panel)
        {
            var controller = panel.AddComponent<BureaucracyUIController>();

            var field = typeof(BureaucracyUIController).GetField("targetRealmID",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, testRealmID);
            }

            Debug.Log($"[GovernmentUISetup] Added BureaucracyUIController for realm {testRealmID}");
        }

        private void AddContractUIController(GameObject panel)
        {
            var controller = panel.AddComponent<ContractUIController>();

            var field = typeof(ContractUIController).GetField("targetRealmID",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, testRealmID);
            }

            Debug.Log($"[GovernmentUISetup] Added ContractUIController for realm {testRealmID}");
        }

        [ContextMenu("Create Test Canvas")]
        public void CreateTestCanvas()
        {
            // Check if canvas already exists
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                targetCanvas = existingCanvas;
                Debug.Log("[GovernmentUISetup] Using existing Canvas");
                return;
            }

            // Create Canvas GameObject
            GameObject canvasObj = new GameObject("GovernmentUI_Canvas");
            targetCanvas = canvasObj.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Add CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Add GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem if it doesn't exist
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[GovernmentUISetup] Created new Canvas and EventSystem");
        }
    }
}
