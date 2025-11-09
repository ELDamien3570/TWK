using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

using TWK.Realms;

namespace TWK.Cultures
{
    
    public class Culture : MonoBehaviour
    {
        public string Name;
        public Dictionary<string, Innovation> CulturalInnovations;
        
        public void Unlock(Realm realm, int nodeID)
        {

        }
    }
}
