using System;
#pragma warning disable CS8618
namespace RuleService.Domain.Entities
{
    /// <summary>
    /// RuleCondition Entity - represents a condition within a rule
    /// </summary>
    public class RuleCondition
    {
        public Guid Id { get; internal set; }
        public Guid RuleId { get; internal set; }
        public string Type { get; internal set; } // AND, OR, NOT
        public string Field { get; internal set; }
        public string Operator { get; internal set; } // EQ, NE, LT, GT, CONTAINS, etc.
        public string Value { get; internal set; } // JSON serialized value

        internal RuleCondition() { }

        /// <summary>
        /// Create a new RuleCondition
        /// </summary>
        public static RuleCondition Create(
            Guid ruleId,
            string type,
            string field,
            string op,
            string value)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be empty", nameof(field));

            return new RuleCondition
            {
                Id = Guid.NewGuid(),
                RuleId = ruleId,
                Type = type,
                Field = field,
                Operator = op,
                Value = value
            };
        }

        /// <summary>
        /// Get human-readable field name
        /// </summary>
        /// <summary>
        /// Get human-readable operator description (keep in domain as it's static domain logic)
        /// </summary>
        public string GetOperatorDisplayName()
        {
            return Operator?.ToUpperInvariant() switch
            {
                "GT" => "greater than",
                "LT" => "less than",
                "GTE" => "at least",
                "LTE" => "at most",
                "EQ" => "equal to",
                "NE" => "not equal to",
                "CONTAINS" => "contains",
                "BETWEEN" => "between",
                _ => Operator ?? "equals"
            };
        }
    }
}

