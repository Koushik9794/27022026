using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260203001)]
public class M20260203001_AddImageUrlToParts : Migration
{
    public override void Up()
    {
        Alter.Table("parts")
            .AddColumn("image_url").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Column("image_url").FromTable("parts");
    }
}
