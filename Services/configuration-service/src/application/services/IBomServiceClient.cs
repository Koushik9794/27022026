using GssCommon.Common.Models.Configurator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationService.Application.Services
{
    public interface IBomServiceClient
    {
        Task PushBomBatchAsync(Guid configurationId, string projectName, IEnumerable<BomItem> items);
    }
}
