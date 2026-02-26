using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuleService.Application.Handlers;
using RuleService.Application.Messages;
using Wolverine.Http;

namespace RuleService.Application.Endpoints;

/// <summary>
/// Endpoints for importing load charts from Excel/CSV files
/// Uses CQRS pattern with handlers for business logic
/// </summary>
public static class MatrixImportEndpoints
{
    /// <summary>
    /// Import a load chart from Excel or CSV file
    /// Expected format:
    /// Row 1: Headers [Upright, BeamSpan, Profile1, Profile2, ...]
    /// Row 2+: Data [ST20, 2700, 2000, 3000, ...]
    /// </summary>
    [WolverinePost("/api/v1/matrices/import")]
    public static async Task<IResult> ImportLoadChart(
        [FromForm] IFormFile file,
        [FromForm] string matrixName,
        [FromForm] string category,
        [FromServices] ImportLoadChartHandler handler)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded" });

        if (string.IsNullOrWhiteSpace(matrixName))
            return Results.BadRequest(new { error = "Matrix name is required" });

        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            List<string[]> rows;

            if (extension == ".csv")
            {
                rows = await ParseCsvAsync(file);
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                return Results.BadRequest(new
                {
                    error = "Excel import requires EPPlus library. Use CSV format or implement Excel parsing.",
                    suggestion = "Convert Excel to CSV or install EPPlus NuGet package"
                });
            }
            else
            {
                return Results.BadRequest(new { error = "Unsupported file format. Use .csv" });
            }

            // Create command and delegate to handler
            var command = new ImportLoadChartCommand(matrixName, category, rows);
            var result = await handler.Handle(command);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Ok(new
            {
                message = "Load chart imported successfully",
                matrixName = result.MatrixName,
                rowsProcessed = result.RowsProcessed,
                dataStructure = JsonSerializer.Deserialize<object>(result.GeneratedJsonb)
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Import failed"
            );
        }
    }

    /// <summary>
    /// Preview import without saving (dry run)
    /// </summary>
    [WolverinePost("/api/v1/matrices/import/preview")]
    public static async Task<IResult> PreviewImport(
        [FromForm] IFormFile file,
        [FromServices] ImportLoadChartHandler handler)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded" });

        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".csv")
            {
                return Results.BadRequest(new { error = "Only CSV preview is supported" });
            }

            var rows = await ParseCsvAsync(file);

            // Create command and delegate to handler
            var command = new PreviewImportCommand(rows);
            var result = await handler.Handle(command);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Ok(new
            {
                fileName = file.FileName,
                rowCount = result.RowCount,
                headers = result.Headers,
                sampleData = result.SampleData,
                generatedStructure = JsonSerializer.Deserialize<object>(result.GeneratedJsonb)
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Preview failed"
            );
        }
    }

    private static async Task<List<string[]>> ParseCsvAsync(IFormFile file)
    {
        var rows = new List<string[]>();

        using var reader = new StreamReader(file.OpenReadStream());

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Simple CSV parsing (doesn't handle quoted commas)
            var values = line.Split(',').Select(v => v.Trim()).ToArray();
            rows.Add(values);
        }

        return rows;
    }
}
