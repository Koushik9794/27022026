using CatalogService.Application.Commands;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using CatalogService.Application.Errors;
using GssCommon.Common;
using Wolverine;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/v1/Component-Master")]
public class ComponentMasterController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public ComponentMasterController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of component masters based on optional filter criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ComponentMasterDto>), 200)]
    public async Task<IActionResult> Get([FromQuery] GetAllComponentMastersQuery query)
    {
        var result = await _mediator.InvokeAsync<IEnumerable<CatalogService.Domain.Aggregates.ComponentMaster>>(query);

        if (result == null)
        {
             return Ok(Enumerable.Empty<ComponentMasterDto>());
        }

        var dtos = result.Select(cm => new ComponentMasterDto(
            cm.Id, cm.ComponentMasterCode, cm.CountryCode, cm.UnspscCode,
            cm.ComponentGroupId, cm.ComponentGroupCode, cm.ComponentGroupName,
            cm.ComponentTypeId, cm.ComponentTypeCode, cm.ComponentTypeName,
            cm.ComponentNameId, cm.ComponentNameCode, cm.ComponentNameName,
            cm.Colour, cm.PowderCode, cm.GfaFlag, cm.UnitBasicPrice, cm.Cbm,
            cm.ShortDescription, cm.Description, cm.DrawingNo, cm.RevNo, cm.InstallationRefNo,
            cm.Attributes, cm.GlbFilepath, cm.ImageUrl, cm.Status, cm.IsDeleted,
            cm.CreatedAt, cm.UpdatedAt, cm.CreatedBy, cm.UpdatedBy
        ));
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets a specific component master by its ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ComponentMasterDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetComponentMasterByIdQuery(id);
        var result = await _mediator.InvokeAsync<Result<CatalogService.Domain.Aggregates.ComponentMaster>>(query);
        
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        var cm = result.Value;
        var dto = new ComponentMasterDto(
            cm.Id, cm.ComponentMasterCode, cm.CountryCode, cm.UnspscCode,
            cm.ComponentGroupId, cm.ComponentGroupCode, cm.ComponentGroupName,
            cm.ComponentTypeId, cm.ComponentTypeCode, cm.ComponentTypeName,
            cm.ComponentNameId, cm.ComponentNameCode, cm.ComponentNameName,
            cm.Colour, cm.PowderCode, cm.GfaFlag, cm.UnitBasicPrice, cm.Cbm,
            cm.ShortDescription, cm.Description, cm.DrawingNo, cm.RevNo, cm.InstallationRefNo,
            cm.Attributes, cm.GlbFilepath, cm.ImageUrl, cm.Status, cm.IsDeleted,
            cm.CreatedAt, cm.UpdatedAt, cm.CreatedBy, cm.UpdatedBy
        );

        return Ok(dto);
    }

    /// <summary>
    /// Gets a specific component master by its Component Master Code and Country Code.
    /// </summary>
    [HttpGet("code/{componentMasterCode}/country/{countryCode}")]
    [ProducesResponseType(typeof(ComponentMasterDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string componentMasterCode, string countryCode)
    {
        var query = new GetComponentMasterByCodeAndCountryQuery(componentMasterCode, countryCode);
        var result = await _mediator.InvokeAsync<Result<CatalogService.Domain.Aggregates.ComponentMaster>>(query);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        var cm = result.Value;
        var dto = new ComponentMasterDto(
            cm.Id, cm.ComponentMasterCode, cm.CountryCode, cm.UnspscCode,
            cm.ComponentGroupId, cm.ComponentGroupCode, cm.ComponentGroupName,
            cm.ComponentTypeId, cm.ComponentTypeCode, cm.ComponentTypeName,
            cm.ComponentNameId, cm.ComponentNameCode, cm.ComponentNameName,
            cm.Colour, cm.PowderCode, cm.GfaFlag, cm.UnitBasicPrice, cm.Cbm,
            cm.ShortDescription, cm.Description, cm.DrawingNo, cm.RevNo, cm.InstallationRefNo,
            cm.Attributes, cm.GlbFilepath, cm.ImageUrl, cm.Status, cm.IsDeleted,
            cm.CreatedAt, cm.UpdatedAt, cm.CreatedBy, cm.UpdatedBy
        );

        return Ok(dto);
    }

    /// <summary>
    /// Creates a new component master.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromForm] CreateComponentMasterRequest request)
    {
        var command = new CreateComponentMasterCommand(
            request.ComponentMasterCode,
            request.CountryCode,
            request.UnspscCode,
            request.ComponentGroupId,
            request.ComponentTypeId,
            request.ComponentNameId,
            request.Colour,
            request.PowderCode,
            request.GfaFlag,
            request.UnitBasicPrice,
            request.Cbm,
            request.ShortDescription,
            request.Description,
            request.DrawingNo,
            request.RevNo,
            request.InstallationRefNo,
            request.Attributes,
            request.GlbFile,
            request.ImageFile,
            request.CreatedBy
        );

        var result = await _mediator.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Updates an existing component master.
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateComponentMasterRequest request)
    {
        var command = new UpdateComponentMasterCommand(
            id,
            request.UnspscCode,
            request.ComponentGroupId,
            request.ComponentTypeId,
            request.ComponentNameId,
            request.Colour,
            request.PowderCode,
            request.GfaFlag,
            request.UnitBasicPrice,
            request.Cbm,
            request.ShortDescription,
            request.Description,
            request.DrawingNo,
            request.RevNo,
            request.InstallationRefNo,
            request.Attributes,
            request.GlbFile,
            request.ImageFile,
            request.Status,
            request.UpdatedBy
        );

        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
            if (result.Error == ComponentMasterErrors.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Soft deletes a component master.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        var command = new DeleteComponentMasterCommand(id, updatedBy);
        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
             if (result.Error == ComponentMasterErrors.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
