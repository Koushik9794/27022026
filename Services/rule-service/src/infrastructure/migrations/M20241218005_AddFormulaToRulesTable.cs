using FluentMigrator;

namespace RuleService.Infrastructure.Migrations
{
    [Migration(20241218005)]
    public class AddFormulaToRulesTable : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("rules").Column("formula").Exists())
            {
                Alter.Table("rules")
                    .AddColumn("formula").AsString(int.MaxValue).Nullable();
            }
        }

        public override void Down()
        {
            if (Schema.Table("rules").Column("formula").Exists())
            {
                Delete.Column("formula").FromTable("rules");
            }
        }
    }
}
