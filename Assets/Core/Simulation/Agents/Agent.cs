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

        public Gender Gender
        {
            get => agentData.Gender;
            set => agentData.Gender = value;
        }

        public Sexuality Sexuality
        {
            get => agentData.Sexuality;
            set => agentData.Sexuality = value;
        }

        public float Prestige
        {
            get => agentData.Prestige;
            set => agentData.Prestige = value;
        }

        public float Morality
        {
            get => agentData.Morality;
            set => agentData.Morality = value;
        }

        public float Reputation => agentData.CalculateReputation();

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

            // Check for natural death (simple age-based for now)
            // TODO: Implement more sophisticated death chances based on health, age, etc.
            if (agentData.Age > 60)
            {
                float deathChance = (agentData.Age - 60) * 0.01f; // 1% per year over 60
                if (UnityEngine.Random.value < deathChance)
                {
                    HandleDeath();
                }
            }
        }

        // ========== DEATH & INHERITANCE ==========

        /// <summary>
        /// Handle agent death - distribute inheritance, update relationships.
        /// </summary>
        public void HandleDeath()
        {
            if (!agentData.IsAlive)
                return;

            agentData.IsAlive = false;
            Debug.Log($"[Agent] {agentData.AgentName} has died at age {agentData.Age}");

            // Distribute inheritance to children
            if (_ledger != null && agentData.ChildrenIDs.Count > 0)
            {
                var inheritance = _ledger.CalculateInheritance();
                int childCount = agentData.ChildrenIDs.Count;

                foreach (int childID in agentData.ChildrenIDs)
                {
                    var childAgent = AgentManager.Instance?.GetAgent(childID);
                    if (childAgent != null && childAgent.Ledger != null)
                    {
                        // Divide inheritance equally among children
                        var childShare = new Dictionary<TWK.Economy.ResourceType, int>();
                        foreach (var kvp in inheritance)
                        {
                            childShare[kvp.Key] = kvp.Value / childCount;
                        }
                        childAgent.Ledger.ReceiveInheritance(childShare, agentData.AgentID);
                    }
                }

                _ledger.ClearWealth();
            }

            // TODO: Transfer properties (estates, caravans, cities) to heirs
            // TODO: Update relationships (widows, orphans, etc.)
        }

        /// <summary>
        /// Kill this agent in combat (when health reaches critical).
        /// </summary>
        public void KillInCombat()
        {
            if (agentData.IsCriticalHealth())
            {
                float deathChance = 0.5f; // 50% chance at critical health
                if (UnityEngine.Random.value < deathChance)
                {
                    Debug.Log($"[Agent] {agentData.AgentName} died in combat!");
                    HandleDeath();
                }
            }
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
            Debug.Log($"Age: {agentData.Age} | Gender: {agentData.Gender} | Sexuality: {agentData.Sexuality}");
            Debug.Log($"Home Realm: {agentData.HomeRealmID} | Culture: {agentData.CultureID}");
            Debug.Log($"Office: {(agentData.HasOffice ? agentData.CurrentOfficeID.ToString() : "None")}");
            Debug.Log($"Salary: {agentData.MonthlySalary} | Gold: {GetGold()} | Wealth: {GetWealthStatus()}");

            Debug.Log($"\n--- Reputation ---");
            Debug.Log($"Prestige: {agentData.Prestige:F1} | Morality: {agentData.Morality:F1} | Reputation: {Reputation:F1}");

            Debug.Log($"\n--- Skills ---");
            Debug.Log($"Skill Focus: {agentData.SkillFocus}");
            foreach (var skill in agentData.SkillLevels)
            {
                Debug.Log($"  {skill.Key}: {skill.Value:F1}");
            }

            Debug.Log($"\n--- Personality Traits ---");
            foreach (var trait in agentData.Traits)
            {
                Debug.Log($"  - {trait}");
            }

            Debug.Log($"\n--- Relationships ---");
            Debug.Log($"Parents: Mother={agentData.MotherID}, Father={agentData.FatherID}");
            Debug.Log($"Spouses: {agentData.SpouseIDs.Count} | Children: {agentData.ChildrenIDs.Count}");
            Debug.Log($"Friends: {agentData.FriendIDs.Count} | Rivals: {agentData.RivalIDs.Count}");
            Debug.Log($"Companions: {agentData.CompanionIDs.Count}");

            Debug.Log($"\n--- Combat Stats ---");
            Debug.Log($"Health: {agentData.Health:F1}/{agentData.MaxHealth:F1}");
            Debug.Log($"Strength: {agentData.Strength:F1} | Leadership: {agentData.Leadership:F1} | Morale: {agentData.Morale:F1}");
            Debug.Log($"Melee: {agentData.MeleeAttack:F1} ATK / {agentData.MeleeArmor:F1} ARM");
            Debug.Log($"Missile: {agentData.MissileAttack:F1} ATK / {agentData.MissileDefense:F1} DEF");
            Debug.Log($"Speed: {agentData.Speed:F1} | Charge: {agentData.ChargeSpeed:F1} | Agility: {agentData.Agility:F1}");
            Debug.Log($"Weapon Slots: {agentData.WeaponSlots} | Equipped: {agentData.EquippedWeaponIDs.Count}");

            Debug.Log($"\n--- Properties ---");
            Debug.Log($"Buildings: {agentData.OwnedBuildingIDs.Count}");
            Debug.Log($"Caravans: {agentData.OwnedCaravanIDs.Count}");
            Debug.Log($"Cities: {agentData.ControlledCityIDs.Count}");
        }
    }
}
