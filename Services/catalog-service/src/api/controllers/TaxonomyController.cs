using System.Text.Json;
using CatalogService.Application.dtos;
using CatalogService.Application.Commands;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using CatalogService.Application.Queries.Taxonomy;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CatalogService.Api.Controllers;

/// <summary>
/// Taxonomy management endpoints for component categories, types, and product groups.
/// </summary>
[ApiController]
[Route("api/v1/taxonomy")]
[Produces("application/json")]
public class TaxonomyController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public TaxonomyController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    #region Component Types

    /// <summary>
    /// Get all component types.
    /// </summary>
    /// <param name="componentGroupCode">[OPTIONAL] Filter by component group code.</param>
    /// <param name="componentGroupId">[OPTIONAL] Filter by component group unique identifier.</param>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive types (true/false).</param>
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<ComponentTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTypes(
        [FromQuery] string? componentGroupCode = null,
        [FromQuery] Guid? componentGroupId = null,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<List<ComponentTypeDto>>>(
            new GetAllComponentTypesQuery(componentGroupCode, componentGroupId, includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a component type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component type.</param>
    [HttpGet("types/{id:guid}")]
    [ProducesResponseType(typeof(ComponentTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTypeById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<ComponentTypeDto?>>(new GetComponentTypeByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        if (result.Value == null) return NotFound();
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a component type by code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique code of the component type.</param>
    [HttpGet("types/code/{code}")]
    [ProducesResponseType(typeof(ComponentTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTypeByCode(string code)
    {
        var result = await _mediator.InvokeAsync<Result<ComponentTypeDto?>>(new GetComponentTypeByCodeQuery(code));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        if (result.Value == null) return NotFound();
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new component type.
    /// </summary>
    /// <param name="request">[REQUIRED] The component type creation request details.</param>
    [HttpPost("types")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateType([FromBody] CreateTypeRequest request)
    {
        JsonDocument? attributeSchema = null;
        if (request.AttributeSchema != null)
        {
            attributeSchema = JsonDocument.Parse(JsonSerializer.Serialize(request.AttributeSchema));
        }

        var command = new CreateComponentTypeCommand(
            request.Code,
            request.Name,
            request.ComponentGroupCode,
            request.Description,
            request.ParentTypeCode,
            attributeSchema
        );

        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetTypeById), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Update a component type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component type.</param>
    /// <param name="request">[REQUIRED] The component type update request details.</param>
    [HttpPut("types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpdateTypeRequest request)
    {
        JsonDocument? attributeSchema = null;
        if (request.AttributeSchema != null)
        {
            attributeSchema = JsonDocument.Parse(JsonSerializer.Serialize(request.AttributeSchema));
        }

        var command = new UpdateComponentTypeCommand(
            id,
            request.Name,
            request.Description,
            request.ComponentGroupCode,
            request.ParentTypeCode,
            attributeSchema
        );

        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a component type (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component type.</param>
    [HttpDelete("types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteType(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteComponentTypeCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    #endregion

    #region Product Groups

    /// <summary>
    /// Get all product groups.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive product groups (true/false).</param>
    [HttpGet("product-groups")]
    [ProducesResponseType(typeof(List<ProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProductGroups([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<List<ProductGroupDto>>>(
            new GetAllProductGroupsQuery(includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a product group by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the product group.</param>
    [HttpGet("product-groups/{id:guid}")]
    [ProducesResponseType(typeof(ProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductGroupById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<ProductGroupDto?>>(new GetProductGroupByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        if (result.Value == null) return NotFound();
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a product group by code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique code of the product group.</param>
    [HttpGet("product-groups/code/{code}")]
    [ProducesResponseType(typeof(ProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductGroupByCode(string code)
    {
        var result = await _mediator.InvokeAsync<Result<ProductGroupDto?>>(new GetProductGroupByCodeQuery(code));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        if (result.Value == null) return NotFound();
        return Ok(result.Value);
    }

    /// <summary>
    /// Get variants of a product group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the parent product group.</param>
    [HttpGet("product-groups/{id:guid}/variants")]
    [ProducesResponseType(typeof(List<ProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductGroupVariants(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<List<ProductGroupDto>>>(new GetProductGroupVariantsQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new product group.
    /// </summary>
    /// <param name="request">[REQUIRED] The product group creation request details.</param>
    [HttpPost("product-groups")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductGroup([FromBody] CreateProductGroupRequest request)
    {
        var command = new CreateProductGroupCommand(request.Code, request.Name, request.Description, request.ParentGroupCode);
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetProductGroupById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Update a product group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the product group.</param>
    /// <param name="request">[REQUIRED] The product group update request details.</param>
    [HttpPut("product-groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductGroup(Guid id, [FromBody] UpdateProductGroupRequest request)
    {
        var command = new UpdateProductGroupCommand(id, request.Name, request.Description, request.ParentGroupCode);
        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a product group (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the product group to delete.</param>
    [HttpDelete("product-groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductGroup(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteProductGroupCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    #endregion


    #region Warehouse Type

    /// <summary>
    /// Get all warehouse types.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive warehouse types (true/false).</param>
    [HttpGet("warehouse-types")]
    [ProducesResponseType(typeof(List<WarehouseTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllWarehouseTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<List<WarehouseTypeDto>>>(
            new GetAllWarehouseTypesQuery(includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a warehouse type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type.</param>
    [HttpGet("warehouse-types/{id:guid}")]
    [ProducesResponseType(typeof(WarehouseTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWarehouseTypeById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<WarehouseTypeDto>>(new GetWarehouseTypesByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new warehouse type.
    /// </summary>
    /// <param name="request">[REQUIRED] The warehouse type creation request details.</param>
    [HttpPost("warehouse-types")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWarehouseType([FromForm] CreateWarehouseTypeRequest request)
    {
        Dictionary<string, object>? attributeSchema = null;

        if (!string.IsNullOrWhiteSpace(request.attributes))
        {
            attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                request.attributes);
        }
        var command = new CreateWarehouseTypeCommand(
            request.name,
            request.label,
            request.Icon,
            request.tooltip,
            request.templatePath_civil,
            request.templatePath_json,
            attributeSchema,
            request.createdBy
        );

        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetWarehouseTypeById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Update a warehouse type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type.</param>
    /// <param name="command">[REQUIRED] The update command details.</param>
    [HttpPut("warehouse-types/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWarehouseType(Guid id, [FromForm] UpdateWarehouseTypeRequest request)
    {
        Dictionary<string, object>? attributeSchema = null;

        if (!string.IsNullOrWhiteSpace(request.attributes))
        {
            attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                request.attributes);
        }
        var command = new UpdateWarehouseTypeCommand(
            request.Id,
            request.name,
            request.label,
            request.Icon,
            request.tooltip,
            request.templatePath_civil,
            request.templatePath_json,
           attributeSchema,
            request.IsActive,
            request.updatedby
        );
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a warehouse type (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type to delete.</param>
    [HttpDelete("warehouse-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWarehouseType(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteWarehouseTypeCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }
    #endregion

    #region Civil Component

    /// <summary>
    /// Get all civil components.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive civil components (true/false).</param>
    [HttpGet("civil-components")]
    [ProducesResponseType(typeof(List<CivilComponentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCivilComponents([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<List<CivilComponentDto>>>(
            new GetAllCivileComponentQuery(includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a civil component by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component.</param>
    [HttpGet("civil-components/{id:guid}")]
    [ProducesResponseType(typeof(CivilComponentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCivilComponentById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<CivilComponentDto>>(new GetCivileComponentByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new civil component.
    /// </summary>
    /// <param name="request">[REQUIRED] The civil component creation request details.</param>
    [HttpPost("civil-components")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCivilComponent([FromForm] CreateCivileComponentRequest request)
    {
        Dictionary<string, object>? attributeSchema = null;

        if (!string.IsNullOrWhiteSpace(request.DefaultElement))
        {
            attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                request.DefaultElement);
        }
        var command = new CreateCivilComponentCommand(
            request.Code,
            request.Name,
            request.Label,
            request.Icon,
            request.Tooltip,
            request.Category,
            attributeSchema,
            request.CreatedBy
        );

        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetCivilComponentById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Update a civil component.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    [HttpPut("civil-components/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCivilComponent(Guid id, [FromForm] UpdateCivileComponentRequest request)
    {
        Dictionary<string, object>? attributeSchema = null;

        if (!string.IsNullOrWhiteSpace(request.DefaultElement))
        {
            attributeSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                request.DefaultElement);
        }
        var command = new UpdateCivilComponentCommand(
            id,
            request.Code,
            request.Name,
            request.Label,
            request.Icon,
            request.Tooltip,
            request.Category,
            attributeSchema,
            request.IsActive,
            request.updatedBy
        );
        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a civil component (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component to delete.</param>
    [HttpDelete("civil-components/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCivilComponent(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteCivilComponentCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }
    #endregion

    #region Component Groups

    /// <summary>
    /// Get all component groups.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive component groups (true/false).</param>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(IEnumerable<ComponentGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllGroups([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<ComponentGroupDto>>>(
            new GetAllComponentGroupsQuery(includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a component group by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component group.</param>
    [HttpGet("groups/{id:guid}")]
    [ProducesResponseType(typeof(ComponentGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<ComponentGroupDto>>(new GetComponentGroupByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a component group by code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique code of the component group.</param>
    [HttpGet("groups/code/{code}")]
    [ProducesResponseType(typeof(ComponentGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupByCode(string code)
    {
        var result = await _mediator.InvokeAsync<Result<ComponentGroupDto>>(new GetComponentGroupByCodeQuery(code));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new component group.
    /// </summary>
    /// <param name="request">[REQUIRED] The component group creation request details.</param>
    [HttpPost("groups")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateComponentGroupRequest request)
    {
        var command = new CreateComponentGroupCommand(request.Code, request.Name, request.Description, request.SortOrder);
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetGroupById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Update a component group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component group.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    [HttpPut("groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateComponentGroupRequest request)
    {
        var command = new UpdateComponentGroupCommand(id, request.Name, request.Description, request.SortOrder);
        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a component group (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component group to delete.</param>
    [HttpDelete("groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteComponentGroupCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    #endregion

    #region Component Names

    /// <summary>
    /// Get all component names.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive component names (true/false).</param>
    [HttpGet("names")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllNames([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<ComponentNameDto>>>(
            new GetAllComponentNamesQuery(includeInactive));
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }
    
    /// <summary>
    /// Get component names by type.
    /// </summary>
    /// <param name="typeId">[REQUIRED] The unique identifier of the component type.</param>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive names (true/false).</param>
    [HttpGet("names/by-type/{typeId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNamesByType(Guid typeId, [FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<ComponentNameDto>>>(
            new GetComponentNamesByTypeQuery(typeId, includeInactive));

        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get a component name by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component name.</param>
    [HttpGet("names/{id:guid}")]
    [ProducesResponseType(typeof(ComponentNameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNameById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<ComponentNameDto>>(new GetComponentNameByIdQuery(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new component name.
    /// </summary>
    /// <param name="request">[REQUIRED] The component name creation request details.</param>
    [HttpPost("names")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateName([FromBody] CreateComponentNameRequest request)
    {
        var command = new CreateComponentNameCommand(request.Code, request.Name, request.Description, request.ComponentTypeId);
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return CreatedAtAction(nameof(GetNameById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Update a component name.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component name.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    [HttpPut("names/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateComponentNameRequest request)
    {
        var command = new UpdateComponentNameCommand(id, request.Name, request.Description, request.ComponentTypeId);
        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    /// <summary>
    /// Delete a component name (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component name to delete.</param>
    [HttpDelete("names/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteName(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteComponentNameCommand(id));
        if (result.IsFailure) return CustomErrorResults.FromError(result.Error, this);
        return Ok();
    }

    #endregion
}

// Request DTOs
/// <summary>
/// Request DTO for creating a new component type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="ComponentGroupCode">[REQUIRED] Associated component group code.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ParentTypeCode">[OPTIONAL] Code of the parent type if any.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
public record CreateTypeRequest(
    string Code,
    string Name,
    string ComponentGroupCode,
    string? Description = null,
    string? ParentTypeCode = null,
    object? AttributeSchema = null
);

/// <summary>
/// Request DTO for updating an existing component type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ComponentGroupCode">[REQUIRED] Associated component group code.</param>
/// <param name="ParentTypeCode">[OPTIONAL] Code of the parent type if any.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
public record UpdateTypeRequest(
    string Name,
    string? Description,
    string ComponentGroupCode,
    string? ParentTypeCode,
    object? AttributeSchema
);

/// <summary>
/// Request DTO for creating a new product group.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="ParentGroupCode">[OPTIONAL] Code of the parent group if any.</param>
public record CreateProductGroupRequest(string Code, string Name, string? Description = null, string? ParentGroupCode = null);
/// <summary>
/// Request DTO for updating an existing product group.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="ParentGroupCode">[OPTIONAL] Code of the parent group if any.</param>
public record UpdateProductGroupRequest(string Name, string? Description, string? ParentGroupCode);

/// <summary>
/// Request DTO for creating a new warehouse type.
/// </summary>
/// <param name="name">[REQUIRED] Internal name.</param>
/// <param name="label">[REQUIRED] Display label.</param>
/// <param name="Icon">[REQUIRED] Icon identifier or URL.</param>
/// <param name="tooltip">[OPTIONAL] Tooltip text.</param>
/// <param name="templatePath_civil">[OPTIONAL] Path to civil template.</param>
/// <param name="templatePath_json">[OPTIONAL] Path to JSON template.</param>
/// <param name="attributes">[OPTIONAL] Dynamic attributes.</param>
/// <param name="createdBy">[OPTIONAL] User identifier.</param>
public record CreateWarehouseTypeRequest(string name, string label, IFormFile Icon, string? tooltip, IFormFile? templatePath_civil, IFormFile? templatePath_json, string? attributes, string? createdBy);

///<summary>
/// Request DTO for updating an existing warehouse type.
/// </summary>
/// <param name="Id">[REQUIRED] Unique identifier.</param>
/// <param name="name">[REQUIRED] Internal name.</param>
/// <param name="label">[REQUIRED] Display label.</param>
/// <param name="Icon">[REQUIRED] Icon identifier or URL.</param>
/// <param name="tooltip">[OPTIONAL] Tooltip text.</param>
/// <param name="templatePath_civil">[OPTIONAL] Path to civil template.</param>
/// <param name="templatePath_json">[OPTIONAL] Path to JSON template.</param>
/// <param name="attributes">[OPTIONAL] Dynamic attributes.</param>
/// <param name="updatedBy">[OPTIONAL] User identifier.</param>
public record UpdateWarehouseTypeRequest(Guid Id, string name, string label, IFormFile Icon, string? tooltip, IFormFile? templatePath_civil, IFormFile? templatePath_json, string? attributes, bool IsActive, string? updatedby);
/// <summary>
/// Request DTO for creating a new civil component.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Label">[REQUIRED] Display label.</param>
/// <param name="Icon">[REQUIRED] Icon identifier.</param>
/// <param name="Tooltip">[OPTIONAL] Tooltip text.</param>
/// <param name="Category">[REQUIRED] Component category.</param>
/// <param name="DefaultElement">[OPTIONAL] Default JSON element.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateCivileComponentRequest(string Code,
    string Name,
    string Label,
    IFormFile Icon,
    string? Tooltip,
    string Category,
    string? DefaultElement,
    string? CreatedBy);
/// <summary>
/// Request DTO for updating an existing civil component.
/// </summary>
/// <param name="Id">[REQUIRED] Unique identifier.</param>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Label">[REQUIRED] Display label.</param>
/// <param name="Icon">[REQUIRED] Icon identifier.</param>
/// <param name="Tooltip">[OPTIONAL] Tooltip text.</param>
/// <param name="Category">[REQUIRED] Component category.</param>
/// <param name="DefaultElement">[OPTIONAL] Default JSON element.</param>
/// <param name="IsActive">[REQUIRED] Active status.</param>
/// <param name="updatedBy">[OPTIONAL] User identifier.</param>
public record UpdateCivileComponentRequest(Guid Id,
    string Code,
    string Name,
    string Label,
    IFormFile Icon,
    string? Tooltip,
    string Category,
    string? DefaultElement,
    bool IsActive,
    string? updatedBy);

/// <summary>
/// Request DTO for creating a new component group.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="SortOrder">[REQUIRED] Order in list.</param>
public record CreateComponentGroupRequest(string Code, string Name, string? Description, int SortOrder);
/// <summary>
/// Request DTO for updating an existing component group.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="SortOrder">[REQUIRED] Order in list.</param>
public record UpdateComponentGroupRequest(string Name, string? Description, int SortOrder);
/// <summary>
/// Request DTO for creating a new component name.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type ID.</param>
public record CreateComponentNameRequest(string Code, string Name, string? Description, Guid ComponentTypeId);
/// <summary>
/// Request DTO for updating an existing component name.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Description.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type ID.</param>
public record UpdateComponentNameRequest(string Name, string? Description, Guid ComponentTypeId);