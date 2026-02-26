namespace RuleService.Application.Messages;

/// <summary>
/// Command to validate a RuleSet
/// </summary>
public record ValidateRuleSet(Guid RuleSetId);

public record ValidationResponse(
    bool IsValid,
    List<ValidationError> Errors
);

public record ValidationError(
    string Field,
    string Message,
    string Severity
);
