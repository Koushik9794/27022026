using RuleService.Domain.Aggregates;
using RuleService.Domain.ValueObjects;

namespace RuleService.Domain.Services
{
    /// <summary>
    /// RuleEvaluationService - Domain Service for evaluating rules
    /// </summary>
    public interface IRuleEvaluationService
    {
        /// <summary>
        /// Evaluate a RuleSet against configuration data
        /// </summary>
        Task<RuleEvaluationResult> EvaluateRuleSetAsync(RuleSet ruleSet, string configurationData, bool preview = false, bool validateOnly = false);

        /// <summary>
        /// Evaluate a single rule expression
        /// </summary>
        Task<RuleOutcome> EvaluateExpressionAsync(RuleExpression expression, object context);
    }

    /// <summary>
    /// Result of evaluating a RuleSet
    /// </summary>
    public class RuleEvaluationResult
    {
        public Guid RuleSetId { get; set; }
        public bool Success { get; set; }
        public List<RuleOutcome> Outcomes { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
    }
}
