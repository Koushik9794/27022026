# GSS Backend

Enterprise-grade backend services for the GSS Warehouse Design Configurator platform.

> [!NOTE]
> **👋 New Developer?** Start with the [Onboarding Guide](docs/onboarding-guide.md) for a structured introduction to the codebase. It will guide you through setup, architecture, and your first contribution.

## 🏗️ Architecture Overview

This monorepo contains microservices built with Domain-Driven Design (DDD) principles, CQRS patterns, and modern .NET practices.

```
gss-backend/
├── Services/                    # Microservices
│   ├── admin-service           # User & authentication management
│   ├── catalog-service         # SKU, Pallet, MHE catalog
│   ├── rule-service            # Business rules engine
│   ├── file-service            # File import/export operations
│   ├── configuration-service   # Warehouse configuration state
│   └── bom-service             # Bill of Materials generation
├── gss-common/                 # Shared libraries & contracts
├── gss-web-api/                # BFF (Backend for Frontend)
├── docs/                       # Architecture & documentation
└── infrastructure/             # DevOps & tooling
```

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (recommended)
- [PostgreSQL 13+](https://www.postgresql.org/download/) (if running locally without Docker)
- [Git](https://git-scm.com/downloads)

#### Windows Developers: WSL2 Setup (Highly Recommended)

For the best Docker experience on Windows, use WSL2:

**Why WSL2?**
- ✅ **10x faster** file I/O performance
- ✅ Better Docker compatibility
- ✅ Native Linux tooling
- ✅ Consistent with production environment

**Setup Steps**:

1. **Install WSL2**:
   ```powershell
   # Run in PowerShell as Administrator
   wsl --install
   
   # Restart your computer
   ```

2. **Install Ubuntu** (recommended distribution):
   ```powershell
   wsl --install -d Ubuntu-22.04
   ```

3. **Set WSL2 as default**:
   ```powershell
   wsl --set-default-version 2
   ```

4. **Install Docker Desktop**:
   - Download from https://www.docker.com/products/docker-desktop
   - During installation, ensure "Use WSL 2 instead of Hyper-V" is checked
   - In Docker Desktop settings, enable WSL2 integration with Ubuntu

5. **Clone repository in WSL**:
   ```bash
   # Open Ubuntu terminal
   cd ~
   git clone <repository-url>
   cd gss-backend
   ```

6. **Install .NET 10 in WSL**:
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 10.0
   
   # Add to PATH (add to ~/.bashrc)
   export DOTNET_ROOT=$HOME/.dotnet
   export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
   ```

**VS Code with WSL**:
- Install "Remote - WSL" extension
- Open project: `code .` from WSL terminal
- VS Code will run in WSL context automatically

> [!TIP]
> **Performance Tip**: Always clone and work with repositories inside WSL filesystem (`~/projects/`), not in Windows filesystem (`/mnt/c/`). This provides 10x better performance.

> [!NOTE]
> **Alternative**: If you prefer not to use WSL2, Docker Desktop on Windows works but with slower file I/O. See [Local Development Strategy](docs/Local_Development_Environment_Strategy.md) for details.

### Option 1: Docker Compose (Recommended)

Start all services with a single command:

```powershell
# Clone the repository
git clone <repository-url>
cd gss-backend

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

**Service Endpoints:**
- Admin Service: http://localhost:5001/swagger
- Catalog Service: http://localhost:5002/swagger
- Rule Service: http://localhost:5000/swagger
- File Service: http://localhost:5003/swagger
- Configuration Service: http://localhost:5004/swagger
- BOM Service: http://localhost:5005/swagger

### Option 2: Run Individual Services

Each service can be run independently:

```powershell
# Navigate to service directory
cd Services/admin-service

# Restore dependencies
dotnet restore

# Run the service
dotnet run

# Access Swagger UI
# http://localhost:5001/swagger
```

See individual service READMEs for detailed setup instructions.

## 📦 Services Catalog

### Core Services

| Service | Port | Description | Status |
|---------|------|-------------|--------|
| **admin-service** | 5001 | User management, roles, authentication | ✅ Active |
| **catalog-service** | 5002 | Taxonomy, SKU Types, Pallet Types, MHE | ✅ Active |
| **rule-service** | 5000 | Business rules engine & validation | ✅ Active |

### Planned Services

| Service | Port | Description | Status |
|---------|------|-------------|--------|
| **file-service** | 5003 | File upload/download, GLB models, Excel/CSV | 🚧 Planned |
| **configuration-service** | 5004 | Warehouse configuration state & versioning | 🚧 Planned |
| **bom-service** | 5005 | Bill of Materials generation | 🚧 Planned |

### Catalog Service API

| Module | Route | Entities |
|--------|-------|----------|
| **Taxonomy** | `/api/v1/taxonomy/*` | ComponentCategory, ComponentType, ProductGroup |
| **SKU Types** | `/api/v1/sku-types` | Customer goods types (Box, Drum, Bin) |
| **Pallet Types** | `/api/v1/pallet-types` | Pallet standards (EURO, US, UK) |

### Supporting Components

- **gss-common**: Shared abstractions, contracts, and utilities
- **gss-web-api**: Backend for Frontend (BFF) layer

## 🏛️ Architecture Principles

All services follow consistent architectural patterns:

### Domain-Driven Design (DDD)

```
service/
├── src/
│   ├── api/                 # REST controllers
│   ├── application/         # CQRS (Commands, Queries, Handlers)
│   ├── domain/              # Business logic (Aggregates, Entities, Value Objects)
│   └── infrastructure/      # Technical concerns (Persistence, External APIs)
└── tests/                   # Unit, Integration, Contract tests
```

### Key Patterns

- **CQRS**: Separate read and write operations using Wolverine
- **Repository Pattern**: Abstract data access with Dapper
- **Value Objects**: Encapsulate domain concepts with validation
- **Domain Events**: Communicate state changes within bounded contexts
- **Migrations**: Database versioning with FluentMigrator
- **Attribute Schema**: Flexible JSON schemas for extensible entity attributes

### Technology Stack

- **.NET 10**: Modern C# with minimal APIs
- **PostgreSQL**: Relational database with JSONB support
- **Dapper**: Lightweight ORM for performance
- **WolverineFx**: In-process messaging for CQRS
- **FluentValidation**: Declarative validation rules
- **FluentMigrator**: Database migration framework
- **Swashbuckle**: Swagger/OpenAPI documentation
- **Docker**: Containerization for all services

## 🛠️ Development Workflows

### Building the Solution

```powershell
# Build entire solution
dotnet build gss-backend.sln

# Build specific service
cd Services/admin-service
dotnet build
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run tests for specific service
cd Services/admin-service
dotnet test tests/

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Database Migrations

Migrations run automatically on service startup. To run manually:

```powershell
cd Services/admin-service
dotnet run  # Migrations execute before service starts
```

### Adding a New Service

1. Copy an existing service structure (e.g., `admin-service`)
2. Update namespace and project references
3. Define domain models and business logic
4. Implement CQRS commands and queries
5. Create API controllers
6. Add to `docker-compose.yml`
7. Update this README

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## 🧪 Testing

### For Developers

```powershell
# Unit tests (domain + application layers)
dotnet test Services/admin-service/tests/

# Integration tests (with test database)
dotnet test --filter Category=Integration
```

### For Testers

See [docs/tester-guide.md](docs/tester-guide.md) for:
- Setting up local test environment
- API testing with Swagger UI
- Test scenarios and data
- Postman collections

See [docs/testing-guide.md](docs/testing-guide.md) for comprehensive testing strategy.

## 📚 Documentation

### Architecture & Design

- [Architecture Decision Records (ADRs)](docs/adr/) - Key architectural decisions
- [Service Design Checklist](docs/service-design-checklist.md) - **Production readiness requirements**
- [Business Rules Engine Architecture](docs/Business_Rules_Engine_Architecture.md)
- [AWS Architecture Planning](docs/AWS_Architecture_Planning_Document.md)
- [Domain Models](docs/Domain_Models.puml)

### Development Guides

- **[Onboarding Guide](docs/onboarding-guide.md)** - **👋 Start here if you're new!**
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [WSL2 Setup Guide](docs/wsl2-setup-guide.md) - **Windows WSL2 setup for better Docker performance**
- [Coding Standards](docs/coding-standards.md) - **C# coding standards and best practices**
- [Testing Guide](docs/testing-guide.md) - Testing strategy
- [Tester Guide](docs/tester-guide.md) - Tester documentation
- [Documentation Naming](docs/documentation-naming-conventions.md) - File naming standards
- [Local Development Strategy](docs/Local_Development_Environment_Strategy.md)

### Service Documentation

Each service has detailed documentation:
- [Admin Service README](Services/admin-service/README.md)
- [Catalog Service README](Services/catalog-service/README.md)
- [Rule Service README](Services/rule-service/README.md)

### API Documentation

- **Swagger UI**: Available at `http://localhost:<port>/swagger` for each service
- **OpenAPI Specs**: Located in `openapi/` directory

## 🔧 Configuration

### Environment Variables

Each service uses environment variables for configuration:

```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=admin_service;...

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5001

# Logging
Logging__LogLevel__Default=Information
```

### Configuration Hierarchy

1. `appsettings.json` - Default configuration
2. `appsettings.{Environment}.json` - Environment-specific
3. `.env` file - Local development (not committed)
4. Environment variables - Docker/Cloud overrides
5. AWS Secrets Manager - Production secrets

### Local Development Setup

```powershell
# Copy environment template for each service
cd Services/admin-service
cp .env.example .env

# Edit with your local settings
notepad .env
```

## 🐛 Troubleshooting

### Service won't start

```powershell
# Check if port is already in use
netstat -ano | findstr :5001

# Check Docker logs
docker-compose logs admin_service

# Verify database connection
psql -U postgres -h localhost -p 5432
```

### Database migration errors

```powershell
# Drop and recreate database
psql -U postgres -c "DROP DATABASE admin_service;"
psql -U postgres -c "CREATE DATABASE admin_service;"

# Restart service (migrations run automatically)
docker-compose restart admin_service
```

### Build errors

```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Docker issues

```powershell
# Remove all containers and volumes
docker-compose down -v

# Rebuild images
docker-compose build --no-cache

# Start fresh
docker-compose up -d
```

## 🤝 Contributing

We welcome contributions! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for:

- Code style and conventions
- Git workflow (branching, commits, PRs)
- Testing requirements
- Documentation standards
- Review process

## 📄 License

Proprietary - GSS

## 🔗 Related Projects

- **gss-frontend**: React-based web application
- **gss-mobile**: Mobile applications for iOS/Android

## 📞 Support

For questions or issues:
- Create an issue in the repository
- Contact the development team
- See internal documentation wiki

---

**Last Updated**: January 2026  
**Maintained By**: GSS Development Team
