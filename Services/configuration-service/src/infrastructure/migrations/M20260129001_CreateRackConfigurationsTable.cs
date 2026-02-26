using FluentMigrator;

namespace ConfigurationService.Infrastructure.Migrations;

[Migration(20260129001)]
public class M20260129001_CreateRackConfigurationsTable : Migration
{
    public override void Up()
    {
        Execute.Sql("DROP TABLE IF EXISTS rack_configurations CASCADE;");

        Create.Table("rack_configurations")
            .WithColumn("id").AsGuid().PrimaryKey("PK_rack_configurations_id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("configuration_layout").AsCustom("JSONB").NotNullable()
            .WithColumn("product_code").AsString(50).NotNullable()
            .WithColumn("scope").AsString(50).NotNullable() // ENQUIRY, PERSONAL, GLOBAL
            .WithColumn("enquiry_id").AsGuid().Nullable().ForeignKey("fk_rack_configs_enquiry", "enquiries", "id")
            .WithColumn("is_approved_by_admin").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("approved_by").AsString(255).Nullable()
            .WithColumn("approved_on").AsDateTime().Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_on").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_on").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_rack_configs_enquiry_id").OnTable("rack_configurations").OnColumn("enquiry_id");
        Create.Index("idx_rack_configs_scope").OnTable("rack_configurations").OnColumn("scope");
    }

    public override void Down()
    {
        Delete.Table("rack_configurations");
    }
}
