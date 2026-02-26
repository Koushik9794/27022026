using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuleService.Domain.Services
{
    /// <summary>
    /// Service for performing smart lookups in JSONB matrices
    /// </summary>
    public interface IMatrixEvaluationService
    {
        /// <summary>
        /// Perform a lookup with specific strategy (Interpolate, RoundUp, etc.)
        /// </summary>
        Task<double?> LookupValueAsync(string matrixName, string[] path, double? numericalValue = null);

        /// <summary>
        /// Calculate utilization for all available choices (e.g., all beam profiles) at a specific path
        /// </summary>
        Task<List<MatrixChoiceResult>> GetChoicesAsync(string matrixName, string[] parentPath, double inputVariable, double requiredLoad);
    }

    public class MatrixChoiceResult
    {
        public string ChoiceId { get; set; } = string.Empty;
        public double Capacity { get; set; }
        public double Utilization { get; set; }
        public bool IsSafe => Utilization <= 100;
    }

    public class MatrixLookupResult
    {
        public bool Success { get; set; }
        public double FoundCapacity { get; set; }
        public double UtilizationPercentage { get; set; }
        public string? ResolvedPath { get; set; }
        public string? Message { get; set; }
    }
}
