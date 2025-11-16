using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Realms;
using TWK.Core;
using TWK.Simulation;

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

        // ========== EVENTS ==========
        /// <summary>
        /// Invoked when any ViewModel is updated.
        /// </summary>
        public event Action OnViewModelsUpdated;

        /// <summary>
        /// Invoked when a specific city ViewModel is updated.
        /// </summary>
        public event Action<int> OnCityViewModelUpdated;

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

            Debug.Log("[ViewModelService] Initialized and subscribed to time events");
        }

        private void OnDestroy()
        {
            if (worldTimeManager != null)
            {
                worldTimeManager.OnDayTick -= OnDayTick;
                worldTimeManager.OnSeasonTick -= OnSeasonTick;
                worldTimeManager.OnYearTick -= OnYearTick;
            }
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
    }
}
