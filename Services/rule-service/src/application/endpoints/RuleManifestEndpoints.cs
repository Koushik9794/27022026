using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RuleService.Application.Messages;
using RuleService.Domain.Services;
using RuleService.Infrastructure.Persistence;
using Wolverine.Http;

namespace RuleService.Application.Handlers;

public static class RuleManifestEndpoints
{
    [WolverineGet("/api/v1/rules/manifest")]
    public static async Task<IResult> GetManifest(
        [FromQuery] Guid productGroupId, 
        [FromQuery] Guid countryId,
        [FromServices] IRuleRepository ruleRepo,
        [FromServices] ILookupMatrixRepository matrixRepo,
        [FromServices] IFieldMetadataService fieldMetadataService)
    {
        // 1. Fetch active rulesets
        var activeRuleSets = await ruleRepo.GetActiveRuleSetsByProductGroupAndCountryAsync(productGroupId, countryId);
        
        if (!activeRuleSets.Any())
        {
            return Results.NotFound(new { message = "No active ruleset found for this configuration." });
        }

        // Use the latest active ruleset
        var mainRuleSet = activeRuleSets.First();

        // 2. Fetch matrix metadata and field metadata
        var matricesTask = matrixRepo.GetAllMetadataAsync();
        var fieldMetadataTask = fieldMetadataService.GetAllFieldMetadataAsync();

        await Task.WhenAll(matricesTask, fieldMetadataTask);
        
        var matrices = matricesTask.Result;
        var fieldMetadata = fieldMetadataTask.Result;

        // 3. Construct manifest
        var manifest = new RuleManifestResponse
        {
            ProductGroupId = productGroupId,
            CountryId = countryId,
            Version = mainRuleSet.UpdatedAt.ToString("yyyyMMdd.HHmm"),
            GeneratedAt = DateTime.UtcNow,
            Rules = mainRuleSet.Rules.Select(r => new RuleDefinitionInfo
            {
                Id = r.Id,
                Name = r.Name,
                Category = r.Category,
                Severity = r.Severity,
                Priority = r.Priority,
                Formula = r.Formula,
                MessageTemplate = r.MessageTemplate,
                Conditions = r.Conditions.Select(c => 
                {
                    var meta = fieldMetadata.GetValueOrDefault(c.Field);
                    return new RuleConditionInfo
                    {
                        Field = c.Field,
                        Operator = c.Operator,
                        Value = c.Value,
                        FieldDisplayName = meta?.DisplayName ?? c.Field,
                        OperatorDisplayName = c.GetOperatorDisplayName(), // Still in entity as it's static/conceptual
                        Unit = meta?.Unit ?? ""
                    };
                }).ToList()
            }).ToList(),
            Matrices = matrices.Select(m => new ManifestMatrixInfo
            {
                Name = m.Name,
                Category = m.Category,
                Version = m.Version
            }).ToList()
        };

        return Results.Ok(manifest);
    }
}
