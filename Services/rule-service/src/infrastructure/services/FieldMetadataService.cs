using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using RuleService.Domain.Services;
using RuleService.Infrastructure.Dapper;

namespace RuleService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of field metadata service using Dapper
    /// Caches field metadata in memory for performance
    /// </summary>
    public class FieldMetadataService : IFieldMetadataService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private static Dictionary<string, FieldMetadata>? _cache;
        private static readonly object _lock = new object();

        public FieldMetadataService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<FieldMetadata?> GetFieldMetadataAsync(string fieldName)
        {
            var allMetadata = await GetAllFieldMetadataAsync();
            return allMetadata.GetValueOrDefault(fieldName);
        }

        public async Task<Dictionary<string, FieldMetadata>> GetAllFieldMetadataAsync()
        {
            // Return cached data if available
            if (_cache != null)
                return _cache;

            lock (_lock)
            {
                // Double-check after acquiring lock
                if (_cache != null)
                    return _cache;

                // Load from database
                using var connection = _connectionFactory.CreateConnection();
                
                const string sql = @"
                    SELECT field_name, display_name, unit, data_type, category, description
                    FROM field_metadata";

                var metadataList = connection.Query<FieldMetadataDto>(sql).ToList();

                // Build cache dictionary
                _cache = metadataList.ToDictionary(
                    m => m.FieldName,
                    m => new FieldMetadata
                    {
                        FieldName = m.FieldName,
                        DisplayName = m.DisplayName,
                        Unit = m.Unit ?? string.Empty,
                        DataType = m.DataType,
                        Category = m.Category ?? string.Empty,
                        Description = m.Description ?? string.Empty
                    }
                );

                return _cache;
            }
        }

        /// <summary>
        /// Clear cache (useful for testing or when metadata is updated)
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _cache = null;
            }
        }

        private class FieldMetadataDto
        {
            public string FieldName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string? Unit { get; set; }
            public string DataType { get; set; } = string.Empty;
            public string? Category { get; set; }
            public string? Description { get; set; }
        }
    }
}
