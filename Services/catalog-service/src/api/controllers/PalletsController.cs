using System.Text.Json;
using CatalogService.Application.Commands.Pallets;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Pallets;
using CatalogService.Application.Errors;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CatalogService.Api.Controllers;

/// <summary>
/// API controller for managing pallet types.
/// Pallet types define what kinds of pallets can be used (EURO, US, UK, etc.).
/// </summary>
[ApiController]
[Route("api/v1/pallet-types")]
[Produces("application/json")]
public class PalletsController(IMessageBus messageBus) : ControllerBase
{
    private readonly IMessageBus _messageBus = messageBus;

    /// <summary>
    /// Get all active pallet types.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive pallet types (true/false).</param>
    /// <returns>A list of pallet types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PalletDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            Result<List<PalletDto>?> result = await _messageBus.InvokeAsync<Result<List<PalletDto>?>>(
            new GetAllPalletsQuery(includeInactive));
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
    /// Get a specific pallet type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the pallet type.</param>
    /// <returns>The pallet type details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PalletDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            Result<PalletDto?> result = await _messageBus.InvokeAsync<Result<PalletDto?>>(
            new GetPalletByIdQuery(id));

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
    /// Get a specific pallet type by code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique code of the pallet type.</param>
    /// <returns>The pallet type details.</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PalletDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code)
    {
        try
        {
            Result<PalletDto?> result = await _messageBus.InvokeAsync<Result<PalletDto?>>(
            new GetPalletByCodeQuery(code));
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
    /// Create a new pallet type.
    /// </summary>
    /// <param name="request">[REQUIRED] The pallet type creation request details.</param>
    /// <returns>The ID of the created pallet type.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreatePalletRequest request)
    {
        try
        {
            Dictionary<string, object>? attributeSchema = null;

            if (!string.IsNullOrWhiteSpace(request.AttributeSchema))
            {
                attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    request.AttributeSchema);
            }

            Result<Guid> result = await _messageBus.InvokeAsync<Result<Guid>>(
                new CreatePalletCommand(
                    request.Code,
                    request.Name,
                    request.Description,
                    attributeSchema,
                    request.GlbFile,
                    request.IsActive,
                    User.Identity?.Name
                ));

            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(new { message = "Pallet created successfully" });
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Update an existing pallet type.
    /// </summary>
    /// <param name="id">[REQUIRED] The pallet type unique identifier.</param>
    /// <param name="request">[REQUIRED] The pallet type update request details.</param>
    /// <returns>The unique identifier of the updated pallet type.</returns>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePalletRequest request)
    {
        try
        {
            Dictionary<string, object>? attributeSchema = null;

            if (!string.IsNullOrWhiteSpace(request.AttributeSchema))
            {
                attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    request.AttributeSchema);
            }
            Result<Guid> result = await _messageBus.InvokeAsync<Result<Guid>>(
                new UpdatePalletCommand(
                    id,
                    request.Name,
                    request.Description,
                    attributeSchema,
                    request.GlbFile,
                    request.IsActive,
                    User.Identity?.Name
                ));

            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(
                new{
                    message="Pallet updated successfully"
                }
            );
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Delete a pallet type (soft delete).
    /// </summary>
    /// <param name="id">Pallet type ID.</param>
    /// <param name="deletedBy">[OPTIONAL] The user performing the deletion.</param>
    /// <returns>Deletion confirmation.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? deletedBy)
    {
        try
        {
            Result<bool> result = await _messageBus.InvokeAsync<Result<bool>>(
                new DeletePalletCommand(id, deletedBy ?? User.Identity?.Name));

            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }

            return Ok(new { message = "Deleted successfully" });
        }
        catch
        {
            throw;
        }
    }

    
}
