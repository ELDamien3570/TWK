using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Core;
using TWK.Simulation;
using TWK.Economy;
using TWK.Realms;

namespace TWK.Government
{
    /// <summary>
    /// Manages all contracts between realms and agents.
    /// Handles vassalage, governorships, tributes, warbands, and resource obligations.
    /// </summary>
    public class ContractManager : MonoBehaviour, ISimulationAgent
    {
        public static ContractManager Instance { get; private set; }

        // ========== CONTRACTS ==========
        /// <summary>
        /// All active contracts.
        /// Key: Contract ID
        /// </summary>
        private Dictionary<int, Contract> activeContracts = new Dictionary<int, Contract>();

        /// <summary>
        /// Contracts indexed by parent realm.
        /// Key: Parent Realm ID, Value: List of Contract IDs
        /// </summary>
        private Dictionary<int, List<int>> contractsByParent = new Dictionary<int, List<int>>();

        /// <summary>
        /// Contracts indexed by subject realm.
        /// Key: Subject Realm ID, Value: List of Contract IDs
        /// </summary>
        private Dictionary<int, List<int>> contractsBySubject = new Dictionary<int, List<int>>();

        /// <summary>
        /// Next available contract ID.
        /// </summary>
        private int nextContractID = 1;

        // ========== TIME TRACKING ==========
        private WorldTimeManager worldTimeManager;
        private int currentMonthCounter = 0;

        // ========== EVENTS ==========
        public event Action<Contract> OnContractCreated;
        public event Action<Contract> OnContractTerminated;
        public event Action<Contract, float> OnLoyaltyChanged;
        public event Action<Contract> OnContractBreach;

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

        public void Initialize(WorldTimeManager timeManager)
        {
            worldTimeManager = timeManager;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;

            Debug.Log("[ContractManager] Initialized");
        }

        private void OnDestroy()
        {
            if (worldTimeManager != null)
            {
                worldTimeManager.OnSeasonTick -= AdvanceSeason;
                worldTimeManager.OnYearTick -= AdvanceYear;
            }
        }

        // ========== CONTRACT CREATION ==========

        /// <summary>
        /// Create a new contract.
        /// </summary>
        public Contract CreateContract(int parentRealmID, ContractType type)
        {
            var contract = new Contract(nextContractID, parentRealmID, type);
            contract.CreatedOnMonth = currentMonthCounter;

            // Initialize with default terms based on contract type
            InitializeDefaultTerms(contract);

            activeContracts[contract.ContractID] = contract;

            // Index by parent
            if (!contractsByParent.ContainsKey(parentRealmID))
            {
                contractsByParent[parentRealmID] = new List<int>();
            }
            contractsByParent[parentRealmID].Add(contract.ContractID);

            nextContractID++;

            OnContractCreated?.Invoke(contract);
            Debug.Log($"[ContractManager] Created {type} contract (ID: {contract.ContractID}) for parent realm {parentRealmID}");

            return contract;
        }

        /// <summary>
        /// Initialize default terms based on contract type.
        /// </summary>
        private void InitializeDefaultTerms(Contract contract)
        {
            switch (contract.Type)
            {
                case ContractType.Governor:
                    // Governor contracts: low resource obligations, high manpower control
                    contract.GoldPercentage = 15f;
                    contract.ManpowerPercentage = 100f; // Full military control
                    contract.AllowSubjectGovernment = false; // Governor represents parent
                    contract.ParentControlsBureaucracy = true;
                    break;

                case ContractType.Vassal:
                    // Vassal contracts: moderate obligations, retain some autonomy
                    contract.GoldPercentage = 25f;
                    contract.ManpowerPercentage = 50f;
                    contract.AllowSubjectGovernment = true;
                    contract.CanReformGovernment = false; // Need parent approval
                    contract.ParentControlsBureaucracy = false;
                    break;

                case ContractType.Tributary:
                    // Tributary contracts: tribute-based, very light obligations
                    contract.GoldPercentage = 10f;
                    contract.ManpowerPercentage = 25f;
                    contract.AllowSubjectGovernment = true;
                    contract.CanReformGovernment = true; // Full autonomy
                    contract.ParentControlsBureaucracy = false;
                    break;

                case ContractType.Warband:
                    // Warband contracts: military service for payment
                    contract.GoldPercentage = 0f; // They receive payment instead
                    contract.ManpowerPercentage = 100f; // Full deployment control
                    contract.MonthlyGoldSubsidy = 50; // Parent pays them
                    contract.ContractDurationMonths = 24; // 2 year contracts
                    break;
            }

            // Calculate initial loyalty
            contract.UpdateLoyalty();
        }

        /// <summary>
        /// Set contract subject (realm or agent).
        /// </summary>
        public void SetContractSubject(Contract contract, int subjectRealmID = -1, int subjectAgentID = -1)
        {
            if (contract == null)
            {
                Debug.LogError("[ContractManager] Cannot set subject on null contract");
                return;
            }

            contract.SubjectRealmID = subjectRealmID;
            contract.SubjectAgentID = subjectAgentID;

            // Index by subject realm if applicable
            if (subjectRealmID != -1)
            {
                if (!contractsBySubject.ContainsKey(subjectRealmID))
                {
                    contractsBySubject[subjectRealmID] = new List<int>();
                }
                contractsBySubject[subjectRealmID].Add(contract.ContractID);
            }

            Debug.Log($"[ContractManager] Contract {contract.ContractID} subject set: Realm={subjectRealmID}, Agent={subjectAgentID}");
        }

        // ========== CONTRACT QUERIES ==========

        /// <summary>
        /// Get a contract by ID.
        /// </summary>
        public Contract GetContract(int contractID)
        {
            if (activeContracts.TryGetValue(contractID, out var contract))
                return contract;

            return null;
        }

        /// <summary>
        /// Get all active contracts.
        /// </summary>
        public List<Contract> GetAllContracts()
        {
            return new List<Contract>(activeContracts.Values);
        }

        /// <summary>
        /// Get all contracts where a realm is the parent.
        /// </summary>
        public List<Contract> GetContractsAsParent(int realmID)
        {
            if (!contractsByParent.TryGetValue(realmID, out var contractIDs))
                return new List<Contract>();

            return contractIDs
                .Select(id => GetContract(id))
                .Where(c => c != null)
                .ToList();
        }

        /// <summary>
        /// Get all contracts where a realm is the subject.
        /// </summary>
        public List<Contract> GetContractsAsSubject(int realmID)
        {
            if (!contractsBySubject.TryGetValue(realmID, out var contractIDs))
                return new List<Contract>();

            return contractIDs
                .Select(id => GetContract(id))
                .Where(c => c != null)
                .ToList();
        }

        /// <summary>
        /// Get contracts by type.
        /// </summary>
        public List<Contract> GetContractsByType(ContractType type)
        {
            return activeContracts.Values
                .Where(c => c.Type == type)
                .ToList();
        }

        // ========== CONTRACT MODIFICATION ==========

        /// <summary>
        /// Update contract resource percentages.
        /// </summary>
        public void UpdateContractTerms(Contract contract, Dictionary<ResourceType, float> newPercentages)
        {
            if (contract == null)
            {
                Debug.LogError("[ContractManager] Cannot update null contract");
                return;
            }

            foreach (var kvp in newPercentages)
            {
                contract.SetResourcePercentage(kvp.Key, kvp.Value);
            }

            RecalculateContractLoyalty(contract);
            Debug.Log($"[ContractManager] Contract {contract.ContractID} terms updated");
        }

        /// <summary>
        /// Update contract manpower percentage.
        /// </summary>
        public void UpdateManpowerObligation(Contract contract, float percentage)
        {
            if (contract == null)
            {
                Debug.LogError("[ContractManager] Cannot update null contract");
                return;
            }

            contract.ManpowerPercentage = Mathf.Clamp(percentage, 0f, 100f);
            RecalculateContractLoyalty(contract);
        }

        /// <summary>
        /// Recalculate loyalty based on contract fairness.
        /// </summary>
        private void RecalculateContractLoyalty(Contract contract)
        {
            float oldLoyalty = contract.CurrentLoyalty;
            contract.UpdateLoyalty();

            if (Mathf.Abs(contract.CurrentLoyalty - oldLoyalty) > 0.1f)
            {
                OnLoyaltyChanged?.Invoke(contract, contract.CurrentLoyalty);
                Debug.Log($"[ContractManager] Contract {contract.ContractID} loyalty changed from {oldLoyalty:F1} to {contract.CurrentLoyalty:F1}");
            }
        }

        // ========== TAX/TRIBUTE COLLECTION ==========

        /// <summary>
        /// Process monthly tax collection from all contracts.
        /// </summary>
        private void ProcessTaxCollection()
        {
            foreach (var contract in activeContracts.Values)
            {
                if (contract.SubjectRealmID == -1)
                    continue; // No subject assigned yet

                CollectFromContract(contract);
            }
        }

        /// <summary>
        /// Collect resources from a specific contract.
        /// </summary>
        private void CollectFromContract(Contract contract)
        {
            var government = GovernmentManager.Instance?.GetRealmGovernment(contract.ParentRealmID);
            if (government == null)
            {
                Debug.LogWarning($"[ContractManager] Parent realm {contract.ParentRealmID} has no government");
                return;
            }

            // Collect based on taxation law
            switch (government.TaxationLaw)
            {
                case TaxationLaw.Tribute:
                    CollectTribute(contract);
                    break;

                case TaxationLaw.TaxCollectors:
                    CollectTaxes(contract);
                    break;
            }

            // Process subsidies (parent to subject)
            ProcessSubsidies(contract);
        }

        private void CollectTribute(Contract contract)
        {
            // TODO: Create tribute caravan system
            // Caravans need armed escort and are subject to bandit attacks
            // For now, just log the collection

            Debug.Log($"[ContractManager] Tribute caravan sent from contract {contract.ContractID} (subject: {contract.SubjectRealmID})");

            // Simplified: Direct transfer with high leakage due to caravan risks
            float leakageRate = 0.3f; // 30% lost to raiders, corruption, etc.
            TransferResources(contract, leakageRate);
        }

        private void CollectTaxes(Contract contract)
        {
            // Administrative tax collection based on parent capacity
            float capacity = GovernmentManager.Instance?.GetRealmCapacity(contract.ParentRealmID) ?? 30f;

            // Leakage based on capacity: 0% capacity = 50% leakage, 100% capacity = 10% leakage
            float leakageRate = 0.5f - (capacity / 100f * 0.4f);

            Debug.Log($"[ContractManager] Tax collectors processed contract {contract.ContractID} (leakage: {leakageRate * 100:F0}%)");

            TransferResources(contract, leakageRate);
        }

        private void TransferResources(Contract contract, float leakageRate)
        {
            // Get subject and parent realms
            var subjectRealm = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
            var parentRealm = RealmManager.Instance?.GetRealm(contract.ParentRealmID);

            if (subjectRealm == null || parentRealm == null)
            {
                Debug.LogWarning($"[ContractManager] Cannot transfer resources: Subject or parent realm not found (Contract {contract.ContractID})");
                return;
            }

            var resourceTypes = new[] { ResourceType.Food, ResourceType.Gold, ResourceType.Piety, ResourceType.Prestige };
            int totalTransferred = 0;

            foreach (var resourceType in resourceTypes)
            {
                float percentage = contract.GetResourcePercentage(resourceType);
                if (percentage <= 0f)
                    continue;

                // Get subject's current resources
                int subjectAmount = subjectRealm.Treasury.GetResource(resourceType);
                if (subjectAmount <= 0)
                    continue;

                // Calculate transfer amount
                float rawPercentage = percentage / 100f;
                int rawAmount = Mathf.FloorToInt(subjectAmount * rawPercentage);

                if (rawAmount <= 0)
                    continue;

                // Apply leakage (corruption, raiders, administrative inefficiency)
                int leakedAmount = Mathf.FloorToInt(rawAmount * leakageRate);
                int transferAmount = rawAmount - leakedAmount;

                if (transferAmount <= 0)
                    continue;

                // Execute transfer: Subject -> Parent
                if (subjectRealm.Treasury.SpendResource(resourceType, transferAmount, -1))
                {
                    parentRealm.Treasury.AddResource(resourceType, transferAmount);
                    totalTransferred++;

                    Debug.Log($"[ContractManager] Contract {contract.ContractID}: Transferred {transferAmount} {resourceType} from Realm {contract.SubjectRealmID} to {contract.ParentRealmID} (rate: {percentage:F0}%, leakage: {leakedAmount})");
                }
                else
                {
                    Debug.LogWarning($"[ContractManager] Contract {contract.ContractID}: Failed to deduct {transferAmount} {resourceType} from subject realm {contract.SubjectRealmID}");
                }
            }

            if (totalTransferred > 0)
            {
                Debug.Log($"[ContractManager] Contract {contract.ContractID}: Completed resource transfer ({totalTransferred} resource types transferred, {leakageRate * 100:F0}% leakage)");
            }
        }

        private void ProcessSubsidies(Contract contract)
        {
            if (contract.MonthlyGoldSubsidy <= 0)
                return;

            // TODO: Transfer gold from parent to subject
            Debug.Log($"[ContractManager] Contract {contract.ContractID}: Parent pays {contract.MonthlyGoldSubsidy} gold subsidy to subject");
        }

        // ========== CONTRACT TERMINATION ==========

        /// <summary>
        /// Terminate a contract.
        /// </summary>
        public void TerminateContract(int contractID)
        {
            var contract = GetContract(contractID);
            if (contract == null)
            {
                Debug.LogWarning($"[ContractManager] Cannot terminate non-existent contract {contractID}");
                return;
            }

            // Remove from indices
            if (contractsByParent.TryGetValue(contract.ParentRealmID, out var parentContracts))
            {
                parentContracts.Remove(contractID);
            }

            if (contract.SubjectRealmID != -1 && contractsBySubject.TryGetValue(contract.SubjectRealmID, out var subjectContracts))
            {
                subjectContracts.Remove(contractID);
            }

            // Remove from active contracts
            activeContracts.Remove(contractID);

            OnContractTerminated?.Invoke(contract);
            Debug.Log($"[ContractManager] Contract {contractID} terminated");
        }

        // ========== CONTRACT BREACHES ==========

        /// <summary>
        /// Check for contract breaches due to low loyalty.
        /// </summary>
        private void CheckContractBreaches()
        {
            foreach (var contract in activeContracts.Values.ToList())
            {
                if (contract.IsRebellious())
                {
                    // Very low loyalty - risk of revolt
                    if (UnityEngine.Random.value < 0.1f) // 10% chance per season
                    {
                        Debug.LogWarning($"[ContractManager] Contract {contract.ContractID} breached - subject revolting!");
                        OnContractBreach?.Invoke(contract);

                        // TODO: Trigger revolt in subject realm
                        if (contract.SubjectRealmID != -1 && GovernmentManager.Instance != null)
                        {
                            var government = GovernmentManager.Instance.GetRealmGovernment(contract.SubjectRealmID);
                            if (government != null)
                            {
                                // Determine revolt type based on contract type
                                RevoltType revoltType = contract.Type == ContractType.Tributary
                                    ? RevoltType.ClanRising
                                    : RevoltType.PretenderWar;

                                GovernmentManager.Instance.TriggerRevolt(contract.SubjectRealmID, revoltType);
                            }
                        }

                        // Auto-terminate extremely rebellious contracts
                        TerminateContract(contract.ContractID);
                    }
                }
            }
        }

        // ========== CONTRACT EXPIRATION ==========

        /// <summary>
        /// Process contract expirations.
        /// </summary>
        private void ProcessContractExpirations()
        {
            foreach (var contract in activeContracts.Values.ToList())
            {
                if (contract.IsExpired(currentMonthCounter))
                {
                    if (contract.AutoRenewOnExpiration)
                    {
                        // Renew the contract
                        contract.CreatedOnMonth = currentMonthCounter;
                        Debug.Log($"[ContractManager] Contract {contract.ContractID} auto-renewed");
                    }
                    else
                    {
                        // Terminate
                        Debug.Log($"[ContractManager] Contract {contract.ContractID} expired (no auto-renew)");
                        TerminateContract(contract.ContractID);
                    }
                }
            }
        }

        // ========== LOYALTY MANAGEMENT ==========

        /// <summary>
        /// Modify contract loyalty.
        /// </summary>
        public void ModifyContractLoyalty(Contract contract, float delta)
        {
            if (contract == null)
            {
                Debug.LogError("[ContractManager] Cannot modify loyalty of null contract");
                return;
            }

            float oldLoyalty = contract.CurrentLoyalty;
            contract.CurrentLoyalty = Mathf.Clamp(contract.CurrentLoyalty + delta, 0f, 100f);

            OnLoyaltyChanged?.Invoke(contract, contract.CurrentLoyalty);

            if (Mathf.Abs(delta) > 0.1f)
            {
                Debug.Log($"[ContractManager] Contract {contract.ContractID} loyalty changed by {delta:F1} to {contract.CurrentLoyalty:F1}");
            }
        }

        /// <summary>
        /// Get average loyalty of all contracts for a parent realm.
        /// </summary>
        public float GetAverageLoyalty(int parentRealmID)
        {
            var contracts = GetContractsAsParent(parentRealmID);
            if (contracts.Count == 0)
                return 100f; // No contracts = no loyalty issues

            float total = contracts.Sum(c => c.CurrentLoyalty);
            return total / contracts.Count;
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Get total monthly income from contracts for a realm.
        /// </summary>
        public Dictionary<ResourceType, float> GetMonthlyIncomeFromContracts(int parentRealmID)
        {
            var income = new Dictionary<ResourceType, float>
            {
                { ResourceType.Food, 0f },
                { ResourceType.Gold, 0f },
                { ResourceType.Piety, 0f },
                { ResourceType.Prestige, 0f }
            };

            var contracts = GetContractsAsParent(parentRealmID);

            foreach (var contract in contracts)
            {
                foreach (var resourceType in income.Keys.ToList())
                {
                    float percentage = contract.GetResourcePercentage(resourceType);
                    // TODO: Calculate actual amounts based on subject realm production
                    income[resourceType] += percentage; // Placeholder
                }
            }

            return income;
        }

        /// <summary>
        /// Get total monthly expenses from contracts (subsidies).
        /// </summary>
        public int GetMonthlySubsidyExpenses(int parentRealmID)
        {
            var contracts = GetContractsAsParent(parentRealmID);
            return contracts.Sum(c => c.MonthlyGoldSubsidy);
        }

        // ========== SIMULATION TICKS ==========

        public void AdvanceDay()
        {
            // Nothing daily for now
        }

        public void AdvanceSeason()
        {
            currentMonthCounter += 3; // Season = 3 months

            // Process monthly tax collection
            ProcessTaxCollection();

            // Check for contract breaches due to low loyalty
            CheckContractBreaches();

            // Process contract expirations
            ProcessContractExpirations();
        }

        public void AdvanceYear()
        {
            // Nothing yearly for now
        }
    }
}
