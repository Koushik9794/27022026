using CatalogService.Application.Services;
using GssCommon.Common.Models.Configurator;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/catalog-lookup")]
    public class CatalogLookupController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogLookupController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<PartMetadata>> Lookup(
            [FromQuery] string componentType, 
            [FromQuery] string attributeName, 
            [FromQuery] double targetValue)
        {
            var result = await _catalogService.LookupPartAsync(componentType, attributeName, targetValue);
            if (result == null)
            {
                return NotFound($"No matching part found for {componentType} with {attributeName} >= {targetValue}");
            }
            return Ok(result);
        }
    }
}
