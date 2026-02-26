using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RuleService.Application.Messages;
using RuleService.Domain.Entities;
using RuleService.Infrastructure.Persistence;

namespace RuleService.Application.Handlers;

/// <summary>
/// Handler for importing load charts from CSV data
/// Follows CQRS pattern with Wolverine
/// </summary>
public class ImportLoadChartHandler
{
    private readonly ILookupMatrixRepository _repository;

    public ImportLoadChartHandler(ILookupMatrixRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handle import command
    /// </summary>
    public async Task<ImportLoadChartResult> Handle(ImportLoadChartCommand command)
    {
        try
        {
            // Validate input
            if (command.CsvRows.Count < 2)
            {
                return new ImportLoadChartResult(
                    Success: false,
                    MatrixName: command.MatrixName,
                    RowsProcessed: 0,
                    GeneratedJsonb: string.Empty,
                    ErrorMessage: "CSV must have at least 2 rows (header + data)"
                );
            }

            // Convert CSV to JSONB structure
            var jsonbData = ConvertToJsonbStructure(command.CsvRows);

            // Create or update matrix
            var matrix = LookupMatrix.Create(
                command.MatrixName,
                command.Category ?? "LOAD_CHART",
                jsonbData
            );

            await _repository.SaveAsync(matrix);

            return new ImportLoadChartResult(
                Success: true,
                MatrixName: command.MatrixName,
                RowsProcessed: command.CsvRows.Count - 1,
                GeneratedJsonb: jsonbData
            );
        }
        catch (Exception ex)
        {
            return new ImportLoadChartResult(
                Success: false,
                MatrixName: command.MatrixName,
                RowsProcessed: 0,
                GeneratedJsonb: string.Empty,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Handle preview command (dry run)
    /// </summary>
    public Task<PreviewImportResult> Handle(PreviewImportCommand command)
    {
        try
        {
            if (command.CsvRows.Count < 2)
            {
                return Task.FromResult(new PreviewImportResult(
                    Success: false,
                    RowCount: command.CsvRows.Count,
                    Headers: Array.Empty<string>(),
                    SampleData: new List<string[]>(),
                    GeneratedJsonb: string.Empty,
                    ErrorMessage: "CSV must have at least 2 rows"
                ));
            }

            var jsonbData = ConvertToJsonbStructure(command.CsvRows);
            var headers = command.CsvRows[0];
            var sampleData = command.CsvRows.Skip(1).Take(5).ToList();

            return Task.FromResult(new PreviewImportResult(
                Success: true,
                RowCount: command.CsvRows.Count,
                Headers: headers,
                SampleData: sampleData,
                GeneratedJsonb: jsonbData
            ));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PreviewImportResult(
                Success: false,
                RowCount: command.CsvRows.Count,
                Headers: Array.Empty<string>(),
                SampleData: new List<string[]>(),
                GeneratedJsonb: string.Empty,
                ErrorMessage: ex.Message
            ));
        }
    }

    /// <summary>
    /// Convert CSV rows to JSONB structure
    /// Expected format: [Upright, BeamSpan, Profile1, Profile2, ...]
    /// </summary>
    private static string ConvertToJsonbStructure(List<string[]> rows)
    {
        var headers = rows[0];

        if (headers.Length < 3)
            throw new InvalidOperationException("CSV must have at least 3 columns: Upright, BeamSpan, and one or more profiles");

        // Build structure: { "uprights": { "ST20": { "HEM_80": [...] } } }
        var uprightData = new Dictionary<string, Dictionary<string, List<DataPoint>>>();

        // Get profile names from headers (skip first 2 columns)
        var profileNames = headers.Skip(2).ToArray();

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (row.Length < 3) continue; // Skip incomplete rows

            var uprightId = row[0];
            if (string.IsNullOrWhiteSpace(uprightId)) continue;

            if (!double.TryParse(row[1], out var beamSpan)) continue;

            // Ensure upright exists in dictionary
            if (!uprightData.ContainsKey(uprightId))
            {
                uprightData[uprightId] = new Dictionary<string, List<DataPoint>>();
            }

            // Process each profile column
            for (int j = 0; j < profileNames.Length && j + 2 < row.Length; j++)
            {
                var profileName = profileNames[j];
                if (string.IsNullOrWhiteSpace(profileName)) continue;

                if (!double.TryParse(row[j + 2], out var capacity)) continue;

                // Ensure profile exists in upright
                if (!uprightData[uprightId].ContainsKey(profileName))
                {
                    uprightData[uprightId][profileName] = new List<DataPoint>();
                }

                // Add data point (X = span, Y = capacity)
                uprightData[uprightId][profileName].Add(new DataPoint
                {
                    X = beamSpan,
                    Y = capacity
                });
            }
        }

        // Sort data points by X (span) for each profile
        foreach (var upright in uprightData.Values)
        {
            foreach (var profile in upright.Values)
            {
                profile.Sort((a, b) => a.X.CompareTo(b.X));
            }
        }

        // Wrap in "uprights" key to match expected structure
        var result = new { uprights = uprightData };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
