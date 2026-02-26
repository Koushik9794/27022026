# Container Diagram

C4 Level 2: Container diagram showing internal components within the Catalog Service.

```mermaid
flowchart TB
    subgraph CatalogService["Catalog Service"]
        API[API Layer]
        App[Application Layer]
        Domain[Domain Layer]
        Infra[Infrastructure Layer]
    end
    
    API --> App
    App --> Domain
    App --> Infra
    Infra --> Domain
    
    subgraph APIDetails["API Layer"]
        SkuCtrl[SkuController]
        PalCtrl[PaletteController]
    end
    
    subgraph AppDetails["Application Layer"]
        Cmds[Commands]
        Queries[Queries]
        Handlers[Handlers]
    end
    
    subgraph DomainDetails["Domain Layer"]
        Sku[Sku Aggregate]
        Pallet[Pallet Aggregate]
        Mhe[MHE Aggregate]
    end
    
    subgraph InfraDetails["Infrastructure Layer"]
        Repos[Repositories]
        Mgr[Migrations]
    end
```

## Layer Responsibilities

| Layer | Responsibility | Technologies |
|-------|----------------|--------------|
| API | HTTP endpoints, validation | ASP.NET Core, Swagger |
| Application | CQRS, orchestration | WolverineFx |
| Domain | Business logic, invariants | Pure C# |
| Infrastructure | Persistence, external I/O | Dapper, FluentMigrator |
