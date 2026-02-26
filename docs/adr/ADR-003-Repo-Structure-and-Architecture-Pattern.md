# ADR-001: Microservice Architecture Pattern - Shared Libraries vs Clean DDD

**Status:** Proposed  
**Date:** 2026-01-07  
**Decision Makers:** Architecture Team  
**Stakeholders:** Backend Development Team, DevOps, Platform Team

---

## Context

The GSS Warehouse Configurator backend consists of multiple microservices. Currently, we have two distinct architectural patterns in use:

1. **GSS-Catalog-Services Pattern**: Uses shared libraries (`GSSDesingConfigurator.*`) with custom abstractions
2. **Admin-Service/Rule-Service Pattern**: Clean DDD with standard NuGet packages and self-contained structure

We need to decide which pattern to standardize on for scalability, maintainability, and developer experience.

---

## Decision Drivers

- **Scalability**: Ability to scale development team and service instances
- **Maintainability**: Ease of understanding, modifying, and debugging code
- **Developer Experience**: Onboarding time, local development setup, debugging ease
- **Deployment Independence**: Microservices should be independently deployable
- **Technology Evolution**: Ability to upgrade dependencies and adopt new patterns
- **Team Autonomy**: Teams should own their services end-to-end

---

## Architectural Comparison

### Pattern 1: GSS-Catalog-Services (Shared Library Approach)

#### Structure
```
GSS-Catalog-Services/
├── src/GSS-Catalog-Services/
│   ├── Domain/
│   ├── Application/
│   ├── infrastructure/
│   ├── Web.Api/
│   └── bootstrap/
└── Shared Libraries (External):
    ├── GSSDesingConfigurator.Contract
    ├── GSSDesingConfigurator.Database
    ├── GSSDesingConfigurator.Field.Validation
    └── GSSDesingConfigurator.Logging
```

#### Key Characteristics

**Dependencies:**
- Custom shared libraries for cross-cutting concerns
- Custom abstractions (`ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventsDispatcher`)
- Custom database executor (`AddDapperExecutor()`)
- Custom logging (`AddCommonLogging()`)
- Custom validation decorators (`AddCommandValidationDecoratorOnly()`)

**Dependency Injection:**
```csharp
builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure()
    .AddDatabase()
    .AddRateLimit()
    .AddErrorResponse()
    .AddLogging(builder.Configuration)
    .AddCommandValidation();
```

**Features:**
- Built-in rate limiting (300 requests/minute with sliding window)
- Custom domain event dispatcher
- Custom error response factory
- Shared validation framework

**Pros:**
- ✅ Code reuse across services
- ✅ Consistent patterns enforced by shared libraries
- ✅ Advanced features (rate limiting, event dispatching) out of the box
- ✅ Centralized logging and validation

**Cons:**
- ❌ **Tight coupling** between services via shared libraries
- ❌ **Deployment dependency**: Updating shared library requires rebuilding all services
- ❌ **Versioning complexity**: Managing shared library versions across services
- ❌ **Onboarding friction**: Developers must understand custom abstractions
- ❌ **Debugging difficulty**: Stepping through shared library code requires source
- ❌ **Limited autonomy**: Teams can't evolve services independently
- ❌ **Hidden complexity**: Custom abstractions hide standard patterns
- ❌ **Testing overhead**: Mocking custom interfaces increases test complexity

---

### Pattern 2: Admin-Service/Rule-Service (Clean DDD Approach)

#### Structure
```
admin-service/
├── AdminService.csproj (single project)
├── Program.cs
├── src/
│   ├── api/
│   ├── application/
│   ├── bootstrap/
│   ├── domain/
│   └── infrastructure/
└── tests/
```

#### Key Characteristics

**Dependencies:**
- Standard NuGet packages only
- MediatR for CQRS
- FluentValidation for validation
- FluentMigrator for database migrations
- Dapper for data access
- Swashbuckle for OpenAPI

**Dependency Injection:**
```csharp
// Explicit, transparent DI registration
builder.Services.AddScoped<IDbConnectionFactory>(_ => 
    new PostgreSqlConnectionFactory(connectionString));

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
});

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IUserRepository, DapperUserRepository>();
```

**Features:**
- FluentMigrator runs on startup
- MediatR pipeline behaviors for cross-cutting concerns
- Health check endpoints
- Comprehensive Swagger documentation
- Simple, explicit configuration

**Pros:**
- ✅ **Zero coupling**: No shared libraries, fully independent
- ✅ **Standard patterns**: Uses well-known NuGet packages (MediatR, FluentValidation)
- ✅ **Easy onboarding**: Developers familiar with .NET ecosystem can contribute immediately
- ✅ **Simple debugging**: All code in one project, easy to step through
- ✅ **Independent deployment**: Update dependencies without affecting other services
- ✅ **Team autonomy**: Each team owns their service completely
- ✅ **VS Code friendly**: Single project, easy to run and debug
- ✅ **Transparent**: No hidden abstractions, clear data flow
- ✅ **Testable**: Standard mocking with Moq/NSubstitute
- ✅ **Technology evolution**: Easy to upgrade packages independently

**Cons:**
- ❌ Code duplication across services (e.g., validation behaviors)
- ❌ No enforced consistency (teams could diverge)
- ❌ Need to implement cross-cutting concerns per service

---

## Scalability Analysis

### Shared Library Approach (GSS-Catalog-Services)

| Dimension | Assessment | Reasoning |
|-----------|------------|-----------|
| **Development Team Scalability** | ⚠️ **Limited** | New developers must learn custom abstractions. Shared library changes require coordination across teams. |
| **Service Instance Scalability** | ✅ **Good** | Services can scale horizontally. Rate limiting built-in. |
| **Codebase Scalability** | ❌ **Poor** | Shared library becomes a monolith. Changes ripple across services. |
| **Deployment Scalability** | ❌ **Poor** | Shared library updates force redeployment of all services. |
| **Technology Scalability** | ❌ **Poor** | Upgrading shared libraries is risky and requires testing all services. |

### Clean DDD Approach (Admin-Service/Rule-Service)

| Dimension | Assessment | Reasoning |
|-----------|------------|-----------|
| **Development Team Scalability** | ✅ **Excellent** | Standard patterns, easy onboarding. Teams work independently. |
| **Service Instance Scalability** | ✅ **Excellent** | Services scale independently. No shared state. |
| **Codebase Scalability** | ✅ **Good** | Each service evolves independently. Clear boundaries. |
| **Deployment Scalability** | ✅ **Excellent** | Truly independent deployments. No coordination needed. |
| **Technology Scalability** | ✅ **Excellent** | Each service can adopt new tech at its own pace. |

---

## Robustness Analysis

### Shared Library Approach

**Strengths:**
- Consistent error handling across services
- Centralized logging and monitoring
- Built-in rate limiting prevents abuse

**Weaknesses:**
- **Single point of failure**: Bug in shared library affects all services
- **Version conflicts**: Different services may need different library versions
- **Breaking changes**: Shared library updates can break multiple services
- **Hidden dependencies**: Difficult to track what each service actually needs

### Clean DDD Approach

**Strengths:**
- **Fault isolation**: Bug in one service doesn't affect others
- **Explicit dependencies**: Clear what each service needs
- **Gradual rollout**: Can test changes in one service before others
- **Rollback safety**: Easy to rollback individual services

**Weaknesses:**
- Inconsistency risk if teams don't follow patterns
- Duplicated code for common concerns
- Need to implement monitoring/logging per service

---

## Developer Experience Comparison

### Local Development Setup

**GSS-Catalog-Services:**
```bash
# Requires:
1. Clone main repo
2. Restore shared libraries (may need internal NuGet feed)
3. Build shared libraries first
4. Build service
5. Configure connection strings
6. Run service

# Debugging:
- Need source code for shared libraries
- Stepping through custom abstractions is confusing
- Hard to understand data flow
```

**Admin-Service:**
```bash
# Requires:
1. Clone service directory
2. dotnet restore (all from nuget.org)
3. dotnet run

# Debugging in VS Code:
- Press F5
- Set breakpoints anywhere
- Clear execution flow
- All code in one place
```

### Onboarding Time

| Task | Shared Library | Clean DDD |
|------|----------------|-----------|
| Understand architecture | 2-3 days | 4-6 hours |
| Make first code change | 1-2 days | 2-4 hours |
| Debug production issue | Difficult | Easy |
| Add new feature | Medium | Easy |

---

## Real-World Scenarios

### Scenario 1: Upgrading to .NET 9

**Shared Library Approach:**
1. Upgrade shared libraries to .NET 9
2. Test all services with new libraries
3. Fix breaking changes in all services
4. Coordinate deployment of all services
5. **Estimated time**: 2-3 weeks

**Clean DDD Approach:**
1. Upgrade one service to .NET 9
2. Test that service
3. Deploy that service
4. Repeat for other services at team's pace
5. **Estimated time**: 1-2 days per service (can be parallel)

### Scenario 2: Adding New Validation Rule

**Shared Library Approach:**
1. Update validation library
2. Version bump
3. Update all services to new version
4. Test all services
5. Deploy all services
6. **Risk**: High (affects all services)

**Clean DDD Approach:**
1. Add validation to one service
2. Test that service
3. Deploy that service
4. Other services unaffected
5. **Risk**: Low (isolated change)

### Scenario 3: New Developer Joins Team

**Shared Library Approach:**
- Read shared library documentation
- Understand custom abstractions
- Learn internal NuGet feed setup
- Understand dependency graph
- **Time to first PR**: 1-2 weeks

**Clean DDD Approach:**
- Read service README
- Run `dotnet run`
- Follow standard .NET patterns
- **Time to first PR**: 1-2 days

---

## Industry Best Practices

### Microservices Principles (Martin Fowler, Sam Newman)

> "The whole point of microservices is to be able to make changes to one service without affecting others."

**Shared libraries violate this principle.**

### The Twelve-Factor App

**Factor III - Config**: Store config in the environment
- ✅ Clean DDD: Uses appsettings.json + environment variables
- ⚠️ Shared Library: Config spread across shared libraries

**Factor V - Build, Release, Run**: Strictly separate build and run stages
- ✅ Clean DDD: Single build artifact per service
- ❌ Shared Library: Multiple artifacts (service + libraries)

**Factor IX - Disposability**: Maximize robustness with fast startup
- ✅ Clean DDD: Fast startup, minimal dependencies
- ⚠️ Shared Library: Slower startup, more dependencies

---

## Decision

**We recommend standardizing on the Clean DDD Single-Project Pattern (admin-service/rule-service approach).**

### Rationale

1. **Scalability**: Clean DDD scales better for teams, deployments, and technology evolution
2. **Robustness**: Fault isolation and independent deployments reduce risk
3. **Developer Experience**: Faster onboarding, easier debugging, better VS Code support
4. **Industry Alignment**: Follows microservices best practices
5. **Long-term Maintainability**: Easier to evolve services independently

### Migration Strategy

1. **New services**: Use Clean DDD pattern exclusively
2. **Existing services**: Refactor to Clean DDD pattern incrementally
3. **Shared libraries**: Deprecate over time, replace with standard packages

---

## Consequences

### Positive

- ✅ Faster development cycles
- ✅ Easier onboarding for new developers
- ✅ Better VS Code debugging experience
- ✅ Independent service evolution
- ✅ Reduced deployment risk
- ✅ Clearer service boundaries
- ✅ Standard .NET ecosystem alignment

### Negative

- ❌ Some code duplication (validation behaviors, logging setup)
- ❌ Need to establish coding standards to prevent divergence
- ❌ Initial refactoring effort for existing services

### Mitigation

- Create service templates with common patterns
- Document best practices in ADRs
- Code reviews to ensure consistency
- Share knowledge through tech talks

---

## Implementation Plan

### Phase 1: Standardize New Services (Immediate)
- All new services use Clean DDD pattern
- Create service template repository
- Document pattern in architecture guide

### Phase 2: Refactor Catalog Service (Q1 2026)
- Refactor GSS-Catalog-Services to `catalog-service`
- Remove shared library dependencies
- Add VS Code debugging configuration
- Migrate to FluentMigrator

### Phase 3: Evaluate Shared Libraries (Q2 2026)
- Audit usage of `GSSDesingConfigurator.*` libraries
- Plan deprecation timeline
- Provide migration guides for teams

### Phase 4: Complete Migration (Q3 2026)
- All services follow Clean DDD pattern
- Shared libraries deprecated
- Update CI/CD pipelines

---

## Alternatives Considered

### Alternative 1: Keep Both Patterns

**Rejected because:**
- Increases cognitive load for developers
- Inconsistent developer experience
- Harder to maintain two patterns
- Confusing for new team members

### Alternative 2: Enhance Shared Libraries

**Rejected because:**
- Doesn't solve fundamental coupling issues
- Still requires coordination for updates
- Doesn't improve deployment independence
- Increases complexity over time

### Alternative 3: Multi-Project DDD (Separate Domain/Application/Infrastructure Projects)

**Rejected because:**
- Overkill for microservices (already have service boundaries)
- Slower builds and debugging
- More complex project structure
- Harder to navigate in VS Code
- Admin-service and rule-service prove single-project works well

---

## References

- [Microservices Patterns](https://microservices.io/patterns/microservices.html) - Chris Richardson
- [Building Microservices](https://www.oreilly.com/library/view/building-microservices-2nd/9781492034018/) - Sam Newman
- [The Twelve-Factor App](https://12factor.net/)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/) - Eric Evans
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin

---

## Appendix: Detailed Comparison Matrix

| Aspect | Shared Library | Clean DDD | Winner |
|--------|----------------|-----------|--------|
| **Deployment Independence** | ❌ Coupled | ✅ Independent | Clean DDD |
| **Onboarding Time** | 2-3 days | 4-6 hours | Clean DDD |
| **Debugging Ease** | Hard | Easy | Clean DDD |
| **VS Code Support** | Poor | Excellent | Clean DDD |
| **Technology Upgrades** | Risky | Safe | Clean DDD |
| **Code Reuse** | ✅ High | ❌ Low | Shared Library |
| **Consistency** | ✅ Enforced | ⚠️ Optional | Shared Library |
| **Team Autonomy** | ❌ Low | ✅ High | Clean DDD |
| **Build Time** | Slower | Faster | Clean DDD |
| **Testing Complexity** | High | Low | Clean DDD |
| **Fault Isolation** | ❌ Poor | ✅ Excellent | Clean DDD |
| **Rollback Safety** | ❌ Risky | ✅ Safe | Clean DDD |

**Overall Winner: Clean DDD (10 vs 2)**

---

## Approval

- [ ] Architecture Team Lead
- [ ] Backend Team Lead
- [ ] DevOps Lead
- [ ] CTO

**Next Steps:**
1. Review and approve this ADR
2. Create catalog-service refactoring task
3. Update architecture documentation
4. Communicate decision to all teams
