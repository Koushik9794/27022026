using System.Text.Json;

namespace CatalogService.Domain.Entities;

public sealed class CivilComponent
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!; // "wall-001"
    public string Name { get; private set; } = default!; // "WALL"
    public string Label { get; private set; } = default!; // "Wall"
    public string Icon { get; private set; } = default!; // "wall.svg"
    public string? Tooltip { get; private set; } // "Structural wall"
    public string Category { get; private set; } = default!; // "CIVIL"

    // "defaultElement" jsonb
    public string? DefaultElement { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    private CivilComponent() { }
    // -------------------------
    // Factory: Create
    // -------------------------
    public static CivilComponent Create(
        string code,
        string name,
        string label,
        string icon,
        string? tooltip,
        string category,
        Dictionary<string, object>? defaultElement,
        string? createdBy)
    {
        ValidateRequired(code, nameof(Code));
        ValidateRequired(name, nameof(Name));
        ValidateRequired(label, nameof(Label));
        ValidateRequired(icon, nameof(Icon));
        ValidateRequired(category, nameof(Category));
        var now = DateTimeOffset.UtcNow;
        return new CivilComponent
        {
            Id = Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Label = label.Trim(),
            Icon = icon.Trim(),
            Tooltip = tooltip?.Trim(),
            Category = category.Trim(),
            DefaultElement = defaultElement != null ? JsonSerializer.Serialize(defaultElement) : null,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            CreatedBy = createdBy,
            UpdatedAt = now,
            UpdatedBy = createdBy
        };
    }

    /// <summary>
    /// Update an existing CivilComponent from persisted data.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="code"></param>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="icon"></param>
    /// <param name="tooltip"></param>
    /// <param name="category"></param>
    /// <param name="defaultElement"></param>
    /// <param name="isActive"></param>

    /// <param name="updatedBy"></param>
    /// <returns></returns>
    ///
    public static CivilComponent Update(
        Guid id,
    string code,
    string name,
    string label,
    string icon,
    string? tooltip,
    string category,
    Dictionary<string, object>? defaultElement,
    bool isActive,
    string? updatedBy)
    {
        ValidateRequired(code, nameof(Code));
        ValidateRequired(name, nameof(Name));
        ValidateRequired(label, nameof(Label));
        ValidateRequired(icon, nameof(Icon));
        ValidateRequired(category, nameof(Category));
        var now = DateTimeOffset.UtcNow;
        return new CivilComponent
        {
            Id = id,
            Code = code.Trim(),
            Name = name.Trim(),
            Label = label.Trim(),
            Icon = icon.Trim(),
            Tooltip = tooltip?.Trim(),
            Category = category.Trim(),
            DefaultElement = defaultElement != null ? JsonSerializer.Serialize(defaultElement) : null,
            IsActive = isActive,
            IsDeleted = false,
            UpdatedAt = now,
            UpdatedBy = updatedBy
        };
    }
    // -------------------------
    // Factory: Rehydrate
    // -------------------------
    public static CivilComponent Rehydrate(
        Guid id,
        string code,
        string name,
        string label,
        string icon,
        string? tooltip,
        string category,
        string? defaultElement,
        bool isActive,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy)
    {
        return new CivilComponent
        {
            Id = id,
            Code = code,
            Name = name,
            Label = label,
            Icon = icon,
            Tooltip = tooltip,
            Category = category,
            DefaultElement = defaultElement,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }
    // -------------------------
    // Update
    // -------------------------
    public void Update(
        string code,
        string name,
        string label,
        string icon,
        string? tooltip,
        string category,
        Dictionary<string, object>? defaultElement,
        bool isActive,
        string? updatedBy)
    {
        EnsureNotDeleted();
        ValidateRequired(code, nameof(Code));
        ValidateRequired(name, nameof(Name));
        ValidateRequired(label, nameof(Label));
        ValidateRequired(icon, nameof(Icon));
        ValidateRequired(category, nameof(Category));
        Code = code.Trim();
        Name = name.Trim();
        Label = label.Trim();
        Icon = icon.Trim();
        Tooltip = tooltip?.Trim();
        Category = category.Trim();
        DefaultElement = defaultElement != null ? JsonSerializer.Serialize(defaultElement) : null;
        IsActive = isActive;

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }
    public void SoftDelete(string? updatedBy)
    {
        EnsureNotDeleted();
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }
    // -------------------------
    // Helpers
    // -------------------------
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException($"CivilComponent '{Code}' is deleted.");
    }
    private static void ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} is required.", paramName);
    }

}
