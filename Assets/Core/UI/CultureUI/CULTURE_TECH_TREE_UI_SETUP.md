# Culture Tech Tree UI Setup Guide

This guide explains how to set up the Culture Tech Tree UI in Unity.

## Overview

The Culture Tech Tree UI allows players to:
- Select and view different cultures
- Switch between tree types (Economics, Military, etc.)
- View tech nodes with their status (locked, available, unlocked)
- See node details (description, prerequisites, unlocks, modifiers)
- Unlock nodes by spending culture XP

## UI Hierarchy

Create this hierarchy in your Canvas:

```
Canvas
└── CultureTechTreeUI (GameObject)
    ├── CultureTechTreeUIController (Script Component)
    │
    ├── Header (Panel)
    │   ├── CultureDropdown (TMP_Dropdown)
    │   ├── CultureName (TextMeshProUGUI)
    │   └── CultureDescription (TextMeshProUGUI)
    │
    ├── TreeTypeTabs (Horizontal Layout Group)
    │   ├── EconomicsTab (Button)
    │   │   └── Text (TextMeshProUGUI) "Economics"
    │   ├── MilitaryTab (Button)
    │   │   └── Text (TextMeshProUGUI) "Military"
    │   ├── ReligionTab (Button)
    │   │   └── Text (TextMeshProUGUI) "Religion"
    │   ├── AdministrationTab (Button)
    │   │   └── Text (TextMeshProUGUI) "Administration"
    │   └── CultureTab (Button)
    │       └── Text (TextMeshProUGUI) "Culture"
    │
    ├── TreeInfoPanel (Panel)
    │   ├── TreeName (TextMeshProUGUI)
    │   ├── AvailableXP (TextMeshProUGUI)
    │   ├── TotalXP (TextMeshProUGUI)
    │   └── OwnerRealm (TextMeshProUGUI)
    │
    ├── NodeListPanel (Panel)
    │   ├── Title (TextMeshProUGUI) "Available Nodes"
    │   └── ScrollView
    │       └── Viewport
    │           └── Content (Vertical Layout Group + Content Size Fitter)
    │               └── [TechNodeItems spawned here at runtime]
    │
    └── NodeDetailsPanel (Panel)
        ├── Title (TextMeshProUGUI) "Node Details"
        ├── NodeName (TextMeshProUGUI)
        ├── NodeDescription (TextMeshProUGUI)
        ├── NodeCost (TextMeshProUGUI)
        ├── Prerequisites (TextMeshProUGUI)
        ├── Unlocks (TextMeshProUGUI)
        ├── Modifiers (TextMeshProUGUI)
        └── UnlockButton (Button)
            └── ButtonText (TextMeshProUGUI)
```

## Step-by-Step Setup

### 1. Create Main Container

1. Right-click in Canvas → Create Empty
2. Name it "CultureTechTreeUI"
3. Add **CultureTechTreeUIController** component
4. Add **Canvas Group** (for show/hide functionality)

### 2. Create Header Panel

**Header Panel:**
1. Create Panel as child of CultureTechTreeUI
2. Name it "Header"
3. Set height: 150

**Culture Dropdown:**
1. Create TMP_Dropdown in Header
2. Position: Top-left
3. Size: 300x40

**Culture Name:**
1. Create TextMeshProUGUI in Header
2. Position: Center
3. Font Size: 32
4. Alignment: Center
5. Text: "Culture Name"

**Culture Description:**
1. Create TextMeshProUGUI in Header
2. Position: Below culture name
3. Font Size: 16
4. Alignment: Center
5. Enable Word Wrapping
6. Text: "Culture description goes here"

### 3. Create Tree Type Tabs

**Tabs Container:**
1. Create Panel in CultureTechTreeUI
2. Name it "TreeTypeTabs"
3. Add **Horizontal Layout Group**:
   - Spacing: 10
   - Child Force Expand: Width = true, Height = false
4. Add **Content Size Fitter**:
   - Horizontal Fit: Preferred Size
   - Vertical Fit: Preferred Size
5. Position: Below Header
6. Height: 60

**Tab Buttons (create 5 of these):**
1. Create Button as child of TreeTypeTabs
2. Names: "EconomicsTab", "MilitaryTab", "ReligionTab", "AdministrationTab", "CultureTab"
3. Add TextMeshProUGUI child to each
4. Button text: "Economics", "Military", "Religion", "Administration", "Culture"
5. Colors:
   - Normal: Light gray
   - Highlighted: White
   - Pressed: Yellow
   - Selected: Green

### 4. Create Tree Info Panel

**Panel:**
1. Create Panel in CultureTechTreeUI
2. Name it "TreeInfoPanel"
3. Position: Below tabs
4. Height: 100
5. Background: Slightly darker than canvas

**Text Fields (create 4):**
1. Create TextMeshProUGUI children
2. Names: "TreeName", "AvailableXP", "TotalXP", "OwnerRealm"
3. Layout vertically with 5px spacing
4. Font Size: 18
5. Example text:
   - "Economics Tech Tree"
   - "Available XP: 0"
   - "Total XP Earned: 0"
   - "Controlled by Realm 0"

### 5. Create Node List Panel

**Panel:**
1. Create Panel in CultureTechTreeUI
2. Name it "NodeListPanel"
3. Position: Left side, below TreeInfoPanel
4. Size: Take up left 60% of remaining space

**Title:**
1. Create TextMeshProUGUI at top of panel
2. Text: "Available Nodes"
3. Font Size: 24

**Scroll View:**
1. Create Scroll View in NodeListPanel
2. Scroll Direction: Vertical only
3. Viewport → Content:
   - Add **Vertical Layout Group**:
     - Spacing: 5
     - Child Force Expand: Width = true, Height = false
   - Add **Content Size Fitter**:
     - Vertical Fit: Preferred Size

### 6. Create Node Details Panel

**Panel:**
1. Create Panel in CultureTechTreeUI
2. Name it "NodeDetailsPanel"
3. Position: Right side, below TreeInfoPanel
4. Size: Take up right 40% of remaining space
5. Initially set Active = false

**Title:**
1. Create TextMeshProUGUI
2. Text: "Node Details"
3. Font Size: 24

**Detail Fields (create these in order):**
1. **NodeName** (TextMeshProUGUI):
   - Font Size: 28
   - Font Style: Bold
   - Color: Yellow

2. **NodeDescription** (TextMeshProUGUI):
   - Font Size: 16
   - Enable Word Wrapping
   - Height: Flexible

3. **NodeCost** (TextMeshProUGUI):
   - Font Size: 20
   - Color: Cyan
   - Text: "Cost: 1 XP"

4. **Prerequisites** (TextMeshProUGUI):
   - Font Size: 16
   - Color: Orange
   - Text: "Requires: [nodes]"

5. **Unlocks** (TextMeshProUGUI):
   - Font Size: 16
   - Color: Green
   - Text: "Unlocks: [buildings]"

6. **Modifiers** (TextMeshProUGUI):
   - Font Size: 16
   - Color: Light Blue
   - Enable Word Wrapping
   - Text: "Modifiers:"

**Unlock Button:**
1. Create Button at bottom of panel
2. Name it "UnlockButton"
3. Size: Full width, Height 50
4. Add TextMeshProUGUI child:
   - Name: "ButtonText"
   - Font Size: 20
   - Alignment: Center
   - Text: "Unlock Node"
5. Button Colors:
   - Normal: Green
   - Highlighted: Light Green
   - Pressed: Dark Green
   - Disabled: Dark Gray

### 7. Create Tech Node Item Prefab

Create this as a separate prefab in `Assets/Prefabs/UI/`:

**TechNodeItem Prefab:**
1. Create Panel
2. Name it "TechNodeItem"
3. Size: Full width, Height 80
4. Add **TechNodeUIItem** script component

**Background:**
1. Add Image component (this will be the backgroundImage reference)
2. Color: Gray (will change based on status)

**Node Icon:**
1. Create Image child
2. Position: Left side
3. Size: 64x64
4. Preserve Aspect: true

**Node Name:**
1. Create TextMeshProUGUI child
2. Position: Middle, left of center
3. Font Size: 20
4. Text: "Node Name"

**Status Indicators (create 3):**
1. Create small Image children
2. Names: "UnlockedIndicator", "AvailableIndicator", "LockedIndicator"
3. Position: Right side
4. Size: 24x24
5. Colors:
   - Unlocked: Green checkmark
   - Available: Yellow circle
   - Locked: Red X
6. Initially set all to Active = false

**Select Button:**
1. Add Button component to root panel
2. Make it cover entire item
3. Transition: Color Tint
4. Colors:
   - Normal: Transparent
   - Highlighted: White (alpha 0.1)
   - Pressed: White (alpha 0.2)

**Save as Prefab:**
- Drag to `Assets/Prefabs/UI/TechNodeItem.prefab`

### 8. Hook Up References

Select CultureTechTreeUI GameObject and in the **CultureTechTreeUIController** component:

**Culture Selection:**
- Culture Dropdown → CultureDropdown
- Culture Name Text → CultureName
- Culture Description Text → CultureDescription

**Tree Type Tabs:**
- Economics Tab Button → EconomicsTab
- Military Tab Button → MilitaryTab
- Religion Tab Button → ReligionTab
- Administration Tab Button → AdministrationTab
- Culture Tab Button → CultureTab

**Tree Info:**
- Tree Name Text → TreeName
- Available XP Text → AvailableXP
- Total XP Text → TotalXP
- Owner Realm Text → OwnerRealm

**Node Display:**
- Node List Container → Content (the one inside ScrollView)
- Node Item Prefab → TechNodeItem prefab

**Selected Node Details:**
- Node Details Panel → NodeDetailsPanel
- Node Name Text → NodeName
- Node Description Text → NodeDescription
- Node Cost Text → NodeCost
- Node Prerequisites Text → Prerequisites
- Node Unlocks Text → Unlocks
- Node Modifiers Text → Modifiers
- Unlock Node Button → UnlockButton
- Unlock Button Text → ButtonText (child of UnlockButton)

**Settings:**
- Player Realm ID → Set to your player's realm ID (default 0)

### 9. Hook Up TechNodeItem Prefab References

Open the TechNodeItem prefab and in the **TechNodeUIItem** component:

**Node Display:**
- Node Name Text → NodeName TextMeshProUGUI
- Node Icon → Icon Image
- Background Image → Root Panel Image
- Select Button → Root Panel Button

**Status Indicators:**
- Unlocked Indicator → UnlockedIndicator
- Available Indicator → AvailableIndicator
- Locked Indicator → LockedIndicator

**Colors** (adjust in inspector):
- Unlocked Color → Light Green
- Available Color → Yellow
- Locked Color → Gray

## Styling Tips

### Layout
- Use **Layout Groups** for automatic positioning
- Use **Content Size Fitters** for dynamic sizing
- Use **Scroll Views** for lists that might overflow

### Colors
**Background Panels:**
- Main panel: Dark gray (0.15, 0.15, 0.15, 0.9)
- Sub-panels: Medium gray (0.2, 0.2, 0.2, 0.8)
- Headers: Lighter gray (0.25, 0.25, 0.25, 1)

**Text:**
- Titles: White or Yellow
- Body text: Light gray
- Status indicators: Green/Yellow/Red

**Buttons:**
- Tab buttons: Subtle grays, highlight on select
- Unlock button: Green when available, gray when disabled

### Fonts
- Titles: 24-32pt, Bold
- Body text: 16-18pt
- Small text: 14pt
- Button text: 18-20pt

## Usage

### At Runtime

The UI will automatically:
1. Populate culture dropdown from CultureManager
2. Show all tech trees for the selected culture
3. Display nodes with correct status colors:
   - **Green**: Unlocked
   - **Yellow**: Available (prerequisites met, not unlocked)
   - **Gray**: Locked (prerequisites not met)
4. Enable unlock button only when:
   - Node is available
   - Player controls the tree (owns the culture)
   - Enough XP is available

### Setting Player Realm

In your game initialization code:
```csharp
var techTreeUI = FindObjectOfType<CultureTechTreeUIController>();
techTreeUI.SetPlayerRealmID(playerRealmID);
```

### Selecting a Culture Programmatically

```csharp
var techTreeUI = FindObjectOfType<CultureTechTreeUIController>();
techTreeUI.SelectCulture(greekCulture);
```

## Testing

1. Create at least one CultureData asset:
   - Right-click → Create → TWK → Culture → Culture Data
   - Fill in name, description
   - Create some TechNode assets
   - Assign nodes to a tech tree

2. Assign culture to CultureManager:
   - Select CultureManager in scene
   - Add culture to All Cultures list

3. Create some population groups with cultures:
   - Give them enough XP to unlock nodes

4. Play and test:
   - Select culture from dropdown
   - Switch between tree tabs
   - Click nodes to see details
   - Try unlocking nodes

## Troubleshooting

**Dropdown is empty:**
- Make sure CultureManager has cultures in its list
- Check that cultures are loaded in Start()

**Nodes don't show:**
- Check that tech trees have nodes assigned
- Verify nodeItemPrefab is assigned
- Check that nodes are in the AllNodes list of the tech tree

**Can't unlock nodes:**
- Verify player realm ID matches tree owner
- Check that node prerequisites are met
- Ensure enough XP is available
- Check that node isn't already unlocked

**UI doesn't update:**
- After unlocking, the UI auto-refreshes
- If not, check console for errors
- Verify all references are hooked up correctly
