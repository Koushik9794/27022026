using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public static class ExcelInspector
{
    public static void Inspect(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            Console.WriteLine($"ERROR: File not found at {fileInfo.FullName}");
            return;
        }

        try 
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(stream);
                Console.WriteLine("Excel File Inspection Results:");
                
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    var sheet = workbook.GetSheetAt(i);
                    Console.WriteLine($"\n--- Sheet: {sheet.SheetName} ---");
                    
                    var headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        var headers = new List<string>();
                        for (int col = 0; col < headerRow.LastCellNum; col++)
                        {
                            var cell = headerRow.GetCell(col);
                            headers.Add(cell?.ToString() ?? "");
                        }
                        Console.WriteLine($"Headers: {string.Join(" | ", headers)}");
                    }

                    // Print first 3 rows
                    for (int row = 1; row <= Math.Min(3, sheet.LastRowNum); row++)
                    {
                        var dataRow = sheet.GetRow(row);
                        if (dataRow == null) continue;

                        var rowData = new List<string>();
                        for (int col = 0; col < (headerRow?.LastCellNum ?? dataRow.LastCellNum); col++)
                        {
                            var cell = dataRow.GetCell(col);
                            rowData.Add(cell?.ToString() ?? "");
                        }
                        Console.WriteLine($"Row {row}: {string.Join(" | ", rowData)}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR Reading Excel with NPOI: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
