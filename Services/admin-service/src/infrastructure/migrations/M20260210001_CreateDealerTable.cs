using FluentMigrator;

namespace AdminService.Infrastructure.Migrations
{
    [Migration(20260210001)]
    public class CreateDealerTable : Migration
    {
        public override void Up()
        {
            Create.Table("dealers")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("code").AsString(50).NotNullable().Unique()
                .WithColumn("name").AsString(200).NotNullable()
                .WithColumn("contact_name").AsString(100).Nullable()
                .WithColumn("contact_email").AsString(255).Nullable()
                .WithColumn("contact_phone").AsString(50).Nullable()
                .WithColumn("country_code").AsFixedLengthString(2).Nullable() // Mapped to country table in catalog-service
                .WithColumn("state").AsString(100).Nullable()
                .WithColumn("city").AsString(100).Nullable()
                .WithColumn("address").AsString(500).Nullable()
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_by").AsGuid().NotNullable()
                .WithColumn("updated_by").AsGuid().Nullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();

            Create.Index("idx_dealers_code")
                .OnTable("dealers")
                .OnColumn("code");

            Create.Index("idx_dealers_country_code")
                .OnTable("dealers")
                .OnColumn("country_code");
        }

        public override void Down()
        {
            Delete.Table("dealers");
        }
    }
}
