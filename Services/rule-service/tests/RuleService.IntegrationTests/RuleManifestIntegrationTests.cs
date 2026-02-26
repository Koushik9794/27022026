using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Npgsql;
using Dapper;
using RuleService.Infrastructure.Persistence;
using RuleService.Application.Handlers;
using RuleService.Domain.Entities;
using RuleService.Domain.Aggregates;
using Microsoft.AspNetCore.Http.HttpResults;
using RuleService.Application.Messages;

namespace RuleService.IntegrationTests
{
    /// <summary>
    /// Integration tests for Rule Manifest API
    /// Requires Docker database to be running
    /// </summary>
    public class RuleManifestIntegrationTests : IDisposable
    {
        private readonly string _connectionString = "Host=127.0.0.1;Port=5433;Database=rule_service;Username=postgres;Password=postgres;SSL Mode=Disable;";
        private readonly DapperRuleRepository _ruleRepo;
        private readonly DapperLookupMatrixRepository _matrixRepo;

        public RuleManifestIntegrationTests()
        {
            var connectionFactory = new TestConnectionFactory(_connectionString);
            _ruleRepo = new DapperRuleRepository(connectionFactory);
            _matrixRepo = new DapperLookupMatrixRepository(_connectionString);
        }

        private class TestConnectionFactory : RuleService.Infrastructure.Dapper.IDbConnectionFactory
        {
            private readonly string _connectionString;
            public TestConnectionFactory(string connectionString) => _connectionString = connectionString;
            public System.Data.IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
        }

        [Fact]
        public async Task GetManifest_WithValidProductAndCountry_ReturnsManifestWithRules()
        {
            // Arrange
            var productGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var countryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            // Seed a test ruleset
            await SeedTestRuleSet(productGroupId, countryId);

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            Assert.NotNull(manifest);
            Assert.Equal(productGroupId, manifest.ProductGroupId);
            Assert.Equal(countryId, manifest.CountryId);
            Assert.NotEmpty(manifest.Version);
            Assert.NotEmpty(manifest.Rules);
        }

        [Fact]
        public async Task GetManifest_WithNoActiveRuleSet_ReturnsNotFound()
        {
            // Arrange
            var nonExistentProductGroup = Guid.NewGuid();
            var nonExistentCountry = Guid.NewGuid();

            // Act
            var result = await RuleManifestEndpoints.GetManifest(nonExistentProductGroup, nonExistentCountry, _ruleRepo, _matrixRepo);

            // Assert
            Assert.IsType<NotFound<object>>(result);
        }

        [Fact]
        public async Task GetManifest_IncludesRuleConditions()
        {
            // Arrange
            var productGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var countryId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            await SeedTestRuleSetWithConditions(productGroupId, countryId);

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            var ruleWithConditions = manifest.Rules.FirstOrDefault(r => r.Conditions.Any());
            Assert.NotNull(ruleWithConditions);
            Assert.NotEmpty(ruleWithConditions.Conditions);
            Assert.Equal("PalletWidth", ruleWithConditions.Conditions[0].Field);
        }

        [Fact]
        public async Task GetManifest_IncludesMatrixMetadata()
        {
            // Arrange
            var productGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var countryId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            await SeedTestRuleSet(productGroupId, countryId);
            await SeedTestMatrix();

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            Assert.NotEmpty(manifest.Matrices);
            var beamChart = manifest.Matrices.FirstOrDefault(m => m.Name == "TestBeamChart");
            Assert.NotNull(beamChart);
            Assert.Equal("LOAD_CHART", beamChart.Category);
        }

        [Fact]
        public async Task GetManifest_VersionChanges_WhenRuleSetUpdated()
        {
            // Arrange
            var productGroupId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var countryId = Guid.Parse("88888888-8888-8888-8888-888888888888");

            var ruleSetId = await SeedTestRuleSet(productGroupId, countryId);

            // Act - Get initial version
            var result1 = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);
            var manifest1 = ((Ok<RuleManifestResponse>)result1).Value;
            var version1 = manifest1.Version;

            // Update the ruleset
            await Task.Delay(1100); // Ensure timestamp difference
            await UpdateRuleSet(ruleSetId);

            // Act - Get updated version
            var result2 = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);
            var manifest2 = ((Ok<RuleManifestResponse>)result2).Value;
            var version2 = manifest2.Version;

            // Assert
            Assert.NotEqual(version1, version2);
        }

        [Fact]
        public async Task GetManifest_RulesOrderedByPriority()
        {
            // Arrange
            var productGroupId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var countryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            await SeedTestRuleSetWithPriorities(productGroupId, countryId);

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            // Rules should be ordered by priority descending (high priority first)
            for (int i = 0; i < manifest.Rules.Count - 1; i++)
            {
                Assert.True(manifest.Rules[i].Priority >= manifest.Rules[i + 1].Priority,
                    $"Rule at index {i} has priority {manifest.Rules[i].Priority}, but next rule has {manifest.Rules[i + 1].Priority}");
            }
        }

        [Fact]
        public async Task GetManifest_IncludesFormulaRules()
        {
            // Arrange
            var productGroupId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var countryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            await SeedTestRuleSetWithFormula(productGroupId, countryId);

            // Act
            var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);

            // Assert
            var okResult = Assert.IsType<Ok<RuleManifestResponse>>(result);
            var manifest = okResult.Value;

            var formulaRule = manifest.Rules.FirstOrDefault(r => !string.IsNullOrEmpty(r.Formula));
            Assert.NotNull(formulaRule);
            Assert.Contains("MATRIX_LOOKUP", formulaRule.Formula);
        }

        // Helper methods for seeding test data
        private async Task<Guid> SeedTestRuleSet(Guid productGroupId, Guid countryId)
        {
            var ruleSet = RuleSet.Create($"Test RuleSet {Guid.NewGuid()}", productGroupId, countryId, DateTime.UtcNow, null);
            ruleSet.Activate();

            var rule = Rule.Create("Test Rule", "Test Description", "SPATIAL", 100, "ERROR");
            ruleSet.AddRule(rule);

            await _ruleRepo.SaveAsync(ruleSet);
            return ruleSet.Id;
        }

        private async Task<Guid> SeedTestRuleSetWithConditions(Guid productGroupId, Guid countryId)
        {
            var ruleSet = RuleSet.Create($"Test RuleSet With Conditions {Guid.NewGuid()}", productGroupId, countryId, DateTime.UtcNow, null);
            ruleSet.Activate();

            var rule = Rule.Create("Width Check", "Validates pallet width", "SPATIAL", 100, "ERROR");
            rule.AddCondition(RuleCondition.Create(rule.Id, "AND", "PalletWidth", "GT", "1000"));
            ruleSet.AddRule(rule);

            await _ruleRepo.SaveAsync(ruleSet);
            return ruleSet.Id;
        }

        private async Task<Guid> SeedTestRuleSetWithPriorities(Guid productGroupId, Guid countryId)
        {
            var ruleSet = RuleSet.Create($"Test RuleSet With Priorities {Guid.NewGuid()}", productGroupId, countryId, DateTime.UtcNow, null);
            ruleSet.Activate();

            var highPriority = Rule.Create("High Priority", "Critical rule", "STRUCTURAL", 1, "ERROR");
            var medPriority = Rule.Create("Medium Priority", "Important rule", "SPATIAL", 50, "WARNING");
            var lowPriority = Rule.Create("Low Priority", "Info rule", "COMPLIANCE", 100, "INFO");

            ruleSet.AddRule(lowPriority);
            ruleSet.AddRule(highPriority);
            ruleSet.AddRule(medPriority);

            await _ruleRepo.SaveAsync(ruleSet);
            return ruleSet.Id;
        }

        private async Task<Guid> SeedTestRuleSetWithFormula(Guid productGroupId, Guid countryId)
        {
            var ruleSet = RuleSet.Create($"Test RuleSet With Formula {Guid.NewGuid()}", productGroupId, countryId, DateTime.UtcNow, null);
            ruleSet.Activate();

            var rule = Rule.Create("Beam Capacity Check", "Validates beam using matrix", "STRUCTURAL", 10, "ERROR");
            rule.SetFormula("MATRIX_LOOKUP('BeamChart', upright, span, profile) > load", "isBeamSafe");
            ruleSet.AddRule(rule);

            await _ruleRepo.SaveAsync(ruleSet);
            return ruleSet.Id;
        }

        private async Task SeedTestMatrix()
        {
            var matrix = LookupMatrix.Create(
                "TestBeamChart",
                "LOAD_CHART",
                "{\"uprights\": {\"ST20\": {\"HEM_80\": [{\"X\": 2700, \"Y\": 2000}]}}}"
            );

            await _matrixRepo.SaveAsync(matrix);
        }

        private async Task UpdateRuleSet(Guid ruleSetId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE rule_sets SET updated_at = NOW() WHERE id = @Id",
                new { Id = ruleSetId }
            );
        }

        public void Dispose()
        {
            // Cleanup test data
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Execute("DELETE FROM ruleset_rules WHERE ruleset_id IN (SELECT id FROM rule_sets WHERE name LIKE 'Test RuleSet%')");
            connection.Execute("DELETE FROM rule_conditions WHERE rule_id IN (SELECT id FROM rules WHERE name LIKE 'Test%' OR name LIKE '%Priority%')");
            connection.Execute("DELETE FROM rules WHERE name LIKE 'Test%' OR name LIKE '%Priority%'");
            connection.Execute("DELETE FROM rule_sets WHERE name LIKE 'Test RuleSet%'");
            connection.Execute("DELETE FROM lookup_matrices WHERE name LIKE 'Test%'");
        }
    }
}
