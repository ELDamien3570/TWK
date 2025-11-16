using System.Collections.Generic;
using UnityEngine;
using TWK.Government;
using TWK.Economy;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying contract data in the UI.
    /// Exposes contract information in a UI-friendly format.
    /// </summary>
    public class ContractViewModel : BaseViewModel
    {
        private Contract _contract;

        // ========== IDENTITY ==========
        public int ContractID { get; private set; }
        public string ContractType { get; private set; }
        public ContractType ContractTypeEnum { get; private set; }
        public string DisplayName { get; private set; }
        public string CustomName { get; private set; }

        // ========== PARTIES ==========
        public int ParentRealmID { get; private set; }
        public string ParentRealmName { get; private set; }
        public int SubjectRealmID { get; private set; }
        public int SubjectAgentID { get; private set; }
        public string SubjectName { get; private set; }
        public bool HasSubjectRealm { get; private set; }
        public bool HasSubjectAgent { get; private set; }

        // ========== RESOURCE OBLIGATIONS ==========
        public Dictionary<string, float> ResourcePercentages { get; private set; }
        public float FoodPercentage { get; private set; }
        public float GoldPercentage { get; private set; }
        public float PietyPercentage { get; private set; }
        public float PrestigePercentage { get; private set; }
        public float TotalResourceBurden { get; private set; }
        public string TotalResourceBurdenDisplay { get; private set; }

        // ========== MANPOWER OBLIGATIONS ==========
        public float ManpowerPercentage { get; private set; }
        public string ManpowerDisplay { get; private set; }

        // ========== GOVERNANCE ==========
        public bool AllowSubjectGovernment { get; private set; }
        public bool CanReformGovernment { get; private set; }
        public bool ParentControlsBureaucracy { get; private set; }
        public string GovernanceStatus { get; private set; }

        // ========== LOYALTY ==========
        public float Loyalty { get; private set; }
        public string LoyaltyDisplay { get; private set; }
        public string LoyaltyStatus { get; private set; }
        public Color LoyaltyColor { get; private set; }
        public float LoyaltyFromContract { get; private set; }

        // ========== LOYALTY STATES ==========
        public bool IsLoyal { get; private set; }
        public bool IsDisloyal { get; private set; }
        public bool IsRebellious { get; private set; }

        // ========== SUBSIDIES ==========
        public int SubsidyCount { get; private set; }
        public List<string> Subsidies { get; private set; }
        public int MonthlyGoldSubsidy { get; private set; }
        public string MonthlyGoldSubsidyDisplay { get; private set; }
        public bool HasSubsidies { get; private set; }

        // ========== DURATION ==========
        public bool IsPermanent { get; private set; }
        public int DurationMonths { get; private set; }
        public string DurationDisplay { get; private set; }
        public bool AutoRenew { get; private set; }
        public int MonthsRemaining { get; private set; }
        public string MonthsRemainingDisplay { get; private set; }

        // ========== CONSTRUCTOR ==========
        public ContractViewModel(Contract contract)
        {
            _contract = contract;
            ResourcePercentages = new Dictionary<string, float>();
            Subsidies = new List<string>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_contract == null)
            {
                DisplayName = "Invalid Contract";
                NotifyPropertyChanged();
                return;
            }

            // Identity
            ContractID = _contract.ContractID;
            ContractType = _contract.Type.ToString();
            ContractTypeEnum = _contract.Type;
            DisplayName = _contract.GetDisplayName();
            CustomName = _contract.CustomName;

            // Parties
            ParentRealmID = _contract.ParentRealmID;
            ParentRealmName = GetRealmName(ParentRealmID);
            SubjectRealmID = _contract.SubjectRealmID;
            SubjectAgentID = _contract.SubjectAgentID;
            SubjectName = GetSubjectName();
            HasSubjectRealm = SubjectRealmID != -1;
            HasSubjectAgent = SubjectAgentID != -1;

            // Resource obligations
            ResourcePercentages.Clear();
            FoodPercentage = _contract.FoodPercentage;
            GoldPercentage = _contract.GoldPercentage;
            PietyPercentage = _contract.PietyPercentage;
            PrestigePercentage = _contract.PrestigePercentage;

            ResourcePercentages["Food"] = FoodPercentage;
            ResourcePercentages["Gold"] = GoldPercentage;
            ResourcePercentages["Piety"] = PietyPercentage;
            ResourcePercentages["Prestige"] = PrestigePercentage;

            TotalResourceBurden = _contract.GetTotalResourceBurden();
            TotalResourceBurdenDisplay = $"{TotalResourceBurden:F1}%";

            // Manpower obligations
            ManpowerPercentage = _contract.ManpowerPercentage;
            ManpowerDisplay = $"{ManpowerPercentage:F0}%";

            // Governance
            AllowSubjectGovernment = _contract.AllowSubjectGovernment;
            CanReformGovernment = _contract.CanReformGovernment;
            ParentControlsBureaucracy = _contract.ParentControlsBureaucracy;
            GovernanceStatus = GetGovernanceStatus();

            // Loyalty
            Loyalty = _contract.CurrentLoyalty;
            LoyaltyDisplay = $"{Loyalty:F0}%";
            LoyaltyStatus = _contract.GetLoyaltyStatus();
            LoyaltyColor = GetLoyaltyColor(Loyalty);
            LoyaltyFromContract = _contract.LoyaltyFromContract;

            IsLoyal = _contract.IsLoyal();
            IsDisloyal = _contract.IsDisloyal();
            IsRebellious = _contract.IsRebellious();

            // Subsidies
            RefreshSubsidies();

            // Duration
            IsPermanent = _contract.IsPermanent();
            DurationMonths = _contract.ContractDurationMonths;
            DurationDisplay = IsPermanent ? "Permanent" : $"{DurationMonths} months";
            AutoRenew = _contract.AutoRenewOnExpiration;

            // Calculate months remaining (need current month from manager)
            if (ContractManager.Instance != null)
            {
                // TODO: Get current month from manager
                MonthsRemaining = _contract.GetMonthsRemaining(0);
                MonthsRemainingDisplay = IsPermanent ? "∞" : $"{MonthsRemaining} months";
            }

            NotifyPropertyChanged();
        }

        private void RefreshSubsidies()
        {
            Subsidies.Clear();
            SubsidyCount = 0;

            if (_contract.Subsidies != null)
            {
                foreach (var subsidy in _contract.Subsidies)
                {
                    if (subsidy != null)
                    {
                        Subsidies.Add(subsidy.Name);
                        SubsidyCount++;
                    }
                }
            }

            MonthlyGoldSubsidy = _contract.MonthlyGoldSubsidy;
            MonthlyGoldSubsidyDisplay = MonthlyGoldSubsidy > 0
                ? $"{MonthlyGoldSubsidy} Gold/month"
                : "None";

            if (MonthlyGoldSubsidy > 0)
            {
                Subsidies.Add(MonthlyGoldSubsidyDisplay);
                SubsidyCount++;
            }

            HasSubsidies = SubsidyCount > 0;
        }

        // ========== HELPER METHODS ==========

        private string GetRealmName(int realmID)
        {
            // TODO: Get from RealmManager when available
            return $"Realm {realmID}";
        }

        private string GetSubjectName()
        {
            if (SubjectRealmID != -1)
                return GetRealmName(SubjectRealmID);
            if (SubjectAgentID != -1)
                return $"Agent {SubjectAgentID}"; // TODO: Get agent name
            return "Unassigned";
        }

        private string GetGovernanceStatus()
        {
            if (ParentControlsBureaucracy)
                return "Full Parent Control";
            if (!AllowSubjectGovernment)
                return "Parent Government Only";
            if (!CanReformGovernment)
                return "Limited Autonomy";
            return "Full Autonomy";
        }

        private Color GetLoyaltyColor(float loyalty)
        {
            if (loyalty >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (loyalty >= 60f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (loyalty >= 40f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (loyalty >= 20f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        // ========== SUMMARY METHODS ==========

        public string GetContractSummary()
        {
            return $"{ContractType}: {ParentRealmName} ← {SubjectName}";
        }

        public string GetObligationsSummary()
        {
            return _contract.GetTermsSummary();
        }

        public string GetLoyaltySummary()
        {
            return $"{LoyaltyDisplay} ({LoyaltyStatus})";
        }

        public string GetDurationSummary()
        {
            if (IsPermanent)
                return "Permanent" + (AutoRenew ? " (Auto-renew)" : "");
            return $"{DurationDisplay} ({MonthsRemainingDisplay} remaining)" + (AutoRenew ? " | Auto-renew" : "");
        }

        /// <summary>
        /// Get a list of all non-zero resource obligations.
        /// </summary>
        public List<ResourceObligation> GetActiveResourceObligations()
        {
            var obligations = new List<ResourceObligation>();

            if (FoodPercentage > 0)
                obligations.Add(new ResourceObligation { ResourceName = "Food", Percentage = FoodPercentage });
            if (GoldPercentage > 0)
                obligations.Add(new ResourceObligation { ResourceName = "Gold", Percentage = GoldPercentage });
            if (PietyPercentage > 0)
                obligations.Add(new ResourceObligation { ResourceName = "Piety", Percentage = PietyPercentage });
            if (PrestigePercentage > 0)
                obligations.Add(new ResourceObligation { ResourceName = "Prestige", Percentage = PrestigePercentage });

            return obligations;
        }
    }

    // ========== DISPLAY CLASS ==========

    public class ResourceObligation
    {
        public string ResourceName;
        public float Percentage;

        public string GetDisplay()
        {
            return $"{ResourceName}: {Percentage:F0}%";
        }
    }
}
