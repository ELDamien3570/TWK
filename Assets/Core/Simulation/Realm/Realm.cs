using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using TWK.Cultures;
using TWK.Agents;
using TWK.Core;
using TWK.Simulation;

namespace TWK.Realms
{
    

    public class Realm : MonoBehaviour, ISimulationAgent
    {
        public string RealmName;
        public List<City> Cities;
        public List<Agent> Leaders;
        public Culture RealmCulture;

        public void Initialize(WorldTimeManager worldTimeManager)
        {
            worldTimeManager.OnDayTick += AdvanceDay;
            worldTimeManager.OnSeasonTick += AdvanceSeason;
            worldTimeManager.OnYearTick += AdvanceYear;
        }

        public void AdvanceDay()
        {

        }

        public void AdvanceSeason()
        {

        }

        public void AdvanceYear()
        {

        }
    }
}