using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260110001)]
public class CreateTaxonomyTables : Migration
{
    public override void Up()
    {
        // Create component_categories table
        Create.Table("component_categories")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("idx_component_categories_code").OnTable("component_categories").OnColumn("code");
        Create.Index("idx_component_categories_is_active").OnTable("component_categories").OnColumn("is_active");

        // Create component_types table
        Create.Table("component_types")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("category_id").AsGuid().NotNullable().ForeignKey("fk_component_types_category", "component_categories", "id")
            .WithColumn("parent_type_id").AsGuid().Nullable().ForeignKey("fk_component_types_parent", "component_types", "id")
            .WithColumn("attribute_schema").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("idx_component_types_code").OnTable("component_types").OnColumn("code");
        Create.Index("idx_component_types_category").OnTable("component_types").OnColumn("category_id");
        Create.Index("idx_component_types_parent").OnTable("component_types").OnColumn("parent_type_id");
        Create.Index("idx_component_types_is_active").OnTable("component_types").OnColumn("is_active");

        // Create product_groups table
        Create.Table("product_groups")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("parent_group_id").AsGuid().Nullable().ForeignKey("fk_product_groups_parent", "product_groups", "id")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("idx_product_groups_code").OnTable("product_groups").OnColumn("code");
        Create.Index("idx_product_groups_parent").OnTable("product_groups").OnColumn("parent_group_id");
        Create.Index("idx_product_groups_is_active").OnTable("product_groups").OnColumn("is_active");
    }

    public override void Down()
    {
        Delete.Table("component_types");
        Delete.Table("component_categories");
        Delete.Table("product_groups");
    }
}
