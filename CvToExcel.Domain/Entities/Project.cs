namespace CvToExcel.Domain.Entities;

public class Project
{
    public Guid Id {get; set;}
    public Guid CvDocumentId{get; set;}
    public CvDocument CvDocument{get;set;} = null!;
    public string Title{get; set;} = null!;
    public string? TechnologiesUsed{get;set;}
    public string? Description {get;set;}
}