using System.ComponentModel.DataAnnotations;
namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for Pallet type.
/// </summary>
public record PalletDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
    string? GlbFilePath,
    bool IsActive,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

/// <summary>
/// Request to create a new pallet type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON string of attribute schema.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
public record CreatePalletRequest(
    [Required] string Code,
    [Required] string Name,
    string? Description,
    string? AttributeSchema,
    IFormFile? GlbFile = null,
    [Required] bool IsActive = true
);

/// <summary>
/// Request to update an existing pallet type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON string of attribute schema.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
public record UpdatePalletRequest(
    [Required] string Name,
    string? Description,
    string? AttributeSchema,
     IFormFile? GlbFile,
    [Required] bool IsActive
);
