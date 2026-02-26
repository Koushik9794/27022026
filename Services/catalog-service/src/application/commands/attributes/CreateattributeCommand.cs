using System.Text.Json;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.commands.attributes;

public sealed record CreateattributeCommand
(string AttributeKey,
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
    string? CreatedBy);
