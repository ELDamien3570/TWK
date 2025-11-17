using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;

namespace TWK.Agents
{
    /// <summary>
    /// Manages an agent's personal wealth and resources.
    /// Separate from realm treasury - this is the character's private money.
    /// Sources: salaries, estates, businesses, inheritance, gifts, bribes.
    /// Uses: personal projects, estate upgrades, gifts, bribes.
    /// </summary>
    public class AgentLedger
    {
        private AgentData _agentData;
        private Dictionary<ResourceType, int> _personalWealth;

        // ========== EVENTS ==========
        public event System.Action<int, ResourceType, int> OnWealthChanged; // agentID, resourceType, newAmount

        // ========== CONSTRUCTOR ==========
        public AgentLedger(AgentData agentData)
        {
            _agentData = agentData;
            _personalWealth = agentData.PersonalWealth;

            // Initialize with starting wealth if new agent
            if (_personalWealth.Count == 0)
            {
                _personalWealth[ResourceType.Gold] = 100; // Starting gold
            }
        }

        // ========== RESOURCE ACCESS ==========

        /// <summary>
        /// Get amount of a resource in personal wealth.
        /// </summary>
        public int GetResource(ResourceType resourceType)
        {
            return _personalWealth.ContainsKey(resourceType) ? _personalWealth[resourceType] : 0;
        }

        /// <summary>
        /// Add resources to personal wealth.
        /// </summary>
        public void AddResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return;

            if (!_personalWealth.ContainsKey(resourceType))
                _personalWealth[resourceType] = 0;

            _personalWealth[resourceType] += amount;
            OnWealthChanged?.Invoke(_agentData.AgentID, resourceType, _personalWealth[resourceType]);
        }

        /// <summary>
        /// Spend resources from personal wealth.
        /// Returns true if successful, false if insufficient funds.
        /// </summary>
        public bool SpendResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return false;

            if (!HasResource(resourceType, amount))
                return false;

            _personalWealth[resourceType] -= amount;
            OnWealthChanged?.Invoke(_agentData.AgentID, resourceType, _personalWealth[resourceType]);
            return true;
        }

        /// <summary>
        /// Check if agent has enough of a resource.
        /// </summary>
        public bool HasResource(ResourceType resourceType, int amount)
        {
            return GetResource(resourceType) >= amount;
        }

        /// <summary>
        /// Get all resources in personal wealth.
        /// </summary>
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_personalWealth);
        }

        // ========== INCOME SOURCES ==========

        /// <summary>
        /// Receive salary payment (called by RealmTreasury).
        /// </summary>
        public void ReceiveSalary(int amount)
        {
            if (amount > 0)
            {
                AddResource(ResourceType.Gold, amount);
                Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} ({_agentData.AgentID}) received salary: {amount} gold");
            }
        }

        /// <summary>
        /// Receive income from owned estates and businesses.
        /// Called daily/monthly.
        /// </summary>
        public void CollectPropertyIncome()
        {
            // TODO: Implement when agent-owned buildings are functional
            // For now, placeholder for future implementation

            int totalIncome = 0;

            // Estates generate passive income
            foreach (int buildingID in _agentData.OwnedBuildingIDs)
            {
                // Get building, calculate income based on building type
                // totalIncome += building.DailyIncome;
            }

            // Caravans generate trade income
            foreach (int caravanID in _agentData.OwnedCaravanIDs)
            {
                // Get caravan, calculate profit from trade
                // totalIncome += caravan.TradeProfit;
            }

            if (totalIncome > 0)
            {
                AddResource(ResourceType.Gold, totalIncome);
                Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} collected {totalIncome} gold from properties");
            }
        }

        /// <summary>
        /// Receive a gift from another agent.
        /// </summary>
        public void ReceiveGift(ResourceType resourceType, int amount, int fromAgentID)
        {
            if (amount > 0)
            {
                AddResource(resourceType, amount);
                Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} received gift: {amount} {resourceType} from agent {fromAgentID}");
            }
        }

        /// <summary>
        /// Receive inheritance from a deceased agent.
        /// </summary>
        public void ReceiveInheritance(Dictionary<ResourceType, int> inheritance, int fromAgentID)
        {
            int totalGold = 0;
            foreach (var kvp in inheritance)
            {
                AddResource(kvp.Key, kvp.Value);
                if (kvp.Key == ResourceType.Gold)
                    totalGold = kvp.Value;
            }

            Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} inherited {totalGold} gold from agent {fromAgentID}");
        }

        // ========== SPENDING ==========

        /// <summary>
        /// Purchase an estate or business.
        /// </summary>
        public bool PurchaseProperty(int cost)
        {
            if (SpendResource(ResourceType.Gold, cost))
            {
                Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} purchased property for {cost} gold");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Give a gift to another agent.
        /// </summary>
        public bool GiveGift(ResourceType resourceType, int amount, AgentLedger recipient)
        {
            if (SpendResource(resourceType, amount))
            {
                recipient.ReceiveGift(resourceType, amount, _agentData.AgentID);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pay a bribe (influence action).
        /// </summary>
        public bool PayBribe(int amount)
        {
            if (SpendResource(ResourceType.Gold, amount))
            {
                Debug.Log($"[AgentLedger] Agent {_agentData.AgentName} paid bribe: {amount} gold");
                return true;
            }

            return false;
        }

        // ========== INHERITANCE ==========

        /// <summary>
        /// Calculate inheritance to distribute upon death.
        /// Returns all personal wealth.
        /// </summary>
        public Dictionary<ResourceType, int> CalculateInheritance()
        {
            return new Dictionary<ResourceType, int>(_personalWealth);
        }

        /// <summary>
        /// Clear all wealth (upon death/distribution).
        /// </summary>
        public void ClearWealth()
        {
            _personalWealth.Clear();
        }

        // ========== WEALTH STATUS ==========

        /// <summary>
        /// Get wealth status as a string.
        /// </summary>
        public string GetWealthStatus()
        {
            int gold = GetResource(ResourceType.Gold);

            if (gold >= 10000) return "Wealthy";
            if (gold >= 5000) return "Prosperous";
            if (gold >= 1000) return "Comfortable";
            if (gold >= 100) return "Modest";
            return "Poor";
        }

        /// <summary>
        /// Can this agent afford a specific cost?
        /// </summary>
        public bool CanAfford(ResourceType resourceType, int amount)
        {
            return HasResource(resourceType, amount);
        }
    }
}
