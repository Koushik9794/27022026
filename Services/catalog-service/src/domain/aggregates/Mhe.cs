namespace CatalogService.Domain.Aggregates;

using System.Security.AccessControl;
using System.Text.Json;
/// <summary>
/// MHE (Material Handling Equipment) aggregate root.
/// Represents material handling equipment in the warehouse catalog.
/// </summary>

public sealed class Mhe
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    public string? Manufacturer { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }

    public string? MheType { get; private set; }
    public string? MheCategory { get; private set; }

    public string? GlbFilePath { get; private set; }

    /// <summary>
    /// JSONB attributes. Must be a JSON object, not array/string.
    /// Defaults to {}.
    /// </summary>
    public Dictionary<string, JsonElement> Attributes { get; private set; } = [];

    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Private constructor for EF Core / Dapper
    private Mhe() { }

    /// <summary>
    /// Factory method to create a new MHE.
    /// </summary>
    public static Mhe Create(
        string code,
        string name,
        string? manufacturer,
        string? brand,
        string? model,
        string? mheType,
        string? mheCategory,
        string? glbFilePath,
        Dictionary<string, JsonElement> Attributes,
        bool isActive = true,
        string? createdBy = null)
    {
        ValidateCode(code);
        ValidateName(name);

        ValidateText(manufacturer, 255, nameof(manufacturer));
        ValidateText(brand, 255, nameof(brand));
        ValidateText(model, 255, nameof(model));
        ValidateText(mheType, 255, nameof(mheType));
        ValidateText(mheCategory, 255, nameof(mheCategory));
        ValidateText(glbFilePath, 1000, nameof(glbFilePath));

        var now = DateTimeOffset.UtcNow;

        return new Mhe
        {
            Id = Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Manufacturer= manufacturer,
            Brand = brand,
            Model = model,
            MheType = mheType,
            MheCategory = mheCategory,
            GlbFilePath = glbFilePath,
            Attributes = Attributes ?? [],
            IsActive = isActive,
            IsDeleted = false,

            CreatedAt = now,
            CreatedBy = createdBy,
            UpdatedAt = now,
            UpdatedBy = createdBy
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static Mhe Rehydrate(
        Guid id,
        string code,
        string name,
        string? manufacturer,
        string? brand,
        string? model,
        string? mheType,
        string? mheCategory,
        string? glbFilePath,
        Dictionary<string, JsonElement> Attributes,
        bool isActive,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy)
    {
        ValidateCode(code);
        ValidateName(name);

        return new Mhe
        {
            Id = id,
            Code = code,
            Name = name,
            Manufacturer = manufacturer,
            Brand = brand,
            Model = model,
            MheType = mheType,
            MheCategory = mheCategory,
            GlbFilePath = glbFilePath,
            Attributes = Attributes ?? [],
            IsActive = isActive,
            IsDeleted = isDeleted,

            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    public void Update(string code, string name, string? manufacturer,
        string? brand,
        string? model,
        string? mheType,
        string? mheCategory,
        string? glbFilePath,
        Dictionary<string, JsonElement> attributes, bool isActive, string? updatedBy)
    {
        EnsureNotDeleted();
        ValidateCode(code);
        ValidateName(name);

        ValidateText(manufacturer, 255, nameof(manufacturer));
        ValidateText(brand, 255, nameof(brand));
        ValidateText(model, 255, nameof(model));
        ValidateText(mheType, 255, nameof(mheType));
        ValidateText(mheCategory, 255, nameof(mheCategory));
        ValidateText(glbFilePath, 1000, nameof(glbFilePath));

        Code = code.Trim();
        Name = name.Trim();
                                                                                      Manufacturer = manufacturer;
        Brand = brand;
        Model = model;
        MheType = mheType;
        MheCategory = mheCategory;
        GlbFilePath = glbFilePath;
        Attributes = attributes ?? [];
        IsActive = isActive && !IsDeleted;
        UpdatedAt = DateTimeOffset.UtcNow; 
        UpdatedBy = updatedBy;
    }

    public void Activate(string? updatedBy)
    {
        EnsureNotDeleted();
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string? updatedBy)
    {
        EnsureNotDeleted();
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

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
        {
            throw new InvalidOperationException($"MHE {Code} is deleted and cannot be modified.");
        }
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MHE code cannot be empty.", nameof(code));
        }

        if (code.Trim().Length > 50)
        {
            throw new ArgumentException("MHE code cannot exceed 50 characters.", nameof(code));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("MHE name cannot be empty.", nameof(name));
        }

        if (name.Trim().Length > 200)
        {
            throw new ArgumentException("MHE name cannot exceed 200 characters.", nameof(name));
        }
    }

    private static void ValidateText(string? value, int maxLength, string paramName)
    {
        if (value is null) return;

        if (value.Trim().Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }
    }
}
