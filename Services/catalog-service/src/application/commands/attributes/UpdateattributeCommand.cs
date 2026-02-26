using System.Text.Json;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.commands.attributes;

public sealed record UpdateattributeCommand
(
    Guid Id,
    string AttributeKey,
    string DisplayName,
    AttributeDataType DataType,
    string? Unit,
    decimal? MinValue,
    decimal? MaxValue,
    JsonElement? DefaultValue,
    bool IsRequired,
     JsonElement? AllowedValues,
    string? Description,
    AttributeScreen Screen,
    bool IsActive,
   string? UpdatedBy
    );
