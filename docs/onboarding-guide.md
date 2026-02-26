# New Developer Onboarding Guide

Welcome to the GSS Backend team! This guide will help you get up and running quickly.

## 📖 Reading Order (Start Here!)

Follow this sequence for the best onboarding experience:

### 1. Setup & Overview

**[README.md](../README.md)** ⭐ **START HERE**
- Project overview and architecture
- Quick start with Docker
- Service catalog

**[WSL2 Setup Guide](wsl2-setup-guide.md)** (Windows only)
- Essential for Windows developers
- 10x better Docker performance

**Get the code running**
```bash
# Clone and start services
git clone <repository-url>
cd gss-backend
docker-compose up -d

# Verify all services are running
docker-compose ps

# Access Swagger UIs
# Admin: http://localhost:5001/swagger
# Catalog: http://localhost:5002/swagger
# Rule: http://localhost:5000/swagger
```

### 2. Understanding the Architecture

**[CONTRIBUTING.md](../CONTRIBUTING.md)**
- Development workflow
- Git branching strategy
- DDD architecture overview

**[Coding Standards](coding-standards.md)**
- C# conventions
- DDD patterns with examples
- Code organization

**Pick ONE service to explore**
- [Admin Service README](../Services/admin-service/README.md)
- [Catalog Service README](../Services/catalog-service/README.md)
- [Rule Service README](../Services/rule-service/README.md)

Read the README, explore the code structure, run the service locally.

### 3. Production Standards

**[Service Design Checklist](service-design-checklist.md)**
- Production readiness requirements
- AWS Well-Architected principles
- Reference when building features

**[Testing Guide](testing-guide.md)**
- Testing philosophy
- Unit, integration, contract tests
- Code coverage requirements

### 4. Reference Documentation

**Architecture Documentation**
- [Business Rules Engine Architecture](Business_Rules_Engine_Architecture.md)
- [AWS Architecture Planning](AWS_Architecture_Planning_Document.md)
- [Domain Models](Domain_Models.puml)

**[Documentation Naming Conventions](documentation-naming-conventions.md)**
- File naming standards

---

## 🎯 Onboarding Goals

### Setup
- ✅ Complete environment setup (WSL2 if Windows)
- ✅ Run all services with `docker-compose up -d`
- ✅ Access all Swagger UIs
- ✅ Read README.md and CONTRIBUTING.md

### Understanding
- ✅ Read Coding Standards
- ✅ Explore one service codebase (admin-service recommended)
- ✅ Understand DDD layers (Domain, Application, Infrastructure, API)
- ✅ Run tests: `dotnet test`

### First Contribution
- ✅ Make a small code change (add a field, update validation)
- ✅ Write a unit test
- ✅ Create a PR following CONTRIBUTING.md guidelines
- ✅ Get code review feedback

### Success Criteria
- ✅ Understand the monorepo structure
- ✅ Know where to find documentation
- ✅ Comfortable with Docker workflow
- ✅ Understand DDD architecture
- ✅ Made your first contribution

---

## 🛠️ Essential Tools

### Required
- **VS Code** with extensions:
  - Remote - WSL (Windows)
  - C# Dev Kit
  - Docker
- **Git** - Version control
- **Docker Desktop** - Container runtime
- **.NET 10 SDK** - Development framework

### Recommended
- **Postman** - API testing
- **DBeaver** or **pgAdmin** - Database exploration
- **Seq** - Log viewer (http://localhost:8081)

---

## 🗺️ Codebase Navigation

### Monorepo Structure

```
gss-backend/
├── Services/              # Each service is independent
│   ├── admin-service/    # 👈 Start here - simplest service
│   ├── catalog-service/
│   ├── rule-service/
│   ├── file-service/     # 🚧 In development
│   ├── configuration-service/
│   └── bom-service/
├── gss-common/           # Shared code (use sparingly)
├── gss-web-api/          # BFF layer
└── docs/                 # All documentation
```

### Service Structure (All services follow this pattern)

```
admin-service/
├── src/
│   ├── api/              # Controllers - thin, delegate to handlers
│   ├── application/      # CQRS - Commands, Queries, Handlers
│   ├── domain/           # Business logic - Aggregates, Value Objects
│   └── infrastructure/   # Technical - Repositories, Migrations
└── tests/                # Unit, Integration, Contract tests
```

### Finding Things

| I want to... | Look in... |
|--------------|------------|
| Add a new endpoint | `src/api/` controllers |
| Add business logic | `src/domain/` aggregates |
| Add a command/query | `src/application/commands` or `queries` |
| Add validation | `src/application/validators` |
| Add database query | `src/infrastructure/persistence` |
| Add migration | `src/infrastructure/migrations` |
| Add tests | `tests/` directory |

---

## 💡 Quick Tips

### Docker Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f admin_service

# Restart a service
docker-compose restart admin_service

# Stop all services
docker-compose down

# Rebuild after code changes
docker-compose up -d --build
```

### .NET Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run specific service
cd Services/admin-service
dotnet run

# Watch mode (auto-rebuild)
dotnet watch run
```

### Git Workflow

```bash
# Create feature branch
git checkout develop
git pull
git checkout -b feature/add-user-validation

# Make changes, commit
git add .
git commit -m "feat(admin): add email validation"

# Push and create PR
git push origin feature/add-user-validation
```

---

## 🤔 Common Questions

### Q: Which service should I start with?
**A:** Start with `admin-service` - it's the simplest and has the clearest DDD structure.

### Q: Do I need to understand all services?
**A:** No! Focus on one service initially. The architecture is consistent across all services.

### Q: How do I debug a service?
**A:** 
1. Open service folder in VS Code
2. Press F5 (launch configuration included)
3. Set breakpoints
4. Test via Swagger UI

### Q: Where do I ask questions?
**A:**
- Team chat for quick questions
- GitHub Discussions for design questions
- Create issues for bugs

### Q: How do I run tests?
**A:**
```bash
# All tests
dotnet test

# Specific service
cd Services/admin-service
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### Q: What if Docker is slow on Windows?
**A:** Use WSL2! See [WSL2 Setup Guide](wsl2-setup-guide.md). It's 10x faster.

### Q: How do I add a new feature?
**A:**
1. Read the service README
2. Understand the domain model
3. Add domain logic first (TDD)
4. Add command/query handler
5. Add API endpoint
6. Write tests
7. Update documentation

---

## 📚 Learning Resources

### Domain-Driven Design
- [Domain-Driven Design Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly/) - Free ebook
- [DDD Reference](https://www.domainlanguage.com/ddd/reference/) - Quick reference

### CQRS & Messaging
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html) - Martin Fowler
- [Wolverine Documentation](https://wolverine.netlify.app/) - Messaging framework we use

### .NET
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [C# 12 Features](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)

### Docker
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

---

## 🎓 Next Steps

1. **Pick a small feature** from the backlog
2. **Pair program** with a senior developer
3. **Review PRs** from other developers
4. **Read ADRs** in `docs/adr/` to understand past decisions
5. **Explore other services** to see different patterns

---

## 🆘 Getting Help

### Stuck on Setup?
- Check [Troubleshooting](../README.md#troubleshooting) in README
- Ask in team chat
- Pair with another developer

### Code Questions?
- Check [Coding Standards](coding-standards.md)
- Look at existing code for patterns
- Ask in code review

### Architecture Questions?
- Read [CONTRIBUTING.md](../CONTRIBUTING.md)
- Check [ADRs](adr/) for past decisions
- Ask the architecture team

---

## ✅ Onboarding Checklist

Copy this to track your progress:

```markdown
### Setup
- [ ] Installed .NET 10 SDK
- [ ] Installed Docker Desktop
- [ ] Installed WSL2 (Windows only)
- [ ] Installed VS Code with extensions
- [ ] Cloned repository
- [ ] Ran `docker-compose up -d` successfully
- [ ] Accessed all Swagger UIs

### Reading
- [ ] Read README.md
- [ ] Read CONTRIBUTING.md
- [ ] Read Coding Standards
- [ ] Read one service README
- [ ] Skimmed Service Design Checklist

### Hands-On
- [ ] Explored admin-service codebase
- [ ] Ran tests successfully
- [ ] Made a small code change
- [ ] Created a PR
- [ ] Got code review feedback

### Understanding
- [ ] Understand monorepo structure
- [ ] Understand DDD layers
- [ ] Understand CQRS pattern
- [ ] Know where to find documentation
- [ ] Know how to ask for help
```

---

**Welcome to the team! 🚀**

If you have any questions or suggestions for improving this guide, please create a PR or discuss with the team.

---

**Last Updated**: January 2026  
**Maintained By**: GSS Development Team
