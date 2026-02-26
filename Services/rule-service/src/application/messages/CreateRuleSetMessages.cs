namespace RuleService.Application.Messages;

/// <summary>
/// Command to create a new RuleSet
/// </summary>
public record CreateRuleSet(
    string Name,
    Guid ProductGroupId,
    Guid CountryId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string RulesJson
);

public record CreateRuleSetResponse(
    Guid RuleSetId,
    string Status
);
