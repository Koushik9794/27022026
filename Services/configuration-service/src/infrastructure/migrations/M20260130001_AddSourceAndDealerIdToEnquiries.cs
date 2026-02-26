using FluentMigrator;

namespace ConfigurationService.Infrastructure.Migrations;

/// <summary>
/// Adds source and dealerid columns to the enquiries table
/// </summary>
[Migration(20260130001)]
public class M20260130001_AddSourceAndDealerIdToEnquiries : Migration
{
    public override void Up()
    {
        Alter.Table("enquiries")
            .AddColumn("source").AsString(200).Nullable()
            .AddColumn("dealerid").AsGuid().Nullable();
    }

    public override void Down()
    {
        Delete.Column("source").FromTable("enquiries");
        Delete.Column("dealerid").FromTable("enquiries");
    }
}
