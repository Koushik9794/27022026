namespace CatalogService.Application.commands.attributes;

public record DeleteattributeCommand
(
        Guid Id,
    string? DeletedBy
);
