# Palette APIs

Contract-first documentation for Palette endpoints.

## Base URL

```
/api/v1/palette
```

---

## GET /api/v1/palette

Retrieve the complete palette configuration for DesignDeck UI.

### Request

```http
GET /api/v1/palette
```

### Response

```json
{
  "groups": [
    {
      "id": "warehouse-types",
      "label": "Warehouse Types",
      "icon": "warehouse",
      "categories": [
        {
          "id": "standard",
          "label": "Standard Warehouses",
          "elements": [
            {
              "id": "wh-bulk",
              "label": "Bulk Storage",
              "tooltip": "Standard bulk storage warehouse",
              "icon": "bulk-storage",
              "roles": ["ADMIN", "DESIGNER", "DEALER"]
            }
          ]
        }
      ]
    },
    {
      "id": "product-groups",
      "label": "Product Groups",
      "categories": [
        {
          "id": "pallet-racking",
          "label": "Pallet Racking",
          "elements": [
            { "id": "spr", "label": "Selective Pallet Racking" }
          ]
        }
      ]
    }
  ],
  "version": "1.0",
  "lastModified": "2026-01-10T00:00:00Z"
}
```

---

## GET /api/v1/palette/groups/{groupId}

Retrieve a specific palette group.

### Request

```http
GET /api/v1/palette/groups/product-groups
```

### Response

```json
{
  "id": "product-groups",
  "label": "Product Groups",
  "categories": [...]
}
```

---

## Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `groups` | Array | Top-level palette groups |
| `categories` | Array | Sub-categories within group |
| `elements` | Array | Individual palette items |
| `roles` | Array | Allowed roles for visibility |
| `version` | String | Palette version |

---

## Role Filtering

The palette can be filtered by role:

```http
GET /api/v1/palette?role=DESIGNER
```

Returns only elements visible to DESIGNER role.

---

## Related Documentation

- [Palette Overview](../08-palette/README.md)
- [SKU APIs](./sku-apis.md)
