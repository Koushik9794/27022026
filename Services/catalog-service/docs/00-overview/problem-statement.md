# Problem Statement

## The Challenge

Warehouse configurator systems require a **centralized catalog** of components:
- SKUs (Stock Keeping Units) with 3D models
- Pallet definitions with dimensions and weight limits
- Material Handling Equipment (MHE) specifications
- UI palette configuration for the configurator

Without a centralized catalog service:
- Components are duplicated across services
- 3D model references become inconsistent
- UI configuration is hardcoded
- Component metadata is scattered

## What the Catalog Service Solves

The Catalog Service provides a **single source of truth** for all warehouse configuration components.

| Problem | Solution |
|---------|----------|
| Duplicate component definitions | Centralized SKU, Pallet, MHE aggregates |
| Inconsistent 3D models | Managed GLB file references per SKU |
| Hardcoded UI menus | Dynamic palette served from database |
| No component versioning | Soft-delete and version tracking |
| Scattered metadata | Rich domain model with encapsulated logic |

## Business Value

1. **Consistency** — All services reference the same catalog
2. **Maintainability** — Single place to update components
3. **Extensibility** — Add new component types easily
4. **Auditability** — Track changes to catalog items
5. **UI Flexibility** — Palette configuration without code changes
