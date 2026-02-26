namespace RuleService.Domain.Events;

/// <summary>
/// Domain event published when a rule is activated
/// </summary>
public record RuleActivated(
    Guid RuleSetId,
    Guid RuleId,
    DateTime ActivatedAt,
    string ActivatedBy
);
