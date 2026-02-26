using System.Text.Json;
using CatalogService.Application.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CatalogService.Application.Handlers.LoadCharts;

public class ImportLoadChartExcelCommandHandler
{
    private readonly ILoadChartRepository _repository;
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly IComponentTypeRepository _componentTypeRepository;
    private readonly IComponentNameRepository _componentNameRepository;

    public ImportLoadChartExcelCommandHandler(
        ILoadChartRepository repository,
        IProductGroupRepository productGroupRepository,
        IComponentTypeRepository componentTypeRepository,
        IComponentNameRepository componentNameRepository)
    {
        _repository = repository;
        _productGroupRepository = productGroupRepository;
        _componentTypeRepository = componentTypeRepository;
        _componentNameRepository = componentNameRepository;
    }

    public async Task<Result<int>> Handle(ImportLoadChartExcelCommand command)
    {
        if (command.File == null || command.File.Length == 0)
            return Result.Failure<int>(Error.Validation("File.Empty", "File is empty."));

        try
        {
            using var stream = command.File.OpenReadStream();
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet? sheet = null;
            
            // Evaluator for formulas
            var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            
            // Look for non-empty sheets, but skip internal/hidden ones if possible
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                var s = workbook.GetSheetAt(i);
                if (s != null && s.LastRowNum > 0 && !workbook.IsSheetHidden(i))
                {
                    sheet = s;
                    break;
                }
            }

            if (sheet == null)
                return Result.Failure<int>(Error.Validation("Sheet.NotFound", "No visible sheet with data found."));

            var formatter = new DataFormatter();
            
            // UNIVERSAL HEADER DETECTION
            Dictionary<string, int> columnMap = [];
            int headerRowIndex = FindHeaderRow(sheet, out columnMap);

            if (headerRowIndex == -1)
            {
                // Try row 0 as absolute fallback if scoring failed but sheet has data
                var row0 = sheet.GetRow(0);
                if (row0 != null)
                {
                    columnMap = GetColumnMap(row0);
                    headerRowIndex = 0;
                }
                
                if (columnMap.Count < 2)
                    return Result.Failure<int>(Error.Validation("Header.NotFound", "Could not reliably detect a header row. Please ensure columns have titles."));
            }

            int importedCount = 0;
            
            // DETECT FORMAT
            var isFlat = columnMap.ContainsKey("Component Type") || columnMap.ContainsKey("Type");
            var isMatrix = !isFlat && (columnMap.ContainsKey("BEAM SPAN") || columnMap.ContainsKey("SPAN") || columnMap.Count > 3);

            var cTypes = await _componentTypeRepository.GetAllAsync(null);
            var componentTypesMap = cTypes
                .GroupBy(ct => ct.Name.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First().Id);

            if (isMatrix)
            {
                // RESOLVE COMPONENT TYPE FOR MATRIX
                // Default to UPRIGHT, but try to infer from columns or ChartType
                Guid resolvedTypeId = Guid.Empty;
                string typeCategory = "UPRIGHT";

                if (command.ChartType.Contains("BEAM", StringComparison.OrdinalIgnoreCase)) typeCategory = "BEAM";
                if (columnMap.ContainsKey("UPRIGHT")) typeCategory = "UPRIGHT";
                if (columnMap.ContainsKey("BEAM") || columnMap.ContainsKey("BEAM CODE")) typeCategory = "BEAM";

                if (!componentTypesMap.TryGetValue(typeCategory, out resolvedTypeId))
                {
                    var fallbackKey = componentTypesMap.Keys.FirstOrDefault(k => k.Contains(typeCategory));
                    if (fallbackKey != null) resolvedTypeId = componentTypesMap[fallbackKey];
                    else resolvedTypeId = componentTypesMap.Values.FirstOrDefault(); // Absolute fallback: first type found
                }

                if (resolvedTypeId == Guid.Empty)
                    return Result.Failure<int>(Error.NotFound("ComponentType", $"Could not find a valid Component Type for {typeCategory}."));

                var reservedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { 
                    "SL.no", "NO", "SR NO", "BEAM SPAN", "SPAN", "LOAD/ LEVEL", "LOAD", "UPRIGHT", "BEAM", "CAPACITY" 
                };
                var componentCodeCols = columnMap.Keys.Where(h => !reservedHeaders.Contains(h)).ToList();

                // Row keys (e.g. Span)
                var rowKeyCol = columnMap.Keys.FirstOrDefault(h => h.Contains("SPAN")) ?? 
                                columnMap.Keys.FirstOrDefault(h => h.Contains("NO")) ?? 
                                columnMap.Keys.First();

                // Cache existing component names for this type
                var existingNames = await _componentNameRepository.GetByTypeIdAsync(resolvedTypeId, true);
                var componentNamesCache = new HashSet<string>(existingNames.Select(n => n.Code), StringComparer.OrdinalIgnoreCase);

                string? lastRowKeyVal = null;
                string? lastLoadVal = null;

                for (int i = headerRowIndex + 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || IsRowEmpty(row)) continue;

                    var currentRowKeyVal = GetValue(row, columnMap, rowKeyCol, formatter, evaluator);
                    var currentLoadVal = GetValue(row, columnMap, "LOAD/ LEVEL", formatter, evaluator) ?? GetValue(row, columnMap, "LOAD", formatter, evaluator);

                    // Carry forward values if current cells are empty (handles merged cells/groups)
                    if (!string.IsNullOrWhiteSpace(currentRowKeyVal)) lastRowKeyVal = currentRowKeyVal;
                    if (!string.IsNullOrWhiteSpace(currentLoadVal)) lastLoadVal = currentLoadVal;

                    foreach (var componentCode in componentCodeCols)
                    {
                        var capacityValue = GetValue(row, columnMap, componentCode, formatter, evaluator);
                        if (string.IsNullOrWhiteSpace(capacityValue) || capacityValue == "0" || capacityValue == "-") continue;

                        var normalizedCode = componentCode.Trim().ToUpperInvariant();

                        if (!componentNamesCache.Contains(normalizedCode))
                        {
                            var newName = ComponentName.Create(normalizedCode, componentCode, resolvedTypeId, "Auto-created during LoadChart import");
                            await _componentNameRepository.CreateAsync(newName);
                            componentNamesCache.Add(normalizedCode);
                        }

                        var attributes = new Dictionary<string, object>
                        {
                            [rowKeyCol.Replace(" ", "_").ToUpperInvariant()] = lastRowKeyVal ?? "",
                            ["CAPACITY"] = capacityValue
                        };
                        if (!string.IsNullOrEmpty(lastLoadVal)) attributes["LOAD"] = lastLoadVal;

                        var attributeDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(attributes)) ?? [];

                        var loadChart = LoadChart.Create(
                            command.ProductGroupId,
                            command.ChartType,
                            normalizedCode,
                            resolvedTypeId,
                            attributeDict,
                            command.CreatedBy ?? Guid.Empty
                        );

                        await _repository.CreateAsync(loadChart);
                        importedCount++;
                    }
                }
            }
            else
            {
                // Flat Format (or fallback)
                // Determine mandatory columns or use whatever is available
                var codeCol = columnMap.Keys.FirstOrDefault(h => h.Contains("Code")) ?? columnMap.Keys.FirstOrDefault(h => h.Contains("Name")) ?? columnMap.Keys.First();
                var typeCol = columnMap.Keys.FirstOrDefault(h => h.Contains("Type")) ?? columnMap.Keys.FirstOrDefault(h => h.Contains("Category"));

                for (int i = headerRowIndex + 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || IsRowEmpty(row)) continue;

                    var componentCode = GetValue(row, columnMap, codeCol, formatter, evaluator);
                    if (string.IsNullOrWhiteSpace(componentCode)) continue;

                    Guid componentTypeId = Guid.Empty;
                    if (typeCol != null)
                    {
                        var typeName = GetValue(row, columnMap, typeCol, formatter, evaluator);
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            var key = typeName.Trim().ToUpperInvariant();
                            if (!componentTypesMap.TryGetValue(key, out componentTypeId))
                            {
                                var fallback = componentTypesMap.Keys.FirstOrDefault(k => k.Contains(key));
                                if (fallback != null) componentTypeId = componentTypesMap[fallback];
                            }
                        }
                    }

                    if (componentTypeId == Guid.Empty)
                        componentTypeId = componentTypesMap.Values.FirstOrDefault();

                    var normalizedCode = componentCode.Trim().ToUpperInvariant();

                    // Check/Create Component Name
                    var existingNames = await _componentNameRepository.GetByTypeIdAsync(componentTypeId, true);
                    var componentNamesCache = new HashSet<string>(existingNames.Select(n => n.Code), StringComparer.OrdinalIgnoreCase);

                    if (!componentNamesCache.Contains(normalizedCode))
                    {
                        var newName = ComponentName.Create(normalizedCode, componentCode, componentTypeId, "Auto-created during LoadChart import");
                        await _componentNameRepository.CreateAsync(newName);
                    }

                    var attributes = new Dictionary<string, object>();
                    foreach (var header in columnMap.Keys)
                    {
                        if (header == codeCol || header == typeCol) continue;
                        var val = GetValue(row, columnMap, header, formatter, evaluator);
                        if (!string.IsNullOrWhiteSpace(val)) attributes[header.Replace(" ", "_").ToUpperInvariant()] = val;
                    }

                    var attributeDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(attributes)) ?? [];

                    var loadChart = LoadChart.Create(
                        command.ProductGroupId,
                        command.ChartType,
                        normalizedCode,
                        componentTypeId,
                        attributeDict,
                        command.CreatedBy ?? Guid.Empty
                    );

                    await _repository.CreateAsync(loadChart);
                    importedCount++;
                }
            }
            return Result.Success(importedCount);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(Error.Failure("Import.Failed", $"Excel import failed: {ex.Message}"));
        }
    }

    private int FindHeaderRow(ISheet sheet, out Dictionary<string, int> columnMap)
    {
        columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int bestRowIndex = -1;
        int bestScore = -1;

        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
        { 
            "Component Code", "Component Type", "BEAM SPAN", "LOAD/ LEVEL", 
            "UPRIGHT", "SPAN", "BEAM", "CAPACITY", "CODE", "TYPE" 
        };

        for (int i = 0; i <= Math.Min(20, sheet.LastRowNum); i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            int score = 0;
            var currentMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int j = 0; j < row.LastCellNum; j++)
            {
                var cell = row.GetCell(j);
                if (cell == null || cell.CellType != CellType.String) continue;

                string val = cell.StringCellValue?.Trim() ?? "";
                if (string.IsNullOrEmpty(val)) continue;

                currentMap[val] = j;
                score += 1; // Generic string

                if (keywords.Any(k => val.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    score += 5; // Keyword match
            }

            if (score > bestScore && score >= 2) 
            {
                bestScore = score;
                bestRowIndex = i;
                columnMap = currentMap;
            }
        }

        return bestRowIndex;
    }

    private Dictionary<string, int> GetColumnMap(IRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            if (cell != null)
            {
                var val = cell.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    map[val] = i;
                }
            }
        }
        return map;
    }

    private string? GetValue(IRow row, Dictionary<string, int> columnMap, string header, DataFormatter formatter, IFormulaEvaluator evaluator)
    {
        if (columnMap.TryGetValue(header, out int index))
        {
            var cell = row.GetCell(index);
            if (cell == null) return null;

            try
            {
                // Evaluates the formula if necessary and returns the formatted value
                return formatter.FormatCellValue(cell, evaluator)?.Trim();
            }
            catch
            {
                // Fallback for tricky scenarios (like unsupported named ranges _xlpm.s)
                // We try to return the cached result of the formula instead of re-evaluating it
                try
                {
                    if (cell.CellType == CellType.Formula)
                    {
                        switch (cell.CachedFormulaResultType)
                        {
                            case CellType.String: return cell.StringCellValue?.Trim();
                            case CellType.Numeric: return cell.NumericCellValue.ToString();
                            case CellType.Boolean: return cell.BooleanCellValue.ToString();
                        }
                    }
                    return formatter.FormatCellValue(cell)?.Trim();
                }
                catch
                {
                    return null;
                }
            }
        }
        return null;
    }

    private bool IsRowEmpty(IRow row)
    {
        for (int i = row.FirstCellNum; i < row.LastCellNum; i++)
        {
            var cell = row.GetCell(i);
            if (cell != null && cell.CellType != CellType.Blank)
                return false;
        }
        return true;
    }
}
