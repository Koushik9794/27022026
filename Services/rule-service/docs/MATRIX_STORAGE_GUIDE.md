# Matrix Storage & Retrieval - Complete Guide

## 🔑 Key Concept: Name-Based Access

**Yes, the `name` is the primary key** for retrieving matrix data. Once you persist a load chart with a specific name, you can retrieve:
- ✅ Single values (specific cell)
- ✅ Multiple values (entire profile)
- ✅ Entire matrix (all data)
- ✅ Interpolated values (calculated on-the-fly)

---

## 📊 Example: BeamChart Matrix

### Step 1: Persist the Load Chart

```csharp
// Admin creates/updates the BeamChart
var beamChart = LookupMatrix.Create(
    name: "BeamChart",  // ← This is the KEY
    category: "LOAD_CHART",
    dataJson: @"{
        ""uprights"": {
            ""ST20"": {
                ""HEM_80"": [
                    { ""X"": 2700, ""Y"": 2000 },
                    { ""X"": 2800, ""Y"": 1800 },
                    { ""X"": 2900, ""Y"": 1600 }
                ],
                ""HEM_100"": [
                    { ""X"": 2700, ""Y"": 3000 },
                    { ""X"": 2800, ""Y"": 2800 },
                    { ""X"": 2900, ""Y"": 2600 }
                ],
                ""HEM_120"": [
                    { ""X"": 2700, ""Y"": 4000 },
                    { ""X"": 2800, ""Y"": 3800 }
                ]
            },
            ""ST25"": {
                ""HEM_80"": [...],
                ""HEM_100"": [...]
            }
        }
    }"
);

await repository.SaveAsync(beamChart);
```

**Database Storage**:
```sql
INSERT INTO lookup_matrices (id, name, category, data, version)
VALUES (
    'uuid',
    'BeamChart',  -- ← UNIQUE KEY
    'LOAD_CHART',
    '{"uprights": {...}}'::jsonb,
    1
);
```

---

## 🎯 Retrieval Patterns

### Pattern 1: Get Single Value (Specific Cell)

**Scenario**: Get capacity for HEM_80 at span 2700mm

```http
GET /api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2700
```

**SQL Executed**:
```sql
SELECT data #> '{uprights, ST20, HEM_80}' 
FROM lookup_matrices 
WHERE name = 'BeamChart';
```

**Response**:
```json
{
  "value": 2000
}
```

**Explanation**:
- Uses `name` ("BeamChart") to find the matrix
- Uses `path` to navigate JSONB tree
- Returns exact value at that path

---

### Pattern 2: Get Interpolated Value (Calculated)

**Scenario**: Get capacity for HEM_80 at span **2750mm** (not in data!)

```http
GET /api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2750
```

**Backend Logic**:
```csharp
// 1. Fetch data points for HEM_80
var dataPoints = [
    { X: 2700, Y: 2000 },
    { X: 2800, Y: 1800 }
];

// 2. Find surrounding points
var p1 = { X: 2700, Y: 2000 };
var p2 = { X: 2800, Y: 1800 };

// 3. Linear interpolation
var targetX = 2750;
var interpolatedY = p1.Y + (p2.Y - p1.Y) * (targetX - p1.X) / (p2.X - p1.X);
// = 2000 + (1800 - 2000) * (2750 - 2700) / (2800 - 2700)
// = 2000 + (-200) * (50) / (100)
// = 2000 - 100
// = 1900
```

**Response**:
```json
{
  "value": 1900
}
```

**Key Point**: The value **1900 doesn't exist in the database**, it's calculated!

---

### Pattern 3: Get Multiple Values (All Profiles)

**Scenario**: Get all beam profiles for ST20 with utilization

```http
GET /api/v1/matrices/BeamChart/choices?uprightId=ST20&span=2750&load=1500
```

**Backend Logic**:
```csharp
// 1. Fetch parent node (all profiles for ST20)
var st20Data = {
    "HEM_80": [...],
    "HEM_100": [...],
    "HEM_120": [...]
};

// 2. For EACH profile, calculate capacity at span 2750
var results = new List<MatrixChoiceResult>();

foreach (var profile in st20Data)
{
    var capacity = PerformInterpolation(profile.Value, 2750);
    var utilization = (1500 / capacity) * 100;
    
    results.Add(new MatrixChoiceResult {
        ChoiceId = profile.Key,
        Capacity = capacity,
        Utilization = utilization,
        IsSafe = utilization <= 100
    });
}

// 3. Sort by utilization (most efficient first)
return results.OrderBy(r => r.Utilization);
```

**Response**:
```json
[
  {
    "choiceId": "HEM_120",
    "capacity": 3900,
    "utilization": 38.46,
    "isSafe": true
  },
  {
    "choiceId": "HEM_100",
    "capacity": 2900,
    "utilization": 51.72,
    "isSafe": true
  },
  {
    "choiceId": "HEM_80",
    "capacity": 1900,
    "utilization": 78.95,
    "isSafe": true
  }
]
```

---

### Pattern 4: Get Entire Matrix (Admin)

**Scenario**: Admin wants to view/edit the complete chart

```http
GET /api/v1/matrices/BeamChart
```

**SQL Executed**:
```sql
SELECT id, name, category, data, version, created_at, updated_at
FROM lookup_matrices
WHERE name = 'BeamChart';
```

**Response**:
```json
{
  "id": "uuid",
  "name": "BeamChart",
  "category": "LOAD_CHART",
  "dataJson": "{\"uprights\": {\"ST20\": {...}, \"ST25\": {...}}}",
  "version": 5,
  "createdAt": "2026-01-25T00:00:00Z",
  "updatedAt": "2026-01-25T06:00:00Z"
}
```

---

## 🗂️ Multiple Matrix Types

You can have **multiple matrices** with different names:

```sql
-- Beam Load Chart
INSERT INTO lookup_matrices (name, category, data) VALUES (
    'BeamChart', 
    'LOAD_CHART', 
    '{"uprights": {...}}'::jsonb
);

-- Seismic Factors
INSERT INTO lookup_matrices (name, category, data) VALUES (
    'SeismicFactors', 
    'SAFETY', 
    '{"zones": {"Zone_A": [...], "Zone_B": [...]}}'::jsonb
);

-- Price Table
INSERT INTO lookup_matrices (name, category, data) VALUES (
    'PriceTable', 
    'PRICING', 
    '{"retail": [...], "wholesale": [...]}'::jsonb
);

-- Anchor Compatibility
INSERT INTO lookup_matrices (name, category, data) VALUES (
    'AnchorChart', 
    'COMPATIBILITY', 
    '{"concrete": {"C20": {...}, "C30": {...}}}'::jsonb
);
```

**Access Each by Name**:
```javascript
// Beam capacity
const beamData = await fetch('/api/v1/matrices/BeamChart/choices?...');

// Seismic factor
const seismicData = await fetch('/api/v1/matrices/SeismicFactors/lookup?path=zones&path=Zone_A&value=8000');

// Price
const priceData = await fetch('/api/v1/matrices/PriceTable/lookup?path=retail&value=50');

// Anchor compatibility
const anchorData = await fetch('/api/v1/matrices/AnchorChart/lookup?path=concrete&path=C30&path=M12');
```

---

## 🔍 Path Navigation Examples

### Example 1: Simple 1D Lookup (Seismic)
```json
{
  "zones": {
    "Zone_A": [
      { "X": 5000, "Y": 1.2 },
      { "X": 10000, "Y": 1.5 }
    ]
  }
}
```

**Lookup**:
```http
GET /api/v1/matrices/SeismicFactors/lookup?path=zones&path=Zone_A&value=7500
```

**Path**: `zones → Zone_A → interpolate(7500)`  
**Result**: `1.35` (interpolated between 1.2 and 1.5)

---

### Example 2: Deep 3D Lookup (Beam)
```json
{
  "uprights": {
    "ST20": {
      "HEM_80": [
        { "X": 2700, "Y": 2000 }
      ]
    }
  }
}
```

**Lookup**:
```http
GET /api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2700
```

**Path**: `uprights → ST20 → HEM_80 → find(2700)`  
**Result**: `2000`

---

### Example 3: Categorical Lookup (No Interpolation)
```json
{
  "concrete": {
    "C30": {
      "M12": {
        "maxLoad": 7000,
        "approved": true
      }
    }
  }
}
```

**Lookup**:
```http
GET /api/v1/matrices/AnchorChart/lookup?path=concrete&path=C30&path=M12
```

**Path**: `concrete → C30 → M12`  
**Result**: `{ "maxLoad": 7000, "approved": true }`

---

## 🎨 Visual Representation

```
Database Table: lookup_matrices
┌──────────────┬──────────────┬──────────────┬─────────────────────────┐
│ name         │ category     │ version      │ data (JSONB)            │
├──────────────┼──────────────┼──────────────┼─────────────────────────┤
│ BeamChart    │ LOAD_CHART   │ 5            │ {"uprights": {...}}     │ ← KEY
│ SeismicFactors│ SAFETY      │ 2            │ {"zones": {...}}        │ ← KEY
│ PriceTable   │ PRICING      │ 10           │ {"retail": [...]}       │ ← KEY
└──────────────┴──────────────┴──────────────┴─────────────────────────┘

Access Pattern:
┌─────────────────────────────────────────────────────────────────┐
│ Client Request                                                  │
│ GET /api/v1/matrices/BeamChart/choices?uprightId=ST20&...      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Backend Query                                                   │
│ SELECT data #> '{uprights, ST20}' FROM lookup_matrices         │
│ WHERE name = 'BeamChart'                                        │
│                    ↑                                            │
│                    └─ Uses NAME as key                          │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ JSONB Path Navigation                                           │
│ data → uprights → ST20 → {HEM_80: [...], HEM_100: [...]}       │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Interpolation + Calculation                                     │
│ For each profile: Interpolate capacity at span 2750            │
│ Calculate utilization = (load / capacity) * 100                │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Response                                                        │
│ [                                                               │
│   { choiceId: "HEM_100", capacity: 2900, utilization: 51.72 }, │
│   { choiceId: "HEM_80", capacity: 1900, utilization: 78.95 }   │
│ ]                                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## ✅ Summary

**Yes, your understanding is 100% correct!**

1. **Name is the key**: `"BeamChart"`, `"SeismicFactors"`, etc.
2. **Single value**: Use `/lookup` with specific path
3. **Multiple values**: Use `/choices` to get all options at a level
4. **Entire matrix**: Use `/{name}` to get everything
5. **Interpolation**: Automatic for numerical lookups
6. **Flexibility**: Works for any chart structure (1D, 2D, 3D, categorical)

The architecture is **universal** - it can handle:
- ✅ Beam load charts (3D: Upright × Span × Profile)
- ✅ Seismic factors (2D: Zone × Height)
- ✅ Price tables (2D: Quantity × Customer Type)
- ✅ Anchor compatibility (Categorical: Concrete × Anchor)
- ✅ Any future chart type you need!

All accessed by **name** as the primary key.
