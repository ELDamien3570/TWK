using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Realms;
using TWK.Government;
using TWK.Economy;
using TWK.Cultures;
using TWK.Agents;
using TWK.Realms.Demographics;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// Comprehensive ViewModel for realm management UI.
    /// Aggregates data from RealmData, Treasury, Government, Contracts, Demographics, etc.
    /// Organized into 6 tabs: Main, Lands, Economics, Government, Culture/Religion, Diplomacy.
    /// </summary>
    public class RealmViewModel
    {
        private int _realmID;
        private Realm _realm;
        private RealmData _realmData;

        public event Action OnPropertyChanged;

        // ========== TAB 1: MAIN INFO ==========

        // Basic Info
        public string RealmName { get; private set; }
        public int RealmID => _realmID;

        // Leaders
        public List<LeaderDisplay> Leaders { get; private set; } = new List<LeaderDisplay>();
        public string LeaderNamesDisplay { get; private set; }
        public int LeaderCount { get; private set; }

        // Government
        public string GovernmentName { get; private set; }
        public string GovernmentTypeDisplay { get; private set; }

        // Stability & Prestige
        public float Stability { get; private set; }
        public string StabilityDisplay { get; private set; }
        public Color StabilityColor { get; private set; }
        public float Prestige { get; private set; }
        public string PrestigeDisplay { get; private set; }

        // Income Summary (by ResourceType)
        public Dictionary<ResourceType, int> TotalIncome { get; private set; } = new Dictionary<ResourceType, int>();
        public Dictionary<ResourceType, int> TotalExpenses { get; private set; } = new Dictionary<ResourceType, int>();
        public Dictionary<ResourceType, int> NetIncome { get; private set; } = new Dictionary<ResourceType, int>();

        public string IncomeDisplayText { get; private set; }
        public string ExpenseDisplayText { get; private set; }
        public string NetIncomeDisplayText { get; private set; }

        // Treasury
        public int TreasuryGold { get; private set; }
        public string TreasuryGoldDisplay { get; private set; }

        // ========== TAB 2: REALM LANDS ==========

        // Holdings Summary
        public int DirectCitiesCount { get; private set; }
        public int VassalCount { get; private set; }
        public int TotalHoldings { get; private set; }
        public string HoldingsSummary { get; private set; }

        // Holdings List
        public List<HoldingDisplay> Holdings { get; private set; } = new List<HoldingDisplay>();

        // ========== TAB 3: ECONOMICS ==========

        // Top Earners
        public List<CityEconomicDisplay> TopEarningCities { get; private set; } = new List<CityEconomicDisplay>();
        public List<BuildingEconomicDisplay> TopEarningBuildings { get; private set; } = new List<BuildingEconomicDisplay>();

        // Economic Breakdown
        public int TotalCityIncome { get; private set; }
        public int TotalVassalTribute { get; private set; }
        public int TotalOfficeCosts { get; private set; }
        public int TotalEdictCosts { get; private set; }
        public string EconomicBreakdown { get; private set; }

        // Resource Production
        public Dictionary<ResourceType, int> TotalProduction { get; private set; } = new Dictionary<ResourceType, int>();
        public Dictionary<ResourceType, int> TotalConsumption { get; private set; } = new Dictionary<ResourceType, int>();

        // ========== TAB 4: GOVERNMENT OVERVIEW ==========

        // Stability Sources
        public float LegitimacyStability { get; private set; }
        public float CapacityStability { get; private set; }
        public float PopulationLoyaltyStability { get; private set; }
        public float VassalLoyaltyStability { get; private set; }
        public string StabilityBreakdown { get; private set; }

        // Officers
        public List<OfficerDisplay> Officers { get; private set; } = new List<OfficerDisplay>();
        public int FilledOffices { get; private set; }
        public int TotalOffices { get; private set; }

        // Population Groups (Most Stable/Unstable)
        public List<PopulationGroupDisplay> StablePopulations { get; private set; } = new List<PopulationGroupDisplay>();
        public List<PopulationGroupDisplay> UnstablePopulations { get; private set; } = new List<PopulationGroupDisplay>();

        // Vassals
        public List<VassalLoyaltyDisplay> LoyalVassals { get; private set; } = new List<VassalLoyaltyDisplay>();
        public List<VassalLoyaltyDisplay> DisloyalVassals { get; private set; } = new List<VassalLoyaltyDisplay>();

        // ========== TAB 5: CULTURE/RELIGION OVERVIEW ==========

        // Culture Breakdown
        public List<CultureDemographicDisplay> CulturesByCity { get; private set; } = new List<CultureDemographicDisplay>();
        public List<CulturePopulationDisplay> CulturesByPopulation { get; private set; } = new List<CulturePopulationDisplay>();
        public List<CultureClassDisplay> CulturesByClass { get; private set; } = new List<CultureClassDisplay>();
        public string DominantCulture { get; private set; }
        public float CulturalUnity { get; private set; }

        // Religion Breakdown
        public List<ReligionDemographicDisplay> ReligionsByCity { get; private set; } = new List<ReligionDemographicDisplay>();
        public List<ReligionPopulationDisplay> ReligionsByPopulation { get; private set; } = new List<ReligionPopulationDisplay>();
        public List<ReligionClassDisplay> ReligionsByClass { get; private set; } = new List<ReligionClassDisplay>();
        public string DominantReligion { get; private set; }
        public float ReligiousUnity { get; private set; }

        // ========== TAB 6: DIPLOMACY (PLACEHOLDER) ==========
        // Reserved for future implementation

        // ========== CONSTRUCTOR & REFRESH ==========

        public RealmViewModel(int realmID)
        {
            _realmID = realmID;
            Refresh();
        }

        public void Refresh()
        {
            if (RealmManager.Instance == null)
            {
                Debug.LogWarning("[RealmViewModel] RealmManager not found");
                return;
            }

            _realm = RealmManager.Instance.GetRealm(_realmID);
            if (_realm == null)
            {
                //Debug.LogWarning($"[RealmViewModel] Realm {_realmID} not found");
                return;
            }

            _realmData = _realm.Data;

            // Refresh all tabs
            RefreshMainInfo();
            RefreshLands();
            RefreshEconomics();
            RefreshGovernment();
            RefreshCultureReligion();

            NotifyPropertyChanged();
        }

        // ========== TAB 1: MAIN INFO REFRESH ==========

        private void RefreshMainInfo()
        {
            // Basic info
            RealmName = _realmData.RealmName;

            // Leaders
            RefreshLeaders();

            // Government
            if (GovernmentManager.Instance != null)
            {
                var government = GovernmentManager.Instance.GetRealmGovernment(_realmID);
                if (government != null)
                {
                    GovernmentName = government.GovernmentName;
                    GovernmentTypeDisplay = $"{government.RegimeForm} {government.StateStructure}";
                }
            }

            // Stability (inverse of revolt risk)
            if (GovernmentManager.Instance != null)
            {
                float revoltRisk = GovernmentManager.Instance.CalculateRevoltRisk(_realmID);
                Stability = 100f - revoltRisk;
                StabilityDisplay = $"{Stability:F0}%";
                StabilityColor = GetStabilityColor(Stability);
            }

            // Prestige (placeholder - implement when prestige system exists)
            Prestige = 50f;
            PrestigeDisplay = $"{Prestige:F0}";

            // Income/Expense Summary
            RefreshIncomeExpenseSummary();

            // Treasury
            if (_realm.Treasury != null)
            {
                TreasuryGold = _realm.Treasury.GetResource(ResourceType.Gold);
                TreasuryGoldDisplay = $"{TreasuryGold:N0} gold";
            }
        }

        private void RefreshLeaders()
        {
            Leaders.Clear();

            if (_realmData.LeaderIDs == null || AgentManager.Instance == null)
            {
                LeaderCount = 0;
                LeaderNamesDisplay = "No leaders";
                return;
            }

            foreach (int agentID in _realmData.LeaderIDs)
            {
                var agent = AgentManager.Instance.GetAgent(agentID);
                if (agent != null)
                {
                    Leaders.Add(new LeaderDisplay
                    {
                        AgentID = agentID,
                        Name = agent.Data.AgentName,
                        Age = agent.Data.Age,
                        // Icon would be set from agent portrait system
                    });
                }
            }

            LeaderCount = Leaders.Count;
            LeaderNamesDisplay = Leaders.Count > 0
                ? string.Join(", ", Leaders.Select(l => l.Name))
                : "No leaders";
        }

        private void RefreshIncomeExpenseSummary()
        {
            TotalIncome.Clear();
            TotalExpenses.Clear();
            NetIncome.Clear();

            // Income: City taxes
            if (_realmData.DirectlyOwnedCityIDs != null)
            {
                foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
                {
                    // Get city production (would need city economic snapshot)
                    int cityGold = ResourceManager.Instance?.GetResource(cityID, ResourceType.Gold) ?? 0;
                    int taxRate = GetTaxRate();
                    int tax = Mathf.FloorToInt(cityGold * (taxRate / 100f));

                    AddToResourceDict(TotalIncome, ResourceType.Gold, tax);
                }
            }

            // Income: Vassal tribute
            if (_realmData.VassalContractIDs != null && ContractManager.Instance != null)
            {
                foreach (int contractID in _realmData.VassalContractIDs)
                {
                    var contract = ContractManager.Instance.GetContract(contractID);
                    if (contract != null)
                    {
                        var vassal = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
                        if (vassal != null && vassal.Treasury != null)
                        {
                            int vassalGold = vassal.Treasury.GetResource(ResourceType.Gold);
                            int tribute = Mathf.FloorToInt(vassalGold * contract.GoldPercentage / 100f);
                            AddToResourceDict(TotalIncome, ResourceType.Gold, tribute);
                        }
                    }
                }
            }

            // Expenses: Office salaries
            if (GovernmentManager.Instance != null)
            {
                var offices = GovernmentManager.Instance.GetRealmOffices(_realmID);
                if (offices != null)
                {
                    foreach (var office in offices)
                    {
                        if (office.AssignedAgentID != -1)
                        {
                            AddToResourceDict(TotalExpenses, ResourceType.Gold, office.MonthlySalary);
                        }
                    }
                }
            }

            // Expenses: Edict maintenance
            if (GovernmentManager.Instance != null)
            {
                var edicts = GovernmentManager.Instance.GetActiveEdicts(_realmID);
                if (edicts != null)
                {
                    foreach (var edict in edicts)
                    {
                        AddToResourceDict(TotalExpenses, ResourceType.Gold, edict.MonthlyMaintenance);
                    }
                }
            }

            // Calculate net income
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                int income = TotalIncome.ContainsKey(resourceType) ? TotalIncome[resourceType] : 0;
                int expense = TotalExpenses.ContainsKey(resourceType) ? TotalExpenses[resourceType] : 0;
                NetIncome[resourceType] = income - expense;
            }

            // Build display strings
            IncomeDisplayText = BuildResourceDisplayString(TotalIncome, "+");
            ExpenseDisplayText = BuildResourceDisplayString(TotalExpenses, "-");
            NetIncomeDisplayText = BuildResourceDisplayString(NetIncome, "");
        }

        private int GetTaxRate()
        {
            if (GovernmentManager.Instance == null)
                return 10;

            var government = GovernmentManager.Instance.GetRealmGovernment(_realmID);
            if (government == null)
                return 10;

            switch (government.TaxationLaw)
            {
                case TaxationLaw.Tribute:
                    return 90;
                default:
                    return 100;
            }
        }

        // ========== TAB 2: LANDS REFRESH ==========

        private void RefreshLands()
        {
            DirectCitiesCount = _realmData.DirectlyOwnedCityIDs?.Count ?? 0;
            VassalCount = _realmData.VassalContractIDs?.Count ?? 0;
            TotalHoldings = DirectCitiesCount + VassalCount;

            HoldingsSummary = $"{DirectCitiesCount} directly owned, {VassalCount} contracted ({TotalHoldings} total)";

            // Build holdings list
            Holdings.Clear();

            // Add direct cities
            if (_realmData.DirectlyOwnedCityIDs != null)
            {
                foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
                {
                    var city = FindCity(cityID);
                    if (city != null)
                    {
                        Holdings.Add(new HoldingDisplay
                        {
                            Name = city.Name,
                            Type = "Direct City",
                            Population = GetCityPopulation(cityID),
                            Income = GetCityIncome(cityID),
                            IsDirectlyOwned = true
                        });
                    }
                }
            }

            // Add vassals
            if (_realmData.VassalContractIDs != null && ContractManager.Instance != null)
            {
                foreach (int contractID in _realmData.VassalContractIDs)
                {
                    var contract = ContractManager.Instance.GetContract(contractID);
                    if (contract != null)
                    {
                        var vassal = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
                        if (vassal != null)
                        {
                            Holdings.Add(new HoldingDisplay
                            {
                                Name = vassal.Data.RealmName,
                                Type = contract.Type.ToString(),
                                Population = vassal.Data.TotalPopulation,
                                Income = GetVassalTribute(contract),
                                IsDirectlyOwned = false,
                                TributeRate = contract.GoldPercentage
                            });
                        }
                    }
                }
            }
        }

        // ========== TAB 3: ECONOMICS REFRESH ==========

        private void RefreshEconomics()
        {
            TopEarningCities.Clear();
            TopEarningBuildings.Clear();
            TotalProduction.Clear();
            TotalConsumption.Clear();

            // Calculate city incomes
            if (_realmData.DirectlyOwnedCityIDs != null)
            {
                var cityEconomics = new List<CityEconomicDisplay>();

                foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
                {
                    var city = FindCity(cityID);
                    if (city != null)
                    {
                        int income = GetCityIncome(cityID);
                        int production = GetCityProduction(cityID, ResourceType.Gold);

                        cityEconomics.Add(new CityEconomicDisplay
                        {
                            CityName = city.Name,
                            TaxIncome = income,
                            GoldProduction = production,
                            Population = GetCityPopulation(cityID)
                        });

                        // Aggregate production and consumption
                        if (city.Data?.EconomySnapshot != null)
                        {
                            foreach (var kvp in city.Data.EconomySnapshot.Production)
                            {
                                AddToResourceDict(TotalProduction, kvp.Key, kvp.Value);
                            }

                            foreach (var kvp in city.Data.EconomySnapshot.Consumption)
                            {
                                AddToResourceDict(TotalConsumption, kvp.Key, kvp.Value);
                            }
                        }
                    }
                }

                // Sort by tax income
                TopEarningCities = cityEconomics.OrderByDescending(c => c.TaxIncome).ToList();
            }

            // Economic breakdown
            TotalCityIncome = TotalIncome.ContainsKey(ResourceType.Gold) ? TotalIncome[ResourceType.Gold] : 0;
            TotalVassalTribute = CalculateTotalVassalTribute();
            TotalOfficeCosts = CalculateTotalOfficeCosts();
            TotalEdictCosts = CalculateTotalEdictCosts();

            EconomicBreakdown = $"City Income: {TotalCityIncome:N0}g\n" +
                              $"Vassal Tribute: {TotalVassalTribute:N0}g\n" +
                              $"Office Salaries: -{TotalOfficeCosts:N0}g\n" +
                              $"Edict Costs: -{TotalEdictCosts:N0}g";
        }

        // ========== TAB 4: GOVERNMENT REFRESH ==========

        private void RefreshGovernment()
        {
            if (GovernmentManager.Instance == null)
                return;

            // Stability sources
            float legitimacy = GovernmentManager.Instance.GetRealmLegitimacy(_realmID);
            float capacity = GovernmentManager.Instance.GetRealmCapacity(_realmID);

            LegitimacyStability = legitimacy;
            CapacityStability = capacity;
            PopulationLoyaltyStability = CalculateAveragePopulationLoyalty();
            VassalLoyaltyStability = CalculateAverageVassalLoyalty();

            StabilityBreakdown = $"Legitimacy: {LegitimacyStability:F0}%\n" +
                               $"Capacity: {CapacityStability:F0}%\n" +
                               $"Population Loyalty: {PopulationLoyaltyStability:F0}%\n" +
                               $"Vassal Loyalty: {VassalLoyaltyStability:F0}%";

            // Officers
            RefreshOfficers();

            // Population groups
            RefreshPopulationGroups();

            // Vassals
            RefreshVassalLoyalty();
        }

        private void RefreshOfficers()
        {
            Officers.Clear();
            FilledOffices = 0;
            TotalOffices = 0;

            if (GovernmentManager.Instance == null)
            {
                Debug.LogWarning($"[RealmViewModel] GovernmentManager.Instance is null for realm {_realmID}");
                return;
            }

            var offices = GovernmentManager.Instance.GetRealmOffices(_realmID);
            if (offices == null)
            {
                Debug.LogWarning($"[RealmViewModel] No offices found for realm {_realmID}");
                return;
            }

            TotalOffices = offices.Count;
            Debug.Log($"[RealmViewModel] Found {TotalOffices} offices for realm {_realmID}");

            foreach (var office in offices)
            {
                bool isFilled = office.AssignedAgentID != -1;
                if (isFilled) FilledOffices++;

                string holderName = "Vacant";
                if (isFilled && AgentManager.Instance != null)
                {
                    var agent = AgentManager.Instance.GetAgent(office.AssignedAgentID);
                    if (agent != null)
                        holderName = agent.Data.AgentName;
                    else
                        Debug.LogWarning($"[RealmViewModel] Agent {office.AssignedAgentID} not found in AgentManager for office {office.OfficeName}");
                }
                else if (isFilled && AgentManager.Instance == null)
                {
                    Debug.LogWarning($"[RealmViewModel] AgentManager.Instance is null, cannot get agent name for office {office.OfficeName}");
                }

                var officerDisplay = new OfficerDisplay
                {
                    OfficeName = office.OfficeName,
                    HolderName = holderName,
                    Salary = office.MonthlySalary,
                    IsFilled = isFilled
                };

                Officers.Add(officerDisplay);
                Debug.Log($"[RealmViewModel] Added officer: {office.OfficeName}, Holder: {holderName}, Filled: {isFilled}");
            }

            Debug.Log($"[RealmViewModel] Total officers in list: {Officers.Count}, Filled: {FilledOffices}/{TotalOffices}");
        }

        private void RefreshPopulationGroups()
        {
            StablePopulations.Clear();
            UnstablePopulations.Clear();

            // TODO: Get population data from PopulationManager when available
            // For now, placeholder
        }

        private void RefreshVassalLoyalty()
        {
            LoyalVassals.Clear();
            DisloyalVassals.Clear();

            if (_realmData.VassalContractIDs == null || ContractManager.Instance == null)
                return;

            foreach (int contractID in _realmData.VassalContractIDs)
            {
                var contract = ContractManager.Instance.GetContract(contractID);
                if (contract == null)
                    continue;

                var vassal = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
                if (vassal == null)
                    continue;

                var display = new VassalLoyaltyDisplay
                {
                    VassalName = vassal.Data.RealmName,
                    Loyalty = contract.CurrentLoyalty,
                    LoyaltyStatus = contract.GetLoyaltyStatus(),
                    TributeRate = contract.GoldPercentage
                };

                if (contract.IsLoyal())
                    LoyalVassals.Add(display);
                else if (contract.IsDisloyal())
                    DisloyalVassals.Add(display);
            }
        }

        // ========== TAB 5: CULTURE/RELIGION REFRESH ==========

        private void RefreshCultureReligion()
        {
            RefreshCultureData();
            RefreshReligionData();
        }

        private void RefreshCultureData()
        {
            CulturesByCity.Clear();
            CulturesByPopulation.Clear();
            CulturesByClass.Clear();

            if (_realmData.DirectlyOwnedCityIDs == null || _realmData.DirectlyOwnedCityIDs.Count == 0)
            {
                DominantCulture = "No Cities";
                CulturalUnity = 0f;
                Debug.LogWarning($"[RealmViewModel] Realm {_realmID} has no cities for culture data");
                return;
            }

            Debug.Log($"[RealmViewModel] Refreshing culture data for realm {_realmID} with {_realmData.DirectlyOwnedCityIDs.Count} cities");

            // Aggregate culture data from all cities
            Dictionary<CultureData, int> cultureTotalCounts = new Dictionary<CultureData, int>();
            Dictionary<CultureData, Dictionary<string, int>> cultureByClass = new Dictionary<CultureData, Dictionary<string, int>>();
            int totalRealmPop = 0;

            foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
            {
                var city = FindCity(cityID);
                if (city == null)
                {
                    Debug.LogWarning($"[RealmViewModel] City {cityID} not found");
                    continue;
                }

                var breakdown = city.GetCultureBreakdown();
                Debug.Log($"[RealmViewModel] City {city.Name} ({cityID}): Breakdown count = {breakdown?.Count ?? 0}");

                if (breakdown != null)
                {
                    foreach (var kvp in breakdown)
                    {
                        var culture = kvp.Key;
                        var (count, percentage) = kvp.Value;

                        // Add to "By City" list
                        CulturesByCity.Add(new CultureDemographicDisplay
                        {
                            CityName = city.Name,
                            CultureName = culture.CultureName,
                            Population = count,
                            Percentage = percentage
                        });

                        // Aggregate for "By Population" (total counts)
                        if (!cultureTotalCounts.ContainsKey(culture))
                            cultureTotalCounts[culture] = 0;
                        cultureTotalCounts[culture] += count;
                        totalRealmPop += count;

                        // Get class breakdown for this culture in this city
                        if (PopulationManager.Instance != null)
                        {
                            var populations = PopulationManager.Instance.GetPopulationsByCity(cityID);
                            foreach (var pop in populations)
                            {
                                if (pop.Culture == culture)
                                {
                                    string className = pop.Archetype.ToString();

                                    if (!cultureByClass.ContainsKey(culture))
                                        cultureByClass[culture] = new Dictionary<string, int>();

                                    if (!cultureByClass[culture].ContainsKey(className))
                                        cultureByClass[culture][className] = 0;

                                    cultureByClass[culture][className] += pop.PopulationCount;
                                }
                            }
                        }

                        Debug.Log($"[RealmViewModel]   - Culture: {culture.CultureName}, Count: {count}, Percentage: {percentage}%");
                    }
                }
            }

            // Populate "By Population" list (sorted by total count)
            foreach (var kvp in cultureTotalCounts.OrderByDescending(x => x.Value))
            {
                float percentage = totalRealmPop > 0 ? (kvp.Value / (float)totalRealmPop) * 100f : 0f;
                CulturesByPopulation.Add(new CulturePopulationDisplay
                {
                    CultureName = kvp.Key.CultureName,
                    TotalPopulation = kvp.Value,
                    Percentage = percentage
                });
            }

            // Populate "By Class" list
            foreach (var cultureKvp in cultureByClass)
            {
                var culture = cultureKvp.Key;
                int cultureTotalPop = cultureTotalCounts[culture];

                foreach (var classKvp in cultureKvp.Value.OrderByDescending(x => x.Value))
                {
                    float percentage = cultureTotalPop > 0 ? (classKvp.Value / (float)cultureTotalPop) * 100f : 0f;
                    CulturesByClass.Add(new CultureClassDisplay
                    {
                        CultureName = culture.CultureName,
                        ClassName = classKvp.Key,
                        Population = classKvp.Value,
                        Percentage = percentage
                    });
                }
            }

            // Find dominant culture
            if (cultureTotalCounts.Count > 0)
            {
                var dominant = cultureTotalCounts.OrderByDescending(kvp => kvp.Value).First();
                DominantCulture = dominant.Key.CultureName;
                CulturalUnity = totalRealmPop > 0 ? (dominant.Value / (float)totalRealmPop) * 100f : 0f;
                Debug.Log($"[RealmViewModel] Dominant culture: {DominantCulture}, Unity: {CulturalUnity}%, Total Pop: {totalRealmPop}");
                Debug.Log($"[RealmViewModel] Populated lists - By City: {CulturesByCity.Count}, By Population: {CulturesByPopulation.Count}, By Class: {CulturesByClass.Count}");
            }
            else
            {
                DominantCulture = "No Population";
                CulturalUnity = 0f;
                Debug.LogWarning($"[RealmViewModel] No culture data found");
            }
        }

        private void RefreshReligionData()
        {
            ReligionsByCity.Clear();
            ReligionsByPopulation.Clear();
            ReligionsByClass.Clear();

            if (_realmData.DirectlyOwnedCityIDs == null || _realmData.DirectlyOwnedCityIDs.Count == 0)
            {
                DominantReligion = "No Cities";
                ReligiousUnity = 0f;
                return;
            }

            Debug.Log($"[RealmViewModel] Refreshing religion data for realm {_realmID} with {_realmData.DirectlyOwnedCityIDs.Count} cities");

            // Aggregate religion data from all cities
            Dictionary<TWK.Religion.ReligionData, int> religionTotalCounts = new Dictionary<TWK.Religion.ReligionData, int>();
            Dictionary<TWK.Religion.ReligionData, Dictionary<string, int>> religionByClass = new Dictionary<TWK.Religion.ReligionData, Dictionary<string, int>>();
            int totalRealmPop = 0;

            foreach (int cityID in _realmData.DirectlyOwnedCityIDs)
            {
                var city = FindCity(cityID);
                if (city == null)
                {
                    Debug.LogWarning($"[RealmViewModel] City {cityID} not found");
                    continue;
                }

                var breakdown = city.GetReligionBreakdown();
                Debug.Log($"[RealmViewModel] City {city.Name} ({cityID}): Religion breakdown count = {breakdown?.Count ?? 0}");

                if (breakdown != null)
                {
                    foreach (var kvp in breakdown)
                    {
                        var religion = kvp.Key;
                        var (count, percentage) = kvp.Value;

                        // Add to "By City" list
                        ReligionsByCity.Add(new ReligionDemographicDisplay
                        {
                            CityName = city.Name,
                            ReligionName = religion.ReligionName,
                            Population = count,
                            Percentage = percentage
                        });

                        // Aggregate for "By Population" (total counts)
                        if (!religionTotalCounts.ContainsKey(religion))
                            religionTotalCounts[religion] = 0;
                        religionTotalCounts[religion] += count;
                        totalRealmPop += count;

                        // Get class breakdown for this religion in this city
                        if (PopulationManager.Instance != null)
                        {
                            var populations = PopulationManager.Instance.GetPopulationsByCity(cityID);
                            foreach (var pop in populations)
                            {
                                if (pop.CurrentReligion == religion)
                                {
                                    string className = pop.Archetype.ToString();

                                    if (!religionByClass.ContainsKey(religion))
                                        religionByClass[religion] = new Dictionary<string, int>();

                                    if (!religionByClass[religion].ContainsKey(className))
                                        religionByClass[religion][className] = 0;

                                    religionByClass[religion][className] += pop.PopulationCount;
                                }
                            }
                        }

                        Debug.Log($"[RealmViewModel]   - Religion: {religion.ReligionName}, Count: {count}, Percentage: {percentage}%");
                    }
                }
            }

            // Populate "By Population" list (sorted by total count)
            foreach (var kvp in religionTotalCounts.OrderByDescending(x => x.Value))
            {
                float percentage = totalRealmPop > 0 ? (kvp.Value / (float)totalRealmPop) * 100f : 0f;
                ReligionsByPopulation.Add(new ReligionPopulationDisplay
                {
                    ReligionName = kvp.Key.ReligionName,
                    TotalPopulation = kvp.Value,
                    Percentage = percentage
                });
            }

            // Populate "By Class" list
            foreach (var religionKvp in religionByClass)
            {
                var religion = religionKvp.Key;
                int religionTotalPop = religionTotalCounts[religion];

                foreach (var classKvp in religionKvp.Value.OrderByDescending(x => x.Value))
                {
                    float percentage = religionTotalPop > 0 ? (classKvp.Value / (float)religionTotalPop) * 100f : 0f;
                    ReligionsByClass.Add(new ReligionClassDisplay
                    {
                        ReligionName = religion.ReligionName,
                        ClassName = classKvp.Key,
                        Population = classKvp.Value,
                        Percentage = percentage
                    });
                }
            }

            // Find dominant religion
            if (religionTotalCounts.Count > 0)
            {
                var dominant = religionTotalCounts.OrderByDescending(kvp => kvp.Value).First();
                DominantReligion = dominant.Key.ReligionName;
                ReligiousUnity = totalRealmPop > 0 ? (dominant.Value / (float)totalRealmPop) * 100f : 0f;
                Debug.Log($"[RealmViewModel] Dominant religion: {DominantReligion}, Unity: {ReligiousUnity}%, Total Pop: {totalRealmPop}");
                Debug.Log($"[RealmViewModel] Populated lists - By City: {ReligionsByCity.Count}, By Population: {ReligionsByPopulation.Count}, By Class: {ReligionsByClass.Count}");
            }
            else
            {
                DominantReligion = "No Population";
                ReligiousUnity = 0f;
                Debug.LogWarning($"[RealmViewModel] No religion data found");
            }
        }

        // ========== HELPER METHODS ==========

        private City FindCity(int cityID)
        {
            City[] cities = GameObject.FindObjectsByType<City>(FindObjectsSortMode.None);
            foreach (City city in cities)
            {
                if (city.CityID == cityID)
                    return city;
            }
            return null;
        }

        private int GetCityPopulation(int cityID)
        {
            // TODO: Get from PopulationManager when available
            return 1000; // Placeholder
        }

        private int GetCityIncome(int cityID)
        {
            if (ResourceManager.Instance == null)
                return 0;

            int cityGold = ResourceManager.Instance.GetResource(cityID, ResourceType.Gold);
            int taxRate = GetTaxRate();
            return Mathf.FloorToInt(cityGold * taxRate / 100f);
        }

        private int GetCityProduction(int cityID, ResourceType resourceType)
        {
            // TODO: Get from city economic snapshot
            return 100; // Placeholder
        }

        private int GetVassalTribute(Contract contract)
        {
            var vassal = RealmManager.Instance?.GetRealm(contract.SubjectRealmID);
            if (vassal == null || vassal.Treasury == null)
                return 0;

            int vassalGold = vassal.Treasury.GetResource(ResourceType.Gold);
            return Mathf.FloorToInt(vassalGold * contract.GoldPercentage / 100f);
        }

        private float CalculateAveragePopulationLoyalty()
        {
            // TODO: Get from PopulationManager
            return 50f; // Placeholder
        }

        private float CalculateAverageVassalLoyalty()
        {
            if (_realmData.VassalContractIDs == null || _realmData.VassalContractIDs.Count == 0)
                return 0f;

            float total = 0f;
            int count = 0;

            foreach (int contractID in _realmData.VassalContractIDs)
            {
                var contract = ContractManager.Instance?.GetContract(contractID);
                if (contract != null)
                {
                    total += contract.CurrentLoyalty;
                    count++;
                }
            }

            return count > 0 ? total / count : 0f;
        }

        private int CalculateTotalVassalTribute()
        {
            int total = 0;
            if (_realmData.VassalContractIDs == null || ContractManager.Instance == null)
                return total;

            foreach (int contractID in _realmData.VassalContractIDs)
            {
                var contract = ContractManager.Instance.GetContract(contractID);
                if (contract != null)
                {
                    total += GetVassalTribute(contract);
                }
            }

            return total;
        }

        private int CalculateTotalOfficeCosts()
        {
            int total = 0;
            if (GovernmentManager.Instance == null)
                return total;

            var offices = GovernmentManager.Instance.GetRealmOffices(_realmID);
            if (offices == null)
                return total;

            foreach (var office in offices)
            {
                if (office.AssignedAgentID != -1)
                {
                    total += office.MonthlySalary;
                }
            }

            return total;
        }

        private int CalculateTotalEdictCosts()
        {
            int total = 0;
            if (GovernmentManager.Instance == null)
                return total;

            var edicts = GovernmentManager.Instance.GetActiveEdicts(_realmID);
            if (edicts == null)
                return total;

            foreach (var edict in edicts)
            {
                total += edict.MonthlyMaintenance;
            }

            return total;
        }

        private void AddToResourceDict(Dictionary<ResourceType, int> dict, ResourceType type, int amount)
        {
            if (!dict.ContainsKey(type))
                dict[type] = 0;
            dict[type] += amount;
        }

        private string BuildResourceDisplayString(Dictionary<ResourceType, int> resources, string prefix)
        {
            if (resources.Count == 0)
                return "None";

            var lines = new List<string>();
            foreach (var kvp in resources)
            {
                if (kvp.Value != 0)
                {
                    lines.Add($"{prefix}{kvp.Value:N0} {kvp.Key}");
                }
            }

            return lines.Count > 0 ? string.Join("\n", lines) : "None";
        }

        private Color GetStabilityColor(float stability)
        {
            if (stability >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (stability >= 60f) return new Color(0.8f, 0.8f, 0.2f); // Yellow
            if (stability >= 40f) return new Color(0.9f, 0.6f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        private void NotifyPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }
    }

    // ========== DISPLAY CLASSES ==========

    public class LeaderDisplay
    {
        public int AgentID;
        public string Name;
        public int Age;
        public Sprite Icon;
    }

    public class HoldingDisplay
    {
        public string Name;
        public string Type;
        public int Population;
        public int Income;
        public bool IsDirectlyOwned;
        public float TributeRate;
    }

    public class CityEconomicDisplay
    {
        public string CityName;
        public int TaxIncome;
        public int GoldProduction;
        public int Population;
    }

    public class BuildingEconomicDisplay
    {
        public string BuildingName;
        public string CityName;
        public int Income;
        public ResourceType ProducesResource;
    }

    public class OfficerDisplay
    {
        public string OfficeName;
        public string HolderName;
        public int Salary;
        public bool IsFilled;
    }

    public class PopulationGroupDisplay
    {
        public string GroupName;
        public int Population;
        public float Loyalty;
        public string Archetype;
    }

    public class VassalLoyaltyDisplay
    {
        public string VassalName;
        public float Loyalty;
        public string LoyaltyStatus;
        public float TributeRate;
    }

    public class CultureDemographicDisplay
    {
        public string CityName;
        public string CultureName;
        public int Population;
        public float Percentage;
    }

    public class CulturePopulationDisplay
    {
        public string CultureName;
        public int TotalPopulation;
        public float Percentage;
    }

    public class CultureClassDisplay
    {
        public string CultureName;
        public string ClassName;
        public int Population;
        public float Percentage;
    }

    public class ReligionDemographicDisplay
    {
        public string CityName;
        public string ReligionName;
        public int Population;
        public float Percentage;
    }

    public class ReligionPopulationDisplay
    {
        public string ReligionName;
        public int TotalPopulation;
        public float Percentage;
    }

    public class ReligionClassDisplay
    {
        public string ReligionName;
        public string ClassName;
        public int Population;
        public float Percentage;
    }
}
