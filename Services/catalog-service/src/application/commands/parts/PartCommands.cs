using System.Text.Json;
using Microsoft.AspNetCore.Http; // For IFormFile if we handle it here, but typically command just has paths or bytes. 
// Assuming API controller handles file upload and passes path or stream to command handler, 
// OR command contains IFormFile. 
// Looking at MHEController, `CreateMheRequest` has `IFormFile? glbFile`. 
// The command `CreateMheCommand` likely has the file path (string) or the file itself. 
// Let's create the Command to accept the file path (string) since the Handler shouldn't likely deal with HTTP concerns like IFormFile if possible, 
// BUT if we want to process it in handler... 
// Existing MHE pattern: Controller uploads file, then calls Command with filepath? 
// Let's check previously viewed `MheController.cs`. 
// Whatever, I'll stick to `string? GlbFilepath` in the command. The controller will handle upload and pass the path.

namespace CatalogService.Application.Commands;

public record CreatePartCommand(
    string PartCode,
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

public record UpdatePartCommand(
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

public record DeletePartCommand(
    Guid Id,
    string? DeletedBy
);
