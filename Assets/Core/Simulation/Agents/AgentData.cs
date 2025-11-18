using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Cultures;

namespace TWK.Agents
{
    /// <summary>
    /// Pure data class for an agent (character). Contains no logic or MonoBehaviour.
    /// This is the "Model" in MVVM - just serializable state.
    /// Follows the same pattern as CityData and RealmData.
    /// </summary>
    [System.Serializable]
    public class AgentData
    {
        // ========== IDENTITY ==========
        public string AgentName;
        public int AgentID;

        // ========== BIOGRAPHICAL ==========
        public int Age;
        public int BirthDay;
        public int BirthYear;
        public int BirthMonth;
        public Gender Gender = Gender.Male;
        public Sexuality Sexuality = Sexuality.Heterosexual;

        // ========== AFFILIATIONS ==========
        /// <summary>
        /// Realm this agent belongs to (home realm).
        /// </summary>
        public int HomeRealmID = -1;

        /// <summary>
        /// Current office held in government (if any).
        /// -1 if no office held.
        /// </summary>
        public int CurrentOfficeID = -1;

        /// <summary>
        /// Realm where this agent currently holds office (may differ from home realm).
        /// -1 if no office held.
        /// </summary>
        public int OfficeRealmID = -1;

        // ========== CULTURE ==========
        public int CultureID = -1;

        // ========== RELATIONSHIPS ==========
        /// <summary>
        /// Mother's agent ID. -1 if unknown/not set.
        /// </summary>
        public int MotherID = -1;

        /// <summary>
        /// Father's agent ID. -1 if unknown/not set.
        /// </summary>
        public int FatherID = -1;

        /// <summary>
        /// List of children (agent IDs).
        /// </summary>
        public List<int> ChildrenIDs = new List<int>();

        /// <summary>
        /// List of wives/husbands (agent IDs).
        /// </summary>
        public List<int> SpouseIDs = new List<int>();

        /// <summary>
        /// List of lovers/mistresses (agent IDs).
        /// </summary>
        public List<int> LoverIDs = new List<int>();

        /// <summary>
        /// List of friends (agent IDs).
        /// </summary>
        public List<int> FriendIDs = new List<int>();

        /// <summary>
        /// List of rivals (agent IDs).
        /// </summary>
        public List<int> RivalIDs = new List<int>();

        // ========== PERSONALITY & REPUTATION ==========
        /// <summary>
        /// Personality traits. Can be unlocked through skill trees or assigned at creation.
        /// </summary>
        public List<PersonalityTrait> Traits = new List<PersonalityTrait>();

        /// <summary>
        /// Prestige level - earned through accomplishments, titles, victories.
        /// </summary>
        public float Prestige = 0f;

        /// <summary>
        /// Morality score - goes down from breaking contracts, exposed schemes, illegitimate children, etc.
        /// </summary>
        public float Morality = 100f;

        /// <summary>
        /// Reputation - aggregate of stats, traits, prestige, morality.
        /// Can be calculated or cached.
        /// </summary>
        public float Reputation = 0f;

        // ========== SKILLS ==========
        [SerializeField]
        private Dictionary<TreeType, float> _skillLevels = new Dictionary<TreeType, float>();

        public Dictionary<TreeType, float> SkillLevels
        {
            get => _skillLevels;
            set => _skillLevels = value;
        }

        public TreeType SkillFocus;
        public float DailySkillGain = 1f;
        public float DailyFocusBonus = 0.5f;

        // ========== PERSONAL WEALTH ==========
        /// <summary>
        /// Personal resources owned by this agent.
        /// Managed by AgentLedger, stored here for serialization.
        /// </summary>
        public Dictionary<TWK.Economy.ResourceType, int> PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();

        /// <summary>
        /// Monthly salary from current office (if any).
        /// </summary>
        public int MonthlySalary = 0;

        // ========== PROPERTIES ==========
        /// <summary>
        /// List of building IDs owned by this agent (estates, businesses, etc).
        /// </summary>
        public List<int> OwnedBuildingIDs = new List<int>();

        /// <summary>
        /// List of trade caravan IDs owned by this agent.
        /// </summary>
        public List<int> OwnedCaravanIDs = new List<int>();

        /// <summary>
        /// List of city IDs personally controlled by this agent.
        /// </summary>
        public List<int> ControlledCityIDs = new List<int>();

        /// <summary>
        /// List of companion agent IDs (officers for personalized needs).
        /// </summary>
        public List<int> CompanionIDs = new List<int>();

        // ========== COMBAT/UNIT STATS ==========
        // Basic Info
        /// <summary>
        /// How many weapons/ammunition holders can be equipped. Starts at 3.
        /// Every 25 strength past 50 adds one slot (75 STR = 4 slots, 100 STR = 5 slots).
        /// </summary>
        public int WeaponSlots = 3;

        /// <summary>
        /// Strength - modifies resistance to charge attacks and affects armor/attack from equipment.
        /// </summary>
        public float Strength = 50f;

        /// <summary>
        /// Leadership - affects unit responsiveness to commands, formation cohesion, and routing behavior.
        /// </summary>
        public float Leadership = 50f;

        /// <summary>
        /// Morale - current morale level. Modified by supply, recent battles, etc.
        /// When morale drops below leadership, units route. Below 35% applies combat penalties.
        /// </summary>
        public float Morale = 100f;

        /// <summary>
        /// Mount ID this character is riding. Affects speed, charge speed, charge bonus, and health.
        /// -1 if unmounted.
        /// </summary>
        public int MountID = -1;

        /// <summary>
        /// Daily resource consumption cost for this unit.
        /// </summary>
        public Dictionary<TWK.Economy.ResourceType, int> DailyCost = new Dictionary<TWK.Economy.ResourceType, int>();

        // Combat Stats
        /// <summary>
        /// Health points.
        /// </summary>
        public float Health = 100f;

        /// <summary>
        /// Maximum health points.
        /// </summary>
        public float MaxHealth = 100f;

        /// <summary>
        /// Standard walking speed.
        /// </summary>
        public float Speed = 5f;

        /// <summary>
        /// How often the unit dodges incoming ranged attacks/charges.
        /// </summary>
        public float Agility = 10f;

        /// <summary>
        /// How accurately the unit can hit targets with ranged/melee attacks.
        /// Interacts with target's agility for hit chance.
        /// </summary>
        public float Accuracy = 50f;

        /// <summary>
        /// Speed when running/charging.
        /// </summary>
        public float ChargeSpeed = 10f;

        /// <summary>
        /// Bonus damage from charging into enemies.
        /// </summary>
        public float ChargeBonus = 5f;

        /// <summary>
        /// Melee damage output per hit. Determined by strength * equipment.
        /// </summary>
        public float MeleeAttack = 10f;

        /// <summary>
        /// Melee damage reduction. Determined by strength * equipment.
        /// Doesn't affect bonus damage.
        /// </summary>
        public float MeleeArmor = 5f;

        /// <summary>
        /// Extra melee damage when hitting mounted units. Derived from weapon stats.
        /// </summary>
        public float MeleeAttackBonusVsMount = 0f;

        /// <summary>
        /// Ranged damage output per attack.
        /// </summary>
        public float MissileAttack = 0f;

        /// <summary>
        /// Ranged damage reduction. Doesn't include bonus damage.
        /// </summary>
        public float MissileDefense = 0f;

        /// <summary>
        /// Extra ranged damage when hitting mounted units. Derived from weapon stats.
        /// </summary>
        public float MissileAttackBonusVsMount = 0f;

        /// <summary>
        /// How many missiles this unit can throw/shoot.
        /// </summary>
        public int Ammunition = 0;

        /// <summary>
        /// Current ammunition remaining.
        /// </summary>
        public int CurrentAmmunition = 0;

        // ========== EQUIPMENT ==========
        /// <summary>
        /// List of equipped weapon IDs (can have multiple based on WeaponSlots).
        /// </summary>
        public List<int> EquippedWeaponIDs = new List<int>();

        /// <summary>
        /// Head armor/helmet ID. -1 if none equipped.
        /// </summary>
        public int HeadEquipmentID = -1;

        /// <summary>
        /// Body armor ID. -1 if none equipped.
        /// </summary>
        public int BodyEquipmentID = -1;

        /// <summary>
        /// Leg armor ID. -1 if none equipped.
        /// </summary>
        public int LegsEquipmentID = -1;

        /// <summary>
        /// Shield ID. -1 if none equipped.
        /// </summary>
        public int ShieldID = -1;

        // ========== STATUS ==========
        /// <summary>
        /// Is this agent alive?
        /// </summary>
        public bool IsAlive = true;

        /// <summary>
        /// Is this agent currently employed in a government office?
        /// </summary>
        public bool HasOffice => CurrentOfficeID != -1;

        /// <summary>
        /// Is this agent a ruler of their home realm?
        /// </summary>
        public bool IsRuler = false;

        // ========== CONSTRUCTOR ==========
        public AgentData()
        {
            // Default constructor for serialization
            _skillLevels = new Dictionary<TreeType, float>();
            PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();
            OwnedBuildingIDs = new List<int>();
            OwnedCaravanIDs = new List<int>();
            ControlledCityIDs = new List<int>();
            CompanionIDs = new List<int>();
            ChildrenIDs = new List<int>();
            SpouseIDs = new List<int>();
            LoverIDs = new List<int>();
            FriendIDs = new List<int>();
            RivalIDs = new List<int>();
            Traits = new List<PersonalityTrait>();
            EquippedWeaponIDs = new List<int>();
            DailyCost = new Dictionary<TWK.Economy.ResourceType, int>();
        }

        public AgentData(int agentID, string name, int homeRealmID, int cultureID = -1)
        {
            this.AgentID = agentID;
            this.AgentName = name;
            this.HomeRealmID = homeRealmID;
            this.CultureID = cultureID;

            _skillLevels = new Dictionary<TreeType, float>();
            PersonalWealth = new Dictionary<TWK.Economy.ResourceType, int>();
            OwnedBuildingIDs = new List<int>();
            OwnedCaravanIDs = new List<int>();
            ControlledCityIDs = new List<int>();
            CompanionIDs = new List<int>();
            ChildrenIDs = new List<int>();
            SpouseIDs = new List<int>();
            LoverIDs = new List<int>();
            FriendIDs = new List<int>();
            RivalIDs = new List<int>();
            Traits = new List<PersonalityTrait>();
            EquippedWeaponIDs = new List<int>();
            DailyCost = new Dictionary<TWK.Economy.ResourceType, int>();

            // Initialize all skill levels to 0
            foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
            {
                _skillLevels[tree] = 0f;
            }
        }

        // ========== HELPER METHODS ==========

        // --- Relationship Management ---

        /// <summary>
        /// Add a child to this agent.
        /// </summary>
        public void AddChild(int childID)
        {
            if (!ChildrenIDs.Contains(childID))
            {
                ChildrenIDs.Add(childID);
            }
        }

        /// <summary>
        /// Remove a child from this agent.
        /// </summary>
        public void RemoveChild(int childID)
        {
            ChildrenIDs.Remove(childID);
        }

        /// <summary>
        /// Add a spouse to this agent.
        /// </summary>
        public void AddSpouse(int spouseID)
        {
            if (!SpouseIDs.Contains(spouseID))
            {
                SpouseIDs.Add(spouseID);
            }
        }

        /// <summary>
        /// Remove a spouse from this agent.
        /// </summary>
        public void RemoveSpouse(int spouseID)
        {
            SpouseIDs.Remove(spouseID);
        }

        /// <summary>
        /// Add a lover to this agent.
        /// </summary>
        public void AddLover(int loverID)
        {
            if (!LoverIDs.Contains(loverID))
            {
                LoverIDs.Add(loverID);
            }
        }

        /// <summary>
        /// Remove a lover from this agent.
        /// </summary>
        public void RemoveLover(int loverID)
        {
            LoverIDs.Remove(loverID);
        }

        /// <summary>
        /// Add a friend to this agent.
        /// </summary>
        public void AddFriend(int friendID)
        {
            if (!FriendIDs.Contains(friendID))
            {
                FriendIDs.Add(friendID);
            }
        }

        /// <summary>
        /// Remove a friend from this agent.
        /// </summary>
        public void RemoveFriend(int friendID)
        {
            FriendIDs.Remove(friendID);
        }

        /// <summary>
        /// Add a rival to this agent.
        /// </summary>
        public void AddRival(int rivalID)
        {
            if (!RivalIDs.Contains(rivalID))
            {
                RivalIDs.Add(rivalID);
            }
        }

        /// <summary>
        /// Remove a rival from this agent.
        /// </summary>
        public void RemoveRival(int rivalID)
        {
            RivalIDs.Remove(rivalID);
        }

        /// <summary>
        /// Add a companion to this agent.
        /// </summary>
        public void AddCompanion(int companionID)
        {
            if (!CompanionIDs.Contains(companionID))
            {
                CompanionIDs.Add(companionID);
            }
        }

        /// <summary>
        /// Remove a companion from this agent.
        /// </summary>
        public void RemoveCompanion(int companionID)
        {
            CompanionIDs.Remove(companionID);
        }

        // --- Personality & Reputation ---

        /// <summary>
        /// Add a personality trait to this agent.
        /// </summary>
        public void AddTrait(PersonalityTrait trait)
        {
            if (!Traits.Contains(trait))
            {
                Traits.Add(trait);
            }
        }

        /// <summary>
        /// Remove a personality trait from this agent.
        /// </summary>
        public void RemoveTrait(PersonalityTrait trait)
        {
            Traits.Remove(trait);
        }

        /// <summary>
        /// Check if this agent has a specific trait.
        /// </summary>
        public bool HasTrait(PersonalityTrait trait)
        {
            return Traits.Contains(trait);
        }

        // --- Equipment Management ---

        /// <summary>
        /// Equip a weapon. Returns false if no slots available.
        /// </summary>
        public bool EquipWeapon(int weaponID)
        {
            if (EquippedWeaponIDs.Count >= WeaponSlots)
            {
                return false; // No slots available
            }

            if (!EquippedWeaponIDs.Contains(weaponID))
            {
                EquippedWeaponIDs.Add(weaponID);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unequip a weapon.
        /// </summary>
        public void UnequipWeapon(int weaponID)
        {
            EquippedWeaponIDs.Remove(weaponID);
        }

        // --- Combat Stats (Read-Only Checks) ---

        /// <summary>
        /// Is this agent at critical health?
        /// </summary>
        public bool IsCriticalHealth()
        {
            return Health < (MaxHealth * 0.25f);
        }

        /// <summary>
        /// Should this unit route based on morale vs leadership?
        /// </summary>
        public bool ShouldRoute()
        {
            return Morale < Leadership;
        }

        /// <summary>
        /// Is morale low enough to apply combat penalties?
        /// </summary>
        public bool HasLowMoralePenalty()
        {
            return Morale < 35f;
        }

        // --- Cities & Properties ---

        /// <summary>
        /// Add a city to this agent's controlled cities.
        /// </summary>
        public void AddControlledCity(int cityID)
        {
            if (!ControlledCityIDs.Contains(cityID))
            {
                ControlledCityIDs.Add(cityID);
            }
        }

        /// <summary>
        /// Remove a city from this agent's controlled cities.
        /// </summary>
        public void RemoveControlledCity(int cityID)
        {
            ControlledCityIDs.Remove(cityID);
        }

        /// <summary>
        /// Add a building to this agent's property list.
        /// </summary>
        public void AddOwnedBuilding(int buildingID)
        {
            if (!OwnedBuildingIDs.Contains(buildingID))
            {
                OwnedBuildingIDs.Add(buildingID);
            }
        }

        /// <summary>
        /// Remove a building from this agent's property list.
        /// </summary>
        public void RemoveOwnedBuilding(int buildingID)
        {
            OwnedBuildingIDs.Remove(buildingID);
        }

        /// <summary>
        /// Add a caravan to this agent's property list.
        /// </summary>
        public void AddOwnedCaravan(int caravanID)
        {
            if (!OwnedCaravanIDs.Contains(caravanID))
            {
                OwnedCaravanIDs.Add(caravanID);
            }
        }

        /// <summary>
        /// Remove a caravan from this agent's property list.
        /// </summary>
        public void RemoveOwnedCaravan(int caravanID)
        {
            OwnedCaravanIDs.Remove(caravanID);
        }

        /// <summary>
        /// Assign this agent to a government office.
        /// </summary>
        public void AssignToOffice(int officeID, int realmID, int salary)
        {
            CurrentOfficeID = officeID;
            OfficeRealmID = realmID;
            MonthlySalary = salary;
        }

        /// <summary>
        /// Remove this agent from their current office.
        /// </summary>
        public void RemoveFromOffice()
        {
            CurrentOfficeID = -1;
            OfficeRealmID = -1;
            MonthlySalary = 0;
        }

        /// <summary>
        /// Calculate total wealth across all resource types.
        /// </summary>
        public int GetTotalWealth()
        {
            int total = 0;
            foreach (var kvp in PersonalWealth)
            {
                // Convert all resources to gold equivalent (simple 1:1 for now)
                total += kvp.Value;
            }
            return total;
        }
    }
}
