using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;
using TWK.Government;

namespace TWK.Realms
{
    /// <summary>
    /// Manages a realm's treasury - the institutional wealth of the realm.
    /// Handles tax collection, tribute, and spending.
    /// Access controlled by government positions.
    /// </summary>
    public class RealmTreasury
    {
        private RealmData _realmData;
        private Dictionary<ResourceType, int> _resources;

        // ========== EVENTS ==========
        public event System.Action<int, ResourceType, int> OnTreasuryChanged; // realmID, resourceType, newAmount

        // ========== CONSTRUCTOR ==========
        public RealmTreasury(RealmData realmData)
        {
            _realmData = realmData;
            _resources = realmData.TreasuryResources;
        }

        // ========== RESOURCE ACCESS ==========

        /// <summary>
        /// Get amount of a resource in treasury.
        /// </summary>
        public int GetResource(ResourceType resourceType)
        {
            return _resources.ContainsKey(resourceType) ? _resources[resourceType] : 0;
        }

        /// <summary>
        /// Add resources to treasury.
        /// </summary>
        public void AddResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return;

            if (!_resources.ContainsKey(resourceType))
                _resources[resourceType] = 0;

            _resources[resourceType] += amount;
            OnTreasuryChanged?.Invoke(_realmData.RealmID, resourceType, _resources[resourceType]);
        }

        /// <summary>
        /// Remove resources from treasury.
        /// Returns true if successful, false if insufficient funds.
        /// </summary>
        public bool SpendResource(ResourceType resourceType, int amount, int spenderAgentID = -1)
        {
            if (amount <= 0) return false;

            // Check if realm has enough
            if (!HasResource(resourceType, amount))
                return false;

            // Check access control
            if (!CanAccessTreasury(spenderAgentID))
            {
                Debug.LogWarning($"[RealmTreasury] Agent {spenderAgentID} denied treasury access for realm {_realmData.RealmID}");
                return false;
            }

            _resources[resourceType] -= amount;
            OnTreasuryChanged?.Invoke(_realmData.RealmID, resourceType, _resources[resourceType]);
            return true;
        }

        /// <summary>
        /// Check if treasury has enough of a resource.
        /// </summary>
        public bool HasResource(ResourceType resourceType, int amount)
        {
            return GetResource(resourceType) >= amount;
        }

        /// <summary>
        /// Get all resources in treasury.
        /// </summary>
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_resources);
        }

        // ========== ACCESS CONTROL ==========

        /// <summary>
        /// Check if an agent can access the treasury.
        /// Access granted to:
        /// - Realm leaders (rulers)
        /// - Treasurer office holder
        /// - Autocratic rulers have unlimited access
        /// - Pluralist rulers may have restricted access
        /// </summary>
        public bool CanAccessTreasury(int agentID)
        {
            // -1 means system access (always allowed for tax collection, etc.)
            if (agentID == -1)
                return true;

            // Check if agent is a realm leader
            if (_realmData.LeaderIDs.Contains(agentID))
                return true;

            // Check government offices for treasurer
            if (GovernmentManager.Instance != null)
            {
                var government = GovernmentManager.Instance.GetRealmGovernment(_realmData.RealmID);
                var offices = GovernmentManager.Instance.GetRealmOffices(_realmData.RealmID);

                if (government != null && offices != null)
                {
                    foreach (var office in offices)
                    {
                        // Treasurer has access
                        if (office.OfficeName == "Treasurer" && office.AssignedAgentID == agentID)
                            return true;
                    }
                }
            }

            return false;
        }

        // ========== TAX COLLECTION ==========

        /// <summary>
        /// Collect taxes from all directly owned cities.
        /// 100% flows to realm treasury (no leakage).
        /// Called daily by RealmManager.
        /// </summary>
        public void CollectCityTaxes()
        {
            if (_realmData.DirectlyOwnedCityIDs == null || _realmData.DirectlyOwnedCityIDs.Count == 0)
                return;

            foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
            {
                // Get city tax based on government taxation law
                int taxAmount = CalculateCityTax(cityID);

                if (taxAmount > 0)
                {
                    // Deduct from city
                    if (ResourceManager.Instance.SpendResource(cityID, ResourceType.Gold, taxAmount))
                    {
                        // Add to realm treasury (100%, no leakage)
                        AddResource(ResourceType.Gold, taxAmount);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate tax amount for a city based on government taxation law.
        /// </summary>
        private int CalculateCityTax(int cityID)
        {
            // Get government taxation law
            if (GovernmentManager.Instance == null)
                return 0;

            var government = GovernmentManager.Instance.GetRealmGovernment(_realmData.RealmID);
            if (government == null)
                return 0;

            // Get city gold
            int cityGold = ResourceManager.Instance.GetResource(cityID, ResourceType.Gold);

            // Calculate tax based on taxation law
            float taxRate = GetTaxRate(government.TaxationLaw);
            int taxAmount = Mathf.FloorToInt(cityGold * taxRate);

            return taxAmount;
        }

        /// <summary>
        /// Get tax rate from taxation law.
        /// Test for logic concerning different taxation laws. Will add more laws later
        /// </summary>
        private float GetTaxRate(TaxationLaw taxLaw)
        {
            switch (taxLaw)
            {
                case TaxationLaw.Tribute:
                    return .9f;
                default:
                    return 1f;
            }
        }

        // ========== TRIBUTE COLLECTION ==========

        /// <summary>
        /// Collect tribute from all vassal realms.
        /// Amount based on contract percentage (tax leakage).
        /// Called daily by RealmManager.
        /// </summary>
        public void CollectVassalTribute()
        {
            if (_realmData.VassalContractIDs == null || _realmData.VassalContractIDs.Count == 0)
                return;

            if (ContractManager.Instance == null)
                return;

            foreach (int contractID in _realmData.VassalContractIDs)
            {
                var contract = ContractManager.Instance.GetContract(contractID);
                if (contract == null)
                    continue;

                // Get subject realm
                var subjectRealm = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
                if (subjectRealm == null)
                    continue;

                // Calculate tribute amount (percentage of subject's gold)
                int subjectGold = subjectRealm.Treasury.GetResource(ResourceType.Gold);
                float tributePercentage = contract.GoldPercentage / 100f; // Contract stores as percentage (0-100)
                int tributeAmount = Mathf.FloorToInt(subjectGold * tributePercentage);

                if (tributeAmount > 0)
                {
                    // Deduct from subject treasury
                    if (subjectRealm.Treasury.SpendResource(ResourceType.Gold, tributeAmount, -1))
                    {
                        // Add to overlord treasury
                        AddResource(ResourceType.Gold, tributeAmount);

                        Debug.Log($"[RealmTreasury] Realm {_realmData.RealmID} collected {tributeAmount} gold tribute from vassal {contract.SubjectRealmID} (rate: {contract.GoldPercentage}%)");
                    }
                }
            }
        }

        // ========== SPENDING ==========

        /// <summary>
        /// Pay for an edict from realm treasury.
        /// </summary>
        public bool PayForEdict(Edict edict, int enactorAgentID)
        {
            if (edict == null || edict.EnactmentCost <= 0)
                return true;

            return SpendResource(ResourceType.Gold, edict.EnactedOnMonth, enactorAgentID);
        }

        /// <summary>
        /// Pay for a government reform from realm treasury.
        /// </summary>
        public bool PayForReform(int goldCost, int enactorAgentID)
        {
            if (goldCost <= 0)
                return true;

            return SpendResource(ResourceType.Gold, goldCost, enactorAgentID);
        }

        /// <summary>
        /// Pay office salaries to all office holders.
        /// Called seasonally (4 times per year).
        /// </summary>
        public void PayOfficeSalaries()
        {
            if (GovernmentManager.Instance == null)
                return;

            var government = GovernmentManager.Instance.GetRealmGovernment(_realmData.RealmID);
            var offices = GovernmentManager.Instance.GetRealmOffices(_realmData.RealmID);

            if (government == null || offices == null)
                return;

            foreach (var office in offices)
            {
                if (office.AssignedAgentID != -1 && office.MonthlySalary > 0)
                {
                    // Try to pay salary from realm treasury
                    if (SpendResource(ResourceType.Gold, office.MonthlySalary, -1))
                    {
                        // Transfer to agent's personal ledger via AgentManager
                        if (TWK.Agents.AgentManager.Instance != null)
                        {
                            var ledger = TWK.Agents.AgentManager.Instance.GetAgentLedger(office.AssignedAgentID);
                            if (ledger != null)
                            {
                                ledger.ReceiveSalary(office.MonthlySalary);
                            }
                            else
                            {
                                Debug.LogWarning($"[RealmTreasury] Could not find ledger for agent {office.AssignedAgentID} to pay salary for {office.OfficeName}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[RealmTreasury] Insufficient funds to pay salary for {office.OfficeName} (agent {office.AssignedAgentID})");
                    }
                }
            }
        }
    }
}
