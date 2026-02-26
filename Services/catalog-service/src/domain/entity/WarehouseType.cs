namespace CatalogService.Domain.Entities;

using System.Text.Json;
public sealed class WarehouseType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public string Icon { get; private set; } = default!;
    public string? Tooltip { get; private set; }
    public string? templatePath_Civil { get; private set; }
    public string? templatePath_Json { get; private set; } 
    // jsonb attributes referencing existing attribute definitions (e.g. {"DEPTH": 1200})
    public string? Attributes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    private WarehouseType() { }
    // -------------------------
    // Factory: Create
    // -------------------------
    public static WarehouseType Create(
        string name,
        string label,
        string icon,
        string? tooltip,
        string? TemplatePath_Civil,
        string? TemplatePath_Json,
        Dictionary<string, object>? attributes,
        string? createdBy)
    {
        ValidateName(name);
        ValidateLabel(label);
        ValidateIcon(icon);
        var now = DateTimeOffset.UtcNow;
        return new WarehouseType
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Label = label.Trim(),
            Icon = icon.Trim(),
            Tooltip = tooltip?.Trim(),
            templatePath_Civil = TemplatePath_Civil?.Trim(),
            templatePath_Json = TemplatePath_Json?.Trim(),
            Attributes = attributes != null ? JsonSerializer.Serialize(attributes) : null,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy,
            UpdatedAt = now,
            UpdatedBy = createdBy
        };
    }
    // -------------------------
    // Update
    // -------------------------
    public void Update(
        string name,
        string label,
        string icon,
        string? tooltip,
        string? TemplatePath_Civil,
        string? TemplatePath_Json,
        Dictionary<string, object>? attributes,
        bool isActive,
        string? updatedBy)
    {
        EnsureNotDeleted();
        ValidateName(name);
        ValidateLabel(label);
        ValidateIcon(icon);
        Name = name.Trim();
        Label = label.Trim();
        Icon = icon.Trim();
        Tooltip = tooltip?.Trim();
        templatePath_Civil = TemplatePath_Civil?.Trim();
        templatePath_Json = TemplatePath_Json?.Trim();
        Attributes = attributes != null ? JsonSerializer.Serialize(attributes) : null;
        IsActive = isActive;

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    public static WarehouseType Rehydrate(
       Guid id,
       string name,
       string label,
       string icon,
       string? tooltip,
        string? TemplatePath_Civil,
        string? TemplatePath_Json,
       string? attributes,
       bool isActive,
       bool isDeleted,
       DateTimeOffset createdAt,
       string? createdBy,
       DateTimeOffset? updatedAt,
       string? updatedBy)
    {
        ValidateName(name);
        ValidateLabel(label);
        ValidateIcon(icon);
        return new WarehouseType
        {
            Id = id,
            Name = name.Trim(),
            Label = label.Trim(),
            Icon = icon.Trim(),
            Tooltip = tooltip?.Trim(),
            templatePath_Civil = TemplatePath_Civil?.Trim(),
            templatePath_Json = TemplatePath_Json?.Trim(),
            Attributes = attributes,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }
    // -------------------------
    // State transitions
    // -------------------------
    public void Delete(string? updatedBy)
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException($"WarehouseType '{Name}' is deleted and cannot be modified.");
    }
    // -------------------------
    // Validation helpers
    // -------------------------
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
    }
    private static void ValidateLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required.", nameof(label));
    }
    private static void ValidateIcon(string icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
            throw new ArgumentException("Icon is required.", nameof(icon));
    }
    // -------------------------
    // JsonElement cloning helper
    // -------------------------
    private static JsonElement? Clone(JsonElement? element)
    {
        if (element is null) return null;
        var e = element.Value;
        if (e.ValueKind == JsonValueKind.Null) return e;
        using var doc = JsonDocument.Parse(e.GetRawText());
        return doc.RootElement.Clone();
    }
}
