using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260204004)]
public class AddPartsCreatedAtIndex : Migration
{
    public override void Up()
    {
        Create.Index("idx_parts_created_at").OnTable("parts").OnColumn("created_at");
    }

    public override void Down()
    {
        Delete.Index("idx_parts_created_at").OnTable("parts");
    }
}
