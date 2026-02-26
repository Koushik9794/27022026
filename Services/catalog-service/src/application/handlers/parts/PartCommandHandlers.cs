using System.Text.Json;
using CatalogService.Application.Commands;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using Wolverine;

namespace CatalogService.Application.Handlers.Parts;

public class PartCommandHandlers
{
    private readonly IPartRepository _partRepository;
    private readonly IComponentGroupRepository _groupRepository;
    private readonly IComponentNameRepository _nameRepository;
    private readonly GssCommon.Export.IFileServiceClient _fileServiceClient;

    public PartCommandHandlers(
        IPartRepository partRepository,
        IComponentGroupRepository groupRepository,
        IComponentNameRepository nameRepository,
        GssCommon.Export.IFileServiceClient fileServiceClient)
    {
        _partRepository = partRepository;
        _groupRepository = groupRepository;
        _nameRepository = nameRepository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<Guid>> Handle(CreatePartCommand command)
    {
        // 1. Check if PartCode + CountryCode already exists
        if (await _partRepository.ExistsByCodeAndCountryAsync(command.PartCode, command.CountryCode))
        {
            return Result.Failure<Guid>(PartErrors.DuplicateCode);
        }

        // 2. Validate Foreign Keys
        var group = await _groupRepository.GetByIdAsync(command.ComponentGroupId);
        if (group == null)
        {
            return Result.Failure<Guid>(ComponentGroupErrors.NotFound);
        }

        if (command.ComponentNameId.HasValue)
        {
            var name = await _nameRepository.GetByIdAsync(command.ComponentNameId.Value);
            if (name == null)
            {
                return Result.Failure<Guid>(ComponentNameErrors.NotFound);
            }
        }

        // 3. Upload GLB File if present
        string? glbFilepath = null;
        if (command.GlbFile != null)
        {
            await using var stream = command.GlbFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.GlbFile.FileName,
                command.GlbFile.ContentType ?? "application/octet-stream",
                CancellationToken.None 
            );
            glbFilepath = uploadResult.FilePath;
        }

        // 3.1 Upload Image File if present
        string? imageUrl = null;
        if (command.ImageFile != null)
        {
            await using var stream = command.ImageFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.ImageFile.FileName,
                command.ImageFile.ContentType ?? "image/png", // Default or detect
                CancellationToken.None 
            );
            imageUrl = uploadResult.FilePath; // Assuming FileService returns path/url in generic way
        }

        // 4. Parse Attributes
        Dictionary<string, JsonElement>? attributes = null;
        if (!string.IsNullOrWhiteSpace(command.Attributes))
        {
            try
            {
                attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(command.Attributes);
            }
            catch (JsonException)
            {
                return Result.Failure<Guid>(Error.Validation("Part.InvalidAttributes", "Invalid Attributes JSON format."));
            }
        }

        // 5. Create Aggregate
        try
        {
            var part = Part.Create(
                command.PartCode,
                command.CountryCode,
                command.ComponentGroupId,
                command.ComponentTypeId,
                command.UnitBasicPrice,
                command.UnspscCode,
                command.ComponentNameId,
                command.Colour,
                command.PowderCode,
                command.GfaFlag,
                command.Cbm,
                command.ShortDescription,
                command.Description,
                command.DrawingNo,
                command.RevNo,
                command.InstallationRefNo,
                attributes,
                glbFilepath,
                imageUrl,
                "ACTIVE", // Default status
                command.CreatedBy
            );

            await _partRepository.CreateAsync(part);
            return Result.Success(part.Id);
        }
        
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("Part.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdatePartCommand command)
    {
        var part = await _partRepository.GetByIdAsync(command.Id);
        if (part == null)
        {
            return Result.Failure<bool>(PartErrors.NotFound);
        }

        // Validate Foreign Keys
        var group = await _groupRepository.GetByIdAsync(command.ComponentGroupId);
        if (group == null)
        {
            return Result.Failure<bool>(ComponentGroupErrors.NotFound);
        }

        if (command.ComponentNameId.HasValue)
        {
            var name = await _nameRepository.GetByIdAsync(command.ComponentNameId.Value);
            if (name == null)
            {
                return Result.Failure<bool>(ComponentNameErrors.NotFound);
            }
        }

        // Parse Attributes
        Dictionary<string, JsonElement>? attributes = null;
        if (!string.IsNullOrWhiteSpace(command.Attributes))
        {
            try
            {
                attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(command.Attributes);
            }
            catch (JsonException)
            {
                return Result.Failure<bool>(Error.Validation("Part.InvalidAttributes", "Invalid Attributes JSON format."));
            }
        }

        // Upload NEW GLB File if present
        string? glbFilepath = null;
        if (command.GlbFile != null)
        {
            await using var stream = command.GlbFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.GlbFile.FileName,
                command.GlbFile.ContentType ?? "application/octet-stream",
                CancellationToken.None
            );
            glbFilepath = uploadResult.FilePath;
        }

        // Upload NEW Image File if present
        string? imageUrl = null;
        if (command.ImageFile != null)
        {
            await using var stream = command.ImageFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.ImageFile.FileName,
                command.ImageFile.ContentType ?? "image/png",
                CancellationToken.None
            );
            imageUrl = uploadResult.FilePath;
        }

        try
        {
            part.Update(
                command.UnspscCode,
                command.ComponentGroupId,
                command.ComponentTypeId,
                command.ComponentNameId,
                command.Colour,
                command.PowderCode,
                command.GfaFlag,
                command.UnitBasicPrice,
                command.Cbm,
                command.ShortDescription,
                command.Description,
                command.DrawingNo,
                command.RevNo,
                command.InstallationRefNo,
                attributes,
                glbFilepath, // Will be null if no new file, Part.Update handles null
                imageUrl,
                command.Status,
                command.UpdatedBy
            );

            await _partRepository.UpdateAsync(part);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("Part.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeletePartCommand command)
    {
        var part = await _partRepository.GetByIdAsync(command.Id);
        if (part == null)
        {
            return Result.Failure<bool>(PartErrors.NotFound);
        }

        part.Delete(command.DeletedBy);
        await _partRepository.UpdateAsync(part); // Update because we do soft delete

        return Result.Success(true);
    }
}
