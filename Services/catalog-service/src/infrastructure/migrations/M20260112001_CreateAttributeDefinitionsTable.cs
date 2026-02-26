using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260112001)]
public class CreateAttributeDefinitionsTable : Migration
{
    public override void Up()
    {
        Create.Table("attribute_definitions")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("attribute_key").AsString(100).NotNullable()
            .WithColumn("display_name").AsString(200).NotNullable()
            .WithColumn("unit").AsString(50).Nullable()
            .WithColumn("data_type").AsString(50).NotNullable() // Enum as string
            .WithColumn("min_value").AsDecimal().Nullable()
            .WithColumn("max_value").AsDecimal().Nullable()
            .WithColumn("default_value").AsCustom("JSONB").Nullable()
            .WithColumn("is_required").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("allowed_values").AsCustom("JSONB").Nullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("screen").AsString(50).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();


        Create.Index("idx_attribute_definitions_key_screen")
            .OnTable("attribute_definitions")
            .OnColumn("attribute_key").Ascending()
            .OnColumn("screen").Ascending()
            .WithOptions().Unique();

        Create.Index("idx_attribute_definitions_is_active").OnTable("attribute_definitions").OnColumn("is_active");
    }

    public override void Down()
    {
        Delete.Table("attribute_definitions");
    }
}
