using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RuleService.Domain.Services;
using RuleService.Infrastructure.Persistence;
using Wolverine.Http;

namespace RuleService.Application.Endpoints;

    public static class RuleEvaluationEndpoints
    {
        [WolverinePost("/api/rules/evaluate")]
        public static async Task<IResult> Evaluate(
            [FromBody] RuleEvalRequest request,
            [FromServices] IRuleEvaluationService evalService,
            [FromServices] IRuleRepository ruleRepo)
        {
            var ruleSet = await ruleRepo.GetByIdAsync(request.RuleSetId);
            if (ruleSet == null) return Results.NotFound("RuleSet not found");

            // Convert Dictionary<string, object> to JSON string for the service which expects it
            var configData = System.Text.Json.JsonSerializer.Serialize(request.Variables);
            
            var result = await evalService.EvaluateRuleSetAsync(ruleSet, configData);
            
            return Results.Ok(new 
            { 
                Success = result.Success, 
                Outcomes = result.Outcomes.Select(o => new {
                    o.RuleId,
                    o.Passed,
                    o.Message,
                    o.Severity,
                    o.Data
                })
            });
        }
    }

    public class RuleEvalRequest
    {
        public Guid RuleSetId { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
    }

