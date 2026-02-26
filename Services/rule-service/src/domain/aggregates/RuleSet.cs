using RuleService.Domain.Entities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace RuleService.Domain.Aggregates
{
    /// <summary>
    /// RuleSet Aggregate Root
    /// Represents a collection of rules that apply to a specific product group and country
    /// </summary>
    public class RuleSet
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Guid ProductGroupId { get; internal set; }
        public Guid CountryId { get; internal set; }
        public DateTime EffectiveFrom { get; internal set; }
        public DateTime? EffectiveTo { get; internal set; }
        public string Status { get; internal set; } // DRAFT, ACTIVE, INACTIVE, ARCHIVED
        public List<Rule> Rules { get; internal set; } = new();
        public DateTime CreatedAt { get; internal set; }
        public DateTime UpdatedAt { get; internal set; }

        internal RuleSet() { }

        /// <summary>
        /// Create a new RuleSet
        /// </summary>
        public static RuleSet Create(
            string name,
            Guid productGroupId,
            Guid countryId,
            DateTime effectiveFrom,
            DateTime? effectiveTo)
        {
            var ruleSet = new RuleSet
            {
                Id = Guid.NewGuid(),
                Name = name,
                ProductGroupId = productGroupId,
                CountryId = countryId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                Status = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return ruleSet;
        }

        /// <summary>
        /// Activate the RuleSet
        /// </summary>
        public void Activate()
        {
            if (Status == "ACTIVE")
                throw new InvalidOperationException("RuleSet is already active");

            Status = "ACTIVE";
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Add a rule to the RuleSet
        /// </summary>
        public void AddRule(Rule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            Rules.Add(rule);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
