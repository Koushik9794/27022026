using GssCommon.Common.Models.Configurator;
using System.Threading.Tasks;

namespace RuleService.Domain.Services
{
    public interface ICatalogServiceClient
    {
        Task<PartMetadata?> LookupPartAsync(string componentType, string attributeName, double targetValue);
    }
}
