# Load Chart Import API - User Guide

## 📤 Overview

The Matrix Import API allows administrators to upload existing Excel or CSV load charts and automatically convert them to the JSONB matrix format used by the Rule Service.

---

## 🎯 Supported Formats

### CSV Format (Recommended)
✅ **Fully supported**  
✅ **No additional dependencies**  
✅ **Easy to generate from Excel**

### Excel Format (.xlsx, .xls)
⚠️ **Requires EPPlus library** (not included by default)  
💡 **Workaround**: Convert Excel to CSV first

---

## 📋 CSV File Format

### Expected Structure

```csv
Upright,BeamSpan,HEM_80,HEM_100,HEM_120,HEM_140
ST20,2700,2000,3000,4000,5000
ST20,2800,1800,2800,3800,4800
ST20,2900,1600,2600,3600,4600
ST25,2700,2200,3200,4200,5200
ST25,2800,2000,3000,4000,5000
ST25,2900,1800,2800,3800,4800
```

### Column Requirements

1. **Column 1**: `Upright` - Upright profile ID (e.g., ST20, ST25)
2. **Column 2**: `BeamSpan` - Beam span in mm (e.g., 2700, 2800)
3. **Column 3+**: Profile names - Beam profile IDs (e.g., HEM_80, HEM_100)

### Data Rows

- Each row represents capacity values for a specific upright and span
- Values are capacities in kg for each beam profile

---

## 🚀 API Endpoints

### 1. Preview Import (Dry Run)

**Purpose**: Validate file format and preview the generated JSONB structure without saving

```http
POST /api/v1/matrices/import/preview
Content-Type: multipart/form-data

file: [CSV file]
```

**Example using cURL**:
```bash
curl -X POST http://localhost:5001/api/v1/matrices/import/preview \
  -F "file=@Load_Chart.csv"
```

**Example using Postman**:
1. Select `POST` method
2. URL: `http://localhost:5001/api/v1/matrices/import/preview`
3. Body → form-data
4. Key: `file` (type: File)
5. Value: Select your CSV file

**Response** (200 OK):
```json
{
  "fileName": "Load_Chart.csv",
  "rowCount": 7,
  "headers": ["Upright", "BeamSpan", "HEM_80", "HEM_100", "HEM_120"],
  "sampleData": [
    ["ST20", "2700", "2000", "3000", "4000"],
    ["ST20", "2800", "1800", "2800", "3800"],
    ["ST20", "2900", "1600", "2600", "3600"]
  ],
  "generatedStructure": {
    "uprights": {
      "ST20": {
        "HEM_80": [
          { "X": 2700, "Y": 2000 },
          { "X": 2800, "Y": 1800 },
          { "X": 2900, "Y": 1600 }
        ],
        "HEM_100": [
          { "X": 2700, "Y": 3000 },
          { "X": 2800, "Y": 2800 }
        ]
      },
      "ST25": {...}
    }
  }
}
```

---

### 2. Import Load Chart

**Purpose**: Upload and save the load chart to the database

```http
POST /api/v1/matrices/import
Content-Type: multipart/form-data

file: [CSV file]
matrixName: BeamChart
category: LOAD_CHART
```

**Example using cURL**:
```bash
curl -X POST http://localhost:5001/api/v1/matrices/import \
  -F "file=@Load_Chart.csv" \
  -F "matrixName=BeamChart" \
  -F "category=LOAD_CHART"
```

**Example using Postman**:
1. Select `POST` method
2. URL: `http://localhost:5001/api/v1/matrices/import`
3. Body → form-data
4. Add fields:
   - `file` (File): Select CSV
   - `matrixName` (Text): `BeamChart`
   - `category` (Text): `LOAD_CHART`

**Response** (200 OK):
```json
{
  "message": "Load chart imported successfully",
  "matrixName": "BeamChart",
  "rowsProcessed": 6,
  "dataStructure": {
    "uprights": {
      "ST20": {...},
      "ST25": {...}
    }
  }
}
```

**Response** (400 Bad Request - Validation Error):
```json
{
  "error": "File must have at least 2 rows (header + data)"
}
```

---

## 📝 Step-by-Step Guide

### Step 1: Prepare Your CSV File

**Option A: Export from Excel**
1. Open your Excel load chart
2. File → Save As
3. Choose "CSV (Comma delimited) (*.csv)"
4. Save

**Option B: Create Manually**
```csv
Upright,BeamSpan,HEM_80,HEM_100,HEM_120
ST20,2700,2000,3000,4000
ST20,2800,1800,2800,3800
```

### Step 2: Preview the Import

```bash
curl -X POST http://localhost:5001/api/v1/matrices/import/preview \
  -F "file=@Load_Chart.csv"
```

**Check the response**:
- ✅ Verify `generatedStructure` looks correct
- ✅ Check `headers` match your expectations
- ✅ Review `sampleData` for accuracy

### Step 3: Import to Database

```bash
curl -X POST http://localhost:5001/api/v1/matrices/import \
  -F "file=@Load_Chart.csv" \
  -F "matrixName=BeamChart" \
  -F "category=LOAD_CHART"
```

### Step 4: Verify Import

```bash
# Get the imported matrix
curl http://localhost:5001/api/v1/matrices/BeamChart

# Test a lookup
curl "http://localhost:5001/api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2700"

# Test choices
curl "http://localhost:5001/api/v1/matrices/BeamChart/choices?uprightId=ST20&span=2750&load=1500"
```

---

## 🎨 Frontend Integration

### React Component Example

```jsx
import { useState } from 'react';

function LoadChartImporter() {
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState(null);
  const [loading, setLoading] = useState(false);

  const handlePreview = async () => {
    if (!file) return;

    setLoading(true);
    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await fetch('/api/v1/matrices/import/preview', {
        method: 'POST',
        body: formData
      });
      
      const data = await response.json();
      setPreview(data);
    } catch (error) {
      console.error('Preview failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleImport = async () => {
    if (!file) return;

    setLoading(true);
    const formData = new FormData();
    formData.append('file', file);
    formData.append('matrixName', 'BeamChart');
    formData.append('category', 'LOAD_CHART');

    try {
      const response = await fetch('/api/v1/matrices/import', {
        method: 'POST',
        body: formData
      });
      
      const data = await response.json();
      alert(`Import successful! Processed ${data.rowsProcessed} rows.`);
    } catch (error) {
      console.error('Import failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="importer">
      <h2>Import Load Chart</h2>
      
      <input 
        type="file" 
        accept=".csv"
        onChange={(e) => setFile(e.target.files[0])}
      />
      
      <button onClick={handlePreview} disabled={!file || loading}>
        Preview
      </button>
      
      <button onClick={handleImport} disabled={!file || loading}>
        Import
      </button>

      {preview && (
        <div className="preview">
          <h3>Preview</h3>
          <p>Rows: {preview.rowCount}</p>
          <p>Headers: {preview.headers.join(', ')}</p>
          
          <h4>Sample Data:</h4>
          <table>
            <thead>
              <tr>
                {preview.headers.map(h => <th key={h}>{h}</th>)}
              </tr>
            </thead>
            <tbody>
              {preview.sampleData.map((row, i) => (
                <tr key={i}>
                  {row.map((cell, j) => <td key={j}>{cell}</td>)}
                </tr>
              ))}
            </tbody>
          </table>

          <h4>Generated Structure:</h4>
          <pre>{JSON.stringify(preview.generatedStructure, null, 2)}</pre>
        </div>
      )}
    </div>
  );
}

export default LoadChartImporter;
```

---

## ⚠️ Common Issues & Solutions

### Issue 1: "File must have at least 2 rows"
**Cause**: CSV is empty or has only headers  
**Solution**: Ensure CSV has header row + at least one data row

### Issue 2: Missing data points in generated structure
**Cause**: Non-numeric values in capacity columns  
**Solution**: Ensure all capacity values are numbers (no text, no empty cells)

### Issue 3: Profiles not appearing
**Cause**: Empty or whitespace-only profile names in header  
**Solution**: Ensure all profile column headers have valid names

### Issue 4: Duplicate data points
**Cause**: Multiple rows with same Upright + BeamSpan combination  
**Solution**: Remove duplicate rows or use latest value

---

## 🔄 Update Existing Matrix

To update an existing matrix, simply import with the **same matrix name**:

```bash
curl -X POST http://localhost:5001/api/v1/matrices/import \
  -F "file=@Updated_Load_Chart.csv" \
  -F "matrixName=BeamChart" \
  -F "category=LOAD_CHART"
```

The system will:
1. Increment the version number
2. Replace the data
3. Update the `updated_at` timestamp

---

## 📊 Supported Matrix Types

While the import API is designed for load charts, you can adapt it for other matrix types:

### Seismic Factors
```csv
Zone,Height,Factor
Zone_A,5000,1.2
Zone_A,10000,1.5
Zone_B,5000,1.3
```

### Price Tables
```csv
CustomerType,Quantity,Price
Retail,1,100
Retail,10,90
Wholesale,1,80
```

**Note**: For non-load-chart formats, you may need to customize the `ConvertToJsonbStructure` method.

---

## 🚀 Advanced: Excel Support (Optional)

To enable Excel import, install EPPlus:

```bash
dotnet add package EPPlus
```

Then update `MatrixImportEndpoints.cs`:

```csharp
using OfficeOpenXml;

private static async Task<List<string[]>> ParseExcelAsync(IFormFile file)
{
    var rows = new List<string[]>();
    
    using var stream = file.OpenReadStream();
    using var package = new ExcelPackage(stream);
    
    var worksheet = package.Workbook.Worksheets[0];
    var rowCount = worksheet.Dimension.Rows;
    var colCount = worksheet.Dimension.Columns;
    
    for (int row = 1; row <= rowCount; row++)
    {
        var values = new string[colCount];
        for (int col = 1; col <= colCount; col++)
        {
            values[col - 1] = worksheet.Cells[row, col].Text;
        }
        rows.Add(values);
    }
    
    return rows;
}
```

---

## ✅ Best Practices

1. **Always preview first**: Use `/preview` endpoint before importing
2. **Validate data**: Ensure no empty cells in capacity columns
3. **Sort by span**: Data points should be ordered by span (ascending)
4. **Consistent naming**: Use same profile IDs across all uprights
5. **Version control**: Keep original CSV files for audit trail
6. **Test after import**: Verify with `/lookup` and `/choices` endpoints

---

## 📞 Support

For issues or questions:
- Check logs for detailed error messages
- Verify CSV format matches expected structure
- Test with preview endpoint first
- Contact backend team for assistance

---

**Last Updated**: 2026-01-25  
**Version**: 1.0
