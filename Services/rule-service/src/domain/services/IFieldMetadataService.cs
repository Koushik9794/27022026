using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuleService.Domain.Services
{
    /// <summary>
    /// Service for retrieving field metadata (display names, units, etc.)
    /// </summary>
    public interface IFieldMetadataService
    {
        Task<FieldMetadata?> GetFieldMetadataAsync(string fieldName);
        Task<Dictionary<string, FieldMetadata>> GetAllFieldMetadataAsync();
    }

    public class FieldMetadata
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
