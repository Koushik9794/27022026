using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Sku;
using CatalogService.Application.Commands.Sku;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Wolverine;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.Controllers;

/// <summary>
/// API controller for managing SKU types.
/// SKU types define what kinds of customer products can be stored (Box, Drum, Bin, etc.).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SkuController(IMessageBus messageBus) : ControllerBase
{
    private readonly IMessageBus _messageBus = messageBus;

    /// <summary>
    /// Get all active SKU types.
    /// </summary>
    /// <returns>A list of SKU types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SkuDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            Result<List<SkuDto>> result = await _messageBus.InvokeAsync<Result<List<SkuDto>>>(new GetAllSkusQuery());
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
    /// Get a specific SKU type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the SKU type.</param>
    /// <returns>SKU type details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SkuDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            Result<SkuDto?> result = await _messageBus.InvokeAsync<Result<SkuDto?>>(new GetSkuByIdQuery(id));

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

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromForm] CreateSkuRequest request)
    {
        try
        {
            Dictionary<string, object>? attributeSchema = null;

            if (!string.IsNullOrWhiteSpace(request.AttributeSchema))
            {
                try
                {
                    attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        request.AttributeSchema);
                }
                catch (JsonException)
                {
                    return CustomErrorResults.FromError(SkuErrors.InvalidAttributeSchema(), this);
                }
            }
            Result<Guid> result = await _messageBus.InvokeAsync<Result<Guid>>(
                new CreateSkuCommand(
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
            return Ok(new
            {
                message = "SKU created successfully"
            });
        }
        catch
        {
            throw;
        }

    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateSkuRequest request)
    {
        try
        {
            Dictionary<string, object>? attributeSchema = null;

            if (!string.IsNullOrWhiteSpace(request.AttributeSchema))
            {
                try
                {
                    attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        request.AttributeSchema);
                }
                catch (JsonException)
                {
                    return CustomErrorResults.FromError(SkuErrors.InvalidAttributeSchema(), this);
                }
            }
            Result<bool> result = await _messageBus.InvokeAsync<Result<bool>>(
                new UpdateSkuCommand(
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
            return Ok(new
            {
                message = "Updated successfully"
            });
        }
        catch
        {
            throw;
        }

    }

   [HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id, [FromQuery] string? deletedBy)
{
    try
    {
        var command = new DeleteSkuCommand(id, deletedBy);

        Result<bool> result =
            await _messageBus.InvokeAsync<Result<bool>>(command);

        if (!result.IsSuccess)
        {
            return CustomErrorResults.FromError(result.Error, this);
        }

        return Ok(new
        {
            message = "Deleted successfully"
        });
    }
    catch
    {
        throw;
    }
}

    [HttpDelete]
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult MissingId()
    {
        return CustomErrorResults.FromError(SkuErrors.MissingId(), this);
    }

/// <summary>
/// Request to create a new SKU type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON string of attribute schema.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
public record CreateSkuRequest(
    [Required] string Code,
    [Required] string Name,
    string? Description,
    string? AttributeSchema,
    IFormFile? GlbFile,
    [Required] bool IsActive = true
);

/// <summary>
/// Request to update an existing SKU type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON string of attribute schema.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
public record UpdateSkuRequest(
    [Required] string Name,
    string? Description,
    string? AttributeSchema,
     IFormFile? GlbFile,
    [Required] bool IsActive
);

}