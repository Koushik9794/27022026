# Context Diagram

C4 Level 1: System Context diagram showing the Catalog Service and external actors/systems.

```mermaid
flowchart TB
    subgraph Users
        Admin[Admin User]
        Designer[Designer]
    end
    
    subgraph GSS["GSS System"]
        BFF[BFF / Web API]
        Catalog[Catalog Service]
        Rules[Rule Service]
        Config[Configuration Service]
    end
    
    subgraph External
        CDN[GLB File CDN]
        DB[(PostgreSQL)]
    end
    
    Admin -->|Manage Catalog| BFF
    Designer -->|Load Palette| BFF
    BFF -->|CRUD, Query| Catalog
    Catalog -->|Store| DB
    Catalog -->|Reference| CDN
    Rules -->|Load Charts| Catalog
    Config -->|Component Lookup| Catalog
```

## External Systems

| System | Role | Integration |
|--------|------|-------------|
| BFF | API Gateway | REST API |
| Rule Service | Consumer | Sync API |
| Configuration Service | Consumer | Sync API |
| PostgreSQL | Persistence | Dapper + FluentMigrator |
| CDN | 3D Model Storage | URL reference |
