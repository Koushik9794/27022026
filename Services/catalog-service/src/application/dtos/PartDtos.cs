using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for Part details.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="PartCode">Unique part code.</param>
/// <param name="CountryCode">Country code.</param>
/// <param name="UnspscCode">UNSPSC code.</param>
/// <param name="ComponentGroupId">Component Group ID.</param>
/// <param name="ComponentGroupCode">Component Group Code.</param>
/// <param name="ComponentGroupName">Component Group Name.</param>
/// <param name="ComponentTypeId">Component Type ID.</param>
/// <param name="ComponentTypeCode">Component Type Code.</param>
/// <param name="ComponentTypeName">Component Type Name.</param>
/// <param name="ComponentNameId">Component Name ID.</param>
/// <param name="ComponentNameCode">Component Name Code.</param>
/// <param name="ComponentNameName">Component Name Name.</param>
/// <param name="Colour">Colour.</param>
/// <param name="PowderCode">Powder Code.</param>
/// <param name="GfaFlag">GFA Flag.</param>
/// <param name="UnitBasicPrice">Unit Basic Price.</param>
/// <param name="Cbm">CBM.</param>
/// <param name="ShortDescription">Short Description.</param>
/// <param name="Description">Description.</param>
/// <param name="DrawingNo">Drawing No.</param>
/// <param name="RevNo">Revision No.</param>
/// <param name="InstallationRefNo">Installation Ref No.</param>
/// <param name="Attributes">Dynamic Attributes.</param>
/// <param name="GlbFilepath">GLB File Path.</param>
/// <param name="ImageUrl">Image URL.</param>
/// <param name="Status">Status.</param>
/// <param name="IsDeleted">Deletion Status.</param>
/// <param name="CreatedAt">Created Date.</param>
/// <param name="UpdatedAt">Updated Date.</param>
/// <param name="CreatedBy">Created User.</param>
/// <param name="UpdatedBy">Updated User.</param>
public record PartDto(
    Guid Id,
    string PartCode,
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
/// Data transfer object for creating a new Part.
/// </summary>
/// <param name="PartCode">[REQUIRED] Unique code for the part (max 100 chars).</param>
/// <param name="CountryCode">[REQUIRED] 2-character country code (ISO-2).</param>
/// <param name="UnspscCode">[OPTIONAL] Global standard code for products and services.</param>
/// <param name="ComponentGroupId">[REQUIRED] ID of the associated Component Group.</param>
/// <param name="ComponentTypeId">[REQUIRED] ID of the associated Component Type.</param>
/// <param name="ComponentNameId">[OPTIONAL] ID of the specific Component Name.</param>
/// <param name="Colour">[OPTIONAL] Part color name or code.</param>
/// <param name="PowderCode">[OPTIONAL] Powder coating code.</param>
/// <param name="GfaFlag">[OPTIONAL] Flag for General For All compatibility.</param>
/// <param name="UnitBasicPrice">[REQUIRED] Base price per unit.</param>
/// <param name="Cbm">[OPTIONAL] Volume in Cubic Meters.</param>
/// <param name="ShortDescription">[OPTIONAL] Brief textual summary.</param>
/// <param name="Description">[OPTIONAL] Detailed technical description.</param>
/// <param name="DrawingNo">[OPTIONAL] Reference drawing number.</param>
/// <param name="RevNo">[OPTIONAL] Revision number.</param>
/// <param name="InstallationRefNo">[OPTIONAL] Installation reference number.</param>
/// <param name="Attributes">[OPTIONAL] JSON string containing dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] GLB 3D model file.</param>
/// <param name="ImageFile">[OPTIONAL] Image file.</param>
/// <param name="CreatedBy">[OPTIONAL] Identifier of the user creating the part.</param>
public record CreatePartRequest(
    [Required] [MaxLength(100)] string PartCode,
    
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
/// Data transfer object for updating an existing Part.
/// </summary>
/// <param name="UnspscCode">[OPTIONAL] Global standard code for products and services.</param>
/// <param name="ComponentGroupId">[REQUIRED] ID of the associated Component Group.</param>
/// <param name="ComponentTypeId">[REQUIRED] ID of the associated Component Type.</param>
/// <param name="ComponentNameId">[OPTIONAL] ID of the specific Component Name.</param>
/// <param name="Colour">[OPTIONAL] Part color name or code.</param>
/// <param name="PowderCode">[OPTIONAL] Powder coating code.</param>
/// <param name="GfaFlag">[OPTIONAL] Flag for General For All compatibility.</param>
/// <param name="UnitBasicPrice">[REQUIRED] Base price per unit.</param>
/// <param name="Cbm">[OPTIONAL] Volume in Cubic Meters.</param>
/// <param name="ShortDescription">[OPTIONAL] Brief textual summary.</param>
/// <param name="Description">[OPTIONAL] Detailed technical description.</param>
/// <param name="DrawingNo">[OPTIONAL] Reference drawing number.</param>
/// <param name="RevNo">[OPTIONAL] Revision number.</param>
/// <param name="InstallationRefNo">[OPTIONAL] Installation reference number.</param>
/// <param name="Attributes">[OPTIONAL] JSON string containing dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] NEW GLB 3D model file (if provided, replaces existing).</param>
/// <param name="ImageFile">[OPTIONAL] NEW Image file (if provided, replaces existing).</param>
/// <param name="Status">[REQUIRED] Current status of the part (e.g., 'ACTIVE', 'INACTIVE').</param>
/// <param name="UpdatedBy">[OPTIONAL] Identifier of the user updating the part.</param>
public record UpdatePartRequest(
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
