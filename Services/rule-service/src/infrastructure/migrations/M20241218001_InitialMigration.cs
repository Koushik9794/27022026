using FluentMigrator;

namespace RuleService.Infrastructure.Migrations
{
    /// <summary>
    /// Initial migration - creates base schema for rule service
    /// </summary>
    [Migration(20241218001)]
    public class InitialMigration : Migration
    {
        public override void Up()
        {
            // Create RuleSet table
            Create.Table("rule_sets")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("product_group_id").AsGuid().NotNullable()
                .WithColumn("country_id").AsGuid().NotNullable()
                .WithColumn("effective_from").AsDateTime2().NotNullable()
                .WithColumn("effective_to").AsDateTime2().Nullable()
                .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("DRAFT")
                .WithColumn("created_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Index("idx_rule_sets_product_country")
                .OnTable("rule_sets")
                .OnColumn("product_group_id").Ascending()
                .OnColumn("country_id").Ascending()
                .WithOptions().NonClustered();

            // Create Rule table
            Create.Table("rules")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("description").AsString(1000).Nullable()
                .WithColumn("category").AsString(50).NotNullable() // SPATIAL, STRUCTURAL, ACCESSORY, PRICING, COMPLIANCE
                .WithColumn("priority").AsInt32().NotNullable()
                .WithColumn("severity").AsString(50).NotNullable() // ERROR, WARNING, INFO
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("created_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Index("idx_rules_category")
                .OnTable("rules")
                .OnColumn("category").Ascending()
                .WithOptions().NonClustered();

            // Create RuleCondition table
            Create.Table("rule_conditions")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("rule_id").AsGuid().NotNullable()
                .WithColumn("type").AsString(50).NotNullable() // AND, OR, NOT
                .WithColumn("field").AsString(255).NotNullable()
                .WithColumn("operator").AsString(50).NotNullable() // EQ, NE, LT, GT, CONTAINS
                .WithColumn("value").AsString(4000).Nullable();

            Create.ForeignKey("fk_rule_conditions_rule")
                .FromTable("rule_conditions").ForeignColumn("rule_id")
                .ToTable("rules").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            // Create RuleVersion table
            Create.Table("rule_versions")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("rule_id").AsGuid().NotNullable()
                .WithColumn("version_number").AsInt32().NotNullable()
                .WithColumn("change_log").AsString(4000).Nullable()
                .WithColumn("rule_definition").AsString(int.MaxValue).NotNullable()
                .WithColumn("created_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("created_by").AsString(255).NotNullable();

            Create.ForeignKey("fk_rule_versions_rule")
                .FromTable("rule_versions").ForeignColumn("rule_id")
                .ToTable("rules").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            // Create RuleSetRule junction table (many-to-many)
            Create.Table("ruleset_rules")
                .WithColumn("ruleset_id").AsGuid().NotNullable()
                .WithColumn("rule_id").AsGuid().NotNullable()
                .WithColumn("added_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.PrimaryKey("pk_ruleset_rules")
                .OnTable("ruleset_rules")
                .Columns("ruleset_id", "rule_id");

            Create.ForeignKey("fk_ruleset_rules_ruleset")
                .FromTable("ruleset_rules").ForeignColumn("ruleset_id")
                .ToTable("rule_sets").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.ForeignKey("fk_ruleset_rules_rule")
                .FromTable("ruleset_rules").ForeignColumn("rule_id")
                .ToTable("rules").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            // Create Audit Log table
            Create.Table("audit_logs")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("entity_id").AsGuid().NotNullable()
                .WithColumn("entity_type").AsString(255).NotNullable()
                .WithColumn("action").AsString(50).NotNullable() // CREATE, UPDATE, DELETE, EVALUATE
                .WithColumn("changes").AsString(int.MaxValue).Nullable() // JSON
                .WithColumn("created_by").AsString(255).NotNullable()
                .WithColumn("created_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Index("idx_audit_logs_entity")
                .OnTable("audit_logs")
                .OnColumn("entity_id").Ascending()
                .OnColumn("entity_type").Ascending()
                .WithOptions().NonClustered();
        }

        public override void Down()
        {
            Delete.Table("audit_logs");
            Delete.Table("ruleset_rules");
            Delete.Table("rule_versions");
            Delete.Table("rule_conditions");
            Delete.Table("rules");
            Delete.Table("rule_sets");
        }
    }
}
