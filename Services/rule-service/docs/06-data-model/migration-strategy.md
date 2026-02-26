# Migration Strategy

## Overview

Database migrations for the Rule Service follow a controlled, versioned approach to ensure zero-downtime deployments and safe schema evolution.

## Migration Tooling

- **FluentMigrator** - .NET migration framework
- **Version Control** - All migrations tracked in source control
- **Idempotent** - Migrations can be re-run safely

## Migration Principles

1. **Forward Only** - No rollback migrations; fix forward
2. **Backward Compatible** - New schema works with old code
3. **Small Batches** - Frequent, small migrations over large changes
4. **Tested** - All migrations tested in non-production first

## Schema Change Patterns

### Adding Columns
```csharp
// Safe: Add nullable column
Create.Column("new_field").OnTable("rulesets").AsString(255).Nullable();

// Later: Backfill data, then add constraint if needed
```

### Removing Columns
```csharp
// Step 1: Stop using column in code (deploy)
// Step 2: Wait for all instances updated
// Step 3: Drop column
Delete.Column("old_field").FromTable("rulesets");
```

### Renaming Tables/Columns
```csharp
// Step 1: Create new column
// Step 2: Dual-write to both columns
// Step 3: Backfill old data
// Step 4: Switch reads to new column
// Step 5: Stop writes to old column
// Step 6: Drop old column
```

## Data Migration

When migrating rule data:

1. **Export** current active rulesets
2. **Transform** data if schema changed
3. **Validate** transformed data
4. **Import** into new schema
5. **Verify** evaluations produce same results

## Rollback Procedure

If migration causes issues:

1. Deploy previous application version
2. Schema remains forward-compatible
3. Fix migration issue
4. Deploy corrected migration
5. Re-deploy latest application

> [!IMPORTANT]
> Never run destructive migrations (DROP, DELETE) until the previous application version is no longer in use.
