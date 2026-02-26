using CatalogService.Application.dtos;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Validation.Common;


public sealed class AttributeDefinitionProviderAdapter : IAttributeDefinitionProvider
{
    private readonly IAttributeDefinitionRepository _repo;

    public AttributeDefinitionProviderAdapter(IAttributeDefinitionRepository repo)
        => _repo = repo;

    public async Task<IReadOnlyList<AttributeDefinitionDto>> GetByScreenKeyAsync(
        string screenKey,
        CancellationToken ct)
    {
        if (!Enum.TryParse<AttributeScreen>(screenKey, ignoreCase: true, out var screen))
            throw new ArgumentException($"Invalid screen key: '{screenKey}'", nameof(screenKey));

        List<AttributeDefinition> defs = await _repo.GetByScreenAsync(screen, ct);

        return defs.Select(MapToDto).ToList();
    }

    private static AttributeDefinitionDto MapToDto(AttributeDefinition d)
    {
        // âœ… If d.AllowedValues is already JsonElement?, use directly.
        // âœ… If it's string JSON, parse it (see ParseAllowedValues below).

        return new AttributeDefinitionDto(
            Id:d.Id,
            AttributeKey: d.AttributeKey,
            DisplayName: d.DisplayName,
            Unit: d.Unit,
            DataType: d.DataType,
            MinValue: d.MinValue,
            MaxValue: d.MaxValue,
            DefaultValue: d.DefaultValue,
            IsRequired: d.IsRequired,
            AllowedValues: d.AllowedValues,
            Description:d.Description,
            Screen: d.Screen,
            IsActive: d.IsActive,
            IsDeleted: d.IsDeleted,
            CreatedAt: d.CreatedAt,
            CreatedBy: d.CreatedBy,
            UpdatedAt: d.UpdatedAt,
            UpdatedBy: d.UpdatedBy

        );
    }

    // If AllowedValues is string? in your entity, use this instead:
    /*
    private static JsonElement? ParseAllowedValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static AttributeDefinitionDto MapToDto(AttributeDefinition d)
        => new(
            d.AttributeKey,
            d.DataType,
            d.MinValue,
            d.MaxValue,
            ParseAllowedValues(d.AllowedValues),
            d.IsRequired
        );
    */
}
