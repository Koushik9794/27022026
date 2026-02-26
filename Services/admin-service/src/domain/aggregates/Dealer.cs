using AdminService.Domain.ValueObjects;
using AdminService.Domain.Entities;
using AdminService.Domain.Events;

namespace AdminService.Domain.Aggregates
{
    /// <summary>
    /// Dealer Aggregate Root
    /// Represents a dealer in the system
    /// </summary>
    public class Dealer
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string? ContactName { get; private set; }
        public Email? ContactEmail { get; private set; }
        public string? ContactPhone { get; private set; }
        public string? CountryCode { get; private set; }
        public string? State { get; private set; }
        public string? City { get; private set; }
        public string? Address { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public Guid CreatedBy { get; private set; }
        public Guid? UpdatedBy { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Dealer() { }

        /// <summary>
        /// Create a new dealer
        /// </summary>
        public static Dealer Create(
            string code,
            string name,
            string? contactName,
            Email? contactEmail,
            string? contactPhone,
            string? countryCode,
            string? state,
            string? city,
            string? address,
            Guid createdBy)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Dealer code is required", nameof(code));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Dealer name is required", nameof(name));

            return new Dealer
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                ContactName = contactName,
                ContactEmail = contactEmail,
                ContactPhone = contactPhone,
                CountryCode = countryCode,
                State = state,
                City = city,
                Address = address,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(
            string name,
            string? contactName,
            Email? contactEmail,
            string? contactPhone,
            string? countryCode,
            string? state,
            string? city,
            string? address,
            bool isActive,
            Guid updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Dealer name is required", nameof(name));

            Name = name;
            ContactName = contactName;
            ContactEmail = contactEmail;
            ContactPhone = contactPhone;
            CountryCode = countryCode;
            State = state;
            City = city;
            Address = address;
            IsActive = isActive;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(Guid updatedBy)
        {
            if (IsDeleted) return;

            IsDeleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate(Guid updatedBy)
        {
            if (IsActive && !IsDeleted) return;

            IsActive = true;
            IsDeleted = false; // Reactivate if it was soft deleted? Or enable soft deleted items? Assuming standard activation.
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate(Guid updatedBy)
        {
            if (!IsActive) return;

            IsActive = false;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
