using UnityEngine;

using TWK.Agents;
using TWK.Realms;
using TWK.Realms.Demographics;
using TWK.Simulation;
using TWK.Economy;
using TWK.UI.ViewModels;
using TWK.Cultures;
using TWK.Religion;
using TWK.Government;


namespace TWK.Core
{

    public class SimulationManager : MonoBehaviour
    {
        [SerializeField] private WorldTimeManager worldTimeManager;
        [SerializeField] private GameObject realmPrefab;
        [SerializeField] private GameObject agentPrefab;

        //For testing purposes only, should be reworked later
        [SerializeField] private Realm[] testRealms;
        [SerializeField] private Agent[] testAgents;
        [SerializeField] private City[] testCities;

        [SerializeField] private CultureData testCulture;
        [SerializeField] private ReligionData testReligion;

        [Header("Realm Setup Options")]
        [Tooltip("Automatically setup realms with government, cities, and treasury on start")]
        [SerializeField] private bool autoSetupRealms = false;
        [SerializeField] private int startingGold = 10000;
        [SerializeField] private bool createTestOffices = true;

        private void Start()
        {
            if (worldTimeManager == null)
                worldTimeManager = WorldTimeManager.Instance;

            if (PopulationManager.Instance != null)
                PopulationManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("PopulationManager not found in scene!");

            //if (ResourceManager.Instance != null)
            //    ResourceManager.Instance.Initialize(worldTimeManager);
            //else
            //    Debug.LogError("ResourceManager not found in scene!");

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("BuildingManager not found in scene!");

            if (CultureManager.Instance != null)
                CultureManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("CultureManager not found in scene!");

            if (ReligionManager.Instance != null)
                ReligionManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("ReligionManager not found in scene!");

            if (ContractManager.Instance != null)
                ContractManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogWarning("ContractManager not found in scene! Contracts will not process.");

            if (GovernmentManager.Instance != null)
                GovernmentManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogWarning("GovernmentManager not found in scene! Government systems will not work.");

            // Initialize ViewModelService for UI
            if (ViewModelService.Instance != null)
                ViewModelService.Instance.Initialize(worldTimeManager);
            else
                Debug.LogWarning("ViewModelService not found in scene! UI will not be updated.");

            TestStart();


        }

        #region Test Setup

        /// <summary>
        /// Setup a test realm with government, cities, and treasury.
        /// Call this after realms and cities are initialized.
        /// Example usage in TestStart() or via Context Menu.
        /// </summary>
        [ContextMenu("Setup Test Realm")]
        public void SetupTestRealm()
        {
            if (testRealms.Length == 0)
            {
                Debug.LogWarning("[SimulationManager] No test realms available");
                return;
            }

            var realm = testRealms[0];

            // Collect city IDs
            int[] cityIDs = new int[testCities.Length];
            for (int i = 0; i < testCities.Length; i++)
            {
                cityIDs[i] = testCities[i].CityID;
            }

            // Collect leader IDs
            int[] leaderIDs = new int[testAgents.Length];
            for (int i = 0; i < testAgents.Length; i++)
            {
                leaderIDs[i] = testAgents[i].GetAgentId();
            }

            // Create setup configuration
            var config = new RealmSetupUtility.RealmSetupConfig
            {
                RealmName = "",
                CityIDs = cityIDs,
                LeaderIDs = leaderIDs,
                RegimeForm = Government.RegimeForm.Autocratic,
                StateStructure = Government.StateStructure.Territorial,
                GovernmentName = "Royal Government",
                TaxationLaw = Government.TaxationLaw.Tribute,
                StartingGold = startingGold,
                CultureID = testCulture != null ? testCulture.GetCultureID() : -1
            };

            // Add starting resources
            config.StartingResources[ResourceType.Food] = 500000;
            config.StartingResources[ResourceType.Gold] = 2000;
            config.StartingResources[ResourceType.Piety] = 1000;
            config.StartingResources[ResourceType.Prestige] = 10000;

            // Setup the realm
            RealmSetupUtility.SetupRealm(realm, config);

            // Add some offices if enabled
            if (createTestOffices)
            {
                RealmSetupUtility.AddOffices(realm.RealmID,
                    "Chancellor",
                    "Marshal",
                    "Steward",
                    "Spymaster"
                );

                // Assign first agent to Chancellor if available
                if (leaderIDs.Length > 0)
                {
                    RealmSetupUtility.AssignOffice(realm.RealmID, "Chancellor", leaderIDs[0]);
                }
            }

            Debug.Log("[SimulationManager] Test realm setup complete!");
        }

        private void TestStart()
        {
            // Initialize test religion
            if (testReligion != null)
            {
                ReligionManager.Instance.RegisterReligion(testReligion);
            }

            // Initialize realms
            if (testRealms.Length > 0)
            {
                foreach (var realm in testRealms)
                {
                    realm.Initialize(worldTimeManager);
                }
            }

            // Initialize cities
            if (testCities.Length > 0)
            {
                foreach (var city in testCities)
                {
                    city.Initialize(worldTimeManager);

                    // Register test populations per city
                    if (PopulationManager.Instance != null && testCulture != null && testReligion != null)
                    {
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Laborer, 14671, testCulture, testReligion, 25);
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Artisan, 142, testCulture, testReligion, 32);
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Noble, 68, testCulture, testReligion, 43);
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Merchant, 24, testCulture, testReligion, 38);
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Clergy, 38, testCulture, testReligion, 29);
                        PopulationManager.Instance.RegisterPopulationWithReligion(city.CityID, PopulationArchetypes.Slave, 1483, testCulture, testReligion, 21);

                        // Recalculate city culture after populations are registered
                        if (CultureManager.Instance != null)
                        {
                            CultureManager.Instance.CalculateCityCulture(city.CityID);
                        }
                    }

                    // Register city with ViewModelService
                    if (ViewModelService.Instance != null)
                        ViewModelService.Instance.RegisterCity(city);
                }
            }

            // Initialize agents
            if (testAgents.Length > 0)
            {
                foreach (var agent in testAgents)
                {
                    agent.Initialize(worldTimeManager);
                }
            }

            // Auto-setup realms if enabled (assigns cities, creates government, etc.)
            if (autoSetupRealms && testRealms.Length > 0)
            {
                SetupTestRealm();
            }
        }

        /// <summary>
        /// Create a vassal realm for testing.
        /// Example: CreateTestVassal(overlordRealmID, "Duchy of Test", new int[] { cityID })
        /// </summary>
        [ContextMenu("Create Test Vassal Realm")]
        public void CreateTestVassalExample()
        {
            if (testRealms.Length < 2)
            {
                Debug.LogWarning("[SimulationManager] Need at least 2 test realms to create vassal relationship");
                return;
            }

            var overlord = testRealms[0];
            var vassal = testRealms[1];

            // Setup vassal realm
            var config = new RealmSetupUtility.RealmSetupConfig
            {
                RealmName = "Duchy of Vassal",
                CityIDs = new int[0], // Give it specific cities if needed
                LeaderIDs = new int[0],
                RegimeForm = Government.RegimeForm.Pluralist,
                StateStructure = Government.StateStructure.CityState,
                GovernmentName = "Ducal Government",
                StartingGold = 5000
            };

            RealmSetupUtility.SetupRealm(vassal, config);

            // Create vassal relationship
            RealmSetupUtility.CreateVassalRelationship(
                overlordRealmID: overlord.RealmID,
                vassalRealmID: vassal.RealmID,
                contractType: Government.ContractType.Vassal,
                goldPercentage: 50f,
                levyPercentage: 25f,
                initialLoyalty: 75f
            );

            Debug.Log($"[SimulationManager] Created vassal relationship: {vassal.RealmName} -> {overlord.RealmName}");
        }
        #endregion
    }
}
