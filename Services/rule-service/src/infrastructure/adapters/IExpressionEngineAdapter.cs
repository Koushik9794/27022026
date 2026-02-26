using System.Collections.Generic;
using System.Threading.Tasks;
using RuleService.Domain.Services;

namespace RuleService.Infrastructure.Adapters
{
    public interface IExpressionEngineAdapter
    {
        /// <summary>
        /// Evaluates an expression with the provided variables and returns the result as object.
        /// The implementation must be safe and respect configured timeouts.
        /// </summary>
        /// <param name="expression">Expression string (e.g. "width > 100 && height > 100")</param>
        /// <param name="variables">Dictionary of variable name to value</param>
        /// <param name="ctx">Optional evaluation context for stateful functions (ADD_BOM, VALIDATE, etc.)</param>
        /// <returns>Evaluation result</returns>
        Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> variables, RuleEvaluationContext? ctx = null);
    }
}
