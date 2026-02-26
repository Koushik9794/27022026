namespace CatalogService.Application.Commands.Pallets;

/// <summary>
/// Command to create a new pallet type.
/// </summary>
public record CreatePalletCommand(
    string Code,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
   IFormFile? GlbFile,
    bool IsActive,
    string? CreatedBy
);

/// <summary>
/// Command to update an existing pallet type.
/// </summary>
public record UpdatePalletCommand(
    Guid Id,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
    IFormFile? GlbFile,
    bool IsActive,
    string? UpdatedBy
);

/// <summary>
/// Command to delete a pallet type (soft delete).
/// </summary>
public record DeletePalletCommand(
    Guid Id,
    string? DeletedBy
);
