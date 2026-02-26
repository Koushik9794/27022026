using ConfigurationService.Domain.Enums;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Enquiry aggregate root - the project container linked to external CRM.
/// Contains multiple Configurations (design variants).
/// </summary>
public sealed class Enquiry
{
    public Guid Id { get; private set; }
    public string ExternalEnquiryId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? EnquiryNo { get; private set; }
    public string? CustomerName { get; private set; }
    public long? CustomerContact { get; private set; }
    public string? CustomerMail { get; private set; }
    public string? ProductGroup { get; private set; }
    public string? BillingDetails { get; private set; }
    public string? Source { get; private set; }
    public Guid? DealerId { get; private set; }

    public EnquiryStatus Status { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Child configurations - Enquiry contains multiple Configurations
    private readonly List<Configuration> _configurations = new();
    public IReadOnlyCollection<Configuration> Configurations => _configurations.AsReadOnly();

    private Enquiry() { }

    public static Enquiry Create(
        string externalEnquiryId,
        string name,
        string? description = null,
        string? enquiryNo = null,
        string ? customerName = null,
        long ? customerContact = null,
        string? customerMail = null,
        string? productgroup = null,
        string? billingdetails = null,
        string? source = null,
        Guid? dealerId = null,
        string? createdBy = null)
    {
        ValidateExternalEnquiryId(externalEnquiryId);
        ValidateName(name);

        return new Enquiry
        {
            Id = Guid.NewGuid(),
            ExternalEnquiryId = externalEnquiryId.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            EnquiryNo = enquiryNo?.Trim(),
            CustomerName = customerName?.Trim(),
            CustomerContact = customerContact,
            CustomerMail = customerMail?.Trim(),
            ProductGroup = productgroup?.Trim(),
            BillingDetails = billingdetails?.Trim(),
            Source = source?.Trim(),
            DealerId   = dealerId,
            Status = EnquiryStatus.Draft,
            Version = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static Enquiry Rehydrate(
        Guid id,
        string externalEnquiryId,
        string name,
        string? description,
        string? enquiryNo,
        string? customerName ,
        long? customerContact,
        string? customerMail ,
        string? product_group,
         string? billing_details,
        string? source,
        Guid? dealerId,
        EnquiryStatus status,
        int version,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy,
        bool isDeleted,
        IEnumerable<Configuration>? configurations = null)
    {
        var enquiry = new Enquiry
        {
            Id = id,
            ExternalEnquiryId = externalEnquiryId,
            Name = name,
            Description = description,
            EnquiryNo = enquiryNo,
            CustomerName = customerName?.Trim(),
            CustomerContact = customerContact,
            CustomerMail = customerMail?.Trim(),
            BillingDetails = billing_details?.Trim(),
            ProductGroup = product_group?.Trim(),
            Source = source?.Trim(),
            DealerId = dealerId,
            Status = status,
            Version = version,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy,
            IsDeleted = isDeleted
        };

        if (configurations != null) enquiry._configurations.AddRange(configurations);

        return enquiry;
    }

    // ============ Configuration Management ============

    public Configuration AddConfiguration(
        string name,
        string? description = null,
        bool isPrimary = false,
        string? createdBy = null)
    {
        // If this is set as primary, clear existing primaries
        if (isPrimary)
        {
            foreach (var c in _configurations.Where(c => c.IsPrimary))
            {
                c.ClearPrimary(createdBy);
            }
        }

        var configuration = Configuration.Create(Id, name, description, isPrimary, createdBy);
        _configurations.Add(configuration);
        IncrementVersion(createdBy);
        return configuration;
    }

    public void RemoveConfiguration(Guid configurationId, string? updatedBy)
    {
        var config = _configurations.FirstOrDefault(c => c.Id == configurationId);
        if (config != null)
        {
            config.Deactivate(updatedBy);
            IncrementVersion(updatedBy);
        }
    }

    public void SetPrimaryConfiguration(Guid configurationId, string? updatedBy)
    {
        foreach (var c in _configurations.Where(c => c.IsPrimary))
        {
            c.ClearPrimary(updatedBy);
        }

        var config = _configurations.FirstOrDefault(c => c.Id == configurationId);
        config?.SetAsPrimary(updatedBy);
        IncrementVersion(updatedBy);
    }

    public Configuration? GetPrimaryConfiguration()
    {
        return _configurations.FirstOrDefault(c => c.IsPrimary && c.IsActive);
    }

    // ============ Enquiry Management ============

    public void Update(string name, string? description, string? enquiryno, string? customerName,  long? customerContact,string? customerMail,string?productgroup,string?billingdetails, string? source,Guid? dealerid, string? updatedBy)
    {
        ValidateName(name);
        Name = name.Trim();
        Description = description?.Trim();
        EnquiryNo = enquiryno?.Trim();
        CustomerName = customerName?.Trim();
        CustomerContact = customerContact;
        CustomerMail = customerMail?.Trim();
        ProductGroup = productgroup?.Trim();
        BillingDetails = billingdetails?.Trim();
        Source = source?.Trim();
        DealerId = dealerid;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(EnquiryStatus newStatus, string? updatedBy)
    {
        ValidateStatusTransition(Status, newStatus);
        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(string? deletedBy)
    {
        IsDeleted = true;
        Status = EnquiryStatus.Archived;
        UpdatedBy = deletedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private void IncrementVersion(string? updatedBy)
    {
        Version++;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateExternalEnquiryId(string externalEnquiryId)
    {
        if (string.IsNullOrWhiteSpace(externalEnquiryId))
            throw new ArgumentException("External enquiry ID cannot be empty.", nameof(externalEnquiryId));
        if (externalEnquiryId.Length > 100)
            throw new ArgumentException("External enquiry ID cannot exceed 100 characters.", nameof(externalEnquiryId));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Enquiry name cannot be empty.", nameof(name));
        if (name.Length > 255)
            throw new ArgumentException("Enquiry name cannot exceed 255 characters.", nameof(name));
    }

    private static void ValidateStatusTransition(EnquiryStatus current, EnquiryStatus target)
    {
        var validTransitions = new Dictionary<EnquiryStatus, EnquiryStatus[]>
        {
            [EnquiryStatus.Draft] = new[] { EnquiryStatus.InProgress, EnquiryStatus.Archived },
            [EnquiryStatus.InProgress] = new[] { EnquiryStatus.Submitted, EnquiryStatus.Draft, EnquiryStatus.Archived },
            [EnquiryStatus.Submitted] = new[] { EnquiryStatus.Converted, EnquiryStatus.InProgress, EnquiryStatus.Archived },
            [EnquiryStatus.Converted] = new[] { EnquiryStatus.Archived },
            [EnquiryStatus.Archived] = Array.Empty<EnquiryStatus>()
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
        {
            throw new InvalidOperationException($"Cannot transition from {current} to {target}.");
        }
    }
}
