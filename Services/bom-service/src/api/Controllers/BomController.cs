using BomService.Domain.Aggregates;
using BomService.Infrastructure.Persistence;
using GssCommon.Common.Models.Configurator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BomService.Api.Controllers
{
    [ApiController]
    [Route("api/bom")]
    public class BomController : ControllerBase
    {
        private readonly IBomRepository _bomRepository;

        public BomController(IBomRepository bomRepository)
        {
            _bomRepository = bomRepository;
        }

        [HttpPost("batch")]
        public async Task<IActionResult> PushBatch([FromBody] BomBatchRequest request)
        {
            if (request == null || request.Items == null)
                return BadRequest("Invalid BOM batch request.");

            var bom = BillOfMaterials.Create(request.ConfigurationId, request.ProjectName);
            bom.AddItems(request.Items);

            await _bomRepository.SaveAsync(bom);

            return Ok(new { BomId = bom.Id, Status = "Persisted" });
        }
    }

    public class BomBatchRequest
    {
        public Guid ConfigurationId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<BomItem> Items { get; set; } = new();
    }
}
