using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Queries;
using GssCommon.Common;
using Wolverine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationService.Api.Controllers;

[ApiController]
[Route("api/v1/rack-configurations")]
[Produces("application/json")]
public class RackConfigurationsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public RackConfigurationsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Creates a new rack configuration with specified scope (Enquiry, Personal, Global).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RackConfigurationResponse>> Create([FromBody] CreateRackConfigurationRequest request)
    {
        try
        {
            // Determine if user is admin. Validating based on specific "Admin" role if available, or just treating as normal user for now if role not defined.
            // Requirement said "if not approved by admin". This usually implies an admin ACTION exists.
            // I'll assume for now I can check role.
            bool isAdmin = User.IsInRole("Admin");
            string? userName = User.Identity?.Name;

            var command = new CreateRackConfigurationCommand(
                request.Name,
                request.ConfigurationLayout,
                request.ProductCode,
                request.Scope,
                request.EnquiryId,
                request.CreatedBy ?? userName ?? "system",
                isAdmin
            );

            // Wolverine InvokeAsync returning Result<Guid>
            // Note: The handler returns Result<Guid>, so InvokeAsync<Result<Guid>> is needed.
            var result = await _bus.InvokeAsync<GssCommon.Common.Result<Guid>>(command);

            if (result.IsSuccess)
            {
                var query = new GetRackConfigurationByIdQuery(result.Value);
                var response = await _bus.InvokeAsync<GssCommon.Common.Result<RackConfigurationResponse>>(query);
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, response.Value);
            }
            else
            {
                return BadRequest(new { error = result.Error });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all rack configurations, optionally filtered by EnquiryId.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RackConfigurationResponse>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = new ListRackConfigurationsQuery(null, includeInactive);
        var result = await _bus.InvokeAsync<GssCommon.Common.Result<IEnumerable<RackConfigurationResponse>>>(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a specific rack configuration by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RackConfigurationResponse>> GetById(Guid id)
    {
        var query = new GetRackConfigurationByIdQuery(id);
        var result = await _bus.InvokeAsync<GssCommon.Common.Result<RackConfigurationResponse>>(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing rack configuration.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Guid>> Update(Guid id, [FromBody] UpdateRackConfigurationRequest request)
    {
        bool isAdmin = User.IsInRole("Admin");
        string? userName = User.Identity?.Name;

        var command = new UpdateRackConfigurationCommand(
            id,
            request.Name,
            request.ConfigurationLayout,
            request.ProductCode,
            request.Scope,
            request.EnquiryId,
            request.UpdatedBy ?? userName ?? "system",
            isAdmin
        );

        var result = await _bus.InvokeAsync<GssCommon.Common.Result<Guid>>(command);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deactivates a specific rack configuration.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        string? userName = User.Identity?.Name;
        var command = new DeleteRackConfigurationCommand(id, userName ?? "system");
        var result = await _bus.InvokeAsync<GssCommon.Common.Result<Guid>>(command);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(new { error = result.Error });
    }
}
