using Microsoft.AspNetCore.Mvc;
using RuleService.Infrastructure.Persistence;
using RuleService.Domain.Services;
using Wolverine.Http;

namespace RuleService.Application.Endpoints;

public static class MatrixEndpoints
{
    [WolverineGet("/api/v1/matrices/{name}")]
    public static async Task<IResult> GetMatrix(string name, [FromServices] ILookupMatrixRepository repository)
    {
        var matrix = await repository.GetByNameAsync(name);
        return matrix != null ? Results.Ok(matrix) : Results.NotFound();
    }

    [WolverineGet("/api/v1/matrices/{name}/choices")]
    public static async Task<IResult> GetChoices(
        string name, 
        string uprightId, 
        double span, 
        double load, 
        [FromServices] IMatrixEvaluationService service)
    {
        // parentPath for beam profiles is ["uprights", uprightId]
        var choices = await service.GetChoicesAsync(name, new[] { "uprights", uprightId }, span, load);
        return Results.Ok(choices);
    }


    [WolverineGet("/api/v1/matrices/{name}/lookup")]
    public static async Task<IResult> LookupValue(
            string name,
            HttpRequest httpRequest,
            [FromServices] IMatrixEvaluationService service)
    {
        // Example: /api/v1/matrices/BeamChart/lookup?path=uprights&path=ST20&path=HEM_80&value=2750
        var path = httpRequest.Query["path"].ToArray();

        double? value = null;
        if (double.TryParse(httpRequest.Query["value"], out var parsed))
            value = parsed;

        var result = await service.LookupValueAsync(name, path, value);
        return result.HasValue ? Results.Ok(new { value = result.Value }) : Results.NotFound();
    }


    [WolverinePatch("/api/v1/matrices/{id}/cell")]
    public static async Task<IResult> UpdateCell(
        Guid id, 
        UpdateCellRequest request, 
        [FromServices] ILookupMatrixRepository repository)
    {
        await repository.UpdateCellAsync(id, request.Path, request.Value);
        
        // In a real scenario, we would publish a message here:
        // await bus.PublishAsync(new MatrixChanged(id, request.Path));
        
        return Results.NoContent();
    }
}

public record UpdateCellRequest(string[] Path, object Value);
