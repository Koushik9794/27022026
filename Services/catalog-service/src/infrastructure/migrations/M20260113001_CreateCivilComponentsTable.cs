using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine.Persistence;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260113002)]
public class CreateCivilComponentsTable : Migration
{
    public override void Up()
    {
        Create.Table("civil_components")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(100).NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("label").AsString(200).NotNullable()
            .WithColumn("icon").AsString(100).NotNullable()
            .WithColumn("tooltip").AsString(500).Nullable()
            .WithColumn("category").AsString(100).NotNullable()
            .WithColumn("default_element").AsCustom("JSONB").Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("created_by").AsString(100).Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("updated_by").AsString(100).Nullable();
        // Unique index on Code
        Create.Index("idx_civil_components_code").OnTable("civil_components").OnColumn("code").Unique();

        // Index on Category for fast filtering
        Create.Index("idx_civil_components_category").OnTable("civil_components").OnColumn("category");
    }
    public override void Down()
    {
        Delete.Table("civil_components");
    }
}
