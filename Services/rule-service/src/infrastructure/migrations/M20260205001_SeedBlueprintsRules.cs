using FluentMigrator;
using System;

namespace RuleService.Infrastructure.Migrations
{
    [Migration(20260205001)]
    public class SeedBlueprintsRules : Migration
    {
        public override void Up()
        {
            var sprProductGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var usCountryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sprRuleSetId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // 1. Upright Rules
            var uprightRule1Id = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = uprightRule1Id,
                name = "SPR-UPRIGHT-001",
                description = "Frame height limit check",
                category = "STRUCTURAL",
                priority = 100,
                severity = "ERROR",
                enabled = true,
                formula = "GetNum(\"RackHeight\") > GetNum(\"Defaults.MAX_RACK_HEIGHT\") ? VALIDATE(\"NL001: Frame height limit exceeded\") : true",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            var uprightRule2Id = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = uprightRule2Id,
                name = "SPR-UPRIGHT-002",
                description = "Frame load check",
                category = "STRUCTURAL",
                priority = 90,
                severity = "ERROR",
                enabled = true,
                formula = "GetNum(\"TotalFrameLoad\") > GetNum(\"MaxFrameCapacity\") ? VALIDATE(\"NL006: Frame load exceeded\") : true",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            var uprightRule3Id = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = uprightRule3Id,
                name = "SPR-UPRIGHT-003",
                description = "Add Upright to BOM",
                category = "STRUCTURAL",
                priority = 80,
                severity = "INFO",
                enabled = true,
                formula = "ADD_BOM(LOOKUP(\"UPRIGHT\", \"Height\", GetNum(\"RackHeight\")), 2.0, \"MBOM\")",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            // 2. Beam Rules
            var beamRule1Id = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = beamRule1Id,
                name = "SPR-BEAM-001",
                description = "Beam overload check",
                category = "STRUCTURAL",
                priority = 100,
                severity = "ERROR",
                enabled = true,
                formula = "GetNum(\"LoadPerLevel\") > GetNum(\"BeamCapacity\") ? VALIDATE(\"BS001: Beam overload\") : true",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            var beamRule2Id = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = beamRule2Id,
                name = "SPR-BEAM-002",
                description = "Add Beam to BOM",
                category = "STRUCTURAL",
                priority = 80,
                severity = "INFO",
                enabled = true,
                formula = "ADD_BOM(LOOKUP(\"BEAM\", \"Span\", GetNum(\"RackWidth\")), 2.0, \"MBOM\")",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            // 3. BasePlate Rules
            var bpRuleId = Guid.NewGuid();
            Insert.IntoTable("rules").Row(new {
                id = bpRuleId,
                name = "SPR-BASEPLATE-001",
                description = "Add Baseplate to BOM",
                category = "STRUCTURAL",
                priority = 100,
                severity = "INFO",
                enabled = true,
                formula = "ADD_BOM(LOOKUP(\"BASEPLATE\", \"HeavyDuty\", 1.0), 2.0, \"MBOM\")",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });

            // Associate rules with SPR RuleSet
            var rules = new[] { uprightRule1Id, uprightRule2Id, uprightRule3Id, beamRule1Id, beamRule2Id, bpRuleId };
            foreach (var ruleId in rules)
            {
                Insert.IntoTable("ruleset_rules").Row(new {
                    ruleset_id = sprRuleSetId,
                    rule_id = ruleId,
                    added_at = DateTime.UtcNow
                });
            }
        }

        public override void Down()
        {
            // No-op for seed data deletion in Down to stay safe, 
            // but in real migration we might want to delete by IDs.
        }
    }
}
