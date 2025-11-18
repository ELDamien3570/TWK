using UnityEngine;
using TWK.Cultures;

namespace TWK.Agents
{
    /// <summary>
    /// Pure static functions for agent simulation logic.
    /// Operates on AgentData without side effects (besides the data itself).
    /// This is the "Logic" layer - pure functions that transform data.
    /// Follows the same pattern as CitySimulation.
    /// </summary>
    public static class AgentSimulation
    {
        // ========== MAIN SIMULATION ==========

        /// <summary>
        /// Simulate one day for an agent.
        /// </summary>
        public static void SimulateDay(AgentData agent, AgentLedger ledger)
        {
            if (!agent.IsAlive)
                return;

            // Daily skill gain
            SimulateDailySkillGain(agent);

            // Daily resource consumption (food, supplies)
            SimulateDailyCosts(agent, ledger);
        }

        /// <summary>
        /// Simulate one season for an agent.
        /// </summary>
        public static void SimulateSeason(AgentData agent, AgentLedger ledger)
        {
            if (!agent.IsAlive)
                return;

            // Collect property income
            if (ledger != null)
            {
                ledger.CollectPropertyIncome();
            }
        }

        /// <summary>
        /// Simulate one year for an agent.
        /// </summary>
        public static void SimulateYear(AgentData agent)
        {
            if (!agent.IsAlive)
                return;

            // Age the agent
            agent.Age++;

            // Check for natural death
            CheckNaturalDeath(agent);
        }

        // ========== SKILL SYSTEM ==========

        /// <summary>
        /// Process daily skill gain for an agent.
        /// Focused skill gets bonus, unfocused skills get penalty.
        /// </summary>
        private static void SimulateDailySkillGain(AgentData agent)
        {
            foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
            {
                if (!agent.SkillLevels.ContainsKey(tree))
                    continue;

                float gain = agent.DailySkillGain;

                if (tree == agent.SkillFocus)
                    gain += agent.DailyFocusBonus;
                else
                    gain -= agent.DailyFocusBonus * 0.5f;

                agent.SkillLevels[tree] += gain;
            }
        }

        // ========== REPUTATION SYSTEM ==========

        /// <summary>
        /// Calculate reputation based on stats, traits, prestige, and morality.
        /// </summary>
        public static float CalculateReputation(AgentData agent)
        {
            // Base reputation from prestige and morality
            float rep = agent.Prestige + (agent.Morality * 0.5f);

            // Add skill bonuses
            foreach (var skill in agent.SkillLevels.Values)
            {
                rep += skill * 0.1f;
            }

            // Trait modifiers
            if (agent.HasTrait(PersonalityTrait.Charismatic)) rep += 10f;
            if (agent.HasTrait(PersonalityTrait.Honest)) rep += 5f;
            if (agent.HasTrait(PersonalityTrait.Diplomatic)) rep += 8f;
            if (agent.HasTrait(PersonalityTrait.Just)) rep += 7f;
            if (agent.HasTrait(PersonalityTrait.Inspiring)) rep += 12f;

            if (agent.HasTrait(PersonalityTrait.Cruel)) rep -= 10f;
            if (agent.HasTrait(PersonalityTrait.Tyrannical)) rep -= 15f;
            if (agent.HasTrait(PersonalityTrait.Cowardly)) rep -= 8f;
            if (agent.HasTrait(PersonalityTrait.Foolish)) rep -= 5f;

            agent.Reputation = rep;
            return rep;
        }

        /// <summary>
        /// Modify prestige by a given amount.
        /// </summary>
        public static void ModifyPrestige(AgentData agent, float amount)
        {
            agent.Prestige += amount;
            if (agent.Prestige < 0) agent.Prestige = 0;
        }

        /// <summary>
        /// Modify morality by a given amount.
        /// </summary>
        public static void ModifyMorality(AgentData agent, float amount)
        {
            agent.Morality += amount;
            if (agent.Morality < 0) agent.Morality = 0;
            if (agent.Morality > 100) agent.Morality = 100;
        }

        // ========== COMBAT SYSTEM ==========

        /// <summary>
        /// Apply damage to an agent.
        /// </summary>
        public static void ApplyDamage(AgentData agent, float damage)
        {
            agent.Health -= damage;
            if (agent.Health < 0) agent.Health = 0;
        }

        /// <summary>
        /// Heal an agent.
        /// </summary>
        public static void Heal(AgentData agent, float amount)
        {
            agent.Health += amount;
            if (agent.Health > agent.MaxHealth) agent.Health = agent.MaxHealth;
        }

        /// <summary>
        /// Modify morale by a given amount.
        /// </summary>
        public static void ModifyMorale(AgentData agent, float amount)
        {
            agent.Morale += amount;
            if (agent.Morale < 0) agent.Morale = 0;
            if (agent.Morale > 100) agent.Morale = 100;
        }

        /// <summary>
        /// Check for combat death when agent is at critical health.
        /// Returns true if agent should die.
        /// </summary>
        public static bool CheckCombatDeath(AgentData agent)
        {
            if (!agent.IsCriticalHealth())
                return false;

            float deathChance = 0.5f; // 50% chance at critical health
            return Random.value < deathChance;
        }

        // ========== EQUIPMENT SYSTEM ==========

        /// <summary>
        /// Recalculate weapon slots based on strength.
        /// Every 25 strength past 50 adds one slot.
        /// </summary>
        public static void RecalculateWeaponSlots(AgentData agent)
        {
            agent.WeaponSlots = 3; // Base slots

            if (agent.Strength > 50f)
            {
                int bonusSlots = (int)((agent.Strength - 50f) / 25f);
                agent.WeaponSlots += bonusSlots;
            }
        }

        /// <summary>
        /// Recalculate combat stats based on equipment and strength.
        /// Called when equipment changes.
        /// </summary>
        public static void RecalculateCombatStats(AgentData agent)
        {
            // TODO: Implement when equipment system is in place
            // For now, just ensure weapon slots are correct
            RecalculateWeaponSlots(agent);

            // Equipment should modify:
            // - MeleeAttack = baseAttack * (strength/50) * equipmentMods
            // - MeleeArmor = baseArmor * (strength/50) * equipmentMods
            // - Speed, ChargeSpeed, ChargeBonus from mount
            // - MissileAttack, MissileDefense from ranged equipment
        }

        // ========== DEATH SYSTEM ==========

        /// <summary>
        /// Check for natural death based on age.
        /// Returns true if agent should die.
        /// </summary>
        public static bool CheckNaturalDeath(AgentData agent)
        {
            if (agent.Age <= 60)
                return false;

            float deathChance = (agent.Age - 60) * 0.01f; // 1% per year over 60

            if (Random.value < deathChance)
            {
                agent.IsAlive = false;
                return true;
            }

            return false;
        }

        // ========== RESOURCE CONSUMPTION ==========

        /// <summary>
        /// Process daily resource costs for an agent (food, supplies, etc).
        /// </summary>
        private static void SimulateDailyCosts(AgentData agent, AgentLedger ledger)
        {
            if (ledger == null || agent.DailyCost == null || agent.DailyCost.Count == 0)
                return;

            foreach (var cost in agent.DailyCost)
            {
                // Try to pay cost from personal wealth
                if (!ledger.SpendResource(cost.Key, cost.Value))
                {
                    // Cannot afford daily costs - reduce morale
                    ModifyMorale(agent, -5f);
                }
            }
        }

        // ========== RELATIONSHIP IMPACT ==========

        /// <summary>
        /// Calculate opinion modifier between two agents based on traits.
        /// Returns a value from -100 to +100.
        /// </summary>
        public static float CalculateOpinionModifier(AgentData agent1, AgentData agent2)
        {
            float opinion = 0f;

            // Trait compatibility
            if (agent1.HasTrait(PersonalityTrait.Honest) && agent2.HasTrait(PersonalityTrait.Honest))
                opinion += 10f;

            if (agent1.HasTrait(PersonalityTrait.Honest) && agent2.HasTrait(PersonalityTrait.Manipulative))
                opinion -= 20f;

            if (agent1.HasTrait(PersonalityTrait.Just) && agent2.HasTrait(PersonalityTrait.Cruel))
                opinion -= 25f;

            if (agent1.HasTrait(PersonalityTrait.Ambitious) && agent2.HasTrait(PersonalityTrait.Ambitious))
                opinion -= 15f; // Rivalry

            // Morality difference
            float moralityDiff = Mathf.Abs(agent1.Morality - agent2.Morality);
            if (moralityDiff > 30f)
                opinion -= 10f;

            // Reputation effect
            if (agent2.Reputation > 50f)
                opinion += 5f;
            else if (agent2.Reputation < -50f)
                opinion -= 5f;

            return Mathf.Clamp(opinion, -100f, 100f);
        }

        // ========== TRAIT EFFECTS ==========

        /// <summary>
        /// Get combat modifier from personality traits.
        /// Used to modify attack/defense in battle.
        /// </summary>
        public static float GetCombatModifierFromTraits(AgentData agent)
        {
            float modifier = 1f;

            if (agent.HasTrait(PersonalityTrait.Brave)) modifier *= 1.1f;
            if (agent.HasTrait(PersonalityTrait.Cowardly)) modifier *= 0.9f;
            if (agent.HasTrait(PersonalityTrait.Aggressive)) modifier *= 1.15f;
            if (agent.HasTrait(PersonalityTrait.Berserker)) modifier *= 1.2f;
            if (agent.HasTrait(PersonalityTrait.Tactician)) modifier *= 1.1f;

            return modifier;
        }

        /// <summary>
        /// Get economic modifier from personality traits.
        /// Used for income/trade calculations.
        /// </summary>
        public static float GetEconomicModifierFromTraits(AgentData agent)
        {
            float modifier = 1f;

            if (agent.HasTrait(PersonalityTrait.Greedy)) modifier *= 1.1f;
            if (agent.HasTrait(PersonalityTrait.Generous)) modifier *= 0.95f;
            if (agent.HasTrait(PersonalityTrait.Frugal)) modifier *= 1.05f;
            if (agent.HasTrait(PersonalityTrait.Wasteful)) modifier *= 0.9f;
            if (agent.HasTrait(PersonalityTrait.Merchant)) modifier *= 1.15f;

            return modifier;
        }

        /// <summary>
        /// Get diplomatic modifier from personality traits.
        /// Used for relationship changes and negotiations.
        /// </summary>
        public static float GetDiplomaticModifierFromTraits(AgentData agent)
        {
            float modifier = 1f;

            if (agent.HasTrait(PersonalityTrait.Charismatic)) modifier *= 1.2f;
            if (agent.HasTrait(PersonalityTrait.Shy)) modifier *= 0.8f;
            if (agent.HasTrait(PersonalityTrait.Diplomatic)) modifier *= 1.15f;
            if (agent.HasTrait(PersonalityTrait.Blunt)) modifier *= 0.9f;

            return modifier;
        }
    }
}
