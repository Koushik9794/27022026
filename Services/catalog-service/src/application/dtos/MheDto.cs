namespace CatalogService.Application.dtos;

using System.Text.Json;
/// <summary>
/// Data transfer object for Material Handling Equipment (MHE).
/// </summary>
/// <param name="Id">Unique identifier of the MHE.</param>
/// <param name="Code">Unique MHE code.</param>
/// <param name="Name">Display name of the MHE.</param>
/// <param name="Manufacturer">Manufacturer name.</param>
/// <param name="Brand">Brand name.</param>
/// <param name="Model">Model identifier.</param>
/// <param name="MheType">Type of MHE (e.g., 'Forklift').</param>
/// <param name="MheCategory">Category of MHE.</param>
/// <param name="GlbFilePath">Path to the 3D model file.</param>
/// <param name="Attributes">Dynamic attributes associated with the MHE.</param>
/// <param name="IsActive">Whether the MHE is active.</param>
/// <param name="IsDeleted">Whether the MHE is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
public record MheDto(
     Guid Id,
     string Code,
     string Name,

     string? Manufacturer,
     string? Brand,
     string? Model,
     string? MheType,
     string? MheCategory,

     string? GlbFilePath,
     Dictionary<string, JsonElement> Attributes,

 bool IsActive,
     bool IsDeleted,

     DateTimeOffset CreatedAt,
     string? CreatedBy,

     DateTimeOffset? UpdatedAt,
     string? UpdatedBy
    );

