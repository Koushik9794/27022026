using GssCommon.Common.Models.Configurator;
using System.Threading.Tasks;

namespace CatalogService.Application.Services
{
    public interface ICatalogService
    {
        Task<PartMetadata?> LookupPartAsync(string componentType, string attributeName, double targetValue);
    }
}
