using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using RuleService.Domain.Aggregates;
using RuleService.Domain.Services;
using RuleService.Domain.ValueObjects;
using RuleService.Infrastructure.Adapters;
using RuleService.Infrastructure.Persistence;

namespace RuleService.Infrastructure.Services
{
    public class RuleEvaluationServiceImpl : IRuleEvaluationService
    {
        private readonly IExpressionEngineAdapter _engine;
        private readonly IRuleRepository _repository;

        public RuleEvaluationServiceImpl(IExpressionEngineAdapter engine, IRuleRepository repository)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RuleEvaluationResult> EvaluateRuleSetAsync(RuleSet ruleSet, string configurationData, bool preview = false, bool validateOnly = false)
        {
            if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));

            var variables = ParseConfiguration(configurationData);
            var ctx = new RuleEvaluationContext
            {
                ProductGroupId = ruleSet.ProductGroupId,
                CountryId = ruleSet.CountryId,
                InputParameters = variables.ToDictionary(k => k.Key, v => v.Value ?? new object())
            };

            var result = new RuleEvaluationResult
            {
                RuleSetId = ruleSet.Id,
                Success = true,
                EvaluatedAt = DateTime.UtcNow
            };

            // Evaluate in priority order (higher priority first)
            var orderedRules = ruleSet.Rules.OrderByDescending(r => r.Priority).ToList();
            foreach (var rule in orderedRules)
            {
                // Use Formula if available (for complex CSV rules), otherwise build from Conditions
                var expr = !string.IsNullOrEmpty(rule.Formula) 
                    ? PrepareFormulaForEvaluation(rule.Formula)
                    : BuildExpressionFromConditions(rule.Conditions);

                try
                {
                    if (validateOnly)
                    {
                        // Attempt to evaluate/compile expression with empty vars to validate syntax
                        await _engine.EvaluateAsync(expr, new Dictionary<string, object?>());
                        var outcomeValid = RuleOutcome.Create(rule.Id, true, "Valid", rule.Severity);
                        result.Outcomes.Add(outcomeValid);
                        continue;
                    }

                    var eval = await _engine.EvaluateAsync(expr, variables, ctx);
                    var passed = false;
                    if (eval is bool b) passed = b;
                    else if (eval != null && bool.TryParse(eval.ToString(), out var parsed)) passed = parsed;
                    else if (eval == null && (expr.Contains("ADD_BOM") || expr.Contains("VALIDATE"))) passed = true; // Heuristic for action rules

                    // Check if violations were added to context
                    if (ctx.Violations.Any(v => v.RuleId == Guid.Empty)) 
                    {
                        // Assign rule id to violations that don't have one
                        foreach(var v in ctx.Violations.Where(v => v.RuleId == Guid.Empty)) v.RuleId = rule.Id;
                    }

                    var outcome = RuleOutcome.Create(rule.Id, passed, passed ? "Passed" : "Failed", rule.Severity);
                    
                    // Attach context data (violations, BOM items) to outcome if relevant
                    if (ctx.BomItems.Any()) 
                    {
                        outcome.Data["BomItems"] = ctx.BomItems.ToList();
                        ctx.BomItems.Clear(); // Clear for next rule
                    }

                    result.Outcomes.Add(outcome);
                    if (!passed) result.Success = false;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Outcomes.Add(RuleOutcome.Create(rule.Id, false, $"Error evaluating rule: {ex.Message}", rule.Severity));
                }
            }

            return result;
        }

        public async Task<RuleOutcome> EvaluateExpressionAsync(RuleExpression expression, object context)
        {
            var variables = context as IDictionary<string, object?> ?? new Dictionary<string, object?>();
            var eval = await _engine.EvaluateAsync(expression.Expression, variables);
            var passed = false;
            if (eval is bool b) passed = b;
            else if (eval != null && bool.TryParse(eval.ToString(), out var parsed)) passed = parsed;

            return RuleOutcome.Create(Guid.Empty, passed, passed ? "Passed" : "Failed", "INFO");
        }

        private IDictionary<string, object?> ParseConfiguration(string configurationData)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(configurationData)) return dict;

            try
            {
                using var doc = JsonDocument.Parse(configurationData);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        dict[prop.Name] = JsonElementToObject(prop.Value);
                    }
                }
            }
            catch
            {
                // Ignore parse errors - return empty vars
            }

            return dict;
        }

        private object? JsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number when element.TryGetInt64(out var l) => l,
                JsonValueKind.Number when element.TryGetDouble(out var d) => d,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }

        private string BuildExpressionFromConditions(List<RuleService.Domain.Entities.RuleCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return "false";

            // Combine conditions with AND by default
            var parts = new List<string>();
            foreach (var c in conditions)
            {
                var op = MapOperator(c.Operator);
                var val = c.Value;
                // If value is numeric, keep as-is, otherwise quote
                if (double.TryParse(val, out _))
                {
                    parts.Add($"{c.Field} {op} {val}");
                }
                else
                {
                    // escape quotes
                    var escaped = val?.Replace("\"", "\\\"") ?? string.Empty;
                    if (op == "CONTAINS")
                        parts.Add($"{c.Field} != null && {c.Field}.ToString().Contains(\"{escaped}\")");
                    else
                        parts.Add($"{c.Field} {op} \"{escaped}\"");
                }
            }

            return string.Join(" && ", parts);
        }

        private string MapOperator(string op)
        {
            return op?.ToUpperInvariant() switch
            {
                "EQ" => "==",
                "NE" => "!=",
                "LT" => "<",
                "GT" => ">",
                "LTE" => "<=",
                "GTE" => ">=",
                "CONTAINS" => "CONTAINS",
                _ => op ?? "=="
            };
        }

        private string PrepareFormulaForEvaluation(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula)) return "true";

            // 1. Clean units from numbers (e.g., 1200mm -> 1200, 800kg -> 800)
            // Be careful not to match variables that might end in these letters
            var cleaned = System.Text.RegularExpressions.Regex.Replace(formula, @"(\d+)(mm|kg|m|cm|inch|lbs)", "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 2. Handle Excel-style IF(condition, trueResult, falseResult)
            // We'll recursively replace IFs with C# ternary ?: operator
            while (cleaned.Contains("IF(", StringComparison.OrdinalIgnoreCase))
            {
                var start = cleaned.IndexOf("IF(", StringComparison.OrdinalIgnoreCase);
                var end = FindClosingParenthesis(cleaned, start + 2);
                if (end == -1) break;

                var inner = cleaned.Substring(start + 3, end - (start + 3));
                var parts = SplitIgnoringNestedParentheses(inner, ',');

                if (parts.Length >= 2)
                {
                    var condition = FixComparisons(parts[0].Trim());
                    var truePart = FixComparisons(parts[1].Trim());
                    var falsePart = parts.Length > 2 ? FixComparisons(parts[2].Trim()) : "true";

                    var ternary = $"({condition} ? {truePart} : {falsePart})";
                    cleaned = cleaned.Remove(start, end - start + 1).Insert(start, ternary);
                }
                else
                {
                    // Invalid IF format
                    break;
                }
            }

            // 3. Handle simple assignments interpreted as equality checks (e.g., "A = B" -> "A == B")
            // But only if it's not already a complex expression with comparison ops
            cleaned = FixComparisons(cleaned);

            return cleaned;
        }

        private string FixComparisons(string expr)
        {
            // If it's a simple assignment "X = Y", change to "X == Y"
            // except if it's already got ==, !=, >=, <=, >, <
            if (expr.Contains("=") && !expr.Contains("==") && !expr.Contains("!=") && 
                !expr.Contains(">=") && !expr.Contains("<=") && !expr.Contains(">") && !expr.Contains("<"))
            {
                return expr.Replace("=", "==");
            }
            return expr;
        }

        private int FindClosingParenthesis(string str, int startIndex)
        {
            int counter = 0;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] == '(') counter++;
                else if (str[i] == ')')
                {
                    if (counter == 0) return i;
                    counter--;
                }
            }
            return -1;
        }

        private string[] SplitIgnoringNestedParentheses(string str, char separator)
        {
            var result = new List<string>();
            int counter = 0;
            int lastStart = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '(') counter++;
                else if (str[i] == ')') counter--;
                else if (str[i] == separator && counter == 0)
                {
                    result.Add(str.Substring(lastStart, i - lastStart));
                    lastStart = i + 1;
                }
            }
            result.Add(str.Substring(lastStart));
            return result.ToArray();
        }
    }
}
