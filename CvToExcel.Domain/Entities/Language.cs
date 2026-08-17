namespace CvToExcel.Domain.Entities;

public class Language
{
     public Guid Id {get; set;}
    public Guid CvDocumentId{get; set;}
    public CvDocument CvDocument { get; set; } = null!;
    public required string Name {get; set;}
    public required string ProficiencyLevel {get;set;}
}