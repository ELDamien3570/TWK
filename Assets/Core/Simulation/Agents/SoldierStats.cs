using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;

namespace TWK.Agents
{
    /// <summary>
    /// Pure data structure for soldier/unit combat statistics.
    /// Can be used by both individual agents and army units.
    /// Represents combat capabilities of a fighter or group of fighters.
    /// </summary>
    [System.Serializable]
    public class SoldierStats
    {
        // ========== BASIC ATTRIBUTES ==========

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
        /// Mount ID this unit is riding. Affects speed, charge speed, charge bonus, and health.
        /// -1 if unmounted.
        /// </summary>
        public int MountID = -1;

        /// <summary>
        /// Daily resource consumption cost for this unit.
        /// </summary>
        public Dictionary<ResourceType, int> DailyCost = new Dictionary<ResourceType, int>();

        // ========== COMBAT STATS ==========

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

        // ========== MELEE STATS ==========

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

        // ========== MISSILE STATS ==========

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

        // ========== CONSTRUCTORS ==========

        public SoldierStats()
        {
            // Default constructor
            EquippedWeaponIDs = new List<int>();
            DailyCost = new Dictionary<ResourceType, int>();
        }

        public SoldierStats(float strength, float leadership)
        {
            this.Strength = strength;
            this.Leadership = leadership;
            EquippedWeaponIDs = new List<int>();
            DailyCost = new Dictionary<ResourceType, int>();
        }

        // ========== HELPER METHODS ==========

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

        // ========== READ-ONLY CHECKS ==========

        /// <summary>
        /// Is this unit at critical health?
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
    }
}
