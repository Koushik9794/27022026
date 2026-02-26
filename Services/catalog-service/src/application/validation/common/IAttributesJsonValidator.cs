using CatalogService.Application.dtos;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.Validation.Common;





public interface IAttributesJsonValidator
{
    Task<IReadOnlyList<(string Path, string Message)>> ValidateAsync(
        string screenKey,
        object? attributesPayload,
        CancellationToken ct);
}




