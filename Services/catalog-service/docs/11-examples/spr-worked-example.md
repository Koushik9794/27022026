# Worked Example — Selective Pallet Racking (SPR)

End-to-end structural formation using the 6-step order.

## Scenario

**Warehouse Context:**
- Area: Bulk Storage Zone
- Floor: Ground Floor
- Clear height: 11,000 mm
- Slab rating: 50 kN/m²
- Seismic zone: Zone 3
- MHE: Reach Truck
- Aisle width: 2,800 mm

**Storage Requirement:**
- Product Group: Selective Pallet Racking (SPR)
- Pallet load: 1,000 kg per pallet
- Pallet size: 1200 × 1000 mm
- Configuration: 1 Starter Bay + 2 Add-On Bays, 4 pallet levels

---

## Step 1 — Structural Relationships

```
Starter Bay
├── Frame A (Upright + Bracing + Base Plate)
├── Frame B (Upright + Bracing + Base Plate)
└── Beam Levels (4)

Add-On Bay 1
├── Shared Frame B
├── Frame C
└── Beam Levels (4)

Add-On Bay 2
├── Shared Frame C
├── Frame D
└── Beam Levels (4)
```

**Outcome:** Load paths are continuous.

---

## Step 2 — Physical Compatibility

| Check | Result |
|-------|--------|
| Beam interface = TEARDROP-50 | ✅ |
| Upright interface = TEARDROP-50 | ✅ |
| Base Plate matches anchor pattern | ✅ |

**Outcome:** All connections compatible.

---

## Step 3 — Structural Constraints

| Constraint | Result |
|------------|--------|
| Frames and Beams mandatory | ✅ |
| Add-On requires Starter | ✅ |
| Cantilever arms forbidden | ✅ |

**Outcome:** System identity = SPR.

---

## Step 4 — Conditional Dependencies (Declared)

| Condition | Dependency |
|-----------|------------|
| Height > 5,000 mm | Bracing required |
| Seismic Zone ≥ 3 | Enhanced anchoring |
| Narrow aisle + Reach Truck | Safety guards |

---

## Step 5 — Contextual Rule Evaluation

| Context | Decision |
|---------|----------|
| Rack height ≈ 10,200 mm | Bracing enforced |
| Seismic Zone = 3 | Anchor grade upgraded |
| Reach Truck + 2,800 mm aisle | Row guards added |

**Outcome:** Safe in this context.

---

## Step 6 — BOM Explosion

### Engineering BOM

| Component | Qty |
|-----------|-----|
| Uprights | 4 |
| Beams | 24 |
| Bracing Sets | 12 |
| Base Plates | 4 |
| Anchors (seismic-grade) | 16 |
| Row Guards | 4 |

### Manufacturing BOM

| SKU | Qty |
|-----|-----|
| GSS-UP-10500-90x70 | 4 |
| GSS-BM-2700-1.6-SB | 24 |
| GSS-BR-DIAG-SET | 12 |
| GSS-BP-150x150 | 4 |
| VENDOR-ANCH-M12-C8.8 | 16 |
| GSS-RG-2700 | 4 |

---

## Validation Summary

| Step | Outcome |
|------|---------|
| Structural Relationships | Correct load paths ✅ |
| Compatibility | Physically buildable ✅ |
| Structural Constraints | Correct identity ✅ |
| Dependencies | Declared correctly ✅ |
| Rule Evaluation | Context-safe ✅ |
| BOM Explosion | Complete and traceable ✅ |

> A configuration that passes this pipeline **can be built safely in the real world**.
