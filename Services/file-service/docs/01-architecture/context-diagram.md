# Architecture & Context

The `file-service` interacts with several other services and external components.

## Context Diagram

```mermaid
graph TD
    Client[Client App] -->|Uploads/Downloads| FileService[File Service]
    FileService -->|Store/Retrieve| S3[AWS S3]
    FileService -->|Parses| LibDXF[DXF Parser Lib]
    FileService -->|Generates| LibPDF[PDF Generator Lib]
    FileService -->|Generates| LibExcel[Excel Generator Lib]
    
    CatalogService[Catalog Service] -->|Refers to| FileService
    ConfigurationService[Configuration Service] -->|Refers to| FileService
```

## Key Components

- **API Layer:** Exposes endpoints for upload, download, and processing triggers.
- **Handlers:** Implements business logic for specific file operations (e.g., `ImportDxfHandler`).
- **Storage Provider:** Abstraction over S3 (or local storage for dev).
- **Processors:** Dedicated logic for parsing specific formats (DXF) or generating outputs (PDF/Excel).
