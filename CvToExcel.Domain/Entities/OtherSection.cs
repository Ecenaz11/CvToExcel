namespace CvToExcel.Domain.Entities;

public class OtherSection
{
    public Guid Id {get; set;}
    public Guid CvDocumentId {get; set;}
    public CvDocument CvDocument { get; set; } = null!;
    public string Title {get; set;} = null!;
    public string? Content {get; set;}

}