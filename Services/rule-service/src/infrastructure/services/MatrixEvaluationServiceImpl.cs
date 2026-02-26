using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RuleService.Domain.Services;
using RuleService.Infrastructure.Persistence;

namespace RuleService.Infrastructure.Services
{
    public class MatrixEvaluationServiceImpl : IMatrixEvaluationService
    {
        private readonly ILookupMatrixRepository _repository;

        public MatrixEvaluationServiceImpl(ILookupMatrixRepository repository)
        {
            _repository = repository;
        }

        public async Task<double?> LookupValueAsync(string matrixName, string[] path, double? numericalValue = null)
        {
            var nodeContent = await _repository.GetNodeByPathAsync(matrixName, path);
            if (string.IsNullOrEmpty(nodeContent)) return null;

            try
            {
                // If it's a direct numerical value, return it
                if (double.TryParse(nodeContent, out var val)) return val;

                // Handle nested logic (Interpolation) if numericalValue is provided
                // This assumes the node is an array of { span: X, value: Y }
                if (numericalValue.HasValue)
                {
                    return PerformInterpolation(nodeContent, numericalValue.Value);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public async Task<List<MatrixChoiceResult>> GetChoicesAsync(string matrixName, string[] parentPath, double inputVariable, double requiredLoad)
        {
            // parentPath is likely ["uprights", "ST20"]
            var parentNode = await _repository.GetNodeByPathAsync(matrixName, parentPath);
            if (string.IsNullOrEmpty(parentNode)) return new List<MatrixChoiceResult>();

            var results = new List<MatrixChoiceResult>();
            try
            {
                using var doc = JsonDocument.Parse(parentNode);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var profileProp in doc.RootElement.EnumerateObject())
                    {
                        var profileId = profileProp.Name;
                        var capacityData = profileProp.Value.ToString();
                        
                        // Calculate interpolated capacity for this specific profile
                        var capacity = PerformInterpolation(capacityData, inputVariable);
                        
                        if (capacity.HasValue && capacity.Value > 0)
                        {
                            results.Add(new MatrixChoiceResult
                            {
                                ChoiceId = profileId,
                                Capacity = Math.Round(capacity.Value, 2),
                                Utilization = Math.Round((requiredLoad / capacity.Value) * 100, 2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return results.OrderBy(r => r.Utilization).ToList();
        }

        public async Task<MatrixLookupResult> GetUtilizationAsync(string matrixName, string[] path, double inputVariable, double requiredLoad)
        {
            var capacity = await LookupValueAsync(matrixName, path, inputVariable);
            
            if (!capacity.HasValue || capacity.Value == 0)
            {
                return new MatrixLookupResult { Success = false, Message = "Capacity not found or zero" };
            }

            var util = (requiredLoad / capacity.Value) * 100;
            
            return new MatrixLookupResult
            {
                Success = true,
                FoundCapacity = capacity.Value,
                UtilizationPercentage = Math.Round(util, 2),
                Message = $"Utilization: {util:F2}%"
            };
        }

        private double? PerformInterpolation(string jsonArray, double targetX)
        {
            var data = JsonSerializer.Deserialize<List<DataPoint>>(jsonArray);
            if (data == null || data.Count == 0) return null;

            var sorted = data.OrderBy(p => p.X).ToList();

            // 1. Check bounds
            if (targetX <= sorted[0].X) return sorted[0].Y;
            if (targetX >= sorted[^1].X) return sorted[^1].Y;

            // 2. Find interval
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var p1 = sorted[i];
                var p2 = sorted[i + 1];

                if (targetX >= p1.X && targetX <= p2.X)
                {
                    // Linear Interpolation: y = y1 + (y2 - y1) * (x - x1) / (x2 - x1)
                    return p1.Y + (p2.Y - p1.Y) * (targetX - p1.X) / (p2.X - p1.X);
                }
            }

            return null;
        }

        private class DataPoint
        {
            public double X { get; set; } // e.g. Span
            public double Y { get; set; } // e.g. Capacity
        }
    }
}
