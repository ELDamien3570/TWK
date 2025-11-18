using System;
using UnityEngine;

namespace TWK.Agents
{
    /// <summary>
    /// Types of relationships between agents.
    /// Some are one-way (parent->child), others bidirectional (friends, spouses).
    /// </summary>
    public enum RelationshipType
    {
        // Family (one-way directional)
        Parent,      // One-way: parent has this toward child
        Child,       // One-way: child has this toward parent

        // Romantic (bidirectional)
        Spouse,      // Marriage
        Lover,       // Extramarital relationship

        // Social (bidirectional)
        Friend,      // Positive relationship
        Rival,       // Negative/competitive relationship

        // Feudal/Political (one-way, often contract-based)
        Liege,       // Subject -> Overlord (vassal has this toward their lord)
        Vassal,      // Overlord -> Subject (lord has this toward their vassal)

        // Service (one-way)
        Companion,   // Leader -> Follower (personal officer/retainer)
        Employer,    // Employer -> Employee
        Employee     // Employee -> Employer
    }

    /// <summary>
    /// Pure data structure for relationships between two agents.
    /// Centrally managed by RelationshipManager to avoid duplication.
    /// </summary>
    [System.Serializable]
    public class RelationshipData
    {
        // ========== IDENTITY ==========
        public int RelationshipID;
        public RelationshipType Type;

        // ========== PARTIES ==========
        /// <summary>
        /// Primary agent ID.
        /// For one-way relationships: the "owner" of the relationship
        /// For bidirectional: the initiator or just one party
        /// </summary>
        public int Agent1ID;

        /// <summary>
        /// Secondary agent ID.
        /// For one-way relationships: the "target" of the relationship
        /// For bidirectional: the other party
        /// </summary>
        public int Agent2ID;

        // ========== STRENGTH ==========
        /// <summary>
        /// Relationship strength from -100 (hatred) to +100 (love/loyalty).
        /// Used for: friendship quality, vassal loyalty, rivalry intensity, etc.
        /// </summary>
        public float Strength = 0f;

        // ========== OPTIONAL CONTRACT LINK ==========
        /// <summary>
        /// Contract ID if this is a feudal relationship (Liege/Vassal).
        /// -1 if no contract.
        /// </summary>
        public int ContractID = -1;

        // ========== METADATA ==========
        /// <summary>
        /// Day when this relationship was formed.
        /// </summary>
        public int FormedDay = 0;

        /// <summary>
        /// Year when this relationship was formed.
        /// </summary>
        public int FormedYear = 0;

        /// <summary>
        /// Is this relationship currently active?
        /// Set to false when relationship ends (divorce, death, contract termination).
        /// </summary>
        public bool IsActive = true;

        // ========== CONSTRUCTORS ==========

        public RelationshipData()
        {
            // Default constructor for serialization
        }

        public RelationshipData(int id, RelationshipType type, int agent1, int agent2, float strength = 0f)
        {
            RelationshipID = id;
            Type = type;
            Agent1ID = agent1;
            Agent2ID = agent2;
            Strength = strength;
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get the other agent in this relationship.
        /// </summary>
        public int GetOtherAgent(int thisAgentID)
        {
            if (thisAgentID == Agent1ID)
                return Agent2ID;
            else if (thisAgentID == Agent2ID)
                return Agent1ID;
            else
                return -1; // Agent not in this relationship
        }

        /// <summary>
        /// Is this a bidirectional relationship type?
        /// </summary>
        public bool IsBidirectional()
        {
            return Type == RelationshipType.Spouse ||
                   Type == RelationshipType.Lover ||
                   Type == RelationshipType.Friend ||
                   Type == RelationshipType.Rival;
        }

        /// <summary>
        /// Get a human-readable label for the strength.
        /// </summary>
        public string GetStrengthLabel()
        {
            if (Type == RelationshipType.Friend || Type == RelationshipType.Spouse || Type == RelationshipType.Lover)
            {
                // Positive relationships
                if (Strength >= 80) return "Devoted";
                if (Strength >= 60) return "Close";
                if (Strength >= 40) return "Good";
                if (Strength >= 20) return "Friendly";
                if (Strength >= 0) return "Cordial";
                if (Strength >= -20) return "Strained";
                if (Strength >= -40) return "Troubled";
                if (Strength >= -60) return "Failing";
                return "Broken";
            }
            else if (Type == RelationshipType.Rival)
            {
                // Rivalry intensity
                if (Strength <= -80) return "Mortal Enemies";
                if (Strength <= -60) return "Bitter Rivals";
                if (Strength <= -40) return "Strong Rivalry";
                if (Strength <= -20) return "Rivals";
                if (Strength <= 0) return "Mild Rivalry";
                return "Fading Rivalry";
            }
            else if (Type == RelationshipType.Liege || Type == RelationshipType.Vassal)
            {
                // Loyalty/fealty
                if (Strength >= 80) return "Loyal";
                if (Strength >= 60) return "Dutiful";
                if (Strength >= 40) return "Compliant";
                if (Strength >= 20) return "Neutral";
                if (Strength >= 0) return "Discontent";
                if (Strength >= -20) return "Disloyal";
                if (Strength >= -40) return "Rebellious";
                return "Treacherous";
            }
            else
            {
                // Generic
                if (Strength >= 50) return "Strong";
                if (Strength >= 0) return "Neutral";
                return "Weak";
            }
        }
    }
}
