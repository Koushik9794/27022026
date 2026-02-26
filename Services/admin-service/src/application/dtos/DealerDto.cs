namespace AdminService.Application.Dtos
{
    public sealed record DealerDto(
        Guid Id,
        string Code,
        string Name,
        string? ContactName,
        string? ContactEmail,
        string? ContactPhone,
        string? CountryCode,
        string? State,
        string? City,
        string? Address,
        bool IsActive,
        Guid CreatedBy,
        Guid? UpdatedBy,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
