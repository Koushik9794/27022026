# Dynamic Rule Messages - Parameterization Guide

## 🎯 Problem Statement

**Current Issue**: Rule messages have hardcoded values

```json
{
  "id": "rule-2",
  "field": "levels",
  "operator": "gt",
  "value": 8,
  "message": "More than 8 levels may require additional structural review."
}
```

**Problem**: If you change `value` to 10, the message still says "8 levels"

---

## ✅ Solution: Message Templates with Placeholders

### Approach 1: Simple Placeholder Replacement (Recommended)

#### Step 1: Update Rule Entity

```csharp
// Domain/Entities/Rule.cs
public class Rule
{
    public string Name { get; internal set; }
    public string MessageTemplate { get; internal set; }  // ← Template with placeholders
    
    // Generate actual message by replacing placeholders
    public string GenerateMessage(Dictionary<string, object> context)
    {
        var message = MessageTemplate;
        
        // Replace {field}, {value}, {actualValue}, etc.
        foreach (var kvp in context)
        {
            message = message.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        
        return message;
    }
}
```

#### Step 2: Update RuleCondition

```csharp
// Domain/Entities/RuleCondition.cs
public class RuleCondition
{
    public string Field { get; internal set; }
    public string Operator { get; internal set; }
    public string Value { get; internal set; }
    
    // Get context for message generation
    public Dictionary<string, object> GetMessageContext(object actualValue)
    {
        return new Dictionary<string, object>
        {
            { "field", GetFieldDisplayName() },
            { "threshold", Value },
            { "actualValue", actualValue },
            { "operator", GetOperatorDisplayName() }
        };
    }
    
    private string GetFieldDisplayName()
    {
        return Field switch
        {
            "levels" => "Number of Levels",
            "palletWidth" => "Pallet Width",
            "frameDepth" => "Frame Depth",
            _ => Field
        };
    }
    
    private string GetOperatorDisplayName()
    {
        return Operator switch
        {
            "GT" => "greater than",
            "LT" => "less than",
            "GTE" => "at least",
            "LTE" => "at most",
            "EQ" => "equal to",
            _ => Operator
        };
    }
}
```

#### Step 3: Update Rule Evaluation

```csharp
// Domain/Services/RuleEvaluationService.cs
public async Task<RuleOutcome> EvaluateRuleAsync(Rule rule, Dictionary<string, object> configuration)
{
    if (rule.Conditions.Any())
    {
        foreach (var condition in rule.Conditions)
        {
            var actualValue = configuration.GetValueOrDefault(condition.Field);
            var passed = EvaluateCondition(condition, actualValue);
            
            if (!passed)
            {
                // Generate dynamic message
                var context = condition.GetMessageContext(actualValue);
                var message = rule.GenerateMessage(context);
                
                return new RuleOutcome
                {
                    Passed = false,
                    Message = message,
                    Severity = rule.Severity,
                    Data = new Dictionary<string, object>
                    {
                        { "field", condition.Field },
                        { "expectedValue", condition.Value },
                        { "actualValue", actualValue }
                    }
                };
            }
        }
    }
    
    return new RuleOutcome { Passed = true };
}
```

---

## 📝 Message Template Examples

### Example 1: Levels Rule

**Database Storage**:
```json
{
  "name": "Maximum Levels Check",
  "messageTemplate": "More than {threshold} levels may require additional structural review. Current: {actualValue} levels.",
  "conditions": [
    {
      "field": "levels",
      "operator": "GT",
      "value": "8"
    }
  ]
}
```

**Runtime Evaluation**:
```csharp
// User has 10 levels
var context = new Dictionary<string, object>
{
    { "threshold", "8" },
    { "actualValue", "10" }
};

var message = rule.GenerateMessage(context);
// Output: "More than 8 levels may require additional structural review. Current: 10 levels."
```

**If threshold changes to 12**:
```json
{
  "conditions": [
    {
      "field": "levels",
      "operator": "GT",
      "value": "12"  // ← Changed
    }
  ]
}
```

**New message**:
```
"More than 12 levels may require additional structural review. Current: 10 levels."
```

---

### Example 2: Pallet Width Rule

**Template**:
```json
{
  "messageTemplate": "{field} must be {operator} {threshold}mm. Current value: {actualValue}mm."
}
```

**Evaluation**:
```csharp
var context = new Dictionary<string, object>
{
    { "field", "Pallet Width" },
    { "operator", "at least" },
    { "threshold", "650" },
    { "actualValue", "600" }
};

// Output: "Pallet Width must be at least 650mm. Current value: 600mm."
```

---

### Example 3: Complex Message with Calculations

**Template**:
```json
{
  "messageTemplate": "{field} exceeds maximum by {difference}mm ({percentage}% over limit)."
}
```

**Evaluation**:
```csharp
var actualValue = 1600;
var threshold = 1500;
var difference = actualValue - threshold;
var percentage = ((double)difference / threshold) * 100;

var context = new Dictionary<string, object>
{
    { "field", "Frame Depth" },
    { "difference", difference },
    { "percentage", percentage.ToString("F1") }
};

// Output: "Frame Depth exceeds maximum by 100mm (6.7% over limit)."
```

---

## 🔧 Implementation in Manifest API

### Update Manifest Response

```csharp
// Application/Messages/RuleManifestResponse.cs
public class RuleManifestResponse
{
    public string Version { get; set; }
    public List<RuleDto> Rules { get; set; }
}

public class RuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string MessageTemplate { get; set; }  // ← Template with placeholders
    public string Category { get; set; }
    public string Severity { get; set; }
    public int Priority { get; set; }
    public List<RuleConditionDto> Conditions { get; set; }
    
    // Optional: Pre-generated message for common case
    public string DefaultMessage { get; set; }
}

public class RuleConditionDto
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
    public string FieldDisplayName { get; set; }  // ← Human-readable field name
}
```

### Manifest Endpoint Response

```json
{
  "version": "20260125.1016",
  "rules": [
    {
      "id": "rule-2",
      "name": "Maximum Levels Check",
      "messageTemplate": "More than {threshold} levels may require additional structural review. Current: {actualValue} levels.",
      "category": "STRUCTURAL",
      "severity": "WARNING",
      "priority": 50,
      "conditions": [
        {
          "field": "levels",
          "operator": "GT",
          "value": "8",
          "fieldDisplayName": "Number of Levels"
        }
      ],
      "defaultMessage": "More than 8 levels may require additional structural review."
    }
  ]
}
```

---

## 🎨 Frontend Usage

### React Component Example

```jsx
function RuleValidator({ configuration }) {
  const [violations, setViolations] = useState([]);
  
  const evaluateRules = (rules) => {
    const results = [];
    
    rules.forEach(rule => {
      rule.conditions.forEach(condition => {
        const actualValue = configuration[condition.field];
        const passed = evaluateCondition(condition, actualValue);
        
        if (!passed) {
          // Generate dynamic message
          const message = rule.messageTemplate
            .replace('{threshold}', condition.value)
            .replace('{actualValue}', actualValue)
            .replace('{field}', condition.fieldDisplayName);
          
          results.push({
            ruleId: rule.id,
            message: message,
            severity: rule.severity,
            field: condition.field
          });
        }
      });
    });
    
    setViolations(results);
  };
  
  return (
    <div className="rule-violations">
      {violations.map(v => (
        <Alert key={v.ruleId} severity={v.severity}>
          {v.message}
        </Alert>
      ))}
    </div>
  );
}
```

**Output**:
```
⚠️ More than 8 levels may require additional structural review. Current: 10 levels.
❌ Pallet Width must be at least 650mm. Current value: 600mm.
```

---

## 🚀 Advanced: Multi-Language Support

### Approach 2: Localized Message Templates

```csharp
// Domain/Entities/Rule.cs
public class Rule
{
    public Dictionary<string, string> MessageTemplates { get; internal set; } = new();
    
    public string GenerateMessage(Dictionary<string, object> context, string language = "en")
    {
        var template = MessageTemplates.GetValueOrDefault(language) 
                    ?? MessageTemplates.GetValueOrDefault("en") 
                    ?? "Rule violation";
        
        foreach (var kvp in context)
        {
            template = template.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        
        return template;
    }
}
```

**Database Storage**:
```json
{
  "messageTemplates": {
    "en": "More than {threshold} levels may require additional structural review. Current: {actualValue} levels.",
    "de": "Mehr als {threshold} Ebenen erfordern möglicherweise eine zusätzliche Strukturprüfung. Aktuell: {actualValue} Ebenen.",
    "fr": "Plus de {threshold} niveaux peuvent nécessiter un examen structurel supplémentaire. Actuel: {actualValue} niveaux."
  }
}
```

---

## 📊 Database Schema Update

### Migration for Message Templates

```csharp
// Infrastructure/Migrations/M20260125001_AddMessageTemplates.cs
public class M20260125001_AddMessageTemplates : Migration
{
    public override void Up()
    {
        // Add message_template column
        Alter.Table("rules")
            .AddColumn("message_template").AsString(500).Nullable();
        
        // Migrate existing messages to templates
        Execute.Sql(@"
            UPDATE rules 
            SET message_template = CONCAT(
                name, 
                ' failed. Expected: ', 
                (SELECT value FROM rule_conditions WHERE rule_id = rules.id LIMIT 1)
            )
            WHERE message_template IS NULL
        ");
    }
    
    public override void Down()
    {
        Delete.Column("message_template").FromTable("rules");
    }
}
```

---

## ✅ Benefits

### 1. **Dynamic Values**
- Change threshold from 8 to 10 → message updates automatically
- No hardcoded values in messages

### 2. **Consistency**
- Same template format across all rules
- Easier to maintain

### 3. **Flexibility**
- Add new placeholders without code changes
- Support for calculations (difference, percentage)

### 4. **Localization Ready**
- Multi-language support
- Culture-specific formatting

### 5. **Better UX**
- Users see actual values in error messages
- More informative feedback

---

## 🎯 Recommended Placeholders

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{field}` | Human-readable field name | "Number of Levels" |
| `{threshold}` | Expected/limit value | "8" |
| `{actualValue}` | User's input value | "10" |
| `{operator}` | Comparison operator | "greater than" |
| `{difference}` | Calculated difference | "2" |
| `{percentage}` | Percentage over/under | "25%" |
| `{unit}` | Measurement unit | "mm", "kg" |
| `{min}` | Minimum allowed | "650" |
| `{max}` | Maximum allowed | "1500" |

---

## 📝 Example Rule Definitions

### Rule 1: Levels Check
```json
{
  "name": "Maximum Levels Check",
  "messageTemplate": "Configuration has {actualValue} levels, which exceeds the recommended maximum of {threshold}. Additional structural review may be required.",
  "conditions": [
    {
      "field": "levels",
      "operator": "GT",
      "value": "8"
    }
  ]
}
```

### Rule 2: Pallet Width
```json
{
  "name": "Minimum Pallet Width",
  "messageTemplate": "{field} is {actualValue}{unit}, but must be {operator} {threshold}{unit}.",
  "conditions": [
    {
      "field": "palletWidth",
      "operator": "LT",
      "value": "650"
    }
  ]
}
```

### Rule 3: Frame Depth Range
```json
{
  "name": "Frame Depth Range",
  "messageTemplate": "{field} of {actualValue}mm is outside the allowed range ({min}mm - {max}mm).",
  "conditions": [
    {
      "field": "frameDepth",
      "operator": "BETWEEN",
      "value": "500,1500"
    }
  ]
}
```

---

## 🚀 Implementation Steps

### Step 1: Update Domain Model (1 hour)
```bash
# Add MessageTemplate property to Rule entity
# Add GenerateMessage method
# Update RuleCondition with display name helpers
```

### Step 2: Create Migration (30 min)
```bash
# Add message_template column
# Migrate existing messages
```

### Step 3: Update Evaluation Service (1 hour)
```bash
# Use GenerateMessage in RuleEvaluationService
# Pass context with actual values
```

### Step 4: Update Manifest Response (30 min)
```bash
# Include messageTemplate in RuleDto
# Add fieldDisplayName to conditions
```

### Step 5: Frontend Integration (1 hour)
```bash
# Update rule evaluation to use templates
# Implement placeholder replacement
```

**Total Effort**: ~4 hours

---

## ✅ Summary

**Current Problem**: ❌ Hardcoded "8 levels" in message

**Solution**: ✅ Message templates with placeholders
- `messageTemplate`: `"More than {threshold} levels..."`
- Runtime replacement: `{threshold}` → actual value from condition
- Dynamic, maintainable, localization-ready

**Result**: Change threshold to 10 → message automatically says "10 levels"

This approach makes your rule system **fully dynamic and maintainable**! 🎯
