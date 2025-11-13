# Building System Refactor - MVVM Architecture

## Overview

The building system has been refactored following MVVM architecture with full support for:
- **Hub/Hublet System:** Hubs allow expansion, hublets must attach to hubs
- **Worker Requirements:** Buildings require specific population archetypes
- **Worker Efficiency:** Different worker types have different efficiency/penalties
- **Scalable Production:** Production scales with worker count (min → optimal)
- **Population Effects:** Buildings increase education/wealth of workers
- **Resource Production:** Goods, prestige, and other resources
- **Growth Bonuses:** Eco hubs can boost population growth

---

## Architecture

```
BuildingDefinition (ScriptableObject config)
    ↓
BuildingInstanceData (pure data)
    ↓
BuildingSimulation (pure logic)
    ↓
BuildingManager (MonoBehaviour coordinator)
    ↓
BuildingViewModel (UI binding)

Legacy Support:
BuildingInstance (struct wrapper - for backward compatibility)
```

---

## Core Components

### **1. BuildingDefinition.cs** (ScriptableObject)

Configuration/template for a building type. Defines what a building CAN do.

#### Hub System:
```csharp
public bool IsHub;                     // Can hublets attach to this?
public bool IsHublet;                  // Must attach to a hub?
public List<TreeType> RequiredHubTypes; // Which hubs can this attach to?
```

**Hub Types (from TreeType):**
- Warfare
- Politics
- Religion
- Economics
- Science
- City (universal hub - implicit)

#### Worker System:
```csharp
public bool RequiresWorkers;           // Does this need workers?
public int MinWorkers;                 // Minimum to operate
public int OptimalWorkers;             // Maximum useful workers
public List<PopulationArchetypes> AllowedWorkerTypes;     // Who can work?
public Dictionary<PopulationArchetypes, int> RequiredWorkers_ByType;  // Must haves
public Dictionary<PopulationArchetypes, float> WorkerEfficiency;      // Multipliers
public Dictionary<PopulationArchetypes, float> WorkerPenalties;       // Penalties
```

**Worker Archetypes:**
- Slave
- Laborer
- Artisan
- Merchant
- Noble
- Clergy

#### Production System:
```csharp
public Dictionary<ResourceType, int> BaseProduction;   // Minimum output
public Dictionary<ResourceType, int> MaxProduction;    // Max with optimal workers
public Dictionary<ResourceType, int> BaseBuildCost;
public Dictionary<ResourceType, int> BaseMaintenanceCost;
```

**Production Formula:**
```
Production = Lerp(BaseProduction, MaxProduction, WorkerRatio) * AvgEfficiency * BaseEfficiency

Where:
- WorkerRatio = Clamp01(CurrentWorkers / OptimalWorkers)
- AvgEfficiency = Sum(WorkerCount[i] * WorkerEfficiency[i]) / TotalWorkers
- WorkerEfficiency accounts for both bonuses and penalties
```

#### Population Effects:
```csharp
public float EducationGrowthPerWorker;  // Education increase per worker per day
public float WealthGrowthPerWorker;     // Wealth increase per worker per day
public PopulationArchetypes? AffectsSpecificArchetype;  // Target specific class?
public float PopulationGrowthBonus;     // Growth bonus (eco hubs)
```

**Example - University:**
```csharp
EducationGrowthPerWorker = 0.1f;  // +0.1 education/day per worker
AllowedWorkerTypes = [Artisan, Clergy, Noble];
WorkerEfficiency = {
    Artisan: 0.8f,   // 80% effective
    Clergy: 1.2f,    // 120% effective (bonus!)
    Noble: 1.0f      // 100% effective
};
```

**Example - Market Village (Eco Hub):**
```csharp
RequiresWorkers = false;           // Passive building
PopulationGrowthBonus = 0.15f;     // +15% growth to city
IsHub = true;
BuildingCategory = TreeType.Economics;
```

**Example - Mine (Hublet):**
```csharp
IsHublet = true;
RequiredHubTypes = [TreeType.Economics, TreeType.Science];
RequiredWorkers_ByType = {
    Laborer: 5,      // Need at least 5 laborers
    Artisan: 2       // Need at least 2 artisans
};
WorkerPenalties = {
    Noble: 0.5f,     // Nobles are 50% less effective (penalty!)
    Clergy: 0.3f     // Clergy are 70% less effective
};
OptimalWorkers = 20;
```

---

### **2. BuildingInstanceData.cs** (Pure Data)

Serializable instance data for a specific building. Contains no logic.

```csharp
public class BuildingInstanceData
{
    // Identity
    public int ID;
    public int CityID;
    public int BuildingDefinitionID;

    // Location
    public Vector3 Position;

    // Hub System
    public int AttachedToHubID;              // If hublet, which hub?
    public List<int> AttachedHubletIDs;      // If hub, which hublets?

    // State
    public bool IsActive;
    public float EfficiencyMultiplier;
    public BuildingConstructionState ConstructionState;
    public int ConstructionDaysRemaining;

    // Workers
    public Dictionary<PopulationArchetypes, int> AssignedWorkers;
    public int TotalWorkers;
}
```

---

### **3. BuildingSimulation.cs** (Pure Logic)

Static functions for building simulation.

**Key Methods:**

#### Simulation:
```csharp
SimulateDay(instance, definition, ledger, cityId)
CalculateProduction(instance, definition)
ApplyPopulationEffects(instance, definition, cityId)
```

#### Construction:
```csharp
AdvanceConstruction(instance)
CompleteConstruction(instance)
```

#### Worker Management:
```csharp
CanAcceptWorkers(instance, definition, additionalWorkers)
GetProductionEfficiency(instance, definition)
GetWorkerDeficit(instance, definition)
```

#### Hub/Hublet Validation:
```csharp
CanHubletAttachToHub(hubletDef, hubDef)
IsAdjacentTo(position1, position2, maxDistance)
FindNearestHub(position, buildings, definitions)
```

#### Population Growth:
```csharp
CalculatePopulationGrowthBonus(cityBuildings, definitions)
```

---

### **4. BuildingViewModel.cs** (UI Layer)

View model for exposing building data to UI in a friendly format.

**Key Properties:**

```csharp
// Identity
public string BuildingName;
public int BuildingID;
public TreeType BuildingCategory;

// State
public bool IsActive;
public bool IsCompleted;
public bool IsUnderConstruction;
public float EfficiencyMultiplier;

// Construction
public int ConstructionDaysRemaining;
public float ConstructionProgress;  // 0-1

// Hub System
public bool IsHub;
public bool IsHublet;
public bool IsAttachedToHub;
public int AttachedHubletCount;

// Workers
public int TotalWorkers;
public int OptimalWorkers;
public float WorkerRatio;  // 0-1
public bool IsSufficientlyStaffed;
public bool IsOptimallyStaffed;
public Dictionary<PopulationArchetypes, int> AssignedWorkers;

// Production
public Dictionary<ResourceType, int> CurrentProduction;
public string ProductionSummary;

// Population Effects
public float EducationGrowthPerWorker;
public float WealthGrowthPerWorker;
public bool HasPopulationEffects;
```

**Helper Methods:**

```csharp
// Status summaries
string GetStatusSummary()          // "Optimal (10/10)" or "Understaffed (5/10)"
string GetWorkerBreakdown()        // "Artisan: 5 (120% eff) | Laborer: 3 (100% eff)"
string GetHubStatus()              // "Hub with 3 attached hublet(s)"
string GetConstructionStatus()     // "3 days remaining (40% complete) | Can cancel"
string GetProductionEfficiency()   // "Workers: 10/10 (100%) | Avg Efficiency: 120%"
string GetPopulationEffectsSummary()  // "+0.1 education/worker/day | +0.05 wealth/worker/day"

// Worker queries
bool CanAssignWorker(PopulationArchetypes archetype)
float GetWorkerEfficiency(PopulationArchetypes archetype)
int GetWorkerCount(PopulationArchetypes archetype)
List<PopulationArchetypes> GetAllowedWorkerTypes()
```

**Usage Example:**

```csharp
// Create ViewModel
var viewModel = new BuildingViewModel(buildingID);

// Bind to UI
nameLabel.text = viewModel.BuildingName;
statusLabel.text = viewModel.GetStatusSummary();
workerLabel.text = $"{viewModel.TotalWorkers}/{viewModel.OptimalWorkers}";
productionLabel.text = viewModel.ProductionSummary;

// Show worker breakdown
foreach (var kvp in viewModel.AssignedWorkers)
{
    Debug.Log($"{kvp.Key}: {kvp.Value} workers");
}

// Check if building needs attention
if (!viewModel.IsSufficientlyStaffed)
    warningIcon.SetActive(true);

// Refresh when data changes
viewModel.Refresh();
```

---

## Usage Examples

### **Creating a Building Definition (Inspector)**

**Warfare Hub - Barracks:**
```
BuildingName: "Barracks"
BuildingCategory: Warfare
IsHub: true
IsHublet: false
RequiresWorkers: true
MinWorkers: 3
OptimalWorkers: 15
AllowedWorkerTypes: [Laborer, Noble]
WorkerEfficiency:
  - Noble: 1.3  (Nobles are better at military organization)
  - Laborer: 0.9
BaseProduction:
  - Prestige: 5
MaxProduction:
  - Prestige: 20
PopulationGrowthBonus: 0 (no bonus)
```

**Economics Hublet - Artisan Workshop:**
```
BuildingName: "Artisan Workshop"
BuildingCategory: Economics
IsHub: false
IsHublet: true
RequiredHubTypes: [Economics, Science]
RequiresWorkers: true
MinWorkers: 2
OptimalWorkers: 10
AllowedWorkerTypes: [Artisan]
RequiredWorkers_ByType:
  - Artisan: 2
WorkerEfficiency:
  - Artisan: 1.2
EducationGrowthPerWorker: 0.05  (Artisans learn on the job)
BaseProduction:
  - Gold: 3
MaxProduction:
  - Gold: 15
```

**Religion Hub - Cathedral:**
```
BuildingName: "Cathedral"
BuildingCategory: Religion
IsHub: true
IsHublet: false
RequiresWorkers: true
MinWorkers: 1
OptimalWorkers: 8
AllowedWorkerTypes: [Clergy]
RequiredWorkers_ByType:
  - Clergy: 1
WorkerEfficiency:
  - Clergy: 1.5
EducationGrowthPerWorker: 0.08  (Religious education)
AffectsSpecificArchetype: Clergy
BaseProduction:
  - Prestige: 10
  - Faith: 5
MaxProduction:
  - Prestige: 30
  - Faith: 20
PopulationGrowthBonus: 0.05  (Religious buildings encourage families)
```

**Economics Hub - Market Village:**
```
BuildingName: "Market Village"
BuildingCategory: Economics
IsHub: true
IsHublet: false
RequiresWorkers: false  (Passive building!)
PopulationGrowthBonus: 0.20  (Markets attract people)
BaseProduction:
  - Gold: 10
MaxProduction:
  - Gold: 10  (Same as base - no workers needed)
```

---

### **Building Placement Rules**

**Hubs:**
- Can be placed anywhere (within city territory)
- Allow hublets to attach
- Extend city control area

**Hublets:**
- Must be adjacent to a compatible hub
- Check `RequiredHubTypes` matches hub's `BuildingCategory`
- If `RequiredHubTypes` is empty, can attach to any hub
- Use `BuildingSimulation.CanHubletAttachToHub()` to validate

**City:**
- Acts as universal hub (implicit)
- All buildings can attach to city
- No RequiredHubTypes check needed

**Example Placement Code:**
```csharp
// Check if hublet can be placed
if (buildingDef.IsHublet)
{
    var nearestHub = BuildingSimulation.FindNearestHub(
        position,
        cityBuildings,
        definitionLookup
    );

    if (nearestHub == null)
    {
        Debug.LogError("No hub nearby!");
        return false;
    }

    if (!BuildingSimulation.IsAdjacentTo(position, nearestHub.Position))
    {
        Debug.LogError("Must be adjacent to hub!");
        return false;
    }

    var hubDef = definitionLookup[nearestHub.BuildingDefinitionID];
    if (!BuildingSimulation.CanHubletAttachToHub(buildingDef, hubDef))
    {
        Debug.LogError("Hublet cannot attach to this hub type!");
        return false;
    }
}
```

---

### **Worker Assignment**

**Automatic (Recommended):**
```csharp
// In CitySimulation or BuildingManager
public static void AssignWorkersToBuildings(int cityId)
{
    var buildings = GetBuildingsForCity(cityId);
    var availableWorkers = GetAvailableWorkersByArchetype(cityId);

    // Priority 1: Buildings with required workers not met
    foreach (var building in buildings)
    {
        var deficit = BuildingSimulation.GetWorkerDeficit(building.Data, building.Definition);

        foreach (var kvp in deficit)
        {
            int toAssign = Math.Min(kvp.Value, availableWorkers[kvp.Key]);
            building.Data.AssignWorker(kvp.Key, toAssign);
            availableWorkers[kvp.Key] -= toAssign;
        }
    }

    // Priority 2: Fill buildings to optimal
    foreach (var building in buildings)
    {
        foreach (var archetype in building.Definition.AllowedWorkerTypes)
        {
            int canAccept = building.Definition.OptimalWorkers - building.Data.TotalWorkers;
            int available = availableWorkers[archetype];
            int toAssign = Math.Min(canAccept, available);

            if (toAssign > 0)
            {
                building.Data.AssignWorker(archetype, toAssign);
                availableWorkers[archetype] -= toAssign;
            }
        }
    }
}
```

---

### **Production Calculation**

**Example - Fully Staffed Workshop:**
```csharp
// Definition:
BaseProduction[Gold] = 3
MaxProduction[Gold] = 15
OptimalWorkers = 10
WorkerEfficiency[Artisan] = 1.2
BaseEfficiency = 1.0

// Instance:
AssignedWorkers[Artisan] = 10
TotalWorkers = 10

// Calculation:
WorkerRatio = 10 / 10 = 1.0
AvgEfficiency = (10 * 1.2) / 10 = 1.2
Production = Lerp(3, 15, 1.0) * 1.2 * 1.0
          = 15 * 1.2 * 1.0
          = 18 gold/day
```

**Example - Under-Staffed Mine:**
```csharp
// Definition:
BaseProduction[Iron] = 5
MaxProduction[Iron] = 30
OptimalWorkers = 20
MinWorkers = 5
WorkerEfficiency[Laborer] = 1.0
WorkerPenalties[Noble] = 0.5  (nobles are -50% at manual labor)

// Instance:
AssignedWorkers[Laborer] = 8
AssignedWorkers[Noble] = 2
TotalWorkers = 10

// Calculation:
WorkerRatio = 10 / 20 = 0.5
LaborerEfficiency = 1.0
NobleEfficiency = 1.0 * (1.0 - 0.5) = 0.5
AvgEfficiency = (8 * 1.0 + 2 * 0.5) / 10 = (8 + 1) / 10 = 0.9
Production = Lerp(5, 30, 0.5) * 0.9 * 1.0
          = 17.5 * 0.9
          = 15.75 → 16 iron/day
```

---

## Population Effects

Buildings affect workers' education and wealth over time.

**Example - University:**
```csharp
// Definition:
EducationGrowthPerWorker = 0.1f
AllowedWorkerTypes = [Artisan, Clergy]

// Instance:
AssignedWorkers[Artisan] = 5
AssignedWorkers[Clergy] = 3

// Daily Effects:
// For each Artisan worker in the building:
Artisan_PopGroup.Education += 0.1 * 5 = +0.5/day

// For each Clergy worker in the building:
Clergy_PopGroup.Education += 0.1 * 3 = +0.3/day
```

**Over 100 days:**
- Artisans gain 50 education
- Clergy gain 30 education
- This drives social mobility (promotions)!

---

## Integration with City System

**CityData should track buildings:**
```csharp
public List<int> BuildingIDs;  // Already implemented!
```

**CitySimulation should simulate buildings:**
```csharp
// In CitySimulation.SimulateDay()
foreach (int buildingId in city.BuildingIDs)
{
    var instance = BuildingManager.GetInstanceData(buildingId);
    var definition = BuildingManager.GetDefinition(instance.BuildingDefinitionID);

    BuildingSimulation.SimulateDay(instance, definition, ledger, city.CityID);
}

// Apply population growth bonuses from buildings
float growthBonus = BuildingSimulation.CalculatePopulationGrowthBonus(
    cityBuildings,
    definitions
);
city.GrowthRate += growthBonus;
```

---

## Implementation Status

**✅ COMPLETED:**

1. **BuildingManager Updated:**
   - ✅ Stores BuildingInstanceData internally
   - ✅ Maintains lookup for BuildingDefinition by ID
   - ✅ Integrated with CitySimulation
   - ✅ Worker management methods (AssignWorkerToBuilding, RemoveWorkerFromBuilding, ClearWorkersFromBuilding)
   - ✅ Hub/Hublet attachment methods (AttachHubletToHub, DetachHubletFromHub, GetHubletsForHub)
   - ✅ Query methods (GetInstanceData, GetDefinition)
   - ✅ Backward compatible with old BuildingInstance struct

2. **BuildingViewModel Created:**
   - ✅ Exposes building data for UI
   - ✅ Shows worker assignments and breakdown
   - ✅ Displays production efficiency
   - ✅ Shows hub/hublet relationships
   - ✅ Helper methods: GetStatusSummary(), GetWorkerBreakdown(), GetHubStatus(), GetPopulationEffectsSummary()
   - ✅ Follows MVVM pattern with Refresh() and property notifications

3. **CitySimulation Integration:**
   - ✅ SimulateBuildings() calls BuildingSimulation.SimulateDay()
   - ✅ Fallback to legacy BuildingInstance for backward compatibility
   - ✅ Gets both BuildingInstanceData and BuildingDefinition from BuildingManager

**🔧 TODO (Future Enhancements):**

1. **Worker Allocation System:**
   - ⏳ Automatic assignment algorithm
   - ⏳ Priority system (required workers first)
   - ⏳ Player override capability

2. **Hub/Hublet Placement Validation:**
   - ⏳ UI for checking adjacency
   - ⏳ Visual feedback for hub type compatibility
   - ⏳ Automatic AttachedToHubID / AttachedHubletIDs updates on placement

3. **Enhanced Population Integration:**
   - ⏳ Track employed workers in PopulationGroup.EmployedCount
   - ⏳ Sync between BuildingInstanceData.AssignedWorkers and PopulationManager
   - ✅ Apply education/wealth effects (already implemented in BuildingSimulation)

4. **BuildingDefinition Loading:**
   - ⏳ LoadBuildingDefinitions() from Resources folder
   - ⏳ Create example building ScriptableObjects (Barracks, Workshop, Market, etc.)
   - ⏳ Populate definitionLookup on BuildingManager.Initialize()

---

## Benefits

✅ **Clear Separation:** Data / Logic / View layers
✅ **Serializable:** Buildings can be saved/loaded
✅ **Testable:** Pure functions without Unity dependencies
✅ **Flexible:** Easy to add new building types
✅ **Scalable:** Worker system supports complex mechanics
✅ **Extensible:** Ready for prestige, culture, and other resources

The building system is now ready for complex economic and social simulations!
