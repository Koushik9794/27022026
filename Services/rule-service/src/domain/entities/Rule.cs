using System;
using System.Collections.Generic;
using RuleService.Domain.ValueObjects;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace RuleService.Domain.Entities
{
    /// <summary>
    /// Rule Entity - represents a single business rule
    /// </summary>
    public class Rule
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Category { get; internal set; } // SPATIAL, STRUCTURAL, ACCESSORY, PRICING, COMPLIANCE
        public int Priority { get; internal set; }
        public string Severity { get; internal set; } // ERROR, WARNING, INFO
        public bool Enabled { get; internal set; }
        public List<RuleCondition> Conditions { get; internal set; } = new();
        
        // Message template with placeholders (e.g., "More than {threshold} levels...")
        public string? MessageTemplate { get; internal set; }
        
        // Formula support properties
        public string? Formula { get; internal set; }
        public string? OutputField { get; internal set; }
        public RuleType RuleType { get; internal set; }
        public ExecutionPhase ExecutionPhase { get; internal set; }
        public bool AppliesToVariants { get; internal set; }
        public List<ParameterDefinition> Parameters { get; internal set; } = new();
        public List<Guid> DependsOnRuleIds { get; internal set; } = new();
        
        public DateTime CreatedAt { get; internal set; }
        public DateTime UpdatedAt { get; internal set; }

        internal Rule() { }

        /// <summary>
        /// Create a new Rule
        /// </summary>
        public static Rule Create(
            string name,
            string description,
            string category,
            int priority,
            string severity,
            RuleType ruleType = RuleType.Condition,
            ExecutionPhase executionPhase = ExecutionPhase.Calculation)
        {
            var rule = new Rule
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Category = category,
                Priority = priority,
                Severity = severity,
                Enabled = true,
                RuleType = ruleType,
                ExecutionPhase = executionPhase,
                AppliesToVariants = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return rule;
        }
        
        /// <summary>
        /// Set formula for calculation rules
        /// </summary>
        public void SetFormula(string formula, string outputField)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new ArgumentException("Formula cannot be empty", nameof(formula));
            if (string.IsNullOrWhiteSpace(outputField))
                throw new ArgumentException("Output field cannot be empty", nameof(outputField));
                
            Formula = formula;
            OutputField = outputField;
            RuleType = RuleType.Formula;
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Add a parameter definition
        /// </summary>
        public void AddParameter(ParameterDefinition parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));
                
            Parameters.Add(parameter);
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Add a rule dependency
        /// </summary>
        public void AddDependency(Guid dependsOnRuleId)
        {
            if (!DependsOnRuleIds.Contains(dependsOnRuleId))
            {
                DependsOnRuleIds.Add(dependsOnRuleId);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Add a condition to the rule
        /// </summary>
        public void AddCondition(RuleCondition condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            Conditions.Add(condition);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Enable or disable the rule
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Set the message template with placeholders
        /// </summary>
        public void SetMessageTemplate(string messageTemplate)
        {
            if (string.IsNullOrWhiteSpace(messageTemplate))
                throw new ArgumentException("Message template cannot be empty", nameof(messageTemplate));
                
            MessageTemplate = messageTemplate;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Generate a message by replacing placeholders with actual values
        /// Supported placeholders: {field}, {threshold}, {actualValue}, {operator}, {min}, {max}, {unit}
        /// </summary>
        public string GenerateMessage(Dictionary<string, object?> context)
        {
            if (string.IsNullOrWhiteSpace(MessageTemplate))
                return Name; // Fallback to rule name if no template
                
            var message = MessageTemplate;
            
            // Replace all placeholders with context values
            foreach (var kvp in context)
            {
                if (kvp.Value != null)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value.ToString());
                }
            }
            
            return message;
        }
    }
}
