using Dapper;
using RuleService.Domain.Aggregates;
using RuleService.Domain.Entities;
using RuleService.Infrastructure.Dapper;
using System.Data;
using System.Text.Json;

#nullable disable
namespace RuleService.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper-based implementation of IRuleRepository
    /// </summary>
    public class DapperRuleRepository : IRuleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperRuleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<RuleSet> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT id, name, product_group_id, country_id, effective_from, effective_to, status, created_at, updated_at
                FROM rule_sets
                WHERE id = @Id";

            var ruleSetDto = await connection.QueryFirstOrDefaultAsync<RuleSetDto>(sql, new { Id = id });
            
            if (ruleSetDto == null)
                return null;

            var ruleSet = MapToRuleSet(ruleSetDto);
            
            // Load associated rules
            var rulesDto = await GetRulesByRuleSetIdAsync(connection, id);
            foreach (var ruleDto in rulesDto)
            {
                var rule = MapToRule(ruleDto);
                ruleSet.AddRule(rule);
            }

            return ruleSet;
        }

        public async Task<List<RuleSet>> GetActiveRuleSetsByProductGroupAndCountryAsync(Guid productGroupId, Guid countryId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT id, name, product_group_id, country_id, effective_from, effective_to, status, created_at, updated_at
                FROM rule_sets
                WHERE product_group_id = @ProductGroupId
                  AND country_id = @CountryId
                  AND status = 'ACTIVE'
                  AND (effective_to IS NULL OR effective_to > CURRENT_TIMESTAMP)
                ORDER BY created_at DESC";

            var ruleSetsDto = (await connection.QueryAsync<RuleSetDto>(sql, new { ProductGroupId = productGroupId, CountryId = countryId })).ToList();
            
            var ruleSets = new List<RuleSet>();
            foreach (var ruleSetDto in ruleSetsDto)
            {
                var ruleSet = MapToRuleSet(ruleSetDto);
                var rulesDto = await GetRulesByRuleSetIdAsync(connection, ruleSetDto.Id);
                foreach (var ruleDto in rulesDto)
                {
                    var rule = MapToRule(ruleDto);
                    ruleSet.AddRule(rule);
                }
                ruleSets.Add(ruleSet);
            }

            return ruleSets;
        }

        public async Task SaveAsync(RuleSet ruleSet)
        {
            if (ruleSet == null)
                throw new ArgumentNullException(nameof(ruleSet));

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert RuleSet
                const string insertRuleSetSql = @"
                    INSERT INTO rule_sets (id, name, product_group_id, country_id, effective_from, effective_to, status, created_at, updated_at)
                    VALUES (@Id, @Name, @ProductGroupId, @CountryId, @EffectiveFrom, @EffectiveTo, @Status, @CreatedAt, @UpdatedAt)";

                await connection.ExecuteAsync(insertRuleSetSql, new
                {
                    ruleSet.Id,
                    ruleSet.Name,
                    ruleSet.ProductGroupId,
                    ruleSet.CountryId,
                    ruleSet.EffectiveFrom,
                    ruleSet.EffectiveTo,
                    ruleSet.Status,
                    ruleSet.CreatedAt,
                    ruleSet.UpdatedAt
                }, transaction);

                // Insert Rules
                foreach (var rule in ruleSet.Rules)
                {
                    await SaveRuleAsync(connection, rule, transaction);
                    await SaveRuleToRuleSetAsync(connection, ruleSet.Id, rule.Id, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task UpdateAsync(RuleSet ruleSet)
        {
            if (ruleSet == null)
                throw new ArgumentNullException(nameof(ruleSet));

            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateSql = @"
                    UPDATE rule_sets
                    SET name = @Name, status = @Status, effective_to = @EffectiveTo, updated_at = @UpdatedAt
                    WHERE id = @Id";

                await connection.ExecuteAsync(updateSql, new
                {
                    ruleSet.Id,
                    ruleSet.Name,
                    ruleSet.Status,
                    ruleSet.EffectiveTo,
                    ruleSet.UpdatedAt
                }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = "DELETE FROM rule_sets WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        private async Task<List<RuleDto>> GetRulesByRuleSetIdAsync(IDbConnection connection, Guid ruleSetId)
        {
            const string sql = @"
                SELECT r.id, r.name, r.description, r.category, r.priority, r.severity, r.enabled, r.formula, r.message_template, r.created_at, r.updated_at
                FROM rules r
                INNER JOIN ruleset_rules rr ON r.id = rr.rule_id
                WHERE rr.ruleset_id = @RuleSetId
                ORDER BY r.priority DESC";

            var rules = (await connection.QueryAsync<RuleDto>(sql, new { RuleSetId = ruleSetId })).ToList();
            
            foreach (var rule in rules)
            {
                rule.Conditions = await GetConditionsByRuleIdAsync(connection, rule.Id);
            }

            return rules;
        }

        private async Task<List<RuleConditionDto>> GetConditionsByRuleIdAsync(IDbConnection connection, Guid ruleId)
        {
            const string sql = @"
                SELECT id, rule_id, type, field, operator, value
                FROM rule_conditions
                WHERE rule_id = @RuleId";

            return (await connection.QueryAsync<RuleConditionDto>(sql, new { RuleId = ruleId })).ToList();
        }

        private async Task SaveRuleAsync(IDbConnection connection, RuleService.Domain.Entities.Rule rule, IDbTransaction transaction)
        {
            const string insertRuleSql = @"
                INSERT INTO rules (id, name, description, category, priority, severity, enabled, formula, message_template, created_at, updated_at)
                VALUES (@Id, @Name, @Description, @Category, @Priority, @Severity, @Enabled, @Formula, @MessageTemplate, @CreatedAt, @UpdatedAt)";

            await connection.ExecuteAsync(insertRuleSql, new
            {
                rule.Id,
                rule.Name,
                rule.Description,
                rule.Category,
                rule.Priority,
                rule.Severity,
                rule.Enabled,
                rule.Formula,
                rule.MessageTemplate,
                rule.CreatedAt,
                rule.UpdatedAt
            }, transaction);

            // Insert conditions
            foreach (var condition in rule.Conditions)
            {
                const string insertConditionSql = @"
                    INSERT INTO rule_conditions (id, rule_id, type, field, operator, value)
                    VALUES (@Id, @RuleId, @Type, @Field, @Operator, @Value)";

                await connection.ExecuteAsync(insertConditionSql, new
                {
                    condition.Id,
                    condition.RuleId,
                    condition.Type,
                    condition.Field,
                    Operator = condition.Operator,
                    condition.Value
                }, transaction);
            }
        }

        private async Task SaveRuleToRuleSetAsync(IDbConnection connection, Guid ruleSetId, Guid ruleId, IDbTransaction transaction)
        {
            const string sql = @"
                INSERT INTO ruleset_rules (ruleset_id, rule_id, added_at)
                VALUES (@RuleSetId, @RuleId, @AddedAt)
                ON CONFLICT DO NOTHING";

            await connection.ExecuteAsync(sql, new
            {
                RuleSetId = ruleSetId,
                RuleId = ruleId,
                AddedAt = DateTime.UtcNow
            }, transaction);
        }

        private RuleSet MapToRuleSet(RuleSetDto dto)
        {
            var ruleSet = new RuleSet
            {
                Id = dto.Id,
                Name = dto.Name,
                ProductGroupId = dto.ProductGroupId,
                CountryId = dto.CountryId,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
            return ruleSet;
        }

        private RuleService.Domain.Entities.Rule MapToRule(RuleDto dto)
        {
            var rule = new RuleService.Domain.Entities.Rule
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority,
                Severity = dto.Severity,
                Enabled = dto.Enabled,
                Formula = dto.Formula,
                MessageTemplate = dto.MessageTemplate,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            if (dto.Conditions != null)
            {
                foreach (var condDto in dto.Conditions)
                {
                    rule.AddCondition(RuleCondition.Create(
                        rule.Id,
                        condDto.Type,
                        condDto.Field,
                        condDto.Operator,
                        condDto.Value
                    ));
                }
            }

            return rule;
        }

        // DTOs for Dapper mapping
        private class RuleSetDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Guid ProductGroupId { get; set; }
            public Guid CountryId { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        private class RuleDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public int Priority { get; set; }
            public string Severity { get; set; }
            public bool Enabled { get; set; }
            public string? Formula { get; set; }
            public string? MessageTemplate { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public List<RuleConditionDto> Conditions { get; set; } = new();
        }

        private class RuleConditionDto
        {
            public Guid Id { get; set; }
            public Guid RuleId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Field { get; set; } = string.Empty;
            public string Operator { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}
