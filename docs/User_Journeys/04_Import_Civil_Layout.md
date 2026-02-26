# User Journey: Import Civil Layout (DXF/DWG)

Actor: Dealer / Design Consultant

Preconditions
- User has a civil layout file (DXF, DWG) exported from CAD
- User has permission to upload files

Main Flow
1. User navigates to `Import` and uploads a civil layout file.
2. Frontend uploads file to the File Processing Service (`POST /files/upload`) which stores the file in S3 and returns a file reference.
3. File Processing Service triggers a background job to parse the CAD file (DXF/DWG parser) and extract geometry, units, coordinate system, and layers.
4. Parsed layout is converted into the internal grid/footprint format (JSON) and optionally simplified for performance.
5. System normalizes units (meters/feet) and maps building reference points (origin, axes).
6. Parsed result saved to `configurations.configurationData` as an imported layout element and a `ConfigurationHistory` entry is created.
7. Frontend displays imported layout in the canvas; user can align/scale and pin layout to site coordinates.
8. Run validation: Rules Engine ensures the imported layout meets minimum constraints (clearances, egress, etc.).

Alternate Flows
- Unsupported file format: show validation error and instructions.
- Parsing errors: provide detailed error report and highlight problematic layers.
- Large files: process asynchronously and notify user when ready via WebSocket or notification.

API Notes
- File upload: `POST /files/upload` → S3
- Parser job: async invocation (background worker / Lambda)
- Result retrieval: `GET /files/{id}/parsed`

Data Produced
- Raw CAD file in S3
- Parsed JSON layout stored inside `Configuration.configurationData`
- `ConfigurationHistory` entry for the import event

Postconditions
- Imported civil layout is part of the configuration and available for validation and visualization
