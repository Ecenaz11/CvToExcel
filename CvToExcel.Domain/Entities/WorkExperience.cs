namespace CvToExcel.Domain.Entities;

public class WorkExperience
{
    public Guid Id {get; set;}
    public Guid CvDocumentId{get; set;}
    public CvDocument CvDocument { get; set; } = null!;
    public required string CompanyName {get;set;}
    public required string JobTitle{get;set;}
    public DateTime? StartDate{get;set;}
    public DateTime? EndDate{get;set;}
    public bool IsCurrent {get;set;}
}