using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine.Persistence;

namespace CatalogService.Infrastructure.Migrations;

[Migration(202601161819)]
public class AlterWarehousetype : Migration
{


    public override void Up()
    {
        Alter.Table("warehouse_types")
            .AddColumn("template_path_civil").AsString(500).Nullable()
            .AddColumn("template_path_json").AsString(500).Nullable();

        Delete.Column("template_path").FromTable("warehouse_types");
    }

    public override void Down()
    {
        // Rollback: remove new columns
        Delete.Column("template_path_civil").FromTable("warehouse_types");
        Delete.Column("template_path_json").FromTable("warehouse_types");

        // Rollback: add old column back
        Alter.Table("warehouse_types")
            .AddColumn("template_path").AsString(500).Nullable();
    }

}
