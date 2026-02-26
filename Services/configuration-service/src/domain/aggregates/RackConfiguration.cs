using System;
using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

public sealed class RackConfiguration
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public JsonDocument ConfigurationLayout { get; private set; } = default!;
    public string ProductCode { get; private set; } = default!;
    public string Scope { get; private set; } = default!; // ENQUIRY, PERSONAL, GLOBAL
    public Guid? EnquiryId { get; private set; }
    public bool IsApprovedByAdmin { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedOn { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedOn { get; private set; }
    public string? UpdatedBy { get; private set; }

    private RackConfiguration() { }

    public static RackConfiguration Create(
        string name,
        JsonDocument configurationLayout,
        string productCode,
        string scope,
        Guid? enquiryId,
        string? createdBy,
        bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (configurationLayout == null)
            throw new ArgumentException("Configuration layout cannot be null.", nameof(configurationLayout));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code cannot be empty.", nameof(productCode));
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Scope cannot be empty.", nameof(scope));

        // Validate Scope logic
        scope = scope.ToUpperInvariant();
        if (scope == "ENQUIRY" && enquiryId == null)
             throw new ArgumentException("Enquiry ID is required for ENQUIRY scope.", nameof(enquiryId));

        if (scope == "PERSONAL" || scope == "GLOBAL")
            enquiryId = null;

        var config = new RackConfiguration
        {
            Id = Guid.NewGuid(),
            Name = name,
            ConfigurationLayout = configurationLayout,
            ProductCode = productCode,
            Scope = scope,
            EnquiryId = enquiryId,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
            IsApprovedByAdmin = false
        };

        if (scope == "GLOBAL")
        {
            if (isAdmin)
            {
                config.IsApprovedByAdmin = true;
                config.ApprovedOn = DateTime.UtcNow;
                config.ApprovedBy = createdBy;
            }
            else
            {
                // Logic based on requirements: "If not approved by Admin, it should automatically fall back to Personal"
                // However, the requirement says "Save Config As Global". It usually implies requesting approval.
                // The requirement says: "If not approved by Admin, it should automatically fall back to Personal".
                // This implies that perhaps we save it as PERSONAL first? Or save as GLOBAL but IsApproved=false?
                // Re-reading: "Example: User 1 -> creates Global config (not approved) -> scope=PERSONAL".
                // "Save As Global (approved) -> scope=GLOBAL".
                // So if not admin, we force scope to PERSONAL? But maybe mark it for approval?
                // The requirements table example shows:
                // "Save As Global (not approved) -> scope=PERSONAL"
                // "Save As Global (approved) -> scope=GLOBAL"
                // So if the user is NOT an admin, we simply save it as PERSONAL scope.

                // Wait, if it saves as PERSONAL, how does it become GLOBAL later?
                // Maybe the intent is that non-admins CANNOT create GLOBAL scope directly?
                // Or maybe they request it?
                // The requirement says: "Save Config As Global ... This is allowed only after Admin approval. If not approved by Admin, it should automatically fall back to Personal".
                // This suggests that the ACTION is "Save As Global", but the RESULT is "Personal" (until approved?).
                // But if scope=PERSONAL, it's just Personal.
                // I will implement strictly as per the "Button Stored As" table:
                // Button: Save As Global (not approved) -> Scope=PERSONAL.
                
                // So effectively, if user !isAdmin, scope becomes PERSONAL.
                config.Scope = "PERSONAL";
            }
        }

        return config;
    }

    public void Update(string name, JsonDocument configurationLayout, string productCode, string scope, Guid? enquiryId, string? updatedBy, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (configurationLayout == null)
            throw new ArgumentException("Configuration layout cannot be null.", nameof(configurationLayout));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code cannot be empty.", nameof(productCode));
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Scope cannot be empty.", nameof(scope));

        scope = scope.ToUpperInvariant();
        if (scope == "ENQUIRY" && enquiryId == null)
            throw new ArgumentException("Enquiry ID is required for ENQUIRY scope.", nameof(enquiryId));

        if (scope == "PERSONAL" || scope == "GLOBAL")
            enquiryId = null;

        Name = name;
        ConfigurationLayout = configurationLayout;
        ProductCode = productCode;
        Scope = scope;
        EnquiryId = enquiryId;
        UpdatedOn = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        if (scope == "GLOBAL")
        {
            if (isAdmin)
            {
                IsApprovedByAdmin = true;
                ApprovedOn = DateTime.UtcNow;
                ApprovedBy = updatedBy;
            }
            else
            {
                // Fallback to personal if not admin
                Scope = "PERSONAL";
                IsApprovedByAdmin = false;
                ApprovedOn = null;
                ApprovedBy = null;
            }
        }
        else
        {
            IsApprovedByAdmin = false;
            ApprovedOn = null;
            ApprovedBy = null;
        }
    }

    public void Deactivate(string? updatedBy)
    {
        IsActive = false;
        UpdatedOn = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public static RackConfiguration Rehydrate(
        Guid id, string name, JsonDocument configurationLayout, string productCode, string scope, 
        Guid? enquiryId, bool isApprovedByAdmin, string? approvedBy, DateTime? approvedOn, 
        bool isActive, DateTime createdOn, string? createdBy, DateTime? updatedOn, string? updatedBy)
    {
        return new RackConfiguration
        {
            Id = id,
            Name = name,
            ConfigurationLayout = configurationLayout,
            ProductCode = productCode,
            Scope = scope,
            EnquiryId = enquiryId,
            IsApprovedByAdmin = isApprovedByAdmin,
            ApprovedBy = approvedBy,
            ApprovedOn = approvedOn,
            IsActive = isActive,
            CreatedOn = createdOn,
            CreatedBy = createdBy,
            UpdatedOn = updatedOn,
            UpdatedBy = updatedBy
        };
    }
}
