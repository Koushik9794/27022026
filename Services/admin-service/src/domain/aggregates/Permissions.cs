using AdminService.Domain.ValueObjects;

namespace AdminService.Domain.Aggregates;

public sealed class Permission
{
    public Guid Id { get; }
    public PermissionName PermissionName { get; private set; }
    public string? Description { get; private set; }
    public ModuleName ModuleName { get; private set; }
    public EntityName? EntityName { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; }
    public DateTime CreatedAt { get; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    private Permission(Guid id, PermissionName name, string? description, ModuleName moduleName,
        EntityName? entityName, bool isActive, bool isDeleted, string createdBy, DateTime createdAt,
        string? modifiedBy, DateTime? modifiedAt)
    {
        Id = id;
        PermissionName = name;
        Description = description;
        ModuleName = moduleName;
        EntityName = entityName;
        IsActive = isActive;
        IsDeleted = isDeleted;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        ModifiedBy = modifiedBy;
        ModifiedAt = modifiedAt;
    }

    public static Permission Create(PermissionName name, string? description, ModuleName moduleName,
        EntityName? entityName, string createdBy, DateTime nowUtc)
        => new(Guid.NewGuid(), name, description, moduleName, entityName, true, false, createdBy, nowUtc, null, null);

    // ✅ Rehydrate for persistence mapping
    public static Permission Rehydrate(Guid id, PermissionName name, string? description, ModuleName moduleName,
        EntityName? entityName, bool isActive, bool isDeleted, string createdBy, DateTime createdAt,
        string? modifiedBy, DateTime? modifiedAt)
        => new(id, name, description, moduleName, entityName, isActive, isDeleted, createdBy, createdAt, modifiedBy, modifiedAt);

    public void Update(string? description, ModuleName moduleName, EntityName? entityName, string modifiedBy, DateTime nowUtc)
    {
        Description = description;
        ModuleName = moduleName;
        EntityName = entityName;
        ModifiedBy = modifiedBy;
        ModifiedAt = nowUtc;
    }

    public void Activate(string modifiedBy, DateTime nowUtc) { IsActive = true; ModifiedBy = modifiedBy; ModifiedAt = nowUtc; }
    public void Deactivate(string modifiedBy, DateTime nowUtc) { IsActive = false; ModifiedBy = modifiedBy; ModifiedAt = nowUtc; }
    public void SoftDelete(string modifiedBy, DateTime nowUtc) { IsDeleted = true; IsActive = false; ModifiedBy = modifiedBy; ModifiedAt = nowUtc; }
}
