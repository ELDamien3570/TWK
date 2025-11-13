# Population System Refactor - Documentation

## Overview

The population system has been refactored from struct-based to class-based architecture to support:
- **Rich demographics** (age, gender distribution)
- **Social mobility** (education-driven promotions/demotions)
- **Labor system** (employment tracking, worker availability)
- **Cultural properties** (fervor, religion, loyalty)
- **Military recruitment** (tracking eligible population)

---

## Architecture

### **1. PopulationGroup** (Class)
**File:** `PopulationGroups.cs`

The core class representing a population group within a city.

#### Key Properties:

**Demographics:**
- `AgeDistribution Demographics` - Age and gender breakdown
- `int PopulationCount` - Total population (derived from demographics)
- `float AverageAge` - Average age of the group
- `float MalePercentage` - Percentage of males (0-100)
- `float FemalePercentage` - Percentage of females (0-100)

**Economic:**
- `float Wealth` - Wealth level (0-100), affects promotion/demotion
- `float Education` - Education level (0-100), primary driver of promotions
- `int EmployedCount` - Number employed in buildings
- `int UnemployedCount` - Available workers minus employed

**Labor:**
- `int AvailableWorkers` - Working age population (18-60)
- `int MilitaryEligible` - Military recruitment pool (adult males 18-45)

**Cultural:**
- `float Fervor` - Religious conviction/resistance to conversion (0-100)
- `Religion CurrentReligion` - Current religion
- `float Loyalty` - Loyalty to the state (-100 to 100)

**Legacy:**
- `float Happiness` - Overall satisfaction (0-1)
- `float GrowthModifier` - Growth rate multiplier

---

### **2. AgeDistribution** (Class)
**File:** `AgeDistribution.cs`

Tracks age and gender distribution for realistic demographic simulation.

#### Age Brackets:
- **Youth (0-17):** Not working age, not military eligible
- **Adult (18-45):** Prime working and military age
- **Middle (46-60):** Working age, not military eligible
- **Elderly (60+):** Not working, not military eligible

#### Key Methods:
- `CreateNormalized(totalPop, avgAge)` - Create realistic distribution
- `AdvanceYear()` - Age population by one year
- `ApplyCasualties(count)` - Apply war casualties (removes adult males)
- `AddBirths(count)` - Add new births (young males/females)
- `Scale(multiplier)` - Scale entire distribution for growth/decline

#### Why This Approach?
- **War Impact:** When men go to war and don't return, the adult male population decreases, creating long-term demographic effects on birth rates and labor force
- **Clean Abstraction:** No need to track individual people, just age/gender buckets
- **Realistic Dynamics:** Birth rates depend on breeding-age females, worker availability depends on working-age distribution

---

### **3. PopulationManager** (MonoBehaviour)
**File:** `PopulationManager.cs`

Manages all population groups and handles simulation logic.

#### New Methods:
- `GetAvailableWorkersByCity(cityId)` - Get total available workers
- `GetMilitaryEligibleByCity(cityId)` - Get total military recruitment pool

#### Simulation:
**Daily (`AdvanceDay`):**
- Population growth based on food, happiness, carrying capacity
- Happiness decay
- Education growth (baseline + buildings/policies)
- Wealth growth (for employed populations)

**Yearly (`AdvanceYear`):**
- Age demographics (shift age brackets)
- Check social mobility (promotions/demotions)

---

### **4. Social Mobility System**

#### Rigid Hierarchy:
```
Slave (0) → Laborer (1) → Artisan (2) → Merchant (3) → Noble (4)
              ↓
            Clergy (special branch)
```

#### Promotion Requirements:
| From       | To       | Education | Wealth | Base Chance/Year |
|------------|----------|-----------|--------|------------------|
| Slave      | -        | -         | -      | 0% (policy only) |
| Laborer    | Artisan  | 40+       | 30+    | 5%               |
| Artisan    | Merchant | 60+       | 50+    | 5%               |
| Merchant   | Noble    | 80+       | 75+    | 5%               |
| Noble      | -        | -         | -      | (top tier)       |

#### Demotion Conditions:
| From     | To       | Trigger                        | Base Chance/Year |
|----------|----------|--------------------------------|------------------|
| Noble    | Merchant | Wealth < 20                    | 2%               |
| Merchant | Artisan  | Wealth < 20                    | 2%               |
| Artisan  | Laborer  | Wealth < 20                    | 2%               |
| Laborer  | Slave    | Policy only (default 0%)       | 0%               |
| Clergy   | Laborer  | Wealth < 15 OR Fervor < 30     | 2%               |

#### How It Works:
1. **Annual Check:** Each year, every population group is evaluated
2. **Eligibility:** Must meet education + wealth thresholds
3. **Chance Roll:** Base chance modified by:
   - Education bonus: Higher education = faster promotion
   - Wealth bonus: Higher wealth = faster promotion
   - Happiness bonus: Happier pops promote faster
4. **Population Split:**
   - 20% of eligible population promotes (creates/merges with target archetype group)
   - 15% of struggling population demotes
   - Demographics transferred proportionally

#### Future Policy Hooks:
- **Emancipation Policy:** Allow slaves to promote to laborers
- **Enslavement Policy:** Allow laborers to demote to slaves (moral/stability penalties)
- **Education Reforms:** Increase education growth rates
- **Social Programs:** Modify promotion/demotion chances

---

### **5. ArchetypeDefinition** (ScriptableObject)
**File:** `ArchetypeDefinition.cs`

Defines rules, requirements, and behavior for each archetype. This allows designers to tune balance via Unity Inspector.

#### Key Properties:
- **Hierarchy:** Social tier, promotion/demotion paths
- **Promotion:** Education/wealth thresholds, base chance
- **Demotion:** Poverty threshold, base chance
- **Economic:** Wealth income, education growth, consumption multiplier
- **Growth:** Base growth rate, typical age
- **Cultural:** Base fervor, loyalty modifier, stability impact
- **Labor:** Work restrictions, efficiency multiplier

#### Example Values (from code defaults):

**Slaves:**
- Wealth: 5, Education: 5, Loyalty: -30
- Cannot promote (policy required)
- Cause negative stability

**Laborers:**
- Wealth: 25, Education: 20, Loyalty: 0
- Highest growth rate (0.0008)
- General labor

**Artisans:**
- Wealth: 50, Education: 45, Loyalty: +10
- Skilled workers
- Higher efficiency

**Merchants:**
- Wealth: 75, Education: 70, Loyalty: +20
- Commercial focus
- Luxury buildings

**Nobles:**
- Wealth: 90, Education: 85, Loyalty: +50
- Government buildings only
- High loyalty, low growth

**Clergy:**
- Wealth: 60, Education: 80, Loyalty: +30
- Religious buildings
- Can demote if fervor drops

---

## Integration with Building System

### Future Enhancements Needed:

**BuildingData should add:**
```csharp
[Header("Workforce Requirements")]
public Dictionary<PopulationArchetypes, int> RequiredWorkers;
public Dictionary<PopulationArchetypes, int> OptimalWorkers;
public Dictionary<PopulationArchetypes, float> EfficiencyByArchetype;
```

**BuildingInstance should add:**
```csharp
public Dictionary<PopulationArchetypes, int> AssignedWorkers;

public float CalculateEfficiency()
{
    // Base efficiency * worker efficiency
    // Return 0-1 based on assigned vs required workers
}
```

**Worker Allocation (Automatic):**
1. Each day, BuildingManager requests workers from PopulationManager
2. PopulationManager allocates available workers by priority:
   - Buildings with requirements not met
   - Buildings with optimal workers not reached
3. Workers are marked as employed (EmployedCount updated)
4. Unemployed workers consume resources but produce nothing

---

## Military Recruitment Integration

### How to Use:

```csharp
// Get military eligible population for a city
int recruitmentPool = PopulationManager.Instance.GetMilitaryEligibleByCity(cityId);

// When recruiting units
int recruits = 1000;
foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(cityId))
{
    // Recruit proportionally from each group
    int toRecruit = Mathf.RoundToInt(recruits * (pop.MilitaryEligible / (float)recruitmentPool));

    // Apply to demographics (removes adult males)
    pop.Demographics.ApplyCasualties(toRecruit);
}
```

### War Casualties:
When units die in battle:
```csharp
// Apply casualties back to population demographics
pop.Demographics.ApplyCasualties(casualties);
```

This creates realistic demographic holes:
- Fewer breeding-age males = lower birth rates
- Fewer working-age males = reduced labor force
- Takes a generation to recover

---

## Migration Path from Old System

### Breaking Changes:
✅ **FIXED** - `PopulationGroups` (struct) → `PopulationGroup` (class)
✅ **FIXED** - Property names: `populationCount` → `PopulationCount`, `archetype` → `Archetype`
✅ **FIXED** - `RegisterPopulation()` now has optional `averageAge` parameter (default 30f)
✅ **FIXED** - `ModifyPopulation()` removed (not needed for classes, direct property access)

### Backward Compatibility:
✅ All existing code using `RegisterPopulation(cityId, archetype, count)` works unchanged
✅ `GetPopulationsByCity()` returns `IEnumerable<PopulationGroup>` instead of struct
✅ `GetIntPopulationByCity()` works identically
✅ Existing growth/happiness simulation intact

---

## Testing & Validation

### Test Checklist:
- [ ] Cities start with population groups
- [ ] Population grows/declines based on food
- [ ] Demographics age over time (run simulation for years)
- [ ] Promotions occur when education/wealth thresholds met
- [ ] Demotions occur when wealth falls
- [ ] Available workers calculated correctly
- [ ] Military recruitment reduces adult male population
- [ ] War casualties create demographic imbalances
- [ ] Birth rates affected by female population

### Debug Commands (add to SimulationManager):
```csharp
// Force a year to pass (test aging/mobility)
WorldTimeManager.Instance.AdvanceYear();

// Check population stats
foreach (var pop in PopulationManager.Instance.GetPopulationsByCity(cityId))
{
    Debug.Log($"{pop.Archetype}: Pop={pop.PopulationCount}, Education={pop.Education}, " +
              $"Wealth={pop.Wealth}, Workers={pop.AvailableWorkers}, Military={pop.MilitaryEligible}");
}
```

---

## Future Enhancements

### Phase 2: Labor System
- [ ] Add worker requirements to BuildingData
- [ ] Implement automatic worker allocation
- [ ] Add unemployment penalties (unrest)
- [ ] Building efficiency based on workers assigned
- [ ] Wages abstracted into building upkeep/income

### Phase 3: Policies
- [ ] Emancipation policy (slaves → laborers)
- [ ] Enslavement policy (laborers → slaves, stability penalty)
- [ ] Education reforms (increase education growth)
- [ ] Social programs (modify promotion chances)
- [ ] Cultural policies (affect fervor, loyalty)

### Phase 4: Religion & Culture
- [ ] Religion conversion mechanics (driven by fervor)
- [ ] Cultural identity (different cultures = different loyalties)
- [ ] Religious buildings (affect fervor)
- [ ] Cultural assimilation over time

### Phase 5: Advanced Demographics
- [ ] Migration between cities (pull factors: employment, wealth)
- [ ] Refugees from war/disasters
- [ ] Disease/plagues (age-selective mortality)
- [ ] Famine (affects elderly/young first)

---

## Summary

The population system now supports:
✅ **Demographics:** Age/gender distribution, realistic aging
✅ **Social Mobility:** Education-driven class changes
✅ **Labor:** Worker availability, employment tracking
✅ **Military:** Recruitment pools, war casualties, demographic impact
✅ **Culture:** Fervor, religion, loyalty
✅ **Extensibility:** Ready for buildings, policies, and advanced features

The rigid social hierarchy (Slave → Laborer → Artisan → Merchant → Noble) creates interesting gameplay:
- Players must invest in education to grow skilled workforce
- Economic prosperity drives social mobility
- War has long-term demographic consequences
- Slavery creates moral trade-offs (labor vs. stability)
- Class structure affects building efficiency and government

**Next Steps:** Integrate with building system to require workers for production!
