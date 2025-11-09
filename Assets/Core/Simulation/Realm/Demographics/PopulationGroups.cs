using UnityEngine;


namespace TWK.Realms.Demographics
{
   
    public struct PopulationGroups
    {
        public int id;
        public int ownerCityId;
        public PopulationArchetypes archetype;

        public int populationCount;
        public float populationHappiness;
        public float growthModifier;
        public float wealthModifier;

        // Add food cost, and education level

        public PopulationGroups(int inId, int inOwnerCityId, PopulationArchetypes inArchetype, int inPopulationCount)
        {
            this.id = inId;
            this.ownerCityId = inOwnerCityId;
            this.archetype = inArchetype;
            this.populationCount = inPopulationCount;
            this.populationHappiness = 1f;
            this.growthModifier = 1f;
            this.wealthModifier = 1f;
        }
    }
}
