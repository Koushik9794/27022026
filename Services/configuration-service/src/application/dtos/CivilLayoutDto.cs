using System.Text.Json;
using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Application.Dtos;

public record CivilLayoutDto(
    Guid Id,
    Guid ConfigurationId,
    Guid? WarehouseType,
    string? SourceFile,
    string? CivilJson,
    int VersionNo,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

public sealed class SaveCivilLayoutRequest
{
    public Guid? WarehouseType { get; set; }
    public IFormFile? SourceFile { get; set; }
    public IFormFile? CivilJson { get; set; }

}
public sealed class UpdateCivilLayoutRequest
{
    public Guid? WarehouseType { get; set; }
    public IFormFile? SourceFile { get; set; }
    public IFormFile? CivilJson { get; set; }

}

public record SaveRackLayoutRequest { 
    public IFormFile? RackJson { get; set; } 
    public string? configurationjson { get; set; }
};
public record RackLayoutDto(
    Guid Id,
    Guid CivilLayoutId,
    Guid ConfigurationVersionId,
    string? RackJson,
    JsonDocument? ConfigurationLayout,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);
public sealed record RevisionIdsDto(Guid ConfigurationVersionId, Guid CivilLayoutId);
