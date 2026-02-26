# OpenAPI Specifications

## Purpose: Service Catalog & Reference

This directory contains an **aggregated reference index** of all GSS Backend microservices.

**This is NOT used at runtime** - it's a documentation/catalog tool.

## Architecture

```
Frontend → BFF (gss-web-api) → Microservices
                ↓
         Aggregates services
         (Runtime API)

openapi/ → Reference Index
           (Documentation only)
```

### Runtime API (What Frontend Uses)
- **gss-web-api** (Port 8080) - BFF that aggregates microservices
- Frontend calls **only** the BFF
- BFF handles service orchestration

### Service-Level Specs (Source of Truth)
Each service maintains its own OpenAPI specification:
```
Services/
├── admin-service/openapi/v1/admin-service-api.yaml
├── catalog-service/openapi/v1/catalog-service-api.yaml
├── rule-service/openapi/v1/rule-service-api.yaml
├── file-service/openapi/v1/file-service-api.yaml
├── configuration-service/openapi/v1/configuration-service-api.yaml
└── bom-service/openapi/v1/bom-service-api.yaml
```

**Owned by**: Service development teams  
**Purpose**: Contract testing, service documentation, code generation  
**Updated**: When service APIs change

### Root-Level Unified Spec (Aggregated View)
This directory (`openapi/`) contains the **aggregated specification**:
```
openapi/v1/
├── index.yaml          # Unified spec (generated)
├── admin.yaml          # Reference to admin-service spec
├── catalog.yaml        # Reference to catalog-service spec
├── rules.yaml          # Reference to rule-service spec
├── files.yaml          # Reference to file-service spec
├── configuration.yaml  # Reference to configuration-service spec
└── bom.yaml            # Reference to bom-service spec
```

**Owned by**: Platform team  
**Purpose**: API Gateway configuration, frontend SDK generation, unified documentation  
**Updated**: Via aggregation script

## Aggregation Process

### Automatic Aggregation
Run the aggregation script to generate the unified spec:

```powershell
# Generate unified spec from service-level specs
.\scripts\aggregate-openapi.ps1

# With validation
.\scripts\aggregate-openapi.ps1 -Validate

# Watch mode (regenerate on changes)
.\scripts\aggregate-openapi.ps1 -Watch
```

### Manual Updates
If you need to manually update the unified spec:
1. **Always update the service-level spec first**
2. Run the aggregation script
3. Review the generated `index.yaml`

## Viewing the Unified Spec

### Using Swagger UI (Docker)

```bash
# Start Swagger UI
docker-compose -f docker-compose.swagger.yml up

# Access at http://localhost:9090
```

### Using Individual Service Swagger UIs

Each service has its own Swagger UI when running:
- Admin Service: http://localhost:5001/swagger
- Catalog Service: http://localhost:5002/swagger
- Rule Service: http://localhost:5000/swagger
- File Service: http://localhost:5003/swagger
- Configuration Service: http://localhost:5004/swagger
- BOM Service: http://localhost:5005/swagger

## Best Practices

### ✅ Do
- Update service-level specs when changing APIs
- Run aggregation script after service spec changes
- Use service Swagger UIs for service-specific testing
- Use unified spec for API Gateway configuration
- Validate specs before committing

### ❌ Don't
- Manually edit the unified `index.yaml` (it's generated)
- Skip running the aggregation script
- Let service specs drift from implementation
- Forget to update OpenAPI when adding endpoints

## Validation

### Prerequisites
```bash
# Install OpenAPI validation tools
npm install -g @apidevtools/swagger-cli
npm install -g @openapitools/openapi-generator-cli
```

### Validate All Specs
```powershell
.\scripts\aggregate-openapi.ps1 -Validate
```

### Validate Individual Service
```bash
swagger-cli validate Services/admin-service/openapi/v1/admin-service-api.yaml
```

## Contract Testing

Service-level specs are used for contract testing:

```csharp
// Example: Contract test for admin-service
[Fact]
public async Task AdminService_ShouldMatchOpenAPISpec()
{
    var specPath = "Services/admin-service/openapi/v1/admin-service-api.yaml";
    var spec = await OpenApiDocument.LoadAsync(specPath);
    
    // Validate implementation matches spec
    var validator = new OpenApiValidator(spec);
    var result = await validator.ValidateAsync(_client);
    
    Assert.True(result.IsValid);
}
```

## CI/CD Integration

The aggregation and validation should be part of CI/CD:

```yaml
# .github/workflows/openapi-validation.yml
name: OpenAPI Validation

on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Validate OpenAPI Specs
        run: |
          npm install -g @apidevtools/swagger-cli
          ./scripts/aggregate-openapi.ps1 -Validate
```

## Related Documentation

- [ADR-002: API Contract Governance Using OpenAPI](../docs/adr/adr-002-api-contract-governance-using-openapi.md)
- [ADR-004: REST API Design Standards](../docs/adr/adr-004-rest-api-design-standards.md)
- [Service Design Checklist](../docs/service-design-checklist.md)

---

**Last Updated**: January 2026  
**Maintained By**: GSS Platform Team
