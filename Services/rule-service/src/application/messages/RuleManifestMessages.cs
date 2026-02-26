using System;
using System.Collections.Generic;

namespace RuleService.Application.Messages
{
    public record GetRuleManifest(Guid ProductGroupId, Guid CountryId);

    public class RuleManifestResponse
    {
        public string Version { get; set; } = string.Empty;
        public Guid ProductGroupId { get; set; }
        public Guid CountryId { get; set; }
        public List<RuleDefinitionInfo> Rules { get; set; } = new();
        public List<ManifestMatrixInfo> Matrices { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class RuleDefinitionInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string? Formula { get; set; }
        public string? MessageTemplate { get; set; }
        public List<RuleConditionInfo> Conditions { get; set; } = new();
    }

    public class RuleConditionInfo
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string FieldDisplayName { get; set; } = string.Empty;
        public string OperatorDisplayName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }

    public class ManifestMatrixInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Version { get; set; }
    }
}
