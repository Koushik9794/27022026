using System.Text.Json;
using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Application.Commands;

public record SaveCivilLayoutCommand(
   
    Guid ConfigurationId,
    Guid? WarehouseType,
    IFormFile? SourceFile,
IFormFile? CivilJson,

    string? UpdatedBy
);
public record UpdateCivilLayoutCommand(
    Guid Id,
    Guid? WarehouseType,
    IFormFile? SourceFile,
IFormFile? CivilJson,

    string? UpdatedBy

);

public record SaveRackLayoutCommand(
    Guid ConfigurationId,
    int Civilversion,
    int Configversion,
    IFormFile? RackJson,
    JsonDocument? ConfigurationLayout,
    string? UpdatedBy
);
public record UpdateRackLayoutCommand(
    Guid Id,
    IFormFile? RackJson,
    JsonDocument? ConfigurationLayout,
    string? UpdatedBy
);
