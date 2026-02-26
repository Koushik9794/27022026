using System.Text.Json;
using CatalogService.Application.commands.attributes;
using CatalogService.Application.dtos;
using CatalogService.Application.queries.Attributes;
using CatalogService.Application.queries.Mhe;
using CatalogService.Domain.Enums;
using GssCommon.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using System.ComponentModel.DataAnnotations;
namespace CatalogService.api.controllers;

[ApiController]
[Route("api/v1/attributes")]
public class AttributeController(IMessageBus _mediator) : ControllerBase
{
    /// <summary>
    /// Get all active Attributes.
    /// </summary>
    /// <param name="IsActive">[OPTIONAL] Filter by active status (true/false).</param>
    /// <remarks>
    /// Returns a list of all active (non-deleted) Attributes in the catalog.
    /// </remarks>
    /// <response code="200">Returns the list of Attributes</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(bool? IsActive)
    {
        try
        {
            //var result = await _mediator.InvokeAsync<List<MheDto>>(new GetAllMhesQuery(IsActive));
            //return Ok(result);

            Result<List<AttributeDefinitionDto>> result = await _mediator.InvokeAsync<Result<List<AttributeDefinitionDto>>>(new GetAllAttributesQuery(IsActive));

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
    /// Get a specific Attribute by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the Attribute.</param>
    /// <remarks>
    /// Retrieves a single Attribute by its unique identifier.
    /// </remarks>
    /// <response code="200">Returns the Attribute</response>
    /// <response code="404">Attribute not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {


            Result<AttributeDefinitionDto?> result = await _mediator.InvokeAsync<Result<AttributeDefinitionDto?>>(new GetAttributesByIdQuery(id));
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
    /// Create a new Attribute.
    /// </summary>
    /// <param name="request">[REQUIRED] The Attribute creation request containing key details like DataType and Key.</param>
    /// <remarks>
    /// Creates a new Attribute definition in the catalog, which can be used to store dynamic properties for various entities (MHE, SKU, Pallet, etc.).
    ///     POST /api/v1/attribute
    ///     {
    ///          "attributeKey": "Capacity",
    ///          "displayName": "Capacity",
    ///          "unit": "KG",
    ///           "dataType": ( 1 as number ,2as enum,3as string,4 as boolean),
    ///            "minValue": 1200,
    ///             "maxValue": 4000,
    ///             "defaultValue": "2000",
    ///             "isRequired": true,
    ///             "allowedValues": "[2000,3000,4000]",
    ///             "description": "Define Cap",
    ///             screen :    MHE=0, //MHE 
    ///                            SKU=1, //SKu
    ///                            PALLET=2, //Pallet
    ///                            ITEM=3, // Part master
    ///                            PG=4, //Product Group
    ///                            CG=5, //Catagory Group
    ///                            CT=6, //Catagory Type
    ///                            CA=7, //Catagory
    ///                           WT=8, //Warehouse Type
    ///                           CC=9 //Civil Component
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Attribute created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// 

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttributeRequest request)
    {
        try
        {
            var command = new CreateattributeCommand(
                request.AttributeKey,
                request.DisplayName,
                request.DataType,
                request.Unit,
                request.MinValue,
                request.MaxValue,
                request.DefaultValue,
                request.IsRequired,
                request.AllowedValues,
                request.Description,
                 request.Screen,
                request.IsActive,
                request.CreatedBy
            );

            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(command);
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
    /// Update an existing Attribute.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the Attribute to update.</param>
    /// <param name="request">[REQUIRED] The Attribute update request details.</param>
    /// <remarks>
    /// Updates an existing Attribute with new values.
    /// </remarks>
    /// <response code="200">Attribute updated successfully</response>
    /// <response code="404">Attribute not found</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttributeRequest request)
    {
        try
        {
            var command = new UpdateattributeCommand(
                id,
                request.AttributeKey,
                request.DisplayName,
                request.DataType,
                request.Unit,
                request.MinValue,
                request.MaxValue,
                request.DefaultValue,
                request.IsRequired,
                request.AllowedValues,
                request.Description,
                request.Screen,
                request.IsActive,
                request.UpdatedBy
            );


            Result<Guid> result = await _mediator.InvokeAsync<Result<Guid>>(command);
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
    /// Delete an Attribute (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the Attribute to delete.</param>
    /// <param name="deletedBy">[OPTIONAL] The user performing the deletion.</param>
    /// <remarks>
    /// Performs a soft delete on the Attribute. The definition will be marked as deleted but not removed from the database.
    /// </remarks>
    /// <response code="200">Attribute deleted successfully</response>
    /// <response code="404">Attribute not found</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? deletedBy)
    {
        try
        {
            var command = new DeleteattributeCommand(id, deletedBy);


            Result<bool> result = await _mediator.InvokeAsync<Result<bool>>(command);
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
}


/// <summary>
/// Request DTO for creating a new attribute definition.
/// </summary>
/// <param name="AttributeKey">[REQUIRED] Internal key for the attribute (e.g., 'capacity').</param>
/// <param name="DisplayName">[REQUIRED] User-facing label (e.g., 'Load Capacity').</param>
/// <param name="Unit">[OPTIONAL] Unit of measure (e.g., 'KG', 'MM').</param>
/// <param name="DataType">[REQUIRED] Data type: 1=Number, 2=Enum, 3=String, 4=Boolean.</param>
/// <param name="MinValue">[OPTIONAL] Minimum allowed value for numeric types.</param>
/// <param name="MaxValue">[OPTIONAL] Maximum allowed value for numeric types.</param>
/// <param name="DefaultValue">[OPTIONAL] JSON value for default.</param>
/// <param name="IsRequired">[REQUIRED] Whether this attribute must be populated on and entity.</param>
/// <param name="AllowedValues">[OPTIONAL] JSON array of allowed values for Enum types.</param>
/// <param name="Description">[OPTIONAL] Internal description.</param>
/// <param name="Screen">[REQUIRED] Target entity type (0=MHE, 1=SKU, 2=Pallet, 3=Part, etc.).</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateAttributeRequest(
    [Required] string AttributeKey, 
    [Required] string DisplayName, 
    string? Unit, 
    [Required] AttributeDataType DataType, 
    decimal? MinValue, 
    decimal? MaxValue, 
    JsonElement? DefaultValue, 
    [Required] bool IsRequired, 
    JsonElement? AllowedValues, 
    string? Description, 
    [Required] AttributeScreen Screen, 
    [Required] bool IsActive, 
    string? CreatedBy);

/// <summary>
/// Request DTO for updating an existing attribute definition.
/// </summary>
/// <param name="AttributeKey">[REQUIRED] Internal key for the attribute (e.g., 'capacity').</param>
/// <param name="DisplayName">[REQUIRED] User-facing label (e.g., 'Load Capacity').</param>
/// <param name="Unit">[OPTIONAL] Unit of measure (e.g., 'KG', 'MM').</param>
/// <param name="DataType">[REQUIRED] Data type: 1=Number, 2=Enum, 3=String, 4=Boolean.</param>
/// <param name="MinValue">[OPTIONAL] Minimum allowed value for numeric types.</param>
/// <param name="MaxValue">[OPTIONAL] Maximum allowed value for numeric types.</param>
/// <param name="DefaultValue">[OPTIONAL] JSON value for default.</param>
/// <param name="IsRequired">[REQUIRED] Whether this attribute must be populated on and entity.</param>
/// <param name="AllowedValues">[OPTIONAL] JSON array of allowed values for Enum types.</param>
/// <param name="Description">[OPTIONAL] Internal description.</param>
/// <param name="Screen">[REQUIRED] Target entity type (0=MHE, 1=SKU, 2=Pallet, 3=Part, etc.).</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier performing the update.</param>
public record UpdateAttributeRequest(
    [Required] string AttributeKey, 
    [Required] string DisplayName, 
    string? Unit, 
    [Required] AttributeDataType DataType, 
    decimal? MinValue, 
    decimal? MaxValue, 
    JsonElement? DefaultValue, 
    [Required] bool IsRequired, 
    JsonElement? AllowedValues, 
    string? Description, 
    [Required] AttributeScreen Screen, 
    [Required] bool IsActive, 
    string? UpdatedBy);

