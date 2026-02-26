using System;

namespace ConfigurationService.Application.Queries;

public record GetRackConfigurationByIdQuery(Guid Id);

public record ListRackConfigurationsQuery(Guid? EnquiryId = null, bool IncludeInactive = false);
