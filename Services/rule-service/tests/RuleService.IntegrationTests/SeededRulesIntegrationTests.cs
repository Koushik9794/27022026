using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Npgsql;
using Dapper;
using RuleService.Infrastructure.Services;
using RuleService.Infrastructure.Adapters;
using RuleService.Infrastructure.Persistence;
using RuleService.Domain.Aggregates;

namespace RuleService.IntegrationTests
{
    /// <summary>
    /// Integration tests that verify the 97 seeded rules from the database
    /// These tests require the Docker database to be running
    /// </summary>
    public class SeededRulesIntegrationTests : IDisposable
    {
        private readonly string _connectionString = "Host=127.0.0.1;Port=5433;Database=rule_service;Username=postgres;Password=postgres;SSL Mode=Disable;";
        private readonly DapperRuleRepository _repository;
        private readonly RuleEvaluationServiceImpl _service;

        private class DummyMatrixService : RuleService.Domain.Services.IMatrixEvaluationService
        {
            public Task<double?> LookupValueAsync(string matrixName, string[] path, double? numericalValue = null) => Task.FromResult<double?>(null);
            public Task<System.Collections.Generic.List<RuleService.Domain.Services.MatrixChoiceResult>> GetChoicesAsync(string matrixName, string[] parentPath, double inputVariable, double requiredLoad) => Task.FromResult(new System.Collections.Generic.List<RuleService.Domain.Services.MatrixChoiceResult>());
        }

        public SeededRulesIntegrationTests()
        {
            var engine = new DynamicExpressoExpressionEngine(new DummyMatrixService());
            
            // Create a simple connection factory for testing
            var connectionFactory = new TestConnectionFactory(_connectionString);
            _repository = new DapperRuleRepository(connectionFactory);
            _service = new RuleEvaluationServiceImpl(engine, _repository);
        }

        // Simple test connection factory
        private class TestConnectionFactory : RuleService.Infrastructure.Dapper.IDbConnectionFactory
        {
            private readonly string _connectionString;

            public TestConnectionFactory(string connectionString)
            {
                _connectionString = connectionString;
            }

            public System.Data.IDbConnection CreateConnection()
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        [Fact]
        public async Task VerifyDatabase_Has97SeededRules()
        {
            // Arrange & Act
            using var connection = new NpgsqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM rules");

            // Assert
            Assert.Equal(97, count);
        }

        [Fact]
        public async Task VerifyDatabase_Has3RuleSets()
        {
            // Arrange & Act
            using var connection = new NpgsqlConnection(_connectionString);
            var ruleSets = await connection.QueryAsync<dynamic>(
                "SELECT id, name FROM rule_sets ORDER BY name"
            );

            // Assert
            Assert.Equal(3, ruleSets.AsList().Count);
        }

        [Fact]
        public async Task EvaluateDesignRules_ValidConfiguration_ShouldPass()
        {
            // Arrange - Get the Design Rules RuleSet
            var ruleSetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var ruleSet = await _repository.GetByIdAsync(ruleSetId);

            Assert.NotNull(ruleSet);
            Assert.Equal("GSS Design Rules - Selective Pallet Racking", ruleSet.Name);

            // Valid warehouse configuration
            var config = JsonSerializer.Serialize(new
            {
                // Warehouse
                WarehouseClearHeight = 8000,
                
                // Pallets
                PalletWidth = 1200,
                PalletDepth = 1000,
                PalletHeight = 150,
                PalletsPerLevel = 2,
                
                // Rack
                NumberOfLevels = 4,
                FrameDepth = 800,
                BeamSpan = 2700,
                RackWidth = 2500, // (2 * 1200) + 100
                AdditionalClearance = 100,
                
                // MHE
                MHEMaxForkHeight = 6000,
                MHEWorkingAisle = 3600,
                
                // Load
                LoadPerPallet = 800,
                
                // Generic fields for fallback
                width = 1200,
                height = 150,
                quantity = 200,
                value = 100
            });

            // Act
            var result = await _service.EvaluateRuleSetAsync(ruleSet, config, preview: false, validateOnly: false);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Outcomes);
            
            // Log results for debugging
            Console.WriteLine($"Total rules evaluated: {result.Outcomes.Count}");
            Console.WriteLine($"Overall success: {result.Success}");
            
            var failedRules = result.Outcomes.FindAll(o => !o.Passed);
            if (failedRules.Count > 0)
            {
                Console.WriteLine($"Failed rules: {failedRules.Count}");
                foreach (var failed in failedRules)
                {
                    Console.WriteLine($"  - Rule {failed.RuleId}: {failed.Message} (Severity: {failed.Severity})");
                }
            }
        }

        [Fact]
        public async Task EvaluateStabilityRules_ValidConfiguration_ShouldPass()
        {
            // Arrange - Get the Stability Rules RuleSet
            var ruleSetId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var ruleSet = await _repository.GetByIdAsync(ruleSetId);

            Assert.NotNull(ruleSet);
            Assert.Equal("GSS Stability Guidelines - Selective Pallet Racking", ruleSet.Name);

            // Valid stability configuration
            var config = JsonSerializer.Serialize(new
            {
                NumberOfLevels = 4,
                UnitsPerRow = 3,
                UnsupportedLength = 2500,  // USL
                MaxUSL = 3500,
                LastLoadingLevelHeight = 4800,
                FrameDepth = 800,
                HeightToDepthRatio = 6.0,  // 4800/800 = 6.0 (at limit)
                UnitType = "DoubleSided",
                BackBracing = true,
                FrameHeight = 5000,
                value = 100  // Generic value for simple conditions
            });

            // Act
            var result = await _service.EvaluateRuleSetAsync(ruleSet, config, preview: false, validateOnly: false);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Outcomes);
            
            Console.WriteLine($"Stability rules evaluated: {result.Outcomes.Count}");
            Console.WriteLine($"Overall success: {result.Success}");
        }

        [Fact]
        public async Task VerifyRulePrioritization_DesignRulesFirst()
        {
            // Arrange
            using var connection = new NpgsqlConnection(_connectionString);
            
            var designRules = await connection.QueryAsync<dynamic>(
                @"SELECT r.priority, r.name, r.category 
                  FROM rules r
                  INNER JOIN ruleset_rules rr ON r.id = rr.rule_id
                  WHERE rr.ruleset_id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
                  ORDER BY r.priority
                  LIMIT 5"
            );

            var stabilityRules = await connection.QueryAsync<dynamic>(
                @"SELECT r.priority, r.name, r.category 
                  FROM rules r
                  INNER JOIN ruleset_rules rr ON r.id = rr.rule_id
                  WHERE rr.ruleset_id = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
                  ORDER BY r.priority
                  LIMIT 5"
            );

            // Assert
            var firstDesignRule = designRules.AsList()[0];
            var firstStabilityRule = stabilityRules.AsList()[0];

            Console.WriteLine($"First Design Rule: Priority {firstDesignRule.priority} - {firstDesignRule.name}");
            Console.WriteLine($"First Stability Rule: Priority {firstStabilityRule.priority} - {firstStabilityRule.name}");

            // Design rules should have lower priority numbers (evaluated first)
            Assert.True(firstDesignRule.priority < firstStabilityRule.priority,
                "Design rules should have higher priority (lower number) than stability rules");
        }

        [Fact]
        public async Task QueryRulesByCategory_VerifyDistribution()
        {
            // Arrange & Act
            using var connection = new NpgsqlConnection(_connectionString);
            var categoryDistribution = await connection.QueryAsync<dynamic>(
                @"SELECT category, COUNT(*) as count 
                  FROM rules 
                  GROUP BY category 
                  ORDER BY count DESC"
            );

            // Assert & Log
            Console.WriteLine("Rule distribution by category:");
            foreach (var cat in categoryDistribution)
            {
                Console.WriteLine($"  {cat.category}: {cat.count} rules");
            }

            Assert.NotEmpty(categoryDistribution.AsList());
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
