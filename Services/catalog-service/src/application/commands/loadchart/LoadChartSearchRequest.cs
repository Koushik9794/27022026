using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CatalogService.application.commands.loadchart;



public sealed record LoadChartSearchCommand : IValidatableObject
{
    [JsonPropertyName("prodctgroup")]
    [Required]
    public string? prodctgroup { get; init; } // ignored for now

    [JsonPropertyName("chart_type")]
    [Required]
    public List<ChartType> ChartTypes { get; init; } = [];

    [JsonPropertyName("levels")]
    public int? Levels { get; init; } // ignored for now

    [JsonPropertyName("beamSpan")]
    public int? BeamSpan { get; init; } // used only when BEAM requested

    [JsonPropertyName("IsStiffenerenable")]
    public bool IsStiffenerEnable { get; init; }

    [JsonPropertyName("levelConfigs")]
    public Dictionary<string, LevelConfig>? LevelConfigs { get; init; } // ignored for now

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChartTypes.Count == 0)
            yield return new ValidationResult("chart_type must contain at least one value.", new[] { nameof(ChartTypes) });

        if (ChartTypes.Contains(ChartType.BEAM) && (BeamSpan is null or <= 0))
            yield return new ValidationResult("beamSpan is required when chart_type contains BEAM.", new[] { nameof(BeamSpan) });
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChartType { UPRIGHT, BEAM }

public sealed class LevelConfig
{
    [JsonPropertyName("USL")]
    public int USL { get; init; }

    [JsonPropertyName("Capacity")]
    public decimal Capacity { get; init; }
}
