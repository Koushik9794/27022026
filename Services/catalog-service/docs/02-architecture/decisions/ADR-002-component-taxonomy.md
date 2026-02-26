# ADR-002: Component Taxonomy as Separate Concern

## Status

Accepted

## Context

The system needs to represent physical warehouse components in a way that:
- Is independent of product groups
- Supports interface compatibility checking
- Enables BOM explosion
- Allows new product groups without schema changes

## Decision

Implement a **Component Taxonomy** as a separate concern from SKUs:

1. **Taxonomy defines WHAT** — Physical component types (Beam, Upright, etc.)
2. **SKU defines WHICH** — Sellable products mapped to taxonomy
3. **Interfaces define HOW** — Physical connection compatibility
4. **Assemblies define GROUPING** — Structural compositions

## Consequences

### Positive
- Product groups don't redefine components
- Any product group can reuse existing taxonomy
- Interface compatibility is explicit and verifiable
- BOM explosion is deterministic

### Negative
- Additional abstraction layer to maintain
- Requires careful taxonomy design upfront
- SKU-to-taxonomy mapping must be maintained
