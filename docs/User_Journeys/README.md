# User Journeys

This folder contains detailed user journeys for the Warehouse Configurator product. Each document describes the actors, preconditions, main flow, alternate flows, API/WebSocket interactions, data produced, and postconditions.

Available journeys

- `01_Login.md` — Authentication and session lifecycle
- `02_Create_Configuration_From_Enquiry.md` — Create a configuration from an enquiry
- `03_Update_Configuration.md` — Update an existing configuration
- `04_Import_Civil_Layout.md` — Import a civil layout (DXF/DWG)
- `05_2D_View.md` — 2D rendering and export workflow
- `06_3D_View.md` — 3D rendering and viewing workflow
- `07_Generate_BOM.md` — Generate Bill of Materials from configuration

Conventions

- `Actor`: Primary user or system initiating the flow
- `Preconditions`: System state before starting the flow
- `Main Flow`: Step-by-step happy path
- `Alternate Flows`: Error/edge-case handling
- `API Notes`: Services/endpoints and protocols used
