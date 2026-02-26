using System.Text.Json;

namespace CatalogService.Application.Commands.Taxonomy;

// Component Type Commands
public record CreateComponentTypeCommand(
    string Code,
    string Name,
    string ComponentGroupCode,
    string? Description = null,
    string? ParentTypeCode = null,
    JsonDocument? AttributeSchema = null
);

public record UpdateComponentTypeCommand(
    Guid Id,
    string Name,
    string? Description,
    string ComponentGroupCode,
    string? ParentTypeCode,
    JsonDocument? AttributeSchema
);

public record DeleteComponentTypeCommand(Guid Id);

// Product Group Commands
public record CreateProductGroupCommand(string Code, string Name, string? Description = null, string? ParentGroupCode = null);
public record UpdateProductGroupCommand(Guid Id, string Name, string? Description, string? ParentGroupCode);
public record DeleteProductGroupCommand(Guid Id);

// Warehouse Type Commands
public record CreateWarehouseTypeCommand(string name, string label, IFormFile Icon, string? tooltip, IFormFile? templatePath_Civil, IFormFile? templatePath_Json, Dictionary<string, object>? attributes, string? createdBy);
public record UpdateWarehouseTypeCommand(Guid Id,string name, string label, IFormFile Icon, string? tooltip, IFormFile? templatePath_Civil, IFormFile? templatePath_Json, Dictionary<string, object>? attributes,bool IsActive, string? UpdateddBy);
public record DeleteWarehouseTypeCommand(Guid Id);
public sealed record CreateCivilComponentCommand(
    string Code,
    string Name,
    string Label,
    IFormFile Icon,
    string? Tooltip,
    string Category,
    Dictionary<string, object>? DefaultElement,
    string? CreatedBy
);
public sealed record UpdateCivilComponentCommand(
    Guid Id,
    string Code,
    string Name,
    string Label,
    IFormFile Icon,
    string? Tooltip,
    string Category,
    Dictionary<string, object>? DefaultElement,
    bool IsActive,
    string? updatedBy
);
public record DeleteCivilComponentCommand(Guid Id);
