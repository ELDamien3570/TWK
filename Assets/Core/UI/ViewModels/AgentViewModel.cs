using System.Collections.Generic;
using UnityEngine;
using TWK.Agents;
using TWK.Cultures;
using TWK.Modifiers;
using TWK.Religion;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying agent data in the UI.
    /// Exposes AgentData in a UI-friendly format with summaries and derived values.
    /// </summary>
    public class AgentViewModel : BaseViewModel
    {
        private AgentData _agentSource;
        private Agent _agent; // For accessing Ledger

        // ========== IDENTITY ==========
        public string AgentName { get; private set; }
        public int AgentID { get; private set; }
        public Sprite Icon { get; private set; } // TODO: Add icon to AgentData

        // ========== BIOGRAPHICAL ==========
        public int Age { get; private set; }
        public string Gender { get; private set; }
        public string Sexuality { get; private set; }
        public string LifeStatus { get; private set; } // "Alive, Age 35" or "Deceased at 67"
        public string BirthDate { get; private set; } // "Day 15, Month 3, Year 1025"

        // ========== CULTURE & RELIGION ==========
        public int CultureID { get; private set; }
        public int ReligionID { get; private set; }
        public string CultureName { get; private set; }
        public string ReligionName { get; private set; }
        public string CultureReligionSummary { get; private set; } // "Roman Culture, Hellenistic Faith"

        // ========== REPUTATION ==========
        public float Prestige { get; private set; }
        public float Morality { get; private set; }
        public float Reputation { get; private set; }
        public string ReputationLevel { get; private set; } // "Renowned" / "Infamous" / "Unknown"

        // ========== RELATIONSHIP SUMMARIES ==========
        public int ChildrenCount { get; private set; }
        public int SpouseCount { get; private set; }
        public int LoverCount { get; private set; }
        public int FriendCount { get; private set; }
        public int RivalCount { get; private set; }
        public int CompanionCount { get; private set; }
        public string FamilySummary { get; private set; } // "3 children, 1 spouse"
        public string SocialSummary { get; private set; } // "5 friends, 2 rivals"

        // ========== SKILLS ==========
        public Dictionary<TreeType, int> SkillNodeCounts { get; private set; } // Unlocked nodes per tree
        public TreeType SkillFocus { get; private set; }
        public string TopSkill { get; private set; } // "Warfare (10 nodes)"

        // ========== TRAITS ==========
        public List<PersonalityTrait> Traits { get; private set; }
        public List<string> TraitNames { get; private set; }
        public string TraitSummary { get; private set; } // "Brave, Scholarly, Just"

        // ========== WEALTH ==========
        public int Gold { get; private set; }
        public string WealthStatus { get; private set; } // "Wealthy" / "Poor"
        public int TotalPropertyCount { get; private set; }
        public string PropertySummary { get; private set; } // "5 buildings, 2 cities"

        // ========== COMBAT SUMMARY ==========
        public string CombatPower { get; private set; } // "Strong (75 STR, 80 Leadership)"
        public string HealthStatus { get; private set; } // "Healthy (100/100)" or "Critical (15/100)"
        public bool IsCritical { get; private set; }

        // ========== MODIFIERS ==========
        public List<Modifier> ActiveModifiers { get; private set; }
        public int ActiveModifierCount { get; private set; }

        // ========== STATUS ==========
        public bool IsAlive { get; private set; }
        public bool IsRuler { get; private set; }
        public bool HasOffice { get; private set; }
        public int CurrentOfficeID { get; private set; }
        public int MonthlySalary { get; private set; }

        // ========== CONSTRUCTOR ==========
        public AgentViewModel(AgentData agentData, Agent agent)
        {
            _agentSource = agentData;
            _agent = agent;
            SkillNodeCounts = new Dictionary<TreeType, int>();
            Traits = new List<PersonalityTrait>();
            TraitNames = new List<string>();
            ActiveModifiers = new List<Modifier>();
            Refresh();
        }

        // ========== REFRESH ==========
        public override void Refresh()
        {
            if (_agentSource == null) return;

            // Identity
            AgentName = _agentSource.AgentName;
            AgentID = _agentSource.AgentID;

            // Biographical
            Age = _agentSource.Age;
            Gender = _agentSource.Gender.ToString();
            Sexuality = _agentSource.Sexuality.ToString();
            IsAlive = _agentSource.IsAlive;

            if (IsAlive)
            {
                LifeStatus = $"Alive, Age {Age}";
            }
            else
            {
                LifeStatus = $"Deceased at {Age}";
            }

            BirthDate = $"Day {_agentSource.BirthDay}, Month {_agentSource.BirthMonth}, Year {_agentSource.BirthYear}";

            // Culture & Religion
            RefreshCultureAndReligion();

            // Reputation
            Prestige = _agentSource.Prestige;
            Morality = _agentSource.Morality;
            Reputation = AgentSimulation.CalculateReputation(_agentSource);
            ReputationLevel = GetReputationLevel(Reputation);

            // Relationships
            RefreshRelationships();

            // Skills
            RefreshSkills();

            // Traits
            RefreshTraits();

            // Wealth
            RefreshWealth();

            // Combat
            RefreshCombat();

            // Modifiers
            RefreshModifiers();

            // Status
            IsRuler = _agentSource.IsRuler;
            HasOffice = _agentSource.HasOffice;
            CurrentOfficeID = _agentSource.CurrentOfficeID;
            MonthlySalary = _agentSource.MonthlySalary;

            NotifyPropertyChanged();
        }

        // ========== REFRESH HELPERS ==========

        private void RefreshCultureAndReligion()
        {
            CultureID = _agentSource.CultureID;
            ReligionID = _agentSource.ReligionID;

            // Get culture name
            if (CultureID >= 0 && CultureManager.Instance != null)
            {
                var culture = CultureManager.Instance.GetCulture(CultureID);
                CultureName = culture != null ? culture.CultureName : "Unknown Culture";
            }
            else
            {
                CultureName = "No Culture";
            }

            // Get religion name
            if (ReligionID >= 0 && ReligionManager.Instance != null)
            {
                var religion = ReligionManager.Instance.GetReligion(ReligionID);
                ReligionName = religion != null ? religion.ReligionName : "Unknown Religion";
            }
            else
            {
                ReligionName = "No Religion";
            }

            // Create summary
            CultureReligionSummary = $"{CultureName}, {ReligionName}";
        }

        private void RefreshRelationships()
        {
            ChildrenCount = _agentSource.ChildrenIDs.Count;
            SpouseCount = _agentSource.SpouseIDs.Count;
            LoverCount = _agentSource.LoverIDs.Count;
            FriendCount = _agentSource.FriendIDs.Count;
            RivalCount = _agentSource.RivalIDs.Count;
            CompanionCount = _agentSource.CompanionIDs.Count;

            // Family summary
            List<string> familyParts = new List<string>();
            if (ChildrenCount > 0) familyParts.Add($"{ChildrenCount} {(ChildrenCount == 1 ? "child" : "children")}");
            if (SpouseCount > 0) familyParts.Add($"{SpouseCount} {(SpouseCount == 1 ? "spouse" : "spouses")}");
            FamilySummary = familyParts.Count > 0 ? string.Join(", ", familyParts) : "No family";

            // Social summary
            List<string> socialParts = new List<string>();
            if (FriendCount > 0) socialParts.Add($"{FriendCount} {(FriendCount == 1 ? "friend" : "friends")}");
            if (RivalCount > 0) socialParts.Add($"{RivalCount} {(RivalCount == 1 ? "rival" : "rivals")}");
            SocialSummary = socialParts.Count > 0 ? string.Join(", ", socialParts) : "No friends or rivals";
        }

        private void RefreshSkills()
        {
            SkillNodeCounts.Clear();
            SkillFocus = _agentSource.SkillFocus;

            // Get skill node counts from culture tech trees
            // For now, use SkillLevels as proxy (will be replaced when skill trees are implemented)
            float maxSkill = 0f;
            TreeType topSkillType = TreeType.Warfare;

            foreach (var skill in _agentSource.SkillLevels)
            {
                // Convert XP to node count (assuming 10 XP per node unlock)
                int nodeCount = Mathf.FloorToInt(skill.Value / 10f);
                SkillNodeCounts[skill.Key] = nodeCount;

                if (skill.Value > maxSkill)
                {
                    maxSkill = skill.Value;
                    topSkillType = skill.Key;
                }
            }

            if (SkillNodeCounts.TryGetValue(topSkillType, out int topNodes))
            {
                TopSkill = $"{topSkillType} ({topNodes} nodes)";
            }
            else
            {
                TopSkill = "No skills";
            }
        }

        private void RefreshTraits()
        {
            Traits.Clear();
            TraitNames.Clear();

            foreach (var trait in _agentSource.Traits)
            {
                Traits.Add(trait);
                TraitNames.Add(trait.ToString());
            }

            if (TraitNames.Count > 0)
            {
                TraitSummary = string.Join(", ", TraitNames.GetRange(0, Mathf.Min(3, TraitNames.Count)));
                if (TraitNames.Count > 3)
                    TraitSummary += $" (+{TraitNames.Count - 3} more)";
            }
            else
            {
                TraitSummary = "No traits";
            }
        }

        private void RefreshWealth()
        {
            if (_agent != null && _agent.Ledger != null)
            {
                Gold = _agent.Ledger.GetResource(TWK.Economy.ResourceType.Gold);
                WealthStatus = _agent.Ledger.GetWealthStatus();
            }
            else
            {
                Gold = 0;
                WealthStatus = "Unknown";
            }

            TotalPropertyCount = _agentSource.OwnedBuildingIDs.Count +
                                _agentSource.OwnedCaravanIDs.Count +
                                _agentSource.ControlledCityIDs.Count;

            List<string> propertyParts = new List<string>();
            if (_agentSource.OwnedBuildingIDs.Count > 0)
                propertyParts.Add($"{_agentSource.OwnedBuildingIDs.Count} buildings");
            if (_agentSource.OwnedCaravanIDs.Count > 0)
                propertyParts.Add($"{_agentSource.OwnedCaravanIDs.Count} caravans");
            if (_agentSource.ControlledCityIDs.Count > 0)
                propertyParts.Add($"{_agentSource.ControlledCityIDs.Count} cities");

            PropertySummary = propertyParts.Count > 0 ? string.Join(", ", propertyParts) : "No properties";
        }

        private void RefreshCombat()
        {
            var stats = _agentSource.CombatStats;

            CombatPower = $"{GetStrengthLabel(stats.Strength)} ({stats.Strength:F0} STR, {stats.Leadership:F0} Leadership)";
            HealthStatus = $"{stats.Health:F0}/{stats.MaxHealth:F0}";
            IsCritical = stats.IsCriticalHealth();

            if (IsCritical)
                HealthStatus = $"Critical ({HealthStatus})";
            else if (stats.Health >= stats.MaxHealth)
                HealthStatus = $"Healthy ({HealthStatus})";
            else if (stats.Health >= stats.MaxHealth * 0.5f)
                HealthStatus = $"Wounded ({HealthStatus})";
            else
                HealthStatus = $"Severely Wounded ({HealthStatus})";
        }

        private void RefreshModifiers()
        {
            ActiveModifiers.Clear();
            ActiveModifierCount = 0;

            // Get modifiers from traits
            foreach (var trait in _agentSource.Traits)
            {
                // Traits act as modifiers
                var traitMod = new Modifier(trait.ToString(), GetTraitDescription(trait));
                traitMod.SourceType = "Trait";

                // Add a simple effect with the trait description
                var effect = new ModifierEffect
                {
                    Target = ModifierTarget.Character,
                    EffectType = ModifierEffectType.CharacterPrestige
                };
                // The GetTraitDescription already provides the description
                traitMod.Effects.Add(effect);

                ActiveModifiers.Add(traitMod);
                ActiveModifierCount++;
            }

            // Get modifiers from skills (default modifiers per tree)
            foreach (var skill in _agentSource.SkillLevels)
            {
                if (skill.Value > 0)
                {
                    var skillMod = new Modifier($"{skill.Key} Training", "Skill development bonus");
                    skillMod.SourceType = "Skill";

                    // Add effects based on skill tree
                    var effect = new ModifierEffect
                    {
                        Target = ModifierTarget.Character,
                        EffectType = GetSkillEffectType(skill.Key),
                        Value = skill.Value * 0.1f,
                        IsPercentage = true
                    };
                    skillMod.Effects.Add(effect);

                    ActiveModifiers.Add(skillMod);
                    ActiveModifierCount++;
                }
            }

            // TODO: Get modifiers from unlocked skill tree nodes when implemented
        }

        private ModifierEffectType GetSkillEffectType(TreeType treeType)
        {
            // Map skill trees to appropriate effect types
            switch (treeType)
            {
                case TreeType.Warfare:
                    return ModifierEffectType.MilitaryPower;
                case TreeType.Politics:
                    return ModifierEffectType.CharacterPrestige;
                case TreeType.Economics:
                    return ModifierEffectType.PopulationIncomeGrowth;
                case TreeType.Science:
                    return ModifierEffectType.CultureXPGain;
                case TreeType.Religion:
                    return ModifierEffectType.CharacterPiety;
                default:
                    return ModifierEffectType.CharacterPrestige;
            }
        }

        // ========== HELPER METHODS ==========

        private string GetReputationLevel(float reputation)
        {
            if (reputation >= 100) return "Legendary";
            if (reputation >= 75) return "Renowned";
            if (reputation >= 50) return "Famous";
            if (reputation >= 25) return "Known";
            if (reputation >= 0) return "Unknown";
            if (reputation >= -25) return "Disreputable";
            if (reputation >= -50) return "Notorious";
            return "Infamous";
        }

        private string GetStrengthLabel(float strength)
        {
            if (strength >= 90) return "Mighty";
            if (strength >= 75) return "Strong";
            if (strength >= 60) return "Able";
            if (strength >= 40) return "Average";
            if (strength >= 25) return "Weak";
            return "Feeble";
        }

        private string GetTraitDescription(PersonalityTrait trait)
        {
            // Basic descriptions - can be expanded
            switch (trait)
            {
                case PersonalityTrait.Brave: return "Bonus to combat morale";
                case PersonalityTrait.Cowardly: return "Penalty to combat morale";
                case PersonalityTrait.Charismatic: return "+20% diplomatic effectiveness";
                case PersonalityTrait.Scholarly: return "Faster skill gain in Science";
                case PersonalityTrait.Greedy: return "+10% income, -5 reputation";
                case PersonalityTrait.Just: return "+7 reputation";
                default: return "Personality modifier";
            }
        }

        public string GetIdentitySummary()
        {
            return $"{AgentName}, Age {Age}, {Gender}, {CultureReligionSummary}";
        }
    }
}
