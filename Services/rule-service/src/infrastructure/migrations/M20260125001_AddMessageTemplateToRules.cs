using FluentMigrator;

namespace RuleService.Infrastructure.Migrations
{
    [Migration(20260125001)]
    public class M20260125001_AddMessageTemplateToRules : Migration
    {
        public override void Up()
        {
            // Add message_template column to rules table
            Alter.Table("rules")
                .AddColumn("message_template").AsString(1000).Nullable();

            // Migrate existing rules to use templates
            // For rules without templates, create a default template from the rule name
            Execute.Sql(@"
                UPDATE rules 
                SET message_template = CONCAT(name, ' validation failed.')
                WHERE message_template IS NULL;
            ");
        }

        public override void Down()
        {
            Delete.Column("message_template").FromTable("rules");
        }
    }
}
