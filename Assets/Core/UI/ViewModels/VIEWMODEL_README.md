# ViewModel System Documentation

## Overview

The ViewModel system provides a clean separation between your game logic (simulation layer) and UI presentation layer. It follows the **Model-View-ViewModel (MVVM)** pattern, making it easy to build UI that reacts to game state changes.

## Architecture

```
Game Logic Layer          ViewModel Layer           UI Layer
┌──────────────┐         ┌─────────────────┐      ┌──────────────┐
│ City         │────────▶│ CityViewModel   │─────▶│ UI Components│
│ PopulationGroup────────▶│ PopGroupVM      │      │              │
│ Buildings    │         │                 │      │              │
└──────────────┘         └─────────────────┘      └──────────────┘
                               │
                         ViewModelService
                         (Auto-updates daily)
```

## Core Components

### **1. BaseViewModel**
Base class for all ViewModels providing:
- Property change notifications (`OnPropertyChanged` event)
- `Refresh()` abstract method for updating data

### **2. PopulationGroupViewModel**
Displays individual population group data including:
- **Identity:** Archetype, ID, City
- **Demographics:** Age distribution, gender breakdown
- **Economic:** Wealth, education
- **Labor:** Available workers, employment status
- **Cultural:** Fervor, religion, loyalty
- **Helper Methods:**
  - `GetPromotionStatus()` - Shows promotion requirements
  - `GetDemotionRisk()` - Warns if at risk of demotion
  - `GetDemographicSummary()` - Formatted age breakdown
  - `GetLaborSummary()` - Formatted labor statistics

### **3. CityViewModel**
Aggregates city-wide data including:
- **Population Summary:** Total, by archetype, demographics
- **Labor Force:** Available workers, employment, unemployment
- **Military:** Eligible population for recruitment
- **Economy:** Resources, production, consumption
- **Aggregate Stats:** Average wealth, education, fervor, loyalty, happiness
- **Population Groups:** Collection of `PopulationGroupViewModel`s

**Helper Methods:**
- `GetPopulationBreakdownSummary()` - Class distribution
- `GetDemographicSummary()` - Age distribution
- `GetGenderSummary()` - Gender ratios
- `GetLaborSummary()` - Labor statistics
- `GetMilitarySummary()` - Military recruitment pool
- `GetEconomySummary()` - Economic snapshot
- `GetSocialMobilitySummary()` - Promotion/demotion summary
- `GetGroupsByArchetype(archetype)` - Filter groups by class
- `GetArchetypePercentage(archetype)` - Percentage of class

### **4. ViewModelService** (MonoBehaviour Singleton)
Manages all ViewModels:
- **Auto-creation:** Register cities to create ViewModels
- **Auto-update:** Updates all ViewModels daily (subscribes to `WorldTimeManager`)
- **Events:**
  - `OnViewModelsUpdated` - All ViewModels updated
  - `OnCityViewModelUpdated(cityId)` - Specific city updated
- **Methods:**
  - `RegisterCity(city)` - Create ViewModel for a city
  - `GetCityViewModel(cityId)` - Retrieve city ViewModel
  - `GetAllCityViewModels()` - Get all city ViewModels
  - `RefreshAllViewModels()` - Manual refresh trigger

---

## Setup

### **Step 1: Add ViewModelService to Scene**

1. Create an empty GameObject in your scene
2. Name it "ViewModelService"
3. Add the `ViewModelService` component
4. Assign the `WorldTimeManager` reference in the Inspector

### **Step 2: Initialize in SimulationManager**

The `SimulationManager` already initializes the `ViewModelService`:

```csharp
// In SimulationManager.Start()
if (ViewModelService.Instance != null)
    ViewModelService.Instance.Initialize(worldTimeManager);
```

### **Step 3: Register Cities**

Cities are automatically registered when created:

```csharp
// In TestStart() after city initialization
if (ViewModelService.Instance != null)
    ViewModelService.Instance.RegisterCity(city);
```

---

## Usage Examples

### **Console Display (Quick Testing)**

Use `CityInfoConsoleDisplay` for console-based debugging:

1. Add `CityInfoConsoleDisplay` component to any GameObject
2. Set the `cityId` in the Inspector
3. Enable `logOnStart` to log city info on startup
4. Enable `autoLog` for periodic updates
5. Right-click the component and select "Log City Info" or "Log All Cities"

**Example Output:**
```
╔════════════════════════════════════════════════════════════════
║ TestCity (ID: 0)
╠════════════════════════════════════════════════════════════════
║ POPULATION: 170
║   Growth Rate: 1.00
║   Average Age: 30.1 years
║   Gender: Male 51.0% | Female 49.0%
║
║ POPULATION BY CLASS
║   Slaves:        15  (8.8%)
║   Laborers:     100  (58.8%)
║   Artisans:      10  (5.9%)
║   Merchants:      5  (2.9%)
║   Nobles:        30  (17.6%)
║   Clergy:        10  (5.9%)
...
```

### **UI Display (TextMeshPro)**

Use `CityInfoDebugUI` for UI-based display:

1. Create a Canvas in your scene
2. Add TextMeshPro components for each section:
   - City Name
   - Population Summary
   - Population Breakdown
   - Demographics
   - Labor & Military
   - Economy
   - Social Mobility
   - Population Groups Detail
3. Add `CityInfoDebugUI` component to the Canvas
4. Assign all TextMeshPro references in the Inspector
5. Set the `cityId` to display
6. Enable `autoRefresh` for automatic updates

The UI will update automatically every day (or at your configured refresh interval).

### **Custom UI Binding**

Create your own UI scripts by subscribing to ViewModel events:

```csharp
using TWK.UI.ViewModels;

public class MyCustomUI : MonoBehaviour
{
    [SerializeField] private int cityId;

    private void Start()
    {
        // Subscribe to updates
        ViewModelService.Instance.OnCityViewModelUpdated += OnCityUpdated;
    }

    private void OnDestroy()
    {
        if (ViewModelService.Instance != null)
            ViewModelService.Instance.OnCityViewModelUpdated -= OnCityUpdated;
    }

    private void OnCityUpdated(int updatedCityId)
    {
        if (updatedCityId != cityId) return;

        var cityVM = ViewModelService.Instance.GetCityViewModel(cityId);

        // Update your UI
        titleText.text = cityVM.CityName;
        populationText.text = $"Population: {cityVM.TotalPopulation:N0}";

        // Access individual population groups
        foreach (var popVM in cityVM.PopulationGroups)
        {
            Debug.Log($"{popVM.ArchetypeName}: {popVM.TotalPopulation}");
        }
    }
}
```

---

## Data Available

### **CityViewModel Properties**

**Population:**
- `TotalPopulation` - Total city population
- `SlaveCount`, `LaborerCount`, `ArtisanCount`, `MerchantCount`, `NobleCount`, `ClergyCount`
- `PopulationByArchetype` - Dictionary of counts by class

**Demographics:**
- `TotalYouth`, `TotalAdults`, `TotalMiddleAge`, `TotalElderly`
- `TotalMales`, `TotalFemales`, `MalePercentage`, `FemalePercentage`
- `AverageAge`

**Labor:**
- `TotalAvailableWorkers` - Working-age population
- `TotalEmployed`, `TotalUnemployed`, `UnemploymentRate`

**Military:**
- `TotalMilitaryEligible` - Adult males (18-45)
- `MilitaryEligiblePercentage`

**Economy:**
- `FoodStockpile`, `GoldStockpile`
- `DailyFoodProduction`, `DailyFoodConsumption`, `DailyFoodNet`
- `BuildingCount`

**Aggregate Stats:**
- `AverageWealth`, `AverageEducation`, `AverageFervor`, `AverageLoyalty`, `AverageHappiness`

**Collections:**
- `PopulationGroups` - List of all population group ViewModels

### **PopulationGroupViewModel Properties**

**Population:**
- `TotalPopulation`, `AverageAge`
- `MalePercentage`, `FemalePercentage`

**Demographics Breakdown:**
- `YoungMales`, `YoungFemales` (0-17)
- `AdultMales`, `AdultFemales` (18-45)
- `MiddleMales`, `MiddleFemales` (46-60)
- `ElderlyMales`, `ElderlyFemales` (60+)
- `TotalYouth`, `TotalAdults`, `TotalMiddleAge`, `TotalElderly`

**Economic:**
- `Wealth` (0-100), `WealthFormatted` (string)
- `Education` (0-100), `EducationFormatted` (string)

**Labor:**
- `AvailableWorkers`, `EmployedCount`, `UnemployedCount`
- `UnemploymentRate`, `UnemploymentRateFormatted`
- `MilitaryEligible`, `MilitaryEligiblePercentage`

**Cultural:**
- `Fervor` (0-100), `FervorFormatted`
- `ReligionName`
- `Loyalty` (-100 to 100), `LoyaltyFormatted`

**Status:**
- `Happiness` (0-1), `HappinessFormatted` (percentage)
- `GrowthModifier`

---

## Performance Considerations

### **Update Frequency**

ViewModels are updated automatically **once per day** (game time). This is efficient because:
- UI doesn't need millisecond-level precision
- Reduces overhead on UI updates
- Batches all ViewModel refreshes together

### **Manual Refresh**

If you need immediate updates (e.g., after a manual event):

```csharp
ViewModelService.Instance.RefreshCityViewModel(cityId);
// or
ViewModelService.Instance.RefreshAllViewModels();
```

### **Event Subscription**

Always unsubscribe from events in `OnDestroy()` to prevent memory leaks:

```csharp
private void OnDestroy()
{
    if (ViewModelService.Instance != null)
    {
        ViewModelService.Instance.OnCityViewModelUpdated -= OnCityUpdated;
    }
}
```

---

## Advanced Usage

### **Filtering and Querying**

```csharp
var cityVM = ViewModelService.Instance.GetCityViewModel(cityId);

// Get all artisan groups
var artisans = cityVM.GetGroupsByArchetype(PopulationArchetypes.Artisan);

// Get percentage of nobles
float noblePercentage = cityVM.GetArchetypePercentage(PopulationArchetypes.Noble);

// Find groups eligible for promotion
var eligibleGroups = cityVM.PopulationGroups
    .Where(p => p.Education >= 40f && p.Wealth >= 30f)
    .ToList();

// Find struggling groups (low wealth)
var strugglingGroups = cityVM.PopulationGroups
    .Where(p => p.Wealth < 20f)
    .ToList();
```

### **Formatted Summaries**

ViewModels provide pre-formatted summary strings:

```csharp
var cityVM = ViewModelService.Instance.GetCityViewModel(cityId);

Debug.Log(cityVM.GetPopulationBreakdownSummary());
// "Slaves: 15 | Laborers: 100 | Artisans: 10 | ..."

Debug.Log(cityVM.GetDemographicSummary());
// "Youth: 34 (20.0%) | Adults: 76 (44.7%) | ..."

Debug.Log(cityVM.GetSocialMobilitySummary());
// "Eligible for promotion: 2 groups | At risk of demotion: 1 groups"
```

### **Custom Calculations**

Access the underlying `PopulationGroup` objects if needed:

```csharp
// Calculate total wealth in the city
float totalWealth = 0f;
foreach (var popVM in cityVM.PopulationGroups)
{
    totalWealth += popVM.Wealth * popVM.TotalPopulation;
}
float avgWealth = totalWealth / cityVM.TotalPopulation;
```

---

## Debugging

### **Context Menu Commands**

**ViewModelService:**
- Right-click → "Refresh All ViewModels" - Manual refresh
- Right-click → "Log All City ViewModels" - Debug output

**CityInfoConsoleDisplay:**
- Right-click → "Log City Info" - Log selected city
- Right-click → "Log All Cities" - Log all cities

**CityInfoDebugUI:**
- Right-click → "Refresh UI" - Force UI update

### **Common Issues**

**"ViewModelService not found"**
- Ensure ViewModelService GameObject is in the scene
- Verify ViewModelService component is attached
- Check that Initialize() is called in SimulationManager

**"No ViewModel found for city ID: X"**
- Ensure city is registered: `ViewModelService.Instance.RegisterCity(city)`
- Check that registration happens AFTER city initialization

**UI Not Updating**
- Verify `autoRefresh` is enabled (CityInfoDebugUI)
- Check that TextMeshPro components are assigned
- Ensure ViewModelService is initialized
- Try manual refresh: Right-click → "Refresh UI"

---

## Future Enhancements

Planned features for the ViewModel system:

### **Phase 1: Current State** ✅
- [x] City and Population ViewModels
- [x] Automatic daily updates
- [x] Console and UI display scripts
- [x] Event-based change notifications

### **Phase 2: Building ViewModels**
- [ ] BuildingViewModel for individual buildings
- [ ] Building efficiency display
- [ ] Worker assignment visualization
- [ ] Production/consumption breakdown per building

### **Phase 3: Realm ViewModels**
- [ ] RealmViewModel for kingdom-level stats
- [ ] Multi-city aggregation
- [ ] Diplomacy and relations display
- [ ] Realm-wide policies

### **Phase 4: Advanced Features**
- [ ] Historical data tracking (population over time)
- [ ] Trend analysis (growth rates, social mobility)
- [ ] Predictive analytics (food shortages, unemployment)
- [ ] Comparison tools (compare cities)

---

## Best Practices

1. **Always use ViewModels for UI** - Never access game logic directly from UI
2. **Subscribe to events** - Let ViewModelService notify you of changes
3. **Unsubscribe in OnDestroy** - Prevent memory leaks
4. **Use helper methods** - Pre-formatted strings save time
5. **Batch updates** - Use `RefreshAllViewModels()` instead of individual refreshes
6. **Don't modify ViewModels** - They're read-only representations of game state

---

## Summary

The ViewModel system provides:
- ✅ **Clean separation** between game logic and UI
- ✅ **Automatic updates** - No manual polling required
- ✅ **Rich data access** - All population/city data in one place
- ✅ **Event-driven** - React to changes efficiently
- ✅ **Pre-formatted** - Helper methods for common displays
- ✅ **Extensible** - Easy to add new ViewModels
- ✅ **Debuggable** - Console display and context menu tools

**Ready to build your UI!** Start with `CityInfoConsoleDisplay` for testing, then create custom UI using `CityViewModel` and `PopulationGroupViewModel`.
