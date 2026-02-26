# GSS Rule Service - Architecture Overview

## Executive Summary

The **GSS Rule Service** is a microservice responsible for managing business rules, engineering validation logic, and dynamic lookup matrices for the Global Storage Solutions platform. It provides a centralized, version-controlled system for validating warehouse rack configurations against safety standards, engineering constraints, and business policies.

---

## 🏗️ High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend Layer                          │
│  (Designer UI, Admin Portal, Mobile Apps)                       │
└────────────────┬────────────────────────────────────────────────┘
                 │ HTTP/REST
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API Gateway / BFF                          │
│  (Authentication, Rate Limiting, Request Routing)               │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    GSS RULE SERVICE                             │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │              Application Layer (Endpoints)                  │ │
│ │  • RuleSetEndpoints      • MatrixEndpoints                  │ │
│ │  • RuleManifestEndpoints • EvaluationEndpoints              │ │
│ └──────────────────────┬──────────────────────────────────────┘ │
│                        │                                         │
│ ┌──────────────────────▼──────────────────────────────────────┐ │
│ │              Domain Layer (Business Logic)                  │ │
│ │  • RuleSet (Aggregate)   • Rule (Entity)                    │ │
│ │  • LookupMatrix (Entity) • RuleCondition (Entity)           │ │
│ │  • IMatrixEvaluationService • IRuleEvaluationService        │ │
│ └──────────────────────┬──────────────────────────────────────┘ │
│                        │                                         │
│ ┌──────────────────────▼──────────────────────────────────────┐ │
│ │           Infrastructure Layer (Data Access)                │ │
│ │  • DapperRuleRepository  • DapperLookupMatrixRepository     │ │
│ │  • MatrixEvaluationServiceImpl                              │ │
│ │  • DynamicExpressoExpressionEngine                          │ │
│ └──────────────────────┬──────────────────────────────────────┘ │
└────────────────────────┼──────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    PostgreSQL Database                          │
│  • rule_sets          • rules           • rule_conditions       │
│  • lookup_matrices    • ruleset_rules                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 Layered Architecture (Clean Architecture)

### 1. **Domain Layer** (`src/domain/`)
**Purpose**: Core business logic, entities, and domain services (framework-agnostic)

#### Aggregates
- **`RuleSet`** - Aggregate root managing a collection of rules
  - Lifecycle: DRAFT → ACTIVE → INACTIVE → ARCHIVED
  - Versioning via `EffectiveFrom` and `EffectiveTo`
  - Scoped by `ProductGroupId` and `CountryId`

#### Entities
- **`Rule`** - Individual validation rule
  - Properties: Name, Category, Priority, Severity, Formula
  - Can have either `Conditions` (field-based) or `Formula` (expression-based)
  
- **`RuleCondition`** - Condition within a rule
  - Structure: Field + Operator + Value
  - Types: AND, OR, NOT
  
- **`LookupMatrix`** - Engineering data matrix (JSONB)
  - Stores load charts, price tables, seismic factors
  - Versioned for change tracking
  - Supports hierarchical data (e.g., Upright → Span → Profile)

#### Value Objects
- **`RuleOutcome`** - Result of rule evaluation
  - Contains: Passed (bool), Message, Severity, Data (extended metrics)

#### Domain Services
- **`IMatrixEvaluationService`** - Smart matrix lookups
  - `LookupValueAsync()` - Interpolation for range-based data
  - `GetChoicesAsync()` - Multi-option evaluation with utilization

- **`IRuleEvaluationService`** - Rule execution engine
  - `EvaluateRuleSetAsync()` - Executes all rules in priority order
  - Supports preview mode and validation-only mode

- **`IExpressionEngine`** - Formula evaluation
  - Parses and executes dynamic expressions
  - Supports custom functions (MATRIX_LOOKUP, MATRIX_UTIL)

---

### 2. **Application Layer** (`src/application/`)
**Purpose**: Use cases, API endpoints, DTOs, message handlers

#### Endpoints (Wolverine HTTP)
```
┌─────────────────────────────────────────────────────────────┐
│ RuleSetEndpoints                                            │
│  POST   /api/v1/ruleset                - Create ruleset    │
│  GET    /api/v1/ruleset/{id}           - Get ruleset       │
│  PATCH  /api/v1/ruleset/{id}/activate  - Activate          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ MatrixEndpoints                                             │
│  GET    /api/v1/matrices/{name}        - Get full matrix   │
│  GET    /api/v1/matrices/{name}/choices - Get options      │
│  PATCH  /api/v1/matrices/{id}/cell     - Update cell       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RuleManifestEndpoints                                       │
│  GET    /api/v1/rules/manifest         - Unified manifest  │
│    Query: productGroupId, countryId                         │
│    Returns: Rules + Matrix metadata + Version              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ EvaluationEndpoints                                         │
│  POST   /api/v1/evaluate               - Evaluate config   │
│    Body: { ruleSetId, configuration, preview }             │
│    Returns: List of RuleOutcomes                            │
└─────────────────────────────────────────────────────────────┘
```

#### Messages (CQRS Pattern)
- **Commands**: `CreateRuleSet`, `ActivateRuleSet`, `UpdateMatrixCell`
- **Queries**: `GetRuleSet`, `GetActiveRules`, `GetRuleManifest`
- **Events**: `RuleSetActivated`, `MatrixUpdated` (for Wolverine pub/sub)

---

### 3. **Infrastructure Layer** (`src/infrastructure/`)
**Purpose**: External concerns (database, messaging, external services)

#### Persistence (Dapper + PostgreSQL)
```csharp
// Repository Pattern
public interface IRuleRepository
{
    Task<RuleSet> GetByIdAsync(Guid id);
    Task<List<RuleSet>> GetActiveRuleSetsByProductGroupAndCountryAsync(Guid pg, Guid c);
    Task SaveAsync(RuleSet ruleSet);
    Task UpdateAsync(RuleSet ruleSet);
}

public class DapperRuleRepository : IRuleRepository
{
    // Uses raw SQL for performance
    // Handles aggregate reconstruction (RuleSet + Rules + Conditions)
    // Transaction management for consistency
}
```

#### Matrix Evaluation Service
```csharp
public class MatrixEvaluationServiceImpl : IMatrixEvaluationService
{
    // Linear interpolation for non-standard values
    // Example: Span 2750mm from data points 2700mm and 2800mm
    
    public async Task<double?> LookupValueAsync(string matrixName, string[] path, double? value)
    {
        // 1. Fetch JSONB node using PostgreSQL #> operator
        // 2. Parse data points
        // 3. Perform interpolation
        // 4. Return calculated value
    }
    
    public async Task<List<MatrixChoiceResult>> GetChoicesAsync(...)
    {
        // 1. Fetch parent node (e.g., all beam profiles for ST20)
        // 2. For each option, calculate capacity at input span
        // 3. Calculate utilization = (requiredLoad / capacity) * 100
        // 4. Sort by utilization (most efficient first)
        // 5. Return ranked list
    }
}
```

#### Expression Engine (DynamicExpresso)
```csharp
public class DynamicExpressoExpressionEngine : IExpressionEngine
{
    private readonly IMatrixEvaluationService _matrixService;
    
    public async Task<object?> EvaluateAsync(string expression, Dictionary<string, object?> variables)
    {
        var interpreter = new Interpreter()
            .SetFunction("MATRIX_LOOKUP", (name, upright, span, profile) => 
            {
                return _matrixService.LookupValueAsync(name, 
                    new[] { "uprights", upright, profile }, span)
                    .GetAwaiter().GetResult();
            })
            .SetFunction("MATRIX_UTIL", (name, upright, span, profile, load) =>
            {
                var capacity = _matrixService.LookupValueAsync(...).GetAwaiter().GetResult();
                return (load / capacity) * 100;
            });
            
        // Register variables and evaluate
        return interpreter.Eval(expression);
    }
}
```

#### Migrations (FluentMigrator)
- **M20241218001** - Initial schema (rule_sets, rules, rule_conditions)
- **M20241218006** - Lookup matrices table with JSONB support

---

## 🗄️ Database Schema

### Core Tables

```sql
-- Aggregate Root
CREATE TABLE rule_sets (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    product_group_id UUID NOT NULL,
    country_id UUID NOT NULL,
    effective_from TIMESTAMP NOT NULL,
    effective_to TIMESTAMP,
    status VARCHAR(50) NOT NULL, -- DRAFT, ACTIVE, INACTIVE, ARCHIVED
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Entity
CREATE TABLE rules (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(100) NOT NULL, -- SPATIAL, STRUCTURAL, COMPLIANCE, etc.
    priority INT NOT NULL, -- Lower = higher priority
    severity VARCHAR(50) NOT NULL, -- ERROR, WARNING, INFO
    enabled BOOLEAN NOT NULL DEFAULT true,
    formula TEXT, -- Expression-based rule
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Many-to-Many
CREATE TABLE ruleset_rules (
    ruleset_id UUID REFERENCES rule_sets(id),
    rule_id UUID REFERENCES rules(id),
    added_at TIMESTAMP NOT NULL,
    PRIMARY KEY (ruleset_id, rule_id)
);

-- Entity (Owned by Rule)
CREATE TABLE rule_conditions (
    id UUID PRIMARY KEY,
    rule_id UUID REFERENCES rules(id) ON DELETE CASCADE,
    type VARCHAR(10) NOT NULL, -- AND, OR, NOT
    field VARCHAR(255) NOT NULL,
    operator VARCHAR(50) NOT NULL, -- GT, LT, EQ, CONTAINS, etc.
    value TEXT NOT NULL
);

-- JSONB Matrix Storage
CREATE TABLE lookup_matrices (
    id UUID PRIMARY KEY,
    name VARCHAR(255) UNIQUE NOT NULL,
    category VARCHAR(100) NOT NULL, -- LOAD_CHART, PRICE_TABLE, SEISMIC_FACTOR
    data JSONB NOT NULL, -- Hierarchical engineering data
    metadata JSONB, -- Lookup strategies, units, etc.
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Indexes for performance
CREATE INDEX idx_rulesets_product_country ON rule_sets(product_group_id, country_id, status);
CREATE INDEX idx_rules_category ON rules(category);
CREATE INDEX idx_matrices_name ON lookup_matrices(name);
CREATE INDEX idx_matrices_data_gin ON lookup_matrices USING GIN(data); -- JSONB index
```

### JSONB Matrix Example
```json
{
  "uprights": {
    "ST20": {
      "HEM_80": [
        { "X": 2700, "Y": 2000 },
        { "X": 2800, "Y": 1800 },
        { "X": 2900, "Y": 1600 }
      ],
      "HEM_100": [
        { "X": 2700, "Y": 3000 },
        { "X": 2800, "Y": 2800 }
      ]
    },
    "ST25": {
      "HEM_80": [...]
    }
  }
}
```

**Path-based Access**:
```sql
-- Get specific profile data
SELECT data #> '{uprights, ST20, HEM_80}' FROM lookup_matrices WHERE name = 'BeamChart';

-- Update specific cell
UPDATE lookup_matrices 
SET data = jsonb_set(data, '{uprights, ST20, HEM_80, 0, Y}', '2100'::jsonb)
WHERE name = 'BeamChart';
```

---

## 🔄 Data Flow Patterns

### Pattern 1: Rule Evaluation Flow
```
User submits configuration
         │
         ▼
┌─────────────────────┐
│ POST /api/v1/evaluate│
└──────────┬──────────┘
           │
           ▼
┌────────────────────────────┐
│ RuleEvaluationService      │
│ 1. Load active RuleSet     │
│ 2. Sort rules by priority  │
│ 3. For each rule:          │
│    - Evaluate conditions   │
│    - OR execute formula    │
│    - Collect outcome       │
└──────────┬─────────────────┘
           │
           ▼
┌────────────────────────────┐
│ ExpressionEngine           │
│ - Parse formula            │
│ - Call MATRIX_LOOKUP()     │
│ - Return boolean result    │
└──────────┬─────────────────┘
           │
           ▼
┌────────────────────────────┐
│ MatrixEvaluationService    │
│ - Fetch JSONB node         │
│ - Perform interpolation    │
│ - Return capacity value    │
└──────────┬─────────────────┘
           │
           ▼
┌────────────────────────────┐
│ Return List<RuleOutcome>   │
│ - Passed/Failed per rule   │
│ - Severity (ERROR/WARNING) │
│ - Extended data (util %)   │
└────────────────────────────┘
```

### Pattern 2: Manifest Delivery Flow
```
Frontend requests manifest
         │
         ▼
┌──────────────────────────────┐
│ GET /api/v1/rules/manifest   │
│ ?productGroupId=...          │
│ &countryId=...               │
└──────────┬───────────────────┘
           │
           ▼
┌────────────────────────────────┐
│ RuleManifestEndpoints          │
│ 1. Query active RuleSets       │
│ 2. Load all rules + conditions │
│ 3. Fetch matrix metadata       │
│ 4. Generate version string     │
└──────────┬─────────────────────┘
           │
           ▼
┌────────────────────────────────┐
│ Aggregate Response             │
│ {                              │
│   version: "20260125.0644",    │
│   rules: [...],                │
│   matrices: [...]              │
│ }                              │
└────────────────────────────────┘
```

### Pattern 3: Matrix Choice Evaluation
```
User changes span slider
         │
         ▼
┌──────────────────────────────────┐
│ GET /api/v1/matrices/BeamChart/  │
│ choices?uprightId=ST20           │
│        &span=2750                │
│        &load=1500                │
└──────────┬───────────────────────┘
           │
           ▼
┌────────────────────────────────────┐
│ MatrixEvaluationService            │
│ 1. Fetch parent node (all profiles│
│    for ST20)                       │
│ 2. For each profile:               │
│    - Interpolate capacity at 2750  │
│    - Calculate utilization %       │
│    - Determine if safe             │
│ 3. Sort by utilization (asc)       │
└──────────┬─────────────────────────┘
           │
           ▼
┌────────────────────────────────────┐
│ Return Ranked Choices              │
│ [                                  │
│   {choiceId: "HEM_100",            │
│    capacity: 2900,                 │
│    utilization: 51.72,             │
│    isSafe: true},                  │
│   {choiceId: "HEM_80", ...}        │
│ ]                                  │
└────────────────────────────────────┘
```

---

## 🧩 Design Patterns

### 1. **Repository Pattern**
- Abstracts data access logic
- Allows swapping Dapper for EF Core without changing domain
- Example: `IRuleRepository`, `ILookupMatrixRepository`

### 2. **Aggregate Pattern (DDD)**
- `RuleSet` is the aggregate root
- Ensures consistency boundaries
- All modifications go through aggregate methods

### 3. **Strategy Pattern**
- Matrix lookup strategies (Interpolate, RoundUp, Exact)
- Stored in `metadata` JSONB field
- Future: Pluggable evaluation strategies

### 4. **CQRS (Command Query Responsibility Segregation)**
- Commands: Modify state (CreateRuleSet, UpdateMatrix)
- Queries: Read-only (GetManifest, GetChoices)
- Enables future event sourcing

### 5. **Specification Pattern** (Implicit)
- Rule conditions act as specifications
- Composable via AND/OR/NOT
- Example: `(PalletWidth > 1000) AND (LoadPerPallet < 2000)`

---

## 🔐 Cross-Cutting Concerns

### Authentication & Authorization
```csharp
// Wolverine HTTP middleware
[Authorize(Roles = "Admin")]
public static class RuleSetEndpoints
{
    [WolverinePost("/api/v1/ruleset")]
    public static CreateRuleSetResponse CreateRuleSet(CreateRuleSet command)
    {
        // Only admins can create rulesets
    }
}

[Authorize(Roles = "Designer,Admin")]
public static class RuleManifestEndpoints
{
    [WolverineGet("/api/v1/rules/manifest")]
    public static async Task<IResult> GetManifest(...)
    {
        // Designers and admins can read manifests
    }
}
```

### Logging & Observability
```csharp
// Structured logging with Serilog
_logger.LogInformation(
    "Evaluating RuleSet {RuleSetId} for ProductGroup {ProductGroupId}",
    ruleSet.Id, productGroupId
);

// Metrics
_metrics.RecordRuleEvaluation(ruleSet.Id, duration, outcomeCount);
```

### Error Handling
```csharp
// Global exception handler
public class GlobalExceptionHandler : IExceptionHandler
{
    public async Task<bool> TryHandleAsync(HttpContext context, Exception exception, ...)
    {
        return exception switch
        {
            NotFoundException => Results.NotFound(new { message = exception.Message }),
            ValidationException => Results.BadRequest(new { errors = exception.Errors }),
            _ => Results.Problem("An error occurred")
        };
    }
}
```

### Caching Strategy
```csharp
// Distributed cache for manifests
public class CachedRuleManifestService
{
    private readonly IDistributedCache _cache;
    
    public async Task<RuleManifestResponse> GetManifestAsync(Guid pg, Guid c)
    {
        var cacheKey = $"manifest:{pg}:{c}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (cached != null)
            return JsonSerializer.Deserialize<RuleManifestResponse>(cached);
            
        var manifest = await _manifestService.GenerateAsync(pg, c);
        
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(manifest), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            
        return manifest;
    }
}
```

---

## 🚀 Deployment Architecture

### Container Structure
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/rule-service/RuleService.csproj", "Services/rule-service/"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RuleService.dll"]
```

### Docker Compose
```yaml
version: '3.8'

services:
  rule-service:
    build:
      context: .
      dockerfile: Services/rule-service/Dockerfile
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=rule_service;Username=postgres;Password=***
    depends_on:
      - postgres
      
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: rule_service
      POSTGRES_PASSWORD: ***
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

### Kubernetes Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rule-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: rule-service
  template:
    metadata:
      labels:
        app: rule-service
    spec:
      containers:
      - name: rule-service
        image: gss/rule-service:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## 📊 Performance Characteristics

### Response Times (Target)
| Endpoint | Target | Actual | Notes |
|----------|--------|--------|-------|
| GET /manifest | < 500ms | ~50ms | Cached after first request |
| POST /evaluate | < 200ms | ~80ms | For 100 rules |
| GET /choices | < 200ms | ~30ms | Interpolation + ranking |
| PATCH /cell | < 100ms | ~20ms | JSONB atomic update |

### Scalability
- **Horizontal**: Stateless service, can scale to N instances
- **Database**: Read replicas for manifest queries
- **Caching**: Redis for manifest responses (5-min TTL)

### Optimization Techniques
1. **JSONB Indexing**: GIN index on `data` column
2. **Partial Queries**: Fetch only required JSONB nodes
3. **Connection Pooling**: Npgsql connection pool (min: 5, max: 100)
4. **Compiled Expressions**: Cache DynamicExpresso interpreters
5. **Batch Operations**: Bulk rule evaluation

---

## 🔮 Future Enhancements

### Phase 2: Event Sourcing
```csharp
// Event store for audit trail
public class RuleSetActivatedEvent
{
    public Guid RuleSetId { get; set; }
    public Guid ActivatedBy { get; set; }
    public DateTime ActivatedAt { get; set; }
}

// Replay events to rebuild state
public class RuleSetProjection
{
    public void Apply(RuleSetActivatedEvent @event)
    {
        // Update read model
    }
}
```

### Phase 3: Machine Learning Integration
```csharp
// Predict optimal beam based on historical data
public interface IBeamRecommendationService
{
    Task<string> PredictOptimalBeamAsync(double span, double load, string uprightType);
}
```

### Phase 4: Multi-Tenancy
```csharp
// Tenant-specific rule isolation
public class TenantRuleRepository : IRuleRepository
{
    private readonly ITenantContext _tenantContext;
    
    public async Task<List<RuleSet>> GetActiveRuleSetsAsync(...)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        // Filter by tenant
    }
}
```

---

## 📚 Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Framework** | .NET 10 | Modern, high-performance runtime |
| **Web** | Wolverine HTTP | Minimal API with message-based routing |
| **Database** | PostgreSQL 15 | JSONB support, ACID compliance |
| **ORM** | Dapper | Lightweight, high-performance SQL |
| **Migrations** | FluentMigrator | Version-controlled schema changes |
| **Expression Engine** | DynamicExpresso | Runtime formula evaluation |
| **Messaging** | Wolverine | Async messaging, pub/sub |
| **Logging** | Serilog | Structured logging |
| **Testing** | xUnit | Unit and integration tests |
| **API Testing** | Postman | Manual/exploratory testing |

---

## 🎯 Key Architectural Decisions

### 1. **Why JSONB for Matrices?**
- ✅ Flexible schema for diverse engineering data
- ✅ Atomic updates via `jsonb_set`
- ✅ Path-based queries (`#>` operator)
- ✅ No need for complex relational schema
- ❌ Harder to enforce constraints

### 2. **Why Dapper over EF Core?**
- ✅ Full control over SQL (performance-critical)
- ✅ Minimal overhead
- ✅ Easy to optimize JSONB queries
- ❌ More boilerplate code

### 3. **Why Wolverine over MediatR?**
- ✅ Built-in HTTP routing
- ✅ Async messaging support
- ✅ Saga orchestration
- ✅ Better performance

### 4. **Why DynamicExpresso?**
- ✅ Supports C# syntax
- ✅ Custom function registration
- ✅ Type-safe evaluation
- ❌ Limited to .NET expressions

---

## 📖 Related Documentation

- [Rule Evaluation Guide](./RULE_EVALUATION_GUIDE.md)
- [Rule Manifest Implementation](./docs/RULE_MANIFEST_IMPLEMENTATION.md)
- [Testing Guide](./docs/RULE_MANIFEST_TESTING_GUIDE.md)
- [API Documentation](./docs/API_REFERENCE.md) (TODO)

---

## 🤝 Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for development setup and guidelines.

---

**Last Updated**: 2026-01-25  
**Version**: 1.0  
**Maintainer**: GSS Backend Team
