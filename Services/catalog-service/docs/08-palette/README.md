# UI Palette

The Palette is the UI configuration for the DesignDeck component menu.

## What Is the Palette?

The palette defines:
- Component groups visible in the configurator UI
- Categories and sub-categories
- Role-based visibility (Admin, Designer, Dealer)
- UI metadata (icons, tooltips)

---

## Palette Structure

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
            { "id": "spr", "label": "Selective Pallet Racking" },
            { "id": "double-deep", "label": "Double Deep Racking" },
            { "id": "drive-in", "label": "Drive-In Racking" }
          ]
        },
        {
          "id": "long-goods",
          "label": "Long Goods Storage",
          "elements": [
            { "id": "cantilever", "label": "Cantilever Racking" }
          ]
        }
      ]
    }
  ]
}
```

---

## Palette Groups

| Group | Contents |
|-------|----------|
| Warehouse Types | Facility templates |
| SKUs | Stock keeping units |
| Pallets | Pallet configurations |
| MHE | Material handling equipment |
| Product Groups | SPR, Cantilever, ASRS |
| Structure | Frames, beams, bracing |
| Openings | Doors, docks |

---

## Role-Based Visibility

| Role | Access |
|------|--------|
| ADMIN | All elements |
| DESIGNER | Design elements |
| DEALER | Customer-facing elements |

---

## Palette API

```http
GET /api/v1/palette
```

Response:
```json
{
  "groups": [...],
  "version": "1.0",
  "lastModified": "2026-01-10T00:00:00Z"
}
```

---

## Future: Database-Backed Palette

Currently served from static JSON file (`data/palette.json`).

Planned migration to database:

```sql
CREATE TABLE palette_groups (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    label VARCHAR(255) NOT NULL,
    icon VARCHAR(100),
    sort_order INT,
    is_active BOOLEAN DEFAULT true
);

CREATE TABLE palette_elements (
    id UUID PRIMARY KEY,
    group_id UUID NOT NULL REFERENCES palette_groups(id),
    category_id UUID,
    code VARCHAR(50) NOT NULL,
    label VARCHAR(255) NOT NULL,
    tooltip TEXT,
    icon VARCHAR(100),
    allowed_roles JSONB,
    properties JSONB,
    sort_order INT
);
```

---

## Related Documentation

- [Palette API](../09-api/palette-apis.md)
- [Component Taxonomy](../03-component-taxonomy/README.md)
