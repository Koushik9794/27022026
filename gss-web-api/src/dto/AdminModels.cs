using System;
using System.Collections.Generic;

namespace GssWebApi.Dto;

// --- Roles ---

public record RoleResponse
{
    public Guid Id { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTimeOffset? ModifiedAt { get; init; }
    public int PermissionCount { get; init; }
}

/// <summary>
/// Request DTO for creating a new role.
/// </summary>
/// <param name="RoleName">[REQUIRED] Unique name for the role.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateRoleRequest(string RoleName, string? Description, string? CreatedBy);
/// <summary>
/// Request DTO for updating an existing role.
/// </summary>
/// <param name="RoleName">[REQUIRED] Unique name for the role.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ModifiedBy">[OPTIONAL] User identifier.</param>
public record UpdateRoleRequest(string RoleName, string? Description, string? ModifiedBy);
public record CreateRoleResult(Guid RoleId);

// --- Permissions ---

public record PermissionResponse
{
    public Guid Id { get; init; }
    public string PermissionName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public string? EntityName { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a new permission.
/// </summary>
/// <param name="PermissionName">[REQUIRED] Unique name for the permission.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ModuleName">[REQUIRED] Associated system module name.</param>
/// <param name="EntityName">[OPTIONAL] Associated entity name (if applicable).</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreatePermissionRequest(
    string PermissionName, 
    string? Description, 
    string ModuleName, 
    string? EntityName, 
    string CreatedBy);

/// <summary>
/// Request DTO for updating an existing permission.
/// </summary>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ModuleName">[REQUIRED] Associated system module name.</param>
/// <param name="EntityName">[OPTIONAL] Associated entity name.</param>
/// <param name="ModifiedBy">[OPTIONAL] User identifier.</param>
public record UpdatePermissionRequest(
    string? Description,
    string ModuleName,
    string? EntityName,
    string? ModifiedBy);

// --- Entities ---

public record EntityResponse
{
    public Guid Id { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string SourceTable { get; init; } = string.Empty;
    public string PkColumn { get; init; } = string.Empty;
    public string LabelColumn { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

/// <summary>
/// Request DTO for creating a new metadata entity.
/// </summary>
/// <param name="EntityName">[REQUIRED] Human-readable identifier for the entity.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
/// <param name="SourceTable">[REQUIRED] Underlying database table name.</param>
/// <param name="PkColumn">[REQUIRED] Primary key column name.</param>
/// <param name="LabelColumn">[REQUIRED] Column used for display labels.</param>
public record CreateEntityRequest(
    string EntityName,
    string? Description,
    string CreatedBy,
    string SourceTable,
    string PkColumn,
    string LabelColumn);

// --- Role Permissions ---

/// <summary>
/// Request DTO for assigning a permission to a role.
/// </summary>
/// <param name="RoleId">[REQUIRED] Unique identifier of the role.</param>
/// <param name="PermissionId">[REQUIRED] Unique identifier of the permission.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record AssignPermissionRequest(Guid RoleId, Guid PermissionId, string CreatedBy);

public record UserResponse(Guid Id, string Email, string DisplayName, string Role, string Status, DateTime CreatedAt, DateTime? LastLoginAt);

// --- Dealers ---

public record DealerDto(
    Guid Id,
    string Code,
    string Name,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? CountryCode,
    string? State,
    string? City,
    string? Address,
    bool IsActive,
    Guid CreatedBy,
    Guid? UpdatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Request DTO for creating a new dealer.
/// </summary>
public record CreateDealerRequest(
    string Code,
    string Name,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? CountryCode,
    string? State,
    string? City,
    string? Address,
    Guid CreatedBy);

/// <summary>
/// Request DTO for updating an existing dealer.
/// </summary>
public record UpdateDealerRequest(
    Guid Id,
    string Name,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? CountryCode,
    string? State,
    string? City,
    string? Address,
    bool IsActive,
    Guid UpdatedBy);
