namespace RuleService.Domain.Events;

/// <summary>
/// Domain event published when a rule is evaluated
/// </summary>
public record RuleEvaluated(
    Guid RuleSetId,
    Guid RuleId,
    bool Passed,
    DateTime EvaluatedAt,
    string? ConfigurationData
);
