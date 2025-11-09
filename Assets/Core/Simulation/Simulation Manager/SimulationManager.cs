using UnityEngine;

using TWK.Agents;
using TWK.Realms;
using TWK.Realms.Demographics;
using TWK.Simulation;
using TWK.Economy;


namespace TWK.Core
{

    public class SimulationManager : MonoBehaviour
    {
        [SerializeField] private WorldTimeManager worldTimeManager;
        [SerializeField] private GameObject realmPrefab;
        [SerializeField] private GameObject agentPrefab;

        //For testing purposes only, should be reworked later
        [SerializeField] private Realm[] testRealms;
        [SerializeField] private Agent[] testAgents;
        [SerializeField] private City[] testCities;
        public FarmBuildingData testFarmData;


        private void Start()
        {
            if (worldTimeManager == null)
                worldTimeManager = WorldTimeManager.Instance;

            if (PopulationManager.Instance != null)
                PopulationManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("PopulationManager not found in scene!");

            if (ResourceManager.Instance != null)
                ResourceManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("ResourceManager not found in scene!");

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.Initialize(worldTimeManager);
            else
                Debug.LogError("BuildingManager not found in scene!");

            TestStart();
            

            worldTimeManager.OnDayTick += AdvanceDayDebug; //Temporary debug hook to show population stats

        }

        #region Test Setup
        public void AdvanceDayDebug() //Test Method
        {
            foreach (var city in testCities)
            {
                int totalPop = city.GetTotalPopulation();
                Debug.Log($"City: {city.Name} | Total Population: {totalPop}");

                var breakdown = city.GetPopulationBreakdown();

                string breakdownString = $"City: {city.Name} | Population Breakdown: ";    
                foreach (var kvp in breakdown)
                {
                    breakdownString += $"{kvp.Key}: {kvp.Value}, ";
                    
                }
                Debug.Log(breakdownString);
            }
        }
        
        
        private void TestStart()
        {
            

            if (testRealms.Length > 0)
            {
                foreach (var realm in testRealms)
                {
                    realm.Initialize(worldTimeManager);
                }
            }

            if (testCities.Length > 0)
            {
                foreach (var city in testCities)
                {
                    city.Initialize(worldTimeManager);
                    city.BuildFarm(testFarmData, Vector3.zero);

                    // Register a few population groups per city
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Laborer, 100);
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Artisan, 10);
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Noble, 30);
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Merchant, 5);
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Clergy, 10);
                    PopulationManager.Instance.RegisterPopulation(city.CityID, PopulationArchetypes.Slave, 15);
                }
            }


            if (testAgents.Length > 0)
            {
                foreach (var agent in testAgents)
                {
                    agent.Initialize(worldTimeManager);
                }
            }
            
        }
        #endregion
    }
}
