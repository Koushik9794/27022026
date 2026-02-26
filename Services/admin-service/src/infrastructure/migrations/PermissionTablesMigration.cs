using FluentMigrator;

namespace AdminService.Infrastructure.Migrations
{
    [Migration(20260112001)]
    public class PermissionTablesMigration : Migration
    {
        public override void Up()
        {
            // 1. Entities table
            Create.Table("app_entities")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("entity_name").AsString(100).NotNullable()
                .WithColumn("description").AsString(500).Nullable()
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_by").AsString(100).Nullable()
                .WithColumn("created_at").AsCustom("TIMESTAMP WITH TIME ZONE").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modified_by").AsString(100).Nullable()
                .WithColumn("modified_at").AsCustom("TIMESTAMP WITH TIME ZONE").Nullable()
                .WithColumn("source_table").AsString(100).NotNullable()
                .WithColumn("pk_column").AsString(100).NotNullable()
                .WithColumn("label_column").AsString(100).NotNullable();

            Create.Index("IX_app_entities_name_not_deleted").OnTable("app_entities").OnColumn("entity_name");

            // 2. Permissions table
            Create.Table("app_permissions")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("permission_name").AsString(100).NotNullable()
                .WithColumn("description").AsString(500).Nullable()
                .WithColumn("module_name").AsString(100).NotNullable()
                .WithColumn("entity_name").AsString(100).Nullable()
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("created_by").AsString(100).Nullable()
                .WithColumn("created_at").AsCustom("TIMESTAMP WITH TIME ZONE").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modified_by").AsString(100).Nullable()
                .WithColumn("modified_at").AsCustom("TIMESTAMP WITH TIME ZONE").Nullable();

            Create.Index("IX_app_permissions_name_not_deleted").OnTable("app_permissions").OnColumn("permission_name");

            // 3. Role-Permission junction table
            Create.Table("app_role_permission")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("role_id").AsGuid().NotNullable().ForeignKey("app_roles", "id")
                .WithColumn("permission_id").AsGuid().NotNullable().ForeignKey("app_permissions", "id")
                .WithColumn("created_by").AsString(100).Nullable()
                .WithColumn("created_at").AsCustom("TIMESTAMP WITH TIME ZONE").NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.UniqueConstraint("UC_app_role_permission_role_id_permission_id")
                .OnTable("app_role_permission")
                .Columns("role_id", "permission_id");
        }

        public override void Down()
        {
            Delete.Table("app_role_permission");
            Delete.Table("app_permissions");
            Delete.Table("app_entities");
        }
    }
}
