# Load Charts Index

Load charts are grouped by **structural component**, not product group.

## Component Categories

| Category | Components | Doc Path |
|----------|------------|----------|
| [Step Beam](./step-beam/) | RF Panel, Guided PSB, 6-Bend Panel | `06-load-charts/step-beam/` |
| [Cantilever](./cantilever/) | Heavy-Duty Arm, Column, Brace | `06-load-charts/cantilever/` |
| [ASRS](./asrs/) | Rail, Shuttle Support, Bin Support | `06-load-charts/asrs/` |

## How Product Groups Use Charts

Product groups **reference** charts through rules:

| Product Group | Charts Used |
|---------------|-------------|
| SPR | Step Beam charts, Upright charts |
| Cantilever | Arm charts, Column charts |
| ASRS | Rail charts, Shuttle charts |
| Mobile | Base charts, Carriage charts |

## Key Documentation

- [Universal Load Chart Model](../03-rule-engine/load-charts.md) — Core concepts
- [Versioning](../03-rule-engine/load-charts/versioning.md) — Immutability rules
- [Assumptions](../03-rule-engine/load-charts/assumptions.md) — Engineering constraints

## Adding a New Chart

1. Identify the **component type**
2. Create chart metadata (chart_code, assumptions)
3. Define dimension schema
4. Import capacity entries
5. Create rules that reference the chart
