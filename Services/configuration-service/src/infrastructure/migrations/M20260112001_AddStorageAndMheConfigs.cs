using FluentMigrator;

namespace ConfigurationService.Infrastructure.Migrations;

/// <summary>
/// Renames rack_configurations to storage_configurations, adds floor_id and mhe_configs table.
/// Supports per-floor design layers with civil layout + storage placements.
/// </summary>
[Migration(20260112001)]
public class M20260112001_AddStorageAndMheConfigs : Migration
{
    public override void Up()
    {
        // Rename rack_configurations to storage_configurations
        Rename.Table("rack_configurations").To("storage_configurations");
        
        // Rename layout_data to design_data
        Rename.Column("layout_data").OnTable("storage_configurations").To("design_data");
        
        // Add floor_id column for per-floor design layers
        Alter.Table("storage_configurations")
            .AddColumn("floor_id").AsGuid().Nullable()
            .AddColumn("last_saved_at").AsDateTime().Nullable();
        
        Create.Index("idx_storage_configs_floor_id").OnTable("storage_configurations").OnColumn("floor_id");

        // Rename foreign key constraint (drop old, create new)
        Delete.ForeignKey("fk_rack_configs_version").OnTable("storage_configurations");
        Create.ForeignKey("fk_storage_configs_version")
            .FromTable("storage_configurations").ForeignColumn("configuration_version_id")
            .ToTable("configuration_versions").PrimaryColumn("id");

        // Rename indexes
        Delete.Index("idx_rack_configs_version_id").OnTable("storage_configurations");
        Delete.Index("idx_rack_configs_product_group").OnTable("storage_configurations");
        Create.Index("idx_storage_configs_version_id").OnTable("storage_configurations").OnColumn("configuration_version_id");
        Create.Index("idx_storage_configs_product_group").OnTable("storage_configurations").OnColumn("product_group");

        // Create mhe_configs table
        Create.Table("mhe_configs")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_mhe_configs_version", "configuration_versions", "id")
            .WithColumn("mhe_type_id").AsGuid().Nullable()  // References catalog-service MHE type
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("attributes").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_mhe_configs_version_id").OnTable("mhe_configs").OnColumn("configuration_version_id");
    }

    public override void Down()
    {
        // Drop mhe_configs table
        Delete.Table("mhe_configs");

        // Remove new columns from storage_configurations
        Delete.Index("idx_storage_configs_floor_id").OnTable("storage_configurations");
        Delete.Column("floor_id").FromTable("storage_configurations");
        Delete.Column("last_saved_at").FromTable("storage_configurations");

        // Rename foreign key back
        Delete.ForeignKey("fk_storage_configs_version").OnTable("storage_configurations");
        Create.ForeignKey("fk_rack_configs_version")
            .FromTable("storage_configurations").ForeignColumn("configuration_version_id")
            .ToTable("configuration_versions").PrimaryColumn("id");

        // Rename indexes back
        Delete.Index("idx_storage_configs_version_id").OnTable("storage_configurations");
        Delete.Index("idx_storage_configs_product_group").OnTable("storage_configurations");
        Create.Index("idx_rack_configs_version_id").OnTable("storage_configurations").OnColumn("configuration_version_id");
        Create.Index("idx_rack_configs_product_group").OnTable("storage_configurations").OnColumn("product_group");

        // Rename design_data back to layout_data
        Rename.Column("design_data").OnTable("storage_configurations").To("layout_data");

        // Rename table back
        Rename.Table("storage_configurations").To("rack_configurations");
    }
}
