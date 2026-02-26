# ADR-004: MediatR Alternative Evaluation for Long-Term Reliability

**Status:** Proposed  
**Date:** 2026-01-07  
**Decision Makers:** Architecture Team, Backend Team Lead  
**Stakeholders:** Backend Development Team, DevOps, CTO

---

## Context

The GSS Warehouse Configurator backend currently uses **MediatR v11.1.0** in two services (`admin-service` and `rule-service`) for implementing the CQRS pattern. We have 6 additional services planned for development.

### Critical Issue: MediatR Licensing Change

**MediatR has transitioned from open-source (Apache 2.0) to a commercial licensing model** under Lucky Penny Software:

- **Previous**: Completely free and open-source
- **Current**: Requires commercial license for most use cases
- **Enforcement**: Warning/error log messages (no runtime restrictions)

### Licensing Tiers

| Tier | Cost (INR/year) | Eligibility | Developers |
|------|----------------|-------------|------------|
| **Community** | FREE* | Revenue < $5M USD, No VC funding > $10M | Unlimited |
| **Standard** | ₹42,500 - 68,000 | Commercial use | 1-10 |
| **Professional** | ₹127,500 - 204,000 | Commercial use | 11-50 |
| **Enterprise** | ₹340,000 - 544,000 | Commercial use | Unlimited |

*Government agencies do NOT qualify for Community license.

### Risk Assessment

1. **Licensing Uncertainty**: Future versions may have stricter enforcement
2. **Cost Escalation**: As team grows, licensing costs increase
3. **Vendor Lock-in**: Dependency on commercial vendor for critical infrastructure
4. **Migration Complexity**: 2 services already using MediatR, 6 more planned
5. **Long-term Viability**: Uncertainty about Lucky Penny Software's roadmap

---

## Decision Drivers

- **Long-term Reliability**: Need stable, maintained solution for 5+ years
- **Licensing Clarity**: No ambiguity or future licensing risks
- **Cost Predictability**: Avoid escalating costs as team/usage grows
- **Community Support**: Active community for troubleshooting and updates
- **Migration Effort**: Minimize disruption to existing services
- **Feature Parity**: Must support CQRS, validation pipeline, and behaviors
- **Performance**: Sub-millisecond overhead for request handling
- **Ecosystem Fit**: Works well with .NET 8/10, Dapper, FluentValidation

---

## Alternatives Evaluated

### Option 1: Continue with MediatR (Commercial License)

#### Description
Purchase commercial license and continue using MediatR.

#### Pros
- ✅ No migration effort for existing services
- ✅ Well-documented and familiar to team
- ✅ Mature ecosystem with extensive examples
- ✅ FluentValidation integration well-established

#### Cons
- ❌ **Recurring annual cost**: ₹42,500+ per year (minimum)
- ❌ **Cost escalation**: Increases with team size
- ❌ **Vendor dependency**: Reliant on Lucky Penny Software
- ❌ **Licensing uncertainty**: Future changes unpredictable
- ❌ **Community fragmentation**: Many developers moving to alternatives

#### Cost Analysis (5-year projection)
- **Year 1-2**: ₹42,500/year (Standard, 1-10 devs)
- **Year 3-5**: ₹127,500/year (Professional, 11-50 devs)
- **Total 5-year cost**: ₹467,500 (~$5,600 USD)

---

### Option 2: Wolverine (MIT License)

#### Description
Modern, high-performance mediator and message bus framework by JasperFx (creator of Marten, Lamar).

**Repository**: https://github.com/JasperFx/wolverine  
**License**: MIT (perpetually free)  
**Maintainer**: Jeremy Miller / JasperFx Software  
**Stars**: 11.8k+ | **Contributors**: 138+

#### Key Features
- In-process mediator (MediatR-like)
- Asynchronous messaging (RabbitMQ, Azure Service Bus, SQS)
- Built-in transactional outbox pattern
- Source generators for performance
- Native AOT support
- Middleware/behaviors support
- OpenTelemetry integration

#### Pros
- ✅ **MIT License**: Perpetually free, no licensing concerns
- ✅ **Active Development**: Maintained by respected .NET architect
- ✅ **Performance**: Faster than MediatR (source generators)
- ✅ **Future-proof**: Supports .NET 8+, Native AOT
- ✅ **More Features**: Messaging, outbox, saga support
- ✅ **Commercial Support Available**: Optional paid support from JasperFx

#### Cons
- ❌ **Migration Effort**: Different API from MediatR
- ❌ **Learning Curve**: More features = more complexity
- ❌ **Smaller Ecosystem**: Fewer community examples than MediatR
- ❌ **Opinionated**: Stronger conventions than MediatR

#### Migration Complexity
- **Moderate**: Different interface signatures
- **Estimated Effort**: 2-3 days per service
- **Risk**: Medium (new patterns to learn)

---

### Option 3: Brighter (MIT License)

#### Description
Command dispatcher and message bus framework focused on clean architecture and CQRS.

**Repository**: https://github.com/BrighterCommand/Brighter  
**License**: MIT (perpetually free)  
**Maintainer**: BrighterCommand Community  
**Stars**: 2k+ | **Contributors**: 74+

#### Key Features
- Command/Query dispatcher
- Ports & Adapters (Hexagonal Architecture)
- Messaging support (RabbitMQ, Kafka, SQS)
- Retry and circuit breaker policies
- Outbox pattern support
- Well-documented

#### Pros
- ✅ **MIT License**: Perpetually free
- ✅ **Clean Architecture Focus**: Aligns with DDD principles
- ✅ **Mature**: 10+ years of development
- ✅ **Messaging Built-in**: No need for separate library
- ✅ **Resilience Patterns**: Retry, circuit breaker out-of-box

#### Cons
- ❌ **Different Paradigm**: Command/Query dispatcher vs mediator
- ❌ **Smaller Community**: Less popular than MediatR/Wolverine
- ❌ **Heavier**: More opinionated than simple mediator
- ❌ **Migration Effort**: Significant API differences

#### Migration Complexity
- **High**: Different architectural approach
- **Estimated Effort**: 3-5 days per service
- **Risk**: Medium-High (paradigm shift)

---

### Option 4: Concordia (MIT License)

#### Description
Lightweight, high-performance mediator using C# Source Generators for compile-time handler registration.

**Repository**: https://github.com/tnc1997/dotnet-concordia  
**License**: MIT (perpetually free)  
**Features**: MediatR-compatible interfaces, zero runtime reflection

#### Pros
- ✅ **MIT License**: Perpetually free
- ✅ **MediatR-Compatible**: Identical interface signatures
- ✅ **High Performance**: Source generators eliminate reflection
- ✅ **Easy Migration**: Drop-in replacement for MediatR
- ✅ **Modern**: Built for .NET 8+

#### Cons
- ❌ **New Project**: Less mature (launched 2024)
- ❌ **Small Community**: Limited examples and support
- ❌ **Unknown Longevity**: Unclear long-term maintenance commitment
- ❌ **Limited Features**: Basic mediator only (no messaging)

#### Migration Complexity
- **Low**: MediatR-compatible interfaces
- **Estimated Effort**: 1-2 days per service
- **Risk**: Low (similar API)

---

### Option 5: Custom Mediator Implementation

#### Description
Implement a lightweight custom mediator pattern using .NET DI.

#### Implementation Approach
```csharp
// Simple mediator interface
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
}

// Handler interface
public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

// Implementation (50-100 lines of code)
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        // Invoke handler...
    }
}
```

#### Pros
- ✅ **Zero Licensing**: No external dependencies
- ✅ **Full Control**: Customize to exact needs
- ✅ **Minimal Code**: ~100 lines for basic implementation
- ✅ **No Migration**: Keep MediatR interfaces
- ✅ **Educational**: Team learns pattern deeply

#### Cons
- ❌ **Maintenance Burden**: Team owns all bugs/features
- ❌ **No Advanced Features**: Must implement behaviors, validation manually
- ❌ **Testing Overhead**: Must write comprehensive tests
- ❌ **Reinventing Wheel**: Well-solved problem

#### Migration Complexity
- **Low**: Can keep MediatR interfaces
- **Estimated Effort**: 3-5 days initial implementation + testing
- **Risk**: Medium (ongoing maintenance)

---

## Comparison Matrix

| Criteria | MediatR (Paid) | Wolverine | Brighter | Concordia | Custom |
|----------|---------------|-----------|----------|-----------|--------|
| **License** | Commercial | MIT | MIT | MIT | N/A |
| **5-Year Cost** | ₹467,500 | ₹0 | ₹0 | ₹0 | ₹0 |
| **Maturity** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ | ⭐ |
| **Community** | Large | Growing | Medium | Small | N/A |
| **Performance** | Good | Excellent | Good | Excellent | Good |
| **Migration Effort** | None | Moderate | High | Low | Low |
| **Feature Set** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **Long-term Risk** | Medium | Low | Low | Medium | Medium |
| **Ecosystem Fit** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |

---

## Decision

**We recommend migrating to Wolverine (Option 2).**

### Rationale

1. **Licensing Certainty**: MIT license eliminates all licensing concerns forever
2. **Long-term Reliability**: Maintained by respected .NET architect (Jeremy Miller) with 20+ years in ecosystem
3. **Future-Proof**: Modern architecture, source generators, Native AOT support
4. **Cost Savings**: ₹467,500 saved over 5 years
5. **Feature Rich**: Exceeds MediatR capabilities (messaging, outbox, sagas)
6. **Performance**: Faster than MediatR due to source generators
7. **Commercial Support Available**: Optional paid support if needed (no vendor lock-in)
8. **Growing Adoption**: Community moving from MediatR to Wolverine

### Why Not Other Options?

- **MediatR**: Licensing risk and recurring costs unacceptable for long-term
- **Brighter**: Higher migration effort, different paradigm
- **Concordia**: Too new, uncertain longevity
- **Custom**: Maintenance burden outweighs benefits

---

## Implementation Strategy

### Phase 1: Proof of Concept (Week 1)
1. Create spike branch with Wolverine in one service (e.g., `admin-service`)
2. Migrate 2-3 handlers to validate approach
3. Test performance and developer experience
4. Document migration patterns

### Phase 2: Migrate Existing Services (Weeks 2-3)
1. Migrate `admin-service` (estimated: 2-3 days)
2. Migrate `rule-service` (estimated: 2-3 days)
3. Update integration tests
4. Deploy to staging for validation

### Phase 3: New Services (Ongoing)
1. All new services use Wolverine from day one
2. Create service template with Wolverine pre-configured
3. Update ADR-003 to reflect Wolverine as standard

### Migration Checklist Per Service

- [ ] Add Wolverine NuGet package
- [ ] Replace `IRequest<T>` with Wolverine equivalents
- [ ] Update handler signatures
- [ ] Migrate validation pipeline behaviors
- [ ] Update DI registration
- [ ] Update tests
- [ ] Verify all endpoints work
- [ ] Remove MediatR packages

---

## Consequences

### Positive

- ✅ **Zero licensing costs** for lifetime of project
- ✅ **No vendor lock-in** or commercial dependencies
- ✅ **Better performance** than current MediatR implementation
- ✅ **Future-ready** with modern .NET features
- ✅ **More capabilities** (messaging, outbox) for future needs
- ✅ **Community alignment** with industry trend away from commercial MediatR

### Negative

- ❌ **Migration effort**: 4-6 days for 2 existing services
- ❌ **Learning curve**: Team needs to learn Wolverine conventions
- ❌ **Smaller ecosystem**: Fewer Stack Overflow answers than MediatR
- ❌ **Documentation gap**: Less comprehensive than MediatR docs

### Mitigation

- Allocate dedicated time for migration (not rushed)
- Create internal documentation and examples
- Leverage JasperFx Discord for community support
- Consider optional commercial support contract if needed

---

## Alternatives Considered and Rejected

### Keep MediatR v11.1.0 (No Upgrade)
**Rejected because:**
- Still subject to licensing enforcement via log warnings
- Missing security updates and bug fixes
- Technical debt accumulates
- Not a long-term solution

### Use MediatR Community License
**Rejected because:**
- Eligibility unclear (revenue thresholds, VC funding)
- Risk of disqualification as company grows
- Requires ongoing compliance monitoring
- Not suitable for enterprise/government use

---

## References

- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Wolverine GitHub](https://github.com/JasperFx/wolverine)
- [MediatR Licensing Announcement](https://mediatr.io)
- [Jeremy Miller's Blog - Wolverine Licensing Commitment](https://jeremydmiller.com/2025/01/06/the-critter-stack-is-staying-open-source/)
- [.NET Community Discussion on MediatR Licensing](https://www.reddit.com/r/dotnet/comments/mediatr_licensing)

---

## Approval

- [ ] Architecture Team Lead
- [ ] Backend Team Lead
- [ ] CTO
- [ ] DevOps Lead

**Next Steps:**
1. Review and approve this ADR
2. Create migration spike for `admin-service`
3. Schedule team knowledge-sharing session on Wolverine
4. Update project templates and documentation
