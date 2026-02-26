namespace RuleService.Application.Messages;

/// <summary>
/// Command to activate a RuleSet
/// </summary>
public record ActivateRuleSet(Guid RuleSetId);

public record ActivationResponse(bool Success, string Message);
