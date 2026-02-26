using CatalogService.Application.Commands;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Application.Errors;
using GssCommon.Common;
using Wolverine;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public PartsController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of parts based on optional filter criteria.
    /// </summary>
    /// <param name="query">[REQUIRED] The query parameters for filtering and pagination.</param>
    /// <returns>A list of parts matching the criteria.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PartDto>), 200)]
    public async Task<IActionResult> Get([FromQuery] GetAllPartsQuery query)
    {
        var result = await _mediator.InvokeAsync<IEnumerable<CatalogService.Domain.Aggregates.Part>>(query);

        if (result == null)
        {
             return Ok(Enumerable.Empty<PartDto>());
        }

        var dtos = result.Select(p => new PartDto(
            p.Id, p.PartCode, p.CountryCode, p.UnspscCode,
            p.ComponentGroupId, p.ComponentGroupCode, p.ComponentGroupName,
            p.ComponentTypeId, p.ComponentTypeCode, p.ComponentTypeName,
            p.ComponentNameId, p.ComponentNameCode, p.ComponentNameName,
            p.Colour, p.PowderCode, p.GfaFlag, p.UnitBasicPrice, p.Cbm,
            p.ShortDescription, p.Description, p.DrawingNo, p.RevNo, p.InstallationRefNo,
            p.Attributes, p.GlbFilepath, p.ImageUrl, p.Status, p.IsDeleted,
            p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy
        ));
        
        return Ok(dtos);
    }

    /// <summary>
    /// Gets a specific part by its ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part.</param>
    /// <returns>The part details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PartDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetPartByIdQuery(id);
        var result = await _mediator.InvokeAsync<Result<CatalogService.Domain.Aggregates.Part>>(query);
        
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        var p = result.Value;
        var dto = new PartDto(
            p.Id, p.PartCode, p.CountryCode, p.UnspscCode,
            p.ComponentGroupId, p.ComponentGroupCode, p.ComponentGroupName,
            p.ComponentTypeId, p.ComponentTypeCode, p.ComponentTypeName,
            p.ComponentNameId, p.ComponentNameCode, p.ComponentNameName,
            p.Colour, p.PowderCode, p.GfaFlag, p.UnitBasicPrice, p.Cbm,
            p.ShortDescription, p.Description, p.DrawingNo, p.RevNo, p.InstallationRefNo,
            p.Attributes, p.GlbFilepath, p.ImageUrl, p.Status, p.IsDeleted,
            p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy
        );

        return Ok(dto);
    }

    /// <summary>
    /// Gets a specific part by its Part Code and Country Code.
    /// </summary>
    /// <param name="partCode">[REQUIRED] The unique part code.</param>
    /// <param name="countryCode">[REQUIRED] The 2-character country code.</param>
    /// <returns>The part details.</returns>
    [HttpGet("code/{partCode}/country/{countryCode}")]
    [ProducesResponseType(typeof(PartDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string partCode, string countryCode)
    {
        var query = new GetPartByCodeAndCountryQuery(partCode, countryCode);
        var result = await _mediator.InvokeAsync<Result<CatalogService.Domain.Aggregates.Part>>(query);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        var p = result.Value;
        var dto = new PartDto(
            p.Id, p.PartCode, p.CountryCode, p.UnspscCode,
            p.ComponentGroupId, p.ComponentGroupCode, p.ComponentGroupName,
            p.ComponentTypeId, p.ComponentTypeCode, p.ComponentTypeName,
            p.ComponentNameId, p.ComponentNameCode, p.ComponentNameName,
            p.Colour, p.PowderCode, p.GfaFlag, p.UnitBasicPrice, p.Cbm,
            p.ShortDescription, p.Description, p.DrawingNo, p.RevNo, p.InstallationRefNo,
            p.Attributes, p.GlbFilepath, p.ImageUrl, p.Status, p.IsDeleted,
            p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy
        );

        return Ok(dto);
    }

    /// <summary>
    /// Creates a new part.
    /// </summary>
    /// <param name="request">[REQUIRED] The part creation request details.</param>
    /// <returns>The ID of the created part.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromForm] CreatePartRequest request)
    {
        var command = new CreatePartCommand(
            request.PartCode,
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
            request.GlbFile, // IFormFile
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
    /// Updates an existing part.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part to update.</param>
    /// <param name="request">[REQUIRED] The part update request details.</param>
    /// <returns>Success status.</returns>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePartRequest request)
    {
        var command = new UpdatePartCommand(
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
            if (result.Error == PartErrors.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Soft deletes a part.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] The user performing the deletion.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        var command = new DeletePartCommand(id, updatedBy);
        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
             if (result.Error == PartErrors.NotFound)
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    #region Lookups

    /// <summary>
    /// Gets all active component groups.
    /// </summary>
    [HttpGet("lookup/groups")]
    [ProducesResponseType(typeof(IEnumerable<ComponentGroupDto>), 200)]
    public async Task<IActionResult> GetGroups()
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<ComponentGroupDto>>>(new GetAllComponentGroupsQuery(false));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Gets component types filtered by group.
    /// </summary>
    [HttpGet("lookup/types/{groupId}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentTypeDto>), 200)]
    public async Task<IActionResult> GetTypes(Guid groupId)
    {
        var result = await _mediator.InvokeAsync<Result<List<ComponentTypeDto>>>(new GetAllComponentTypesQuery(null, groupId, false));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Gets component names filtered by type.
    /// </summary>
    [HttpGet("lookup/names/{typeId}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), 200)]
    public async Task<IActionResult> GetNames(Guid typeId)
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<ComponentNameDto>>>(new GetComponentNamesByTypeQuery(typeId, false));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    #endregion
}
