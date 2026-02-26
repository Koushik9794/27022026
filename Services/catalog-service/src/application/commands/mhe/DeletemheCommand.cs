namespace CatalogService.Application.commands.Mhe;

public record DeleteMheCommand(
    Guid Id,
    string? DeletedBy
);
