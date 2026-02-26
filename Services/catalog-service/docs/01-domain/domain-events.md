# Domain Events

Events published by the Catalog Service.

| Event Name | Trigger | Payload Summary |
|------------|---------|-----------------|
| `SkuCreated` | New SKU created | `{ skuId, code, name }` |
| `SkuUpdated` | SKU modified | `{ skuId, changes }` |
| `SkuDeleted` | SKU soft-deleted | `{ skuId, deletedAt }` |
| `SkuActivated` | SKU status set active | `{ skuId }` |
| `SkuDeactivated` | SKU status set inactive | `{ skuId }` |
| `PalletCreated` | New pallet created | `{ palletId, code, name }` |
| `PalletUpdated` | Pallet modified | `{ palletId, changes }` |
| `PalletDeleted` | Pallet soft-deleted | `{ palletId }` |
| `MheCreated` | New MHE created | `{ mheId, code, name }` |
| `MheUpdated` | MHE modified | `{ mheId, changes }` |
| `MheDeleted` | MHE soft-deleted | `{ mheId }` |

## Event Consumers

| Consumer | Events | Purpose |
|----------|--------|---------|
| Rule Service | SkuUpdated, SkuDeleted | Invalidate load chart cache |
| Configuration Service | All | Validate component references |
| Audit Service | All | Track catalog changes |
