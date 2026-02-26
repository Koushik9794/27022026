namespace ConfigurationService.Application.Queries;

public record GetCivilLayoutByConfigIdQuery(Guid ConfigurationId);
public record GetCivilLayoutByIdQuery(Guid Id);

public record GetRackLayoutByVersionIdQuery(Guid ConfigurationId, int Version);

public record GetRackLayoutByIdQuery(Guid Id);
