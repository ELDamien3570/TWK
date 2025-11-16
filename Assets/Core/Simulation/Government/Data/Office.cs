using UnityEngine;
using TWK.Cultures;

namespace TWK.Government
{
    /// <summary>
    /// Represents a bureaucratic office that can be filled by an agent.
    /// Offices execute automated tasks based on their purpose and agent skill.
    /// </summary>
    [System.Serializable]
    public class Office
    {
        // ========== IDENTITY ==========
        [Tooltip("Name of this office (e.g., 'Governor of Memphis', 'Trade Minister')")]
        public string OfficeName;

        [Tooltip("Skill tree this office uses (determines agent efficiency)")]
        public TreeType SkillTree; // Economics, Warfare, Politics, etc.

        [Tooltip("Automated purpose of this office")]
        public OfficePurpose Purpose = OfficePurpose.None;

        // ========== ASSIGNMENT ==========
        [Tooltip("ID of the agent assigned to this office (-1 = vacant)")]
        public int AssignedAgentID = -1;

        [Tooltip("Target city ID where this office operates (-1 = realm-wide)")]
        public int TargetCityID = -1;

        // ========== EFFICIENCY ==========
        [Tooltip("Base efficiency before agent skill modifiers")]
        [Range(0f, 2f)]
        public float BaseEfficiency = 1.0f;

        [Tooltip("Current efficiency including agent skill (calculated at runtime)")]
        [Range(0f, 3f)]
        public float CurrentEfficiency = 1.0f;

        // ========== COSTS ==========
        [Tooltip("Monthly salary in gold")]
        public int MonthlySalary = 10;

        // ========== AUTOMATION ==========
        [Tooltip("Is automation enabled for this office?")]
        public bool IsAutomationEnabled = true;

        [Tooltip("Last day this office executed its automation")]
        public int LastExecutionDay = -1;

        [Tooltip("How often automation runs (in days)")]
        public int ExecutionFrequencyDays = 30;

        // ========== CONSTRUCTORS ==========

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Office()
        {
        }

        /// <summary>
        /// Create a new office with basic configuration.
        /// </summary>
        public Office(string name, TreeType skillTree, OfficePurpose purpose = OfficePurpose.None)
        {
            OfficeName = name;
            SkillTree = skillTree;
            Purpose = purpose;
        }

        // ========== ASSIGNMENT METHODS ==========

        /// <summary>
        /// Is this office currently filled by an agent?
        /// </summary>
        public bool IsFilled()
        {
            return AssignedAgentID != -1;
        }

        /// <summary>
        /// Is this office currently vacant?
        /// </summary>
        public bool IsVacant()
        {
            return AssignedAgentID == -1;
        }

        /// <summary>
        /// Assign an agent to this office.
        /// </summary>
        public void AssignAgent(int agentID)
        {
            AssignedAgentID = agentID;
            RecalculateEfficiency();
        }

        /// <summary>
        /// Remove the current agent from this office.
        /// </summary>
        public void ClearAssignment()
        {
            AssignedAgentID = -1;
            CurrentEfficiency = 0f;
        }

        // ========== EFFICIENCY METHODS ==========

        /// <summary>
        /// Recalculate efficiency based on agent skill.
        /// This should be called by GovernmentManager when agent is assigned.
        /// </summary>
        public void RecalculateEfficiency()
        {
            if (!IsFilled())
            {
                CurrentEfficiency = 0f;
                return;
            }

            // Base efficiency until agent skill is integrated
            // TODO: Get agent skill from AgentManager and apply multiplier
            CurrentEfficiency = BaseEfficiency;
        }

        /// <summary>
        /// Update efficiency with agent skill level.
        /// Called by GovernmentManager when it has access to agent data.
        /// </summary>
        public void UpdateEfficiency(float agentSkillLevel)
        {
            if (!IsFilled())
            {
                CurrentEfficiency = 0f;
                return;
            }

            // Formula: Base * (1 + skill/100)
            // Example: Base 1.0, Skill 50 = 1.0 * 1.5 = 1.5 efficiency
            CurrentEfficiency = BaseEfficiency * (1f + agentSkillLevel * 0.01f);
        }

        // ========== AUTOMATION METHODS ==========

        /// <summary>
        /// Can this office execute its automation now?
        /// </summary>
        public bool CanExecuteNow(int currentDay)
        {
            // Must be filled and automation enabled
            if (!IsFilled() || !IsAutomationEnabled)
                return false;

            // No purpose = nothing to automate
            if (Purpose == OfficePurpose.None)
                return false;

            // First execution
            if (LastExecutionDay == -1)
                return true;

            // Check if enough days have passed
            return currentDay >= LastExecutionDay + ExecutionFrequencyDays;
        }

        /// <summary>
        /// Mark this office as having executed its automation.
        /// </summary>
        public void MarkExecuted(int currentDay)
        {
            LastExecutionDay = currentDay;
        }

        // ========== TARGET METHODS ==========

        /// <summary>
        /// Does this office target a specific city?
        /// </summary>
        public bool HasCityTarget()
        {
            return TargetCityID != -1;
        }

        /// <summary>
        /// Is this a realm-wide office?
        /// </summary>
        public bool IsRealmWide()
        {
            return TargetCityID == -1;
        }

        /// <summary>
        /// Set the target city for this office.
        /// </summary>
        public void SetTargetCity(int cityID)
        {
            TargetCityID = cityID;
        }

        /// <summary>
        /// Clear the target city (make realm-wide).
        /// </summary>
        public void ClearTargetCity()
        {
            TargetCityID = -1;
        }

        // ========== COST METHODS ==========

        /// <summary>
        /// Get the monthly maintenance cost for this office.
        /// </summary>
        public int GetMonthlyCost()
        {
            // Vacant offices don't cost anything
            if (IsVacant())
                return 0;

            return MonthlySalary;
        }

        // ========== DISPLAY HELPERS ==========

        /// <summary>
        /// Get a formatted display string for this office.
        /// </summary>
        public string GetDisplayName()
        {
            if (string.IsNullOrEmpty(OfficeName))
                return $"{SkillTree} Office";
            return OfficeName;
        }

        /// <summary>
        /// Get efficiency as a percentage string.
        /// </summary>
        public string GetEfficiencyPercentage()
        {
            return $"{CurrentEfficiency * 100:F0}%";
        }

        /// <summary>
        /// Get status description.
        /// </summary>
        public string GetStatusDescription()
        {
            if (IsVacant())
                return "VACANT";

            if (!IsAutomationEnabled)
                return "Automation Disabled";

            if (Purpose == OfficePurpose.None)
                return "No Purpose Assigned";

            return $"Active - {GetEfficiencyPercentage()} Efficiency";
        }
    }
}
