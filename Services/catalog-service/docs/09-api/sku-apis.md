# SKU APIs

Contract-first documentation for SKU management endpoints.

## Base URL

```
/api/v1/sku
```

---

## GET /api/v1/sku

Retrieve all SKUs.

### Request

```http
GET /api/v1/sku
```

### Response

```json
{
  "skus": [
    {
      "id": "sku-001",
      "code": "GSS-BM-2700-1.6-SB",
      "name": "Beam 2700mm Standard",
      "glbFile": "/models/beam-2700.glb",
      "isActive": true,
      "isDeleted": false,
      "createdAt": "2026-01-10T00:00:00Z"
    }
  ]
}
```

---

## GET /api/v1/sku/{id}

Retrieve SKU by ID.

### Request

```http
GET /api/v1/sku/sku-001
```

### Response

```json
{
  "id": "sku-001",
  "code": "GSS-BM-2700-1.6-SB",
  "name": "Beam 2700mm Standard",
  "glbFile": "/models/beam-2700.glb",
  "isActive": true,
  "isDeleted": false,
  "createdAt": "2026-01-10T00:00:00Z"
}
```

---

## POST /api/v1/sku

Create a new SKU.

### Request

```http
POST /api/v1/sku
Content-Type: application/json
```

```json
{
  "code": "GSS-BM-3600-1.6-SB",
  "name": "Beam 3600mm Standard",
  "glbFile": "/models/beam-3600.glb"
}
```

### Response

```json
{
  "id": "sku-002",
  "code": "GSS-BM-3600-1.6-SB",
  "name": "Beam 3600mm Standard",
  "createdAt": "2026-01-10T00:00:00Z"
}
```

---

## PUT /api/v1/sku/{id}

Update an existing SKU.

### Request

```http
PUT /api/v1/sku/sku-002
Content-Type: application/json
```

```json
{
  "name": "Beam 3600mm Heavy Duty",
  "glbFile": "/models/beam-3600-hd.glb"
}
```

### Response

```json
{
  "id": "sku-002",
  "code": "GSS-BM-3600-1.6-SB",
  "name": "Beam 3600mm Heavy Duty",
  "updatedAt": "2026-01-10T00:00:00Z"
}
```

---

## DELETE /api/v1/sku/{id}

Soft-delete a SKU.

### Request

```http
DELETE /api/v1/sku/sku-002
```

### Response

```json
{
  "id": "sku-002",
  "isDeleted": true,
  "deletedAt": "2026-01-10T00:00:00Z"
}
```

---

## Related Documentation

- [Error Codes](./error-codes.md)
- [Palette API](./palette-apis.md)
