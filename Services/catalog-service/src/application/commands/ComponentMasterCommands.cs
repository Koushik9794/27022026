using Microsoft.AspNetCore.Http;

namespace CatalogService.Application.Commands;

public record CreateComponentMasterCommand(
    string ComponentMasterCode,
    string CountryCode,
    string? UnspscCode,
    Guid ComponentGroupId,
    Guid ComponentTypeId,
    Guid? ComponentNameId,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    string? Attributes, // JSON string from request
    IFormFile? GlbFile,
    IFormFile? ImageFile,
    string? CreatedBy
);

public record UpdateComponentMasterCommand(
    Guid Id,
    string? UnspscCode,
    Guid ComponentGroupId,
    Guid ComponentTypeId,
    Guid? ComponentNameId,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    string? Attributes, // JSON string
    IFormFile? GlbFile,
    IFormFile? ImageFile,
    string Status,
    string? UpdatedBy
);

public record DeleteComponentMasterCommand(
    Guid Id,
    string? DeletedBy
);
