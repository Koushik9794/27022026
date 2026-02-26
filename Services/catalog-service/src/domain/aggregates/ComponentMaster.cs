using System.Text.Json;

namespace CatalogService.Domain.Aggregates;

/// <summary>
/// ComponentMaster aggregate root.
/// Represents a specific component master detail in the system.
/// Mirror of Part entity.
/// </summary>
public sealed class ComponentMaster
{
    public Guid Id { get; private set; }
    public string ComponentMasterCode { get; private set; } = default!;
    public string CountryCode { get; private set; } = default!;
    public string? UnspscCode { get; private set; }
    public Guid ComponentGroupId { get; private set; }
    public Guid ComponentTypeId { get; private set; }
    public Guid? ComponentNameId { get; private set; }
    
    public string? Colour { get; private set; }
    public string? PowderCode { get; private set; }
    public bool GfaFlag { get; private set; }
    
    public decimal UnitBasicPrice { get; private set; }
    public decimal? Cbm { get; private set; }
    
    public string? ShortDescription { get; private set; }
    public string? Description { get; private set; }
    public string? DrawingNo { get; private set; }
    public string? RevNo { get; private set; }
    public string? InstallationRefNo { get; private set; }
    
    /// <summary>
    /// Flexible attributes stored as JSON.
    /// </summary>
    public Dictionary<string, JsonElement> Attributes { get; private set; } = [];
    
    public string? GlbFilepath { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Status { get; private set; } = "ACTIVE"; // ACTIVE, INACTIVE
    public bool IsDeleted { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Navigation properties (rehydration only)
    public string? ComponentGroupCode { get; private set; }
    public string? ComponentGroupName { get; private set; }
    public string? ComponentTypeCode { get; private set; }
    public string? ComponentTypeName { get; private set; }
    public string? ComponentNameCode { get; private set; }
    public string? ComponentNameName { get; private set; }

    private ComponentMaster() { }

    public static ComponentMaster Create(
        string componentMasterCode,
        string countryCode,
        Guid componentGroupId,
        Guid componentTypeId,
        decimal unitBasicPrice,
        string? unspscCode = null,
        Guid? componentNameId = null,
        string? colour = null,
        string? powderCode = null,
        bool gfaFlag = false,
        decimal? cbm = null,
        string? shortDescription = null,
        string? description = null,
        string? drawingNo = null,
        string? revNo = null,
        string? installationRefNo = null,
        Dictionary<string, JsonElement>? attributes = null,
        string? glbFilepath = null,
        string? imageUrl = null,
        string status = "ACTIVE",
        string? createdBy = null)
    {
        ValidateComponentMasterCode(componentMasterCode);
        ValidateCountryCode(countryCode);
        
        if (componentGroupId == Guid.Empty) throw new ArgumentException("Component Group ID is required.", nameof(componentGroupId));
        if (componentTypeId == Guid.Empty) throw new ArgumentException("Component Type ID is required.", nameof(componentTypeId));
        if (unitBasicPrice < 0) throw new ArgumentException("Unit Basic Price cannot be negative.", nameof(unitBasicPrice));

        return new ComponentMaster
        {
            Id = Guid.NewGuid(),
            ComponentMasterCode = componentMasterCode.Trim(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            UnspscCode = unspscCode?.Trim(),
            ComponentGroupId = componentGroupId,
            ComponentTypeId = componentTypeId,
            ComponentNameId = componentNameId,
            Colour = colour?.Trim(),
            PowderCode = powderCode?.Trim(),
            GfaFlag = gfaFlag,
            UnitBasicPrice = unitBasicPrice,
            Cbm = cbm,
            ShortDescription = shortDescription?.Trim(),
            Description = description?.Trim(),
            DrawingNo = drawingNo?.Trim(),
            RevNo = revNo?.Trim(),
            InstallationRefNo = installationRefNo?.Trim(),
            Attributes = attributes ?? [],
            GlbFilepath = glbFilepath?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            Status = status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = createdBy
        };
    }

    public void Update(
        string? unspscCode,
        Guid componentGroupId,
        Guid componentTypeId,
        Guid? componentNameId,
        string? colour,
        string? powderCode,
        bool gfaFlag,
        decimal unitBasicPrice,
        decimal? cbm,
        string? shortDescription,
        string? description,
        string? drawingNo,
        string? revNo,
        string? installationRefNo,
        Dictionary<string, JsonElement>? attributes,
        string? glbFilepath,
        string? imageUrl,
        string status,
        string? updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException($"ComponentMaster {ComponentMasterCode} is deleted and cannot be modified.");
        
        if (componentGroupId == Guid.Empty) throw new ArgumentException("Component Group ID is required.", nameof(componentGroupId));
        if (componentTypeId == Guid.Empty) throw new ArgumentException("Component Type ID is required.", nameof(componentTypeId));
        if (unitBasicPrice < 0) throw new ArgumentException("Unit Basic Price cannot be negative.", nameof(unitBasicPrice));

        UnspscCode = unspscCode?.Trim();
        ComponentGroupId = componentGroupId;
        ComponentTypeId = componentTypeId;
        ComponentNameId = componentNameId;
        Colour = colour?.Trim();
        PowderCode = powderCode?.Trim();
        GfaFlag = gfaFlag;
        UnitBasicPrice = unitBasicPrice;
        Cbm = cbm;
        ShortDescription = shortDescription?.Trim();
        Description = description?.Trim();
        DrawingNo = drawingNo?.Trim();
        RevNo = revNo?.Trim();
        InstallationRefNo = installationRefNo?.Trim();
        Attributes = attributes ?? [];
        
        if (glbFilepath != null) GlbFilepath = glbFilepath;
        if (imageUrl != null) ImageUrl = imageUrl;
        
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Delete(string? deletedBy)
    {
        IsDeleted = true;
        Status = "INACTIVE";
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    public static ComponentMaster Rehydrate(
        Guid id,
        string componentMasterCode,
        string countryCode,
        string? unspscCode,
        Guid componentGroupId,
        Guid componentTypeId,
        Guid? componentNameId,
        string? colour,
        string? powderCode,
        bool gfaFlag,
        decimal unitBasicPrice,
        decimal? cbm,
        string? shortDescription,
        string? description,
        string? drawingNo,
        string? revNo,
        string? installationRefNo,
        Dictionary<string, JsonElement> attributes,
        string? glbFilepath,
        string? imageUrl,
        string status,
        bool isDeleted,
        DateTime createdAt,
        DateTime? updatedAt,
        string? createdBy,
        string? updatedBy,
        string? componentGroupCode = null,
        string? componentGroupName = null,
        string? componentTypeCode = null,
        string? componentTypeName = null,
        string? componentNameCode = null,
        string? componentNameName = null)
    {
        return new ComponentMaster
        {
            Id = id,
            ComponentMasterCode = componentMasterCode,
            CountryCode = countryCode,
            UnspscCode = unspscCode,
            ComponentGroupId = componentGroupId,
            ComponentTypeId = componentTypeId,
            ComponentNameId = componentNameId,
            Colour = colour,
            PowderCode = powderCode,
            GfaFlag = gfaFlag,
            UnitBasicPrice = unitBasicPrice,
            Cbm = cbm,
            ShortDescription = shortDescription,
            Description = description,
            DrawingNo = drawingNo,
            RevNo = revNo,
            InstallationRefNo = installationRefNo,
            Attributes = attributes ?? [],
            GlbFilepath = glbFilepath,
            ImageUrl = imageUrl,
            Status = status,
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            ComponentGroupCode = componentGroupCode,
            ComponentGroupName = componentGroupName,
            ComponentTypeCode = componentTypeCode,
            ComponentTypeName = componentTypeName,
            ComponentNameCode = componentNameCode,
            ComponentNameName = componentNameName
        };
    }

    private static void ValidateComponentMasterCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Component Master Code cannot be empty.", nameof(code));
        if (code.Length > 100) throw new ArgumentException("Component Master Code cannot exceed 100 characters.", nameof(code));
    }

    private static void ValidateCountryCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Country Code cannot be empty.", nameof(code));
        if (code.Length != 2) throw new ArgumentException("Country Code must be exactly 2 characters (ISO-2).", nameof(code));
    }
}
