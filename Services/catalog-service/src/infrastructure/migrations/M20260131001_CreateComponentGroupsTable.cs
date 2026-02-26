using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260131001)]
public class CreateComponentGroupsTable : Migration
{
    public override void Up()
    {
        Create.Table("component_groups")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("idx_component_groups_code").OnTable("component_groups").OnColumn("code");
        Create.Index("idx_component_groups_is_active").OnTable("component_groups").OnColumn("is_active");
    }

    public override void Down()
    {
        Delete.Table("component_groups");
    }
}
