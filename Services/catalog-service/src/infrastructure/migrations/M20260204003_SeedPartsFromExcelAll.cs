using FluentMigrator;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260204003)]
public class SeedPartsFromExcelAll : Migration
{
    public override void Up()
    {
        // 1. Relax global unique constraint on Code in component_names
        Execute.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                -- Drop constraints relying on 'code' in component_names
                FOR r IN (
                    SELECT conname 
                    FROM pg_constraint 
                    WHERE conrelid = 'component_names'::regclass 
                    AND array_length(conkey, 1) = 1 
                    AND conkey[1] = (SELECT attnum FROM pg_attribute WHERE attrelid = 'component_names'::regclass AND attname = 'code')
                    AND contype = 'u'
                ) LOOP
                    EXECUTE 'ALTER TABLE component_names DROP CONSTRAINT ""' || r.conname || '""';
                END LOOP;

                -- Drop unique indexes involving only 'code'
                FOR r IN (
                    SELECT indexname 
                    FROM pg_indexes 
                    WHERE tablename = 'component_names' 
                    AND indexdef LIKE '%(code)%' 
                    AND indexdef LIKE '%UNIQUE%'
                ) LOOP
                    EXECUTE 'DROP INDEX IF EXISTS ""' || r.indexname || '""';
                END LOOP;
            END
            $$;
        ");

        // 2. Add composite unique constraint (Code + Type)
        Execute.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'UQ_component_names_code_type') THEN
                    ALTER TABLE component_names ADD CONSTRAINT ""UQ_component_names_code_type"" UNIQUE (code, component_type_id);
                END IF;
            END
            $$;
        ");

        Execute.WithConnection((conn, tran) =>
        {
            SeedFromExcel(conn, tran);
        });
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM parts WHERE created_by = 'SYSTEM_SEED'");
        // Taxonomy data is intentionally left as it might be shared.
    }

    private void SeedFromExcel(IDbConnection conn, IDbTransaction tran)
    {
        var excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "infrastructure", "20260119 GSS Master BOM Only SPRS v1.xlsx");
        if (!File.Exists(excelPath)) excelPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "infrastructure", "20260119 GSS Master BOM Only SPRS v1.xlsx");
        if (!File.Exists(excelPath)) excelPath = Path.Combine(Directory.GetCurrentDirectory(), "20260119 GSS Master BOM Only SPRS v1.xlsx");

        if (!File.Exists(excelPath)) throw new FileNotFoundException("Excel seed file not found", excelPath);

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook(stream);
        var formatter = new NPOI.SS.UserModel.DataFormatter();
        var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

        var sheetsToProcess = new[] { "Master BOM - SPRS (2)", "Master BOM - SPRS", "GLB FILE REQUIRED FOR UPLOAD", "Frame Components " };

        foreach (var sheetName in sheetsToProcess)
        {
            var sheet = workbook.GetSheet(sheetName);
            if (sheet == null) continue;

            // Find header row (it's not always the first row)
            int headerRowIndex = -1;
            for (int r = 0; r <= Math.Min(10, sheet.LastRowNum); r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                var rowStr = string.Join("|", row.Cells.Select(c => c.ToString() ?? ""));
                if (rowStr.Contains("Component group", StringComparison.OrdinalIgnoreCase) || rowStr.Contains("RM code", StringComparison.OrdinalIgnoreCase))
                {
                    headerRowIndex = r;
                    break;
                }
            }

            if (headerRowIndex == -1) continue;

            var headerRow = sheet.GetRow(headerRowIndex);
            var columnIndices = GetColumnIndices(headerRow);

            string? lastGroupName = null;
            string? lastTypeName = null;
            string? lastComponentName = null;

            for (int r = headerRowIndex + 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                var groupName = GetValue(row, columnIndices, "Component group", formatter, evaluator);
                var typeName = GetValue(row, columnIndices, "Component Type", formatter, evaluator);
                var componentName = GetValue(row, columnIndices, "Components", formatter, evaluator);
                var partCode = GetValue(row, columnIndices, "RM code", formatter, evaluator);
                var description = GetValue(row, columnIndices, "Description", formatter, evaluator) ?? GetValue(row, columnIndices, "Short Description", formatter, evaluator);
                var priceStr = GetValue(row, columnIndices, "Unit Basic Price(INR)", formatter, evaluator);
                var color = GetValue(row, columnIndices, "Colour", formatter, evaluator);
                var powderCode = GetValue(row, columnIndices, "Powder code", formatter, evaluator);
                var gfaFlagStr = GetValue(row, columnIndices, "GFA / Non GFA", formatter, evaluator);
                var unspscCode = GetValue(row, columnIndices, "UNSPSC Code", formatter, evaluator);
                var cbmStr = GetValue(row, columnIndices, "CBM", formatter, evaluator);
                var drawingNo = GetValue(row, columnIndices, "Drawing No", formatter, evaluator);
                var revNo = GetValue(row, columnIndices, "Rev No", formatter, evaluator);
                var installRef = GetValue(row, columnIndices, "Installation reference number", formatter, evaluator);

                // Carry forward taxonomy values
                if (!string.IsNullOrWhiteSpace(groupName)) lastGroupName = groupName;
                if (!string.IsNullOrWhiteSpace(typeName)) lastTypeName = typeName;
                if (!string.IsNullOrWhiteSpace(componentName)) lastComponentName = componentName;

                groupName = lastGroupName;
                typeName = lastTypeName;
                componentName = lastComponentName;

                // Skip rows that are truly empty in columns we care about
                if (string.IsNullOrWhiteSpace(groupName) && string.IsNullOrWhiteSpace(typeName) && string.IsNullOrWhiteSpace(partCode) && string.IsNullOrWhiteSpace(description)) continue;

                // For some rows, taxonomy might be missing but RM code exists - we'll skip if no taxonomy at all
                if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(typeName)) continue;

                var groupId = UpsertGroup(conn, tran, groupName);
                var typeId = UpsertType(conn, tran, groupId, typeName);
                Guid? nameId = null;
                if (!string.IsNullOrWhiteSpace(componentName))
                {
                    nameId = UpsertName(conn, tran, typeId, componentName);
                }

                if (string.IsNullOrWhiteSpace(partCode))
                {
                    if (string.IsNullOrWhiteSpace(description)) continue;
                    partCode = GeneratePartCode(typeName, componentName, description, r);
                }

                decimal price = 0;
                if (!string.IsNullOrWhiteSpace(priceStr)) decimal.TryParse(priceStr, out price);

                decimal? cbm = null;
                if (!string.IsNullOrWhiteSpace(cbmStr) && decimal.TryParse(cbmStr, out var cbmVal)) cbm = cbmVal;

                bool gfaFlag = gfaFlagStr?.Equals("GFA", StringComparison.OrdinalIgnoreCase) ?? false;

                var unmappedAttributes = new Dictionary<string, object>();
                var handledColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { 
                    "Component group", "Component Type", "Components", "RM code", "Description", 
                    "Short Description", "Unit Basic Price(INR)", "Colour", "Powder code", 
                    "GFA / Non GFA", "UNSPSC Code", "CBM", "Drawing No", "Rev No", 
                    "Installation reference number" 
                };

                foreach (var colName in columnIndices.Keys)
                {
                    if (!handledColumns.Contains(colName))
                    {
                        var val = GetValue(row, columnIndices, colName, formatter, evaluator);
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            unmappedAttributes[colName] = val;
                        }
                    }
                }

                UpsertPart(conn, tran, new PartSeedData
                {
                    Id = Guid.NewGuid(),
                    PartCode = partCode,
                    CountryCode = "IN",
                    GroupId = groupId,
                    TypeId = typeId,
                    NameId = nameId,
                    Price = price,
                    Description = description ?? partCode,
                    ShortDescription = GetValue(row, columnIndices, "Short Description", formatter, evaluator),
                    UnspscCode = unspscCode,
                    Colour = color,
                    PowderCode = powderCode,
                    GfaFlag = gfaFlag,
                    Cbm = cbm,
                    DrawingNo = drawingNo,
                    RevNo = revNo,
                    InstallationRefNo = installRef,
                    Attributes = unmappedAttributes
                });
            }
        }
    }

    private Dictionary<string, int> GetColumnIndices(NPOI.SS.UserModel.IRow headerRow)
    {
        var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            if (cell != null)
            {
                var val = cell.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    indices[val] = i;
                }
            }
        }
        return indices;
    }

    private string? GetValue(NPOI.SS.UserModel.IRow row, Dictionary<string, int> indices, string columnName, NPOI.SS.UserModel.DataFormatter formatter, NPOI.SS.UserModel.IFormulaEvaluator evaluator)
    {
        if (indices.TryGetValue(columnName, out var index))
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
                    if (cell.CellType == NPOI.SS.UserModel.CellType.Formula)
                    {
                        switch (cell.CachedFormulaResultType)
                        {
                            case NPOI.SS.UserModel.CellType.String: return cell.StringCellValue?.Trim();
                            case NPOI.SS.UserModel.CellType.Numeric: return cell.NumericCellValue.ToString();
                            case NPOI.SS.UserModel.CellType.Boolean: return cell.BooleanCellValue.ToString();
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

    private class PartSeedData
    {
        public Guid Id { get; set; }
        public string PartCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public Guid GroupId { get; set; }
        public Guid TypeId { get; set; }
        public Guid? NameId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? UnspscCode { get; set; }
        public string? Colour { get; set; }
        public string? PowderCode { get; set; }
        public bool GfaFlag { get; set; }
        public decimal? Cbm { get; set; }
        public string? DrawingNo { get; set; }
        public string? RevNo { get; set; }
        public string? InstallationRefNo { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = [];
    }

    private void UpsertPart(IDbConnection conn, IDbTransaction tran, PartSeedData data)
    {
        using var checkCmd = conn.CreateCommand();
        checkCmd.Transaction = tran;
        checkCmd.CommandText = "SELECT id FROM parts WHERE part_code = @PartCode AND country_code = @CountryCode";
        AddParameter(checkCmd, "PartCode", data.PartCode);
        AddParameter(checkCmd, "CountryCode", data.CountryCode);

        var existingId = checkCmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value)
        {
            using var updateCmd = conn.CreateCommand();
            updateCmd.Transaction = tran;
            updateCmd.CommandText = @"
                UPDATE parts SET 
                    component_group_id = @GroupId, 
                    component_type_id = @TypeId, 
                    component_name_id = @NameId, 
                    unit_basic_price = @Price,
                    description = @Description,
                    short_description = @ShortDescription,
                    unspsc_code = @UnspscCode,
                    colour = @Colour,
                    powder_code = @PowderCode,
                    gfa_flag = @GfaFlag,
                    cbm = @Cbm,
                    drawing_no = @DrawingNo,
                    rev_no = @RevNo,
                    installation_ref_no = @InstallRef,
                    updated_at = @Now,
                    updated_by = 'SYSTEM_SEED',
                    attributes = @Attributes::jsonb
                WHERE id = @Id";
            AddParameter(updateCmd, "Id", existingId);
            AddParameter(updateCmd, "GroupId", data.GroupId);
            AddParameter(updateCmd, "TypeId", data.TypeId);
            AddParameter(updateCmd, "NameId", (object?)data.NameId ?? DBNull.Value);
            AddParameter(updateCmd, "Price", data.Price);
            AddParameter(updateCmd, "Description", data.Description);
            AddParameter(updateCmd, "ShortDescription", (object?)data.ShortDescription ?? DBNull.Value);
            AddParameter(updateCmd, "UnspscCode", (object?)data.UnspscCode ?? DBNull.Value);
            AddParameter(updateCmd, "Colour", (object?)data.Colour ?? DBNull.Value);
            AddParameter(updateCmd, "PowderCode", (object?)data.PowderCode ?? DBNull.Value);
            AddParameter(updateCmd, "GfaFlag", data.GfaFlag);
            AddParameter(updateCmd, "Cbm", (object?)data.Cbm ?? DBNull.Value);
            AddParameter(updateCmd, "DrawingNo", (object?)data.DrawingNo ?? DBNull.Value);
            AddParameter(updateCmd, "RevNo", (object?)data.RevNo ?? DBNull.Value);
            AddParameter(updateCmd, "InstallRef", (object?)data.InstallationRefNo ?? DBNull.Value);
            AddParameter(updateCmd, "Now", DateTime.UtcNow);
            AddParameter(updateCmd, "Attributes", System.Text.Json.JsonSerializer.Serialize(data.Attributes));
            updateCmd.ExecuteNonQuery();
            return;
        }

        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = @"
            INSERT INTO parts (
                id, part_code, country_code, component_group_id, component_type_id, component_name_id, 
                unit_basic_price, status, is_deleted, created_at, created_by, description, short_description,
                unspsc_code, colour, powder_code, gfa_flag, cbm, drawing_no, rev_no, installation_ref_no, attributes
            )
            VALUES (
                @Id, @PartCode, @CountryCode, @GroupId, @TypeId, @NameId, 
                @Price, 'ACTIVE', false, @Now, 'SYSTEM_SEED', @Description, @ShortDescription,
                @UnspscCode, @Colour, @PowderCode, @GfaFlag, @Cbm, @DrawingNo, @RevNo, @InstallRef, @Attributes::jsonb
            )";

        AddParameter(insertCmd, "Id", data.Id);
        AddParameter(insertCmd, "PartCode", data.PartCode);
        AddParameter(insertCmd, "CountryCode", data.CountryCode);
        AddParameter(insertCmd, "GroupId", data.GroupId);
        AddParameter(insertCmd, "TypeId", data.TypeId);
        AddParameter(insertCmd, "NameId", (object?)data.NameId ?? DBNull.Value);
        AddParameter(insertCmd, "Price", data.Price);
        AddParameter(insertCmd, "Description", data.Description);
        AddParameter(insertCmd, "ShortDescription", (object?)data.ShortDescription ?? DBNull.Value);
        AddParameter(insertCmd, "UnspscCode", (object?)data.UnspscCode ?? DBNull.Value);
        AddParameter(insertCmd, "Colour", (object?)data.Colour ?? DBNull.Value);
        AddParameter(insertCmd, "PowderCode", (object?)data.PowderCode ?? DBNull.Value);
        AddParameter(insertCmd, "GfaFlag", data.GfaFlag);
        AddParameter(insertCmd, "Cbm", (object?)data.Cbm ?? DBNull.Value);
        AddParameter(insertCmd, "DrawingNo", (object?)data.DrawingNo ?? DBNull.Value);
        AddParameter(insertCmd, "RevNo", (object?)data.RevNo ?? DBNull.Value);
        AddParameter(insertCmd, "InstallRef", (object?)data.InstallationRefNo ?? DBNull.Value);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        AddParameter(insertCmd, "Attributes", System.Text.Json.JsonSerializer.Serialize(data.Attributes));

        insertCmd.ExecuteNonQuery();
    }

    private Guid UpsertGroup(IDbConnection conn, IDbTransaction tran, string name)
    {
        var code = Sanitize(name);
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_groups WHERE UPPER(code) = @Code";
        AddParameter(cmd, "Code", code);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value) return (Guid)existingId;

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = "INSERT INTO component_groups (id, code, name, description, is_active, created_at) VALUES (@Id, @Code, @Name, @Name, true, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();
        return newId;
    }

    private Guid UpsertType(IDbConnection conn, IDbTransaction tran, Guid groupId, string name)
    {
        var code = Sanitize(name);
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_types WHERE UPPER(code) = @Code AND component_group_id = @GroupId";
        AddParameter(cmd, "Code", code);
        AddParameter(cmd, "GroupId", groupId);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value) return (Guid)existingId;

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = "INSERT INTO component_types (id, code, name, description, component_group_id, is_active, created_at) VALUES (@Id, @Code, @Name, @Name, @GroupId, true, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "GroupId", groupId);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();
        return newId;
    }

    private Guid UpsertName(IDbConnection conn, IDbTransaction tran, Guid typeId, string name)
    {
        var code = Sanitize(name);
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_names WHERE UPPER(code) = @Code AND component_type_id = @TypeId";
        AddParameter(cmd, "Code", code);
        AddParameter(cmd, "TypeId", typeId);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value) return (Guid)existingId;

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = "INSERT INTO component_names (id, code, name, description, component_type_id, is_active, created_at) VALUES (@Id, @Code, @Name, @Name, @TypeId, true, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "TypeId", typeId);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();
        return newId;
    }

    private string GeneratePartCode(string typeName, string? nameName, string? description, int rowIndex)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "UNKNOWN";
        }

        // 1. Beam GBHX/GBHO logic -> "GBHX 75 x 1000"
        var beamMatch = Regex.Match(description, @"^Beam\s+([A-Z0-9]+)\s+([\d\.]+)\s*x\s*([\d\.]+)\s*x\s*([\d\.]+)(?:[- ].*)?$", RegexOptions.IgnoreCase);
        if (beamMatch.Success)
        {
            var typeCode = beamMatch.Groups[1].Value;
            var length = beamMatch.Groups[2].Value;
            var height = beamMatch.Groups[4].Value;
            return $"{typeCode} {height} x {length}";
        }

        // 2. Tie logic -> "Tie 50 x 300"
        var tieMatch = Regex.Match(description, @"^Tie\s+([\d\.]+)\s*x\s*([\d\.]+)$", RegexOptions.IgnoreCase);
        if (tieMatch.Success)
        {
            return $"Tie {tieMatch.Groups[1].Value} x {tieMatch.Groups[2].Value}";
        }

        // 3. Walkway logic (explicitly mentioned, though default handles it now)
        if (description.StartsWith("Walkway Panel-Hole", StringComparison.OrdinalIgnoreCase))
        {
            var code = description.Trim();
            return code.Length > 100 ? code.Substring(0, 100) : code;
        }

        // 4. Default: Match the format provided in examples (use description directly)
        var finalCode = description.Trim();
        if (finalCode.Length > 100)
        {
            finalCode = finalCode.Substring(0, 100);
        }
        return finalCode;
    }

    private string Sanitize(string input)
    {
        var s = input.ToUpperInvariant();
        s = Regex.Replace(s, "[^A-Z0-9]+", "-");
        return s.Trim('-');
    }

    private void AddParameter(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
