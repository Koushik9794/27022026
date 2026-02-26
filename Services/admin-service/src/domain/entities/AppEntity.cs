namespace AdminService.Domain.Entities;

public sealed class AppEntity
{
    public Guid Id { get; init; }
    public string SourceTable { get; init; } = default!;
    public string PkColumn { get; init; } = default!;
    public string LabelColumn { get; init; } = default!;
    public string EntityName { get; init; } = default!;
    public bool IsActive { get; init; }
}
