# Government System Setup Guide

This guide will help you set up and test the Government & Offices system.

## Quick Start (5 Minutes)

### 1. Create Test Scene

1. Create a new scene: `File > New Scene`
2. Save it as `GovernmentSystemTest` in `Assets/Scenes/`

### 2. Set Up Managers

Create an empty GameObject called `GameManagers` and add these components:

1. **WorldTimeManager** (if not already present)
2. **GovernmentManager** (`Assets/Core/Simulation/Government/Managers/`)
3. **ContractManager** (`Assets/Core/Simulation/Government/Managers/`)

**Important**: Make sure WorldTimeManager initializes first, then call:
```csharp
GovernmentManager.Instance.Initialize(WorldTimeManager.Instance);
ContractManager.Instance.Initialize(WorldTimeManager.Instance);
```

### 3. Set Up Test Script

1. Create another empty GameObject called `GovernmentSystemTest`
2. Add the `GovernmentSystemTest` component to it (`Assets/Core/Simulation/Government/Testing/`)
3. In the Inspector:
   - Check `Auto Run Tests`
   - Check `Create Test Data`
   - Check `Verbose Logging`
   - Set `Test Realm ID` to `1`

### 4. Set Up UI (Optional but Recommended)

1. Create an empty GameObject called `UISetup`
2. Add the `GovernmentUISetup` component (`Assets/Core/UI/GovernmentUI/`)
3. Right-click the component and select:
   - `Create Test Canvas`
   - `Setup All UI Panels`

This will create basic UI panels for Government, Bureaucracy, and Contracts.

### 5. Run the Test

Press Play! You should see:

```
=== GOVERNMENT SYSTEM TEST STARTING ===
[GovernmentTest] Creating test institutions...
[GovernmentTest] Created 3 test institutions
[GovernmentTest] Creating test governments...
[GovernmentTest] Created 3 test governments
...
=== GOVERNMENT SYSTEM TEST COMPLETE ===
```

---

## Manual Testing

The `GovernmentSystemTest` script has several manual test methods you can run from the Inspector:

### Right-Click Context Menu Options:

1. **Run All Tests** - Runs the complete test suite
2. **Test 1: Create Government** - Sets up government for test realm
3. **Test 2: Create Contracts** - Creates vassal and governor contracts
4. **Test 3: Create Offices** - Creates bureaucratic offices
5. **Test 4: Enact Edicts** - Enacts test edicts
6. **Test 5: Modify Legitimacy** - Reduces legitimacy by 20
7. **Test 6: Calculate Revolt Risk** - Shows current revolt risk

### UI Setup Context Menu Options:

1. **Create Test Canvas** - Creates a Canvas if none exists
2. **Setup All UI Panels** - Creates all three UI panels
3. **Setup Government Panel** - Creates just the government panel
4. **Setup Bureaucracy Panel** - Creates just the bureaucracy panel
5. **Setup Contract Panel** - Creates just the contract panel

---

## Test Data Created

The test script automatically creates:

### Institutions (3):
1. **Bureaucratic Administration** - +15% building efficiency
2. **Standing Army** - +25% military power
3. **Divine Theocracy** - +20% religious fervor

### Governments (3):
1. **Feudal Monarchy**
   - Autocratic, Territorial
   - Hereditary succession
   - Base Capacity: 35, Base Legitimacy: 60
   - Institution: Bureaucratic Administration

2. **Tribal Chiefdom**
   - Chiefdom, Tribal
   - Election succession
   - Base Capacity: 20, Base Legitimacy: 50
   - No institutions

3. **Theocratic Empire**
   - Autocratic, Territorial
   - Appointment succession
   - Base Capacity: 50, Base Legitimacy: 70
   - Institutions: Bureaucratic Administration + Divine Theocracy

### Edicts (3):
1. **Forced Labor**
   - Duration: 6 months
   - Effect: +30% construction speed
   - Loyalty: Laborers -15, Slaves -10, Nobles +5

2. **Tax Relief**
   - Duration: 12 months
   - Effect: None (just loyalty)
   - Loyalty: Laborers +10, Artisans +8, Merchants +12, Nobles -5

3. **Religious Devotion**
   - Duration: 24 months
   - Effect: +15% population fervor
   - Loyalty: Clergy +20, Laborers -5

---

## Integration with Existing Systems

The government system integrates with:

### 1. ModifierManager
- Edicts register modifiers automatically
- Government institutions provide permanent modifiers
- All modifiers are properly tagged with source information

### 2. CultureManager
- Culture match affects legitimacy (±10)
- Office efficiency uses agent skills (when AgentManager exists)

### 3. ReligionManager
- Clergy happiness affects legitimacy
- Theocracies get 2x multiplier on clergy effects

### 4. PopulationManager
- Edicts affect population loyalty by archetype
- Office automation can boost slave loyalty
- Population filtering for realm-specific operations

### 5. WorldTimeManager
- Daily ticks: Office automation
- Seasonal ticks: Edict expiration, revolt checks
- Yearly ticks: Legitimacy decay, culture/religion effects

---

## ViewModels

If you have `ViewModelService` in your scene, the system will automatically create ViewModels:

- **GovernmentViewModel** - For GovernmentUIController
- **BureaucracyViewModel** - For BureaucracyUIController
- **ContractViewModel** - For ContractUIController

ViewModels auto-refresh on manager events and provide formatted data for UI display.

---

## Troubleshooting

### "GovernmentManager.Instance is null"
- Make sure GovernmentManager is in the scene
- Check that it's on an active GameObject

### "WorldTimeManager not found"
- Add WorldTimeManager component to a GameObject
- Ensure it initializes before calling `Initialize()` on government managers

### "No Canvas found"
- Use `GovernmentUISetup > Create Test Canvas` context menu
- Or manually add a Canvas to the scene

### UI Controllers not working
- Ensure SerializeField references are assigned in Inspector
- Check that managers are initialized before UI controllers Start()
- Use the basic panels created by GovernmentUISetup as starting point

### Compilation errors
- All API compatibility issues should be resolved
- If you see Contract/Modifier errors, ensure you pulled latest changes

---

## Next Steps

### Phase 7: Advanced Features (Not Yet Implemented)
- Full office automation for all purposes
- Reform UI with cost preview
- Enhanced revolt mechanics with faction spawning
- More sophisticated legitimacy calculations

### Phase 8: Testing & Polish (Not Yet Implemented)
- Balance modifiers and costs
- Create proper UI prefabs with full styling
- Add tooltips and help text
- Performance optimization

---

## Example Usage in Your Game

```csharp
using TWK.Government;

// Set up a realm's government
public void SetupRealmGovernment(int realmID, GovernmentData government)
{
    GovernmentManager.Instance.SetRealmGovernment(realmID, government);

    // Create initial offices
    var taxOffice = GovernmentManager.Instance.CreateOffice(
        realmID,
        "Royal Tax Collector",
        TreeType.Economics,
        OfficePurpose.ManageTaxCollection
    );

    // Assign your ruler as tax collector
    GovernmentManager.Instance.AssignOffice(realmID, taxOffice, rulerAgentID);
}

// Create a vassal relationship
public void CreateVassalship(int overlordID, int vassalID)
{
    var contract = ContractManager.Instance.CreateContract(overlordID, ContractType.Vassal);
    contract.SetSubjectRealm(vassalID);

    // Customize terms
    contract.GoldPercentage = 30f;
    contract.ManpowerPercentage = 50f;
    contract.AllowSubjectGovernment = true;
    contract.UpdateLoyalty();
}

// Enact an edict
public void EnactWarEdict(int realmID, Edict warEdict)
{
    bool success = GovernmentManager.Instance.EnactEdict(realmID, warEdict);
    if (success)
    {
        Debug.Log($"Enacted {warEdict.EdictName} successfully!");
    }
}
```

---

## File Locations

### Managers
- `Assets/Core/Simulation/Government/Managers/GovernmentManager.cs`
- `Assets/Core/Simulation/Government/Managers/ContractManager.cs`

### Data
- `Assets/Core/Simulation/Government/Data/GovernmentData.cs`
- `Assets/Core/Simulation/Government/Data/Institution.cs`
- `Assets/Core/Simulation/Government/Data/Office.cs`
- `Assets/Core/Simulation/Government/Data/Contract.cs`
- `Assets/Core/Simulation/Government/Data/Edict.cs`
- `Assets/Core/Simulation/Government/Data/GovernmentEnums.cs`

### UI
- `Assets/Core/UI/GovernmentUI/GovernmentUIController.cs`
- `Assets/Core/UI/GovernmentUI/BureaucracyUIController.cs`
- `Assets/Core/UI/GovernmentUI/ContractUIController.cs`

### ViewModels
- `Assets/Core/UI/ViewModels/GovernmentViewModel.cs`
- `Assets/Core/UI/ViewModels/BureaucracyViewModel.cs`
- `Assets/Core/UI/ViewModels/ContractViewModel.cs`

### Testing
- `Assets/Core/Simulation/Government/Testing/GovernmentSystemTest.cs`
- `Assets/Core/UI/GovernmentUI/GovernmentUISetup.cs`

---

## Support

If you encounter issues:

1. Check the Console for error messages
2. Ensure all managers are properly initialized
3. Verify that WorldTimeManager exists and is active
4. Review the test output to see which systems are working

The test script provides detailed logging - use `Verbose Logging` option for maximum detail.
