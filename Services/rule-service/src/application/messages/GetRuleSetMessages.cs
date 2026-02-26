namespace RuleService.Application.Messages;

/// <summary>
/// Query to get a RuleSet by ID
/// </summary>
public record GetRuleSet(Guid RuleSetId);

public record RuleSetResponse(
    Guid Id,
    string Name,
    Guid ProductGroupId,
    Guid CountryId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string Status,
    List<RuleInfo> Rules
);
