using System.Text.Json;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.dtos;

public sealed record AttributeDefinitionDto
(
    Guid Id,
    string AttributeKey,
    string DisplayName,
    string? Unit,
    AttributeDataType DataType,
    decimal? MinValue,
    decimal? MaxValue,
    JsonElement? DefaultValue,
    bool IsRequired,
    JsonElement? AllowedValues,
    string? Description,

    AttributeScreen? Screen,
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy
);

