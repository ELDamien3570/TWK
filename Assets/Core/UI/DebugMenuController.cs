using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TWK.Agents;
using TWK.Realms;
using TWK.Core;
using TWK.Government;
using TWK.Cultures;
using TWK.Religion;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// Debug menu for testing game mechanics.
    /// Provides quick access to spawn entities, trigger events, and test systems.
    /// </summary>
    public class DebugMenuController : MonoBehaviour
    {
        // ========== AGENT TESTING ==========
        [Header("Agent Testing")]
        [SerializeField] private Button spawnAgentButton;
        [SerializeField] private Button killCurrentAgentButton;
        [SerializeField] private Button ageAgentButton;
        [SerializeField] private Button addGoldToAgentButton;
        [SerializeField] private TMP_InputField goldAmountInput;

        // ========== REALM TESTING ==========
        [Header("Realm Testing")]
        [SerializeField] private Button addGoldToRealmButton;
        [SerializeField] private Button addResourcesButton;
        [SerializeField] private Button createVassalButton;
        [SerializeField] private TMP_InputField realmGoldAmountInput;

        // ========== RELATIONSHIP TESTING ==========
        [Header("Relationship Testing")]
        [SerializeField] private Button createFriendshipButton;
        [SerializeField] private Button createRivalryButton;
        [SerializeField] private Button modifyRelationshipButton;
        [SerializeField] private TMP_Dropdown targetAgentDropdown;
        [SerializeField] private TMP_InputField relationshipStrengthInput;

        // ========== CONTRACT TESTING ==========
        [Header("Contract Testing")]
        [SerializeField] private Button createGovernorContractButton;
        [SerializeField] private Button terminateContractButton;
        [SerializeField] private Button modifyLoyaltyButton;
        [SerializeField] private TMP_InputField loyaltyModInput;

        // ========== OFFICE TESTING ==========
        [Header("Office Testing")]
        [SerializeField] private Button assignToOfficeButton;
        [SerializeField] private Button removeFromOfficeButton;
        [SerializeField] private TMP_Dropdown officeDropdown;

        // ========== TIME TESTING ==========
        [Header("Time Testing")]
        [SerializeField] private Button advanceDayButton;
        [SerializeField] private Button advanceSeasonButton;
        [SerializeField] private Button advanceYearButton;
        [SerializeField] private Button setSpeed1xButton;
        [SerializeField] private Button setSpeed5xButton;
        [SerializeField] private Button setSpeed10xButton;

        // ========== LOGGING ==========
        [Header("Logging")]
        [SerializeField] private Button logPlayerInfoButton;
        [SerializeField] private Button logAllAgentsButton;
        [SerializeField] private Button logAllRealmsButton;
        [SerializeField] private Button logAllContractsButton;
        [SerializeField] private Button logAllRelationshipsButton;
        [SerializeField] private TextMeshProUGUI debugOutputText;

        // ========== INITIALIZATION ==========

        private void Start()
        {
            SetupButtons();
            PopulateDropdowns();
        }

        private void SetupButtons()
        {
            // Agent testing
            spawnAgentButton?.onClick.AddListener(SpawnTestAgent);
            killCurrentAgentButton?.onClick.AddListener(KillCurrentAgent);
            ageAgentButton?.onClick.AddListener(AgeCurrentAgent);
            addGoldToAgentButton?.onClick.AddListener(AddGoldToCurrentAgent);

            // Realm testing
            addGoldToRealmButton?.onClick.AddListener(AddGoldToCurrentRealm);
            addResourcesButton?.onClick.AddListener(AddResourcesToRealm);
            createVassalButton?.onClick.AddListener(CreateTestVassal);

            // Relationship testing
            createFriendshipButton?.onClick.AddListener(CreateFriendship);
            createRivalryButton?.onClick.AddListener(CreateRivalry);
            modifyRelationshipButton?.onClick.AddListener(ModifyRelationship);

            // Contract testing
            createGovernorContractButton?.onClick.AddListener(CreateGovernorContract);
            terminateContractButton?.onClick.AddListener(TerminateContract);
            modifyLoyaltyButton?.onClick.AddListener(ModifyContractLoyalty);

            // Office testing
            assignToOfficeButton?.onClick.AddListener(AssignToOffice);
            removeFromOfficeButton?.onClick.AddListener(RemoveFromOffice);

            // Time testing
            advanceDayButton?.onClick.AddListener(AdvanceDay);
            advanceSeasonButton?.onClick.AddListener(AdvanceSeason);
            advanceYearButton?.onClick.AddListener(AdvanceYear);
            setSpeed1xButton?.onClick.AddListener(() => SetTimeSpeed(1f));
            setSpeed5xButton?.onClick.AddListener(() => SetTimeSpeed(5f));
            setSpeed10xButton?.onClick.AddListener(() => SetTimeSpeed(10f));

            // Logging
            logPlayerInfoButton?.onClick.AddListener(LogPlayerInfo);
            logAllAgentsButton?.onClick.AddListener(LogAllAgents);
            logAllRealmsButton?.onClick.AddListener(LogAllRealms);
            logAllContractsButton?.onClick.AddListener(LogAllContracts);
            logAllRelationshipsButton?.onClick.AddListener(LogAllRelationships);
        }

        private void PopulateDropdowns()
        {
            // Populate agent dropdown
            if (targetAgentDropdown != null && AgentManager.Instance != null)
            {
                targetAgentDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();

                foreach (var agent in AgentManager.Instance.GetAllAgents())
                {
                    options.Add($"{agent.Data.AgentName} (ID: {agent.Data.AgentID})");
                }

                targetAgentDropdown.AddOptions(options);
            }

            // Populate office dropdown
            if (officeDropdown != null && GameUIController.Instance != null)
            {
                officeDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();

                int realmID = GameUIController.Instance.CurrentPlayerRealmID;
                if (GovernmentManager.Instance != null)
                {
                    var offices = GovernmentManager.Instance.GetRealmOffices(realmID);
                    if (offices != null)
                    {
                        foreach (var office in offices)
                        {
                            options.Add($"{office.OfficeName} (ID: {office.OfficeID})");
                        }
                    }
                }

                officeDropdown.AddOptions(options);
            }
        }

        // ========== AGENT TESTING ==========

        private void SpawnTestAgent()
        {
            if (AgentManager.Instance == null || GameUIController.Instance == null)
            {
                LogDebug("Cannot spawn agent - managers not found");
                return;
            }

            int realmID = GameUIController.Instance.CurrentPlayerRealmID;
            int cultureID = GameUIController.Instance.GetPlayerAgent()?.Data.CultureID ?? 0;
            int religionID = GameUIController.Instance.GetPlayerAgent()?.Data.ReligionID ?? 0;

            var agentData = new AgentData(
                AgentManager.Instance.GetNextAgentID(),
                $"Test Agent {Random.Range(100, 999)}",
                realmID,
                cultureID,
                religionID
            );

            agentData.Age = Random.Range(18, 45);
            agentData.BirthYear = WorldTimeManager.Instance.CurrentYear - agentData.Age;

            var agent = AgentManager.Instance.CreateAgent(agentData);
            LogDebug($"Spawned agent: {agent.Data.AgentName} (ID: {agent.Data.AgentID})");
        }

        private void KillCurrentAgent()
        {
            if (GameUIController.Instance == null) return;

            int agentID = GameUIController.Instance.CurrentPlayerAgentID;
            var agent = AgentManager.Instance?.GetAgent(agentID);

            if (agent != null)
            {
                agent.Data.IsAlive = false;
                AgentManager.Instance.ProcessAgentDeath(agentID, -1);
                LogDebug($"Killed agent: {agent.Data.AgentName}");
            }
        }

        private void AgeCurrentAgent()
        {
            if (GameUIController.Instance == null) return;

            var agent = GameUIController.Instance.GetPlayerAgent();
            if (agent != null)
            {
                agent.Data.Age += 10;
                LogDebug($"Aged {agent.Data.AgentName} by 10 years (now {agent.Data.Age})");
            }
        }

        private void AddGoldToCurrentAgent()
        {
            if (GameUIController.Instance == null) return;

            int amount = 1000;
            if (goldAmountInput != null && int.TryParse(goldAmountInput.text, out int parsed))
            {
                amount = parsed;
            }

            int agentID = GameUIController.Instance.CurrentPlayerAgentID;
            var ledger = AgentManager.Instance?.GetAgentLedger(agentID);

            if (ledger != null)
            {
                ledger.AddGold(amount);
                LogDebug($"Added {amount} gold to agent (new total: {ledger.GetTotalWealth()})");
            }
        }

        // ========== REALM TESTING ==========

        private void AddGoldToCurrentRealm()
        {
            if (GameUIController.Instance == null) return;

            int amount = 10000;
            if (realmGoldAmountInput != null && int.TryParse(realmGoldAmountInput.text, out int parsed))
            {
                amount = parsed;
            }

            var realm = GameUIController.Instance.GetPlayerRealm();
            if (realm != null)
            {
                realm.Treasury.AddResource(Economy.ResourceType.Gold, amount);
                LogDebug($"Added {amount} gold to realm {realm.RealmName}");
            }
        }

        private void AddResourcesToRealm()
        {
            var realm = GameUIController.Instance?.GetPlayerRealm();
            if (realm != null)
            {
                realm.Treasury.AddResource(Economy.ResourceType.Food, 50000);
                realm.Treasury.AddResource(Economy.ResourceType.Piety, 1000);
                realm.Treasury.AddResource(Economy.ResourceType.Prestige, 500);
                LogDebug($"Added resources to realm {realm.RealmName}");
            }
        }

        private void CreateTestVassal()
        {
            LogDebug("Create vassal not implemented - requires second realm");
            // TODO: Implement when we have easy realm creation
        }

        // ========== RELATIONSHIP TESTING ==========

        private void CreateFriendship()
        {
            CreateRelationship(RelationshipType.Friend, 50f);
        }

        private void CreateRivalry()
        {
            CreateRelationship(RelationshipType.Rival, -50f);
        }

        private void CreateRelationship(RelationshipType type, float strength)
        {
            if (GameUIController.Instance == null || RelationshipManager.Instance == null) return;

            int agent1ID = GameUIController.Instance.CurrentPlayerAgentID;
            int agent2ID = GetSelectedAgentID();

            if (agent1ID == agent2ID)
            {
                LogDebug("Cannot create relationship with self");
                return;
            }

            var relationship = RelationshipManager.Instance.FormRelationship(agent1ID, agent2ID, type, strength);
            if (relationship != null)
            {
                LogDebug($"Created {type} relationship between agents {agent1ID} and {agent2ID}");
            }
        }

        private void ModifyRelationship()
        {
            if (GameUIController.Instance == null || RelationshipManager.Instance == null) return;

            float strengthChange = 10f;
            if (relationshipStrengthInput != null && float.TryParse(relationshipStrengthInput.text, out float parsed))
            {
                strengthChange = parsed;
            }

            int agent1ID = GameUIController.Instance.CurrentPlayerAgentID;
            int agent2ID = GetSelectedAgentID();

            var relationships = RelationshipManager.Instance.GetRelationshipsBetween(agent1ID, agent2ID);
            if (relationships != null && relationships.Count > 0)
            {
                RelationshipManager.Instance.ModifyRelationshipStrength(relationships[0].RelationshipID, strengthChange);
                LogDebug($"Modified relationship strength by {strengthChange}");
            }
            else
            {
                LogDebug("No relationship found to modify");
            }
        }

        private int GetSelectedAgentID()
        {
            if (targetAgentDropdown == null || AgentManager.Instance == null) return -1;

            int index = targetAgentDropdown.value;
            var agents = AgentManager.Instance.GetAllAgents();

            if (index >= 0 && index < agents.Count)
            {
                return agents[index].Data.AgentID;
            }

            return -1;
        }

        // ========== CONTRACT TESTING ==========

        private void CreateGovernorContract()
        {
            LogDebug("Governor contract creation not implemented - requires city selection");
            // TODO: Implement when city UI exists
        }

        private void TerminateContract()
        {
            LogDebug("Contract termination not implemented - requires contract selection");
            // TODO: Implement contract selection dropdown
        }

        private void ModifyContractLoyalty()
        {
            LogDebug("Loyalty modification not implemented - requires contract selection");
            // TODO: Implement contract selection dropdown
        }

        // ========== OFFICE TESTING ==========

        private void AssignToOffice()
        {
            if (GameUIController.Instance == null || GovernmentManager.Instance == null) return;

            int realmID = GameUIController.Instance.CurrentPlayerRealmID;
            int agentID = GameUIController.Instance.CurrentPlayerAgentID;
            int officeIndex = officeDropdown?.value ?? -1;

            if (officeIndex < 0)
            {
                LogDebug("No office selected");
                return;
            }

            var offices = GovernmentManager.Instance.GetRealmOffices(realmID);
            if (offices != null && officeIndex < offices.Count)
            {
                int officeID = offices[officeIndex].OfficeID;
                GovernmentManager.Instance.AssignOffice(realmID, officeID, agentID);
                LogDebug($"Assigned agent {agentID} to office {offices[officeIndex].OfficeName}");
            }
        }

        private void RemoveFromOffice()
        {
            if (GameUIController.Instance == null || GovernmentManager.Instance == null) return;

            int realmID = GameUIController.Instance.CurrentPlayerRealmID;
            int agentID = GameUIController.Instance.CurrentPlayerAgentID;

            // Find office this agent holds
            var offices = GovernmentManager.Instance.GetRealmOffices(realmID);
            if (offices != null)
            {
                foreach (var office in offices)
                {
                    if (office.AssignedAgentID == agentID)
                    {
                        GovernmentManager.Instance.RemoveOfficeHolder(realmID, office.OfficeID);
                        LogDebug($"Removed agent from {office.OfficeName}");
                        return;
                    }
                }
            }

            LogDebug("Agent holds no office");
        }

        // ========== TIME TESTING ==========

        private void AdvanceDay()
        {
            WorldTimeManager.Instance?.AdvanceDay();
            LogDebug($"Advanced to day {WorldTimeManager.Instance?.CurrentDay}");
        }

        private void AdvanceSeason()
        {
            for (int i = 0; i < 30; i++)
            {
                WorldTimeManager.Instance?.AdvanceDay();
            }
            LogDebug($"Advanced to season {WorldTimeManager.Instance?.CurrentSeason}");
        }

        private void AdvanceYear()
        {
            for (int i = 0; i < 120; i++)
            {
                WorldTimeManager.Instance?.AdvanceDay();
            }
            LogDebug($"Advanced to year {WorldTimeManager.Instance?.CurrentYear}");
        }

        private void SetTimeSpeed(float speed)
        {
            WorldTimeManager.Instance?.SetTimeScale(speed);
            WorldTimeManager.Instance?.SetPaused(false);
            LogDebug($"Set time speed to {speed}x");
        }

        // ========== LOGGING ==========

        private void LogPlayerInfo()
        {
            if (GameUIController.Instance == null) return;

            var agent = GameUIController.Instance.GetPlayerAgent();
            var realm = GameUIController.Instance.GetPlayerRealm();

            string output = "===== PLAYER INFO =====\n";

            if (agent != null)
            {
                output += $"Agent: {agent.Data.AgentName} (ID: {agent.Data.AgentID})\n";
                output += $"Age: {agent.Data.Age}\n";
                output += $"Culture: {agent.Data.CultureID}\n";
                output += $"Religion: {agent.Data.ReligionID}\n";
                output += $"Alive: {agent.Data.IsAlive}\n";
            }

            if (realm != null)
            {
                output += $"\nRealm: {realm.RealmName} (ID: {realm.RealmID})\n";
                output += $"Cities: {realm.Data.DirectlyOwnedCityIDs.Count}\n";
                output += $"Vassals: {realm.Data.VassalContractIDs.Count}\n";
                output += $"Gold: {realm.Treasury.GetResource(Economy.ResourceType.Gold)}\n";
            }

            LogDebug(output);
        }

        private void LogAllAgents()
        {
            if (AgentManager.Instance == null) return;

            var agents = AgentManager.Instance.GetAllAgents();
            string output = $"===== ALL AGENTS ({agents.Count}) =====\n";

            foreach (var agent in agents)
            {
                output += $"- {agent.Data.AgentName} (ID: {agent.Data.AgentID}, Age: {agent.Data.Age}, Alive: {agent.Data.IsAlive})\n";
            }

            LogDebug(output);
        }

        private void LogAllRealms()
        {
            if (RealmManager.Instance == null) return;

            var realms = RealmManager.Instance.GetAllRealms();
            string output = $"===== ALL REALMS ({realms.Count}) =====\n";

            foreach (var realm in realms)
            {
                output += $"- {realm.RealmName} (ID: {realm.RealmID}, Cities: {realm.Data.DirectlyOwnedCityIDs.Count})\n";
            }

            LogDebug(output);
        }

        private void LogAllContracts()
        {
            if (ContractManager.Instance == null) return;

            var contracts = ContractManager.Instance.GetAllContracts();
            string output = $"===== ALL CONTRACTS ({contracts.Count}) =====\n";

            foreach (var contract in contracts)
            {
                output += $"- Contract {contract.ContractID}: {contract.Type} (Loyalty: {contract.GetLoyaltyLabel()})\n";
            }

            LogDebug(output);
        }

        private void LogAllRelationships()
        {
            if (RelationshipManager.Instance == null) return;

            var relationships = RelationshipManager.Instance.GetAllRelationships();
            string output = $"===== ALL RELATIONSHIPS ({relationships.Count}) =====\n";

            foreach (var rel in relationships)
            {
                output += $"- {rel.Type} between agents {rel.Agent1ID} and {rel.Agent2ID} (Strength: {rel.Strength:F0})\n";
            }

            LogDebug(output);
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[DebugMenu] {message}");

            if (debugOutputText != null)
            {
                debugOutputText.text = message;
            }
        }
    }
}
