using FluentMigrator;

namespace BomService.Infrastructure.Migrations
{
    [Migration(20260205001)]
    public class InitialMigration : Migration
    {
        public override void Up()
        {
            Create.Table("bill_of_materials")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("configuration_id").AsGuid().NotNullable()
                .WithColumn("project_name").AsString(255).NotNullable()
                .WithColumn("created_at").AsDateTime2().NotNullable();

            Create.Table("bom_items")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("bom_id").AsGuid().NotNullable()
                .WithColumn("sku").AsString(100).NotNullable()
                .WithColumn("qty").AsDouble().NotNullable()
                .WithColumn("category").AsString(50).NotNullable();

            Create.ForeignKey("fk_bom_items_bom")
                .FromTable("bom_items").ForeignColumn("bom_id")
                .ToTable("bill_of_materials").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.Table("bom_items");
            Delete.Table("bill_of_materials");
        }
    }
}
