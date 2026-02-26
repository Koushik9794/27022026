using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using RuleService.Application.Handlers;
using RuleService.Application.Messages;
using RuleService.Domain.Aggregates;
using RuleService.Domain.Entities;
using RuleService.Infrastructure.Persistence;
using Xunit;

namespace RuleService.UnitTests
{
    public class RuleManifestTests
    {
        [Fact]
        public async Task GetManifest_Returns_Correct_Structure_With_Rules_And_Matrices()
        {
            // Arrange
            var productGroupId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            
            // Mock Rule Repository
            var mockRuleRepo = new Mock<IRuleRepository>();
            var ruleSet = RuleSet.Create("Test Manifest Set", productGroupId, countryId, DateTime.UtcNow, null);
            
            var rule = RuleService.Domain.Entities.Rule.Create("Test Rule", "Description", "SPATIAL", 1, "ERROR");
            rule.AddCondition(RuleCondition.Create(rule.Id, "AND", "PalletWidth", "GT", "1000"));
            ruleSet.AddRule(rule);
            
            mockRuleRepo.Setup(r => r.GetActiveRuleSetsByProductGroupAndCountryAsync(productGroupId, countryId))
                .ReturnsAsync(new List<RuleSet> { ruleSet });

            // Mock Matrix Repository
            var mockMatrixRepo = new Mock<ILookupMatrixRepository>();
            var matrix = LookupMatrix.Create("BeamChart", "Selection", "{}");
            mockMatrixRepo.Setup(m => m.GetAllMetadataAsync())
                .ReturnsAsync(new List<LookupMatrix> { matrix });

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, mockRuleRepo.Object, mockMatrixRepo.Object);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            Assert.NotNull(manifest);
            Assert.Equal(productGroupId, manifest.ProductGroupId);
            Assert.Single(manifest.Rules);
            Assert.Equal("Test Rule", manifest.Rules[0].Name);
            Assert.Single(manifest.Rules[0].Conditions);
            Assert.Equal("PalletWidth", manifest.Rules[0].Conditions[0].Field);
            
            Assert.Single(manifest.Matrices);
            Assert.Equal("BeamChart", manifest.Matrices[0].Name);
        }

        [Fact]
        public async Task MatrixLookup_Calculates_Correct_Interpolated_Value()
        {
            // Arrange
            var mockRepo = new Mock<ILookupMatrixRepository>();
            
            // Setup a matrix for ST20 Upright with two span points for HEM_80
            // Span 2700 -> 2000kg
            // Span 2800 -> 1800kg
            var jsonData = @"
            [
                { ""X"": 2700, ""Y"": 2000 },
                { ""X"": 2800, ""Y"": 1800 }
            ]";

            mockRepo.Setup(m => m.GetNodeByPathAsync("BeamChart", It.Is<string[]>(p => p[0] == "uprights" && p[1] == "ST20" && p[2] == "HEM_80")))
                .ReturnsAsync(jsonData);

            var service = new RuleService.Infrastructure.Services.MatrixEvaluationServiceImpl(mockRepo.Object);

            // Act: Lookup for Span 2750 (midway)
            var result = await service.LookupValueAsync("BeamChart", new[] { "uprights", "ST20", "HEM_80" }, 2750);

            // Assert: Interpolation should be 1900kg
            Assert.NotNull(result);
            Assert.Equal(1900, result.Value);
        }

        [Fact]
        public async Task GetChoices_Returns_Utilization_For_All_Profiles()
        {
            // Arrange
            var mockRepo = new Mock<ILookupMatrixRepository>();
            
            // Setup parent node for ST20 containing two profiles
            var parentJson = @"
            {
                ""HEM_80"": [ { ""X"": 2700, ""Y"": 2000 }, { ""X"": 2800, ""Y"": 1800 } ],
                ""HEM_100"": [ { ""X"": 2700, ""Y"": 3000 }, { ""X"": 2800, ""Y"": 2800 } ]
            }";

            mockRepo.Setup(m => m.GetNodeByPathAsync("BeamChart", It.Is<string[]>(p => p[0] == "uprights" && p[1] == "ST20")))
                .ReturnsAsync(parentJson);

            var service = new RuleService.Infrastructure.Services.MatrixEvaluationServiceImpl(mockRepo.Object);

            // Act: Get choices for Span 2750 and Load 1500kg
            // HEM_80 Capacity at 2750 = 1900 -> Util = 1500/1900 = 78.95%
            // HEM_100 Capacity at 2750 = 2900 -> Util = 1500/2900 = 51.72%
            var choices = await service.GetChoicesAsync("BeamChart", new[] { "uprights", "ST20" }, 2750, 1500);

            // Assert
            Assert.Equal(2, choices.Count);
            Assert.Equal("HEM_100", choices[0].ChoiceId); // sorted by utilization (lower first)
            Assert.Equal(51.72, choices[0].Utilization);
            Assert.Equal("HEM_80", choices[1].ChoiceId);
            Assert.Equal(78.95, choices[1].Utilization);
        }
    }
}
