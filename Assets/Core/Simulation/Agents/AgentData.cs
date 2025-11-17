using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;

namespace TWK.Agents
{
    /// <summary>
    /// Pure data class for an agent (character). Contains no logic or MonoBehaviour.
    /// This is the "Model" in MVVM - just serializable state.
    /// Follows the same pattern as CityData and RealmData.
    /// </summary>
    [System.Serializable]
    public class AgentData
    {
        // ========== IDENTITY ==========
        public string AgentName;
        public int AgentID;

        // ========== BIOGRAPHICAL ==========
        public int Age;
        public int BirthDay;
        public int BirthYear;
        public int BirthMonth;

        // ========== AFFILIATIONS ==========
        /// <summary>
        /// Realm this agent belongs to (home realm).
        /// </summary>
        public int HomeRealmID = -1;

        /// <summary>
        /// Current office held in government (if any).
        /// -1 if no office held.
        /// </summary>
        public int CurrentOfficeID = -1;

        /// <summary>
        /// Realm where this agent currently holds office (may differ from home realm).
        /// -1 if no office held.
        /// </summary>
        public int OfficeRealmID = -1;

        // ========== CULTURE ==========
        public int CultureID = -1;

        // ========== SKILLS ==========
        [SerializeField]
        private Dictionary<TreeType, float> _skillLevels = new Dictionary<TreeType, float>();

        public Dictionary<TreeType, float> SkillLevels
        {
            get => _skillLevels;
            set => _skillLevels = value;
        }

        public TreeType SkillFocus;
        public float DailySkillGain = 1f;
        public float DailyFocusBonus = 0.5f;

        // ========== PERSONAL WEALTH ==========
        /// <summary>
        /// Personal resources owned by this agent.
        /// Managed by AgentLedger, stored here for serialization.
        /// </summary>
        public Dictionary<TWK.Economy.ResourceType, int> PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();

        /// <summary>
        /// Monthly salary from current office (if any).
        /// </summary>
        public int MonthlySalary = 0;

        // ========== PROPERTIES ==========
        /// <summary>
        /// List of building IDs owned by this agent (estates, businesses, etc).
        /// </summary>
        public List<int> OwnedBuildingIDs = new List<int>();

        /// <summary>
        /// List of trade caravan IDs owned by this agent.
        /// </summary>
        public List<int> OwnedCaravanIDs = new List<int>();

        // ========== STATUS ==========
        /// <summary>
        /// Is this agent alive?
        /// </summary>
        public bool IsAlive = true;

        /// <summary>
        /// Is this agent currently employed in a government office?
        /// </summary>
        public bool HasOffice => CurrentOfficeID != -1;

        /// <summary>
        /// Is this agent a ruler of their home realm?
        /// </summary>
        public bool IsRuler = false;

        // ========== CONSTRUCTOR ==========
        public AgentData()
        {
            // Default constructor for serialization
            _skillLevels = new Dictionary<TreeType, float>();
            PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();
            OwnedBuildingIDs = new List<int>();
            OwnedCaravanIDs = new List<int>();
        }

        public AgentData(int agentID, string name, int homeRealmID, int cultureID = -1)
        {
            this.AgentID = agentID;
            this.AgentName = name;
            this.HomeRealmID = homeRealmID;
            this.CultureID = cultureID;

            _skillLevels = new Dictionary<TreeType, float>();
            PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();
            OwnedBuildingIDs = new List<int>();
            OwnedCaravanIDs = new List<int>();

            // Initialize all skill levels to 0
            foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
            {
                _skillLevels[tree] = 0f;
            }
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Add a building to this agent's property list.
        /// </summary>
        public void AddOwnedBuilding(int buildingID)
        {
            if (!OwnedBuildingIDs.Contains(buildingID))
            {
                OwnedBuildingIDs.Add(buildingID);
            }
        }

        /// <summary>
        /// Remove a building from this agent's property list.
        /// </summary>
        public void RemoveOwnedBuilding(int buildingID)
        {
            OwnedBuildingIDs.Remove(buildingID);
        }

        /// <summary>
        /// Add a caravan to this agent's property list.
        /// </summary>
        public void AddOwnedCaravan(int caravanID)
        {
            if (!OwnedCaravanIDs.Contains(caravanID))
            {
                OwnedCaravanIDs.Add(caravanID);
            }
        }

        /// <summary>
        /// Remove a caravan from this agent's property list.
        /// </summary>
        public void RemoveOwnedCaravan(int caravanID)
        {
            OwnedCaravanIDs.Remove(caravanID);
        }

        /// <summary>
        /// Assign this agent to a government office.
        /// </summary>
        public void AssignToOffice(int officeID, int realmID, int salary)
        {
            CurrentOfficeID = officeID;
            OfficeRealmID = realmID;
            MonthlySalary = salary;
        }

        /// <summary>
        /// Remove this agent from their current office.
        /// </summary>
        public void RemoveFromOffice()
        {
            CurrentOfficeID = -1;
            OfficeRealmID = -1;
            MonthlySalary = 0;
        }

        /// <summary>
        /// Calculate total wealth across all resource types.
        /// </summary>
        public int GetTotalWealth()
        {
            int total = 0;
            foreach (var kvp in PersonalWealth)
            {
                // Convert all resources to gold equivalent (simple 1:1 for now)
                total += kvp.Value;
            }
            return total;
        }
    }
}
