using System.Collections.Generic;
using UnityEngine;
using TWK.Government;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying government data in the UI.
    /// Exposes government information in a UI-friendly format.
    /// </summary>
    public class GovernmentViewModel : BaseViewModel
    {
        private int _realmID;
        private GovernmentData _governmentData;

        // ========== IDENTITY ==========
        public string GovernmentName { get; private set; }
        public int GovernmentID { get; private set; }
        public string Description { get; private set; }
        public Sprite Icon { get; private set; }
        public Color GovernmentColor { get; private set; }

        // ========== REGIME ==========
        public RegimeForm RegimeForm { get; private set; }
        public string RegimeFormDisplay { get; private set; }
        public StateStructure StateStructure { get; private set; }
        public string StateStructureDisplay { get; private set; }
        public SuccessionLaw SuccessionLaw { get; private set; }
        public string SuccessionLawDisplay { get; private set; }
        public Administration Administration { get; private set; }
        public string AdministrationDisplay { get; private set; }
        public Mobility Mobility { get; private set; }
        public string MobilityDisplay { get; private set; }

        // ========== REGIME HELPERS ==========
        public bool IsAutocratic { get; private set; }
        public bool IsPluralist { get; private set; }
        public bool IsChiefdom { get; private set; }
        public bool IsTerritorial { get; private set; }
        public bool IsCityState { get; private set; }
        public bool IsNomadic { get; private set; }

        // ========== INSTITUTIONS ==========
        public int InstitutionCount { get; private set; }
        public List<InstitutionDisplay> Institutions { get; private set; }

        // ========== LAWS ==========
        public MilitaryServiceLaw MilitaryService { get; private set; }
        public string MilitaryServiceDisplay { get; private set; }
        public TaxationLaw TaxationLaw { get; private set; }
        public string TaxationDisplay { get; private set; }
        public TradeLaw TradeLaw { get; private set; }
        public string TradeDisplay { get; private set; }
        public JusticeLaw JusticeLaw { get; private set; }
        public string JusticeDisplay { get; private set; }

        // ========== MILITARY SERVICE HELPERS ==========
        public bool HasLevies { get; private set; }
        public bool HasWarbands { get; private set; }
        public bool HasProfessionalArmies { get; private set; }

        // ========== STATS ==========
        public float Legitimacy { get; private set; }
        public string LegitimacyDisplay { get; private set; }
        public string LegitimacyStatus { get; private set; }
        public Color LegitimacyColor { get; private set; }

        public float Capacity { get; private set; }
        public string CapacityDisplay { get; private set; }
        public string CapacityStatus { get; private set; }
        public Color CapacityColor { get; private set; }

        public float RevoltRisk { get; private set; }
        public string RevoltRiskDisplay { get; private set; }
        public string RevoltRiskStatus { get; private set; }
        public Color RevoltRiskColor { get; private set; }

        // ========== MODIFIERS ==========
        public int ActiveModifierCount { get; private set; }
        public List<ModifierDisplay> ActiveModifiers { get; private set; }

        // ========== CONSTRUCTOR ==========
        public GovernmentViewModel(int realmID)
        {
            _realmID = realmID;
            Institutions = new List<InstitutionDisplay>();
            ActiveModifiers = new List<ModifierDisplay>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (GovernmentManager.Instance == null)
            {
                GovernmentName = "No Government Manager";
                return;
            }

            _governmentData = GovernmentManager.Instance.GetRealmGovernment(_realmID);

            if (_governmentData == null)
            {
                GovernmentName = "No Government";
                Legitimacy = 50f;
                Capacity = 30f;
                NotifyPropertyChanged();
                return;
            }

            // Identity
            GovernmentName = _governmentData.GovernmentName;
            GovernmentID = _governmentData.GetStableGovernmentID();
            Description = _governmentData.Description;
            Icon = _governmentData.Icon;
            GovernmentColor = _governmentData.GovernmentColor;

            // Regime
            RegimeForm = _governmentData.RegimeForm;
            RegimeFormDisplay = RegimeForm.ToString();
            StateStructure = _governmentData.StateStructure;
            StateStructureDisplay = StateStructure.ToString();
            SuccessionLaw = _governmentData.SuccessionLaw;
            SuccessionLawDisplay = SuccessionLaw.ToString();
            Administration = _governmentData.Administration;
            AdministrationDisplay = Administration.ToString();
            Mobility = _governmentData.Mobility;
            MobilityDisplay = Mobility.ToString();

            // Regime helpers
            IsAutocratic = _governmentData.IsAutocratic();
            IsPluralist = _governmentData.IsPluralist();
            IsChiefdom = _governmentData.IsChiefdom();
            IsTerritorial = _governmentData.IsTerritorial();
            IsCityState = _governmentData.IsCityState();
            IsNomadic = _governmentData.IsNomadic();

            // Institutions
            RefreshInstitutions();

            // Laws
            MilitaryService = _governmentData.MilitaryService;
            MilitaryServiceDisplay = FormatMilitaryService(MilitaryService);
            TaxationLaw = _governmentData.TaxationLaw;
            TaxationDisplay = TaxationLaw.ToString();
            TradeLaw = _governmentData.TradeLaw;
            TradeDisplay = TradeLaw.ToString();
            JusticeLaw = _governmentData.JusticeLaw;
            JusticeDisplay = JusticeLaw.ToString();

            // Military service helpers
            HasLevies = _governmentData.HasLevies();
            HasWarbands = _governmentData.HasWarbands();
            HasProfessionalArmies = _governmentData.HasProfessionalArmies();

            // Stats
            Legitimacy = GovernmentManager.Instance.GetRealmLegitimacy(_realmID);
            LegitimacyDisplay = $"{Legitimacy:F0}%";
            LegitimacyStatus = GetLegitimacyStatus(Legitimacy);
            LegitimacyColor = GetLegitimacyColor(Legitimacy);

            Capacity = GovernmentManager.Instance.GetRealmCapacity(_realmID);
            CapacityDisplay = $"{Capacity:F0}%";
            CapacityStatus = GetCapacityStatus(Capacity);
            CapacityColor = GetCapacityColor(Capacity);

            RevoltRisk = GovernmentManager.Instance.CalculateRevoltRisk(_realmID);
            RevoltRiskDisplay = $"{RevoltRisk:F0}%";
            RevoltRiskStatus = GetRevoltRiskStatus(RevoltRisk);
            RevoltRiskColor = GetRevoltRiskColor(RevoltRisk);

            // Modifiers
            RefreshModifiers();

            NotifyPropertyChanged();
        }

        private void RefreshInstitutions()
        {
            Institutions.Clear();

            if (_governmentData == null || _governmentData.Institutions == null)
            {
                InstitutionCount = 0;
                return;
            }

            foreach (var institution in _governmentData.Institutions)
            {
                if (institution != null)
                {
                    Institutions.Add(new InstitutionDisplay
                    {
                        Name = institution.InstitutionName,
                        Description = institution.Description,
                        Category = institution.Category.ToString(),
                        Icon = institution.Icon,
                        SpecialAbilities = institution.SpecialAbilities
                    });
                }
            }

            InstitutionCount = Institutions.Count;
        }

        private void RefreshModifiers()
        {
            ActiveModifiers.Clear();

            if (GovernmentManager.Instance == null)
            {
                ActiveModifierCount = 0;
                return;
            }

            var modifiers = GovernmentManager.Instance.GetGovernmentModifiers(_realmID);
            foreach (var modifier in modifiers)
            {
                if (modifier != null)
                {
                    ActiveModifiers.Add(new ModifierDisplay
                    {
                        Name = modifier.Name,
                        Description = modifier.GetFullDescription(),
                        Source = modifier.SourceType
                    });
                }
            }

            ActiveModifierCount = ActiveModifiers.Count;
        }

        // ========== FORMATTING HELPERS ==========

        private string FormatMilitaryService(MilitaryServiceLaw law)
        {
            var parts = new List<string>();
            if ((law & MilitaryServiceLaw.Levies) != 0) parts.Add("Levies");
            if ((law & MilitaryServiceLaw.Warbands) != 0) parts.Add("Warbands");
            if ((law & MilitaryServiceLaw.ProfessionalArmies) != 0) parts.Add("Professional Armies");

            if (parts.Count == 0)
                return "None";

            return string.Join(", ", parts);
        }

        private string GetLegitimacyStatus(float legitimacy)
        {
            if (legitimacy >= 80f) return "Very High";
            if (legitimacy >= 60f) return "High";
            if (legitimacy >= 40f) return "Moderate";
            if (legitimacy >= 20f) return "Low";
            return "Crisis";
        }

        private Color GetLegitimacyColor(float legitimacy)
        {
            if (legitimacy >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (legitimacy >= 60f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (legitimacy >= 40f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (legitimacy >= 20f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        private string GetCapacityStatus(float capacity)
        {
            if (capacity >= 80f) return "Excellent";
            if (capacity >= 60f) return "Good";
            if (capacity >= 40f) return "Fair";
            if (capacity >= 20f) return "Poor";
            return "Inadequate";
        }

        private Color GetCapacityColor(float capacity)
        {
            if (capacity >= 80f) return new Color(0.2f, 0.8f, 0.2f); // Green
            if (capacity >= 60f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            if (capacity >= 40f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (capacity >= 20f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            return new Color(0.9f, 0.2f, 0.2f); // Red
        }

        private string GetRevoltRiskStatus(float risk)
        {
            if (risk >= 75f) return "Imminent";
            if (risk >= 50f) return "High";
            if (risk >= 25f) return "Moderate";
            if (risk >= 10f) return "Low";
            return "Minimal";
        }

        private Color GetRevoltRiskColor(float risk)
        {
            if (risk >= 75f) return new Color(0.9f, 0.2f, 0.2f); // Red
            if (risk >= 50f) return new Color(0.9f, 0.5f, 0.2f); // Orange
            if (risk >= 25f) return new Color(0.9f, 0.9f, 0.3f); // Yellow
            if (risk >= 10f) return new Color(0.6f, 0.9f, 0.3f); // Light green
            return new Color(0.2f, 0.8f, 0.2f); // Green
        }

        // ========== SUMMARY METHODS ==========

        public string GetRegimeSummary()
        {
            return $"{RegimeFormDisplay} | {StateStructureDisplay} | {SuccessionLawDisplay}";
        }

        public string GetLawsSummary()
        {
            return $"Military: {MilitaryServiceDisplay} | Tax: {TaxationDisplay} | Trade: {TradeDisplay} | Justice: {JusticeDisplay}";
        }

        public string GetStatsSummary()
        {
            return $"Legitimacy: {LegitimacyDisplay} ({LegitimacyStatus}) | Capacity: {CapacityDisplay} ({CapacityStatus})";
        }
    }

    // ========== DISPLAY CLASSES ==========

    public class InstitutionDisplay
    {
        public string Name;
        public string Description;
        public string Category;
        public Sprite Icon;
        public InstitutionAbilities SpecialAbilities;
    }

    public class ModifierDisplay
    {
        public string Name;
        public string Description;
        public string Source;
    }
}
