using FluentMigrator;

namespace RuleService.Infrastructure.Migrations
{
    /// <summary>
    /// Migration to add lookup_matrices table for handling load charts, price matrices, etc.
    /// Supports range lookups and continuous interpolation via JSONB.
    /// </summary>
    [Migration(20241218006)]
    public class AddLookupMatricesTable : Migration
    {
        public override void Up()
        {
            Create.Table("lookup_matrices")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable().Unique()
                .WithColumn("category").AsString(100).NotNullable() // SELECTION, PRICING, STABILITY
                .WithColumn("data").AsCustom("jsonb").NotNullable()
                .WithColumn("metadata").AsCustom("jsonb").Nullable() // Stores lookup strategy, units, tolerance
                .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1)
                .WithColumn("created_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Index("idx_matrices_name").OnTable("lookup_matrices").OnColumn("name");
            Create.Index("idx_matrices_category").OnTable("lookup_matrices").OnColumn("category");
        }

        public override void Down()
        {
            Delete.Table("lookup_matrices");
        }
    }
}
