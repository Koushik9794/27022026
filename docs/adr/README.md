# Architecture Decision Records (ADRs)

This directory contains Architecture Decision Records for the GSS Backend project.

## What is an ADR?

An Architecture Decision Record (ADR) captures an important architectural decision made along with its context and consequences.

## Format

Each ADR follows this structure:
- **Status**: Proposed, Accepted, Deprecated, Superseded
- **Context**: The issue motivating this decision
- **Decision**: The change we're proposing or have agreed to
- **Consequences**: The resulting context after applying the decision

## Active ADRs

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](adr-001-microservices-architecture.md.md) | Microservices Architecture | Accepted | - |
| [ADR-002](adr-002-api-contract-governance-using-openapi.md) | API Contract Governance Using OpenAPI | Accepted | - |
| [ADR-003](adr-003-repo-structure-and-architecture-pattern.md) | Repository Structure and Architecture Pattern | Accepted | - |
| [ADR-004](adr-004-rest-api-design-standards.md) | REST API Design Standards | Accepted | Jan 2026 |

## Creating a New ADR

1. **Copy the template**:
   ```bash
   cp adr-template.md adr-NNN-descriptive-title.md
   ```

2. **Fill in the sections**:
   - Status (start with "Proposed")
   - Context (why is this decision needed?)
   - Decision (what are we doing?)
   - Consequences (what are the impacts?)

3. **Get review**:
   - Create PR
   - Discuss with architecture team
   - Update status to "Accepted" when approved

4. **Update this index**:
   - Add entry to the table above
   - Link to the new ADR

## ADR Lifecycle

```
Proposed → Accepted → [Deprecated/Superseded]
```

- **Proposed**: Under discussion
- **Accepted**: Approved and in use
- **Deprecated**: No longer recommended but not replaced
- **Superseded**: Replaced by a newer ADR

## Guidelines

### When to Create an ADR

Create an ADR for decisions that:
- ✅ Affect the overall architecture
- ✅ Are difficult or expensive to reverse
- ✅ Impact multiple services or teams
- ✅ Set technical standards or patterns
- ✅ Choose between significant alternatives

### When NOT to Create an ADR

Don't create ADRs for:
- ❌ Implementation details within a single service
- ❌ Temporary or experimental changes
- ❌ Obvious or trivial decisions
- ❌ Decisions that can be easily reversed

### Writing Tips

- **Be concise**: ADRs should be readable in 5-10 minutes
- **Be specific**: Include code examples where helpful
- **Explain why**: Context is more important than the decision itself
- **List alternatives**: Show what was considered and why it was rejected
- **Document consequences**: Both positive and negative

## Related Documentation

- [CONTRIBUTING.md](../../CONTRIBUTING.md) - Development guidelines
- [Coding Standards](../coding-standards.md) - Code style and patterns
- [Service Design Checklist](../service-design-checklist.md) - Production readiness

## References

- [ADR GitHub Organization](https://adr.github.io/)
- [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
