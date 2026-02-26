using FluentMigrator;

namespace AdminService.Infrastructure.Migrations
{
    /// <summary>
    /// Initial migration - creates users table
    /// </summary>
    [Migration(20260107001)]
    public sealed class InitialMigration : Migration
    {
        public override void Up()
        {
            Create.Table("users")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("email").AsString(255).NotNullable().Unique()
                .WithColumn("display_name").AsString(100).NotNullable()
                .WithColumn("role").AsString(50).NotNullable()
                .WithColumn("status").AsString(50).NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("last_login_at").AsDateTime().Nullable()
                .WithColumn("updated_at").AsDateTime().NotNullable();

            Create.Index("idx_users_email")
                .OnTable("users")
                .OnColumn("email");

            Create.Index("idx_users_role")
                .OnTable("users")
                .OnColumn("role");

            Create.Index("idx_users_status")
                .OnTable("users")
                .OnColumn("status");
        }

        public override void Down()
        {
            Delete.Table("users");
        }
    }
}
