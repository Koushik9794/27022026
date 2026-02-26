namespace CatalogService.Application.Commands;

public record CreateComponentGroupCommand(
    string Code,
    string Name,
    string? Description,
    int SortOrder
);

public record UpdateComponentGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    int SortOrder
);

public record DeleteComponentGroupCommand(
    Guid Id
);
