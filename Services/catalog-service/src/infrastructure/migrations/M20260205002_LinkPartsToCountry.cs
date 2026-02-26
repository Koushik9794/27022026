using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260205002)]
public class LinkPartsToCountry : Migration
{
    public override void Up()
    {
        // Ensure default data exists to satisfy FK constraint
        Execute.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM currency WHERE currency_code = 'INR') THEN
                    INSERT INTO currency (id, currency_code, currency_name, decimal_unit, is_active, is_delete, created_at, updated_at)
                    VALUES (gen_random_uuid(), 'INR', 'Indian Rupee', 2, true, false, now(), now());
                END IF;

                IF NOT EXISTS (SELECT 1 FROM country WHERE country_code = 'IN') THEN
                    INSERT INTO country (id, country_code, country_name, currency_code, is_active, is_delete, created_at, updated_at)
                    VALUES (gen_random_uuid(), 'IN', 'India', 'INR', true, false, now(), now());
                END IF;
            END
            $$;
        ");

        // Add foreign key constraint to parts.country_code
        
        Create.ForeignKey("fk_parts_country")
            .FromTable("parts").ForeignColumn("country_code")
            .ToTable("country").PrimaryColumn("country_code");
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_parts_country").OnTable("parts");
    }
}
