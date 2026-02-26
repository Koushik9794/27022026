using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260107001)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        // Create SKUs table
        Create.Table("skus")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("glb_file_path").AsString().Nullable()
            .WithColumn("attribute_schema").AsCustom("JSONB").Nullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();

        Create.Index("idx_skus_code").OnTable("skus").OnColumn("code");
        Create.Index("idx_skus_is_deleted").OnTable("skus").OnColumn("is_deleted");

        // Create Pallets table
        Create.Table("pallets")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("pallet_type").AsString(100).Nullable()
             .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("glb_file_path").AsString().Nullable()
            .WithColumn("attribute_schema").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();

        Create.Index("idx_pallets_code").OnTable("pallets").OnColumn("code");
        Create.Index("idx_pallets_is_deleted").OnTable("pallets").OnColumn("is_deleted");

        // Create MHEs table
        Create.Table("mhes")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("manufacturer").AsString().Nullable()
            .WithColumn("brand").AsString().Nullable()
            .WithColumn("model").AsString().Nullable()
             .WithColumn("mhe_type").AsString().Nullable()
              .WithColumn("mhe_category").AsString().Nullable()
            .WithColumn("glb_file_path").AsString().Nullable()
            .WithColumn("attributes").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();

        Create.Index("idx_mhes_code").OnTable("mhes").OnColumn("code");
        Create.Index("idx_mhes_is_deleted").OnTable("mhes").OnColumn("is_deleted");
    }

    public override void Down()
    {
        Delete.Table("skus");
        Delete.Table("pallets");
        Delete.Table("mhes");
    }
}
