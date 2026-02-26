using System.Text.Json;

namespace CatalogService.Domain.Aggregates;

/// <summary>
/// LoadChart aggregate root.
/// Stores engineering specifications and metadata for product load charts.
/// </summary>
public sealed class LoadChart
{
    public Guid Id { get; private set; }
    public Guid ProductGroupId { get; private set; }
    public string ChartType { get; private set; } = default!;
    public string ComponentCode { get; private set; } = default!;
    public Guid ComponentTypeId { get; private set; }
    public Dictionary<string, JsonElement> Attributes { get; private set; } = [];
    public bool IsActive { get; private set; }
    public bool IsDelete { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties (not persisted directly)
    public string? ProductGroupName { get; private set; }
    public string? ComponentName { get; private set; }

    private LoadChart() { }

    public static LoadChart Create(
        Guid productGroupId,
        string chartType,
        string componentCode,
        Guid componentTypeId,
        Dictionary<string, JsonElement>? attributes = null,
        Guid? createdBy = null)
    {
        if (productGroupId == Guid.Empty) throw new ArgumentException("Product Group ID is required.", nameof(productGroupId));
        if (string.IsNullOrWhiteSpace(chartType)) throw new ArgumentException("Chart Type is required.", nameof(chartType));
        if (string.IsNullOrWhiteSpace(componentCode)) throw new ArgumentException("Component Code is required.", nameof(componentCode));
        if (componentTypeId == Guid.Empty) throw new ArgumentException("Component Type ID is required.", nameof(componentTypeId));

        return new LoadChart
        {
            Id = Guid.NewGuid(),
            ProductGroupId = productGroupId,
            ChartType = chartType.Trim(),
            ComponentCode = componentCode.Trim().ToUpperInvariant(),
            ComponentTypeId = componentTypeId,
            Attributes = attributes ?? [],
            IsActive = true,
            IsDelete = false,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        Guid productGroupId,
        string chartType,
        string componentCode,
        Guid componentTypeId,
        Dictionary<string, JsonElement>? attributes,
        bool isActive,
        Guid? updatedBy)
    {
        if (productGroupId == Guid.Empty) throw new ArgumentException("Product Group ID is required.", nameof(productGroupId));
        if (string.IsNullOrWhiteSpace(chartType)) throw new ArgumentException("Chart Type is required.", nameof(chartType));
        if (string.IsNullOrWhiteSpace(componentCode)) throw new ArgumentException("Component Code is required.", nameof(componentCode));
        if (componentTypeId == Guid.Empty) throw new ArgumentException("Component Type ID is required.", nameof(componentTypeId));

        ProductGroupId = productGroupId;
        ChartType = chartType.Trim();
        ComponentCode = componentCode.Trim().ToUpperInvariant();
        ComponentTypeId = componentTypeId;
        Attributes = attributes ?? [];
        IsActive = isActive;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(Guid? updatedBy)
    {
        IsDelete = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public static LoadChart Rehydrate(
        Guid id,
        Guid productGroupId,
        string chartType,
        string componentCode,
        Guid componentTypeId,
        Dictionary<string, JsonElement> attributes,
        bool isActive,
        bool isDelete,
        Guid? createdBy,
        Guid? updatedBy,
        DateTime createdAt,
        DateTime? updatedAt,
        string? productGroupName = null,
        string? componentName = null)
    {
        return new LoadChart
        {
            Id = id,
            ProductGroupId = productGroupId,
            ChartType = chartType,
            ComponentCode = componentCode,
            ComponentTypeId = componentTypeId,
            Attributes = attributes ?? [],
            IsActive = isActive,
            IsDelete = isDelete,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ProductGroupName = productGroupName,
            ComponentName = componentName
        };
    }
}
