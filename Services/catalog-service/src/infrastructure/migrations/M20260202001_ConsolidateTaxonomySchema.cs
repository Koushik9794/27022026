using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260202001)]
public class ConsolidateTaxonomySchema : Migration
{
    public override void Up()
    {
        // 1. Drop existing foreign key and index from component_types
        Delete.ForeignKey("fk_component_types_category").OnTable("component_types");
        Delete.Index("idx_component_types_category").OnTable("component_types");

        // 2. Remap and transfer data
        Execute.WithConnection((conn, tran) =>
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;

            // Remap component_types based on TRIMMING and CASE-INSENSITIVE code matches
            cmd.CommandText = @"
                UPDATE component_types ct
                SET category_id = cg.id
                FROM component_categories cc
                JOIN component_groups cg ON UPPER(TRIM(cc.code)) = UPPER(TRIM(cg.code))
                WHERE ct.category_id = cc.id;";
            cmd.ExecuteNonQuery();

            // Second, insert categories that don't exist as groups yet
            // Use NOT EXISTS with case-insensitive check
            cmd.CommandText = @"
                INSERT INTO component_groups (id, code, name, description, sort_order, is_active, created_at, updated_at)
                SELECT cc.id, cc.code, cc.name, cc.description, cc.sort_order, cc.is_active, cc.created_at, cc.updated_at 
                FROM component_categories cc
                WHERE NOT EXISTS (
                    SELECT 1 FROM component_groups cg 
                    WHERE UPPER(TRIM(cg.code)) = UPPER(TRIM(cc.code))
                )
                ON CONFLICT (id) DO NOTHING;";
            cmd.ExecuteNonQuery();
        });

        // 3. Rename the column in component_types
        Rename.Column("category_id").OnTable("component_types").To("component_group_id");

        // 4. Add new foreign key pointing to component_groups
        Create.ForeignKey("fk_component_types_group")
            .FromTable("component_types").ForeignColumn("component_group_id")
            .ToTable("component_groups").PrimaryColumn("id");

        // 4. Create new index for the renamed column
        Create.Index("idx_component_types_group").OnTable("component_types").OnColumn("component_group_id");

        // 5. Drop the obsolete component_categories table
        Delete.Table("component_categories");
    }

    public override void Down()
    {
        // Re-recreate component_categories table structure
        Create.Table("component_categories")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        // Rename the column back
        Rename.Column("component_group_id").OnTable("component_types").To("category_id");

        // Drop the new FK and Index
        Delete.ForeignKey("fk_component_types_group").OnTable("component_types");
        Delete.Index("idx_component_types_group").OnTable("component_types");

        // Re-create the old FK and Index
        Create.ForeignKey("fk_component_types_category")
            .FromTable("component_types").ForeignColumn("category_id")
            .ToTable("component_categories").PrimaryColumn("id");
            
        Create.Index("idx_component_types_category").OnTable("component_types").OnColumn("category_id");
    }
}
