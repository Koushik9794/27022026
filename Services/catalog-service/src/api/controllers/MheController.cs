using CatalogService.Application.commands.Mhe;
using CatalogService.Application.dtos;
using CatalogService.Application.queries.Mhe;
using GssCommon.Common;
using ImTools;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

using System.ComponentModel.DataAnnotations;
namespace CatalogService.api.controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class MheController(IMessageBus _mediator) : ControllerBase
{
    /// <summary>
    /// Get all active Material Handling Equipment (MHE).
    /// </summary>
    /// <param name="IsActive">[OPTIONAL] Filter by active status (true/false).</param>
    /// <remarks>
    /// Returns a list of all MHEs in the catalog, optionally filtered by their active status.
    /// </remarks>
    /// <response code="200">Returns the list of MHEs</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<MheDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(bool? IsActive)
    {
        try
        {
            //var result = await _mediator.InvokeAsync<List<MheDto>>(new GetAllMhesQuery(IsActive));
            //return Ok(result);

            Result<List<MheDto>?> result = await _mediator.InvokeAsync<Result<List<MheDto>?>>(new GetAllMheQuery(IsActive));
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Get a specific MHE by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE.</param>
    /// <remarks>
    /// Retrieves a single MHE by its unique identifier.
    /// </remarks>
    /// <response code="200">Returns the MHE</response>
    /// <response code="404">MHE not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var guidId) || guidId == Guid.Empty)
            {
                return BadRequest(new { message = "invalid id or null id not valid" });
            }

            Result<MheDto?> result = await _mediator.InvokeAsync<Result<MheDto?>>(new GetMheByIdQuery(guidId));
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);

        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Create a new MHE.
    /// </summary>
    /// <param name="request">[REQUIRED] The MHE creation request details.</param>
    /// <remarks>
    /// Creates a new material handling unit in the catalog.
    /// </remarks>
    /// <response code="201">MHE created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateMheRequest request)
    {
        try
        {

            var command = new CreateMheCommand(
                request.Code ?? string.Empty,
                request.Name ?? string.Empty,
                request.Manufacturer ?? string.Empty,
                request.Brand ?? string.Empty,
                request.Model ?? string.Empty,
                request.MheType ?? string.Empty,
                request.MheCategory ?? string.Empty,
                request.GlbFile,
                request.Attributes,
                request.IsActive,
                request.CreatedBy
            );

            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(command);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(
                new
                {
                    message = "Mhe Created successfully"
                }
            );

        }
        catch
        {
            throw;
        }



    }

    /// <summary>
    /// Update an existing MHE.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE to update.</param>
    /// <param name="request">[REQUIRED] The MHE update request details.</param>
    /// <remarks>
    /// Updates an existing MHE with new values.
    /// </remarks>
    /// <response code="200">MHE updated successfully</response>
    /// <response code="404">MHE not found</response>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(string id, [FromForm] UpdateMheRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var guidId) || guidId == Guid.Empty)
            {
                return BadRequest(new { message = "invalid id or null id not valid" });
            }

            var command = new UpdateMheCommand(
                guidId,
                request.Code ?? string.Empty,
                request.Name ?? string.Empty,
                request.Manufacturer ?? string.Empty,
                request.Brand ?? string.Empty,
                request.Model ?? string.Empty,
                request.MheType ?? string.Empty,
                request.MheCategory ?? string.Empty,
                request.GlbFile,
                request.Attributes,
                request.IsActive,
                request.UpdatedBy
            );


            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(command);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(
                new
                {
                    message = "Mhe Updated successfully"
                }
            );

        }
        catch
        {
            throw;
        }

    }


    /// <summary>
    /// Delete an MHE (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE to delete.</param>
    /// <param name="deletedBy">[OPTIONAL] The user performing the deletion.</param>
    /// <remarks>
    /// Performs a soft delete on the MHE. The record will be marked as deleted but not removed from the database.
    /// </remarks>
    /// <response code="200">MHE deleted successfully</response>
    /// <response code="404">MHE not found</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string? deletedBy)
    {
        try
        {
            if (!Guid.TryParse(id, out var guidId) || guidId == Guid.Empty)
            {
                return BadRequest(new { message = "invalid id or null id not valid" });
            }

            var command = new DeleteMheCommand(guidId, deletedBy);


            Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(command);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            
            return Ok(new
            {
                message = "Mhe Deleted successfully"
            });

        }
        catch
        {
            throw;
        }
    }
}




/// <summary>
/// Request to create a new MHE.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Manufacturer">[OPTIONAL] Manufacturer.</param>
/// <param name="Brand">[OPTIONAL] Brand.</param>
/// <param name="Model">[OPTIONAL] Model.</param>
/// <param name="MheType">[OPTIONAL] Type.</param>
/// <param name="MheCategory">[OPTIONAL] Category.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateMheRequest(
    [Required] string? Code, 
    [Required] string? Name, 
    string? Manufacturer, 
    string? Brand, 
    string? Model, 
    string? MheType, 
    string? MheCategory, 
    IFormFile? GlbFile, 
    string? Attributes, 
    [Required] bool IsActive, 
    string? CreatedBy);

/// <summary>
/// Request to update an existing MHE.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Manufacturer">[OPTIONAL] Manufacturer.</param>
/// <param name="Brand">[OPTIONAL] Brand.</param>
/// <param name="Model">[OPTIONAL] Model.</param>
/// <param name="MheType">[OPTIONAL] Type.</param>
/// <param name="MheCategory">[OPTIONAL] Category.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier performing the update.</param>
public record UpdateMheRequest(
    [Required] string? Code, 
    [Required] string? Name, 
    string? Manufacturer, 
    string? Brand, 
    string? Model, 
    string? MheType, 
    string? MheCategory, 
    IFormFile? GlbFile, 
    string? Attributes, 
    [Required] bool IsActive, 
    string? UpdatedBy);
