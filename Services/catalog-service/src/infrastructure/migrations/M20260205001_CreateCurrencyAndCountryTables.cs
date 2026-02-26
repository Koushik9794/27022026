using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260205001)]
public class CreateCurrencyAndCountryTables : Migration
{
    public override void Up()
    {
        // 1. Create currency table
        Create.Table("currency")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("currency_code").AsFixedLengthString(3).NotNullable().Unique()
            .WithColumn("currency_name").AsString().NotNullable()
            .WithColumn("currency_value").AsString().Nullable()
            .WithColumn("decimal_unit").AsInt16().NotNullable().WithDefaultValue(2)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_delete").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("created_by").AsString().Nullable()
            .WithColumn("updated_by").AsString().Nullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("ix_currency_code").OnTable("currency").OnColumn("currency_code");

        // 2. Create country table
        Create.Table("country")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("country_code").AsFixedLengthString(2).NotNullable().Unique()
            .WithColumn("country_name").AsString().NotNullable()
            .WithColumn("currency_code").AsFixedLengthString(3).NotNullable()
                .ForeignKey("fk_country_currency", "currency", "currency_code")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_delete").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("created_by").AsString().Nullable()
            .WithColumn("updated_by").AsString().Nullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("ix_country_code").OnTable("country").OnColumn("country_code");
        Create.Index("ix_country_currency_code").OnTable("country").OnColumn("currency_code");

        // 3. Create exchange_currency table
        Create.Table("exchange_currency")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("base_currency").AsFixedLengthString(3).NotNullable()
                .ForeignKey("fk_fx_base", "currency", "currency_code")
            .WithColumn("quote_currency").AsFixedLengthString(3).NotNullable()
                .ForeignKey("fk_fx_quote", "currency", "currency_code")
            .WithColumn("rate").AsDecimal(20, 8).NotNullable()
            .WithColumn("valid_from").AsDate().NotNullable()
            .WithColumn("valid_end").AsDate().Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_delete").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("created_by").AsString().Nullable()
            .WithColumn("updated_by").AsString().Nullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("ix_fx_pair_from").OnTable("exchange_currency")
            .OnColumn("base_currency").Ascending()
            .OnColumn("quote_currency").Ascending()
            .OnColumn("valid_from").Descending();

        // Unique constraint for currency pair and start date
        Create.UniqueConstraint("uq_fx_pair_start")
            .OnTable("exchange_currency")
            .Columns("base_currency", "quote_currency", "valid_from");

        // Seed default Currency (INR) and Country (IN)
        Insert.IntoTable("currency").Row(new
        {
            id = Guid.NewGuid(),
            currency_code = "INR",
            currency_name = "Indian Rupee",
            decimal_unit = 2,
            is_active = true,
            is_delete = false,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        });

        Insert.IntoTable("country").Row(new
        {
            id = Guid.NewGuid(),
            country_code = "IN",
            country_name = "India",
            currency_code = "INR",
            is_active = true,
            is_delete = false,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        });
    }

    public override void Down()
    {
        Delete.Table("exchange_currency");
        Delete.Table("country");
        Delete.Table("currency");
    }
}
