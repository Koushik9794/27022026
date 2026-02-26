using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260131003)]
public class CreatePartsTable : Migration
{
    public override void Up()
    {
        Create.Table("parts")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("part_code").AsString(100).NotNullable()
            .WithColumn("country_code").AsString(2).NotNullable()
            .WithColumn("unspsc_code").AsString(50).Nullable()
            .WithColumn("component_group_id").AsGuid().NotNullable().ForeignKey("fk_parts_group", "component_groups", "id")
            .WithColumn("component_type_id").AsGuid().NotNullable().ForeignKey("fk_parts_type", "component_types", "id")
            .WithColumn("component_name_id").AsGuid().Nullable().ForeignKey("fk_parts_name", "component_names", "id")
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
            .WithColumn("status").AsString(20).NotNullable().WithDefaultValue("ACTIVE")
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();

        // Unique constraint on part_code + country_code
        Create.UniqueConstraint("uq_parts_code_country")
            .OnTable("parts")
            .Columns("part_code", "country_code");

        // Indexes
        Create.Index("idx_parts_code").OnTable("parts").OnColumn("part_code");
        Create.Index("idx_parts_country").OnTable("parts").OnColumn("country_code");
        Create.Index("idx_parts_group").OnTable("parts").OnColumn("component_group_id");
        Create.Index("idx_parts_type").OnTable("parts").OnColumn("component_type_id");
        Create.Index("idx_parts_name").OnTable("parts").OnColumn("component_name_id");
        Create.Index("idx_parts_status").OnTable("parts").OnColumn("status");
        Create.Index("idx_parts_is_deleted").OnTable("parts").OnColumn("is_deleted");
    }

    public override void Down()
    {
        Delete.Table("parts");
    }
}
