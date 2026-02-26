using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ProductGroupErrors
{
    public static readonly Error NotFound = Error.NotFound("ProductGroup.NotFound", "Product group not found.");
    public static readonly Error DuplicateCode = Error.Conflict("ProductGroup.DuplicateCode", "Product group with this code already exists.");
    public static readonly Error ParentNotFound = Error.NotFound("ProductGroup.ParentNotFound", "Parent product group not found.");
}
