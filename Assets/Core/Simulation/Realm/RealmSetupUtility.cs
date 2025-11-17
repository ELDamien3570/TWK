using System.Collections.Generic;
using UnityEngine;
using TWK.Government;
using TWK.Economy;

namespace TWK.Realms
{
    /// <summary>
    /// Utility class for setting up realms with all their component parts.
    /// Useful for testing and initialization scenarios.
    /// </summary>
    public static class RealmSetupUtility
    {
        /// <summary>
        /// Configuration data for creating a new realm.
        /// </summary>
        public class RealmSetupConfig
        {
            public string RealmName;
            public int[] CityIDs;
            public int[] LeaderIDs;

            // Government settings
            public RegimeForm RegimeForm = RegimeForm.Chiefdom;
            public StateStructure StateStructure = StateStructure.Territorial;
            public string GovernmentName;

            // Economic settings
            public int StartingGold = 1000;
            public Dictionary<ResourceType, int> StartingResources = new Dictionary<ResourceType, int>();

            // Culture
            public int CultureID = -1;

            // Tax law
            public TaxationLaw TaxationLaw = TaxationLaw.Tribute;
        }

        /// <summary>
        /// Create and fully configure a realm with all components.
        /// </summary>
        /// <param name="realm">The Realm MonoBehaviour to configure</param>
        /// <param name="config">Configuration data</param>
        public static void SetupRealm(Realm realm, RealmSetupConfig config)
        {
            if (realm == null)
            {
                Debug.LogError("[RealmSetupUtility] Cannot setup null realm");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[RealmSetupUtility] Cannot setup realm with null config");
                return;
            }

            // Set basic info
            realm.RealmName = config.RealmName;

            Debug.Log($"[RealmSetupUtility] Setting up realm: {config.RealmName}");

            // Add cities
            if (config.CityIDs != null && config.CityIDs.Length > 0)
            {
                foreach (int cityID in config.CityIDs)
                {
                    realm.AddCity(cityID);
                    Debug.Log($"[RealmSetupUtility]   - Added city {cityID}");
                }
            }

            // Add leaders
            if (config.LeaderIDs != null && config.LeaderIDs.Length > 0)
            {
                foreach (int leaderID in config.LeaderIDs)
                {
                    realm.AddLeader(leaderID);
                    Debug.Log($"[RealmSetupUtility]   - Added leader {leaderID}");
                }
            }

            // Set culture
            if (config.CultureID != -1)
            {
                realm.Data.RealmCultureID = config.CultureID;
                Debug.Log($"[RealmSetupUtility]   - Set culture ID: {config.CultureID}");
            }

            // Create government
            if (GovernmentManager.Instance != null)
            {
                string govName = string.IsNullOrEmpty(config.GovernmentName)
                    ? $"Government of {config.RealmName}"
                    : config.GovernmentName;

                var government = GovernmentManager.Instance.CreateGovernment(
                    realm.RealmID,
                    config.RegimeForm,
                    config.StateStructure,
                    govName
                );

                if (government != null)
                {
                    // Set taxation law
                    government.TaxationLaw = config.TaxationLaw;

                    realm.Data.GovernmentID = government.GovernmentID;
                    Debug.Log($"[RealmSetupUtility]   - Created government: {govName} ({config.RegimeForm} {config.StateStructure})");
                    Debug.Log($"[RealmSetupUtility]   - Set taxation law: {config.TaxationLaw}");
                }
                else
                {
                    Debug.LogWarning($"[RealmSetupUtility]   - Failed to create government");
                }
            }
            else
            {
                Debug.LogWarning("[RealmSetupUtility] GovernmentManager not found - skipping government setup");
            }

            // Initialize treasury
            if (realm.Treasury != null)
            {
                // Add starting gold
                realm.Treasury.AddResource(ResourceType.Gold, config.StartingGold);
                Debug.Log($"[RealmSetupUtility]   - Added {config.StartingGold} gold to treasury");

                // Add other starting resources
                foreach (var kvp in config.StartingResources)
                {
                    realm.Treasury.AddResource(kvp.Key, kvp.Value);
                    Debug.Log($"[RealmSetupUtility]   - Added {kvp.Value} {kvp.Key} to treasury");
                }
            }
            else
            {
                Debug.LogWarning("[RealmSetupUtility] Realm treasury not initialized yet");
            }

            Debug.Log($"[RealmSetupUtility] Realm setup complete: {config.RealmName}");
        }

        /// <summary>
        /// Quick setup method with just the essentials.
        /// </summary>
        public static void QuickSetup(
            Realm realm,
            string realmName,
            int[] cityIDs = null,
            int[] leaderIDs = null,
            RegimeForm regimeForm = RegimeForm.Autocratic,
            StateStructure stateStructure = StateStructure.Territorial,
            int startingGold = 1000)
        {
            var config = new RealmSetupConfig
            {
                RealmName = realmName,
                CityIDs = cityIDs ?? new int[0],
                LeaderIDs = leaderIDs ?? new int[0],
                RegimeForm = regimeForm,
                StateStructure = stateStructure,
                StartingGold = startingGold
            };

            SetupRealm(realm, config);
        }

        /// <summary>
        /// Create a vassal relationship between two realms.
        /// </summary>
        public static void CreateVassalRelationship(
            int overlordRealmID,
            int vassalRealmID,
            ContractType contractType = ContractType.Vassal,
            float goldPercentage = 50f,
            float levyPercentage = 25f,
            float initialLoyalty = 75f)
        {
            if (ContractManager.Instance == null)
            {
                Debug.LogError("[RealmSetupUtility] ContractManager not found - cannot create vassal relationship");
                return;
            }

            var contract = ContractManager.Instance.CreateContract(
                overlordRealmID,
                vassalRealmID,
                contractType
            );

            if (contract != null)
            {
                contract.GoldPercentage = goldPercentage;
                contract.ManpowerPercentage = levyPercentage;
                contract.CurrentLoyalty = initialLoyalty;

                Debug.Log($"[RealmSetupUtility] Created {contractType} contract: Realm {vassalRealmID} -> Realm {overlordRealmID} " +
                    $"(Gold: {goldPercentage}%, Levy: {levyPercentage}%, Loyalty: {initialLoyalty}%)");
            }
            else
            {
                Debug.LogWarning($"[RealmSetupUtility] Failed to create vassal contract");
            }
        }

        /// <summary>
        /// Add offices to a realm's government.
        /// </summary>
        public static void AddOffices(int realmID, params string[] officeNames)
        {
            if (GovernmentManager.Instance == null)
            {
                Debug.LogError("[RealmSetupUtility] GovernmentManager not found");
                return;
            }

            var government = GovernmentManager.Instance.GetRealmGovernment(realmID);
            if (government == null)
            {
                Debug.LogError($"[RealmSetupUtility] Government not found for realm {realmID}");
                return;
            }

            foreach (string officeName in officeNames)
            {
                var office = GovernmentManager.Instance.CreateOffice(realmID, officeName, Cultures.TreeType.Economics, OfficePurpose.ManageTaxCollection);
                if (office != null)
                {
                    Debug.Log($"[RealmSetupUtility]   - Created office: {officeName}");
                }
            }
        }

        /// <summary>
        /// Assign an agent to an office.
        /// </summary>
        public static void AssignOffice(int realmID, string officeName, int agentID)
        {
            if (GovernmentManager.Instance == null)
            {
                Debug.LogError("[RealmSetupUtility] GovernmentManager not found");
                return;
            }

            var offices = GovernmentManager.Instance.GetRealmOffices(realmID);
            if (offices == null)
            {
                Debug.LogError($"[RealmSetupUtility] No offices found for realm {realmID}");
                return;
            }

            foreach (var office in offices)
            {
                if (office.OfficeName == officeName)
                {
                    office.AssignedAgentID = agentID;
                    Debug.Log($"[RealmSetupUtility] Assigned agent {agentID} to office {officeName}");
                    return;
                }
            }

            Debug.LogWarning($"[RealmSetupUtility] Office '{officeName}' not found in realm {realmID}");
        }
    }
}
