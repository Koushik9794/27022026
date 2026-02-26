# Error Codes

Standard error codes returned by the Catalog Service APIs.

## HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful request |
| 201 | Created | Resource successfully created |
| 400 | Bad Request | Invalid input or validation failure |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Duplicate code or constraint violation |
| 422 | Unprocessable Entity | Semantic validation error |
| 500 | Internal Server Error | Unexpected server error |

## Application Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| CAT_001 | SKU not found | Verify SKU ID exists |
| CAT_002 | SKU code already exists | Use unique code |
| CAT_003 | SKU is deleted | Cannot modify deleted SKU |
| CAT_004 | Invalid GLB file path | Check file path format |
| PAL_001 | Pallet not found | Verify pallet ID exists |
| PAL_002 | Pallet code already exists | Use unique code |
| MHE_001 | MHE not found | Verify MHE ID exists |
| MHE_002 | MHE code already exists | Use unique code |
| PAL_CFG_001 | Palette group not found | Verify group ID |

## Error Response Format

```json
{
  "success": false,
  "error": {
    "code": "CAT_002",
    "message": "SKU code already exists",
    "details": {
      "code": "GSS-BM-2700-1.6-SB",
      "existingSkuId": "sku-001"
    }
  }
}
```

## Validation Errors

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": {
      "errors": [
        { "field": "code", "message": "Code is required" },
        { "field": "name", "message": "Name must not be empty" }
      ]
    }
  }
}
```
