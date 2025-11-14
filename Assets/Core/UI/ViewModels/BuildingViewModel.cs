using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TWK.Economy;
using TWK.Realms.Demographics;
using TWK.Cultures;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying building data.
    /// Exposes BuildingInstanceData and BuildingDefinition in a UI-friendly format.
    /// </summary>
    public class BuildingViewModel : BaseViewModel
    {
        private BuildingInstanceData _instanceData;
        private BuildingDefinition _definition;
        private BuildingData _legacyData; // For backward compatibility
        private int _buildingID;

        // ========== IDENTITY ==========
        public int BuildingID { get; private set; }
        public int CityID { get; private set; }
        public string BuildingName { get; private set; }
        public TreeType BuildingCategory { get; private set; }
        public string CategoryName { get; private set; }

        // ========== LOCATION ==========
        public Vector3 Position { get; private set; }
        public string PositionFormatted { get; private set; }

        // ========== STATE ==========
        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsUnderConstruction { get; private set; }
        public bool CanBeCancelled { get; private set; }
        public float EfficiencyMultiplier { get; private set; }
        public string EfficiencyFormatted { get; private set; }

        // ========== CONSTRUCTION ==========
        public BuildingConstructionState ConstructionState { get; private set; }
        public string ConstructionStateFormatted { get; private set; }
        public int ConstructionDaysRemaining { get; private set; }
        public int TotalConstructionDays { get; private set; }
        public float ConstructionProgress { get; private set; }
        public string ConstructionProgressFormatted { get; private set; }

        // ========== HUB SYSTEM ==========
        public bool IsHub { get; private set; }
        public bool IsHublet { get; private set; }
        public int AttachedToHubID { get; private set; }
        public bool IsAttachedToHub { get; private set; }
        public int AttachedHubletCount { get; private set; }
        public List<int> AttachedHubletIDs { get; private set; }
        public List<TreeType> RequiredHubTypes { get; private set; }

        // ========== WORKERS ==========
        public bool RequiresWorkers { get; private set; }
        public int MinWorkers { get; private set; }
        public int OptimalWorkers { get; private set; }
        public int TotalWorkers { get; private set; }
        public Dictionary<PopulationArchetypes, int> AssignedWorkers { get; private set; }
        public float WorkerRatio { get; private set; }
        public string WorkerRatioFormatted { get; private set; }
        public bool IsSufficientlyStaffed { get; private set; }
        public bool IsOptimallyStaffed { get; private set; }
        public float AverageWorkerEfficiency { get; private set; }

        // ========== COSTS ==========
        public Dictionary<ResourceType, int> BuildCost { get; private set; }
        public Dictionary<ResourceType, int> MaintenanceCost { get; private set; }
        public string BuildCostSummary { get; private set; }
        public string MaintenanceCostSummary { get; private set; }

        // ========== PRODUCTION ==========
        public Dictionary<ResourceType, int> BaseProduction { get; private set; }
        public Dictionary<ResourceType, int> MaxProduction { get; private set; }
        public Dictionary<ResourceType, int> CurrentProduction { get; private set; }
        public string ProductionSummary { get; private set; }

        // ========== POPULATION EFFECTS ==========
        public float EducationGrowthPerWorker { get; private set; }
        public float WealthGrowthPerWorker { get; private set; }
        public PopulationArchetypes? AffectsSpecificArchetype { get; private set; }
        public string AffectedArchetypeName { get; private set; }
        public float PopulationGrowthBonus { get; private set; }
        public bool HasPopulationEffects { get; private set; }

        // ========== CONSTRUCTOR ==========
        public BuildingViewModel(int buildingID)
        {
            _buildingID = buildingID;
            AssignedWorkers = new Dictionary<PopulationArchetypes, int>();
            AttachedHubletIDs = new List<int>();
            RequiredHubTypes = new List<TreeType>();
            BuildCost = new Dictionary<ResourceType, int>();
            MaintenanceCost = new Dictionary<ResourceType, int>();
            BaseProduction = new Dictionary<ResourceType, int>();
            MaxProduction = new Dictionary<ResourceType, int>();
            CurrentProduction = new Dictionary<ResourceType, int>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            // Get latest data from BuildingManager
            _instanceData = BuildingManager.Instance.GetInstanceData(_buildingID);
            if (_instanceData == null)
            {
                Debug.LogWarning($"[BuildingViewModel] Building ID {_buildingID} not found");
                return;
            }

            _definition = BuildingManager.Instance.GetDefinition(_instanceData.BuildingDefinitionID);

            // Try to get legacy data if definition not available (backward compatibility)
            if (_definition == null)
            {
                _legacyData = BuildingManager.Instance.GetLegacyBuildingData(_buildingID);
            }

            RefreshIdentity();
            RefreshState();
            RefreshConstruction();
            RefreshHubSystem();
            RefreshWorkers();
            RefreshCosts();
            RefreshProduction();
            RefreshPopulationEffects();

            NotifyPropertyChanged();
        }

        private void RefreshIdentity()
        {
            BuildingID = _instanceData.ID;
            CityID = _instanceData.CityID;

            // Use definition if available, otherwise try legacy data, then fallback
            if (_definition != null)
            {
                BuildingName = _definition.BuildingName;
                BuildingCategory = _definition.BuildingCategory;
                CategoryName = _definition.BuildingCategory.ToString();
            }
            else if (_legacyData != null)
            {
                BuildingName = _legacyData.BuildingName;
                BuildingCategory = _legacyData.buildingCategory;
                CategoryName = _legacyData.buildingCategory.ToString();
            }
            else
            {
                BuildingName = $"Building #{_instanceData.ID}";
                BuildingCategory = TreeType.Economics; // Default
                CategoryName = "Unknown";
            }

            Position = _instanceData.Position;
            PositionFormatted = $"({Position.x:F1}, {Position.y:F1}, {Position.z:F1})";
        }

        private void RefreshState()
        {
            IsActive = _instanceData.IsActive;
            IsCompleted = _instanceData.IsCompleted;
            IsUnderConstruction = _instanceData.IsUnderConstruction;
            CanBeCancelled = _instanceData.CanBeCancelled;

            EfficiencyMultiplier = _instanceData.EfficiencyMultiplier;
            EfficiencyFormatted = $"{EfficiencyMultiplier * 100f:F0}%";
        }

        private void RefreshConstruction()
        {
            ConstructionState = _instanceData.ConstructionState;
            ConstructionStateFormatted = ConstructionState.ToString();
            ConstructionDaysRemaining = _instanceData.ConstructionDaysRemaining;
            TotalConstructionDays = _instanceData.TotalConstructionDays;
            ConstructionProgress = _instanceData.ConstructionProgress;
            ConstructionProgressFormatted = $"{ConstructionProgress * 100f:F0}%";
        }

        private void RefreshHubSystem()
        {
            // Use definition if available, otherwise use defaults
            if (_definition != null)
            {
                IsHub = _definition.IsHub;
                IsHublet = _definition.IsHublet;
                RequiredHubTypes = new List<TreeType>(_definition.RequiredHubTypes);
            }
            else
            {
                IsHub = false;
                IsHublet = false;
                RequiredHubTypes = new List<TreeType>();
            }

            AttachedToHubID = _instanceData.AttachedToHubID;
            IsAttachedToHub = AttachedToHubID >= 0;
            AttachedHubletIDs = new List<int>(_instanceData.AttachedHubletIDs);
            AttachedHubletCount = AttachedHubletIDs.Count;
        }

        private void RefreshWorkers()
        {
            // Use definition if available, otherwise use defaults
            if (_definition != null)
            {
                RequiresWorkers = _definition.RequiresWorkers;
                MinWorkers = _definition.MinWorkers;
                OptimalWorkers = _definition.OptimalWorkers;
            }
            else
            {
                RequiresWorkers = false;
                MinWorkers = 0;
                OptimalWorkers = 0;
            }

            TotalWorkers = _instanceData.TotalWorkers;
            AssignedWorkers = new Dictionary<PopulationArchetypes, int>(_instanceData.AssignedWorkers);

            // Calculate worker ratio
            WorkerRatio = OptimalWorkers > 0 ? Mathf.Clamp01(TotalWorkers / (float)OptimalWorkers) : 0f;
            WorkerRatioFormatted = $"{WorkerRatio * 100f:F0}%";

            IsSufficientlyStaffed = TotalWorkers >= MinWorkers;
            IsOptimallyStaffed = TotalWorkers >= OptimalWorkers;

            // Calculate average worker efficiency
            if (TotalWorkers > 0 && _definition != null)
            {
                float totalEfficiency = 0f;
                foreach (var kvp in AssignedWorkers)
                {
                    float archetypeEfficiency = _definition.GetWorkerEfficiency(kvp.Key);
                    totalEfficiency += archetypeEfficiency * kvp.Value;
                }
                AverageWorkerEfficiency = totalEfficiency / TotalWorkers;
            }
            else
            {
                AverageWorkerEfficiency = 1f; // Default efficiency if no definition
            }
        }

        private void RefreshCosts()
        {
            if (_definition != null)
            {
                BuildCost = _definition.BaseBuildCost.ToDictionary();
                MaintenanceCost = _definition.CalculateMaintenanceCost(TotalWorkers);
            }
            else if (_legacyData != null)
            {
                // Use legacy BuildingData for costs
                BuildCost = new Dictionary<ResourceType, int>(_legacyData.BaseBuildCost);
                MaintenanceCost = new Dictionary<ResourceType, int>(_legacyData.BaseMaintenanceCost);
            }
            else
            {
                BuildCost = new Dictionary<ResourceType, int>();
                MaintenanceCost = new Dictionary<ResourceType, int>();
            }

            BuildCostSummary = FormatResourceDictionary(BuildCost);
            MaintenanceCostSummary = FormatResourceDictionary(MaintenanceCost);
        }

        private void RefreshProduction()
        {
            if (_definition != null)
            {
                BaseProduction = _definition.BaseProduction.ToDictionary();
                MaxProduction = _definition.MaxProduction.ToDictionary();
                CurrentProduction = BuildingSimulation.CalculateProduction(_instanceData, _definition);
            }
            else if (_legacyData != null)
            {
                // Use legacy BuildingData for production
                BaseProduction = new Dictionary<ResourceType, int>(_legacyData.BaseProduction);
                MaxProduction = new Dictionary<ResourceType, int>(_legacyData.BaseProduction);

                // Calculate current production based on legacy data and building state
                CurrentProduction = new Dictionary<ResourceType, int>();
                if (_instanceData.IsCompleted && _instanceData.IsActive)
                {
                    foreach (var kvp in _legacyData.BaseProduction)
                    {
                        // Apply efficiency multiplier to base production
                        int production = Mathf.RoundToInt(kvp.Value * _instanceData.EfficiencyMultiplier);
                        CurrentProduction[kvp.Key] = production;
                    }
                }
            }
            else
            {
                BaseProduction = new Dictionary<ResourceType, int>();
                MaxProduction = new Dictionary<ResourceType, int>();
                CurrentProduction = new Dictionary<ResourceType, int>();
            }

            ProductionSummary = FormatResourceDictionary(CurrentProduction);
        }

        private void RefreshPopulationEffects()
        {
            if (_definition != null)
            {
                EducationGrowthPerWorker = _definition.EducationGrowthPerWorker;
                WealthGrowthPerWorker = _definition.WealthGrowthPerWorker;
                AffectsSpecificArchetype = _definition.AffectsSpecificArchetype;
                AffectedArchetypeName = AffectsSpecificArchetype?.ToString() ?? "All Workers";
                PopulationGrowthBonus = _definition.PopulationGrowthBonus;
            }
            else
            {
                EducationGrowthPerWorker = 0f;
                WealthGrowthPerWorker = 0f;
                AffectsSpecificArchetype = null;
                AffectedArchetypeName = "All Workers";
                PopulationGrowthBonus = 0f;
            }

            HasPopulationEffects = EducationGrowthPerWorker > 0f ||
                                  WealthGrowthPerWorker > 0f ||
                                  PopulationGrowthBonus > 0f;
        }

        // ========== HELPER METHODS ==========

        public string GetStatusSummary()
        {
            if (IsUnderConstruction)
                return $"Under Construction ({ConstructionProgressFormatted})";
            if (!IsActive)
                return "Inactive";
            if (!IsSufficientlyStaffed)
                return $"Understaffed ({TotalWorkers}/{MinWorkers})";
            if (!IsOptimallyStaffed)
                return $"Staffed ({TotalWorkers}/{OptimalWorkers})";
            return $"Optimal ({TotalWorkers}/{OptimalWorkers})";
        }

        public string GetWorkerBreakdown()
        {
            if (AssignedWorkers.Count == 0)
                return "No workers assigned";

            var breakdown = AssignedWorkers
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => $"{kvp.Key}: {kvp.Value} ({_definition.GetWorkerEfficiency(kvp.Key) * 100f:F0}% eff)")
                .ToList();

            return string.Join(" | ", breakdown);
        }

        public string GetHubStatus()
        {
            if (IsHub && AttachedHubletCount > 0)
                return $"Hub with {AttachedHubletCount} attached hublet(s)";
            if (IsHub)
                return "Hub (no hublets attached)";
            if (IsHublet && IsAttachedToHub)
                return $"Hublet attached to hub #{AttachedToHubID}";
            if (IsHublet)
                return "Hublet (not attached)";
            return "Standard building";
        }

        public string GetPopulationEffectsSummary()
        {
            if (!HasPopulationEffects)
                return "No population effects";

            var effects = new List<string>();
            if (EducationGrowthPerWorker > 0f)
                effects.Add($"+{EducationGrowthPerWorker:F2} education/worker/day");
            if (WealthGrowthPerWorker > 0f)
                effects.Add($"+{WealthGrowthPerWorker:F2} wealth/worker/day");
            if (PopulationGrowthBonus > 0f)
                effects.Add($"+{PopulationGrowthBonus:F2}% population growth");

            string effectStr = string.Join(" | ", effects);
            return AffectsSpecificArchetype.HasValue
                ? $"{effectStr} (affects {AffectedArchetypeName} only)"
                : effectStr;
        }

        public string GetProductionEfficiency()
        {
            if (!RequiresWorkers)
                return $"No workers required | Efficiency: {EfficiencyFormatted}";

            return $"Workers: {TotalWorkers}/{OptimalWorkers} ({WorkerRatioFormatted}) | " +
                   $"Avg Efficiency: {AverageWorkerEfficiency * 100f:F0}% | " +
                   $"Building: {EfficiencyFormatted}";
        }

        public string GetConstructionStatus()
        {
            if (!IsUnderConstruction)
                return "Completed";

            return $"{ConstructionDaysRemaining} days remaining ({ConstructionProgressFormatted} complete)" +
                   (CanBeCancelled ? " | Can cancel" : "");
        }

        public bool CanAssignWorker(PopulationArchetypes archetype)
        {
            if (_definition == null)
                return false;
            return _definition.CanWorkerTypeWork(archetype);
        }

        public float GetWorkerEfficiency(PopulationArchetypes archetype)
        {
            if (_definition == null)
                return 1f; // Default efficiency
            return _definition.GetWorkerEfficiency(archetype);
        }

        public int GetWorkerCount(PopulationArchetypes archetype)
        {
            return _instanceData.GetWorkerCount(archetype);
        }

        public List<PopulationArchetypes> GetAllowedWorkerTypes()
        {
            if (_definition == null)
                return new List<PopulationArchetypes>(); // Empty list if no definition

            return _definition.AllowedWorkerTypes.Count > 0
                ? new List<PopulationArchetypes>(_definition.AllowedWorkerTypes)
                : System.Enum.GetValues(typeof(PopulationArchetypes)).Cast<PopulationArchetypes>().ToList();
        }

        private string FormatResourceDictionary(Dictionary<ResourceType, int> resources)
        {
            if (resources == null || resources.Count == 0)
                return "None";

            var formatted = resources
                .Where(kvp => kvp.Value != 0)
                .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                .ToList();

            return formatted.Count > 0 ? string.Join(" | ", formatted) : "None";
        }
    }
}
