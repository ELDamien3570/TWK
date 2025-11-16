using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;
using TWK.Economy;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying culture data in the UI.
    /// Exposes CultureData in a UI-friendly format with tech tree progress.
    /// </summary>
    public class CultureViewModel : BaseViewModel
    {
        private CultureData _cultureSource;

        // ========== IDENTITY ==========
        public string CultureName { get; private set; }
        public int CultureID { get; private set; }
        public string Description { get; private set; }
        public Color CultureColor { get; private set; }
        public Sprite Icon { get; private set; }

        // ========== HYBRID CULTURE ==========
        public bool IsHybrid { get; private set; }
        public int ParentCultureCount { get; private set; }
        public List<string> ParentCultureNames { get; private set; }

        // ========== CULTURAL PILLARS ==========
        public int PillarCount { get; private set; }
        public List<string> PillarNames { get; private set; }

        // ========== TECH TREES ==========
        public int TechTreeCount { get; private set; }
        public Dictionary<TreeType, TechTreeSummary> TechTreeSummaries { get; private set; }

        // ========== BUILDINGS ==========
        public int DefaultBuildingCount { get; private set; }
        public int TotalUnlockedBuildingCount { get; private set; }
        public List<string> DefaultBuildingNames { get; private set; }

        // ========== MODIFIERS ==========
        public int ActiveModifierCount { get; private set; }

        // ========== CONSTRUCTOR ==========
        public CultureViewModel(CultureData cultureData)
        {
            _cultureSource = cultureData;
            ParentCultureNames = new List<string>();
            PillarNames = new List<string>();
            DefaultBuildingNames = new List<string>();
            TechTreeSummaries = new Dictionary<TreeType, TechTreeSummary>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_cultureSource == null) return;

            // Identity
            CultureName = _cultureSource.CultureName;
            CultureID = _cultureSource.GetCultureID();
            Description = _cultureSource.Description;
            CultureColor = _cultureSource.CultureColor;
            Icon = _cultureSource.Icon;

            // Hybrid culture info
            IsHybrid = _cultureSource.IsHybrid;
            ParentCultureCount = _cultureSource.ParentCultures?.Count ?? 0;
            ParentCultureNames.Clear();
            if (_cultureSource.ParentCultures != null)
            {
                foreach (var parent in _cultureSource.ParentCultures)
                {
                    if (parent != null)
                        ParentCultureNames.Add(parent.CultureName);
                }
            }

            // Cultural pillars
            PillarCount = _cultureSource.Pillars?.Count ?? 0;
            PillarNames.Clear();
            if (_cultureSource.Pillars != null)
            {
                foreach (var pillar in _cultureSource.Pillars)
                {
                    if (pillar != null)
                        PillarNames.Add(pillar.PillarName);
                }
            }

            // Tech trees
            RefreshTechTrees();

            // Buildings
            DefaultBuildingCount = _cultureSource.DefaultBuildingUnlocks?.Count ?? 0;
            DefaultBuildingNames.Clear();
            if (_cultureSource.DefaultBuildingUnlocks != null)
            {
                foreach (var building in _cultureSource.DefaultBuildingUnlocks)
                {
                    if (building != null)
                        DefaultBuildingNames.Add(building.BuildingName);
                }
            }

            TotalUnlockedBuildingCount = _cultureSource.GetAllUnlockedBuildings()?.Count ?? 0;

            // Modifiers
            ActiveModifierCount = _cultureSource.GetAllModifiers()?.Count ?? 0;

            NotifyPropertyChanged();
        }

        private void RefreshTechTrees()
        {
            TechTreeSummaries.Clear();
            TechTreeCount = 0;

            if (_cultureSource.TechTrees == null) return;

            foreach (var tree in _cultureSource.TechTrees)
            {
                if (tree == null) continue;

                TechTreeCount++;

                var summary = new TechTreeSummary
                {
                    TreeType = tree.TreeType,
                    OwnerRealmID = tree.OwnerRealmID,
                    AccumulatedXP = tree.AccumulatedXP,
                    TotalXPEarned = tree.TotalXPEarned,
                    TotalNodes = tree.AllNodes?.Count ?? 0,
                    UnlockedNodes = tree.UnlockedNodeIDs?.Count ?? 0,
                    UnlockedBuildingCount = tree.GetUnlockedBuildings()?.Count ?? 0
                };

                summary.ProgressPercentage = summary.TotalNodes > 0
                    ? (summary.UnlockedNodes / (float)summary.TotalNodes) * 100f
                    : 0f;

                TechTreeSummaries[tree.TreeType] = summary;
            }
        }

        // ========== HELPER METHODS ==========
        public string GetIdentitySummary()
        {
            string hybrid = IsHybrid ? " (Hybrid)" : "";
            return $"{CultureName}{hybrid} - {PillarCount} Pillars";
        }

        public string GetTechTreeSummary()
        {
            int totalNodes = 0;
            int unlockedNodes = 0;

            foreach (var summary in TechTreeSummaries.Values)
            {
                totalNodes += summary.TotalNodes;
                unlockedNodes += summary.UnlockedNodes;
            }

            float progress = totalNodes > 0 ? (unlockedNodes / (float)totalNodes) * 100f : 0f;
            return $"Tech Progress: {unlockedNodes}/{totalNodes} nodes ({progress:F1}%)";
        }

        public string GetBuildingSummary()
        {
            return $"Default: {DefaultBuildingCount} | Total Unlocked: {TotalUnlockedBuildingCount}";
        }

        public string GetPillarsList()
        {
            return PillarNames.Count > 0 ? string.Join(", ", PillarNames) : "None";
        }

        public string GetParentCulturesList()
        {
            return ParentCultureNames.Count > 0 ? string.Join(" + ", ParentCultureNames) : "Original Culture";
        }

        public TechTreeSummary GetTechTreeSummary(TreeType treeType)
        {
            return TechTreeSummaries.GetValueOrDefault(treeType, null);
        }

        public string GetTreeProgressSummary(TreeType treeType)
        {
            var summary = GetTechTreeSummary(treeType);
            if (summary == null)
                return $"{treeType}: No tree";

            return $"{treeType}: {summary.UnlockedNodes}/{summary.TotalNodes} nodes ({summary.ProgressPercentage:F1}%) | XP: {summary.AccumulatedXP:F0}";
        }
    }

    /// <summary>
    /// Summary data for a single tech tree.
    /// </summary>
    public class TechTreeSummary
    {
        public TreeType TreeType { get; set; }
        public int OwnerRealmID { get; set; }
        public float AccumulatedXP { get; set; }
        public float TotalXPEarned { get; set; }
        public int TotalNodes { get; set; }
        public int UnlockedNodes { get; set; }
        public int UnlockedBuildingCount { get; set; }
        public float ProgressPercentage { get; set; }
    }
}
