using ClosedXML.Excel;
using CvToExcel.Application.Contracts;
using CvToExcel.Application.Interfaces;

namespace CvToExcel.Infrastructure.ExcelWriting;

public class ClosedXmlExcelWriter : IExcelWriter
{
    private readonly string _filePath;

    public ClosedXmlExcelWriter()
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "storage", "excel");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "candidates.xlsx");
    }
    public Task WriteAsync(ExcelTableResult table, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Candidates");

        for (var col = 0; col < table.Columns.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = table.Columns[col];
        }
        for (var row = 0; row < table.Rows.Count; row++)
        {
            for (var col = 0; col < table.Columns.Count; col++)
            {
                table.Rows[row].TryGetValue(table.Columns[col], out var cellValue);
                worksheet.Cell(row + 2, col + 1).Value = cellValue ?? string.Empty;
            }
        }
        workbook.SaveAs(_filePath);
        return Task.CompletedTask;
    }
}