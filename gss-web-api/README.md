# GSS Web API - Backend for Frontend (BFF)

Backend-for-Frontend service for the Godrej Storage Solutions (GSS) warehouse configurator platform.

## Overview

The `gss-web-api` is a **Backend-for-Frontend (BFF)** service that acts as an orchestration and aggregation layer between the frontend application and internal microservices (`admin-service`, `catalog-service`, `rule-service`).

### Key Responsibilities

- **Intent-Based APIs**: Expose endpoints designed around user journeys and frontend screens, not internal microservice structures
- **Orchestration**: Coordinate calls to multiple microservices to fulfill frontend requests
- **Data Aggregation**: Combine data from multiple sources into cohesive responses
- **Security Gateway**: Validate JWT tokens, resolve user context, enforce authorization
- **Resilience**: Implement circuit breakers, retries, and timeouts for downstream calls
- **Protocol Translation**: Shield frontend from internal service complexities

## Architecture

```
Frontend (React/Vue) 
    ↓
gss-web-api (BFF)
    ↓
┌─────────────┬─────────────┬─────────────┐
│ admin-      │ catalog-    │ rule-       │
│ service     │ service     │ service     │
└─────────────┴─────────────┴─────────────┘
```

### Technology Stack

- **.NET 10.0** - Runtime framework
- **Wolverine** - CQRS/Messaging framework
- **Polly** - Resilience patterns (circuit breakers, retries, timeouts)
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Request validation
- **JWT Bearer** - Authentication

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for running dependent services)
- VS Code or Visual Studio

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd gss-backend/gss-web-api
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Start dependent services** (from repository root)
   ```bash
   cd ..
   docker-compose up -d admin-service catalog-service rule-service
   ```

4. **Run the BFF**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   - Open browser: `http://localhost:5003/swagger`
   - Explore API endpoints and test them interactively

### Configuration

Configuration follows the standard .NET configuration hierarchy:
1. `appsettings.json` - Base configuration with defaults
2. `appsettings.Development.json` - Development overrides
3. Environment variables - Override any setting (recommended for production)
4. `.env` files - Local development (not committed to git)

**Configuration Sources (in order of precedence):**
```
Environment Variables > .env files > appsettings.{Environment}.json > appsettings.json
```

**Key Configuration Sections:**

#### Service Endpoints
```json
{
  "ServiceEndpoints": {
    "AdminService": "http://localhost:5001",
    "CatalogService": "http://localhost:5002",
    "RuleService": "http://localhost:5004"
  }
}
```

**Environment Variable Format:**
```bash
ServiceEndpoints__AdminService=http://admin-service:8080
ServiceEndpoints__CatalogService=http://catalog-service:8080
```

#### Authentication
```json
{
  "Authentication": {
    "Authority": "https://cognito-idp.ap-south-1.amazonaws.com/...",
    "Audience": "gss-web-api",
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true
  }
}
```

#### Resilience Policies
```json
{
  "Resilience": {
    "RetryCount": 3,
    "RetryDelaySeconds": 2,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30
  }
}
```

#### Using .env Files (Local Development)

1. **Copy the example file:**
   ```bash
   cp .env.example .env.local
   ```

2. **Update with your local values:**
   ```bash
   # .env.local (not committed to git)
   ServiceEndpoints__AdminService=http://localhost:5001
   ServiceEndpoints__CatalogService=http://localhost:5002
   ServiceEndpoints__RuleService=http://localhost:5004
   ```

3. **Load .env file (optional - for VS Code):**
   - Install `DotEnv` extension
   - Or use `dotnet-env` package
   - Or manually export before running: `export $(cat .env.local | xargs)`

**Note:** `.env` and `.env.local` are gitignored. Only `.env.example` is committed as a template.

#### Docker/ECS Configuration

For containerized deployments, use environment variables:

```yaml
# docker-compose.yml
services:
  gss-web-api:
    environment:
      - ServiceEndpoints__AdminService=http://admin-service:8080
      - ServiceEndpoints__CatalogService=http://catalog-service:8080
      - ServiceEndpoints__RuleService=http://rule-service:8080
      - Authentication__Authority=${COGNITO_AUTHORITY}
      - ASPNETCORE_ENVIRONMENT=Production
```

```json
// ECS Task Definition
"environment": [
  {
    "name": "ServiceEndpoints__AdminService",
    "value": "http://admin-service.internal:8080"
  },
  {
    "name": "Authentication__Authority",
    "value": "https://cognito-idp.ap-south-1.amazonaws.com/..."
  }
]
```

## Project Structure

```
gss-web-api/
├── src/
│   ├── api/
│   │   └── controllers/          # API controllers (intent-based)
│   │       ├── AuthenticationController.cs
│   │       ├── UserContextController.cs
│   │       ├── ConfigurationController.cs
│   │       ├── JobController.cs
│   │       └── FileController.cs
│   ├── orchestration/
│   │   └── handlers/              # Wolverine command/query handlers
│   ├── clients/                   # Typed HTTP clients for microservices
│   │   ├── IAdminServiceClient.cs
│   │   ├── ICatalogServiceClient.cs
│   │   └── IRuleServiceClient.cs
│   ├── middleware/                # Custom middleware
│   │   ├── CorrelationIdMiddleware.cs
│   │   └── GlobalExceptionMiddleware.cs
│   ├── infrastructure/            # Cross-cutting concerns
│   │   ├── policies/              # Polly resilience policies
│   │   └── auth/                  # Authentication/authorization
│   ├── dto/                       # Request/response models
│   │   ├── AuthenticationModels.cs
│   │   ├── ConfigurationModels.cs
│   │   ├── UserContextModels.cs
│   │   ├── JobModels.cs
│   │   └── CommonModels.cs
│   └── bootstrap/
│       └── Program.cs             # Application entry point
├── tests/
│   ├── GssWebApi.UnitTests/       # Unit tests
│   ├── GssWebApi.ContractTests/   # API contract tests
│   ├── GssWebApi.IntegrationTests/ # Integration tests
│   └── GssWebApi.E2ETests/        # End-to-end tests
├── docs/
│   └── adr/                       # Architecture Decision Records
├── openapi.yaml                   # OpenAPI 3.0 specification
├── GssWebApi.csproj
├── README.md
└── CONTRIBUTING.md
```

## API Documentation

### OpenAPI Specification

The complete API specification is available in [`openapi.yaml`](./openapi.yaml).

### Key Endpoints

#### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - User logout

#### User Context
- `GET /api/v1/me/context` - Get user context (permissions, dealer info, preferences)
- `GET /api/v1/me/preferences` - Get user preferences

#### Configurations
- `POST /api/v1/configurations` - Create configuration
- `GET /api/v1/configurations/{id}` - Get configuration
- `PUT /api/v1/configurations/{id}` - Update configuration
- `POST /api/v1/configurations/{id}/validate` - Validate configuration

#### Jobs
- `POST /api/v1/jobs/generate-bom` - Generate BOM
- `POST /api/v1/jobs/generate-quote` - Generate quote
- `GET /api/v1/jobs/{jobId}/status` - Get job status

#### Files
- `POST /api/v1/files/upload` - Upload civil layout (DXF/DWG)

#### Design
- `POST /api/v1/design/2d/render` - Generate 2D rendering
- `GET /api/v1/design/3d/{configurationId}` - Get 3D model assets

#### Health
- `GET /health` - Basic health check
- `GET /health/detailed` - Detailed health with dependency status

## Authentication Flow

The BFF uses a two-step authentication process:

1. **Login** (`POST /auth/login`) - Returns JWT access and refresh tokens
2. **Context Resolution** (`GET /me/context`) - Returns user role, permissions, dealer info, and feature flags

**Why Two Steps?**
- JWT tokens contain basic claims (userId, email, role)
- Full context (permissions, feature flags) comes from `/me/context`
- Separates authentication (who you are) from authorization (what you can do)

**Frontend Flow:**
```javascript
// 1. Login
const { accessToken, refreshToken } = await api.post('/auth/login', { email, password });
storeTokens(accessToken, refreshToken);

// 2. Get user context
const userContext = await api.get('/me/context', {
  headers: { Authorization: `Bearer ${accessToken}` }
});

// 3. Use context for UI rendering
if (userContext.permissions.includes('configurations.create')) {
  showCreateButton();
}

if (userContext.featureFlags.enable3DView) {
  show3DViewOption();
}
```

**Detailed Documentation:** See [`docs/AUTHENTICATION_FLOW.md`](./docs/AUTHENTICATION_FLOW.md) for:
- Complete sequence diagrams
- Permission model and examples
- Feature flags usage
- Token refresh flow
- Frontend implementation guide

## Entitlements System

The BFF returns **entitlements** in the `/me/context` response, which control what features, product groups, and export formats are available to the user.

**Entitlements include:**
- **Allowed Product Groups** - Which product categories user can work with (e.g., PALLET_RACKING, SHELVING)
- **Allowed Export Formats** - Which export formats are available (e.g., PDF, PNG, DXF, DWG)
- **Feature Flags** - Which UI features are enabled (e.g., 3D view, bulk import, AI assistant)
- **Limits** - Maximum configurations, file sizes, concurrent users, etc.
- **Subscription Tier** - User's subscription level (BASIC, PROFESSIONAL, ENTERPRISE)

**Example Response:**
```json
{
  "userId": "...",
  "role": "DEALER",
  "permissions": ["configurations.create", "bom.generate"],
  "entitlements": {
    "allowedProductGroups": ["PALLET_RACKING", "SHELVING"],
    "allowedExportFormats": ["PDF", "PNG"],
    "maxConfigurations": 50,
    "subscriptionTier": "PROFESSIONAL",
    "enable3DView": true,
    "enableBulkImport": false
  }
}
```

**Frontend Usage:**
```javascript
// Check if product group is allowed
if (userContext.entitlements.allowedProductGroups.includes('MEZZANINE')) {
  showMezzanineOption();
} else {
  showUpgradePrompt('MEZZANINE', 'PROFESSIONAL');
}

// Check if export format is available
if (userContext.entitlements.allowedExportFormats.includes('DXF')) {
  enableDXFExport();
}

// Check configuration limit
if (configCount >= userContext.entitlements.maxConfigurations) {
  showUpgradePrompt('More Configurations', 'ENTERPRISE');
}
```

**System Documentation:** See [`../docs/ENTITLEMENTS.md`](../docs/ENTITLEMENTS.md) and [`../docs/ADVANCED_ENTITLEMENTS.md`](../docs/ADVANCED_ENTITLEMENTS.md) for:
- Complete entitlement architecture
- Database schema (role, dealer, user entitlements)
- Multi-role support
- Subscription tier management
- Future-proof JSONB schema design

## User Journey Mapping

All API endpoints are mapped to user journeys in [`docs/User_Journeys/`](../docs/User_Journeys/):

- **01_Login** → Authentication endpoints, user context
- **02_Create_Configuration_From_Enquiry** → Configuration creation, BOM/quote generation
- **03_Update_Configuration** → Configuration updates, validation
- **04_Import_Civil_Layout** → File upload and processing
- **05_2D_View** → 2D rendering
- **06_3D_View** → 3D model retrieval

## Testing

### Running Tests

```bash
# Unit tests
dotnet test tests/GssWebApi.UnitTests

# Contract tests
dotnet test tests/GssWebApi.ContractTests

# Integration tests
dotnet test tests/GssWebApi.IntegrationTests

# E2E tests (requires all services running)
docker-compose up -d
dotnet test tests/GssWebApi.E2ETests

# All tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Coverage Target

- **Unit Tests**: 80%+ coverage
- **Contract Tests**: All API endpoints validated against OpenAPI spec
- **Integration Tests**: All orchestration flows tested with mocked microservices
- **E2E Tests**: Critical user journeys tested end-to-end

## Deployment

### Docker

**Build the image:**
```bash
docker build -t gss-web-api:latest .
```

**Run standalone:**
```bash
docker run -p 5003:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ServiceEndpoints__AdminService=http://admin-service:8080 \
  -e ServiceEndpoints__CatalogService=http://catalog-service:8080 \
  -e ServiceEndpoints__RuleService=http://rule-service:8080 \
  -e Authentication__Authority=https://cognito-idp.ap-south-1.amazonaws.com/... \
  gss-web-api:latest
```

**Run with docker-compose (includes all services):**
```bash
# From gss-web-api directory
docker-compose up -d

# View logs
docker-compose logs -f gss-web-api

# Stop all services
docker-compose down
```

**Access the BFF:**
- API: http://localhost:5003
- Swagger: http://localhost:5003/swagger
- Health: http://localhost:5003/health

### AWS ECS Fargate

The service is designed for deployment on AWS ECS Fargate. See [`docs/AWS_Architecture_Planning_Document.md`](../docs/AWS_Architecture_Planning_Document.md) for details.

**Key Deployment Considerations:**
- **Stateless design** - No local state, horizontally scalable
- **Health checks** - Configured at `/health` for ECS target group
- **Environment variables** - All configuration via env vars (no secrets in image)
- **Secrets management** - Use AWS Secrets Manager for sensitive data
- **Logging** - Structured logs sent to CloudWatch via awslogs driver
- **Port** - Container listens on port 8080 (configurable via `ASPNETCORE_URLS`)

**ECS Task Definition Example:**
```json
{
  "family": "gss-web-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [
    {
      "name": "gss-web-api",
      "image": "123456789012.dkr.ecr.ap-south-1.amazonaws.com/gss-web-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ServiceEndpoints__AdminService",
          "value": "http://admin-service.internal:8080"
        },
        {
          "name": "ServiceEndpoints__CatalogService",
          "value": "http://catalog-service.internal:8080"
        },
        {
          "name": "ServiceEndpoints__RuleService",
          "value": "http://rule-service.internal:8080"
        }
      ],
      "secrets": [
        {
          "name": "Authentication__Authority",
          "valueFrom": "arn:aws:secretsmanager:ap-south-1:123456789012:secret:gss/cognito-authority"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/gss-web-api",
          "awslogs-region": "ap-south-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

## Observability

### Logging

Structured logging with Serilog:
- All requests logged with correlation IDs
- Downstream service calls tracked
- Errors logged with context

### Metrics

Key metrics to monitor:
- Request rate and latency (p50, p95, p99)
- Error rate by endpoint
- Downstream service health
- Circuit breaker state

### Tracing

Correlation IDs propagated to all downstream services via `X-Correlation-ID` header.

## Security

- **Authentication**: JWT Bearer tokens validated at ingress
- **Authorization**: Role-based and permission-based checks
- **Secrets**: Never expose internal service URLs or credentials
- **CORS**: Configured for specific frontend origins
- **Rate Limiting**: (TODO) Implement rate limiting per user/IP

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md) for development guidelines and contribution standards.

## License

Proprietary - Godrej Storage Solutions

## Support

For issues or questions:
- **Email**: support@gss.com
- **Internal Slack**: #gss-backend-support
