using System;
using System.Collections.Generic;
using UnityEngine;
using TWK.Economy;

namespace TWK.Realms
{
    /// <summary>
    /// Pure data class for a city. Contains no logic or MonoBehaviour.
    /// This is the "Model" in MVVM - just serializable state.
    /// </summary>
    [System.Serializable]
    public class CityData
    {
        // ========== IDENTITY ==========
        public string Name;
        public int CityID;
        public int OwnerRealmID;    


        // ========== OWNERSHIP ==========
        public int OwnerRealmID = -1;

        // ========== GEOGRAPHIC ==========
        public float LocationX, LocationY, LocationZ;
        public float TerritoryRadius;

        // Location property for convenient Vector3 access
        public Vector3 Location
        {
            get => new Vector3(LocationX, LocationY, LocationZ);
            set { LocationX = value.x; LocationY = value.y; LocationZ = value.z; }
        }

        // ========== GROWTH & DEVELOPMENT ==========
        public float GrowthRate;

        // ========== BUILDINGS ==========
        public List<int> BuildingIDs = new List<int>();

        // ========== ECONOMY SNAPSHOT ==========
        public CityEconomySnapshot EconomySnapshot;

        // ========== CONSTRUCTOR ==========
        public CityData()
        {
            //OwnerRealmID = iOwnerRealmId;

            // Default constructor for serialization
            EconomySnapshot = new CityEconomySnapshot
            {
                Production = new Dictionary<ResourceType, int>(),
                Consumption = new Dictionary<ResourceType, int>(),
                Net = new Dictionary<ResourceType, int>()
            };
        }

        public CityData(int cityId, string name, Vector3 location, int ownerRealmID, float growthRate = 1f,  float territoryRadius = 10f)
        {
            this.CityID = cityId;
            this.Name = name;
            this.Location = location;
            this.GrowthRate = growthRate;
            this.TerritoryRadius = territoryRadius;
            this.OwnerRealmID = ownerRealmID;   

            this.BuildingIDs = new List<int>();
            this.EconomySnapshot = new CityEconomySnapshot
            {
                Production = new Dictionary<ResourceType, int>(),
                Consumption = new Dictionary<ResourceType, int>(),
                Net = new Dictionary<ResourceType, int>()
            };
        }
    }

    /// <summary>
    /// Snapshot of city economy for a given time period.
    /// </summary>
    [System.Serializable]
    public struct CityEconomySnapshot
    {
        public Dictionary<ResourceType, int> Production;
        public Dictionary<ResourceType, int> Consumption;
        public Dictionary<ResourceType, int> Net;
    }
}
