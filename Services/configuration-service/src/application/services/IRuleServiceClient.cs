using GssCommon.Common.Models.Configurator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationService.Application.Services
{
    public interface IRuleServiceClient
    {
        Task<RuleEvaluationResponse> EvaluateRulesAsync(RuleEvaluationRequest request);
    }

    public class RuleEvaluationRequest
    {
        public Guid RuleSetId { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
    }

    public class RuleEvaluationResponse
    {
        public bool Success { get; set; }
        public List<RuleOutcomeDto> Outcomes { get; set; } = new();
    }

    public class RuleOutcomeDto
    {
        public Guid RuleId { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
