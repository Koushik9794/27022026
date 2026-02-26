#pragma warning disable CS8618
namespace RuleService.Domain.ValueObjects
{
    /// <summary>
    /// EffectivePeriod Value Object - represents the period during which a rule is effective
    /// </summary>
    public class EffectivePeriod
    {
        public DateTime From { get; private set; }
        public DateTime? To { get; private set; }

        private EffectivePeriod() { }

        private EffectivePeriod(DateTime from, DateTime? to)
        {
            if (to.HasValue && to.Value <= from)
                throw new ArgumentException("EffectiveTo must be after EffectiveFrom");

            From = from;
            To = to;
        }

        /// <summary>
        /// Create a new EffectivePeriod
        /// </summary>
        public static EffectivePeriod Create(DateTime from, DateTime? to = null)
        {
            return new EffectivePeriod(from, to);
        }

        /// <summary>
        /// Check if the period is currently effective
        /// </summary>
        public bool IsEffectiveNow()
        {
            var now = DateTime.UtcNow;
            return now >= From && (!To.HasValue || now <= To.Value);
        }

        public override bool Equals(object obj) =>
            obj is EffectivePeriod other && From == other.From && To == other.To;

        public override int GetHashCode() => HashCode.Combine(From, To);
    }
}

