using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Taxonomy (Groups, Types, Names, Product Groups).
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/taxonomy")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminTaxonomyController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminTaxonomyController> _logger;

    public AdminTaxonomyController(ICatalogServiceClient catalogClient, ILogger<AdminTaxonomyController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    #region Component Groups
    /// <summary>
    /// Get all component groups.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive groups (default: false).</param>
    [HttpGet("groups")]
    [ProducesResponseType(typeof(IEnumerable<ComponentGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups([FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetComponentGroupsAsync(includeInactive);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.GetComponentGroupByIdAsync(id);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.CreateComponentGroupAsync(request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Update an existing component group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component group.</param>
    /// <param name="request">[REQUIRED] The component group update request details.</param>
    [HttpPut("groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateComponentGroupRequest request)
    {
        var response = await _catalogClient.UpdateComponentGroupAsync(id, request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Delete a component group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component group to delete.</param>
    [HttpDelete("groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var response = await _catalogClient.DeleteComponentGroupAsync(id);
        return await ProxyResponse(response);
    }
    #endregion

    #region Component Types
    /// <summary>
    /// Get all component types.
    /// </summary>
    /// <param name="componentGroupCode">[OPTIONAL] Filter by component group code.</param>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive types (default: false).</param>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IEnumerable<ComponentTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypes([FromQuery] string? componentGroupCode = null, [FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetComponentTypesAsync(componentGroupCode, includeInactive);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.GetComponentTypeByIdAsync(id);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Create a new component type.
    /// </summary>
    /// <param name="request">[REQUIRED] The component type creation request details.</param>
    [HttpPost("types")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateType([FromBody] CreateComponentTypeRequest request)
    {
        var response = await _catalogClient.CreateComponentTypeAsync(request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Update an existing component type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component type.</param>
    /// <param name="request">[REQUIRED] The component type update request details.</param>
    [HttpPut("types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpdateComponentTypeRequest request)
    {
        var response = await _catalogClient.UpdateComponentTypeAsync(id, request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Delete a component type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component type to delete.</param>
    [HttpDelete("types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteType(Guid id)
    {
        var response = await _catalogClient.DeleteComponentTypeAsync(id);
        return await ProxyResponse(response);
    }
    #endregion

    #region Component Names
    /// <summary>
    /// Get all component names.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive names (default: false).</param>
    [HttpGet("names")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNames([FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetComponentNamesAsync(includeInactive);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Get component names by type.
    /// </summary>
    /// <param name="typeId">[REQUIRED] The unique identifier of the component type.</param>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive names (default: false).</param>
    [HttpGet("names/by-type/{typeId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNamesByType(Guid typeId, [FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetComponentNamesByTypeAsync(typeId, includeInactive);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.GetComponentNameByIdAsync(id);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.CreateComponentNameAsync(request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Update an existing component name.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component name.</param>
    /// <param name="request">[REQUIRED] The component name update request details.</param>
    [HttpPut("names/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateComponentNameRequest request)
    {
        var response = await _catalogClient.UpdateComponentNameAsync(id, request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Delete a component name.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the component name to delete.</param>
    [HttpDelete("names/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteName(Guid id)
    {
        var response = await _catalogClient.DeleteComponentNameAsync(id);
        return await ProxyResponse(response);
    }
    #endregion

    #region Product Groups
    /// <summary>
    /// Get all product groups.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive product groups (default: false).</param>
    [HttpGet("product-groups")]
    [ProducesResponseType(typeof(IEnumerable<ProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductGroups([FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetProductGroupsAsync(includeInactive);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.GetProductGroupByIdAsync(id);
        return await ProxyResponse(response);
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
        var response = await _catalogClient.CreateProductGroupAsync(request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Update an existing product group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the product group.</param>
    /// <param name="request">[REQUIRED] The product group update request details.</param>
    [HttpPut("product-groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductGroup(Guid id, [FromBody] UpdateProductGroupRequest request)
    {
        var response = await _catalogClient.UpdateProductGroupAsync(id, request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Delete a product group.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the product group to delete.</param>
    [HttpDelete("product-groups/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductGroup(Guid id)
    {
        var response = await _catalogClient.DeleteProductGroupAsync(id);
        return await ProxyResponse(response);
    }
    #endregion

    #region Warehouse Types

    /// <summary>
    /// Get all Warehouse Types.
    /// </summary>
    [HttpGet("warehouse-types")]
    [ProducesResponseType(typeof(IEnumerable<WarehouseTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarehouseType()
    {
        var response = await _catalogClient.GetWareHouseTypesAsync();
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a Warehouse Type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type.</param>
    [HttpGet("warehouse-types/{id:guid}")]
    [ProducesResponseType(typeof(WarehouseTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWareHouseTypeById(Guid id)
    {
        var response = await _catalogClient.GetProductGroupByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new Warehouse Type.
    /// </summary>
    /// <param name="request">[REQUIRED] The warehouse type creation request details.</param>
    [HttpPost("warehouse-types")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWareHouseType([FromBody] object request)
    {
        var response = await _catalogClient.CreateWareHouseTypesAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing Warehouse Type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type.</param>
    /// <param name="request">[REQUIRED] The warehouse type update request details.</param>
    [HttpPut("warehouse-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWareHouseType(Guid id, [FromBody] object request)
    {
        var response = await _catalogClient.UpdateWareHouseTypesAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a Warehouse Type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the warehouse type to delete.</param>
    [HttpDelete("warehouse-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWareHouseType(Guid id)
    {
        var response = await _catalogClient.DeleteWareHouseTypesAsync(id);
        return await ProxyResponse(response);
    }

    #endregion


    #region Civil Components

    /// <summary>
    /// Get all Civil Components.
    /// </summary>
    [HttpGet("Civil-Components")]
    [ProducesResponseType(typeof(IEnumerable<CivilComponentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCivilComponentsType()
    {
        var response = await _catalogClient.GetCivilComponentsAsync();
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a Civil Component by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component.</param>
    [HttpGet("Civil-Components/{id:guid}")]
    [ProducesResponseType(typeof(CivilComponentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeCivilComponentById(Guid id)
    {
        var response = await _catalogClient.GetCivilComponentsByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new Civil Component.
    /// </summary>
    /// <param name="request">[REQUIRED] The civil component creation request details.</param>
    [HttpPost("Civil-Components")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCivilComponentType([FromBody] object request)
    {
        var response = await _catalogClient.CreateCivilComponentsAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing Civil Component.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component.</param>
    /// <param name="request">[REQUIRED] The civil component update request details.</param>
    [HttpPut("Civil-Components/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCivilComponentType(Guid id, [FromBody] object request)
    {
        var response = await _catalogClient.UpdateCivilComponentsAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a Civil Component.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the civil component to delete.</param>
    [HttpDelete("Civil-Components/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCivilComponentType(Guid id)
    {
        var response = await _catalogClient.DeleteCivilComponentsAsync(id);
        return await ProxyResponse(response);
    }

    #endregion

    private async Task<IActionResult> ProxyResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
