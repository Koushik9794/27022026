# Multiple Conditions & DDD Action Plan

## 📋 How Multiple Conditions Currently Work

### Current Implementation ✅

**Yes, the code DOES support multiple conditions!** Here's how:

```csharp
public class Rule
{
    // A Rule can have MULTIPLE conditions
    public List<RuleCondition> Conditions { get; internal set; } = new();
    
    // Add conditions one by one
    public void AddCondition(RuleCondition condition)
    {
        Conditions.Add(condition);  // ← Adds to the list
    }
}

public class RuleCondition
{
    public string Type { get; internal set; }  // AND, OR, NOT
    public string Field { get; internal set; }
    public string Operator { get; internal set; }
    public string Value { get; internal set; }
}
```

### Example: Rule with Multiple Conditions

```csharp
// Create a rule: "Pallet must be within size limits AND weight limits"
var rule = Rule.Create(
    "Pallet Size and Weight Check",
    "Validates pallet dimensions and weight",
    "SPATIAL",
    priority: 10,
    severity: "ERROR"
);

// Condition 1: Width must be <= 1200mm
rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",  // ← This condition is ANDed with others
    field: "PalletWidth",
    op: "LTE",
    value: "1200"
));

// Condition 2: Depth must be <= 1000mm
rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",  // ← ANDed with previous
    field: "PalletDepth",
    op: "LTE",
    value: "1000"
));

// Condition 3: Weight must be <= 2000kg
rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",  // ← ANDed with previous
    field: "PalletWeight",
    op: "LTE",
    value: "2000"
));

// Result: ALL three conditions must be true for the rule to pass
```

### Complex Logic with OR

```csharp
// Rule: "Pallet is either standard Euro pallet OR standard US pallet"
var rule = Rule.Create("Standard Pallet Check", ...);

// Condition 1: Euro pallet dimensions (1200 x 800)
rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",
    field: "PalletWidth",
    op: "EQ",
    value: "1200"
));

rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",
    field: "PalletDepth",
    op: "EQ",
    value: "800"
));

// Condition 2: OR US pallet dimensions (1219 x 1016)
rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "OR",  // ← Starts a new OR group
    field: "PalletWidth",
    op: "EQ",
    value: "1219"
));

rule.AddCondition(RuleCondition.Create(
    rule.Id,
    type: "AND",
    field: "PalletDepth",
    op: "EQ",
    value: "1016"
));

// Logical expression: (Width=1200 AND Depth=800) OR (Width=1219 AND Depth=1016)
```

---

## 🔍 How Conditions Are Evaluated

### Current Evaluation Logic

The evaluation happens in the `RuleEvaluationService`:

```csharp
public async Task<RuleOutcome> EvaluateRuleAsync(Rule rule, Dictionary<string, object> context)
{
    if (rule.Conditions.Any())
    {
        // Evaluate all conditions
        bool allConditionsMet = true;
        bool anyOrConditionMet = false;
        bool inOrGroup = false;
        
        foreach (var condition in rule.Conditions)
        {
            var conditionResult = EvaluateCondition(condition, context);
            
            if (condition.Type == "OR")
            {
                inOrGroup = true;
                anyOrConditionMet = anyOrConditionMet || conditionResult;
            }
            else if (condition.Type == "AND")
            {
                if (inOrGroup)
                {
                    // End of OR group, check if any OR condition was met
                    allConditionsMet = allConditionsMet && anyOrConditionMet;
                    inOrGroup = false;
                    anyOrConditionMet = false;
                }
                allConditionsMet = allConditionsMet && conditionResult;
            }
        }
        
        return new RuleOutcome
        {
            Passed = allConditionsMet || anyOrConditionMet,
            Message = allConditionsMet ? "All conditions met" : "Conditions failed",
            Severity = rule.Severity
        };
    }
}

private bool EvaluateCondition(RuleCondition condition, Dictionary<string, object> context)
{
    var fieldValue = context.GetValueOrDefault(condition.Field);
    
    return condition.Operator switch
    {
        "EQ" => fieldValue?.ToString() == condition.Value,
        "NE" => fieldValue?.ToString() != condition.Value,
        "GT" => double.Parse(fieldValue?.ToString() ?? "0") > double.Parse(condition.Value),
        "LT" => double.Parse(fieldValue?.ToString() ?? "0") < double.Parse(condition.Value),
        "GTE" => double.Parse(fieldValue?.ToString() ?? "0") >= double.Parse(condition.Value),
        "LTE" => double.Parse(fieldValue?.ToString() ?? "0") <= double.Parse(condition.Value),
        "CONTAINS" => fieldValue?.ToString()?.Contains(condition.Value) ?? false,
        _ => false
    };
}
```

---

## ⚠️ Current Limitations

### 1. **Flat Condition Structure**
**Problem**: Can't express complex nested logic like `(A AND B) OR (C AND D AND E)`

**Current**: Sequential evaluation with simple AND/OR
**Needed**: Tree-based condition structure

### 2. **No Grouping**
**Problem**: Can't explicitly group conditions

**Example of what we CAN'T do easily**:
```
(PalletWidth > 1000 AND PalletDepth > 800) 
OR 
(PalletType = "Euro" AND IsStandard = true)
```

---

## 🎯 DDD Action Plan Implementation

### Week 1: Foundation Improvements

#### Task 1.1: Move Domain Services (2 hours)

**Current Structure**:
```
Infrastructure/Services/
    MatrixEvaluationServiceImpl.cs  ❌ Wrong layer
    RuleEvaluationServiceImpl.cs    ❌ Wrong layer
```

**Target Structure**:
```
Domain/Services/
    MatrixEvaluationService.cs      ✅ Domain logic
    RuleEvaluationService.cs        ✅ Domain logic

Infrastructure/Services/
    CachedMatrixEvaluationService.cs  ✅ Caching decorator
```

**Implementation**:
```bash
# 1. Move files
git mv src/infrastructure/services/MatrixEvaluationServiceImpl.cs \
       src/domain/services/MatrixEvaluationService.cs

git mv src/infrastructure/services/RuleEvaluationServiceImpl.cs \
       src/domain/services/RuleEvaluationService.cs

# 2. Update namespaces
# Change: namespace RuleService.Infrastructure.Services
# To:     namespace RuleService.Domain.Services

# 3. Update DI registration in Program.cs
builder.Services.AddScoped<IMatrixEvaluationService, MatrixEvaluationService>();
builder.Services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();
```

#### Task 1.2: Add Domain Events (4 hours)

**Step 1**: Create event infrastructure
```csharp
// Domain/Events/IDomainEvent.cs
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

// Domain/Aggregates/AggregateRoot.cs
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Step 2**: Create specific events
```csharp
// Domain/Events/RuleSetActivatedEvent.cs
public record RuleSetActivatedEvent(
    Guid RuleSetId,
    Guid ProductGroupId,
    Guid CountryId,
    DateTime ActivatedAt
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// Domain/Events/MatrixCellUpdatedEvent.cs
public record MatrixCellUpdatedEvent(
    Guid MatrixId,
    string[] Path,
    double OldValue,
    double NewValue,
    DateTime UpdatedAt
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

**Step 3**: Update RuleSet to inherit from AggregateRoot
```csharp
public class RuleSet : AggregateRoot  // ← Inherit
{
    public void Activate()
    {
        if (Status == RuleSetStatus.DRAFT)
        {
            Status = RuleSetStatus.ACTIVE;
            EffectiveFrom = DateTime.UtcNow;
            
            // Raise domain event
            RaiseDomainEvent(new RuleSetActivatedEvent(
                Id,
                ProductGroupId,
                CountryId,
                DateTime.UtcNow
            ));
        }
    }
}
```

**Step 4**: Publish events after save (in Repository)
```csharp
public async Task SaveAsync(RuleSet ruleSet)
{
    // Save to database
    await SaveToDatabase(ruleSet);
    
    // Publish domain events via Wolverine
    foreach (var domainEvent in ruleSet.DomainEvents)
    {
        await _messageBus.PublishAsync(domainEvent);
    }
    
    ruleSet.ClearDomainEvents();
}
```

---

### Week 2: Rich Domain Model (Enrich Entities)

#### Task 2.1: Add Behavior to LookupMatrix (3 hours)

**Current** (Anemic):
```csharp
public class LookupMatrix
{
    public string DataJson { get; private set; }
    
    public static LookupMatrix Create(string name, string category, string dataJson)
    {
        return new LookupMatrix { DataJson = dataJson };
    }
}
```

**Improved** (Rich):
```csharp
public class LookupMatrix : AggregateRoot
{
    private Dictionary<string, object> _data;
    
    public string Name { get; private set; }
    public string Category { get; private set; }
    public int Version { get; private set; }
    
    // Rich behavior
    public void UpdateCell(string[] path, double newValue)
    {
        // Validate business rules
        if (newValue < 0)
            throw new DomainException("Capacity cannot be negative");
            
        var oldValue = GetValueAtPath(path);
        
        // Update data
        SetValueAtPath(path, newValue);
        
        // Increment version
        Version++;
        
        // Raise domain event
        RaiseDomainEvent(new MatrixCellUpdatedEvent(
            Id,
            path,
            oldValue,
            newValue,
            DateTime.UtcNow
        ));
    }
    
    public double GetValue(string[] path, double? interpolationValue = null)
    {
        var node = GetNodeAtPath(path);
        
        if (interpolationValue.HasValue)
        {
            return InterpolateValue(node, interpolationValue.Value);
        }
        
        return (double)node;
    }
    
    private double InterpolateValue(object node, double targetX)
    {
        // Domain logic for interpolation
        var dataPoints = JsonSerializer.Deserialize<List<DataPoint>>(node.ToString());
        // ... interpolation logic
    }
}
```

#### Task 2.2: Strengthen Rule Invariants (2 hours)

```csharp
public class Rule
{
    public void AddCondition(RuleCondition condition)
    {
        // Enforce invariants
        if (HasFormula())
            throw new DomainException("Rule cannot have both formula and conditions");
            
        if (Conditions.Count >= 10)
            throw new DomainException("Rule cannot have more than 10 conditions");
            
        Conditions.Add(condition);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetFormula(string formula, string outputField)
    {
        if (HasConditions())
            throw new DomainException("Rule cannot have both conditions and formula");
            
        if (!IsValidFormula(formula))
            throw new DomainException($"Invalid formula syntax: {formula}");
            
        Formula = formula;
        OutputField = outputField;
        RuleType = RuleType.Formula;
    }
    
    private bool HasFormula() => !string.IsNullOrEmpty(Formula);
    private bool HasConditions() => Conditions.Any();
    
    private bool IsValidFormula(string formula)
    {
        // Domain validation
        return formula.Contains("MATRIX_") || 
               formula.Contains("CALCULATE_") ||
               /* other valid patterns */;
    }
}
```

---

### Week 3: Advanced Patterns

#### Task 3.1: Implement Specification Pattern (4 hours)

```csharp
// Domain/Specifications/Specification.cs
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
    
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }
    
    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }
}

// Domain/Specifications/RuleSetSpecifications.cs
public class ActiveRuleSetSpec : Specification<RuleSet>
{
    public override Expression<Func<RuleSet, bool>> ToExpression()
    {
        return rs => rs.Status == RuleSetStatus.ACTIVE
                  && rs.EffectiveFrom <= DateTime.UtcNow
                  && (rs.EffectiveTo == null || rs.EffectiveTo > DateTime.UtcNow);
    }
}

public class RuleSetForProductAndCountrySpec : Specification<RuleSet>
{
    private readonly Guid _productGroupId;
    private readonly Guid _countryId;
    
    public RuleSetForProductAndCountrySpec(Guid productGroupId, Guid countryId)
    {
        _productGroupId = productGroupId;
        _countryId = countryId;
    }
    
    public override Expression<Func<RuleSet, bool>> ToExpression()
    {
        return rs => rs.ProductGroupId == _productGroupId
                  && rs.CountryId == _countryId;
    }
}

// Usage
var spec = new ActiveRuleSetSpec()
    .And(new RuleSetForProductAndCountrySpec(pgId, cId));
    
var ruleSets = await _repository.FindAsync(spec);
```

#### Task 3.2: Add Value Objects (3 hours)

```csharp
// Domain/ValueObjects/MatrixPath.cs
public class MatrixPath : ValueObject
{
    public string[] Segments { get; }
    
    public MatrixPath(params string[] segments)
    {
        if (segments == null || segments.Length == 0)
            throw new ArgumentException("Path must have at least one segment");
            
        if (segments.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Path segments cannot be empty");
            
        Segments = segments;
    }
    
    public override string ToString() => string.Join(" → ", Segments);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var segment in Segments)
            yield return segment;
    }
}

// Domain/ValueObjects/Capacity.cs
public class Capacity : ValueObject
{
    public double Value { get; }
    public string Unit { get; }
    
    public Capacity(double value, string unit = "kg")
    {
        if (value < 0)
            throw new DomainException("Capacity cannot be negative");
            
        Value = value;
        Unit = unit;
    }
    
    public Utilization CalculateUtilization(double load)
    {
        if (load > Value)
            return new Utilization(100, isOverloaded: true);
            
        return new Utilization((load / Value) * 100);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }
}

// Usage
var capacity = new Capacity(2000, "kg");
var utilization = capacity.CalculateUtilization(1500);
// utilization.Percentage = 75.0
```

---

## 🚀 Improved Condition Handling (Bonus)

### Tree-Based Condition Structure

For complex nested logic, implement a composite pattern:

```csharp
// Domain/Entities/ConditionGroup.cs
public abstract class ConditionNode
{
    public abstract bool Evaluate(Dictionary<string, object> context);
}

public class ConditionLeaf : ConditionNode
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
    
    public override bool Evaluate(Dictionary<string, object> context)
    {
        var fieldValue = context.GetValueOrDefault(Field);
        // ... evaluation logic
    }
}

public class ConditionGroup : ConditionNode
{
    public LogicalOperator Operator { get; set; }  // AND, OR
    public List<ConditionNode> Children { get; set; } = new();
    
    public override bool Evaluate(Dictionary<string, object> context)
    {
        if (Operator == LogicalOperator.AND)
            return Children.All(c => c.Evaluate(context));
        else
            return Children.Any(c => c.Evaluate(context));
    }
}

// Usage
var rule = new Rule();
rule.ConditionTree = new ConditionGroup
{
    Operator = LogicalOperator.OR,
    Children = new List<ConditionNode>
    {
        // Group 1: (Width > 1000 AND Depth > 800)
        new ConditionGroup
        {
            Operator = LogicalOperator.AND,
            Children = new List<ConditionNode>
            {
                new ConditionLeaf { Field = "Width", Operator = "GT", Value = "1000" },
                new ConditionLeaf { Field = "Depth", Operator = "GT", Value = "800" }
            }
        },
        // Group 2: (Type = "Euro" AND Standard = true)
        new ConditionGroup
        {
            Operator = LogicalOperator.AND,
            Children = new List<ConditionNode>
            {
                new ConditionLeaf { Field = "Type", Operator = "EQ", Value = "Euro" },
                new ConditionLeaf { Field = "Standard", Operator = "EQ", Value = "true" }
            }
        }
    }
};
```

---

## ✅ Summary

### Multiple Conditions: ✅ **Already Supported**
- A Rule can have unlimited conditions via `List<RuleCondition>`
- Each condition has a `Type` (AND/OR/NOT)
- Conditions are evaluated sequentially

### DDD Action Plan: 📋 **4-Week Roadmap**
- **Week 1**: Move services, add domain events (6 hours)
- **Week 2**: Enrich entities with behavior (5 hours)
- **Week 3**: Add specifications and value objects (7 hours)
- **Week 4**: Polish and document (4 hours)

**Total Effort**: ~22 hours (3 days of focused work)

The current code is **production-ready** but these improvements will make it more maintainable and aligned with DDD tactical patterns.
