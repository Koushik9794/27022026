using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogService.Application.Dtos;

namespace CatalogService.Application.dtos;

/// <summary>
/// Response DTO for civil designer initialization data.
/// </summary>
public class CivilDesignerResponseDto
{
    /// <summary>[OPTIONAL] List of warehouse types.</summary>
    [JsonPropertyName("Warehouse_type")]
    public List<WarehouseTypeDto> WarehouseTypes { get; set; } = [];

    /// <summary>[OPTIONAL] List of civil components.</summary>
    [JsonPropertyName("civilComponents")]
    public List<CivilComponentDto> CivilComponents { get; set; } = [];

    /// <summary>[OPTIONAL] List of SKU types.</summary>
    [JsonPropertyName("skus")]
    public List<SkuDto> Skus { get; set; } = [];

    /// <summary>[OPTIONAL] List of pallet types.</summary>
    [JsonPropertyName("pallets")]
    public List<PalletDto> Pallets { get; set; } = [];

    /// <summary>[OPTIONAL] List of Material Handling Equipment.</summary>
    [JsonPropertyName("mhe")]
    public List<MheDto> Mhe { get; set; } = [];
}
