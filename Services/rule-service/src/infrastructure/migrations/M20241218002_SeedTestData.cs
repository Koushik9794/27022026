using FluentMigrator;
using System;

namespace RuleService.Infrastructure.Migrations
{
    /// <summary>
    /// Seed migration - adds test data for development
    /// Run only in Development environment
    /// </summary>
    [Migration(20241218002)]
    public class SeedTestData : Migration
    {
        public override void Up()
        {
            var productGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var countryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var ruleSetId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var rule1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var rule2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

            // Seed RuleSets
            Insert.IntoTable("rule_sets").Row(new
            {
                id = ruleSetId,
                name = "Standard Warehouse Rules - US",
                product_group_id = productGroupId,
                country_id = countryId,
                effective_from = DateTime.UtcNow,
                effective_to = (DateTime?)null,
                status = "ACTIVE",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            // Seed Rules
            Insert.IntoTable("rules").Row(new
            {
                id = rule1Id,
                name = "Spatial Constraint - Minimum Dimensions",
                description = "Ensures items meet minimum spatial requirements",
                category = "SPATIAL",
                priority = 1,
                severity = "ERROR",
                enabled = true,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            Insert.IntoTable("rules").Row(new
            {
                id = rule2Id,
                name = "Pricing Rule - Bulk Discount",
                description = "Apply bulk discount for orders over 100 units",
                category = "PRICING",
                priority = 2,
                severity = "INFO",
                enabled = true,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            // Seed RuleConditions
            Insert.IntoTable("rule_conditions").Row(new
            {
                id = Guid.NewGuid(),
                rule_id = rule1Id,
                type = "AND",
                field = "width",
                @operator = "GT",
                value = "100"
            });

            Insert.IntoTable("rule_conditions").Row(new
            {
                id = Guid.NewGuid(),
                rule_id = rule1Id,
                type = "AND",
                field = "height",
                @operator = "GT",
                value = "100"
            });

            Insert.IntoTable("rule_conditions").Row(new
            {
                id = Guid.NewGuid(),
                rule_id = rule2Id,
                type = "AND",
                field = "quantity",
                @operator = "GT",
                value = "100"
            });

            // Seed RuleVersions
            Insert.IntoTable("rule_versions").Row(new
            {
                id = Guid.NewGuid(),
                rule_id = rule1Id,
                version_number = 1,
                change_log = "Initial version",
                rule_definition = "{\"type\": \"SPATIAL\", \"conditions\": [{\"field\": \"width\", \"operator\": \"GT\", \"value\": 100}]}",
                created_at = DateTime.UtcNow,
                created_by = "System"
            });

            Insert.IntoTable("rule_versions").Row(new
            {
                id = Guid.NewGuid(),
                rule_id = rule2Id,
                version_number = 1,
                change_log = "Initial version",
                rule_definition = "{\"type\": \"PRICING\", \"discount\": 0.15}",
                created_at = DateTime.UtcNow,
                created_by = "System"
            });

            // Seed RuleSet-Rule associations
            Insert.IntoTable("ruleset_rules").Row(new
            {
                ruleset_id = ruleSetId,
                rule_id = rule1Id,
                added_at = DateTime.UtcNow
            });

            Insert.IntoTable("ruleset_rules").Row(new
            {
                ruleset_id = ruleSetId,
                rule_id = rule2Id,
                added_at = DateTime.UtcNow
            });

            // Seed Audit Logs
            Insert.IntoTable("audit_logs").Row(new
            {
                id = Guid.NewGuid(),
                entity_id = ruleSetId,
                entity_type = "RuleSet",
                action = "CREATE",
                changes = "{\"name\": \"Standard Warehouse Rules - US\"}",
                created_by = "System",
                created_at = DateTime.UtcNow
            });
        }

        public override void Down()
        {
            // Delete in reverse order of foreign keys
            Delete.FromTable("audit_logs").AllRows();
            Delete.FromTable("ruleset_rules").AllRows();
            Delete.FromTable("rule_versions").AllRows();
            Delete.FromTable("rule_conditions").AllRows();
            Delete.FromTable("rules").AllRows();
            Delete.FromTable("rule_sets").AllRows();
        }
    }
}
