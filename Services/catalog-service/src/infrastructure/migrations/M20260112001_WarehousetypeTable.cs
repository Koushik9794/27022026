namespace CatalogService.Infrastructure.Migrations;

using FluentMigrator;

[Migration(20260113001)]
public class CreateWarehouseTypesTable : Migration
{
    public override void Up()
    {
        Create.Table("warehouse_types")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("label").AsString(200).NotNullable()
            .WithColumn("icon").AsString(100).NotNullable()
            .WithColumn("tooltip").AsString(500).Nullable()
            .WithColumn("template_path").AsString(500).Nullable()
            .WithColumn("attributes").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();
        // Unique index on Name
        Create.Index("idx_warehouse_types_name").OnTable("warehouse_types").OnColumn("name").Unique();

        // Index on IsActive for filtering
        Create.Index("idx_warehouse_types_is_active").OnTable("warehouse_types").OnColumn("is_active");
    }
    public override void Down()
    {
        Delete.Table("warehouse_types");
    }
}
