using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260131004)]
public class SeedPartMasterData : Migration
{
    public override void Up()
    {
        // Seeding disabled as per user request
    }

    public override void Down()
    {
        Delete.FromTable("component_names").AllRows();
        Delete.FromTable("component_groups").AllRows();
    }
}
