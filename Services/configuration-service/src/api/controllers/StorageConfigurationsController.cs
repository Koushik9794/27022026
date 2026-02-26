using System.Text.Json;
using ConfigurationService.Application.Commands;
using Wolverine;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

/// <summary>
/// API controller for Storage Configuration and Design operations.
/// Includes autosave endpoint for design data.
/// </summary>
[ApiController]
[Route("api/v1/storage-configurations")]
[Produces("application/json")]
public class StorageConfigurationsController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public StorageConfigurationsController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Autosave endpoint - updates design data for an existing storage configuration.
    /// Called frequently from UI as designer makes changes.
    /// </summary>
    /// <param name="id">Storage configuration ID</param>
    /// <param name="request">Design data to save</param>
    /// <returns>Success indicator</returns>
    [HttpPut("{id:guid}/design")]
    public async Task<IActionResult> SaveDesign(Guid id, [FromBody] SaveDesignRequest request)
    {
        try
        {
            var success = await _mediator.InvokeAsync<bool>(new SaveDesignCommand(
                id, 
                request.DesignData, 
                User.Identity?.Name));
            
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new storage configuration for a version.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateStorageConfigurationRequest request)
    {
        try
        {
            var id = await _mediator.InvokeAsync<Guid>(new AddStorageConfigurationCommand(
                request.ConfigurationId,
                request.VersionNumber,
                request.Name,
                request.ProductGroup,
                request.Description,
                request.FloorId,
                request.DesignData,
                User.Identity?.Name));
            
            return CreatedAtAction(nameof(SaveDesign), new { id }, id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// ============ Request DTOs ============

public record SaveDesignRequest(JsonDocument DesignData);

public record CreateStorageConfigurationRequest(
    Guid ConfigurationId,
    int VersionNumber,
    string Name,
    string ProductGroup,
    string? Description,
    Guid? FloorId,
    JsonDocument? DesignData
);
