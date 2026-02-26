using System;

namespace RuleService.Domain.Entities
{
    /// <summary>
    /// Represents a data matrix (Load Chart, Price List, etc.) stored as JSONB
    /// </summary>
    public class LookupMatrix
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string DataJson { get; private set; } // Raw JSON string for storage
        public string MetadataJson { get; private set; }
        public int Version { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private LookupMatrix() { }

        public static LookupMatrix Create(string name, string category, string dataJson, string? metadataJson = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(dataJson)) throw new ArgumentException("Data is required", nameof(dataJson));

            return new LookupMatrix
            {
                Id = Guid.NewGuid(),
                Name = name,
                Category = category,
                DataJson = dataJson,
                MetadataJson = metadataJson ?? "{}",
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateData(string dataJson, string? changeLog = null)
        {
            DataJson = dataJson;
            Version++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMetadata(string metadataJson)
        {
            MetadataJson = metadataJson;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
