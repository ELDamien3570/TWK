# City UI Setup Guide

This guide explains how to set up the City UI in Unity using the CityUIController system.

## Overview

The City UI displays comprehensive city information with a tabbed interface:
- **Main Info Bar:** City name, economy, population, military
- **Tabs:** Population Overview, Pop Groups, Military, Economic, Buildings, Culture, Religion

---

## Hierarchy Structure

```
Canvas (CityUI)
├── MainInfo (Panel)
│   ├── CityNameText (TextMeshProUGUI)
│   ├── EconomyOverviewText (TextMeshProUGUI)
│   ├── PopulationOverviewText (TextMeshProUGUI)
│   └── MilitaryEligibilityText (TextMeshProUGUI)
├── TabButtons (HorizontalLayoutGroup)
│   ├── PopulationTabButton (Button + TextMeshProUGUI)
│   ├── PopGroupsTabButton (Button + TextMeshProUGUI)
│   ├── MilitaryTabButton (Button + TextMeshProUGUI)
│   ├── EconomicTabButton (Button + TextMeshProUGUI)
│   ├── BuildingTabButton (Button + TextMeshProUGUI)
│   ├── CultureTabButton (Button + TextMeshProUGUI)
│   └── ReligionTabButton (Button + TextMeshProUGUI)
└── TabContent (Panel)
    ├── PopulationTab (Panel)
    │   ├── OverviewDetailsText (TextMeshProUGUI)
    │   ├── DemographicsText (TextMeshProUGUI)
    │   ├── GenderText (TextMeshProUGUI)
    │   ├── SocialMobilityText (TextMeshProUGUI)
    │   ├── ArchetypeBreakdownText (TextMeshProUGUI)
    │   └── LaborText (TextMeshProUGUI)
    ├── PopGroupsTab (Panel)
    │   └── PopGroupList (ScrollView)
    │       └── Content (VerticalLayoutGroup)
    ├── MilitaryTab (Panel)
    │   ├── MilitaryTotalText (TextMeshProUGUI)
    │   └── MilitaryByArchetypeText (TextMeshProUGUI)
    ├── EconomicTab (Panel)
    │   ├── ResourcesText (TextMeshProUGUI)
    │   ├── ProductionText (TextMeshProUGUI)
    │   └── ConsumptionText (TextMeshProUGUI)
    ├── BuildingTab (Panel)
    │   ├── BuildingList (ScrollView)
    │   │   └── Content (VerticalLayoutGroup)
    │   └── BuildPanel (Panel)
    │       ├── BuildFarmButton (Button + TextMeshProUGUI "Build Farm")
    │       └── BuildFarmCostText (TextMeshProUGUI)
    ├── CultureTab (Panel)
    │   └── PlaceholderText (TextMeshProUGUI "Coming Soon")
    └── ReligionTab (Panel)
        └── PlaceholderText (TextMeshProUGUI "Coming Soon")
```

---

## Step-by-Step Setup

### 1. Create Canvas

1. Right-click in Hierarchy → UI → Canvas
2. Rename to "CityUI"
3. Set Canvas Scaler to "Scale With Screen Size"
4. Reference Resolution: 1920x1080

### 2. Create Main Info Bar

1. Create Panel under Canvas (name: "MainInfo")
2. Anchor to top, set height to ~150
3. Add VerticalLayoutGroup (spacing: 10)
4. Create 4 TextMeshProUGUI children:
   - CityNameText (font size: 36, bold)
   - EconomyOverviewText (font size: 20)
   - PopulationOverviewText (font size: 20)
   - MilitaryEligibilityText (font size: 20)

### 3. Create Tab Buttons

1. Create Panel under Canvas (name: "TabButtons")
2. Anchor to top-left, position below MainInfo
3. Add HorizontalLayoutGroup (spacing: 10)
4. Create 7 Buttons:
   - PopulationTabButton → "Population"
   - PopGroupsTabButton → "Pop Groups"
   - MilitaryTabButton → "Military"
   - EconomicTabButton → "Economic"
   - BuildingTabButton → "Buildings"
   - CultureTabButton → "Culture"
   - ReligionTabButton → "Religion"
5. Each button should have a TextMeshProUGUI child for the label

### 4. Create Tab Content Panel

1. Create Panel under Canvas (name: "TabContent")
2. Anchor to fill remaining space below TabButtons
3. Create 7 child panels (one for each tab)

### 5. Setup Population Tab

Create Panel "PopulationTab" with 6 TextMeshProUGUI children:
- **OverviewDetailsText:** Total population, averages
- **DemographicsText:** Age breakdown
- **GenderText:** Gender distribution
- **SocialMobilityText:** Promotion/demotion info
- **ArchetypeBreakdownText:** Class distribution
- **LaborText:** Employment info

Layout with GridLayoutGroup or VerticalLayoutGroup.

### 6. Setup Pop Groups Tab

1. Create Panel "PopGroupsTab"
2. Add ScrollView → "PopGroupList"
3. Content area needs VerticalLayoutGroup + ContentSizeFitter
4. Create prefab "PopGroupItemPrefab":
   - Panel with VerticalLayoutGroup
   - Add PopulationGroupUIItem component
   - Add 6 TextMeshProUGUI fields:
     - ArchetypeNameText
     - PopulationText
     - WealthEducationText
     - LaborText
     - DemographicsText
     - PromotionStatusText

### 7. Setup Military Tab

Create Panel "MilitaryTab" with 2 TextMeshProUGUI children:
- **MilitaryTotalText:** Total eligible + percentage
- **MilitaryByArchetypeText:** Breakdown by class

### 8. Setup Economic Tab

Create Panel "EconomicTab" with 3 TextMeshProUGUI children:
- **ResourcesText:** Current stockpiles
- **ProductionText:** Daily production
- **ConsumptionText:** Daily consumption

### 9. Setup Building Tab

1. Create Panel "BuildingTab"
2. Add ScrollView → "BuildingList"
3. Content area needs VerticalLayoutGroup + ContentSizeFitter
4. Create "BuildPanel" at bottom:
   - BuildFarmButton (Button + "Build Farm" text)
   - BuildFarmCostText ("Cost: 50 Gold")
5. Create prefab "BuildingItemPrefab":
   - Panel with VerticalLayoutGroup
   - Add BuildingUIItem component
   - Add 5 TextMeshProUGUI fields:
     - BuildingNameText
     - StatusText
     - WorkerText
     - ProductionText
     - HubStatusText

### 10. Setup Culture & Religion Tabs

Create Panels with placeholder text:
- **CultureTab:** "Culture system coming soon"
- **ReligionTab:** "Religion system coming soon"

### 11. Attach CityUIController

1. Add CityUIController component to Canvas
2. Assign references:
   - **Target City:** Drag your City GameObject
   - **Main Info:** Assign all text fields
   - **Tabs:** Assign all tab panels
   - **Tab Buttons:** Assign all buttons
   - **Tab-specific fields:** Assign texts/containers for each tab
   - **Prefabs:** Assign PopGroupItemPrefab and BuildingItemPrefab

---

## Quick Setup (Alternative)

If you have access to Unity's UI Builder or prefer code:

```csharp
// Example: Programmatically create basic structure
public class CityUIBuilder : MonoBehaviour
{
    public static GameObject CreateCityUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("CityUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add CityUIController
        CityUIController controller = canvasObj.AddComponent<CityUIController>();

        // ... create UI elements programmatically

        return canvasObj;
    }
}
```

---

## Prefab Templates

### PopGroupItemPrefab Layout

```
PopGroupItem (Panel - 400x180)
├── ArchetypeNameText (TMP) - "Laborer (ID: 5)"
├── PopulationText (TMP) - "Population: 1500 | Avg Age: 32"
├── WealthEducationText (TMP) - "Wealth: 45 | Education: 25"
├── LaborText (TMP) - "Employed: 800/1200"
├── DemographicsText (TMP) - "Youth: 300 | Adults: 900..."
└── PromotionStatusText (TMP) - "Eligible for promotion"
```

### BuildingItemPrefab Layout

```
BuildingItem (Panel - 400x200)
├── BuildingNameText (TMP) - "Farm (Economics)"
├── StatusText (TMP) - "Optimal (10/10)"
├── WorkerText (TMP) - "Workers: 10/10 (100%)"
├── ProductionText (TMP) - "Food: 175/day"
└── HubStatusText (TMP) - "Standard building"
```

---

## Usage

Once set up, the CityUIController will:
1. Automatically create a CityViewModel on Start()
2. Populate all tabs with data
3. Handle tab switching
4. Refresh UI when needed
5. Handle farm construction when Build Farm is clicked

**To refresh manually:**
```csharp
CityUIController controller = FindObjectOfType<CityUIController>();
controller.RefreshUI();
```

**To change target city:**
```csharp
controller.SetTargetCity(newCity);
```

---

## Styling Tips

### Colors

- **Headers:** Bold, size 24-36
- **Body Text:** Regular, size 16-20
- **Success:** Green (#4CAF50)
- **Warning:** Yellow (#FFC107)
- **Error:** Red (#F44336)
- **Info:** Blue (#2196F3)

### Layout

- Use LayoutGroups for automatic positioning
- Add ContentSizeFitter to ScrollView content
- Set padding/spacing on LayoutGroups (10-20 units)
- Use anchors for responsive design

### Fonts

- Import TextMeshPro (Window → TextMeshPro → Import TMP Essential Resources)
- Use TMP for all text (better quality, more features)

---

## Testing

1. Assign a City with population and buildings to the controller
2. Enter Play Mode
3. Click tab buttons to switch views
4. Click "Build Farm" to test construction
5. Check Console for any errors

---

## Notes

- Culture and Religion tabs are placeholders for future implementation
- Build button currently only supports Farm (more buildings unlock via culture tech later)
- UI refreshes automatically when tabs switch
- Population groups and buildings are dynamically created from ViewModels
- Farm building uses legacy BuildingData system (will migrate to BuildingDefinition)

---

## Troubleshooting

**UI not showing:**
- Check Canvas render mode
- Ensure Camera is assigned if using World Space
- Check sorting order if overlapping with other canvases

**Tabs not switching:**
- Verify tab buttons have onClick listeners
- Check that tab panels are assigned in inspector
- Ensure CityUIController is on the Canvas

**Build Farm not working:**
- Verify FarmBuildingData exists in Resources/Buildings/
- Check city has enough resources (50 Gold)
- Look for errors in Console

**Population groups not showing:**
- Verify PopGroupItemPrefab is assigned
- Check prefab has PopulationGroupUIItem component
- Ensure city has population groups assigned

The UI is now ready for gameplay integration!
