using FluentMigrator;

namespace ConfigurationService.Infrastructure.Migrations;

/// <summary>
/// Creates the core tables for the configuration service.
/// Hierarchy: Enquiry → Configuration → ConfigurationVersion → (SKUs, Pallets, Warehouse, Rack)
/// </summary>
[Migration(20260111001)]
public class M20260111001_CreateEnquiryTables : Migration
{
    public override void Up()
    {
        // Create enquiries table
        Create.Table("enquiries")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("external_enquiry_id").AsString(100).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("enquiry_no").AsString(205).NotNullable()
            .WithColumn("description").AsString(2000).Nullable()
            .WithColumn("customername").AsString().Nullable()
            .WithColumn("customercontact").AsInt32().Nullable()
            .WithColumn("customeremail").AsString(500).Nullable()
            .WithColumn("product_group").AsString(500).Nullable()
            .WithColumn("Billing_details").AsString(500).Nullable()
            .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("Draft")
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_enquiries_external_id").OnTable("enquiries").OnColumn("external_enquiry_id");
        Create.Index("idx_enquiries_status").OnTable("enquiries").OnColumn("status");

        // Create configurations table - design variants within an enquiry
        Create.Table("configurations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("enquiry_id").AsGuid().NotNullable().ForeignKey("fk_configurations_enquiry", "enquiries", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_primary").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_configurations_enquiry_id").OnTable("configurations").OnColumn("enquiry_id");

        // Create configuration_versions table - versioned snapshots of a configuration
        Create.Table("configuration_versions")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_id").AsGuid().NotNullable().ForeignKey("fk_config_versions_config", "configurations", "id")
            .WithColumn("version_number").AsInt32().NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("status").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_current").AsBoolean().NotNullable().WithDefaultValue(false)
             .WithColumn("is_locked").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable();

        Create.Index("idx_config_versions_config_id").OnTable("configuration_versions").OnColumn("configuration_id");
        Create.UniqueConstraint("uq_config_versions_config_version").OnTable("configuration_versions").Columns("configuration_id", "version_number");

        // Create enquiry_snapshots table (for named snapshots)
        Create.Table("enquiry_snapshots")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("enquiry_id").AsGuid().NotNullable().ForeignKey("fk_snapshots_enquiry", "enquiries", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("configuration_state").AsCustom("JSONB").NotNullable()
            .WithColumn("version_at_snapshot").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable();

        Create.Index("idx_snapshots_enquiry_id").OnTable("enquiry_snapshots").OnColumn("enquiry_id");

        // Create configuration_skus table - linked to configuration_version
        Create.Table("configuration_skus")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_config_skus_version", "configuration_versions", "id")
            .WithColumn("sku_type_id").AsGuid().Nullable()
            .WithColumn("code").AsString(50).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("attributes").AsCustom("JSONB").Nullable()
            .WithColumn("units_per_layer").AsInt32().Nullable()
            .WithColumn("layers_per_pallet").AsInt32().Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_config_skus_version_id").OnTable("configuration_skus").OnColumn("configuration_version_id");
        Create.UniqueConstraint("uq_config_skus_version_code").OnTable("configuration_skus").Columns("configuration_version_id", "code");

        // Create configuration_pallets table - linked to configuration_version
        Create.Table("configuration_pallets")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_config_pallets_version", "configuration_versions", "id")
            .WithColumn("pallet_type_id").AsGuid().Nullable()
            .WithColumn("code").AsString(50).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("attributes").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_config_pallets_version_id").OnTable("configuration_pallets").OnColumn("configuration_version_id");
        Create.UniqueConstraint("uq_config_pallets_version_code").OnTable("configuration_pallets").Columns("configuration_version_id", "code");

        // Create warehouse_configs table - linked to configuration_version
        Create.Table("warehouse_configs")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_warehouse_configs_version", "configuration_versions", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("length_m").AsDecimal(10, 2).NotNullable()
            .WithColumn("width_m").AsDecimal(10, 2).NotNullable()
            .WithColumn("clear_height_m").AsDecimal(10, 2).NotNullable()
            .WithColumn("floor_type").AsString(100).Nullable()
            .WithColumn("floor_load_capacity_kn_m2").AsDecimal(10, 2).Nullable()
            .WithColumn("seismic_zone").AsString(50).Nullable()
            .WithColumn("mhe_type").AsString(100).Nullable()
            .WithColumn("aisle_width_mm").AsDecimal(10, 2).Nullable()
            .WithColumn("temperature_range").AsString(100).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_warehouse_configs_version_id").OnTable("warehouse_configs").OnColumn("configuration_version_id");

        // Create rack_configurations table - linked to configuration_version
        Create.Table("rack_configurations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("configuration_version_id").AsGuid().NotNullable().ForeignKey("fk_rack_configs_version", "configuration_versions", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("product_group").AsString(50).NotNullable()
            .WithColumn("layout_data").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(255).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(255).Nullable();

        Create.Index("idx_rack_configs_version_id").OnTable("rack_configurations").OnColumn("configuration_version_id");
        Create.Index("idx_rack_configs_product_group").OnTable("rack_configurations").OnColumn("product_group");
    }

    public override void Down()
    {
        Delete.Table("rack_configurations");
        Delete.Table("warehouse_configs");
        Delete.Table("configuration_pallets");
        Delete.Table("configuration_skus");
        Delete.Table("enquiry_snapshots");
        Delete.Table("configuration_versions");
        Delete.Table("configurations");
        Delete.Table("enquiries");
    }
}
