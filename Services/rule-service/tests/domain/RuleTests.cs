using Xunit;
using RuleService.Domain.Entities;

namespace RuleService.Tests.Domain
{
    /// <summary>
    /// Unit tests for Rule Entity
    /// </summary>
    public class RuleTests
    {
        [Fact]
        public void Create_WithValidParameters_ShouldCreateRule()
        {
            // Arrange
            var name = "Test Rule";
            var category = "SPATIAL";
            var priority = 1;

            // Act
            var rule = Rule.Create(name, "Description", category, priority, "ERROR");

            // Assert
            Assert.NotNull(rule);
            Assert.Equal(name, rule.Name);
            Assert.Equal(category, rule.Category);
            Assert.True(rule.Enabled);
        }

        [Fact]
        public void SetEnabled_WithTrue_ShouldEnableRule()
        {
            // Arrange
            var rule = Rule.Create("Test", "Desc", "SPATIAL", 1, "ERROR");
            rule.SetEnabled(false);

            // Act
            rule.SetEnabled(true);

            // Assert
            Assert.True(rule.Enabled);
        }

        [Fact]
        public void AddCondition_WithValidCondition_ShouldAddToConditionsList()
        {
            // Arrange
            var rule = Rule.Create("Test", "Desc", "SPATIAL", 1, "ERROR");
            var condition = RuleCondition.Create(rule.Id, "AND", "width", "GT", "100");

            // Act
            rule.AddCondition(condition);

            // Assert
            Assert.Single(rule.Conditions);
        }
    }
}
