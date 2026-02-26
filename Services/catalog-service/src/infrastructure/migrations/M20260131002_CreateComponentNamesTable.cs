using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260131002)]
public class CreateComponentNamesTable : Migration
{
    public override void Up()
    {
        Create.Table("component_names")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("component_type_id").AsGuid().NotNullable().ForeignKey("fk_component_names_type", "component_types", "id")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Index("idx_component_names_code").OnTable("component_names").OnColumn("code");
        Create.Index("idx_component_names_type").OnTable("component_names").OnColumn("component_type_id");
        Create.Index("idx_component_names_is_active").OnTable("component_names").OnColumn("is_active");
    }

    public override void Down()
    {
        Delete.Table("component_names");
    }
}
