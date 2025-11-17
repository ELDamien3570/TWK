using System.Collections.Generic;
using UnityEngine;
using TWK.Core;

namespace TWK.Realms
{
    /// <summary>
    /// Singleton manager for all realms in the game.
    /// Handles realm creation, lookup, and daily economic cycles.
    /// Coordinates tax collection and tribute payments.
    /// </summary>
    public class RealmManager : MonoBehaviour
    {
        public static RealmManager Instance { get; private set; }

        // ========== REALM REGISTRY ==========
        private Dictionary<int, Realm> _realms = new Dictionary<int, Realm>();
        private int _nextRealmID = 1;

        // ========== EVENTS ==========
        public event System.Action<int> OnRealmCreated; // realmID
        public event System.Action<int> OnRealmDestroyed; // realmID
        public event System.Action<int, string> OnRealmNameChanged; // realmID, newName
        public event System.Action<int, int> OnRealmLeaderChanged; // realmID, newLeaderID
        public event System.Action<int, int> OnCityAddedToRealm; // realmID, cityID
        public event System.Action<int, int> OnCityRemovedFromRealm; // realmID, cityID

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ========== REALM CREATION & REGISTRATION ==========

        /// <summary>
        /// Register a realm with the manager.
        /// Called by Realm.Initialize().
        /// </summary>
        public void RegisterRealm(Realm realm)
        {
            if (realm == null)
            {
                Debug.LogError("[RealmManager] Cannot register null realm");
                return;
            }

            if (_realms.ContainsKey(realm.RealmID))
            {
                Debug.LogWarning($"[RealmManager] Realm {realm.RealmID} already registered");
                return;
            }

            _realms[realm.RealmID] = realm;
            OnRealmCreated?.Invoke(realm.RealmID);
            Debug.Log($"[RealmManager] Registered realm: {realm.Data.RealmName} (ID: {realm.RealmID})");
        }

        /// <summary>
        /// Unregister a realm from the manager.
        /// Called by Realm.OnDestroy().
        /// </summary>
        public void UnregisterRealm(int realmID)
        {
            if (_realms.Remove(realmID))
            {
                OnRealmDestroyed?.Invoke(realmID);
                Debug.Log($"[RealmManager] Unregistered realm: {realmID}");
            }
        }

        /// <summary>
        /// Get next available realm ID.
        /// </summary>
        public int GetNextRealmID()
        {
            return _nextRealmID++;
        }

        // ========== REALM LOOKUP ==========

        /// <summary>
        /// Get a realm by ID.
        /// </summary>
        public Realm GetRealm(int realmID)
        {
            return _realms.ContainsKey(realmID) ? _realms[realmID] : null;
        }

        /// <summary>
        /// Get all realms.
        /// </summary>
        public List<Realm> GetAllRealms()
        {
            return new List<Realm>(_realms.Values);
        }

        /// <summary>
        /// Get all independent realms (no overlord).
        /// </summary>
        public List<Realm> GetIndependentRealms()
        {
            var independent = new List<Realm>();
            foreach (var realm in _realms.Values)
            {
                if (realm.Data.IsIndependent)
                {
                    independent.Add(realm);
                }
            }
            return independent;
        }

        /// <summary>
        /// Get all vassal realms of a specific overlord.
        /// </summary>
        public List<Realm> GetVassalRealms(int overlordRealmID)
        {
            var vassals = new List<Realm>();
            var overlordRealm = GetRealm(overlordRealmID);

            if (overlordRealm == null || overlordRealm.Data.VassalContractIDs == null)
                return vassals;

            foreach (int contractID in overlordRealm.Data.VassalContractIDs)
            {
                var contract = ContractManager.Instance?.GetContract(contractID);
                if (contract != null)
                {
                    var vassal = GetRealm(contract.SubjectRealmID);
                    if (vassal != null)
                    {
                        vassals.Add(vassal);
                    }
                }
            }

            return vassals;
        }

        /// <summary>
        /// Get the overlord of a realm (if any).
        /// </summary>
        public Realm GetOverlordRealm(int subjectRealmID)
        {
            var subjectRealm = GetRealm(subjectRealmID);
            if (subjectRealm == null || subjectRealm.Data.IsIndependent)
                return null;

            var contract = ContractManager.Instance?.GetContract(subjectRealm.Data.OverlordContractID);
            if (contract == null)
                return null;

            return GetRealm(contract.OverlordRealmID);
        }

        // ========== REALM PROPERTIES ==========

        /// <summary>
        /// Change a realm's name.
        /// </summary>
        public void SetRealmName(int realmID, string newName)
        {
            var realm = GetRealm(realmID);
            if (realm == null)
                return;

            realm.Data.RealmName = newName;
            OnRealmNameChanged?.Invoke(realmID, newName);
        }

        /// <summary>
        /// Add a leader to a realm.
        /// </summary>
        public void AddRealmLeader(int realmID, int agentID)
        {
            var realm = GetRealm(realmID);
            if (realm == null)
                return;

            realm.Data.AddLeader(agentID);
            OnRealmLeaderChanged?.Invoke(realmID, agentID);
        }

        /// <summary>
        /// Remove a leader from a realm.
        /// </summary>
        public void RemoveRealmLeader(int realmID, int agentID)
        {
            var realm = GetRealm(realmID);
            if (realm == null)
                return;

            realm.Data.RemoveLeader(agentID);
        }

        // ========== CITY OWNERSHIP ==========

        /// <summary>
        /// Add a city to a realm's direct control.
        /// </summary>
        public void AddCityToRealm(int realmID, int cityID)
        {
            var realm = GetRealm(realmID);
            if (realm == null)
            {
                Debug.LogError($"[RealmManager] Cannot add city {cityID} - realm {realmID} not found");
                return;
            }

            realm.Data.AddCity(cityID);
            OnCityAddedToRealm?.Invoke(realmID, cityID);
            Debug.Log($"[RealmManager] Added city {cityID} to realm {realmID}");
        }

        /// <summary>
        /// Remove a city from a realm's direct control.
        /// </summary>
        public void RemoveCityFromRealm(int realmID, int cityID)
        {
            var realm = GetRealm(realmID);
            if (realm == null)
                return;

            realm.Data.RemoveCity(cityID);
            OnCityRemovedFromRealm?.Invoke(realmID, cityID);
            Debug.Log($"[RealmManager] Removed city {cityID} from realm {realmID}");
        }

        // ========== TAX & TRIBUTE COLLECTION ==========

        /// <summary>
        /// Collect taxes and tribute for all realms.
        /// Called daily by WorldTimeManager.
        /// </summary>
        public void CollectAllRealmRevenue()
        {
            foreach (var realm in _realms.Values)
            {
                if (realm.Treasury != null)
                {
                    // Collect from directly owned cities (100%)
                    realm.Treasury.CollectCityTaxes();

                    // Collect from vassals (contract %)
                    realm.Treasury.CollectVassalTribute();
                }
            }
        }

        /// <summary>
        /// Pay all office salaries for all realms.
        /// Called monthly.
        /// </summary>
        public void PayAllOfficeSalaries()
        {
            foreach (var realm in _realms.Values)
            {
                if (realm.Treasury != null)
                {
                    realm.Treasury.PayOfficeSalaries();
                }
            }
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Get total number of realms.
        /// </summary>
        public int GetRealmCount()
        {
            return _realms.Count;
        }

        /// <summary>
        /// Get total population across all realms.
        /// </summary>
        public int GetTotalWorldPopulation()
        {
            int total = 0;
            foreach (var realm in _realms.Values)
            {
                total += realm.Data.TotalPopulation;
            }
            return total;
        }

        // ========== DEBUG ==========

        [ContextMenu("Print All Realms")]
        public void PrintAllRealms()
        {
            Debug.Log($"[RealmManager] Total Realms: {_realms.Count}");
            foreach (var realm in _realms.Values)
            {
                Debug.Log($"  - {realm.Data.RealmName} (ID: {realm.RealmID}), Cities: {realm.Data.DirectlyOwnedCityIDs.Count}, Vassals: {realm.Data.VassalContractIDs.Count}");
            }
        }
    }
}
