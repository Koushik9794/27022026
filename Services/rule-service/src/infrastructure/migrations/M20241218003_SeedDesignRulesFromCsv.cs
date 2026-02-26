using FluentMigrator;
using System;
using System.IO;
using System.Linq;

namespace RuleService.Infrastructure.Migrations
{
    /// <summary>
    /// Seeds design rules from GSS_DesignRules_Extraction.csv
    /// </summary>
    [Migration(20241218003)]
    public class SeedDesignRulesFromCsv : Migration
    {
        private readonly Guid _productGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private readonly Guid _countryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private readonly Guid _ruleSetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"); // New RuleSet for design rules

        public override void Up()
        {
            Console.WriteLine("=== Starting CSV Design Rules Migration ===");
            
            // Create RuleSet for Design Rules
            Insert.IntoTable("rule_sets").Row(new
            {
                id = _ruleSetId,
                name = "GSS Design Rules - Selective Pallet Racking",
                product_group_id = _productGroupId,
                country_id = _countryId,
                effective_from = DateTime.UtcNow,
                effective_to = (DateTime?)null,
                status = "ACTIVE",
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });
            Console.WriteLine($"Created RuleSet: {_ruleSetId}");

            // CSV Path - try multiple locations
            var csvPaths = new[]
            {
                Path.Combine("..", "..", "..", "..", "docs", "requirements", "GSS_DesignRules_Extraction.csv"),
                "/app/GSS_DesignRules_Extraction.csv",
                "GSS_DesignRules_Extraction.csv",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GSS_DesignRules_Extraction.csv")
            };

            string? csvPath = null;
            foreach (var path in csvPaths)
            {
                Console.WriteLine($"Checking CSV path: {path}");
                if (File.Exists(path))
                {
                    csvPath = path;
                    Console.WriteLine($"✓ Found CSV at: {csvPath}");
                    break;
                }
            }

            if (csvPath == null)
            {
                Console.WriteLine("ERROR: CSV file not found in any of the expected locations:");
                foreach (var path in csvPaths)
                {
                    Console.WriteLine($"  - {path}");
                }
                Console.WriteLine("Skipping design rules seeding.");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            Console.WriteLine($"Read {lines.Length} lines from CSV (including header)");
            var priority = 1;
            var rulesProcessed = 0;
            var rulesSkipped = 0;

            foreach (var line in lines.Skip(1)) // Skip header
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    rulesSkipped++;
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length < 5)
                {
                    Console.WriteLine($"Skipping line (insufficient columns): {line.Substring(0, Math.Min(50, line.Length))}...");
                    rulesSkipped++;
                    continue;
                }

                var group = parts[0].Trim();
                var ruleId = parts[1].Trim();
                var name = parts[2].Trim();
                var description = parts[3].Trim();
                var formula = parts[4].Trim();

                if (string.IsNullOrEmpty(ruleId) || string.IsNullOrEmpty(name))
                {
                    Console.WriteLine($"Skipping line (missing ID or name): Group={group}, ID={ruleId}, Name={name}");
                    rulesSkipped++;
                    continue;
                }

                var category = DetermineCategory(group);
                var severity = DetermineSeverity(formula, description);
                var guid = Guid.NewGuid();

                try
                {
                    // Insert Rule
                    Insert.IntoTable("rules").Row(new
                    {
                        id = guid,
                        name = $"{ruleId} - {name}",
                        description = description,
                        category = category,
                        priority = priority++,
                        severity = severity,
                        enabled = true,
                        formula = formula, // Store original formula
                        created_at = DateTime.UtcNow,
                        updated_at = DateTime.UtcNow
                    });

                    // Parse and insert conditions from formula
                    InsertConditionsFromFormula(guid, formula);

                    // Link rule to ruleset
                    Insert.IntoTable("ruleset_rules").Row(new
                    {
                        ruleset_id = _ruleSetId,
                        rule_id = guid,
                        added_at = DateTime.UtcNow
                    });

                    // Insert version
                    Insert.IntoTable("rule_versions").Row(new
                    {
                        id = Guid.NewGuid(),
                        rule_id = guid,
                        version_number = 1,
                        change_log = "Initial import from CSV",
                        rule_definition = formula,
                        created_at = DateTime.UtcNow,
                        created_by = "System"
                    });

                    rulesProcessed++;
                    
                    if (rulesProcessed % 10 == 0)
                    {
                        Console.WriteLine($"Processed {rulesProcessed} rules so far...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR processing rule {ruleId}: {ex.Message}");
                    rulesSkipped++;
                }
            }

            Console.WriteLine($"=== CSV Migration Complete ===");
            Console.WriteLine($"Total rules processed: {rulesProcessed}");
            Console.WriteLine($"Total rules skipped: {rulesSkipped}");
        }

        private string DetermineCategory(string group)
        {
            if (string.IsNullOrEmpty(group)) return "STRUCTURAL";

            if (group.Contains("Level") || group.Contains("Height")) return "SPATIAL";
            if (group.Contains("Width") || group.Contains("Depth")) return "SPATIAL";
            if (group.Contains("Beam") || group.Contains("Frame")) return "STRUCTURAL";
            if (group.Contains("MHE")) return "ACCESSORY";
            if (group.Contains("Pallet")) return "COMPLIANCE";
            
            return "STRUCTURAL";
        }

        private string DetermineSeverity(string formula, string description)
        {
            var text = (formula + " " + description).ToLower();
            
            if (text.Contains("must") || text.Contains("required") || text.Contains("invalid"))
                return "ERROR";
            if (text.Contains("should") || text.Contains("warning"))
                return "WARNING";
            
            return "INFO";
        }

        private void InsertConditionsFromFormula(Guid ruleId, string formula)
        {
            // Simple condition extraction - can be enhanced
            if (formula.Contains(">=") || formula.Contains("≥"))
            {
                Insert.IntoTable("rule_conditions").Row(new
                {
                    id = Guid.NewGuid(),
                    rule_id = ruleId,
                    type = "AND",
                    field = "value",
                    @operator = "GTE",
                    value = "0"
                });
            }
            else if (formula.Contains("<=") || formula.Contains("≤"))
            {
                Insert.IntoTable("rule_conditions").Row(new
                {
                    id = Guid.NewGuid(),
                    rule_id = ruleId,
                    type = "AND",
                    field = "value",
                    @operator = "LTE",
                    value = "0"
                });
            }
            else if (formula.Contains(">"))
            {
                Insert.IntoTable("rule_conditions").Row(new
                {
                    id = Guid.NewGuid(),
                    rule_id = ruleId,
                    type = "AND",
                    field = "value",
                    @operator = "GT",
                    value = "0"
                });
            }
            else if (formula.Contains("<"))
            {
                Insert.IntoTable("rule_conditions").Row(new
                {
                    id = Guid.NewGuid(),
                    rule_id = ruleId,
                    type = "AND",
                    field = "value",
                    @operator = "LT",
                    value = "0"
                });
            }
            else if (formula.Contains("="))
            {
                Insert.IntoTable("rule_conditions").Row(new
                {
                    id = Guid.NewGuid(),
                    rule_id = ruleId,
                    type = "AND",
                    field = "value",
                    @operator = "EQ",
                    value = "0"
                });
            }
        }

        public override void Down()
        {
            // Delete all rules associated with this ruleset
            Execute.Sql($"DELETE FROM ruleset_rules WHERE ruleset_id = '{_ruleSetId}'");
            Execute.Sql($"DELETE FROM rule_versions WHERE rule_id IN (SELECT rule_id FROM ruleset_rules WHERE ruleset_id = '{_ruleSetId}')");
            Execute.Sql($"DELETE FROM rule_conditions WHERE rule_id IN (SELECT rule_id FROM ruleset_rules WHERE ruleset_id = '{_ruleSetId}')");
            Execute.Sql($"DELETE FROM rules WHERE id IN (SELECT rule_id FROM ruleset_rules WHERE ruleset_id = '{_ruleSetId}')");
            Delete.FromTable("rule_sets").Row(new { id = _ruleSetId });
        }
    }
}
