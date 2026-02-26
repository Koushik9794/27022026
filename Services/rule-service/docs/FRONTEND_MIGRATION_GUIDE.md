# Frontend Migration Guide: From Static Excel to Dynamic Matrix API

## Current Architecture Problems

### ❌ Issues with Current Approach

```javascript
// PROBLEM 1: Single hardcoded Excel file
const response = await fetch('/Load_Chart.xlsx');

// PROBLEM 2: Assumes specific column structure
// Expected: [Upright, BeamSpan, BeamProfile1, BeamProfile2, ...]

// PROBLEM 3: No support for multiple chart types
// What about: Seismic charts? Price tables? Anchor charts?

// PROBLEM 4: Manual parsing logic
const jsonData = utils.sheet_to_json(sheet, { header: 1 });

// PROBLEM 5: No versioning or cache invalidation
if (loadChartData) return loadChartData; // Stale forever!

// PROBLEM 6: Hardcoded normalization
const normalize = (str) => str.toString().toUpperCase().replace(/[\s\-_]/g, '');
```

---

## ✅ New Architecture Advantages

### Universal Matrix Support

The new backend architecture supports **ANY** type of load chart through JSONB:

```json
// BEAM LOAD CHART (3D: Upright × Span × Profile)
{
  "uprights": {
    "ST20": {
      "HEM_80": [
        { "X": 2700, "Y": 2000 },
        { "X": 2800, "Y": 1800 }
      ],
      "HEM_100": [...]
    },
    "ST25": {...}
  }
}

// SEISMIC FACTOR CHART (2D: Zone × Height)
{
  "zones": {
    "Zone_A": [
      { "X": 5000, "Y": 1.2 },
      { "X": 10000, "Y": 1.5 }
    ],
    "Zone_B": [...]
  }
}

// PRICE TABLE (2D: Quantity × Customer Type)
{
  "pricing": {
    "Retail": [
      { "X": 1, "Y": 100 },
      { "X": 10, "Y": 90 },
      { "X": 100, "Y": 75 }
    ],
    "Wholesale": [...]
  }
}

// ANCHOR COMPATIBILITY (Categorical: Concrete Grade × Anchor Type)
{
  "concrete": {
    "C20": {
      "M10": { "maxLoad": 5000, "approved": true },
      "M12": { "maxLoad": 7000, "approved": true }
    },
    "C30": {...}
  }
}
```

---

## 🔄 Migration Strategy

### Phase 1: Parallel Operation (Recommended)

Keep your existing Excel code while adding new API client:

```javascript
// services/matrixService.js
import { read, utils } from 'xlsx';

// LEGACY: Keep for fallback
let loadChartData = null;

// NEW: API-based matrix service
class MatrixService {
    constructor() {
        this.manifest = null;
        this.cache = new Map();
    }

    /**
     * Fetch manifest from backend (replaces Excel loading)
     */
    async loadManifest(productGroupId, countryId) {
        if (this.manifest) return this.manifest;

        try {
            const response = await fetch(
                `/api/v1/rules/manifest?productGroupId=${productGroupId}&countryId=${countryId}`
            );
            
            if (!response.ok) {
                console.warn('Manifest API failed, falling back to Excel');
                return this.loadBeamCapacityDataLegacy(); // Fallback
            }

            this.manifest = await response.json();
            console.log(`[Matrix] Loaded manifest v${this.manifest.version}`);
            console.log(`[Matrix] Available matrices:`, this.manifest.matrices.map(m => m.name));
            
            return this.manifest;
        } catch (error) {
            console.error('Failed to load manifest:', error);
            return this.loadBeamCapacityDataLegacy(); // Fallback
        }
    }

    /**
     * Get valid beam profiles with utilization ranking
     * NEW: Uses backend interpolation and multi-option evaluation
     */
    async getValidBeamProfiles(uprightId, beamSpan, requiredLoad) {
        try {
            const response = await fetch(
                `/api/v1/matrices/BeamChart/choices?uprightId=${uprightId}&span=${beamSpan}&load=${requiredLoad}`
            );

            if (!response.ok) {
                console.warn('Choices API failed, falling back to legacy');
                return this.getValidBeamProfilesLegacy(uprightId, beamSpan, requiredLoad);
            }

            const choices = await response.json();
            
            // Backend returns:
            // [
            //   { choiceId: "HEM_100", capacity: 2900, utilization: 51.72, isSafe: true },
            //   { choiceId: "HEM_80", capacity: 1900, utilization: 78.95, isSafe: true }
            // ]

            console.log(`[Matrix] Found ${choices.length} valid beams for ${uprightId} @ ${beamSpan}mm`);
            
            // Map to your BEAM_PROFILES format
            return choices
                .filter(c => c.isSafe)
                .map(c => ({
                    ...BEAM_PROFILES.find(p => p.id === c.choiceId),
                    capacity: c.capacity,
                    utilization: c.utilization
                }));

        } catch (error) {
            console.error('Error fetching beam choices:', error);
            return this.getValidBeamProfilesLegacy(uprightId, beamSpan, requiredLoad);
        }
    }

    /**
     * Get optimal beam (backend does the optimization)
     */
    async getOptimalBeamProfile(uprightId, beamSpan, requiredLoad) {
        const choices = await this.getValidBeamProfiles(uprightId, beamSpan, requiredLoad);
        
        if (!choices || choices.length === 0) return null;

        // Backend already sorted by utilization (lowest first = most efficient)
        // Prefer HEM series
        const hemChoice = choices.find(c => c.id.toLowerCase().startsWith('hem'));
        if (hemChoice) return hemChoice.id;

        // Otherwise, return first (most efficient)
        return choices[0].id;
    }

    /**
     * LEGACY: Keep for fallback
     */
    async loadBeamCapacityDataLegacy() {
        // Your existing Excel parsing code
        if (loadChartData) return loadChartData;
        // ... existing implementation
    }

    getValidBeamProfilesLegacy(uprightId, beamSpan, requiredLoad) {
        // Your existing filtering logic
    }
}

export const matrixService = new MatrixService();
```

---

## 🎯 Key Improvements

### 1. **No More Excel Parsing**
```javascript
// OLD: Parse Excel every time
const arrayBuffer = await response.arrayBuffer();
const workbook = read(arrayBuffer, { type: 'array' });
const jsonData = utils.sheet_to_json(sheet, { header: 1 });

// NEW: Get pre-processed JSON
const choices = await fetch('/api/v1/matrices/BeamChart/choices?...');
```

### 2. **Automatic Interpolation**
```javascript
// OLD: Only works for exact span matches
const entry = loadChartData.find(d => Math.abs(d.beamSpan - beamSpan) < 10);

// NEW: Backend interpolates for ANY span
// Example: Span 2750mm interpolated from 2700mm and 2800mm data points
const choices = await matrixService.getValidBeamProfiles('ST20', 2750, 1500);
// Returns accurate capacity even though 2750 isn't in the chart!
```

### 3. **Pre-Calculated Utilization**
```javascript
// OLD: You calculate utilization manually
const capacity = entry.capacities[profileId];
const utilization = (requiredLoad / capacity) * 100;

// NEW: Backend returns utilization
const choices = await fetch('/api/v1/matrices/BeamChart/choices?...');
// [{ choiceId: "HEM_100", capacity: 2900, utilization: 51.72, isSafe: true }]
```

### 4. **Multi-Chart Support**
```javascript
// OLD: Only one hardcoded chart
const response = await fetch('/Load_Chart.xlsx');

// NEW: Multiple charts from manifest
const manifest = await matrixService.loadManifest(productGroupId, countryId);
// manifest.matrices = [
//   { name: "BeamChart", category: "LOAD_CHART", version: 5 },
//   { name: "SeismicFactors", category: "SAFETY", version: 2 },
//   { name: "PriceTable", category: "PRICING", version: 10 }
// ]

// Use different charts for different purposes
const beamChoices = await fetch('/api/v1/matrices/BeamChart/choices?...');
const seismicFactor = await fetch('/api/v1/matrices/SeismicFactors/lookup?zone=A&height=8000');
const price = await fetch('/api/v1/matrices/PriceTable/lookup?qty=50&type=Retail');
```

### 5. **Version-Based Cache Invalidation**
```javascript
// OLD: Cache forever (stale data!)
if (loadChartData) return loadChartData;

// NEW: Version header from backend
const manifest = await fetch('/api/v1/rules/manifest?...');
// Response headers: X-GSS-Rule-Version: 20260125.0644

// Interceptor detects version change
axios.interceptors.response.use(response => {
    const serverVersion = response.headers['x-gss-rule-version'];
    const localVersion = localStorage.getItem('manifestVersion');
    
    if (serverVersion !== localVersion) {
        console.log('New manifest version detected, reloading...');
        matrixService.manifest = null; // Clear cache
        matrixService.loadManifest(productGroupId, countryId);
        localStorage.setItem('manifestVersion', serverVersion);
    }
    
    return response;
});
```

---

## 📊 Comparison Table

| Feature | Current (Excel) | New (Matrix API) |
|---------|----------------|------------------|
| **Data Source** | Static `/Load_Chart.xlsx` | Dynamic `/api/v1/matrices/{name}` |
| **Chart Types** | 1 (Beam Load) | Unlimited (Beam, Seismic, Price, etc.) |
| **Interpolation** | ❌ No (requires exact match) | ✅ Yes (linear interpolation) |
| **Utilization Calc** | Manual (frontend) | Automatic (backend) |
| **Cache Invalidation** | ❌ Never (stale forever) | ✅ Version-based (real-time) |
| **Multi-Option Ranking** | Manual sorting | Pre-sorted by efficiency |
| **Admin Updates** | Redeploy frontend | Instant (database update) |
| **Offline Support** | ✅ Yes (static file) | ⚠️ Requires cache strategy |
| **Bundle Size** | +50KB (xlsx lib) | -50KB (no xlsx) |
| **Performance** | Slow (parse on load) | Fast (pre-processed JSON) |
| **Flexibility** | Low (hardcoded structure) | High (any JSONB structure) |

---

## 🚀 Migration Steps

### Step 1: Add Matrix Service (Week 1)
```bash
# Install axios if not already
npm install axios

# Create new service file
touch src/services/matrixService.js
```

### Step 2: Parallel Testing (Week 2)
```javascript
// In your component
import { matrixService } from './services/matrixService';
import { getValidBeamProfiles as legacyGetValidBeams } from './services/loadChart';

// Compare results
const newBeams = await matrixService.getValidBeamProfiles(upright, span, load);
const oldBeams = legacyGetValidBeams(upright, span, load);

console.log('New API:', newBeams);
console.log('Legacy Excel:', oldBeams);
// Verify they match!
```

### Step 3: Feature Flag Rollout (Week 3)
```javascript
const USE_MATRIX_API = import.meta.env.VITE_USE_MATRIX_API === 'true';

const beams = USE_MATRIX_API
    ? await matrixService.getValidBeamProfiles(upright, span, load)
    : legacyGetValidBeams(upright, span, load);
```

### Step 4: Full Cutover (Week 4)
```javascript
// Remove xlsx dependency
npm uninstall xlsx

// Delete legacy files
rm public/Load_Chart.xlsx
rm src/services/loadChart.js

// Update all imports
import { matrixService } from './services/matrixService';
```

---

## 🎨 UI Enhancements with New API

### Show Utilization Percentage
```jsx
// OLD: Just show valid beams
<select>
  {validBeams.map(beam => (
    <option key={beam.id} value={beam.id}>
      {beam.name}
    </option>
  ))}
</select>

// NEW: Show utilization for informed decisions
<select>
  {validBeams.map(beam => (
    <option key={beam.id} value={beam.id}>
      {beam.name} - {beam.utilization.toFixed(1)}% utilized
      {beam.utilization < 60 && ' ⚡ Efficient'}
      {beam.utilization > 90 && ' ⚠️ Near Limit'}
    </option>
  ))}
</select>
```

### Visual Capacity Indicator
```jsx
<div className="beam-option">
  <span>{beam.name}</span>
  <div className="capacity-bar">
    <div 
      className="utilization" 
      style={{ 
        width: `${beam.utilization}%`,
        backgroundColor: beam.utilization > 90 ? 'red' : 'green'
      }}
    />
  </div>
  <span>{beam.utilization.toFixed(1)}%</span>
</div>
```

---

## 🔮 Future Capabilities

### 1. **Real-Time Updates**
```javascript
// Admin updates beam chart in database
// Frontend automatically detects via version header
// No code deployment needed!
```

### 2. **Multiple Chart Types**
```javascript
// Beam capacity
const beamChoices = await matrixService.getChoices('BeamChart', ...);

// Seismic factors
const seismicFactor = await matrixService.lookup('SeismicFactors', ['Zone_A'], height);

// Pricing
const price = await matrixService.lookup('PriceTable', ['Retail'], quantity);

// Anchor compatibility
const anchorData = await matrixService.lookup('AnchorChart', ['C30', 'M12']);
```

### 3. **Smart Recommendations**
```javascript
// Backend can suggest alternatives
const recommendations = await fetch('/api/v1/matrices/BeamChart/recommendations', {
    method: 'POST',
    body: JSON.stringify({
        uprightId: 'ST20',
        span: 2750,
        currentLoad: 1500,
        targetUtilization: 70 // Prefer 70% utilization
    })
});

// Returns: "Consider reducing span to 2600mm to use HEM_80 at 68% utilization"
```

---

## ✅ Recommendation

**Use the new Matrix API architecture** because:

1. ✅ **Supports unlimited chart types** (not just beam loads)
2. ✅ **Automatic interpolation** (works for any span value)
3. ✅ **Real-time updates** (no frontend redeployment)
4. ✅ **Pre-calculated metrics** (utilization, safety)
5. ✅ **Smaller bundle size** (no xlsx library)
6. ✅ **Better performance** (no client-side parsing)
7. ✅ **Centralized validation** (backend is source of truth)

The current Excel approach is **limited to one chart type** and requires **exact span matches**. The new architecture is **infinitely flexible** and **production-ready**.

---

## 📞 Support

For migration assistance:
- Backend API: See `ARCHITECTURE.md`
- Testing: See `docs/RULE_MANIFEST_TESTING_GUIDE.md`
- Postman Collection: `tests/postman/RuleManifestAPI.postman_collection.json`
