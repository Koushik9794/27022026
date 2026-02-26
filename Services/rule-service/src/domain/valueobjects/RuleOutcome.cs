#pragma warning disable CS8618
namespace RuleService.Domain.ValueObjects
{
    /// <summary>
    /// RuleOutcome Value Object - represents the result of rule evaluation
    /// </summary>
    public class RuleOutcome
    {
        public Guid RuleId { get; private set; }
        public bool Passed { get; private set; }
        public string Message { get; private set; }
        public string Severity { get; private set; } // ERROR, WARNING, INFO

        public Dictionary<string, object> Data { get; private set; } = new();

        private RuleOutcome() { }

        private RuleOutcome(Guid ruleId, bool passed, string message, string severity, Dictionary<string, object>? data = null)
        {
            RuleId = ruleId;
            Passed = passed;
            Message = message ?? string.Empty;
            Severity = severity;
            Data = data ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Create a new RuleOutcome
        /// </summary>
        public static RuleOutcome Create(Guid ruleId, bool passed, string message, string severity, Dictionary<string, object>? data = null)
        {
            return new RuleOutcome(ruleId, passed, message, severity, data);
        }

        public override string ToString() => $"{(Passed ? "PASS" : "FAIL")}: {Message}";

        public override bool Equals(object obj) =>
            obj is RuleOutcome other && Passed == other.Passed && Message == other.Message;

        public override int GetHashCode() => HashCode.Combine(Passed, Message);
    }
}

