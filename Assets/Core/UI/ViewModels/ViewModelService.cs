using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Realms;
using TWK.Core;
using TWK.Simulation;
using TWK.Religion;
using TWK.Cultures;
using TWK.Government;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// Service for managing ViewModels.
    /// Creates, updates, and provides access to ViewModels for the UI layer.
    /// </summary>
    public class ViewModelService : MonoBehaviour
    {
        public static ViewModelService Instance { get; private set; }

        [SerializeField] private WorldTimeManager worldTimeManager;

        // ========== VIEWMODEL COLLECTIONS ==========
        private Dictionary<int, CityViewModel> cityViewModels = new Dictionary<int, CityViewModel>();
        private Dictionary<int, ReligionViewModel> religionViewModels = new Dictionary<int, ReligionViewModel>();
        private Dictionary<int, CultureViewModel> cultureViewModels = new Dictionary<int, CultureViewModel>();
        private Dictionary<int, GovernmentViewModel> governmentViewModels = new Dictionary<int, GovernmentViewModel>();
        private Dictionary<int, BureaucracyViewModel> bureaucracyViewModels = new Dictionary<int, BureaucracyViewModel>();
        private Dictionary<int, ContractViewModel> contractViewModels = new Dictionary<int, ContractViewModel>();

        // ========== EVENTS ==========
        /// <summary>
        /// Invoked when any ViewModel is updated.
        /// </summary>
        public event Action OnViewModelsUpdated;

        /// <summary>
        /// Invoked when a specific city ViewModel is updated.
        /// </summary>
        public event Action<int> OnCityViewModelUpdated;

        /// <summary>
        /// Invoked when a specific religion ViewModel is updated.
        /// </summary>
        public event Action<int> OnReligionViewModelUpdated;

        /// <summary>
        /// Invoked when a specific culture ViewModel is updated.
        /// </summary>
        public event Action<int> OnCultureViewModelUpdated;

        /// <summary>
        /// Invoked when a specific government ViewModel is updated.
        /// </summary>
        public event Action<int> OnGovernmentViewModelUpdated;

        /// <summary>
        /// Invoked when a specific bureaucracy ViewModel is updated.
        /// </summary>
        public event Action<int> OnBureaucracyViewModelUpdated;

        /// <summary>
        /// Invoked when a specific contract ViewModel is updated.
        /// </summary>
        public event Action<int> OnContractViewModelUpdated;

        // ========== LIFECYCLE ==========
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(WorldTimeManager timeManager)
        {
            worldTimeManager = timeManager;

            // Subscribe to time events for automatic updates
            worldTimeManager.OnDayTick += OnDayTick;
            worldTimeManager.OnSeasonTick += OnSeasonTick;
            worldTimeManager.OnYearTick += OnYearTick;

            // Subscribe to manager events
            SubscribeToManagerEvents();

            // Auto-register existing data
            RegisterExistingData();

            Debug.Log("[ViewModelService] Initialized and subscribed to time events");
        }

        /// <summary>
        /// Register all existing religions and cultures that are already in the managers.
        /// </summary>
        private void RegisterExistingData()
        {
            // Register existing religions
            if (ReligionManager.Instance != null)
            {
                foreach (var religion in ReligionManager.Instance.GetAllReligions())
                {
                    RegisterReligion(religion);
                }
                Debug.Log($"[ViewModelService] Auto-registered {religionViewModels.Count} religions");
            }

            // Register existing cultures
            if (CultureManager.Instance != null)
            {
                foreach (var culture in CultureManager.Instance.GetAllCultures())
                {
                    RegisterCulture(culture);
                }
                Debug.Log($"[ViewModelService] Auto-registered {cultureViewModels.Count} cultures");
            }
        }

        private void SubscribeToManagerEvents()
        {
            // Subscribe to ReligionManager events
            if (ReligionManager.Instance != null)
            {
                ReligionManager.Instance.newReligionRegistered += OnNewReligionRegistered;
                ReligionManager.Instance.OnPopulationConverted += OnPopulationConverted;
            }

            // Subscribe to CultureManager events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCityCultureChanged += OnCityCultureChanged;
                CultureManager.Instance.OnCultureBuildingsChanged += OnCultureBuildingsChanged;
                CultureManager.Instance.OnCultureXPAdded += OnCultureXPAdded;
            }

            // Subscribe to GovernmentManager events
            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnGovernmentChanged += OnGovernmentChanged;
                GovernmentManager.Instance.OnOfficeAssigned += OnOfficeAssigned;
                GovernmentManager.Instance.OnOfficeCreated += OnOfficeCreated;
                GovernmentManager.Instance.OnEdictEnacted += OnEdictEnacted;
                GovernmentManager.Instance.OnLegitimacyChanged += OnLegitimacyChanged;
                GovernmentManager.Instance.OnCapacityChanged += OnCapacityChanged;
            }

            // Subscribe to ContractManager events
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated += OnContractCreated;
                ContractManager.Instance.OnContractTerminated += OnContractTerminated;
                ContractManager.Instance.OnLoyaltyChanged += OnContractLoyaltyChanged;
            }
        }

        private void UnsubscribeFromManagerEvents()
        {
            // Unsubscribe from ReligionManager events
            if (ReligionManager.Instance != null)
            {
                ReligionManager.Instance.newReligionRegistered -= OnNewReligionRegistered;
                ReligionManager.Instance.OnPopulationConverted -= OnPopulationConverted;
            }

            // Unsubscribe from CultureManager events
            if (CultureManager.Instance != null)
            {
                CultureManager.Instance.OnCityCultureChanged -= OnCityCultureChanged;
                CultureManager.Instance.OnCultureBuildingsChanged -= OnCultureBuildingsChanged;
                CultureManager.Instance.OnCultureXPAdded -= OnCultureXPAdded;
            }

            // Unsubscribe from GovernmentManager events
            if (GovernmentManager.Instance != null)
            {
                GovernmentManager.Instance.OnGovernmentChanged -= OnGovernmentChanged;
                GovernmentManager.Instance.OnOfficeAssigned -= OnOfficeAssigned;
                GovernmentManager.Instance.OnOfficeCreated -= OnOfficeCreated;
                GovernmentManager.Instance.OnEdictEnacted -= OnEdictEnacted;
                GovernmentManager.Instance.OnLegitimacyChanged -= OnLegitimacyChanged;
                GovernmentManager.Instance.OnCapacityChanged -= OnCapacityChanged;
            }

            // Unsubscribe from ContractManager events
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractCreated -= OnContractCreated;
                ContractManager.Instance.OnContractTerminated -= OnContractTerminated;
                ContractManager.Instance.OnLoyaltyChanged -= OnContractLoyaltyChanged;
            }
        }

        private void OnDestroy()
        {
            if (worldTimeManager != null)
            {
                worldTimeManager.OnDayTick -= OnDayTick;
                worldTimeManager.OnSeasonTick -= OnSeasonTick;
                worldTimeManager.OnYearTick -= OnYearTick;
            }

            UnsubscribeFromManagerEvents();
        }

        // ========== TIME EVENT HANDLERS ==========
        private void OnDayTick()
        {
            // Update all ViewModels daily
            RefreshAllViewModels();
        }

        private void OnSeasonTick()
        {
            // Optional: perform season-specific updates
        }

        private void OnYearTick()
        {
            // Optional: perform year-specific updates
        }

        // ========== CITY VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register a city and create its ViewModel.
        /// </summary>
        public void RegisterCity(City city)
        {
            if (city == null)
            {
                Debug.LogError("[ViewModelService] Cannot register null city");
                return;
            }

            if (cityViewModels.ContainsKey(city.CityID))
            {
                //Debug.LogWarning($"[ViewModelService] City {city.Name} (ID: {city.CityID}) already registered");
                return;
            }

            var viewModel = new CityViewModel(city);
            cityViewModels[city.CityID] = viewModel;

           // Debug.Log($"[ViewModelService] Registered city: {city.Name} (ID: {city.CityID})");
        }

        /// <summary>
        /// Get a city ViewModel by ID.
        /// </summary>
        public CityViewModel GetCityViewModel(int cityId)
        {
            if (cityViewModels.TryGetValue(cityId, out var viewModel))
                return viewModel;

            Debug.LogWarning($"[ViewModelService] City ViewModel not found for ID: {cityId}");
            return null;
        }

        /// <summary>
        /// Get all city ViewModels.
        /// </summary>
        public IEnumerable<CityViewModel> GetAllCityViewModels()
        {
            return cityViewModels.Values;
        }

        /// <summary>
        /// Refresh a specific city ViewModel.
        /// </summary>
        public void RefreshCityViewModel(int cityId)
        {
            if (cityViewModels.TryGetValue(cityId, out var viewModel))
            {
                viewModel.Refresh();
                OnCityViewModelUpdated?.Invoke(cityId);
            }
        }

        /// <summary>
        /// Refresh all ViewModels.
        /// </summary>
        public void RefreshAllViewModels()
        {
            foreach (var viewModel in cityViewModels.Values)
            {
                viewModel.Refresh();
            }

            foreach (var viewModel in religionViewModels.Values)
            {
                viewModel.Refresh();
            }

            foreach (var viewModel in cultureViewModels.Values)
            {
                viewModel.Refresh();
            }

            foreach (var viewModel in governmentViewModels.Values)
            {
                viewModel.Refresh();
            }

            foreach (var viewModel in bureaucracyViewModels.Values)
            {
                viewModel.Refresh();
            }

            foreach (var viewModel in contractViewModels.Values)
            {
                viewModel.Refresh();
            }

            OnViewModelsUpdated?.Invoke();
        }

        /// <summary>
        /// Manually trigger a refresh (useful for debugging).
        /// </summary>
        [ContextMenu("Refresh All ViewModels")]
        public void ManualRefresh()
        {
            RefreshAllViewModels();
            Debug.Log("[ViewModelService] Manual refresh completed");
        }

        // ========== RELIGION VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register a religion and create its ViewModel.
        /// </summary>
        public void RegisterReligion(ReligionData religion)
        {
            if (religion == null)
            {
                Debug.LogError("[ViewModelService] Cannot register null religion");
                return;
            }

            int religionID = religion.GetStableReligionID();

            if (religionViewModels.ContainsKey(religionID))
            {
                //Debug.LogWarning($"[ViewModelService] Religion {religion.ReligionName} (ID: {religionID}) already registered");
                return;
            }

            var viewModel = new ReligionViewModel(religion);
            religionViewModels[religionID] = viewModel;

            //Debug.Log($"[ViewModelService] Registered religion: {religion.ReligionName} (ID: {religionID})");
        }

        /// <summary>
        /// Get a religion ViewModel by ID.
        /// </summary>
        public ReligionViewModel GetReligionViewModel(int religionID)
        {
            if (religionViewModels.TryGetValue(religionID, out var viewModel))
                return viewModel;

            Debug.LogWarning($"[ViewModelService] Religion ViewModel not found for ID: {religionID}");
            return null;
        }

        /// <summary>
        /// Get all religion ViewModels.
        /// </summary>
        public IEnumerable<ReligionViewModel> GetAllReligionViewModels()
        {
            return religionViewModels.Values;
        }

        /// <summary>
        /// Refresh a specific religion ViewModel.
        /// </summary>
        public void RefreshReligionViewModel(int religionID)
        {
            if (religionViewModels.TryGetValue(religionID, out var viewModel))
            {
                viewModel.Refresh();
                OnReligionViewModelUpdated?.Invoke(religionID);
            }
        }

        /// <summary>
        /// Refresh all religion ViewModels.
        /// </summary>
        public void RefreshAllReligionViewModels()
        {
            foreach (var viewModel in religionViewModels.Values)
            {
                viewModel.Refresh();
            }
        }

        // ========== CULTURE VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register a culture and create its ViewModel.
        /// </summary>
        public void RegisterCulture(CultureData culture)
        {
            if (culture == null)
            {
                Debug.LogError("[ViewModelService] Cannot register null culture");
                return;
            }

            int cultureID = culture.GetCultureID();

            if (cultureViewModels.ContainsKey(cultureID))
            {
                //Debug.LogWarning($"[ViewModelService] Culture {culture.CultureName} (ID: {cultureID}) already registered");
                return;
            }

            var viewModel = new CultureViewModel(culture);
            cultureViewModels[cultureID] = viewModel;

            //Debug.Log($"[ViewModelService] Registered culture: {culture.CultureName} (ID: {cultureID})");
        }

        /// <summary>
        /// Get a culture ViewModel by ID.
        /// </summary>
        public CultureViewModel GetCultureViewModel(int cultureID)
        {
            if (cultureViewModels.TryGetValue(cultureID, out var viewModel))
                return viewModel;

            Debug.LogWarning($"[ViewModelService] Culture ViewModel not found for ID: {cultureID}");
            return null;
        }

        /// <summary>
        /// Get all culture ViewModels.
        /// </summary>
        public IEnumerable<CultureViewModel> GetAllCultureViewModels()
        {
            return cultureViewModels.Values;
        }

        /// <summary>
        /// Refresh a specific culture ViewModel.
        /// </summary>
        public void RefreshCultureViewModel(int cultureID)
        {
            if (cultureViewModels.TryGetValue(cultureID, out var viewModel))
            {
                viewModel.Refresh();
                OnCultureViewModelUpdated?.Invoke(cultureID);
            }
        }

        /// <summary>
        /// Refresh all culture ViewModels.
        /// </summary>
        public void RefreshAllCultureViewModels()
        {
            foreach (var viewModel in cultureViewModels.Values)
            {
                viewModel.Refresh();
            }
        }

        // ========== GOVERNMENT VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register or get a government ViewModel for a realm.
        /// </summary>
        public GovernmentViewModel GetGovernmentViewModel(int realmID)
        {
            if (governmentViewModels.TryGetValue(realmID, out var viewModel))
                return viewModel;

            // Auto-create on first access
            var newViewModel = new GovernmentViewModel(realmID);
            governmentViewModels[realmID] = newViewModel;

            return newViewModel;
        }

        /// <summary>
        /// Refresh a specific government ViewModel.
        /// </summary>
        public void RefreshGovernmentViewModel(int realmID)
        {
            if (governmentViewModels.TryGetValue(realmID, out var viewModel))
            {
                viewModel.Refresh();
                OnGovernmentViewModelUpdated?.Invoke(realmID);
            }
        }

        // ========== BUREAUCRACY VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register or get a bureaucracy ViewModel for a realm.
        /// </summary>
        public BureaucracyViewModel GetBureaucracyViewModel(int realmID)
        {
            if (bureaucracyViewModels.TryGetValue(realmID, out var viewModel))
                return viewModel;

            // Auto-create on first access
            var newViewModel = new BureaucracyViewModel(realmID);
            bureaucracyViewModels[realmID] = newViewModel;

            return newViewModel;
        }

        /// <summary>
        /// Refresh a specific bureaucracy ViewModel.
        /// </summary>
        public void RefreshBureaucracyViewModel(int realmID)
        {
            if (bureaucracyViewModels.TryGetValue(realmID, out var viewModel))
            {
                viewModel.Refresh();
                OnBureaucracyViewModelUpdated?.Invoke(realmID);
            }
        }

        // ========== CONTRACT VIEWMODEL MANAGEMENT ==========
        /// <summary>
        /// Register a contract ViewModel.
        /// </summary>
        public void RegisterContract(Contract contract)
        {
            if (contract == null)
            {
                Debug.LogError("[ViewModelService] Cannot register null contract");
                return;
            }

            if (contractViewModels.ContainsKey(contract.ContractID))
                return;

            var viewModel = new ContractViewModel(contract);
            contractViewModels[contract.ContractID] = viewModel;
        }

        /// <summary>
        /// Get a contract ViewModel by ID.
        /// </summary>
        public ContractViewModel GetContractViewModel(int contractID)
        {
            if (contractViewModels.TryGetValue(contractID, out var viewModel))
                return viewModel;

            Debug.LogWarning($"[ViewModelService] Contract ViewModel not found for ID: {contractID}");
            return null;
        }

        /// <summary>
        /// Refresh a specific contract ViewModel.
        /// </summary>
        public void RefreshContractViewModel(int contractID)
        {
            if (contractViewModels.TryGetValue(contractID, out var viewModel))
            {
                viewModel.Refresh();
                OnContractViewModelUpdated?.Invoke(contractID);
            }
        }

        /// <summary>
        /// Remove a contract ViewModel when contract is terminated.
        /// </summary>
        public void UnregisterContract(int contractID)
        {
            contractViewModels.Remove(contractID);
        }

        // ========== EVENT HANDLERS ==========
        private void OnNewReligionRegistered()
        {
            // Automatically register all religions when a new one is added
            if (ReligionManager.Instance != null)
            {
                foreach (var religion in ReligionManager.Instance.GetAllReligions())
                {
                    RegisterReligion(religion);
                }
            }
        }

        private void OnPopulationConverted(int popGroupID, int oldReligionID, int newReligionID)
        {
            // Refresh both religion ViewModels when conversion happens
            RefreshReligionViewModel(oldReligionID);
            RefreshReligionViewModel(newReligionID);
        }

        private void OnCityCultureChanged(int cityID, int oldCultureID, int newCultureID)
        {
            // Refresh both culture ViewModels when city culture changes
            RefreshCultureViewModel(oldCultureID);
            RefreshCultureViewModel(newCultureID);

            // Also refresh the city ViewModel
            RefreshCityViewModel(cityID);
        }

        private void OnCultureBuildingsChanged(int cultureID)
        {
            // Refresh culture ViewModel when buildings change
            RefreshCultureViewModel(cultureID);
        }

        private void OnCultureXPAdded(int cultureID, TreeType treeType, float xpAmount)
        {
            // Refresh culture ViewModel when XP is added
            RefreshCultureViewModel(cultureID);
        }

        // ========== GOVERNMENT EVENT HANDLERS ==========
        private void OnGovernmentChanged(int realmID, GovernmentData newGovernment)
        {
            // Refresh government and bureaucracy ViewModels when government changes
            RefreshGovernmentViewModel(realmID);
            RefreshBureaucracyViewModel(realmID);
        }

        private void OnOfficeAssigned(int realmID, Office office)
        {
            // Refresh bureaucracy ViewModel when office is assigned
            RefreshBureaucracyViewModel(realmID);
        }

        private void OnOfficeCreated(int realmID, Office office)
        {
            // Refresh bureaucracy ViewModel when new office is created
            RefreshBureaucracyViewModel(realmID);
        }

        private void OnEdictEnacted(int realmID, Edict edict)
        {
            // Refresh government ViewModel when edict is enacted
            RefreshGovernmentViewModel(realmID);
        }

        private void OnLegitimacyChanged(int realmID, float newLegitimacy)
        {
            // Refresh government ViewModel when legitimacy changes
            RefreshGovernmentViewModel(realmID);
        }

        private void OnCapacityChanged(int realmID, float newCapacity)
        {
            // Refresh government and bureaucracy ViewModels when capacity changes
            RefreshGovernmentViewModel(realmID);
            RefreshBureaucracyViewModel(realmID);
        }

        // ========== CONTRACT EVENT HANDLERS ==========
        private void OnContractCreated(Contract contract)
        {
            // Auto-register the contract ViewModel
            RegisterContract(contract);
        }

        private void OnContractTerminated(Contract contract)
        {
            // Unregister the contract ViewModel
            UnregisterContract(contract.ContractID);
        }

        private void OnContractLoyaltyChanged(Contract contract, float newLoyalty)
        {
            // Refresh the contract ViewModel
            RefreshContractViewModel(contract.ContractID);
        }

        // ========== DEBUGGING ==========
        [ContextMenu("Log All City ViewModels")]
        private void LogAllCityViewModels()
        {
            Debug.Log($"[ViewModelService] Total Cities: {cityViewModels.Count}");

            foreach (var kvp in cityViewModels)
            {
                var vm = kvp.Value;
                Debug.Log($"--- City: {vm.CityName} (ID: {vm.CityID}) ---");
                Debug.Log($"  Population: {vm.TotalPopulation}");
                Debug.Log($"  {vm.GetPopulationBreakdownSummary()}");
                Debug.Log($"  {vm.GetDemographicSummary()}");
                Debug.Log($"  {vm.GetGenderSummary()}");
                Debug.Log($"  {vm.GetLaborSummary()}");
                Debug.Log($"  {vm.GetEconomySummary()}");
            }
        }

        [ContextMenu("Log All Religion ViewModels")]
        private void LogAllReligionViewModels()
        {
            Debug.Log($"[ViewModelService] Total Religions: {religionViewModels.Count}");

            foreach (var kvp in religionViewModels)
            {
                var vm = kvp.Value;
                Debug.Log($"--- Religion: {vm.ReligionName} (ID: {vm.ReligionID}) ---");
                Debug.Log($"  {vm.GetIdentitySummary()}");
                Debug.Log($"  {vm.GetOrganizationSummary()}");
                Debug.Log($"  {vm.GetConversionSummary()}");
                Debug.Log($"  {vm.GetContentSummary()}");
            }
        }

        [ContextMenu("Log All Culture ViewModels")]
        private void LogAllCultureViewModels()
        {
            Debug.Log($"[ViewModelService] Total Cultures: {cultureViewModels.Count}");

            foreach (var kvp in cultureViewModels)
            {
                var vm = kvp.Value;
                Debug.Log($"--- Culture: {vm.CultureName} (ID: {vm.CultureID}) ---");
                Debug.Log($"  {vm.GetIdentitySummary()}");
                Debug.Log($"  {vm.GetTechTreeSummary()}");
                Debug.Log($"  {vm.GetBuildingSummary()}");
                Debug.Log($"  Pillars: {vm.GetPillarsList()}");
            }
        }
    }
}
