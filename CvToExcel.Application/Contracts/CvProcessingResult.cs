namespace CvToExcel.Application.Contracts;

public class CvProcessingResult
{
    public CvExtractionResult NewCandidate { get; set; } = new();
    public ExcelTableResult Table { get; set; } = new();
}

public class ExcelTableResult
{
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, string?>> Rows { get; set; } = new();
}