using System.Collections.Generic;
using UnityEngine;
using TWK.Religion;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying religion data in the UI.
    /// Exposes ReligionData in a UI-friendly format.
    /// </summary>
    public class ReligionViewModel : BaseViewModel
    {
        private ReligionData _religionSource;

        // ========== IDENTITY ==========
        public string ReligionName { get; private set; }
        public int ReligionID { get; private set; }
        public string Description { get; private set; }
        public ReligionType ReligionType { get; private set; }
        public bool IsCult { get; private set; }

        // ========== ORGANIZATION ==========
        public ReligionTradition Tradition { get; private set; }
        public ReligionCentralization Centralization { get; private set; }
        public ReligionEvangelism Evangelism { get; private set; }
        public ReligionSyncretism Syncretism { get; private set; }

        public bool IsCentralized { get; private set; }
        public bool IsEvangelical { get; private set; }
        public bool IsSyncretic { get; private set; }

        // ========== HEAD OF FAITH ==========
        public string HeadOfFaithTitle { get; private set; }
        public HeadOfFaithPowers HeadPowers { get; private set; }

        // ========== CONVERSION ==========
        public float ConversionResistance { get; private set; }
        public float ConversionSpeed { get; private set; }

        // ========== FERVOR ==========
        public float BaseFervor { get; private set; }
        public float FervorDecayRate { get; private set; }

        // ========== VISUAL ==========
        public Sprite ReligionIcon { get; private set; }
        public Color ReligionColor { get; private set; }

        // ========== DEITIES & CONTENT ==========
        public int DeityCount { get; private set; }
        public List<string> DeityNames { get; private set; }
        public int TenetCount { get; private set; }
        public List<string> TenetNames { get; private set; }
        public int HolyLandCount { get; private set; }
        public int FestivalCount { get; private set; }
        public int RitualCount { get; private set; }

        // ========== CONSTRUCTOR ==========
        public ReligionViewModel(ReligionData religionData)
        {
            _religionSource = religionData;
            DeityNames = new List<string>();
            TenetNames = new List<string>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_religionSource == null) return;

            // Identity
            ReligionName = _religionSource.ReligionName;
            ReligionID = _religionSource.GetStableReligionID();
            Description = _religionSource.Description;
            ReligionType = _religionSource.ReligionType;
            IsCult = _religionSource.IsCult();

            // Organization
            Tradition = _religionSource.Tradition;
            Centralization = _religionSource.Centralization;
            Evangelism = _religionSource.Evangelism;
            Syncretism = _religionSource.Syncretism;

            IsCentralized = _religionSource.IsCentralized();
            IsEvangelical = _religionSource.IsEvangelical();
            IsSyncretic = _religionSource.IsSyncretic();

            // Head of Faith
            HeadOfFaithTitle = _religionSource.HeadOfFaithTitle;
            HeadPowers = _religionSource.HeadPowers;

            // Conversion
            ConversionResistance = _religionSource.ConversionResistance;
            ConversionSpeed = _religionSource.ConversionSpeed;

            // Fervor
            BaseFervor = _religionSource.BaseFervor;
            FervorDecayRate = _religionSource.FervorDecayRate;

            // Visual
            ReligionIcon = _religionSource.ReligionIcon;
            ReligionColor = _religionSource.ReligionColor;

            // Deities
            DeityCount = _religionSource.Deities?.Count ?? 0;
            DeityNames.Clear();
            if (_religionSource.Deities != null)
            {
                foreach (var deity in _religionSource.Deities)
                {
                    if (deity != null)
                        DeityNames.Add(deity.DeityName);
                }
            }

            // Tenets
            TenetCount = _religionSource.Tenets?.Count ?? 0;
            TenetNames.Clear();
            if (_religionSource.Tenets != null)
            {
                foreach (var tenet in _religionSource.Tenets)
                {
                    if (tenet != null)
                        TenetNames.Add(tenet.TenetName);
                }
            }

            // Content counts
            HolyLandCount = _religionSource.GetAllHolyLands()?.Count ?? 0;
            FestivalCount = _religionSource.GetAllFestivals()?.Count ?? 0;
            RitualCount = _religionSource.GetAllRituals()?.Count ?? 0;

            NotifyPropertyChanged();
        }

        // ========== HELPER METHODS ==========
        public string GetIdentitySummary()
        {
            return $"{ReligionName} - {ReligionType} ({Centralization}, {Evangelism})";
        }

        public string GetOrganizationSummary()
        {
            return $"Tradition: {Tradition} | Organization: {Centralization} | Evangelism: {Evangelism} | Syncretism: {Syncretism}";
        }

        public string GetConversionSummary()
        {
            return $"Resistance: {ConversionResistance:F0} | Speed: {ConversionSpeed:F1}x";
        }

        public string GetFervorSummary()
        {
            return $"Base: {BaseFervor:F0} | Decay: {FervorDecayRate:F1}/season";
        }

        public string GetContentSummary()
        {
            return $"Deities: {DeityCount} | Tenets: {TenetCount} | Holy Lands: {HolyLandCount} | Festivals: {FestivalCount} | Rituals: {RitualCount}";
        }

        public string GetDeitiesList()
        {
            return DeityNames.Count > 0 ? string.Join(", ", DeityNames) : "None";
        }

        public string GetTenetsList()
        {
            return TenetNames.Count > 0 ? string.Join(", ", TenetNames) : "None";
        }
    }
}
