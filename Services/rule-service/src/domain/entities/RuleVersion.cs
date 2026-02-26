#pragma warning disable CS8618
namespace RuleService.Domain.Entities
{
    /// <summary>
    /// RuleVersion Entity - tracks versions of rules for audit trail
    /// </summary>
    public class RuleVersion
    {
        public Guid Id { get; internal set; }
        public Guid RuleId { get; internal set; }
        public int VersionNumber { get; internal set; }
        public string ChangeLog { get; internal set; }
        public string RuleDefinition { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public string CreatedBy { get; internal set; }

        private RuleVersion() { }

        /// <summary>
        /// Create a new RuleVersion
        /// </summary>
        public static RuleVersion Create(
            Guid ruleId,
            int versionNumber,
            string changeLog,
            string ruleDefinition,
            string createdBy)
        {
            return new RuleVersion
            {
                Id = Guid.NewGuid(),
                RuleId = ruleId,
                VersionNumber = versionNumber,
                ChangeLog = changeLog,
                RuleDefinition = ruleDefinition,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
    }
}

