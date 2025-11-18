using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Government;

namespace TWK.Agents
{
    /// <summary>
    /// Singleton manager for all relationships between agents.
    /// Centralized storage prevents duplication and desyncing.
    /// Automatically creates relationships when contracts are formed.
    /// </summary>
    public class RelationshipManager : MonoBehaviour
    {
        public static RelationshipManager Instance { get; private set; }

        // ========== RELATIONSHIP STORAGE ==========
        /// <summary>
        /// All relationships by ID.
        /// </summary>
        private Dictionary<int, RelationshipData> _relationships = new Dictionary<int, RelationshipData>();

        /// <summary>
        /// Relationships indexed by agent.
        /// Key: AgentID, Value: List of relationship IDs involving this agent.
        /// </summary>
        private Dictionary<int, List<int>> _relationshipsByAgent = new Dictionary<int, List<int>>();

        /// <summary>
        /// Relationships indexed by contract.
        /// Key: ContractID, Value: Relationship ID.
        /// </summary>
        private Dictionary<int, int> _relationshipsByContract = new Dictionary<int, int>();

        private int _nextRelationshipID = 1;

        // ========== EVENTS ==========
        public event Action<RelationshipData> OnRelationshipFormed;
        public event Action<RelationshipData> OnRelationshipEnded;
        public event Action<RelationshipData, float> OnRelationshipStrengthChanged;

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

        private void Start()
        {
            // Subscribe to contract events for automatic relationship creation
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated += HandleContractCreated;
                ContractManager.Instance.OnContractTerminated += HandleContractTerminated;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Unsubscribe from contract events
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated -= HandleContractCreated;
                ContractManager.Instance.OnContractTerminated -= HandleContractTerminated;
            }
        }

        // ========== CONTRACT INTEGRATION ==========

        /// <summary>
        /// Automatically create relationships when contracts are formed.
        /// </summary>
        private void HandleContractCreated(Contract contract)
        {
            // Only create relationships for contracts involving agents
            if (contract.SubjectAgentID == -1)
                return; // Realm-to-realm contracts don't create agent relationships (yet)

            int overlordAgentID = GetRealmRulerAgentID(contract.ParentRealmID);
            if (overlordAgentID == -1)
            {
                Debug.LogWarning($"[RelationshipManager] Cannot create contract relationship - parent realm {contract.ParentRealmID} has no ruler");
                return;
            }

            float initialStrength = 50f; // Neutral starting loyalty

            // Determine relationship type based on contract type
            RelationshipType relType = contract.Type switch
            {
                ContractType.Governor => RelationshipType.Employer,
                ContractType.Warband => RelationshipType.Employer,
                _ => RelationshipType.Liege // Vassal/Tributary
            };

            // Create Liege relationship (subject -> overlord)
            var liegeRel = FormRelationship(
                contract.SubjectAgentID,
                overlordAgentID,
                relType == RelationshipType.Employer ? RelationshipType.Employee : RelationshipType.Liege,
                initialStrength,
                contract.ContractID
            );

            // Create Vassal/Employer relationship (overlord -> subject)
            var vassalRel = FormRelationship(
                overlordAgentID,
                contract.SubjectAgentID,
                relType,
                initialStrength,
                contract.ContractID
            );

            Debug.Log($"[RelationshipManager] Created contract relationships for Contract {contract.ContractID}");
        }

        /// <summary>
        /// End relationships when contracts are terminated.
        /// </summary>
        private void HandleContractTerminated(Contract contract)
        {
            if (_relationshipsByContract.TryGetValue(contract.ContractID, out int relationshipID))
            {
                EndRelationship(relationshipID);
            }
        }

        /// <summary>
        /// Get the ruler agent ID for a realm.
        /// TODO: This should query RealmData.LeaderIDs when that's implemented.
        /// </summary>
        private int GetRealmRulerAgentID(int realmID)
        {
            // Temporary implementation - returns first agent marked as ruler in realm
            var agents = AgentManager.Instance?.GetAgentsByRealm(realmID);
            if (agents != null)
            {
                foreach (var agent in agents)
                {
                    if (agent.Data.IsRuler)
                        return agent.Data.AgentID;
                }
            }
            return -1;
        }

        // ========== RELATIONSHIP MANAGEMENT ==========

        /// <summary>
        /// Form a new relationship between two agents.
        /// </summary>
        public RelationshipData FormRelationship(
            int agent1ID,
            int agent2ID,
            RelationshipType type,
            float initialStrength = 0f,
            int contractID = -1)
        {
            // Validate agents exist
            if (AgentManager.Instance?.GetAgent(agent1ID) == null ||
                AgentManager.Instance?.GetAgent(agent2ID) == null)
            {
                Debug.LogError($"[RelationshipManager] Cannot form relationship - agents {agent1ID} or {agent2ID} not found");
                return null;
            }

            // Check if relationship already exists
            var existing = GetRelationship(agent1ID, agent2ID, type);
            if (existing != null && existing.IsActive)
            {
                Debug.LogWarning($"[RelationshipManager] Relationship already exists between {agent1ID} and {agent2ID}");
                return existing;
            }

            // Create new relationship
            var relationship = new RelationshipData(_nextRelationshipID, type, agent1ID, agent2ID, initialStrength);
            relationship.ContractID = contractID;
            // TODO: Set FormedDay/Year from WorldTimeManager

            _relationships[relationship.RelationshipID] = relationship;

            // Index by agents
            AddToAgentIndex(agent1ID, relationship.RelationshipID);
            AddToAgentIndex(agent2ID, relationship.RelationshipID);

            // Index by contract if applicable
            if (contractID != -1)
            {
                _relationshipsByContract[contractID] = relationship.RelationshipID;
            }

            // Update agent's relationship lists for fast UI queries
            UpdateAgentRelationshipLists(agent1ID, agent2ID, type, add: true);

            _nextRelationshipID++;

            OnRelationshipFormed?.Invoke(relationship);
            Debug.Log($"[RelationshipManager] Formed {type} relationship between agents {agent1ID} and {agent2ID} (strength: {initialStrength})");

            return relationship;
        }

        /// <summary>
        /// End a relationship.
        /// </summary>
        public void EndRelationship(int relationshipID)
        {
            if (!_relationships.TryGetValue(relationshipID, out var relationship))
                return;

            relationship.IsActive = false;

            // Update agent's relationship lists
            UpdateAgentRelationshipLists(relationship.Agent1ID, relationship.Agent2ID, relationship.Type, add: false);

            // Remove from contract index
            if (relationship.ContractID != -1)
            {
                _relationshipsByContract.Remove(relationship.ContractID);
            }

            OnRelationshipEnded?.Invoke(relationship);
            Debug.Log($"[RelationshipManager] Ended {relationship.Type} relationship {relationshipID}");
        }

        /// <summary>
        /// Modify relationship strength.
        /// </summary>
        public void ModifyRelationshipStrength(int relationshipID, float amount)
        {
            if (!_relationships.TryGetValue(relationshipID, out var relationship))
                return;

            float oldStrength = relationship.Strength;
            relationship.Strength += amount;
            relationship.Strength = Mathf.Clamp(relationship.Strength, -100f, 100f);

            OnRelationshipStrengthChanged?.Invoke(relationship, relationship.Strength - oldStrength);

            // Update contract loyalty if this is a vassal relationship
            if (relationship.ContractID != -1 && ContractManager.Instance != null)
            {
                var contract = ContractManager.Instance.GetContract(relationship.ContractID);
                if (contract != null)
                {
                    contract.CurrentLoyalty = Mathf.Clamp(relationship.Strength, 0f, 100f);
                }
            }
        }

        // ========== QUERIES ==========

        /// <summary>
        /// Get all relationships for an agent (both active and inactive).
        /// </summary>
        public List<RelationshipData> GetAgentRelationships(int agentID, bool activeOnly = true)
        {
            var result = new List<RelationshipData>();

            if (!_relationshipsByAgent.TryGetValue(agentID, out var relationshipIDs))
                return result;

            foreach (int relID in relationshipIDs)
            {
                if (_relationships.TryGetValue(relID, out var relationship))
                {
                    if (!activeOnly || relationship.IsActive)
                    {
                        result.Add(relationship);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get a specific relationship between two agents.
        /// </summary>
        public RelationshipData GetRelationship(int agent1ID, int agent2ID, RelationshipType type)
        {
            if (!_relationshipsByAgent.TryGetValue(agent1ID, out var relationshipIDs))
                return null;

            foreach (int relID in relationshipIDs)
            {
                if (_relationships.TryGetValue(relID, out var relationship))
                {
                    if (relationship.Type == type &&
                        ((relationship.Agent1ID == agent1ID && relationship.Agent2ID == agent2ID) ||
                         (relationship.Agent1ID == agent2ID && relationship.Agent2ID == agent1ID)))
                    {
                        return relationship;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get relationship by ID.
        /// </summary>
        public RelationshipData GetRelationship(int relationshipID)
        {
            return _relationships.TryGetValue(relationshipID, out var relationship) ? relationship : null;
        }

        /// <summary>
        /// Get relationship linked to a contract.
        /// </summary>
        public RelationshipData GetRelationshipByContract(int contractID)
        {
            if (_relationshipsByContract.TryGetValue(contractID, out int relationshipID))
            {
                return GetRelationship(relationshipID);
            }
            return null;
        }

        // ========== INTERNAL HELPERS ==========

        private void AddToAgentIndex(int agentID, int relationshipID)
        {
            if (!_relationshipsByAgent.ContainsKey(agentID))
            {
                _relationshipsByAgent[agentID] = new List<int>();
            }
            _relationshipsByAgent[agentID].Add(relationshipID);
        }

        /// <summary>
        /// Update AgentData's relationship ID lists for fast UI queries.
        /// This keeps the legacy lists in sync with the centralized relationship system.
        /// </summary>
        private void UpdateAgentRelationshipLists(int agent1ID, int agent2ID, RelationshipType type, bool add)
        {
            var agent1 = AgentManager.Instance?.GetAgent(agent1ID);
            var agent2 = AgentManager.Instance?.GetAgent(agent2ID);

            if (agent1 == null || agent2 == null)
                return;

            // Update based on relationship type
            switch (type)
            {
                case RelationshipType.Parent:
                    // agent1 is parent of agent2
                    if (add)
                    {
                        agent1.Data.AddChild(agent2ID);
                        if (agent1.Data.Gender == Gender.Female)
                            agent2.Data.MotherID = agent1ID;
                        else
                            agent2.Data.FatherID = agent1ID;
                    }
                    else
                    {
                        agent1.Data.RemoveChild(agent2ID);
                    }
                    break;

                case RelationshipType.Child:
                    // agent1 is child of agent2
                    if (add)
                    {
                        agent2.Data.AddChild(agent1ID);
                        if (agent2.Data.Gender == Gender.Female)
                            agent1.Data.MotherID = agent2ID;
                        else
                            agent1.Data.FatherID = agent2ID;
                    }
                    else
                    {
                        agent2.Data.RemoveChild(agent1ID);
                    }
                    break;

                case RelationshipType.Spouse:
                    if (add)
                    {
                        agent1.Data.AddSpouse(agent2ID);
                        agent2.Data.AddSpouse(agent1ID);
                    }
                    else
                    {
                        agent1.Data.RemoveSpouse(agent2ID);
                        agent2.Data.RemoveSpouse(agent1ID);
                    }
                    break;

                case RelationshipType.Lover:
                    if (add)
                    {
                        agent1.Data.AddLover(agent2ID);
                        agent2.Data.AddLover(agent1ID);
                    }
                    else
                    {
                        agent1.Data.RemoveLover(agent2ID);
                        agent2.Data.RemoveLover(agent1ID);
                    }
                    break;

                case RelationshipType.Friend:
                    if (add)
                    {
                        agent1.Data.AddFriend(agent2ID);
                        agent2.Data.AddFriend(agent1ID);
                    }
                    else
                    {
                        agent1.Data.RemoveFriend(agent2ID);
                        agent2.Data.RemoveFriend(agent1ID);
                    }
                    break;

                case RelationshipType.Rival:
                    if (add)
                    {
                        agent1.Data.AddRival(agent2ID);
                        agent2.Data.AddRival(agent1ID);
                    }
                    else
                    {
                        agent1.Data.RemoveRival(agent2ID);
                        agent2.Data.RemoveRival(agent1ID);
                    }
                    break;

                case RelationshipType.Companion:
                    if (add)
                        agent1.Data.AddCompanion(agent2ID);
                    else
                        agent1.Data.RemoveCompanion(agent2ID);
                    break;

                // Vassal/Liege relationships don't have ID lists in AgentData
                // They're tracked through RelationshipManager only
            }
        }

        // ========== DEBUG ==========

        [ContextMenu("Print All Relationships")]
        public void PrintAllRelationships()
        {
            Debug.Log($"[RelationshipManager] Total Relationships: {_relationships.Count}");
            foreach (var relationship in _relationships.Values)
            {
                var agent1 = AgentManager.Instance?.GetAgent(relationship.Agent1ID);
                var agent2 = AgentManager.Instance?.GetAgent(relationship.Agent2ID);

                string agent1Name = agent1?.Data.AgentName ?? "Unknown";
                string agent2Name = agent2?.Data.AgentName ?? "Unknown";

                Debug.Log($"  - {relationship.Type}: {agent1Name} -> {agent2Name} " +
                         $"(Strength: {relationship.Strength:F1}, Active: {relationship.IsActive}, " +
                         $"Contract: {relationship.ContractID})");
            }
        }
    }
}
