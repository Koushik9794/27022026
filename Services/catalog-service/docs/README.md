# Catalog Service Documentation

This directory contains comprehensive documentation for the Catalog Service.

## Quick Links

| Section | Purpose | Start Here |
|---------|---------|------------|
| [00-overview](./00-overview/) | Problem, goals, scope | [problem-statement.md](./00-overview/problem-statement.md) |
| [01-domain](./01-domain/) | Domain model, DDD concepts | [ubiquitous-language.md](./01-domain/ubiquitous-language.md) |
| [02-architecture](./02-architecture/) | System design, ADRs | [context-diagram.md](./02-architecture/context-diagram.md) |
| [03-component-taxonomy](./03-component-taxonomy/) | Physical component types | [README.md](./03-component-taxonomy/README.md) |
| [04-warehouse-spatial](./04-warehouse-spatial/) | Warehouse spatial context | [README.md](./04-warehouse-spatial/README.md) |
| [05-assemblies](./05-assemblies/) | Structural groupings | [README.md](./05-assemblies/README.md) |
| [06-symbols](./06-symbols/) | 2D rendering symbols | [README.md](./06-symbols/README.md) |
| [07-bom](./07-bom/) | BOM explosion | [README.md](./07-bom/README.md) |
| [08-palette](./08-palette/) | DesignDeck UI palette | [README.md](./08-palette/README.md) |
| [09-api](./09-api/) | Contract-first API docs | [sku-apis.md](./09-api/sku-apis.md) |
| [10-data-model](./10-data-model/) | Schema, migrations | [schema.md](./10-data-model/schema.md) |
| [11-examples](./11-examples/) | Worked examples | [spr-worked-example.md](./11-examples/spr-worked-example.md) |

---

## Core Concepts

### Two Orthogonal Taxonomies

| Taxonomy | Answers | Purpose |
|----------|---------|---------|
| [Component Taxonomy](./03-component-taxonomy/) | **WHAT** exists | Physical component types |
| [Warehouse Spatial](./04-warehouse-spatial/) | **WHERE** it exists | Location and context |

> Safety emerges **only when both are evaluated together**.

### Structural Formation Order

```
1. Structural Relationships (Topology)
   ↓
2. Physical Compatibility (Interfaces)
   ↓
3. Structural Constraints (Always-True)
   ↓
4. Conditional Dependencies (Declarative)
   ↓
5. Contextual Rule Evaluation (Safety)
   ↓
6. BOM Explosion (Materialization)
```

See [Structural Formation Order](./05-assemblies/structural-formation-order.md) for details.

---

## For New Team Members

1. Start with [00-overview/problem-statement.md](./00-overview/problem-statement.md)
2. Understand [03-component-taxonomy/README.md](./03-component-taxonomy/README.md)
3. Review [05-assemblies/structural-formation-order.md](./05-assemblies/structural-formation-order.md)
4. Explore [11-examples/spr-worked-example.md](./11-examples/spr-worked-example.md)

## For API Consumers

1. Read [09-api/sku-apis.md](./09-api/sku-apis.md)
2. Review [09-api/palette-apis.md](./09-api/palette-apis.md)
3. Check [09-api/error-codes.md](./09-api/error-codes.md)

## For Mechanical/Structural Experts

1. Review [03-component-taxonomy/README.md](./03-component-taxonomy/README.md)
2. Validate [03-component-taxonomy/interfaces/README.md](./03-component-taxonomy/interfaces/README.md)
3. Check [05-assemblies/structural-formation-order.md](./05-assemblies/structural-formation-order.md)
4. Validate [07-bom/README.md](./07-bom/README.md)
