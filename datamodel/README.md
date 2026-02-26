# Conceptual Data Model (CDM v1) - Walkthrough Guide

This document provides a guide to understanding the `ConceptualModel.puml` file. The model is structured into **7 Logical Packages** representing the end-to-end lifecycle of the Warehouse Configurator Platform.

## 📖 How to Read the Model

The model is designed to be read sequentially from Package 1 to Package 7.

### 1. Foundation & Standards
**"The Language"**
- **Purpose:** Defines the basic building blocks and standards.
- **Key Entities:**
    - `Taxonomy`: The hierarchical classification of all things (Components, Spatial, Operational).
    - `AttributeDefinition` & `AttributeValue`: Allows dynamic properties for any object without changing the schema.
    - `Country`, `Currency`, `UnitOfMeasure`: Ensures the system is globally aware.
    - `SafetyFactor`: Stores regulatory limits (e.g., Seismic Zone 4 factors) linked to a `Country`.

### 2. Master Catalog
**"The Menu"**
- **Purpose:** Defines WHAT can be sold and configured.
- **Key Entities:**
    - `ProductGroup`: High-level systems (e.g., "Selective Pallet Racking").
    - `ProductGroupTemplate`: **[NEW]** Predefined configurations (e.g., "Wide Aisle Standard") that users can drag-and-drop.
    - `ComponentType`: Abstract definitions of parts (e.g., "Upright Frame").
    - `Item` & `ItemVariant`: The actual physical manufacturing parts with `dimensions`, `weightKg`, and `carbonFactor`.
    - `MasterBOM`: The "Recipe" for how components fit together.

### 3. Project Context
**"The Who & Where"**
- **Purpose:** Contextualizes the work for a specific customer and site.
- **Key Entities:**
    - `Enquiry`: The sales container.
    - `Collaborator`: Who is working on it (Designer, Sales Rep).
    - `Warehouse` & `Floor`: The physical constraints (Slab rating, Seismic zone).

### 4. Solution Design
**"The Work"**
- **Purpose:** Where the user creates the specific configuration.
- **Key Entities:**
    - `ConfigurationVersion`: The snapshot of a design.
    - `CivilLayoutTemplate`: The starting point (can be imported from DXF).
    - `StorageSystemInstance`: The actual racking runs placed on the layout.
    - `InstallationPhase`: **[NEW]** Phasing of the installation (e.g., "Phase 1: Aisles 1-5").
    - `GADrawing`: **[NEW]** 2D Output definitions (Plan, Elevation, Sections).

### 5. Engineering & Validation
**"The Brain"**
- **Purpose:** Validates that the design is safe and viable.
- **Key Entities:**
    - `EngineeringRule`: The logic (e.g., "Height < 6x Depth"). Now includes `failureMessage`.
    - `RuleTestCase`: **[NEW]** "Golden" test cases to verify rules are correct.
    - `EngineeringEvaluation` & `EvaluationResult`: Granular pass/fail results for every check.

### 6. Manufacturing & Costing
**"The Result"**
- **Purpose:** Converts the design into numbers (Parts & Price).
- **Key Entities:**
    - `BOM` & `BOMExplosion`: Converting "1 Run" into "10 Uprights + 20 Beams".
    - `RateCard`: Pricing tables with `validFrom`/`validTo` dates.
    - `SustainabilityMetrics`: **[NEW]** Aggregated Steel Weight and Carbon Footprint (kgCO2e).

### 7. Platform Services
**"The Infrastructure"**
- **Purpose:** Administrative and cross-cutting concerns.
- **Key Entities:**
    - `OutputArtifact`: Parent class for all generated files (PDFs, DWGs, Quotes).
    - `FileMetadata`: Unified storage pointer for all files (local or S3).
    - `AuditLog`: Who changed what.
    - `Notification` & `UserFeedback`: User engagement loops.

---

## 🚀 Key Workflows

### A. The "Drag and Drop" Flow
1.  User selects a `ProductGroup` (Catalog).
2.  System shows available `ProductGroupTemplates` (Catalog).
3.  User places it -> Creates a `StorageSystemInstance` (Design).
4.  Instance inherits defaults from the Template.

### B. The "Quote Generation" Flow
1.  Design is Finalized (`ConfigurationVersion`).
2.  System explodes `MasterBOM` to create `BOMLine`s linked to `ItemVariants` (Manufacturing).
3.  `CostLine`s are calculated using the active `RateCard`.
4.  `Quote` artifact is generated based on `CostSummary`.

### C. The "Green Button" Flow (Sustainability)
1.  Each `ItemVariant` has a `weightKg` and `carbonFactor` (Catalog).
2.  BOM explosion sums these up into `SustainabilityMetrics` (Manufacturing).
3.  `CarbonFootprintReport` is generated as an output (Platform).

---

## 📐 Diagram
Open `ConceptualModel.puml` in a PlantUML viewer to see the visual representation of these relationships.
