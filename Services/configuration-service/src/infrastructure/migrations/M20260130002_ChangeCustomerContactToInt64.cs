using FluentMigrator;

namespace ConfigurationService.Infrastructure.Migrations;

/// <summary>
/// Changes customercontact column from Int32 to Int64 to handle larger phone numbers
/// </summary>
[Migration(20260130002)]
public class M20260130002_ChangeCustomerContactToInt64 : Migration
{
    public override void Up()
    {
        Alter.Table("enquiries")
            .AlterColumn("customercontact").AsInt64().Nullable();

    }

    public override void Down()
    {
        Alter.Table("enquiries")
            .AlterColumn("customercontact").AsInt32().Nullable();

        Delete.Column("configuration_layout").FromTable("rack_layout");
    }
}
