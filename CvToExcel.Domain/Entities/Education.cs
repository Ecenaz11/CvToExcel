namespace CvToExcel.Domain.Entities;

public class Education
{
    public Guid Id { get; set; }
    public Guid CvDocumentId { get; set; }
    public CvDocument CvDocument { get; set; } = null!;
    public string Institution { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Degree { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent {get;set;}
}