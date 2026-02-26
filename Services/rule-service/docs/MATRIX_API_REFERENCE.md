# Matrix API - Complete Endpoint Reference

## ✅ Implemented Endpoints

### 1. **Get Matrix Choices** (Multi-Option Evaluation)
```http
GET /api/v1/matrices/{name}/choices?uprightId={id}&span={mm}&load={kg}
```

**Purpose**: Get all valid beam profiles ranked by utilization efficiency

**Parameters**:
- `name` (path): Matrix name (e.g., "BeamChart")
- `uprightId` (query): Upright profile ID (e.g., "ST20")
- `span` (query): Beam span in mm (e.g., 2750)
- `load` (query): Required load in kg (e.g., 1500)

**Response** (200 OK):
```json
[
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

**Features**:
- ✅ Automatic interpolation (works for any span value)
- ✅ Pre-calculated utilization percentage
- ✅ Sorted by efficiency (lowest utilization first)
- ✅ Safety indicator (isSafe = utilization <= 100%)

**Frontend Usage**:
```javascript
const response = await fetch(
  `/api/v1/matrices/BeamChart/choices?uprightId=ST20&span=2750&load=1500`
);
const choices = await response.json();

// choices[0] is the most efficient option
// choices.filter(c => c.isSafe) gives only safe options
```

---

### 2. **Lookup Single Value** (Interpolation)
```http
GET /api/v1/matrices/{name}/lookup?path={p1}&path={p2}&path={p3}&value={num}
```

**Purpose**: Get interpolated capacity for a specific profile and span

**Parameters**:
- `name` (path): Matrix name
- `path` (query, multiple): Path segments (e.g., `path=uprights&path=ST20&path=HEM_80`)
- `value` (query, optional): Numerical value for interpolation (e.g., span in mm)

**Response** (200 OK):
```json
{
  "value": 1900
}
```

**Example**:
```http
GET /api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2750
```

Returns interpolated capacity for HEM_80 beam at 2750mm span.

**Frontend Usage**:
```javascript
const response = await fetch(
  `/api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2750`
);
const { value } = await response.json();
console.log(`Capacity: ${value} kg`);
```

---

### 3. **Get Full Matrix**
```http
GET /api/v1/matrices/{name}
```

**Purpose**: Retrieve complete matrix data (for admin/debugging)

**Response** (200 OK):
```json
{
  "id": "uuid",
  "name": "BeamChart",
  "category": "LOAD_CHART",
  "dataJson": "{\"uprights\": {...}}",
  "version": 5,
  "createdAt": "2026-01-25T00:00:00Z",
  "updatedAt": "2026-01-25T06:00:00Z"
}
```

**Use Case**: Admin panel to view/edit matrix structure

---

### 4. **Update Matrix Cell**
```http
PATCH /api/v1/matrices/{id}/cell
```

**Purpose**: Update a specific value in the matrix (admin only)

**Request Body**:
```json
{
  "path": ["uprights", "ST20", "HEM_80", "0", "Y"],
  "value": 2100
}
```

**Response**: 204 No Content

**Example**: Update capacity for HEM_80 at first span point to 2100kg

---

### 5. **Get Rule Manifest** (Includes Matrix Metadata)
```http
GET /api/v1/rules/manifest?productGroupId={guid}&countryId={guid}
```

**Purpose**: Get unified bundle of rules + matrix metadata

**Response** (200 OK):
```json
{
  "version": "20260125.0644",
  "productGroupId": "...",
  "countryId": "...",
  "rules": [...],
  "matrices": [
    {
      "name": "BeamChart",
      "category": "LOAD_CHART",
      "version": 5
    }
  ],
  "generatedAt": "2026-01-25T06:44:00Z"
}
```

**Use Case**: Initial load to discover available matrices and rules

---

## 🎯 Recommended Frontend Flow

### Step 1: Load Manifest (Once on App Start)
```javascript
const manifest = await fetch(
  `/api/v1/rules/manifest?productGroupId=${pgId}&countryId=${cId}`
).then(r => r.json());

console.log('Available matrices:', manifest.matrices);
// Store manifest.version for cache invalidation
```

### Step 2: Get Beam Choices (When User Changes Inputs)
```javascript
// User changes upright, span, or load
const choices = await fetch(
  `/api/v1/matrices/BeamChart/choices?uprightId=${upright}&span=${span}&load=${load}`
).then(r => r.json());

// Update dropdown with ranked options
setBeamOptions(choices);
```

### Step 3: Show Utilization in UI
```jsx
<select value={selectedBeam} onChange={handleBeamChange}>
  {choices.map(choice => (
    <option key={choice.choiceId} value={choice.choiceId}>
      {choice.choiceId} - {choice.utilization.toFixed(1)}% utilized
      {choice.utilization < 60 && ' ⚡'}
      {choice.utilization > 90 && ' ⚠️'}
    </option>
  ))}
</select>
```

---

## 📊 API Comparison

| Endpoint | Purpose | Interpolation | Returns |
|----------|---------|---------------|---------|
| `/choices` | Get all valid options | ✅ Yes | Array of choices with utilization |
| `/lookup` | Get single value | ✅ Yes | Single capacity value |
| `/{name}` | Get full matrix | ❌ No | Complete JSONB data |
| `/manifest` | Get metadata | ❌ No | List of available matrices |

---

## 🚀 Performance Tips

### 1. Cache Manifest
```javascript
// Cache manifest in localStorage
const cachedVersion = localStorage.getItem('manifestVersion');
const serverVersion = manifest.version;

if (cachedVersion !== serverVersion) {
  localStorage.setItem('manifest', JSON.stringify(manifest));
  localStorage.setItem('manifestVersion', serverVersion);
}
```

### 2. Debounce Choices API
```javascript
import { debounce } from 'lodash';

const fetchChoices = debounce(async (upright, span, load) => {
  const choices = await fetch(`/api/v1/matrices/BeamChart/choices?...`);
  // Update UI
}, 300); // Wait 300ms after user stops typing
```

### 3. Prefetch Common Scenarios
```javascript
// Prefetch for common upright types
const commonUprights = ['ST20', 'ST25', 'ST30'];
commonUprights.forEach(upright => {
  fetch(`/api/v1/matrices/BeamChart/choices?uprightId=${upright}&span=2700&load=1000`);
});
```

---

## ✅ Summary

**Yes, the API fully supports the `/api/v1/matrices/BeamChart/choices` endpoint!**

All the endpoints mentioned in the migration guide are **implemented and working**:
- ✅ `/api/v1/matrices/{name}/choices` - Multi-option evaluation
- ✅ `/api/v1/matrices/{name}/lookup` - Single value lookup
- ✅ `/api/v1/matrices/{name}` - Full matrix retrieval
- ✅ `/api/v1/matrices/{id}/cell` - Cell updates
- ✅ `/api/v1/rules/manifest` - Unified manifest

The architecture is **production-ready** and supports **unlimited matrix types** beyond just beam load charts.
