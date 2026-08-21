namespace CvToExcel.Application.Contracts;

public class CvStatusResult
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime UploadAt { get; set; }
}