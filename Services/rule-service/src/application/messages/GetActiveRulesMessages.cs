namespace RuleService.Application.Messages;

/// <summary>
/// Query to get active rules for a product group and country
/// </summary>
public record GetActiveRules(
    Guid ProductGroupId,
    Guid CountryId
);

public record ActiveRulesResponse(
    List<RuleInfo> Rules
);

public record RuleInfo(
    Guid Id,
    string Name,
    string Category,
    int Priority,
    string Severity,
    bool Enabled,
    string RuleDefinition
);
