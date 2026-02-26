namespace CatalogService.Application.Commands;

public record CreateComponentNameCommand(
    string Code,
    string Name,
    string? Description,
    Guid ComponentTypeId
);

public record UpdateComponentNameCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid ComponentTypeId
);

public record DeleteComponentNameCommand(
    Guid Id
);
