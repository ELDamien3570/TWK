using System;
using System.Collections.Generic;
using UnityEngine;

namespace TWK.Realms
{
    /// <summary>
    /// Pure data class for a realm. Contains no logic or MonoBehaviour.
    /// This is the "Model" in MVVM - just serializable state.
    /// Follows the same pattern as CityData.
    /// </summary>
    [System.Serializable]
    public class RealmData
    {
        // ========== IDENTITY ==========
        public string RealmName;
        public int RealmID;

        // ========== GOVERNMENT ==========
        /// <summary>
        /// Reference to this realm's government (managed by GovernmentManager).
        /// </summary>
        public int GovernmentID = -1;

        // ========== LEADERSHIP ==========
        /// <summary>
        /// IDs of agents who are leaders/rulers of this realm.
        /// Multiple leaders possible (co-rulers, councils, etc).
        /// </summary>
        public List<int> LeaderIDs = new List<int>();

        // ========== CULTURE ==========
        /// <summary>
        /// Primary culture of the realm's ruling class.
        /// </summary>
        public int RealmCultureID = -1;

        // ========== TERRITORIAL CONTROL ==========
        /// <summary>
        /// Cities directly owned by this realm (no intermediary contracts).
        /// Tax flows 100% to realm treasury from these cities.
        /// </summary>
        public List<int> DirectlyOwnedCityIDs = new List<int>();

        /// <summary>
        /// IDs of contracts where this realm is the overlord.
        /// These are vassals, tributaries, governors, etc.
        /// Tax leakage occurs here based on contract %.
        /// </summary>
        public List<int> VassalContractIDs = new List<int>();

        /// <summary>
        /// ID of contract where this realm is the subject (if any).
        /// -1 if this realm is independent.
        /// </summary>
        public int OverlordContractID = -1;

        // ========== DIPLOMATIC STATUS ==========
        /// <summary>
        /// True if this realm is independent (no overlord).
        /// </summary>
        public bool IsIndependent => OverlordContractID == -1;

        /// <summary>
        /// True if this realm has vassals.
        /// </summary>
        public bool HasVassals => VassalContractIDs != null && VassalContractIDs.Count > 0;

        // ========== TREASURY ==========
        /// <summary>
        /// Current resources in realm treasury.
        /// Managed by RealmTreasury, stored here for serialization.
        /// </summary>
        public Dictionary<TWK.Economy.ResourceType, int> TreasuryResources = new Dictionary<TWK.Economy.ResourceType, int>();

        // ========== REALM PROPERTIES ==========
        /// <summary>
        /// Total population across all directly owned cities.
        /// Cached, updated periodically.
        /// </summary>
        public int TotalPopulation;

        /// <summary>
        /// Total military strength.
        /// Cached, updated periodically.
        /// </summary>
        public int TotalMilitaryStrength;

        // ========== CONSTRUCTOR ==========
        public RealmData()
        {
            // Default constructor for serialization
            LeaderIDs = new List<int>();
            DirectlyOwnedCityIDs = new List<int>();
            VassalContractIDs = new List<int>();
            TreasuryResources = new Dictionary<TWK.Economy.ResourceType, int>();
        }

        public RealmData(int realmID, string realmName, int cultureID = -1)
        {
            this.RealmID = realmID;
            this.RealmName = realmName;
            this.RealmCultureID = cultureID;

            LeaderIDs = new List<int>();
            DirectlyOwnedCityIDs = new List<int>();
            VassalContractIDs = new List<int>();
            TreasuryResources = new Dictionary<TWK.Economy.ResourceType, int>();
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Add a directly owned city to this realm.
        /// </summary>
        public void AddCity(int cityID)
        {
            if (!DirectlyOwnedCityIDs.Contains(cityID))
            {
                DirectlyOwnedCityIDs.Add(cityID);
            }
        }

        /// <summary>
        /// Remove a directly owned city from this realm.
        /// </summary>
        public void RemoveCity(int cityID)
        {
            DirectlyOwnedCityIDs.Remove(cityID);
        }

        /// <summary>
        /// Add a vassal contract to this realm.
        /// </summary>
        public void AddVassalContract(int contractID)
        {
            if (!VassalContractIDs.Contains(contractID))
            {
                VassalContractIDs.Add(contractID);
            }
        }

        /// <summary>
        /// Remove a vassal contract from this realm.
        /// </summary>
        public void RemoveVassalContract(int contractID)
        {
            VassalContractIDs.Remove(contractID);
        }

        /// <summary>
        /// Add a leader to this realm.
        /// </summary>
        public void AddLeader(int agentID)
        {
            if (!LeaderIDs.Contains(agentID))
            {
                LeaderIDs.Add(agentID);
            }
        }

        /// <summary>
        /// Remove a leader from this realm.
        /// </summary>
        public void RemoveLeader(int agentID)
        {
            LeaderIDs.Remove(agentID);
        }
    }
}
