using System.Text.Json;
using CatalogService.Application.Commands;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.ComponentMaster;

public class ComponentMasterCommandHandlers
{
    private readonly IComponentMasterRepository _cmRepository;
    private readonly IComponentGroupRepository _groupRepository;
    private readonly IComponentNameRepository _nameRepository;
    private readonly IComponentTypeRepository _typeRepository;
    private readonly GssCommon.Export.IFileServiceClient _fileServiceClient;

    public ComponentMasterCommandHandlers(
        IComponentMasterRepository cmRepository,
        IComponentGroupRepository groupRepository,
        IComponentNameRepository nameRepository,
        IComponentTypeRepository typeRepository,
        GssCommon.Export.IFileServiceClient fileServiceClient)
    {
        _cmRepository = cmRepository;
        _groupRepository = groupRepository;
        _nameRepository = nameRepository;
        _typeRepository = typeRepository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<Guid>> Handle(CreateComponentMasterCommand command)
    {
        // 1. Check if ComponentMasterCode + CountryCode already exists
        if (await _cmRepository.ExistsByCodeAndCountryAsync(command.ComponentMasterCode, command.CountryCode))
        {
            return Result.Failure<Guid>(ComponentMasterErrors.DuplicateCode);
        }

        // 2. Validate Foreign Keys
        var group = await _groupRepository.GetByIdAsync(command.ComponentGroupId);
        if (group == null)
        {
            return Result.Failure<Guid>(ComponentGroupErrors.NotFound);
        }

        var type = await _typeRepository.GetByIdAsync(command.ComponentTypeId);
        if (type == null)
        {
            return Result.Failure<Guid>(ComponentTypeErrors.NotFound);
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
                command.ImageFile.ContentType ?? "image/png",
                CancellationToken.None 
            );
            imageUrl = uploadResult.FilePath;
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
                return Result.Failure<Guid>(Error.Validation("ComponentMaster.InvalidAttributes", "Invalid Attributes JSON format."));
            }
        }

        // 5. Create Aggregate
        try
        {
            var cm = CatalogService.Domain.Aggregates.ComponentMaster.Create(
                command.ComponentMasterCode,
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
                "ACTIVE",
                command.CreatedBy
            );

            await _cmRepository.CreateAsync(cm);
            return Result.Success(cm.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("ComponentMaster.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdateComponentMasterCommand command)
    {
        var cm = await _cmRepository.GetByIdAsync(command.Id);
        if (cm == null)
        {
            return Result.Failure<bool>(ComponentMasterErrors.NotFound);
        }

        // Validate Foreign Keys
        var group = await _groupRepository.GetByIdAsync(command.ComponentGroupId);
        if (group == null)
        {
            return Result.Failure<bool>(ComponentGroupErrors.NotFound);
        }

        var type = await _typeRepository.GetByIdAsync(command.ComponentTypeId);
        if (type == null)
        {
            return Result.Failure<bool>(ComponentTypeErrors.NotFound);
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
                return Result.Failure<bool>(Error.Validation("ComponentMaster.InvalidAttributes", "Invalid Attributes JSON format."));
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
            cm.Update(
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
                glbFilepath,
                imageUrl,
                command.Status,
                command.UpdatedBy
            );

            await _cmRepository.UpdateAsync(cm);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("ComponentMaster.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeleteComponentMasterCommand command)
    {
        var cm = await _cmRepository.GetByIdAsync(command.Id);
        if (cm == null)
        {
            return Result.Failure<bool>(ComponentMasterErrors.NotFound);
        }

        cm.Delete(command.DeletedBy);
        await _cmRepository.UpdateAsync(cm);

        return Result.Success(true);
    }
}
