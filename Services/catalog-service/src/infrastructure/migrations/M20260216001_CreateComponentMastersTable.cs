using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260216001)]
public class CreateComponentMastersTable : Migration
{
    public override void Up()
    {
        Create.Table("component_masters")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("component_master_code").AsString(100).NotNullable()
            .WithColumn("country_code").AsString(2).NotNullable()
            .WithColumn("unspsc_code").AsString(50).Nullable()
            .WithColumn("component_group_id").AsGuid().NotNullable().ForeignKey("fk_component_masters_group", "component_groups", "id")
            .WithColumn("component_type_id").AsGuid().NotNullable().ForeignKey("fk_component_masters_type", "component_types", "id")
            .WithColumn("component_name_id").AsGuid().Nullable().ForeignKey("fk_component_masters_name", "component_names", "id")
            .WithColumn("colour").AsString(50).Nullable()
            .WithColumn("powder_code").AsString(50).Nullable()
            .WithColumn("gfa_flag").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("unit_basic_price").AsDecimal(14, 2).NotNullable()
            .WithColumn("cbm").AsDecimal(12, 5).Nullable()
            .WithColumn("short_description").AsString(255).Nullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("drawing_no").AsString(100).Nullable()
            .WithColumn("rev_no").AsString(50).Nullable()
            .WithColumn("installation_ref_no").AsString(100).Nullable()
            .WithColumn("attributes").AsCustom("JSONB").NotNullable().WithDefaultValue("{}")
            .WithColumn("glb_filepath").AsString(1000).Nullable()
            .WithColumn("image_url").AsString(1000).Nullable()
            .WithColumn("status").AsString(20).NotNullable().WithDefaultValue("ACTIVE")
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();

        // Unique constraint on component_master_code + country_code
        Create.UniqueConstraint("uq_component_masters_code_country")
            .OnTable("component_masters")
            .Columns("component_master_code", "country_code");

        // Indexes
        Create.Index("idx_component_masters_code").OnTable("component_masters").OnColumn("component_master_code");
        Create.Index("idx_component_masters_country").OnTable("component_masters").OnColumn("country_code");
        Create.Index("idx_component_masters_group").OnTable("component_masters").OnColumn("component_group_id");
        Create.Index("idx_component_masters_type").OnTable("component_masters").OnColumn("component_type_id");
        Create.Index("idx_component_masters_name").OnTable("component_masters").OnColumn("component_name_id");
        Create.Index("idx_component_masters_status").OnTable("component_masters").OnColumn("status");
        Create.Index("idx_component_masters_is_deleted").OnTable("component_masters").OnColumn("is_deleted");
    }

    public override void Down()
    {
        Delete.Table("component_masters");
    }
}
