using System.Collections.Generic;
using UnityEngine;

using TWK.Cultures;
using TWK.Realms;
using TWK.Simulation;
using TWK.Core;

namespace TWK.Agents
{
    

    public class Agent : MonoBehaviour, ISimulationAgent
    {
        public Realm HomeRealm;

        public string AgentName;
        public int Age;
        public int BirthDay;
        public int BirthYear;
        public int BirthMonth;

        public Dictionary<TreeType, float> SkillLevels = new Dictionary<TreeType, float>();
        public TreeType SkillFocus;

        public float DailySkillGain = 1f;
        public float DailyFocusBonus = 0.5f;

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            worldTimeManager.OnDayTick += AdvanceDay;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;

            foreach (TreeType tree in System.Enum.GetValues(typeof(TreeType)))
            {
                if (!SkillLevels.ContainsKey(tree))
                    SkillLevels.Add(tree, 0f);
            }
        }

        public void GainSkill(TreeType tree, float amount)
        {
            SkillLevels[tree] += amount;
            //Debug.Log($"{AgentName} gained {amount} skill in {tree}. New level: {SkillLevels[tree]}");
        }

        public void AdvanceDay()
        {
            //Debug.Log($"{AgentName} received AdvanceDay tick");
            OnDailySkillGain();

        }

        public void AdvanceSeason()
        {

        }

        public void AdvanceYear()
        {

        }

        private void OnDailySkillGain()
        {
            var trees = new List<TreeType>(SkillLevels.Keys); // snapshot of keys

            foreach (var tree in trees)
            {
                float gain = DailySkillGain;
                if (tree == SkillFocus)
                    gain += DailyFocusBonus;
                else
                    gain -= DailyFocusBonus * 0.5f; 

                GainSkill(tree, gain);
                //Debug.Log($"{AgentName} Dispatched daily skill gain for {tree}");
            }
        }
    }
}
