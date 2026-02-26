# Data Access & Migrations Guide

## Overview

The rule-service uses a three-tier data access architecture:

1. **FluentMigrator** - Schema versioning & migrations (like Alembic)
2. **Dapper** - Lightweight ORM for queries
3. **Domain Model** - Business logic in domain layer

## Migrations

Migrations are stored in `src/infrastructure/migrations/` and follow FluentMigrator conventions.

### Creating a Migration

1. **Add new migration class:**

```csharp
[Migration(20241218002)]  // Timestamp-based version
public class AddRulesPriorityIndex : Migration
{
    public override void Up()
    {
        Create.Index("idx_rules_priority")
            .OnTable("rules")
            .OnColumn("priority").Ascending();
    }

    public override void Down()
    {
        Drop.Index("idx_rules_priority").OnTable("rules");
    }
}
```

2. **Build and run:**

```bash
cd Services/rule-service
dotnet build
dotnet run  # Runs pending migrations on startup
```

### Migration Naming Convention

- **File**: `M{TIMESTAMP}_{DescriptiveName}.cs`
- **Migration ID**: Integer timestamp (e.g., `20241218001`)
- **Timestamp**: YYYYMMDDhhmmss format

## Dapper Repositories

Repositories use Dapper for type-safe SQL mapping.

### Repository Pattern

```csharp
public class DapperRuleRepository : IRuleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public async Task<RuleSet> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, name, ... FROM rule_sets WHERE id = @Id";
        
        var dto = await connection.QueryFirstOrDefaultAsync<RuleSetDto>(sql, new { Id = id });
        return MapToRuleSet(dto);  // Map DTO to domain model
    }
}
```

### DTOs (Data Transfer Objects)

- **Purpose**: Map database columns to C# properties
- **Location**: Inside repository class
- **Naming**: `{Entity}Dto`

```csharp
private class RuleSetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    // ... other columns
}
```

### Dapper Tips

- Column names use snake_case in database, properties use PascalCase in C#
- Dapper automatically handles the mapping
- Use `QueryAsync<T>` for multiple results
- Use `QueryFirstOrDefaultAsync<T>` for single result
- Use `ExecuteAsync` for insert/update/delete

## Database Schema

Current schema (from initial migration):

### Tables

**rule_sets**
- id (GUID, PK)
- name (string)
- product_group_id (GUID)
- country_id (GUID)
- effective_from (DateTime)
- effective_to (DateTime, nullable)
- status (string) - DRAFT, ACTIVE, INACTIVE, ARCHIVED
- created_at (DateTime)
- updated_at (DateTime)

**rules**
- id (GUID, PK)
- name (string)
- description (string)
- category (string) - SPATIAL, STRUCTURAL, ACCESSORY, PRICING, COMPLIANCE
- priority (int)
- severity (string) - ERROR, WARNING, INFO
- enabled (bool)
- created_at (DateTime)
- updated_at (DateTime)

**rule_conditions**
- id (GUID, PK)
- rule_id (GUID, FK)
- type (string) - AND, OR, NOT
- field (string)
- operator (string) - EQ, NE, LT, GT, CONTAINS
- value (string, JSON)

**ruleset_rules** (Many-to-many)
- ruleset_id (GUID, FK)
- rule_id (GUID, FK)
- added_at (DateTime)

**rule_versions**
- id (GUID, PK)
- rule_id (GUID, FK)
- version_number (int)
- change_log (string)
- rule_definition (string)
- created_by (string)
- created_at (DateTime)

**audit_logs**
- id (GUID, PK)
- entity_id (GUID)
- entity_type (string)
- action (string)
- changes (string, JSON)
- created_by (string)
- created_at (DateTime)

## Running Migrations

### Automatic (on app startup)
```bash
cd Services/rule-service
dotnet run
# Migrations run automatically from Program.cs
```

### Manual
```bash
# Compile migrations
dotnet build

# Check pending migrations
dotnet exec FluentMigrator.dll --help

# Run specific migration
dotnet exec FluentMigrator.dll --version=20241218001
```

## Best Practices

1. **Migration Reversibility**
   - Always implement `Down()` method
   - Test rollbacks locally
   - Avoid irreversible operations (e.g., DROP without backup)

2. **Naming**
   - Use clear, descriptive names
   - Include action: `Add`, `Remove`, `Rename`, `Create`
   - Example: `M20241218002_AddRuleVersionTracking.cs`

3. **Performance**
   - Add indexes for frequently queried columns
   - Use batch operations in Dapper
   - Consider materialized views for complex queries

4. **Domain Model Alignment**
   - Keep migrations in sync with domain entities
   - Use migrations as the source of truth for schema
   - Never modify schema directly in database

5. **Testing**
   - Test migrations up and down
   - Use test database for validation
   - Include data seeding migrations for tests

## Rollback Strategy

If a migration causes issues:

1. **Create a new "fix" migration** (don't modify existing):
   ```csharp
   [Migration(20241218003)]
   public class FixRuleSetSchema : Migration
   {
       // Corrective changes
   }
   ```

2. **Document the issue**
   - Add comments explaining the fix
   - Reference the original problematic migration

3. **Test thoroughly**
   - Verify data integrity
   - Test in staging before production

## Monitoring

Check migration status:

```sql
-- PostgreSQL
SELECT * FROM "__EFMigrationsHistory" 
ORDER BY MigrationId DESC;
```

View current schema:

```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public';
```

