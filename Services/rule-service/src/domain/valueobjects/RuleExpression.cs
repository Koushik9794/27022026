#pragma warning disable CS8618
namespace RuleService.Domain.ValueObjects
{
    /// <summary>
    /// RuleExpression Value Object - represents the expression to be evaluated
    /// </summary>
    public class RuleExpression
    {
        public string Expression { get; private set; }

        private RuleExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be empty", nameof(expression));

            Expression = expression;
        }

        /// <summary>
        /// Create a new RuleExpression
        /// </summary>
        public static RuleExpression Create(string expression)
        {
            return new RuleExpression(expression);
        }

        public override string ToString() => Expression;

        public override bool Equals(object obj) =>
            obj is RuleExpression other && Expression == other.Expression;

        public override int GetHashCode() => Expression.GetHashCode();
    }
}

