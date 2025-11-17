using System.Collections.Generic;
using UnityEngine;

namespace TWK.Agents
{
    /// <summary>
    /// Singleton manager for all agents (characters) in the game.
    /// Handles agent creation, lookup, and lifecycle management.
    /// Manages agent ledgers and salary payments.
    /// </summary>
    public class AgentManager : MonoBehaviour
    {
        public static AgentManager Instance { get; private set; }

        // ========== AGENT REGISTRY ==========
        private Dictionary<int, Agent> _agents = new Dictionary<int, Agent>();
        private Dictionary<int, AgentLedger> _agentLedgers = new Dictionary<int, AgentLedger>();
        private int _nextAgentID = 1;

        // ========== EVENTS ==========
        public event System.Action<int> OnAgentCreated; // agentID
        public event System.Action<int> OnAgentDied; // agentID
        public event System.Action<int, int> OnAgentAssignedToOffice; // agentID, officeID
        public event System.Action<int> OnAgentRemovedFromOffice; // agentID

        // ========== INITIALIZATION ==========
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ========== AGENT CREATION & REGISTRATION ==========

        /// <summary>
        /// Register an agent with the manager.
        /// Called by Agent.Initialize().
        /// </summary>
        public void RegisterAgent(Agent agent)
        {
            if (agent == null)
            {
                Debug.LogError("[AgentManager] Cannot register null agent");
                return;
            }

            if (_agents.ContainsKey(agent.Data.AgentID))
            {
                Debug.LogWarning($"[AgentManager] Agent {agent.Data.AgentID} already registered");
                return;
            }

            _agents[agent.Data.AgentID] = agent;

            // Create ledger for this agent
            var ledger = new AgentLedger(agent.Data);
            _agentLedgers[agent.Data.AgentID] = ledger;

            OnAgentCreated?.Invoke(agent.Data.AgentID);
            Debug.Log($"[AgentManager] Registered agent: {agent.Data.AgentName} (ID: {agent.Data.AgentID})");
        }

        /// <summary>
        /// Unregister an agent from the manager.
        /// Called by Agent.OnDestroy() or when agent dies.
        /// </summary>
        public void UnregisterAgent(int agentID)
        {
            if (_agents.Remove(agentID))
            {
                _agentLedgers.Remove(agentID);
                OnAgentDied?.Invoke(agentID);
                Debug.Log($"[AgentManager] Unregistered agent: {agentID}");
            }
        }

        /// <summary>
        /// Get next available agent ID.
        /// </summary>
        public int GetNextAgentID()
        {
            return _nextAgentID++;
        }

        // ========== AGENT LOOKUP ==========

        /// <summary>
        /// Get an agent by ID.
        /// </summary>
        public Agent GetAgent(int agentID)
        {
            return _agents.ContainsKey(agentID) ? _agents[agentID] : null;
        }

        /// <summary>
        /// Get an agent's ledger by ID.
        /// </summary>
        public AgentLedger GetAgentLedger(int agentID)
        {
            return _agentLedgers.ContainsKey(agentID) ? _agentLedgers[agentID] : null;
        }

        /// <summary>
        /// Get all agents.
        /// </summary>
        public List<Agent> GetAllAgents()
        {
            return new List<Agent>(_agents.Values);
        }

        /// <summary>
        /// Get all agents in a specific realm.
        /// </summary>
        public List<Agent> GetAgentsByRealm(int realmID)
        {
            var realmAgents = new List<Agent>();
            foreach (var agent in _agents.Values)
            {
                if (agent.Data.HomeRealmID == realmID)
                {
                    realmAgents.Add(agent);
                }
            }
            return realmAgents;
        }

        /// <summary>
        /// Get agent currently holding a specific office.
        /// </summary>
        public Agent GetAgentByOffice(int officeID, int realmID)
        {
            foreach (var agent in _agents.Values)
            {
                if (agent.Data.CurrentOfficeID == officeID && agent.Data.OfficeRealmID == realmID)
                {
                    return agent;
                }
            }
            return null;
        }

        // ========== OFFICE MANAGEMENT ==========

        /// <summary>
        /// Assign an agent to a government office.
        /// </summary>
        public void AssignAgentToOffice(int agentID, int officeID, int realmID, int salary)
        {
            var agent = GetAgent(agentID);
            if (agent == null)
            {
                Debug.LogError($"[AgentManager] Cannot assign agent {agentID} to office - agent not found");
                return;
            }

            agent.Data.AssignToOffice(officeID, realmID, salary);
            OnAgentAssignedToOffice?.Invoke(agentID, officeID);
            Debug.Log($"[AgentManager] Assigned {agent.Data.AgentName} to office {officeID} in realm {realmID} (salary: {salary})");
        }

        /// <summary>
        /// Remove an agent from their current office.
        /// </summary>
        public void RemoveAgentFromOffice(int agentID)
        {
            var agent = GetAgent(agentID);
            if (agent == null)
                return;

            agent.Data.RemoveFromOffice();
            OnAgentRemovedFromOffice?.Invoke(agentID);
            Debug.Log($"[AgentManager] Removed {agent.Data.AgentName} from office");
        }

        // ========== SALARY PAYMENTS ==========

        /// <summary>
        /// Pay salaries to all agents holding government offices.
        /// Called seasonally by RealmManager/WorldTimeManager.
        /// </summary>
        public void PayAllAgentSalaries()
        {
            foreach (var agent in _agents.Values)
            {
                if (agent.Data.HasOffice && agent.Data.MonthlySalary > 0)
                {
                    PayAgentSalary(agent.Data.AgentID);
                }
            }
        }

        /// <summary>
        /// Pay salary to a specific agent.
        /// </summary>
        private void PayAgentSalary(int agentID)
        {
            var agent = GetAgent(agentID);
            var ledger = GetAgentLedger(agentID);

            if (agent == null || ledger == null)
                return;

            if (!agent.Data.HasOffice)
                return;

            // Salary is already deducted from realm treasury by RealmTreasury.PayOfficeSalaries()
            // Here we just add it to the agent's personal ledger
            ledger.ReceiveSalary(agent.Data.MonthlySalary);
        }

        // ========== PROPERTY INCOME ==========

        /// <summary>
        /// Collect property income for all agents.
        /// Called daily/monthly.
        /// </summary>
        public void CollectAllPropertyIncome()
        {
            foreach (var ledger in _agentLedgers.Values)
            {
                ledger.CollectPropertyIncome();
            }
        }

        // ========== INHERITANCE ==========

        /// <summary>
        /// Handle agent death and distribute inheritance.
        /// </summary>
        public void ProcessAgentDeath(int deceasedAgentID, int heirAgentID)
        {
            var deceasedLedger = GetAgentLedger(deceasedAgentID);
            var heirLedger = GetAgentLedger(heirAgentID);

            if (deceasedLedger == null)
            {
                Debug.LogWarning($"[AgentManager] Cannot process death - deceased agent {deceasedAgentID} ledger not found");
                return;
            }

            // Calculate inheritance
            var inheritance = deceasedLedger.CalculateInheritance();

            // Transfer to heir if they exist
            if (heirLedger != null)
            {
                heirLedger.ReceiveInheritance(inheritance, deceasedAgentID);
            }

            // Clear deceased agent's wealth
            deceasedLedger.ClearWealth();

            // Mark agent as dead
            var agent = GetAgent(deceasedAgentID);
            if (agent != null)
            {
                agent.Data.IsAlive = false;

                // Remove from any office held
                if (agent.Data.HasOffice)
                {
                    RemoveAgentFromOffice(deceasedAgentID);
                }
            }

            Debug.Log($"[AgentManager] Processed death of agent {deceasedAgentID}, inheritance transferred to {heirAgentID}");
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Get total number of agents.
        /// </summary>
        public int GetAgentCount()
        {
            return _agents.Count;
        }

        /// <summary>
        /// Get number of living agents.
        /// </summary>
        public int GetLivingAgentCount()
        {
            int count = 0;
            foreach (var agent in _agents.Values)
            {
                if (agent.Data.IsAlive)
                    count++;
            }
            return count;
        }

        // ========== DEBUG ==========

        [ContextMenu("Print All Agents")]
        public void PrintAllAgents()
        {
            Debug.Log($"[AgentManager] Total Agents: {_agents.Count}");
            foreach (var agent in _agents.Values)
            {
                var ledger = GetAgentLedger(agent.Data.AgentID);
                int gold = ledger != null ? ledger.GetResource(TWK.Economy.ResourceType.Gold) : 0;

                Debug.Log($"  - {agent.Data.AgentName} (ID: {agent.Data.AgentID}), Age: {agent.Data.Age}, Gold: {gold}, Office: {agent.Data.HasOffice}");
            }
        }
    }
}
