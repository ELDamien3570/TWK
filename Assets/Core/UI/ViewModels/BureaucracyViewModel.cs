using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Government;
using TWK.Cultures;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying bureaucracy and office data in the UI.
    /// Exposes office management information in a UI-friendly format.
    /// </summary>
    public class BureaucracyViewModel : BaseViewModel
    {
        private int _realmID;

        // ========== OFFICE DATA ==========
        public List<OfficeDisplay> Offices { get; private set; }
        public int TotalOffices { get; private set; }
        public int FilledOffices { get; private set; }
        public int VacantOffices { get; private set; }

        // ========== COSTS ==========
        public int MonthlySalaryCost { get; private set; }
        public string MonthlySalaryCostDisplay { get; private set; }
        public int NextOfficeCost { get; private set; }
        public string NextOfficeCostDisplay { get; private set; }

        // ========== EFFICIENCY ==========
        public float AverageEfficiency { get; private set; }
        public string AverageEfficiencyDisplay { get; private set; }
        public string EfficiencyStatus { get; private set; }
        public Color EfficiencyColor { get; private set; }

        // ========== STATISTICS ==========
        public int OfficesByEconomics { get; private set; }
        public int OfficesByWarfare { get; private set; }
        public int OfficesByPolitics { get; private set; }
        public int OfficesByReligion { get; private set; }
        public int OfficesByScience { get; private set; }

        // ========== CONSTRUCTOR ==========
        public BureaucracyViewModel(int realmID)
        {
            _realmID = realmID;
            Offices = new List<OfficeDisplay>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (GovernmentManager.Instance == null)
            {
                TotalOffices = 0;
                NotifyPropertyChanged();
                return;
            }

            var offices = GovernmentManager.Instance.GetRealmOffices(_realmID);

            Offices.Clear();
            TotalOffices = offices.Count;
            FilledOffices = 0;
            VacantOffices = 0;
            MonthlySalaryCost = 0;
            float totalEfficiency = 0f;

            // Reset skill tree counters
            OfficesByEconomics = 0;
            OfficesByWarfare = 0;
            OfficesByPolitics = 0;
            OfficesByReligion = 0;
            OfficesByScience = 0;

            foreach (var office in offices)
            {
                if (office == null)
                    continue;

                Offices.Add(new OfficeDisplay
                {
                    OfficeName = office.OfficeName,
                    SkillTree = office.SkillTree.ToString(),
                    SkillTreeEnum = office.SkillTree,
                    Purpose = FormatPurpose(office.Purpose),
                    PurposeEnum = office.Purpose,
                    AssignedAgentName = GetAgentName(office.AssignedAgentID),
                    AssignedAgentID = office.AssignedAgentID,
                    Efficiency = office.CurrentEfficiency,
                    EfficiencyPercentage = office.GetEfficiencyPercentage(),
                    IsFilled = office.IsFilled(),
                    IsAutomated = office.IsAutomationEnabled,
                    MonthlySalary = office.MonthlySalary,
                    TargetCityID = office.TargetCityID,
                    HasTarget = office.HasCityTarget()
                });

                if (office.IsFilled())
                {
                    FilledOffices++;
                    MonthlySalaryCost += office.MonthlySalary;
                    totalEfficiency += office.CurrentEfficiency;
                }
                else
                {
                    VacantOffices++;
                }

                // Count by skill tree
                switch (office.SkillTree)
                {
                    case TreeType.Economics:
                        OfficesByEconomics++;
                        break;
                    case TreeType.Warfare:
                        OfficesByWarfare++;
                        break;
                    case TreeType.Politics:
                        OfficesByPolitics++;
                        break;
                    case TreeType.Religion:
                        OfficesByReligion++;
                        break;
                    case TreeType.Science:
                        OfficesByScience++;
                        break;
                }
            }

            AverageEfficiency = FilledOffices > 0 ? totalEfficiency / FilledOffices : 0f;
            AverageEfficiencyDisplay = $"{AverageEfficiency * 100:F0}%";
            EfficiencyStatus = GetEfficiencyStatus(AverageEfficiency);
            EfficiencyColor = GetEfficiencyColor(AverageEfficiency);

            // Calculate next office cost
            NextOfficeCost = GovernmentManager.Instance.CalculateOfficeCost(_realmID);
            NextOfficeCostDisplay = $"{NextOfficeCost} Gold";

            MonthlySalaryCostDisplay = $"{MonthlySalaryCost} Gold/month";

            NotifyPropertyChanged();
        }

        // ========== HELPER METHODS ==========

        private string GetAgentName(int agentID)
        {
            if (agentID == -1)
                return "VACANT";

            // TODO: Get agent name from Agent system
            return $"Agent {agentID}";
        }

        private string FormatPurpose(OfficePurpose purpose)
        {
            if (purpose == OfficePurpose.None)
                return "No Purpose";

            // Add spaces before capital letters
            return System.Text.RegularExpressions.Regex.Replace(
                purpose.ToString(),
                "([a-z])([A-Z])",
                "$1 $2"
            );
        }

        private string GetEfficiencyStatus(float efficiency)
        {
            if (efficiency >= 0.9f) return "Excellent";
            if (efficiency >= 0.7f) return "Good";
            if (efficiency >= 0.5f) return "Fair";
            if (efficiency >= 0.3f) return "Poor";
            return "Inefficient";
        }

        private Color GetEfficiencyColor(float efficiency)
        {
            if (efficiency >= 0.9f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (efficiency >= 0.7f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (efficiency >= 0.5f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (efficiency >= 0.3f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        // ========== QUERY METHODS ==========

        /// <summary>
        /// Get offices filtered by skill tree.
        /// </summary>
        public List<OfficeDisplay> GetOfficesBySkillTree(TreeType skillTree)
        {
            return Offices.Where(o => o.SkillTreeEnum == skillTree).ToList();
        }

        /// <summary>
        /// Get only filled offices.
        /// </summary>
        public List<OfficeDisplay> GetFilledOffices()
        {
            return Offices.Where(o => o.IsFilled).ToList();
        }

        /// <summary>
        /// Get only vacant offices.
        /// </summary>
        public List<OfficeDisplay> GetVacantOffices()
        {
            return Offices.Where(o => !o.IsFilled).ToList();
        }

        // ========== SUMMARY METHODS ==========

        public string GetOfficeSummary()
        {
            return $"Total: {TotalOffices} | Filled: {FilledOffices} | Vacant: {VacantOffices}";
        }

        public string GetCostSummary()
        {
            return $"Monthly Cost: {MonthlySalaryCostDisplay} | Next Office: {NextOfficeCostDisplay}";
        }

        public string GetSkillTreeBreakdown()
        {
            var parts = new List<string>();
            if (OfficesByEconomics > 0) parts.Add($"Economics: {OfficesByEconomics}");
            if (OfficesByWarfare > 0) parts.Add($"Warfare: {OfficesByWarfare}");
            if (OfficesByPolitics > 0) parts.Add($"Politics: {OfficesByPolitics}");
            if (OfficesByReligion > 0) parts.Add($"Religion: {OfficesByReligion}");
            if (OfficesByScience > 0) parts.Add($"Science: {OfficesByScience}");

            if (parts.Count == 0)
                return "No offices";

            return string.Join(" | ", parts);
        }
    }

    // ========== DISPLAY CLASS ==========

    public class OfficeDisplay
    {
        public string OfficeName;
        public string SkillTree;
        public TreeType SkillTreeEnum;
        public string Purpose;
        public OfficePurpose PurposeEnum;
        public string AssignedAgentName;
        public int AssignedAgentID;
        public float Efficiency;
        public string EfficiencyPercentage;
        public bool IsFilled;
        public bool IsAutomated;
        public int MonthlySalary;
        public int TargetCityID;
        public bool HasTarget;

        public string GetStatusText()
        {
            if (!IsFilled)
                return "VACANT";
            if (!IsAutomated)
                return "Automation Disabled";
            return $"Active - {EfficiencyPercentage} Efficiency";
        }

        public Color GetStatusColor()
        {
            if (!IsFilled)
                return new Color(0.5f, 0.5f, 0.5f); // Gray

            if (Efficiency >= 0.9f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (Efficiency >= 0.7f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (Efficiency >= 0.5f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (Efficiency >= 0.3f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }
    }
}
