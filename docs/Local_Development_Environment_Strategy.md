# Local Development Environment Strategy
## 100% Local Development with Docker Compose

**Version:** 2.0  
**Date:** January 2026  
**Audience:** Development Teams, DevOps Engineers

---

## Table of Contents

1. [Why Local-First Development](#why-local-first-development)
2. [Architecture Overview](#architecture-overview)
3. [Complete Local Stack](#complete-local-stack)
4. [Quick Start](#quick-start)
5. [Developer Workflow](#developer-workflow)
6. [Cost Savings Analysis](#cost-savings-analysis)
7. [Testing Strategy](#testing-strategy)
8. [Troubleshooting](#troubleshooting)

---

## Why Local-First Development

### Core Principles

**1. Zero Cloud Costs for Development**
- Developers work 100% locally - no AWS charges
- Cloud environments only for staging/production
- Estimated savings: **$2,000-5,000/month** per team

**2. Production Parity**
- Same PostgreSQL version (15)
- Same .NET version (10)
- Same service architecture
- Same message patterns (Wolverine)

**3. Fast Feedback Loop**
- Code changes visible in 2-3 seconds
- No deployment wait times
- Instant debugging with breakpoints
- Hot reload enabled

**4. Work Offline**
- No internet required after initial setup
- No AWS credentials needed
- All services run in Docker locally
- Complete autonomy

**5. Consistent Environments**
- Every developer has identical setup
- `docker-compose.yml` in version control
- One command starts everything
- No "works on my machine" issues

---

## Architecture Overview

### Current GSS Backend Services

```
┌─────────────────────────────────────────────────────────────┐
│                  GSS Backend - Local Stack                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              6 Microservices                         │   │
│  │                                                      │   │
│  │  ┌──────────────┐  ┌──────────────┐                │   │
│  │  │ admin-service│  │catalog-service│               │   │
│  │  │  (Port 5001) │  │  (Port 5002) │                │   │
│  │  └──────┬───────┘  └──────┬───────┘                │   │
│  │         │                  │                         │   │
│  │  ┌──────────────┐  ┌──────────────┐                │   │
│  │  │ rule-service │  │ file-service │                │   │
│  │  │  (Port 5000) │  │  (Port 5003) │                │   │
│  │  └──────┬───────┘  └──────┬───────┘                │   │
│  │         │                  │                         │   │
│  │  ┌──────────────┐  ┌──────────────┐                │   │
│  │  │configuration │  │  bom-service │                │   │
│  │  │   -service   │  │  (Port 5005) │                │   │
│  │  │  (Port 5004) │  │              │                │   │
│  │  └──────┬───────┘  └──────┬───────┘                │   │
│  └─────────┼────────────────┼────────────────────────┘   │
│            │                 │                             │
│            ▼                 ▼                             │
│  ┌──────────────────────────────────────────────────┐    │
│  │           Shared Infrastructure                   │    │
│  │                                                   │    │
│  │  ┌──────────────┐  ┌──────────────┐             │    │
│  │  │  PostgreSQL  │  │    Redis     │             │    │
│  │  │   (5432)     │  │   (6379)     │             │    │
│  │  └──────────────┘  └──────────────┘             │    │
│  │                                                   │    │
│  │  ┌──────────────┐  ┌──────────────┐             │    │
│  │  │     Seq      │  │   MinIO/S3   │             │    │
│  │  │   (8081)     │  │   (9000)     │             │    │
│  │  └──────────────┘  └──────────────┘             │    │
│  └───────────────────────────────────────────────────┘    │
│                                                            │
│  ┌──────────────────────────────────────────────────┐     │
│  │         Development Tools (Optional)             │     │
│  │  ┌──────────────┐  ┌──────────────┐             │     │
│  │  │   pgAdmin    │  │ Redis Insight│             │     │
│  │  │   (8090)     │  │   (8091)     │             │     │
│  │  └──────────────┘  └──────────────┘             │     │
│  └──────────────────────────────────────────────────┘     │
└────────────────────────────────────────────────────────────┘
```

### Service Responsibilities

| Service | Purpose | Port | Database |
|---------|---------|------|----------|
| **admin-service** | User authentication, roles, permissions | 5001 | PostgreSQL |
| **catalog-service** | SKUs, parts, pallets, product groups | 5002 | PostgreSQL |
| **rule-service** | Business rules engine, validation | 5000 | PostgreSQL |
| **file-service** | File upload/download, GLB models | 5003 | PostgreSQL + MinIO |
| **configuration-service** | Warehouse configuration state | 5004 | PostgreSQL |
| **bom-service** | Bill of Materials generation | 5005 | PostgreSQL |

---

## Complete Local Stack

### Prerequisites

**Required:**
- **Docker Desktop** (with WSL2 on Windows)
- **.NET 10 SDK**
- **Git**
- **VS Code** (recommended)

**Optional:**
- **Postman** or **Insomnia** for API testing
- **DBeaver** or **pgAdmin** for database exploration

### Project Structure

```
gss-backend/
├── docker-compose.yml              # Main orchestration
├── .env.example                    # Environment template
├── .env                           # Local config (gitignored)
│
├── Services/                       # Microservices
│   ├── admin-service/
│   │   ├── Dockerfile
│   │   ├── AdminService.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── src/
│   │       ├── api/              # Controllers
│   │       ├── application/      # Commands, Queries, Handlers
│   │       ├── domain/           # Aggregates, Value Objects
│   │       └── infrastructure/   # Repositories, Migrations
│   ├── catalog-service/
│   ├── rule-service/
│   ├── file-service/
│   ├── configuration-service/
│   └── bom-service/
│
├── gss-web-api/                   # BFF (Backend for Frontend)
│   └── (aggregates microservices)
│
├── docs/                          # Documentation
│   ├── User_Journeys/
│   ├── adr/
│   └── *.md
│
└── tests/                         # Tests
    ├── unit/
    ├── integration/
    └── e2e/
```

---

## Quick Start

### 1. Clone and Setup

```bash
# Clone repository
git clone <repository-url>
cd gss-backend

# Copy environment template
cp .env.example .env

# (Optional) Customize .env for your local setup
```

### 2. Start All Services

```bash
# Start everything with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f

# Check service health
docker-compose ps
```

### 3. Verify Services

All services should be running and healthy:

```bash
# Check all services
curl http://localhost:5001/health  # admin-service
curl http://localhost:5002/health  # catalog-service
curl http://localhost:5000/health  # rule-service
curl http://localhost:5003/health  # file-service
curl http://localhost:5004/health  # configuration-service
curl http://localhost:5005/health  # bom-service
```

### 4. Access Swagger UIs

Open in browser:
- Admin Service: http://localhost:5001/swagger
- Catalog Service: http://localhost:5002/swagger
- Rule Service: http://localhost:5000/swagger
- File Service: http://localhost:5003/swagger
- Configuration Service: http://localhost:5004/swagger
- BOM Service: http://localhost:5005/swagger

### 5. Access Development Tools

- **Seq (Logs)**: http://localhost:8081
- **pgAdmin (Database)**: http://localhost:8090
- **Redis Insight**: http://localhost:8091
- **MinIO Console**: http://localhost:9001

---

## Developer Workflow

### Daily Development

#### Option 1: Run Everything in Docker (Recommended for Full Stack)

```bash
# Start all services
docker-compose up -d

# View logs for specific service
docker-compose logs -f admin-service

# Restart a service after changes
docker-compose restart admin-service

# Rebuild after code changes
docker-compose up -d --build admin-service
```

#### Option 2: Run One Service Locally, Rest in Docker (Recommended for Active Development)

```bash
# Start infrastructure and other services
docker-compose up -d postgres redis seq

# Run your service locally for debugging
cd Services/admin-service
dotnet run

# Or with hot reload
dotnet watch run
```

**Benefits:**
- Fast feedback (no Docker rebuild)
- Full debugging with breakpoints
- Hot reload on code changes
- Other services still available

### Making Changes

#### 1. Code Changes

```bash
# Make your changes in VS Code
# If running in Docker, rebuild:
docker-compose up -d --build <service-name>

# If running locally with dotnet watch:
# Changes auto-reload in 2-3 seconds
```

#### 2. Database Changes

```bash
# Add a new FluentMigrator migration
cd Services/admin-service
dotnet ef migrations add YourMigrationName

# Apply migrations (auto-applied on service start)
# Or manually:
dotnet run -- migrate
```

#### 3. Testing Changes

```bash
# Run unit tests
dotnet test

# Run integration tests (requires Docker services)
docker-compose up -d postgres redis
dotnet test --filter Category=Integration

# Run specific service tests
cd Services/admin-service
dotnet test
```

### Debugging

#### VS Code Debugging

1. Open service folder in VS Code
2. Press `F5` (launch configuration included)
3. Set breakpoints
4. Test via Swagger UI or Postman

#### Docker Debugging

```bash
# View service logs
docker-compose logs -f admin-service

# Execute commands in container
docker-compose exec admin-service bash

# View database
docker-compose exec postgres psql -U postgres -d admin_service_db
```

---

## Cost Savings Analysis

### Cloud Costs Without Local Development

**Scenario**: 5 developers, each using cloud dev environment

| Resource | Monthly Cost | Annual Cost |
|----------|--------------|-------------|
| EC2 instances (6 services × 5 devs) | $900 | $10,800 |
| RDS PostgreSQL (5 instances) | $500 | $6,000 |
| S3 storage and requests | $100 | $1,200 |
| Data transfer | $200 | $2,400 |
| **Total** | **$1,700** | **$20,400** |

### With 100% Local Development

| Resource | Monthly Cost | Annual Cost |
|----------|--------------|-------------|
| Developer machines (already owned) | $0 | $0 |
| Docker Desktop (free for small teams) | $0 | $0 |
| **Total** | **$0** | **$0** |

### **Annual Savings: $20,400** 💰

### Additional Benefits

- **Faster development**: No network latency
- **Work offline**: No internet required
- **No AWS complexity**: Simpler onboarding
- **Consistent environments**: No configuration drift

---

## Testing Strategy

### Test Pyramid

```
        /\
       /  \      E2E Tests (Few)
      /────\     - Full user journeys
     /      \    - Critical paths only
    /────────\   
   /          \  Integration Tests (Some)
  /────────────\ - API + Database
 /              \- Service interactions
/────────────────\
    Unit Tests    (Many)
    - Domain logic
    - Business rules
    - Validators
```

### Running Tests Locally

#### Unit Tests (Fast, No Dependencies)

```bash
# Run all unit tests
dotnet test --filter Category=Unit

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific service
cd Services/admin-service
dotnet test --filter Category=Unit
```

#### Integration Tests (Require Docker Services)

```bash
# Start required infrastructure
docker-compose up -d postgres redis

# Run integration tests
dotnet test --filter Category=Integration

# Stop infrastructure
docker-compose down
```

#### End-to-End Tests (Full Stack)

```bash
# Start all services
docker-compose up -d

# Run E2E tests (if using Playwright/Selenium)
cd tests/e2e
npm test

# Or manual testing via Swagger UIs
```

### Test Data Management

#### Seed Data

```bash
# Database seed data is in:
# infrastructure/postgres/seed-data/

# To reset database with seed data:
docker-compose down -v  # Remove volumes
docker-compose up -d    # Recreate with seed data
```

#### Test Isolation

- Each test should be independent
- Use transactions for database tests
- Clean up after integration tests
- Use unique IDs to avoid conflicts

---

## Troubleshooting

### Common Issues

#### Services Won't Start

```bash
# Check Docker is running
docker --version
docker-compose --version

# Check for port conflicts
netstat -an | findstr "5001"  # Windows
lsof -i :5001                 # Mac/Linux

# View service logs
docker-compose logs <service-name>

# Restart everything
docker-compose down
docker-compose up -d
```

#### Database Connection Issues

```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check connection
docker-compose exec postgres psql -U postgres -l

# View PostgreSQL logs
docker-compose logs postgres

# Reset database
docker-compose down -v
docker-compose up -d postgres
```

#### Slow Performance on Windows

**Solution**: Use WSL2!

```bash
# Check WSL version
wsl --list --verbose

# Should show WSL 2, not WSL 1
# If WSL 1, upgrade:
wsl --set-version Ubuntu 2

# Move Docker to WSL2
# Docker Desktop > Settings > General > Use WSL 2 based engine
```

**Performance improvement**: 10x faster!

#### Out of Disk Space

```bash
# Clean up Docker
docker system prune -a --volumes

# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune
```

#### Service Build Fails

```bash
# Clear Docker cache
docker-compose build --no-cache <service-name>

# Check .NET SDK version
dotnet --version  # Should be 10.x

# Restore NuGet packages
cd Services/<service-name>
dotnet restore
dotnet build
```

### Getting Help

1. **Check logs first**: `docker-compose logs -f <service-name>`
2. **Check health endpoints**: `curl http://localhost:5001/health`
3. **Verify environment**: Check `.env` file
4. **Ask team**: Share logs and error messages
5. **Check documentation**: [README.md](../README.md), [CONTRIBUTING.md](../CONTRIBUTING.md)

---

## Best Practices

### DO ✅

- **Start fresh daily**: `docker-compose down && docker-compose up -d`
- **Use hot reload**: `dotnet watch run` for active development
- **Run tests locally**: Before pushing code
- **Check health endpoints**: Verify services are running
- **Use Swagger UIs**: For API testing
- **Keep Docker updated**: Latest stable version
- **Use WSL2 on Windows**: 10x performance boost

### DON'T ❌

- **Don't commit `.env`**: Use `.env.example` as template
- **Don't use cloud for dev**: Keep costs zero
- **Don't skip tests**: Run locally before PR
- **Don't ignore health checks**: They indicate real issues
- **Don't run without Docker**: Inconsistent environments
- **Don't use production data**: Use seed data only

---

## Advanced Topics

### Custom Configuration

Edit `.env` file for local customization:

```bash
# Database
POSTGRES_PASSWORD=your-password

# Service Ports (if conflicts)
ADMIN_SERVICE_PORT=5001
CATALOG_SERVICE_PORT=5002

# Logging Level
ASPNETCORE_ENVIRONMENT=Development
SERILOG__MINIMUMLEVEL=Debug
```

### Multiple Environments

```bash
# Development (default)
docker-compose up -d

# Testing
docker-compose -f docker-compose.test.yml up -d

# Production-like
docker-compose -f docker-compose.prod.yml up -d
```

### Performance Tuning

```yaml
# docker-compose.override.yml (local only)
services:
  admin-service:
    environment:
      - DOTNET_gcServer=1
      - DOTNET_gcConcurrent=1
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
```

---

## Summary

### Key Takeaways

1. **100% local development** = **$20,000+ annual savings**
2. **One command** starts entire stack: `docker-compose up -d`
3. **Fast feedback**: 2-3 second hot reload
4. **Production parity**: Same stack as cloud
5. **Work offline**: No internet needed
6. **Consistent**: Every developer identical setup

### Next Steps

1. ✅ Complete [WSL2 Setup](wsl2-setup-guide.md) (Windows only)
2. ✅ Clone repository and run `docker-compose up -d`
3. ✅ Access Swagger UIs and test APIs
4. ✅ Read [Onboarding Guide](onboarding-guide.md)
5. ✅ Make your first code change
6. ✅ Run tests locally

---

**Questions?** Check [README.md](../README.md) or ask the team!

**Last Updated**: January 2026  
**Maintained By**: GSS Development Team
