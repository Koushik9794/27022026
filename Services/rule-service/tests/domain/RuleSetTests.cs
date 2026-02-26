using Xunit;
using RuleService.Domain.Aggregates;
using RuleService.Domain.Entities;

namespace RuleService.Tests.Domain
{
    /// <summary>
    /// Unit tests for RuleSet Aggregate
    /// </summary>
    public class RuleSetTests
    {
        [Fact]
        public void Create_WithValidParameters_ShouldCreateRuleSet()
        {
            // Arrange
            var name = "Test RuleSet";
            var productGroupId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            var effectiveFrom = DateTime.UtcNow;

            // Act
            var ruleSet = RuleSet.Create(name, productGroupId, countryId, effectiveFrom, null);

            // Assert
            Assert.NotNull(ruleSet);
            Assert.Equal(name, ruleSet.Name);
            Assert.Equal("DRAFT", ruleSet.Status);
        }

        [Fact]
        public void Activate_WithDraftRuleSet_ShouldChangeStatusToActive()
        {
            // Arrange
            var ruleSet = RuleSet.Create("Test", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null);

            // Act
            ruleSet.Activate();

            // Assert
            Assert.Equal("ACTIVE", ruleSet.Status);
        }

        [Fact]
        public void AddRule_WithValidRule_ShouldAddToRuleList()
        {
            // Arrange
            var ruleSet = RuleSet.Create("Test", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null);
            var rule = Rule.Create("Test Rule", "Description", "SPATIAL", 1, "ERROR");

            // Act
            ruleSet.AddRule(rule);

            // Assert
            Assert.Single(ruleSet.Rules);
            Assert.Contains(rule, ruleSet.Rules);
        }
    }
}
