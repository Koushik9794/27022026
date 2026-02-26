using FluentMigrator;

namespace AdminService.Infrastructure.Migrations
{
    [Migration(20260111001)]
    public class RoleTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("app_roles")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("role_name").AsString(100).NotNullable()
                .WithColumn("description").AsString(500).Nullable()
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_by").AsString(100).Nullable()
                .WithColumn("created_at").AsCustom("TIMESTAMP WITH TIME ZONE").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modified_by").AsString(100).Nullable()
                .WithColumn("modified_at").AsCustom("TIMESTAMP WITH TIME ZONE").Nullable();

            // Partial unique index: uniqueness on role_name only when is_deleted=false
            Execute.Sql("CREATE UNIQUE INDEX IX_app_roles_role_name_not_deleted ON app_roles (role_name) WHERE (is_deleted = false);");
        }

        public override void Down()
        {
            Delete.Table("app_roles");
        }
    }
}
