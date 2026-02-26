using RuleService.Application.Messages;
using Wolverine.Http;

namespace RuleService.Application.Handlers;

public static class RuleSetEndpoints
{
    [WolverinePost("/api/v1/ruleset")]
    public static CreateRuleSetResponse CreateRuleSet(CreateRuleSet command)
    {
        return new CreateRuleSetResponse(Guid.NewGuid(), "DRAFT");
    }

    [WolverineGet("/api/v1/ruleset/{ruleSetId}")]
    public static RuleSetResponse GetRuleSet(Guid ruleSetId)
    {
        return new RuleSetResponse(
            Id: ruleSetId,
            Name: "Sample RuleSet",
            ProductGroupId: Guid.NewGuid(),
            CountryId: Guid.NewGuid(),
            EffectiveFrom: DateTime.UtcNow,
            EffectiveTo: null,
            Status: "DRAFT",
            Rules: new List<RuleInfo>()
        );
    }
}
