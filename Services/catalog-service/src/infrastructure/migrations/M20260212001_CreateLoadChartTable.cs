using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260212001)]
public class CreateLoadChartTable : Migration
{
    public override void Up()
    {
        Create.Table("load_chart")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("product_group_id").AsGuid().NotNullable().ForeignKey("fk_load_chart_product_group", "product_groups", "id")
            .WithColumn("chart_type").AsString().NotNullable()
            .WithColumn("component_code").AsString(50).NotNullable()
            .WithColumn("component_type_id").AsGuid().NotNullable()
            .WithColumn("attributes").AsCustom("JSONB").NotNullable().WithDefaultValue("{}")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_delete").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_by").AsGuid().Nullable()
            .WithColumn("updated_by").AsGuid().Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTimeOffset().Nullable();

        // Composite foreign key to component_names (code + component_type_id)
        Execute.Sql(@"
            ALTER TABLE load_chart 
            ADD CONSTRAINT fk_load_chart_component 
            FOREIGN KEY (component_code, component_type_id) 
            REFERENCES component_names (code, component_type_id)
        ");

        Create.Index("idx_load_chart_product_group").OnTable("load_chart").OnColumn("product_group_id");
        Create.Index("idx_load_chart_component").OnTable("load_chart").OnColumn("component_code");
        Create.Index("idx_load_chart_component_type").OnTable("load_chart").OnColumn("component_type_id");
        Create.Index("idx_load_chart_chart_type").OnTable("load_chart").OnColumn("chart_type");
    }

    public override void Down()
    {
        Delete.Table("load_chart");
    }
}
