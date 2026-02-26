using ConfigurationService.Application.Dtos;
using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Infrastructure.Persistence;

public interface ICivilLayoutRepository
{
    Task<ConfigurationDto?> GetCivilLayoutByConfigurationIdAsync(Guid ConfigId);
    Task<CivilLayout?> GetCivilLayoutByIdAsync(Guid Id);
    Task<int> CreateCivilLayoutAsync(CivilLayout civilLayout);
    Task<Guid> UpsertCivilLayoutAsync(CivilLayout civilLayout);


    Task<Configuration?> GetRackLayoutByVersionIdAsync(Guid ConfigId, int VersionNo);

    Task<RevisionIdsDto> GetRevisionIdAsync(Guid ConfigId, int ConfigversionNo, int CivilVersionNo);

    Task<Guid> CreateRackLayoutAsync(RackLayout rackLayout);
    Task<Guid> UpdateRackLayoutAsync(RackLayout rackLayout);

    Task<RackLayout?> GetRackLayoutByIdAsync(Guid Id);
}

