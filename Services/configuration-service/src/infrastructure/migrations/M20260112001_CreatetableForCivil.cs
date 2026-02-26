using FluentMigrator;

namespace ConfigurationService.infrastructure.migrations;

[Migration(202601180110)]
public class M20260112001_CreatetableForCivil : Migration
{
    public override void Up()
    {
        // Create Civil layout table - design variants within an enquiry
        Create.Table("civil_layout")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_id").AsGuid().NotNullable().ForeignKey("fk_civil_layout", "configurations", "id")
            .WithColumn("warehouse_type").AsString().Nullable()
            .WithColumn("source_file").AsString().Nullable()
            .WithColumn("civil_json").AsString().Nullable()
            .WithColumn("versionno").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();
        Create.Index("idx_civil_layout_configuration_id").OnTable("civil_layout").OnColumn("configuration_id");

        Create.Table("rack_layout")
    .WithColumn("id").AsGuid().PrimaryKey()
    .WithColumn("civil_layout_id").AsGuid().NotNullable().ForeignKey("fk_rack_layout", "civil_layout", "id")
    .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_configuration_rack_layout", "configuration_versions", "id")
    .WithColumn("rack_json").AsString().Nullable()
    .WithColumn("configuration_layout").AsCustom("JSONB").Nullable()
    .WithColumn("created_at").AsDateTime().NotNullable()
    .WithColumn("created_by").AsString(255).Nullable()
    .WithColumn("updated_at").AsDateTime().Nullable()
    .WithColumn("updated_by").AsString(255).Nullable();
        Create.Index("idx_rack_layout_civil_layout_id").OnTable("rack_layout").OnColumn("civil_layout_id");
        Create.Index("idx_rack_layout_configuration_id").OnTable("rack_layout").OnColumn("configuration_version_id");
    }
    public override void Down()
    {
        Delete.Table("civil_layout");
        Delete.Table("rack_layout");
    }
}
