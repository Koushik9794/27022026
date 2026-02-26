using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for ComponentMaster details.
/// </summary>
public record ComponentMasterDto(
    Guid Id,
    string ComponentMasterCode,
    string CountryCode,
    string? UnspscCode,
    Guid ComponentGroupId,
    string? ComponentGroupCode,
    string? ComponentGroupName,
    Guid ComponentTypeId,
    string? ComponentTypeCode,
    string? ComponentTypeName,
    Guid? ComponentNameId,
    string? ComponentNameCode,
    string? ComponentNameName,
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
    Dictionary<string, JsonElement>? Attributes,
    string? GlbFilepath,
    string? ImageUrl,
    string Status,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);

/// <summary>
/// Data transfer object for creating a new ComponentMaster.
/// </summary>
public record CreateComponentMasterRequest(
    [Required] [MaxLength(100)] string ComponentMasterCode,
    
    [Required] [StringLength(2, MinimumLength = 2)] string CountryCode,
    
    [MaxLength(50)] string? UnspscCode,
    
    [Required] Guid ComponentGroupId,
    
    [Required] Guid ComponentTypeId,
    
    Guid? ComponentNameId,
    
    [MaxLength(50)] string? Colour,
    
    [MaxLength(50)] string? PowderCode,
    
    bool GfaFlag,
    
    [Required] [Range(0, double.MaxValue)] decimal UnitBasicPrice,
    
    [Range(0, double.MaxValue)] decimal? Cbm,
    
    [MaxLength(255)] string? ShortDescription,
    
    [MaxLength(1000)] string? Description,
    
    [MaxLength(100)] string? DrawingNo,
    
    [MaxLength(50)] string? RevNo,
    
    [MaxLength(100)] string? InstallationRefNo,
    
    string? Attributes, 
    
    IFormFile? GlbFile,

    IFormFile? ImageFile,
    
    string? CreatedBy
);

/// <summary>
/// Data transfer object for updating an existing ComponentMaster.
/// </summary>
public record UpdateComponentMasterRequest(
    [MaxLength(50)] string? UnspscCode,
    
    [Required] Guid ComponentGroupId,
    
    [Required] Guid ComponentTypeId,
    
    Guid? ComponentNameId,
    
    [MaxLength(50)] string? Colour,
    
    [MaxLength(50)] string? PowderCode,
    
    bool GfaFlag,
    
    [Required] [Range(0, double.MaxValue)] decimal UnitBasicPrice,
    
    [Range(0, double.MaxValue)] decimal? Cbm,
    
    [MaxLength(255)] string? ShortDescription,
    
    [MaxLength(1000)] string? Description,
    
    [MaxLength(100)] string? DrawingNo,
    
    [MaxLength(50)] string? RevNo,
    
    [MaxLength(100)] string? InstallationRefNo,
    
    string? Attributes, 
    
    IFormFile? GlbFile,

    IFormFile? ImageFile,
    
    [Required] string Status,
    
    string? UpdatedBy
);
