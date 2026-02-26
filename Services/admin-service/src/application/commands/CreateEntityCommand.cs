using MediatR;
namespace AdminService.Application.Commands;

public sealed record CreateEntityCommand(
    string EntityName,
    string? Description,
    string CreatedBy,
    string SourceTable,
    string PkColumn,
    string LabelColumn
) : IRequest<string>;
