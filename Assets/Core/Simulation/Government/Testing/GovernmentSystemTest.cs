using System.Collections.Generic;
using UnityEngine;
using TWK.Government;
using TWK.Modifiers;
using TWK.Cultures;

namespace TWK.Testing
{
    /// <summary>
    /// Test script to set up and verify the government system.
    /// Attach to a GameObject in your test scene and run Play mode.
    /// </summary>
    public class GovernmentSystemTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoRunTests = true;
        [SerializeField] private bool createTestData = true;
        [SerializeField] private int testRealmID = 1;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private List<Institution> testInstitutions = new List<Institution>();
        private List<GovernmentData> testGovernments = new List<GovernmentData>();
        private List<Edict> testEdicts = new List<Edict>();

        private void Start()
        {
            if (autoRunTests)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== GOVERNMENT SYSTEM TEST STARTING ===");

            if (createTestData)
            {
                CreateTestInstitutions();
                CreateTestGovernments();
                CreateTestEdicts();
            }

            TestGovernmentManager();
            TestContractManager();
            TestOfficeSystem();
            TestEdictSystem();

            Debug.Log("=== GOVERNMENT SYSTEM TEST COMPLETE ===");
        }

        // ========== TEST DATA CREATION ==========

        private void CreateTestInstitutions()
        {
            Log("Creating test institutions...");

            // Institution 1: Bureaucracy
            var bureaucracy = ScriptableObject.CreateInstance<Institution>();
            bureaucracy.InstitutionName = "Bureaucratic Administration";
            bureaucracy.Description = "A professional civil service that handles day-to-day governance.";
            bureaucracy.Category = InstitutionCategory.Economics;
            bureaucracy.ReformCost = 500;
            bureaucracy.Modifiers = new List<Modifier>
            {
                CreateModifier("Administrative Efficiency", ModifierEffectType.BuildingEfficiency, 0.15f, true)
            };
            testInstitutions.Add(bureaucracy);

            // Institution 2: Standing Army
            var standingArmy = ScriptableObject.CreateInstance<Institution>();
            standingArmy.InstitutionName = "Standing Army";
            standingArmy.Description = "Professional soldiers maintained year-round.";
            standingArmy.Category = InstitutionCategory.Warfare;
            standingArmy.ReformCost = 800;
            standingArmy.Modifiers = new List<Modifier>
            {
                CreateModifier("Military Strength", ModifierEffectType.MilitaryPower, 0.25f, true)
            };
            testInstitutions.Add(standingArmy);

            // Institution 3: Theocracy
            var theocracy = ScriptableObject.CreateInstance<Institution>();
            theocracy.InstitutionName = "Divine Theocracy";
            theocracy.Description = "Religious leaders hold political power.";
            theocracy.Category = InstitutionCategory.Religion;
            theocracy.ReformCost = 1000;
            theocracy.Modifiers = new List<Modifier>
            {
                CreateModifier("Religious Fervor", ModifierEffectType.PopulationFervor, 0.20f, true)
            };
            testInstitutions.Add(theocracy);

            Log($"Created {testInstitutions.Count} test institutions");
        }

        private void CreateTestGovernments()
        {
            Log("Creating test governments...");

            // Government 1: Feudal Monarchy
            var feudalMonarchy = ScriptableObject.CreateInstance<GovernmentData>();
            feudalMonarchy.GovernmentName = "Feudal Monarchy";
            feudalMonarchy.Description = "A hereditary monarchy with vassal lords managing territories.";
            feudalMonarchy.RegimeForm = RegimeForm.Autocratic;
            feudalMonarchy.StateStructure = StateStructure.Territorial;
            feudalMonarchy.SuccessionLaw = SuccessionLaw.Hereditary;
            feudalMonarchy.Administration = Administration.Patrimonial;
            feudalMonarchy.Mobility = Mobility.Sedentary;
            feudalMonarchy.MilitaryService = MilitaryServiceLaw.Levies | MilitaryServiceLaw.Warbands;
            feudalMonarchy.TaxationLaw = TaxationLaw.TaxCollectors;
            feudalMonarchy.TradeLaw = TradeLaw.NoControl;
            feudalMonarchy.JusticeLaw = JusticeLaw.Fair;
            feudalMonarchy.BaseCapacity = 35f;
            feudalMonarchy.BaseLegitimacy = 60f;
            feudalMonarchy.MinimumProvinces = 3;
            feudalMonarchy.Institutions = new List<Institution> { testInstitutions[0] }; // Bureaucracy
            testGovernments.Add(feudalMonarchy);

            // Government 2: Tribal Chiefdom
            var tribalChiefdom = ScriptableObject.CreateInstance<GovernmentData>();
            tribalChiefdom.GovernmentName = "Tribal Chiefdom";
            tribalChiefdom.Description = "Led by a chief with tribal assemblies making decisions.";
            tribalChiefdom.RegimeForm = RegimeForm.Chiefdom;
            tribalChiefdom.StateStructure = StateStructure.Territorial;
            tribalChiefdom.SuccessionLaw = SuccessionLaw.Kinship;
            tribalChiefdom.Administration = Administration.Rational;
            tribalChiefdom.Mobility = Mobility.Nomadic;
            tribalChiefdom.MilitaryService = MilitaryServiceLaw.Warbands;
            tribalChiefdom.TaxationLaw = TaxationLaw.Tribute;
            tribalChiefdom.TradeLaw = TradeLaw.NoControl;
            tribalChiefdom.JusticeLaw = JusticeLaw.Severe;
            tribalChiefdom.BaseCapacity = 20f;
            tribalChiefdom.BaseLegitimacy = 50f;
            tribalChiefdom.MinimumProvinces = 1;
            tribalChiefdom.Institutions = new List<Institution>();
            testGovernments.Add(tribalChiefdom);

            // Government 3: Theocratic Empire
            var theocraticEmpire = ScriptableObject.CreateInstance<GovernmentData>();
            theocraticEmpire.GovernmentName = "Theocratic Empire";
            theocraticEmpire.Description = "A divine ruler governs through religious authority.";
            theocraticEmpire.RegimeForm = RegimeForm.Autocratic;
            theocraticEmpire.StateStructure = StateStructure.Territorial;
            theocraticEmpire.SuccessionLaw = SuccessionLaw.Divinity;
            theocraticEmpire.Administration = Administration.Patrimonial;
            theocraticEmpire.Mobility = Mobility.Sedentary;
            theocraticEmpire.MilitaryService = MilitaryServiceLaw.Levies | MilitaryServiceLaw.ProfessionalArmies;
            theocraticEmpire.TaxationLaw = TaxationLaw.TaxCollectors;
            theocraticEmpire.TradeLaw = TradeLaw.StateMonopoly;
            theocraticEmpire.JusticeLaw = JusticeLaw.Severe;
            theocraticEmpire.BaseCapacity = 50f;
            theocraticEmpire.BaseLegitimacy = 70f;
            theocraticEmpire.MinimumProvinces = 5;
            theocraticEmpire.MinimumCapacity = 40f;
            theocraticEmpire.Institutions = new List<Institution> { testInstitutions[0], testInstitutions[2] }; // Bureaucracy + Theocracy
            testGovernments.Add(theocraticEmpire);

            Log($"Created {testGovernments.Count} test governments");
        }

        private void CreateTestEdicts()
        {
            Log("Creating test edicts...");

            // Edict 1: Forced Labor
            var forcedLabor = new Edict("Forced Labor", "Conscript laborers for major projects.");
            forcedLabor.LaborerLoyalty = -15f;
            forcedLabor.SlaveLoyalty = -10f;
            forcedLabor.NobleLoyalty = 5f;
            forcedLabor.EnactmentCost = 200;
            forcedLabor.MonthlyMaintenance = 50;
            forcedLabor.IsPermanent = false;
            forcedLabor.DurationMonths = 6;
            forcedLabor.Modifiers = new List<Modifier>
            {
                CreateModifier("Forced Labor Boost", ModifierEffectType.BuildingConstructionSpeed, 0.30f, true)
            };
            testEdicts.Add(forcedLabor);

            // Edict 2: Tax Relief
            var taxRelief = new Edict("Tax Relief", "Reduce taxes to improve public happiness.");
            taxRelief.LaborerLoyalty = 10f;
            taxRelief.ArtisanLoyalty = 8f;
            taxRelief.MerchantLoyalty = 12f;
            taxRelief.NobleLoyalty = -5f;
            taxRelief.EnactmentCost = 100;
            taxRelief.MonthlyMaintenance = 0;
            taxRelief.IsPermanent = false;
            taxRelief.DurationMonths = 12;
            testEdicts.Add(taxRelief);

            // Edict 3: Religious Devotion
            var religiousDevotion = new Edict("Religious Devotion", "Mandate religious observance.");
            religiousDevotion.ClergyLoyalty = 20f;
            religiousDevotion.LaborerLoyalty = -5f;
            religiousDevotion.EnactmentCost = 300;
            religiousDevotion.MonthlyMaintenance = 100;
            religiousDevotion.IsPermanent = false;
            religiousDevotion.DurationMonths = 24;
            religiousDevotion.Modifiers = new List<Modifier>
            {
                CreateModifier("Divine Blessing", ModifierEffectType.PopulationFervor, 0.15f, true)
            };
            testEdicts.Add(religiousDevotion);

            Log($"Created {testEdicts.Count} test edicts");
        }

        private Modifier CreateModifier(string name, ModifierEffectType effectType, float value, bool isPercentage)
        {
            var modifier = new Modifier
            {
                Name = name,
                Description = $"{name} effect",
                Duration = ModifierDuration.Permanent,
                Effects = new List<ModifierEffect>
                {
                    new ModifierEffect
                    {
                        EffectType = effectType,
                        Value = value,
                        IsPercentage = isPercentage
                    }
                }
            };
            return modifier;
        }

        // ========== MANAGER TESTS ==========

        private void TestGovernmentManager()
        {
            Log("\n--- Testing GovernmentManager ---");

            if (GovernmentManager.Instance == null)
            {
                LogError("GovernmentManager.Instance is null! Ensure it exists in the scene.");
                return;
            }

            // Register test governments
            foreach (var gov in testGovernments)
            {
                GovernmentManager.Instance.RegisterGovernment(gov);
            }

            // Set government for test realm
            GovernmentManager.Instance.SetRealmGovernment(testRealmID, testGovernments[0]);
            Log($"Set realm {testRealmID} to {testGovernments[0].GovernmentName}");

            // Test legitimacy and capacity
            float legitimacy = GovernmentManager.Instance.GetRealmLegitimacy(testRealmID);
            float capacity = GovernmentManager.Instance.GetRealmCapacity(testRealmID);
            Log($"Realm {testRealmID} - Legitimacy: {legitimacy:F1}, Capacity: {capacity:F1}");

            // Test legitimacy modification
            GovernmentManager.Instance.ModifyLegitimacy(testRealmID, 10f);
            legitimacy = GovernmentManager.Instance.GetRealmLegitimacy(testRealmID);
            Log($"After +10 modification - Legitimacy: {legitimacy:F1}");

            Log("GovernmentManager tests passed!");
        }

        private void TestContractManager()
        {
            Log("\n--- Testing ContractManager ---");

            if (ContractManager.Instance == null)
            {
                LogError("ContractManager.Instance is null! Ensure it exists in the scene.");
                return;
            }

            // Create test contracts
            var vassalContract = ContractManager.Instance.CreateContract(testRealmID, ContractType.Vassal);
            vassalContract.SetSubjectRealm(testRealmID + 1);
            Log($"Created Vassal contract {vassalContract.ContractID} for realm {testRealmID}");

            var governorContract = ContractManager.Instance.CreateContract(testRealmID, ContractType.Governor);
            governorContract.SetSubjectAgent(100); // Test agent ID
            Log($"Created Governor contract {governorContract.ContractID} for realm {testRealmID}");

            // Test contract retrieval
            var parentContracts = ContractManager.Instance.GetContractsAsParent(testRealmID);
            Log($"Realm {testRealmID} has {parentContracts.Count} contracts as parent");

            // Test loyalty
            float loyalty = vassalContract.CurrentLoyalty;
            Log($"Vassal contract loyalty: {loyalty:F1} ({vassalContract.GetLoyaltyStatus()})");

            Log("ContractManager tests passed!");
        }

        private void TestOfficeSystem()
        {
            Log("\n--- Testing Office System ---");

            if (GovernmentManager.Instance == null) return;

            // Create test offices
            var taxOffice = GovernmentManager.Instance.CreateOffice(
                testRealmID,
                "Tax Collector",
                TreeType.Economics,
                OfficePurpose.ManageTaxCollection
            );
            Log($"Created office: {taxOffice.OfficeName}");

            var loyaltyOffice = GovernmentManager.Instance.CreateOffice(
                testRealmID,
                "Slave Overseer",
                TreeType.Politics,
                OfficePurpose.RaiseLoyaltyInSlaveGroups
            );
            Log($"Created office: {loyaltyOffice.OfficeName}");

            // Test office assignment
            GovernmentManager.Instance.AssignOffice(testRealmID, taxOffice, 101); // Test agent ID
            Log($"Assigned agent 101 to {taxOffice.OfficeName}, efficiency: {taxOffice.CurrentEfficiency:F2}");

            // Get office stats
            var offices = GovernmentManager.Instance.GetRealmOffices(testRealmID);
            var filled = GovernmentManager.Instance.GetFilledOffices(testRealmID);
            var vacant = GovernmentManager.Instance.GetVacantOffices(testRealmID);
            Log($"Offices - Total: {offices.Count}, Filled: {filled.Count}, Vacant: {vacant.Count}");

            int monthlyCost = GovernmentManager.Instance.GetMonthlyOfficeCosts(testRealmID);
            Log($"Monthly office costs: {monthlyCost} gold");

            Log("Office System tests passed!");
        }

        private void TestEdictSystem()
        {
            Log("\n--- Testing Edict System ---");

            if (GovernmentManager.Instance == null) return;

            // Enact test edict
            bool enacted = GovernmentManager.Instance.EnactEdict(testRealmID, testEdicts[0]);
            if (enacted)
            {
                Log($"Successfully enacted: {testEdicts[0].EdictName}");
            }
            else
            {
                LogWarning($"Failed to enact: {testEdicts[0].EdictName}");
            }

            // Get active edicts
            var activeEdicts = GovernmentManager.Instance.GetActiveEdicts(testRealmID);
            Log($"Active edicts: {activeEdicts.Count}");

            foreach (var edict in activeEdicts)
            {
                Log($"  - {edict.EdictName} (expires in {edict.GetMonthsRemaining(0)} months)");
            }

            Log("Edict System tests passed!");
        }

        // ========== UTILITY METHODS ==========

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[GovernmentTest] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GovernmentTest] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GovernmentTest] {message}");
        }

        // ========== MANUAL TEST METHODS ==========

        [ContextMenu("Test 1: Create Government")]
        public void ManualTestCreateGovernment()
        {
            CreateTestInstitutions();
            CreateTestGovernments();
            TestGovernmentManager();
        }

        [ContextMenu("Test 2: Create Contracts")]
        public void ManualTestCreateContracts()
        {
            TestContractManager();
        }

        [ContextMenu("Test 3: Create Offices")]
        public void ManualTestCreateOffices()
        {
            TestOfficeSystem();
        }

        [ContextMenu("Test 4: Enact Edicts")]
        public void ManualTestEnactEdicts()
        {
            CreateTestEdicts();
            TestEdictSystem();
        }

        [ContextMenu("Test 5: Modify Legitimacy")]
        public void ManualTestModifyLegitimacy()
        {
            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.ModifyLegitimacy(testRealmID, -20f);
                Log($"Reduced legitimacy by 20. New value: {GovernmentManager.Instance.GetRealmLegitimacy(testRealmID):F1}");
            }
        }

        [ContextMenu("Test 6: Calculate Revolt Risk")]
        public void ManualTestRevoltRisk()
        {
            if (GovernmentManager.Instance != null)
            {
                float risk = GovernmentManager.Instance.CalculateRevoltRisk(testRealmID);
                Log($"Revolt risk for realm {testRealmID}: {risk:F1}%");
            }
        }
    }
}
