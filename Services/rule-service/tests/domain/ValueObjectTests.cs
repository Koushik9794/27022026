using Xunit;
using RuleService.Domain.ValueObjects;

namespace RuleService.Tests.Domain
{
    /// <summary>
    /// Unit tests for Value Objects
    /// </summary>
    public class ValueObjectTests
    {
        [Fact]
        public void EffectivePeriod_Create_WithValidDates_ShouldCreatePeriod()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow.AddDays(1);

            // Act
            var period = EffectivePeriod.Create(from, to);

            // Assert
            Assert.Equal(from, period.From);
            Assert.Equal(to, period.To);
        }

        [Fact]
        public void EffectivePeriod_IsEffectiveNow_WithCurrentDate_ShouldReturnTrue()
        {
            // Arrange
            var from = DateTime.UtcNow.AddHours(-1);
            var to = DateTime.UtcNow.AddHours(1);
            var period = EffectivePeriod.Create(from, to);

            // Act
            var result = period.IsEffectiveNow();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RuleExpression_Create_WithValidExpression_ShouldCreateExpression()
        {
            // Arrange
            var expression = "width > 100 AND height < 200";

            // Act
            var ruleExpr = RuleExpression.Create(expression);

            // Assert
            Assert.Equal(expression, ruleExpr.Expression);
        }

        [Fact]
        public void RuleOutcome_Create_ShouldCreateOutcome()
        {
            // Arrange & Act
            var outcome = RuleOutcome.Create(true, "Rule passed", "INFO");

            // Assert
            Assert.True(outcome.Passed);
            Assert.Equal("Rule passed", outcome.Message);
            Assert.Equal("INFO", outcome.Severity);
        }
    }
}
