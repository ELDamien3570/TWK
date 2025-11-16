using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;
using TWK.Modifiers;

namespace TWK.Government
{
    /// <summary>
    /// Defines resource and manpower obligations between realms or agents.
    /// Contracts govern vassalage, governorship, tributary relationships, and warband employment.
    /// </summary>
    [System.Serializable]
    public class Contract
    {
        // ========== IDENTITY ==========
        [Tooltip("Unique identifier for this contract")]
        public int ContractID;

        [Tooltip("Type of contract relationship")]
        public ContractType Type = ContractType.Governor;

        [Tooltip("Custom name for this contract (optional)")]
        public string CustomName;

        // ========== PARTIES ==========
        [Tooltip("Parent realm ID (the overlord/employer)")]
        public int ParentRealmID;

        [Tooltip("Subject realm ID (for Vassal/Tributary contracts, -1 if not applicable)")]
        public int SubjectRealmID = -1;

        [Tooltip("Subject agent ID (for Governor/Warband contracts, -1 if not applicable)")]
        public int SubjectAgentID = -1;

        // ========== RESOURCE OBLIGATIONS ==========
        [Header("Resource Percentages")]
        [Tooltip("Percentage of Food the subject must provide to parent")]
        [Range(0, 100)]
        public float FoodPercentage = 0f;

        [Tooltip("Percentage of Gold the subject must provide to parent")]
        [Range(0, 100)]
        public float GoldPercentage = 10f;

        [Tooltip("Percentage of Piety the subject must provide to parent")]
        [Range(0, 100)]
        public float PietyPercentage = 0f;

        [Tooltip("Percentage of Prestige the subject must provide to parent")]
        [Range(0, 100)]
        public float PrestigePercentage = 0f;

        // ========== MANPOWER OBLIGATIONS ==========
        [Header("Manpower")]
        [Tooltip("Percentage of military forces the subject must provide")]
        [Range(0, 100)]
        public float ManpowerPercentage = 50f;

        // ========== GOVERNANCE CONTROL ==========
        [Header("Governance")]
        [Tooltip("Can the subject realm maintain its own government form?")]
        public bool AllowSubjectGovernment = true;

        [Tooltip("Can the subject reform its government without parent approval?")]
        public bool CanReformGovernment = false;

        [Tooltip("Does the parent directly control subject's bureaucracy/offices?")]
        public bool ParentControlsBureaucracy = false;

        // ========== SUBSIDIES ==========
        [Header("Subsidies (Parent to Subject)")]
        [Tooltip("Modifiers granted by parent to subject as support")]
        public List<Modifier> Subsidies = new List<Modifier>();

        [Tooltip("Monthly gold subsidy from parent to subject")]
        public int MonthlyGoldSubsidy = 0;

        // ========== LOYALTY ==========
        [Header("Loyalty Tracking")]
        [Tooltip("Current loyalty of subject to parent (0-100)")]
        [Range(0, 100)]
        public float CurrentLoyalty = 50f;

        [Tooltip("Loyalty modifier from contract fairness (calculated)")]
        public float LoyaltyFromContract = 0f;

        // ========== AUTOMATION ==========
        [Header("Contract Duration")]
        [Tooltip("Auto-renew when contract expires?")]
        public bool AutoRenewOnExpiration = true;

        [Tooltip("Contract duration in months (-1 = permanent)")]
        public int ContractDurationMonths = -1;

        [Tooltip("Month when contract was created (for expiration tracking)")]
        public int CreatedOnMonth = 0;

        // ========== CONSTRUCTORS ==========

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Contract()
        {
        }

        /// <summary>
        /// Create a new contract with basic configuration.
        /// </summary>
        public Contract(int contractID, int parentRealmID, ContractType type)
        {
            ContractID = contractID;
            ParentRealmID = parentRealmID;
            Type = type;
        }

        // ========== LOYALTY CALCULATION ==========

        /// <summary>
        /// Calculate and update loyalty impact from contract fairness.
        /// Higher resource/manpower demands = lower loyalty.
        /// Subsidies improve loyalty.
        /// </summary>
        public float CalculateLoyaltyModifier()
        {
            // Calculate average resource demand
            float resourceDemand = (FoodPercentage + GoldPercentage +
                                   PietyPercentage + PrestigePercentage) / 4f;

            // Combine resource and manpower demands
            float totalDemand = (resourceDemand + ManpowerPercentage) / 2f;

            // Formula: 0% demand = +20 loyalty, 100% demand = -30 loyalty
            LoyaltyFromContract = 20f - (totalDemand * 0.5f);

            // Subsidies improve loyalty
            LoyaltyFromContract += Subsidies.Count * 5f; // +5 per subsidy modifier
            LoyaltyFromContract += MonthlyGoldSubsidy * 0.1f; // +0.1 per gold

            // Governance restrictions reduce loyalty
            if (!AllowSubjectGovernment)
                LoyaltyFromContract -= 10f;
            if (ParentControlsBureaucracy)
                LoyaltyFromContract -= 5f;

            return LoyaltyFromContract;
        }

        /// <summary>
        /// Update current loyalty based on contract fairness.
        /// </summary>
        public void UpdateLoyalty()
        {
            float loyaltyModifier = CalculateLoyaltyModifier();

            // Base loyalty (50) + contract modifier
            // Clamp between 0 and 100
            CurrentLoyalty = Mathf.Clamp(50f + loyaltyModifier, 0f, 100f);
        }

        // ========== RESOURCE QUERIES ==========

        /// <summary>
        /// Get resource percentage for a specific resource type.
        /// </summary>
        public float GetResourcePercentage(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Food => FoodPercentage,
                ResourceType.Gold => GoldPercentage,
                ResourceType.Piety => PietyPercentage,
                ResourceType.Prestige => PrestigePercentage,
                _ => 0f
            };
        }

        /// <summary>
        /// Set resource percentage for a specific resource type.
        /// </summary>
        public void SetResourcePercentage(ResourceType resourceType, float percentage)
        {
            percentage = Mathf.Clamp(percentage, 0f, 100f);

            switch (resourceType)
            {
                case ResourceType.Food:
                    FoodPercentage = percentage;
                    break;
                case ResourceType.Gold:
                    GoldPercentage = percentage;
                    break;
                case ResourceType.Piety:
                    PietyPercentage = percentage;
                    break;
                case ResourceType.Prestige:
                    PrestigePercentage = percentage;
                    break;
            }

            UpdateLoyalty();
        }

        /// <summary>
        /// Get total resource burden (average of all resource percentages).
        /// </summary>
        public float GetTotalResourceBurden()
        {
            return (FoodPercentage + GoldPercentage + PietyPercentage + PrestigePercentage) / 4f;
        }

        // ========== CONTRACT TYPE HELPERS ==========

        /// <summary>
        /// Is this a Governor contract?
        /// </summary>
        public bool IsGovernor() => Type == ContractType.Governor;

        /// <summary>
        /// Is this a Vassal contract?
        /// </summary>
        public bool IsVassal() => Type == ContractType.Vassal;

        /// <summary>
        /// Is this a Tributary contract?
        /// </summary>
        public bool IsTributary() => Type == ContractType.Tributary;

        /// <summary>
        /// Is this a Warband contract?
        /// </summary>
        public bool IsWarband() => Type == ContractType.Warband;

        // ========== LOYALTY STATUS ==========

        /// <summary>
        /// Is the subject loyal? (>= 60%)
        /// </summary>
        public bool IsLoyal() => CurrentLoyalty >= 60f;

        /// <summary>
        /// Is the subject disloyal? (< 40%)
        /// </summary>
        public bool IsDisloyal() => CurrentLoyalty < 40f;

        /// <summary>
        /// Is the subject about to rebel? (< 20%)
        /// </summary>
        public bool IsRebellious() => CurrentLoyalty < 20f;

        /// <summary>
        /// Get loyalty status as a string.
        /// </summary>
        public string GetLoyaltyStatus()
        {
            if (CurrentLoyalty >= 80f) return "Loyal";
            if (CurrentLoyalty >= 60f) return "Content";
            if (CurrentLoyalty >= 40f) return "Uncertain";
            if (CurrentLoyalty >= 20f) return "Disloyal";
            return "Rebellious";
        }

        // ========== EXPIRATION ==========

        /// <summary>
        /// Is this contract permanent?
        /// </summary>
        public bool IsPermanent() => ContractDurationMonths == -1;

        /// <summary>
        /// Is this contract expired?
        /// </summary>
        public bool IsExpired(int currentMonth)
        {
            if (IsPermanent())
                return false;

            return currentMonth >= CreatedOnMonth + ContractDurationMonths;
        }

        /// <summary>
        /// Get months remaining until expiration.
        /// </summary>
        public int GetMonthsRemaining(int currentMonth)
        {
            if (IsPermanent())
                return -1;

            int remaining = (CreatedOnMonth + ContractDurationMonths) - currentMonth;
            return Mathf.Max(0, remaining);
        }

        // ========== DISPLAY HELPERS ==========

        /// <summary>
        /// Get the display name for this contract.
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(CustomName))
                return CustomName;

            return $"{Type} Contract #{ContractID}";
        }

        /// <summary>
        /// Get a summary of contract terms.
        /// </summary>
        public string GetTermsSummary()
        {
            var terms = new List<string>();

            if (FoodPercentage > 0) terms.Add($"Food: {FoodPercentage:F0}%");
            if (GoldPercentage > 0) terms.Add($"Gold: {GoldPercentage:F0}%");
            if (PietyPercentage > 0) terms.Add($"Piety: {PietyPercentage:F0}%");
            if (PrestigePercentage > 0) terms.Add($"Prestige: {PrestigePercentage:F0}%");
            if (ManpowerPercentage > 0) terms.Add($"Manpower: {ManpowerPercentage:F0}%");

            if (terms.Count == 0)
                return "No obligations";

            return string.Join(", ", terms);
        }
    }
}
