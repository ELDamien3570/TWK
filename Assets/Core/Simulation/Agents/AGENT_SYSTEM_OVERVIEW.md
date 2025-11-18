# Agent System Overview

## Introduction
The Agent system has been comprehensively fleshed out to support complex character simulation including relationships, combat, personality, and reputation. All data structures are in place for future integration.

## File Structure
- **AgentEnums.cs** - Contains all enums (Gender, Sexuality, PersonalityTrait)
- **AgentData.cs** - Pure data model with all agent properties and helper methods
- **Agent.cs** - MonoBehaviour that wraps AgentData and handles Unity-specific logic
- **AgentLedger.cs** - Manages personal wealth and resources
- **AgentManager.cs** - Central manager for all agents

## Core Systems

### 1. Demographics
- **Gender**: Male, Female, Other
- **Sexuality**: Heterosexual, Homosexual, Bisexual
- **Age**: Tracked with birth day/month/year
- **Natural Death**: Age-based death chance (1% per year after 60)

### 2. Relationships
All relationships stored as agent ID references for future integration:

- **Parents**: MotherID, FatherID (-1 if unknown)
- **Children**: List of ChildrenIDs
- **Spouses**: List of SpouseIDs (supports polygamy)
- **Lovers**: List of LoverIDs (extramarital relationships)
- **Friends**: List of FriendIDs
- **Rivals**: List of RivalIDs
- **Companions**: List of CompanionIDs (personal officers)

**Helper Methods**:
- `AddChild(int)`, `RemoveChild(int)`
- `AddSpouse(int)`, `RemoveSpouse(int)`
- `AddLover(int)`, `RemoveLover(int)`
- `AddFriend(int)`, `RemoveFriend(int)`
- `AddRival(int)`, `RemoveRival(int)`
- `AddCompanion(int)`, `RemoveCompanion(int)`

### 3. Personality & Reputation

#### Personality Traits
Traits are enum-based and can be unlocked through skill trees or assigned at creation. Categories include:
- **Combat**: Brave, Cowardly, Aggressive, Defensive, Tactician, Berserker
- **Social**: Charismatic, Shy, Diplomatic, Blunt, Manipulative, Honest
- **Economic**: Greedy, Generous, Frugal, Wasteful, Merchant
- **Leadership**: Just, Cruel, Ambitious, Content, Inspiring, Tyrannical
- **Personal**: Lustful, Chaste, Gluttonous, Temperate, Wrathful, Calm, Patient, Impatient, Humble, Proud
- **Intellectual**: Scholarly, Ignorant, Curious, Stubborn, Wise, Foolish

**Helper Methods**:
- `AddTrait(PersonalityTrait)`, `RemoveTrait(PersonalityTrait)`
- `HasTrait(PersonalityTrait)`

#### Reputation System
- **Prestige**: Earned through accomplishments, titles, victories
- **Morality**: Decreases from breaking contracts, exposed schemes, illegitimate children
- **Reputation**: Aggregate score calculated from prestige, morality, skills, and traits

**Helper Methods**:
- `ModifyPrestige(float)`
- `ModifyMorality(float)`
- `CalculateReputation()` - Auto-calculates based on all factors

### 4. Skill Trees
Based on `TreeType` enum (Warfare, Politics, Economics, Science, Religion):
- **SkillLevels**: Dictionary tracking progress in each tree
- **SkillFocus**: Current focus tree gets bonus XP
- **DailySkillGain**: Base skill growth per day
- **DailyFocusBonus**: Extra gain for focused skill

### 5. Combat/Unit Stats

#### Basic Attributes
- **Strength** (50 base): Affects charge resistance, equipment bonuses, weapon slots
- **Leadership** (50 base): Command responsiveness, formation cohesion, routing threshold
- **Morale** (100 base): Combat effectiveness, routing check vs leadership
- **WeaponSlots** (3 base): Increases by 1 per 25 strength above 50

#### Combat Stats
- **Health/MaxHealth** (100 base)
- **Speed** (5 base): Walking speed
- **Agility** (10 base): Dodge chance for ranged/charges
- **Accuracy** (50 base): Hit chance, interacts with target's agility
- **ChargeSpeed** (10 base): Running/charging speed
- **ChargeBonus** (5 base): Extra damage from charges

#### Melee Stats
- **MeleeAttack** (10 base): Damage per hit (strength * equipment)
- **MeleeArmor** (5 base): Damage reduction (strength * equipment)
- **MeleeAttackBonusVsMount**: Extra damage vs cavalry

#### Missile Stats
- **MissileAttack** (0 base): Ranged damage per shot
- **MissileDefense** (0 base): Ranged damage reduction
- **MissileAttackBonusVsMount**: Extra damage vs cavalry
- **Ammunition/CurrentAmmunition**: Shot capacity

#### Mount
- **MountID** (-1 if unmounted): References mount object
- Mount affects: speed, charge speed, charge bonus, health

#### Daily Cost
- **DailyCost**: Dictionary of resources consumed per day

**Key Mechanics**:
- Morale < Leadership → Unit routes
- Morale < 35% → Combat penalties
- Critical health (<25%) → 50% death chance in combat

**Helper Methods**:
- `TakeDamage(float)`, `Heal(float)`
- `IsCriticalHealth()`
- `ModifyMorale(float)`
- `ShouldRoute()`, `HasLowMoralePenalty()`
- `RecalculateWeaponSlots()`
- `KillInCombat()`

### 6. Equipment System
Equipment stored as IDs for future item system integration:

- **EquippedWeaponIDs**: List (max = WeaponSlots)
- **HeadEquipmentID**: Helmet (-1 if none)
- **BodyEquipmentID**: Armor (-1 if none)
- **LegsEquipmentID**: Leg armor (-1 if none)
- **ShieldID**: Shield (-1 if none)
- **MountID**: Mount/horse (-1 if none)

**Helper Methods**:
- `EquipWeapon(int)` - Returns false if slots full
- `UnequipWeapon(int)`

### 7. Properties & Influence

- **OwnedBuildingIDs**: Estates, businesses (economic hubs)
- **OwnedCaravanIDs**: Trade caravans
- **ControlledCityIDs**: Cities personally controlled
- **HomeRealmID**: Home realm affiliation
- **OfficeRealmID**: Realm where they hold office

**Helper Methods**:
- `AddOwnedBuilding(int)`, `RemoveOwnedBuilding(int)`
- `AddOwnedCaravan(int)`, `RemoveOwnedCaravan(int)`
- `AddControlledCity(int)`, `RemoveControlledCity(int)`

### 8. Death & Inheritance

**Death Triggers**:
- Natural death: Age-based probability (1% per year after 60)
- Combat death: 50% chance when reaching critical health (<25%)

**Inheritance System**:
- Wealth distributed equally among children
- Properties (estates, caravans, cities) ready for transfer (TODO)
- Relationship updates (widows, orphans) ready (TODO)

**Methods**:
- `HandleDeath()` - Main death handler
- `KillInCombat()` - Combat-specific death check

### 9. Personal Ledger
Managed by `AgentLedger` class (separate file):
- Personal wealth tracking (separate from realm treasury)
- Salary from offices
- Property income
- Gifts and bribes
- Inheritance distribution

## Usage Examples

### Creating an Agent with Full Details
```csharp
AgentData agent = new AgentData(1, "Marcus Aurelius", realmID: 5, cultureID: 2);
agent.Gender = Gender.Male;
agent.Sexuality = Sexuality.Heterosexual;
agent.Age = 35;

// Set parents
agent.MotherID = 45;
agent.FatherID = 46;

// Add traits
agent.AddTrait(PersonalityTrait.Just);
agent.AddTrait(PersonalityTrait.Scholarly);
agent.AddTrait(PersonalityTrait.Brave);

// Set combat stats
agent.Strength = 75f;
agent.Leadership = 80f;
agent.Morale = 100f;
agent.RecalculateWeaponSlots(); // Now has 4 slots (75 STR)

// Set reputation
agent.Prestige = 50f;
agent.Morality = 90f;
agent.CalculateReputation();

// Add relationships
agent.AddSpouse(10);
agent.AddChild(11);
agent.AddChild(12);
agent.AddFriend(20);
agent.AddRival(30);
```

### Combat Example
```csharp
// During battle
agent.TakeDamage(75f); // Takes damage

if (agent.IsCriticalHealth())
{
    // 50% chance of death
    KillInCombat();
}

// Check morale
if (agent.ShouldRoute())
{
    // Unit retreats
}

if (agent.HasLowMoralePenalty())
{
    // Apply combat penalties
}
```

### Managing Equipment
```csharp
// Equip weapons
bool equipped = agent.EquipWeapon(swordID);
equipped = agent.EquipWeapon(spearID);
equipped = agent.EquipWeapon(bowID);

// Equip armor
agent.HeadEquipmentID = helmetID;
agent.BodyEquipmentID = breastplateID;
agent.LegsEquipmentID = greavesID;
agent.ShieldID = shieldID;

// Mount a horse
agent.MountID = horseID;
```

### Relationship Building
```csharp
// Marriage
agent1.AddSpouse(agent2.AgentID);
agent2.AddSpouse(agent1.AgentID);

// Child birth
int childID = CreateNewAgent();
agent1.AddChild(childID);
agent2.AddChild(childID);
childData.MotherID = agent1.AgentID;
childData.FatherID = agent2.AgentID;

// Friendship/Rivalry
agent1.AddFriend(agent3.AgentID);
agent1.AddRival(agent4.AgentID);
```

### Reputation Management
```csharp
// Gain prestige from victory
agent.ModifyPrestige(10f);

// Lose morality from scandal
agent.ModifyMorality(-20f); // Illegitimate child exposed

// Recalculate reputation
float rep = agent.CalculateReputation();
```

## Integration Status

### ✅ Completed (Data Structures Ready)
- All demographic fields
- Relationship tracking infrastructure
- Personality traits system
- Reputation calculations
- Combat stats framework
- Equipment slots
- Property ownership lists
- Death and inheritance foundations

### 🔄 Pending Integration
- Actual weapon/armor item definitions
- Mount definitions and stats application
- Equipment stat calculations (strength * equipment)
- Relationship simulation logic (friendship/rivalry changes)
- Dynasty/succession mechanics
- Combat system hookup
- Property income generation
- Trait effects on gameplay

## Debug Tools
Use the context menu in Unity Inspector:
- Right-click on Agent component → "Print Agent Info"
- Shows comprehensive agent details including all new systems

## Notes
- All ID-based references (-1 means "none" or "unknown")
- Lists automatically initialized in constructors
- Helper methods prevent duplicates in lists
- Data-driven design: AgentData is pure data, Agent.cs handles Unity logic
- Performance: Combat stats only need recalculation on equipment change or battle start
