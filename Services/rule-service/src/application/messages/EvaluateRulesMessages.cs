namespace RuleService.Application.Messages;

/// <summary>
/// Query to evaluate rules against configuration data
/// </summary>
public record EvaluateRules(
    Guid RuleSetId,
    string ConfigurationData, // JSON
    bool PreviewMode = false,
    bool ValidateOnly = false
);

public record EvaluationResponse(
    Guid EvaluationId,
    bool Success,
    List<RuleEvaluationResult> Results
);

public record RuleEvaluationResult(
    Guid RuleId,
    bool Passed,
    string Message,
    string Severity
);
