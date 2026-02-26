using System.Text.Json;
using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Queries;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace ConfigurationService.Api.Controllers;

/// <summary>
/// API controller for managing Enquiries and Configurations.
/// </summary>
[ApiController]
[Route("api/v1/enquiries")]
[Produces("application/json")]
public class EnquiriesController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public EnquiriesController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    // ============ Enquiry Endpoints ============

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
    {
        try
        {
            Result<IEnumerable<EnquiryDto>?> result = await _mediator.InvokeAsync<Result<IEnumerable<EnquiryDto>?>>(new GetAllEnquiriesQuery(includeDeleted));
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {

            Result<EnquiryDto> result = await _mediator.InvokeAsync<Result<EnquiryDto>>(new GetEnquiryByIdQuery(id));
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

    [HttpGet("external/{externalId}")]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        try
        {

            Result<EnquiryDto> result = await _mediator.InvokeAsync<Result<EnquiryDto>>(new GetEnquiryByExternalIdQuery(externalId));
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
    public async Task<IActionResult> Create([FromBody] CreateEnquiryRequest request)
    {
        try
        {
            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(new CreateEnquiryCommand(
                request.ExternalEnquiryId, request.Name, request.Description, request.EnquiryNo, request.CustomerName, request.CustomerContact, request.CustomerMail, request.ProductGroup, request.BillingDetails, request.Source, request.DealerId, User.Identity?.Name));
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnquiryRequest request)
    {
        try
        {
            Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(new UpdateEnquiryCommand(id, request.Name, request.Description, request.EnquiryNo, request.CustomerName, request.CustomerContact, request.CustomerMail, request.ProductGroup, request.BillingDetails, request.Source, request.DealerId, User.Identity?.Name));
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(new DeleteEnquiryCommand(id, User.Identity?.Name));
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

    // ============ Configuration Endpoints ============

    [HttpGet("{enquiryId:guid}/configurations")]
    public async Task<IActionResult> GetConfigurations(Guid enquiryId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            Result<IEnumerable<ConfigurationDto>> result = await _mediator.InvokeAsync<Result<IEnumerable<ConfigurationDto>>>(new GetConfigurationsByEnquiryIdQuery(enquiryId, includeInactive));
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
    [HttpGet("{enquiryId:guid}/configurationList")]
    public async Task<IActionResult> GetConfigurationList(Guid enquiryId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            Result<EnquiryDto> result = await _mediator.InvokeAsync<Result<EnquiryDto>>(new GetConfigurationListByEnquiryIdQuery(enquiryId, includeInactive));
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
    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}")]
    public async Task<IActionResult> GetConfiguration(Guid enquiryId, Guid configId)
    {
        try
        {
            Result<ConfigurationDto> result = await _mediator.InvokeAsync<Result<ConfigurationDto>>(new GetConfigurationWithVersionsQuery(configId));
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

    [HttpPost("{enquiryId:guid}/configurations")]
    public async Task<IActionResult> CreateConfiguration(Guid enquiryId, [FromBody] CreateConfigurationRequest request)
    {
        try
        {
            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(new CreateConfigurationCommand(
                enquiryId, request.Name, request.Description, request.IsPrimary, User.Identity?.Name));
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    //[HttpPost("{enquiryId:guid}/configurations/{configId:guid}/set-primary")]
    //public async Task<IActionResult> SetPrimary(Guid enquiryId, Guid configId)
    //{
    //    try
    //    {
    //        Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(new SetPrimaryConfigurationCommand(configId, User.Identity?.Name));
    //        if (!result.IsSuccess)
    //        {
    //            return CustomErrorResults.FromError(result.Error, this);
    //        }
    //        return Ok(result.Value);
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        return BadRequest(new { error = ex.Message });
    //    }
    //}

    [HttpDelete("{enquiryId:guid}/configurations/{configId:guid}")]
    public async Task<IActionResult> DeleteConfiguration(Guid enquiryId, Guid configId)
    {
        try
        {
            Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(new DeleteConfigurationCommand(configId, User.Identity?.Name));
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============ Version Endpoints ============

 
    [HttpPost("{enquiryId:guid}/configurations/{configId:guid}/versions")]
    public async Task<IActionResult> CreateVersion(Guid enquiryId, Guid configId, [FromBody] CreateVersionRequest request)
    {
        try
        {
            Result<int> result = await _mediator.InvokeAsync<Result<int>>(new CreateConfigurationVersionCommand(configId, request.Description, User.Identity?.Name));
            //return Created($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/{versionNumber}", versionNumber);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{enquiryId:guid}/configurations/{configId:guid}/version-lock/{versionNumber:int}")]
    public async Task<IActionResult> LockVersion(Guid enquiryId, Guid configId, int versionNumber, [FromBody] CreateVersionRequest request)
    {
        try
        {
            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(new LockVersionCommand(enquiryId, configId, versionNumber, User.Identity?.Name));
            //return Created($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/{versionNumber}", versionNumber);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpPut("{enquiryId:guid}/configurations/{configId:guid}/un-lock-version/{versionNumber:int}")]
    public async Task<IActionResult> UnlockVersion(Guid enquiryId, Guid configId, int versionNumber, [FromBody] CreateVersionRequest request)
    {
        try
        {
            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(new UnLockVersionCommand(enquiryId, configId, versionNumber, User.Identity?.Name));
            //return Created($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/{versionNumber}", versionNumber);
            if (!result.IsSuccess)
            {
                return CustomErrorResults.FromError(result.Error, this);
            }
            return Ok(result.Value);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    //[HttpGet("{enquiryId:guid}/configurations/{configId:guid}/versions/current")]
    //public async Task<IActionResult> GetCurrentVersion(Guid enquiryId, Guid configId)
    //{
    //    try
    //    {
    //        Result<ConfigurationVersionDto> result = await _mediator.InvokeAsync<Result<ConfigurationVersionDto>>(new GetCurrentVersionQuery(configId));
    //        if (!result.IsSuccess)
    //        {
    //            return CustomErrorResults.FromError(result.Error, this);
    //        }
    //        return Ok(result.Value);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    //[HttpPost("{enquiryId:guid}/configurations/{configId:guid}/versions/{versionNumber:int}/set-current")]
    //public async Task<IActionResult> SetCurrentVersion(Guid enquiryId, Guid configId, int versionNumber)
    //{
    //    try
    //    {
    //        Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(new SetCurrentVersionCommand(configId, versionNumber, User.Identity?.Name));
    //        if (!result.IsSuccess)
    //        {
    //            return CustomErrorResults.FromError(result.Error, this);
    //        }
    //        return Ok(result.Value);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}
    // ============ Civil Layout & Rack Layout Endpoints ============

    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}/civil-layout")]
    public async Task<ActionResult<ConfigurationDto>> GetCivilLayoutList(Guid enquiryId, Guid configId)
    {
        var result = await _mediator.InvokeAsync<ConfigurationDto?>(new GetCivilLayoutByConfigIdQuery(configId));
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpGet("configurations/civil-layout/{Id:guid}")]
    public async Task<ActionResult<CivilLayoutDto>> GetCivilLayoutById(Guid Id)
    {
        var result = await _mediator.InvokeAsync<CivilLayoutDto?>(new GetCivilLayoutByIdQuery(Id));
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpPut("configurations/civil-layout/{Id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> UpdateCivilLayout(Guid Id,[FromForm] UpdateCivilLayoutRequest request)
    {
        var id = await _mediator.InvokeAsync<Guid>(new UpdateCivilLayoutCommand(
            Id, request.WarehouseType, request.SourceFile, request.CivilJson,  User.Identity?.Name));
        return Ok(id);
    }

    [HttpPost("{enquiryId:guid}/configurations/{configId:guid}/civil-layout")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> CreateCivilLayout(Guid enquiryId, Guid configId, [FromForm] SaveCivilLayoutRequest request)
    {
        var id = await _mediator.InvokeAsync<int>(new SaveCivilLayoutCommand(
            configId, request.WarehouseType, request.SourceFile, request.CivilJson,  User.Identity?.Name));
        return CreatedAtAction(nameof(GetCivilLayoutList), new { enquiryId, configId }, id);
    }

    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}/versions/{versionNumber:int}/rack-layout")]
    public async Task<ActionResult<ConfigurationDto>> GetRackLayout(Guid enquiryId, Guid configId, int versionNumber)
    {


        var result = await _mediator.InvokeAsync<ConfigurationDto?>(new GetRackLayoutByVersionIdQuery(configId, versionNumber));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("configurations/rack-layout/{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> SaveRackLayout(Guid id, [FromForm] SaveRackLayoutRequest request)
    {
        JsonDocument? configLayout = null;
        if (!string.IsNullOrEmpty(request.configurationjson))
        {
            configLayout = JsonDocument.Parse(request.configurationjson);
        }

        await _mediator.InvokeAsync<Guid>(new UpdateRackLayoutCommand(
            id,
            request.RackJson,
            configLayout,
            User.Identity?.Name));
        return Ok(id);
    }

    [HttpPost("configurations/{configId:guid}/civil-versions/{civilVersion:int}/config-versions/{configVersion:int}/rack-layout")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> CreateRackLayout(Guid configId, int civilVersion, int configVersion, [FromForm] SaveRackLayoutRequest request)
    {
        JsonDocument? configLayout = null;
        if (!string.IsNullOrEmpty(request.configurationjson))
        {
            configLayout = JsonDocument.Parse(request.configurationjson);
        }

        var id = await _mediator.InvokeAsync<Guid>(new SaveRackLayoutCommand(
            configId,
            civilVersion,
            configVersion,
            request.RackJson,
            configLayout,
            User.Identity?.Name));
        return Ok(id);
    }
}
