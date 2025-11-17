using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;
using TWK.Realms;
using TWK.Simulation;
using TWK.Core;

namespace TWK.Agents
{
    /// <summary>
    /// Agent MonoBehaviour - the "View" in MVVM.
    /// Holds AgentData and coordinates Unity-specific behavior.
    /// Manages agent skills, salary, and personal wealth.
    /// Follows the same pattern as City.cs and Realm.cs.
    /// </summary>
    public class Agent : MonoBehaviour, ISimulationAgent
    {
        // ========== DATA MODEL ==========
        [SerializeField] private AgentData agentData = new AgentData();

        // ========== INSPECTOR CONFIGURATION (for initialization) ==========
        [Header("Agent Configuration")]
        [SerializeField] private string agentName = "NewAgent";
        [SerializeField] private int agentID = 0;
        [SerializeField] private int homeRealmID = -1;
        [SerializeField] private int cultureID = -1;

        // ========== LEGACY COMPATIBILITY ==========
        // These properties redirect to agentData for backwards compatibility
        public Realm HomeRealm
        {
            get
            {
                if (agentData.HomeRealmID == -1)
                    return null;

                return TWK.Realms.RealmManager.Instance?.GetRealm(agentData.HomeRealmID);
            }
            set
            {
                if (value != null)
                {
                    agentData.HomeRealmID = value.RealmID;
                }
            }
        }

        public string AgentName
        {
            get => agentData.AgentName;
            set => agentData.AgentName = value;
        }

        public int Age
        {
            get => agentData.Age;
            set => agentData.Age = value;
        }

        public int BirthDay
        {
            get => agentData.BirthDay;
            set => agentData.BirthDay = value;
        }

        public int BirthYear
        {
            get => agentData.BirthYear;
            set => agentData.BirthYear = value;
        }

        public int BirthMonth
        {
            get => agentData.BirthMonth;
            set => agentData.BirthMonth = value;
        }

        public Dictionary<TreeType, float> SkillLevels
        {
            get => agentData.SkillLevels;
            set => agentData.SkillLevels = value;
        }

        public TreeType SkillFocus
        {
            get => agentData.SkillFocus;
            set => agentData.SkillFocus = value;
        }

        public float DailySkillGain
        {
            get => agentData.DailySkillGain;
            set => agentData.DailySkillGain = value;
        }

        public float DailyFocusBonus
        {
            get => agentData.DailyFocusBonus;
            set => agentData.DailyFocusBonus = value;
        }

        // ========== DATA ACCESS ==========
        /// <summary>
        /// Direct access to the underlying data model for ViewModels.
        /// </summary>
        public AgentData Data => agentData;

        // ========== RUNTIME DEPENDENCIES ==========
        private WorldTimeManager _worldTimeManager;
        private AgentLedger _ledger;

        /// <summary>
        /// Access to the agent's personal ledger.
        /// </summary>
        public AgentLedger Ledger => _ledger;

        // ========== LIFECYCLE ==========
        private void Awake()
        {
            // Initialize agentData from Inspector values
            if (string.IsNullOrEmpty(agentData.AgentName))
            {
                agentData.AgentName = agentName;
                agentData.AgentID = agentID;
                agentData.HomeRealmID = homeRealmID;
                agentData.CultureID = cultureID;

                // Initialize skill levels
                foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
                {
                    if (!agentData.SkillLevels.ContainsKey(tree))
                        agentData.SkillLevels.Add(tree, 0f);
                }
            }
        }

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            _worldTimeManager = worldTimeManager;

            // Subscribe to time events
            _worldTimeManager.OnDayTick += AdvanceDay;
            _worldTimeManager.OnSeasonTick += AdvanceSeason;
            _worldTimeManager.OnYearTick += AdvanceYear;

            // Register with AgentManager
            if (AgentManager.Instance != null)
            {
                AgentManager.Instance.RegisterAgent(this);
                _ledger = AgentManager.Instance.GetAgentLedger(agentData.AgentID);
            }
            else
            {
                Debug.LogWarning($"[Agent] AgentManager not found when initializing {agentData.AgentName}");
                // Create standalone ledger
                _ledger = new AgentLedger(agentData);
            }

            // Initialize skill levels if not already done
            foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
            {
                if (!agentData.SkillLevels.ContainsKey(tree))
                    agentData.SkillLevels.Add(tree, 0f);
            }

            Debug.Log($"[Agent] Initialized: {agentData.AgentName} (ID: {agentData.AgentID})");
        }

        private void OnDestroy()
        {
            // Unsubscribe from time events
            if (_worldTimeManager != null)
            {
                _worldTimeManager.OnDayTick -= AdvanceDay;
                _worldTimeManager.OnSeasonTick -= AdvanceSeason;
                _worldTimeManager.OnYearTick -= AdvanceYear;
            }

            // Unregister from AgentManager
            if (AgentManager.Instance != null)
            {
                AgentManager.Instance.UnregisterAgent(agentData.AgentID);
            }
        }

        // ========== SIMULATION TICKS ==========
        public void AdvanceDay()
        {
            if (!agentData.IsAlive)
                return;

            OnDailySkillGain();
        }

        public void AdvanceSeason()
        {
            if (!agentData.IsAlive)
                return;

            // Seasonal agent logic
        }

        public void AdvanceYear()
        {
            if (!agentData.IsAlive)
                return;

            // Age the agent
            agentData.Age++;
        }

        // ========== SKILL SYSTEM ==========

        public void GainSkill(TreeType tree, float amount)
        {
            agentData.SkillLevels[tree] += amount;
        }

        private void OnDailySkillGain()
        {
            var trees = new List<TreeType>(agentData.SkillLevels.Keys); // snapshot of keys

            foreach (var tree in trees)
            {
                float gain = agentData.DailySkillGain;
                if (tree == agentData.SkillFocus)
                    gain += agentData.DailyFocusBonus;
                else
                    gain -= agentData.DailyFocusBonus * 0.5f;

                GainSkill(tree, gain);
            }
        }

        // ========== WEALTH & PROPERTY ==========

        /// <summary>
        /// Get agent's current gold.
        /// </summary>
        public int GetGold()
        {
            return _ledger != null ? _ledger.GetResource(TWK.Economy.ResourceType.Gold) : 0;
        }

        /// <summary>
        /// Get agent's wealth status.
        /// </summary>
        public string GetWealthStatus()
        {
            return _ledger != null ? _ledger.GetWealthStatus() : "Unknown";
        }

        // ========== OFFICE MANAGEMENT ==========

        /// <summary>
        /// Is this agent currently holding a government office?
        /// </summary>
        public bool HasOffice()
        {
            return agentData.HasOffice;
        }

        /// <summary>
        /// Get the office this agent currently holds.
        /// </summary>
        public int GetCurrentOffice()
        {
            return agentData.CurrentOfficeID;
        }


        public int GetAgentId()
        {
            return agentData.AgentID;
        }   
        // ========== DEBUG ==========

        [ContextMenu("Print Agent Info")]
        public void PrintAgentInfo()
        {
            Debug.Log($"=== Agent: {agentData.AgentName} (ID: {agentData.AgentID}) ===");
            Debug.Log($"Age: {agentData.Age}");
            Debug.Log($"Home Realm: {agentData.HomeRealmID}");
            Debug.Log($"Culture: {agentData.CultureID}");
            Debug.Log($"Office: {(agentData.HasOffice ? agentData.CurrentOfficeID.ToString() : "None")}");
            Debug.Log($"Salary: {agentData.MonthlySalary}");
            Debug.Log($"Gold: {GetGold()}");
            Debug.Log($"Wealth Status: {GetWealthStatus()}");
            Debug.Log($"Skill Focus: {agentData.SkillFocus}");

            foreach (var skill in agentData.SkillLevels)
            {
                Debug.Log($"  {skill.Key}: {skill.Value:F1}");
            }
        }
    }
}
