# Documentation Naming Conventions

## File Naming Standards

### Standard Documentation Files (UPPERCASE)

Use **UPPERCASE** for standard, well-known documentation files:

- ✅ `README.md` - Project overview and quick start
- ✅ `CONTRIBUTING.md` - Contribution guidelines
- ✅ `LICENSE` - License file
- ✅ `CHANGELOG.md` - Version history
- ✅ `CODE_OF_CONDUCT.md` - Code of conduct

**Rationale**: These are industry-standard names that developers expect to find. Using UPPERCASE makes them immediately recognizable.

### Specific Guides (lowercase-with-hyphens)

Use **lowercase with hyphens** for specific guides and documentation:

- ✅ `testing-guide.md` - Testing documentation
- ✅ `tester-guide.md` - Tester-specific documentation
- ✅ `coding-standards.md` - Coding standards
- ✅ `service-design-checklist.md` - Production readiness checklist
- ✅ `wsl2-setup-guide.md` - WSL2 setup instructions
- ✅ `api-reference.md` - API documentation
- ✅ `deployment-guide.md` - Deployment instructions

**Rationale**: Lowercase with hyphens is:
- More readable in URLs
- Standard for web content
- Easier to type
- Consistent with modern documentation practices

### Architecture Decision Records (ADRs)

Use numbered format with lowercase:

- ✅ `adr-001-use-postgresql.md`
- ✅ `adr-002-adopt-ddd-architecture.md`
- ✅ `adr-003-cqrs-with-mediatr.md`

### Service-Specific Documentation

Service READMEs should always be UPPERCASE:

- ✅ `Services/admin-service/README.md`
- ✅ `Services/catalog-service/README.md`

## Directory Structure

```
gss-backend/
├── README.md                           # UPPERCASE - Main project README
├── CONTRIBUTING.md                     # UPPERCASE - Standard file
├── LICENSE                             # UPPERCASE - Standard file
├── docs/
│   ├── testing-guide.md               # lowercase-with-hyphens
│   ├── tester-guide.md                # lowercase-with-hyphens
│   ├── coding-standards.md            # lowercase-with-hyphens
│   ├── service-design-checklist.md    # lowercase-with-hyphens
│   ├── wsl2-setup-guide.md            # lowercase-with-hyphens
│   ├── adr/
│   │   ├── adr-001-use-postgresql.md  # Numbered ADRs
│   │   └── adr-002-adopt-ddd.md
│   └── API_Workflows/                  # Existing structure
└── Services/
    ├── admin-service/
    │   └── README.md                   # UPPERCASE - Service README
    └── catalog-service/
        └── README.md                   # UPPERCASE - Service README
```

## Quick Reference

| Type | Format | Example |
|------|--------|---------|
| Standard docs | UPPERCASE.md | `README.md`, `CONTRIBUTING.md` |
| Specific guides | lowercase-with-hyphens.md | `testing-guide.md` |
| ADRs | adr-NNN-description.md | `adr-001-use-postgresql.md` |
| Service README | UPPERCASE.md | `Services/*/README.md` |

## Naming Guidelines

### Do's ✅

- Use descriptive names: `wsl2-setup-guide.md` not `wsl.md`
- Be consistent within categories
- Use hyphens, not underscores: `coding-standards.md` not `coding_standards.md`
- Keep names concise but clear
- Use singular for guides: `testing-guide.md` not `testing-guides.md`

### Don'ts ❌

- Don't mix cases: ~~`Testing_Guide.md`~~
- Don't use spaces: ~~`Testing Guide.md`~~
- Don't use abbreviations: ~~`tst-gd.md`~~
- Don't use camelCase: ~~`testingGuide.md`~~
- Don't use PascalCase for guides: ~~`TestingGuide.md`~~

## Rationale

This convention follows industry best practices:

1. **GitHub/GitLab Standard**: UPPERCASE for standard files (README, CONTRIBUTING)
2. **Web Standards**: lowercase-with-hyphens for URLs and file paths
3. **Readability**: Hyphens are more readable than underscores or camelCase
4. **Consistency**: Clear rules reduce decision fatigue
5. **Tooling**: Many static site generators expect lowercase-with-hyphens

## Migration Checklist

When renaming documentation:

- [ ] Rename the file
- [ ] Update all references in other documentation
- [ ] Update links in README.md
- [ ] Update links in CONTRIBUTING.md
- [ ] Update links in service READMEs
- [ ] Update any navigation/index files
- [ ] Test all links work correctly

## Examples from Other Projects

**Good Examples**:
- [Kubernetes](https://github.com/kubernetes/kubernetes): `README.md`, `CONTRIBUTING.md`, `docs/user-guide.md`
- [React](https://github.com/facebook/react): `README.md`, `CONTRIBUTING.md`, `docs/installation.md`
- [.NET](https://github.com/dotnet/runtime): `README.md`, `CONTRIBUTING.md`, `docs/coding-guidelines.md`

## Enforcement

Add to code review checklist:
- [ ] New documentation follows naming conventions
- [ ] Links use correct case
- [ ] No mixed naming styles

---

**Last Updated**: January 2026  
**Maintained By**: GSS Development Team
