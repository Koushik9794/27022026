namespace CatalogService.application.dtos;

public sealed class LoadChartCandidateDto
{

    public string component_type { get; init; } = default!;
    public string profile_text { get; init; } = default!;
    public string Bracing_Type { get; init; } = default!; // D / X / D+S / X+S / B
    public double chart_capacity { get; init; } = default!;
    public double req_load { get; init; } = default!;
    public double utilization_pct { get; init; } = default!;

}
