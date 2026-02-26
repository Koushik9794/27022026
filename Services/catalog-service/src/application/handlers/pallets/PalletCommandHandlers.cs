using CatalogService.Application.Errors;
using CatalogService.Application.Commands.Pallets;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using GssCommon.Export;

/// <summary>
/// Handles pallet type command operations.
/// </summary>
public class CreatePalletCommandHandler(IPalletRepository repository, IFileServiceClient fileServiceClient)
{

    private readonly IPalletRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;


    public async Task<Result<Guid>> Handle(CreatePalletCommand command)
    {
        try

        {
            // Check for duplicate code
            if (await _repository.ExistsAsync(command.Code))
            {
                return Result.Failure<Guid>(PalletErrors.CodeExists(command.Code));

            }
            string? glbFilePath = null;
            if (command.GlbFile is not null)
            {
                await using var stream = command.GlbFile.OpenReadStream();
                var uploadResult = await _fileServiceClient.UploadAsync(
                    stream,
                    command.GlbFile.FileName,
                    command.GlbFile.ContentType ?? "application/octet-stream"
                );
                glbFilePath = uploadResult.FilePath;
            }
            var pallet = Pallet.Create(
                command.Code,
                command.Name,
                command.Description,
                glbFilePath,
                command.AttributeSchema,

                command.IsActive,
                command.CreatedBy
            );

            await _repository.CreateAsync(pallet);
            return Result<Guid>.Success(pallet.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(PalletErrors.CreateFailed(ex.Message));
        }

    }
}



public class UpdatepalletCommandHandler(IPalletRepository repository, IFileServiceClient fileServiceClient)
{
    private readonly IPalletRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;

    public async Task<Result<Guid>> Handle(UpdatePalletCommand command)
    {
        try
        {
            var pallet = await _repository.GetByIdAsync(command.Id);
            if (pallet == null)
            {
                return Result.Failure<Guid>(PalletErrors.NotFound(command.Id));
            }

            string? glbFilePath = null;
            if (command.GlbFile is not null)
            {
                await using var stream = command.GlbFile.OpenReadStream();
                var uploadResult = await _fileServiceClient.UploadAsync(
                    stream,
                    command.GlbFile.FileName,
                    command.GlbFile.ContentType ?? "application/octet-stream"
                );
                glbFilePath = uploadResult.FilePath;
            }
            pallet.Update(
                command.Name,
                command.Description,
                command.AttributeSchema,
                glbFilePath,
                command.IsActive,
                command.UpdatedBy
            );

            await _repository.UpdateAsync(pallet);
            return Result<Guid>.Success(pallet.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(PalletErrors.UpdateFailed(ex.Message));
        }

    }
}

public class DeletePalletCommandHandler(IPalletRepository repository)
{
    private readonly IPalletRepository _repository = repository;


    public async Task<Result<bool>> Handle(DeletePalletCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate empty Guid
        if (command.Id == Guid.Empty)
        {
            return Result.Failure<bool>(PalletErrors.InvalidId());
        }

        // 2. Check existence
        var pallet = await _repository.GetByIdAsync(command.Id);
        if (pallet == null)
        {
            return Result.Failure<bool>(PalletErrors.NotFound(command.Id));
        }

        // 3. Repository handles delete + persistence
        return await _repository.DeleteAsync(command.Id, command.DeletedBy);
    }
}
