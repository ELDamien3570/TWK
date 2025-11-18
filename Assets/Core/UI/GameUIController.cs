using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Agents;
using TWK.Realms;
using TWK.Core;
using TWK.Simulation;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// Master UI controller that manages all menus and maintains player context.
    /// Tracks which agent/realm the player is controlling and coordinates all UI panels.
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        public static GameUIController Instance { get; private set; }

        // ========== PLAYER CONTEXT ==========
        [Header("Player Context")]
        [SerializeField] private int defaultPlayerAgentID = 0;
        [SerializeField] private int defaultPlayerRealmID = 0;

        private int currentPlayerAgentID = -1;
        private int currentPlayerRealmID = -1;

        /// <summary>
        /// Current agent the player is controlling.
        /// </summary>
        public int CurrentPlayerAgentID => currentPlayerAgentID;

        /// <summary>
        /// Current realm the player is controlling.
        /// </summary>
        public int CurrentPlayerRealmID => currentPlayerRealmID;

        // ========== MENU CONTROLLERS ==========
        [Header("Menu Controllers")]
        [SerializeField] private AgentUIController agentUIController;
        [SerializeField] private GameObject diplomacyUIController;
        [SerializeField] private GameObject realmUIController;
        [SerializeField] private GameObject cityUIController;
        [SerializeField] private GameObject debugMenuPanel;

        // ========== UI PANELS ==========
        [Header("Main UI")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private TextMeshProUGUI playerContextText;
        [SerializeField] private Button agentMenuButton;
        [SerializeField] private Button diplomacyMenuButton;
        [SerializeField] private Button realmMenuButton;
        [SerializeField] private Button cityMenuButton;
        [SerializeField] private Button toggleDebugButton;

        // ========== EVENTS ==========
        public event System.Action<int> OnPlayerAgentChanged;
        public event System.Action<int> OnPlayerRealmChanged;

        // ========== INITIALIZATION ==========

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupButtons();
            InitializePlayerContext();
            UpdatePlayerContextDisplay();

            // Close all menus initially
            CloseAllMenus();

            Debug.Log("[GameUIController] Initialized");
        }

        private void Update()
        {
            // Hotkeys
            if (Input.GetKeyDown(KeyCode.F1)) ToggleDebugMenu();
            if (Input.GetKeyDown(KeyCode.Escape)) CloseAllMenus();
        }

        // ========== PLAYER CONTEXT MANAGEMENT ==========

        /// <summary>
        /// Initialize player context from serialized defaults or first available.
        /// </summary>
        private void InitializePlayerContext()
        {
            // Try to set from defaults
            if (defaultPlayerAgentID >= 0)
            {
                SetPlayerAgent(defaultPlayerAgentID);
            }
            else
            {
                // Find first available agent
                if (AgentManager.Instance != null)
                {
                    var agents = AgentManager.Instance.GetAllAgents();
                    if (agents != null && agents.Count > 0)
                    {
                        SetPlayerAgent(agents[0].Data.AgentID);
                    }
                }
            }

            // Try to set realm from defaults
            if (defaultPlayerRealmID >= 0)
            {
                SetPlayerRealm(defaultPlayerRealmID);
            }
            else
            {
                // Find first available realm
                if (RealmManager.Instance != null)
                {
                    var realms = RealmManager.Instance.GetAllRealms();
                    if (realms != null && realms.Count > 0)
                    {
                        SetPlayerRealm(realms[0].RealmID);
                    }
                }
            }

            Debug.Log($"[GameUIController] Player context initialized: Agent {currentPlayerAgentID}, Realm {currentPlayerRealmID}");
        }

        /// <summary>
        /// Set the current player agent. This is who the player is "playing as".
        /// </summary>
        public void SetPlayerAgent(int agentID)
        {
            if (AgentManager.Instance?.GetAgent(agentID) == null)
            {
                Debug.LogWarning($"[GameUIController] Cannot set player agent to {agentID} - agent not found");
                return;
            }

            currentPlayerAgentID = agentID;
            OnPlayerAgentChanged?.Invoke(agentID);
            UpdatePlayerContextDisplay();

            Debug.Log($"[GameUIController] Player agent changed to: {agentID}");
        }

        /// <summary>
        /// Set the current player realm. This is the realm the player is controlling.
        /// </summary>
        public void SetPlayerRealm(int realmID)
        {
            if (RealmManager.Instance?.GetRealm(realmID) == null)
            {
                Debug.LogWarning($"[GameUIController] Cannot set player realm to {realmID} - realm not found");
                return;
            }

            currentPlayerRealmID = realmID;
            OnPlayerRealmChanged?.Invoke(realmID);
            UpdatePlayerContextDisplay();

            Debug.Log($"[GameUIController] Player realm changed to: {realmID}");
        }

        /// <summary>
        /// Get the current player agent data.
        /// </summary>
        public Agent GetPlayerAgent()
        {
            return AgentManager.Instance?.GetAgent(currentPlayerAgentID);
        }

        /// <summary>
        /// Get the current player realm.
        /// </summary>
        public Realm GetPlayerRealm()
        {
            return RealmManager.Instance?.GetRealm(currentPlayerRealmID);
        }

        private void UpdatePlayerContextDisplay()
        {
            if (playerContextText == null) return;

            string agentName = GetPlayerAgent()?.Data.AgentName ?? "None";
            string realmName = GetPlayerRealm()?.RealmName ?? "None";

            playerContextText.text = $"Playing as: {agentName} | Realm: {realmName}";
        }

        // ========== MENU MANAGEMENT ==========

        private void SetupButtons()
        {
            agentMenuButton?.onClick.AddListener(OpenAgentMenu);
            diplomacyMenuButton?.onClick.AddListener(OpenDiplomacyMenu);
            realmMenuButton?.onClick.AddListener(OpenRealmMenu);
            cityMenuButton?.onClick.AddListener(OpenCityMenu);
            toggleDebugButton?.onClick.AddListener(ToggleDebugMenu);
        }

        /// <summary>
        /// Open the agent menu for the current player agent.
        /// </summary>
        public void OpenAgentMenu()
        {
            CloseAllMenus();

            if (agentUIController != null)
            {
                agentUIController.gameObject.SetActive(true);
                agentUIController.SetAgent(currentPlayerAgentID);
            }
            else
            {
                Debug.LogWarning("[GameUIController] AgentUIController not assigned");
            }
        }

        /// <summary>
        /// Open the agent menu for a specific agent (not necessarily the player).
        /// </summary>
        public void OpenAgentMenu(int agentID)
        {
            CloseAllMenus();

            if (agentUIController != null)
            {
                agentUIController.gameObject.SetActive(true);
                agentUIController.SetAgent(agentID);
            }
        }

        /// <summary>
        /// Open the diplomacy menu.
        /// </summary>
        public void OpenDiplomacyMenu()
        {
            CloseAllMenus();

            if (diplomacyUIController != null)
            {
                diplomacyUIController.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[GameUIController] DiplomacyUIController not assigned");
            }
        }

        /// <summary>
        /// Open the realm menu for the current player realm.
        /// </summary>
        public void OpenRealmMenu()
        {
            CloseAllMenus();

            if (realmUIController != null)
            {
                realmUIController.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[GameUIController] RealmUIController not assigned");
            }
        }

        /// <summary>
        /// Open the city menu.
        /// </summary>
        public void OpenCityMenu()
        {
            CloseAllMenus();

            if (cityUIController != null)
            {
                cityUIController.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[GameUIController] CityUIController not assigned");
            }
        }

        /// <summary>
        /// Toggle the debug menu.
        /// </summary>
        public void ToggleDebugMenu()
        {
            if (debugMenuPanel != null)
            {
                debugMenuPanel.SetActive(!debugMenuPanel.activeSelf);
            }
        }

        /// <summary>
        /// Close all menus.
        /// </summary>
        public void CloseAllMenus()
        {
            if (agentUIController != null)
                agentUIController.gameObject.SetActive(false);

            if (diplomacyUIController != null)
                diplomacyUIController.SetActive(false);

            if (realmUIController != null)
                realmUIController.SetActive(false);

            if (cityUIController != null)
                cityUIController.SetActive(false);

            if (debugMenuPanel != null)
                debugMenuPanel.SetActive(false);
        }

        // ========== DEBUG HELPERS ==========

        [ContextMenu("Log Player Context")]
        public void LogPlayerContext()
        {
            Debug.Log("===== PLAYER CONTEXT =====");
            Debug.Log($"Player Agent ID: {currentPlayerAgentID}");
            Debug.Log($"Player Realm ID: {currentPlayerRealmID}");

            var agent = GetPlayerAgent();
            if (agent != null)
            {
                Debug.Log($"Agent Name: {agent.Data.AgentName}");
                Debug.Log($"Agent Age: {agent.Data.Age}");
                Debug.Log($"Agent Culture: {agent.Data.CultureID}");
            }

            var realm = GetPlayerRealm();
            if (realm != null)
            {
                Debug.Log($"Realm Name: {realm.RealmName}");
                Debug.Log($"Realm Leaders: {realm.Data.LeaderIDs.Count}");
                Debug.Log($"Realm Cities: {realm.Data.DirectlyOwnedCityIDs.Count}");
            }

            Debug.Log("==========================");
        }

        [ContextMenu("Switch to Next Agent")]
        public void DebugSwitchToNextAgent()
        {
            if (AgentManager.Instance == null) return;

            var agents = AgentManager.Instance.GetAllAgents();
            if (agents == null || agents.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].Data.AgentID == currentPlayerAgentID)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % agents.Count;
            SetPlayerAgent(agents[nextIndex].Data.AgentID);
        }

        [ContextMenu("Switch to Next Realm")]
        public void DebugSwitchToNextRealm()
        {
            if (RealmManager.Instance == null) return;

            var realms = RealmManager.Instance.GetAllRealms();
            if (realms == null || realms.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < realms.Count; i++)
            {
                if (realms[i].RealmID == currentPlayerRealmID)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % realms.Count;
            SetPlayerRealm(realms[nextIndex].RealmID);
        }
    }
}
